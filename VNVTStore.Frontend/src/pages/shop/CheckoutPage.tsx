import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ChevronRight, CreditCard, Truck, MapPin, Phone, User, Mail, FileText, Ticket, X } from 'lucide-react';
import { motion } from 'framer-motion';
import { Button, Input, Select, Modal } from '@/components/ui';
import CustomImage from '@/components/common/Image';
import { useCartStore, useAuthStore, useToast } from '@/store';
import { CouponSelector } from '@/components/common';
import { formatCurrency } from '@/utils/format';
import { orderService, type CreateOrderRequest } from '@/services/orderService';
import { paymentService } from '@/services/paymentService';
import { PaymentMethod } from '@/constants';
import { useCheckoutStore } from '@/store/checkoutStore';
import { REGEX } from '@/constants/regex';
import { userService, type AddressDto } from '@/services/userService';
import { Badge } from '@/components/ui';

import { useSEO } from '@/hooks/useSEO';

export const CheckoutPage = () => {
  const { t } = useTranslation();
  
  useSEO({
    title: 'Thanh toán',
    noindex: true,
  });
  const navigate = useNavigate();
  const toast = useToast();
  const { items, getTotal, clearCart, fetchCart } = useCartStore();
  const { user, isAuthenticated } = useAuthStore();
  
  // Store hooks
  const { 
    step, setStep, 
    formData, setFormData, 
    paymentMethod, setPaymentMethod, 
    voucherCode, setVoucherCode,
    resetCheckout 
  } = useCheckoutStore();

  const [showLoginModal, setShowLoginModal] = useState(false);
  const [isGuestCheckout, setIsGuestCheckout] = useState(true); 
  const [showCouponSelector, setShowCouponSelector] = useState(false);
  const [savedAddresses, setSavedAddresses] = useState<AddressDto[]>([]);
  const [showAddressModal, setShowAddressModal] = useState(false);

  useEffect(() => {
    if (isAuthenticated) {
      userService.getMyAddresses().then(res => {
        if (res.success && res.data) {
          setSavedAddresses(res.data as unknown as AddressDto[]);
        }
      });
    }
  }, [isAuthenticated]);

  useEffect(() => {
    if (!isAuthenticated && !isGuestCheckout) {
      setShowLoginModal(true);
    } else {
      setShowLoginModal(false);
    }
  }, [isAuthenticated, isGuestCheckout]);
  
  // Sync user info to form if empty
  useEffect(() => {
     if (isAuthenticated && user && !formData.fullName) {
         setFormData({
             fullName: user.fullName || '',
             phone: user.phone || '',
             email: user.email || ''
         });
     }
  }, [isAuthenticated, user, formData.fullName, setFormData]);

  const [isProcessing, setIsProcessing] = useState(false);

  const subtotal = getTotal();
  const shippingFee = subtotal >= 500000 ? 0 : 30000;
  // const total = subtotal + shippingFee; // Removed unused total variable

  // Pre-fill if user has address (Logic omitted for brevity, could use userService.getMyAddresses())

  const handleInputChange = (field: string, value: string) => {
    setFormData({ [field]: value });
  };

  // Voucher Logic
  const [appliedVoucher, setAppliedVoucher] = useState<{ code: string, discount: number, type: string } | null>(null);

  const handleApplyVoucher = async () => {
    if (!voucherCode.trim()) return;
    try {
      const res = await import('@/services/promotionService').then(m => m.promotionService.getByCode(voucherCode));
      if (res.success && res.data) {
        const promo = res.data;
        // Validate
        const now = new Date();
        const startDate = new Date(promo.startDate);
        const endDate = new Date(promo.endDate);

        if (!promo.isActive || startDate > now || endDate < now) {
          toast.error(t('checkout.voucherExpired') || 'Mã giảm giá không hợp lệ hoặc đã hết hạn');
          return;
        }

        if (promo.minOrderAmount !== undefined && subtotal < promo.minOrderAmount) {
          toast.error(`${t('checkout.minOrderAmount') || 'Đơn hàng tối thiểu'}: ${formatCurrency(promo.minOrderAmount)}`);
          return;
        }

        // Calculate Discount
        let discountCount = 0;
        if (promo.discountType === 'PERCENTAGE') {
          discountCount = subtotal * (promo.discountValue / 100);
          if (promo.maxDiscountAmount) discountCount = Math.min(discountCount, promo.maxDiscountAmount);
        } else {
          discountCount = promo.discountValue;
        }

        setAppliedVoucher({
          code: promo.code,
          discount: discountCount,
          type: promo.discountType
        });
        toast.success(t('checkout.voucherApplied') || 'Áp dụng mã giảm giá thành công');
      } else {
        toast.error(res.message || t('checkout.voucherInvalid') || 'Mã giảm giá không tồn tại');
      }
    } catch {
      toast.error(t('checkout.voucherError') || 'Lỗi kiểm tra mã giảm giá');
    }
  };

  const discountAmount = appliedVoucher ? appliedVoucher.discount : 0;
  // Recalculate total with discount
  const finalTotal = Math.max(0, subtotal + shippingFee - discountAmount);

  /* Validation Regex */
  const emailRegex = REGEX.EMAIL;
  const phoneRegex = REGEX.PHONE;

  const [errors, setErrors] = useState({
      email: '',
      phone: ''
  });

  const validate = () => {
    let isValid = true;
    const newErrors = { email: '', phone: '' };

    if (formData.email && !emailRegex.test(formData.email)) {
        newErrors.email = t('validation.invalidEmail') || 'Email không hợp lệ';
        isValid = false;
    }

    if (!phoneRegex.test(formData.phone)) {
        newErrors.phone = t('validation.invalidPhone') || 'Số điện thoại không hợp lệ (VD: 0901234567)';
        isValid = false;
    }

    setErrors(newErrors);
    return isValid;
  };

  const handleSubmit = async () => {
    setIsProcessing(true);
    try {
      // Validation
      if (!formData.fullName || !formData.phone || !formData.address || !formData.city || !formData.district) {
        toast.error(t('validation.required') || 'Vui lòng điền đầy đủ thông tin giao hàng');
        setIsProcessing(false);
        return;
      }

      if (!validate()) {
          toast.error(t('validation.checkErrors') || 'Vui lòng kiểm tra lại thông tin giao hàng');
          setStep(1); // Go back to step 1 to show errors
          setIsProcessing(false);
          return;
      }

      const orderData: CreateOrderRequest = {
        fullName: formData.fullName,
        email: formData.email,
        phone: formData.phone,
        address: formData.address,
        city: formData.city,
        district: formData.district,
        ward: formData.ward,
        note: formData.note,
        paymentMethod: paymentMethod,
        couponCode: appliedVoucher?.code,
        items: items.map(item => ({
          productCode: item.product.code,
          quantity: item.quantity,
          size: item.size,
          color: item.color
        }))
      };

      // 1. Create Order
      const orderRes = await orderService.create(orderData);

      if (orderRes.success && orderRes.data) {
        const orderCode = orderRes.data.code;
        toast.success(t('messages.orderSuccess') || 'Đặt hàng thành công!');
        
        resetCheckout(); // RESET STORE

        // 2. Process Payment
        try {
          // Use the ACTUAL final amount from backend response if available, else local calc
          const totalAmount = orderRes.data.finalAmount || finalTotal;
          await paymentService.create({
            orderCode: orderCode,
            paymentMethod: paymentMethod,
            amount: totalAmount
          });
        } catch (paymentError) {
          console.error('Payment creation failed', paymentError);
          toast.error(t('messages.paymentError') || 'Có lỗi khi tạo thanh toán, vui lòng liên hệ CSKH.');
        }

        // 3. Clear Cart & Redirect
        if (isAuthenticated) {
          await fetchCart();
        } else {
          await clearCart();
        }
        navigate(`/order-success?code=${orderCode}`);
      } else {
        toast.error(orderRes.message || t('messages.orderError') || 'Đặt hàng thất bại');
      }
    } catch (error: unknown) {
      console.error('Order failed', error);
      const msg = error instanceof Error ? error.message : t('messages.generalError') || 'Có lỗi xảy ra';
      toast.error(msg);
    } finally {
      setIsProcessing(false);
    }
  };

  // Re-fetch cart if empty? No, handled by App/ShopLayout.

  if (items.length === 0) {
    return (
      <div className="min-h-screen bg-secondary flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">{t('cart.empty')}</h1>
          <Link to="/products">
            <Button>{t('cart.continueShopping')}</Button>
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-secondary">
      {/* Breadcrumb */}
      <div className="bg-primary border-b py-3">
        <div className="container mx-auto px-4 flex items-center gap-2 text-sm">
          <Link to="/" className="text-secondary hover:text-primary">{t('common.home')}</Link>
          <ChevronRight size={14} />
          <Link to="/cart" className="text-secondary hover:text-primary">{t('cart.title')}</Link>
          <ChevronRight size={14} />
          <span className="text-primary font-medium">{t('checkout.title')}</span>
        </div>
      </div>

      <div className="container mx-auto px-4 py-8">
        {/* Progress Steps */}
        <div className="flex items-center justify-center mb-8">
          {[
            { num: 1, label: t('checkout.shipping') },
            { num: 2, label: t('checkout.payment') },
            { num: 3, label: t('checkout.confirm') },
          ].map((s, i) => (
            <div key={s.num} className="flex items-center">
              <div
                className={`w-10 h-10 rounded-full flex items-center justify-center font-bold ${step >= s.num ? 'bg-primary text-white' : 'bg-tertiary text-secondary'
                  }`}
              >
                {s.num}
              </div>
              <span className={`ml-2 hidden sm:inline ${step >= s.num ? 'text-primary' : 'text-tertiary'}`}>
                {s.label}
              </span>
              {i < 2 && <div className={`w-12 h-1 mx-4 ${step > s.num ? 'bg-primary' : 'bg-tertiary'}`} />}
            </div>
          ))}
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Form */}
          <div className="lg:col-span-2">
            {/* Step 1: Shipping */}
            {step === 1 && (
              <div className="bg-primary rounded-xl p-6">
                <h2 className="text-xl font-bold mb-6 flex items-center gap-2">
                   <Truck size={24} />
                   {t('checkout.shippingInfo')}
                 </h2>

                 {isAuthenticated && savedAddresses.length > 0 && (
                   <div className="mb-6 p-4 bg-indigo-50 dark:bg-indigo-900/20 border border-indigo-100 dark:border-indigo-800 rounded-2xl">
                     <div className="flex justify-between items-center">
                       <div className="flex items-center gap-3">
                         <MapPin className="text-indigo-600" size={20} />
                         <div>
                           <p className="font-bold text-sm text-primary">{t('checkout.useSavedAddress', 'Sử dụng địa chỉ đã lưu')}</p>
                           <p className="text-xs text-tertiary">{t('checkout.savedAddressesCount', 'Bạn có {{count}} địa chỉ đã lưu', { count: savedAddresses.length })}</p>
                         </div>
                       </div>
                       <Button size="sm" variant="outline" onClick={() => setShowAddressModal(true)}>
                         {t('common.select', 'Chọn')}
                       </Button>
                     </div>
                   </div>
                 )}

                 {/* Guest Banner */}
                 {!isAuthenticated && isGuestCheckout && (
                   <div className="mb-6 overflow-hidden relative group bg-gradient-to-br from-amber-400/10 to-orange-500/10 border border-amber-500/20 rounded-2xl p-6">
                     <div className="absolute top-0 right-0 p-4 opacity-10 group-hover:opacity-20 transition-opacity">
                       <Ticket size={80} className="rotate-12" />
                     </div>
                     <div className="relative z-10 flex flex-col md:flex-row items-center justify-between gap-4">
                       <div>
                         <h3 className="text-lg font-black text-amber-900 dark:text-amber-400 mb-1 flex items-center gap-2">
                           {t('home.registerMember', 'Đăng ký thành viên')}
                           <Badge color="error" className="animate-pulse">{t('common.new', 'Mới')}</Badge>
                         </h3>
                         <p className="text-sm text-slate-700 dark:text-slate-300 font-medium">
                           {t('checkout.guestBannerText', 'Đăng ký ngay để nhận 50.000đ và tích điểm cho đơn hàng này!')}
                         </p>
                       </div>
                       <Link to="/register" state={{ from: '/checkout' }}>
                         <Button size="sm" className="bg-amber-500 hover:bg-amber-600 border-none shadow-lg shadow-amber-500/20">
                           {t('auth.registerNow', 'Đăng ký ngay')}
                         </Button>
                       </Link>
                     </div>
                   </div>
                 )}

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Input
                    label={t('checkout.fullName')}
                    placeholder="Nguyễn Văn A"
                    value={formData.fullName}
                    onChange={(e) => handleInputChange('fullName', e.target.value)}
                    leftIcon={<User size={18} />}
                    required
                    isRequired
                  />
                  <Input
                    label={t('checkout.phone')}
                    placeholder="0901234567"
                    value={formData.phone}
                    onChange={(e) => {
                        handleInputChange('phone', e.target.value);
                        if(errors.phone) setErrors({...errors, phone: ''});
                    }}
                    leftIcon={<Phone size={18} />}
                    required
                    isRequired
                    error={errors.phone}
                  />
                  <div className="md:col-span-2">
                    <Input
                      label={t('shared.email')}
                      type="email"
                      placeholder="email@example.com"
                      value={formData.email}
                      onChange={(e) => {
                          handleInputChange('email', e.target.value);
                          if(errors.email) setErrors({...errors, email: ''});
                      }}
                      leftIcon={<Mail size={18} />}
                      error={errors.email}
                    />
                  </div>
                    <Select
                      label={t('checkout.city')}
                      value={formData.city}
                      onChange={(e) => handleInputChange('city', e.target.value)}
                      options={[
                        { value: '', label: 'Chọn Tỉnh/Thành phố' },
                        { value: 'hcm', label: 'TP. Hồ Chí Minh' },
                        { value: 'hn', label: 'Hà Nội' },
                        { value: 'dn', label: 'Đà Nẵng' },
                      ]}
                      required
                      isRequired
                    />
                    <Select
                      label={t('checkout.district')}
                      value={formData.district}
                      onChange={(e) => handleInputChange('district', e.target.value)}
                      options={[
                        { value: '', label: 'Chọn Quận/Huyện' },
                        { value: 'q1', label: 'Quận 1' },
                        { value: 'q3', label: 'Quận 3' },
                        { value: 'q7', label: 'Quận 7' },
                      ]}
                      required
                      isRequired
                    />
                  <div className="md:col-span-2">
                    <Input
                      label={t('checkout.address')}
                      placeholder="Số nhà, tên đường..."
                      value={formData.address}
                      onChange={(e) => handleInputChange('address', e.target.value)}
                      leftIcon={<MapPin size={18} />}
                      required
                      isRequired
                    />
                  </div>
                  <div className="md:col-span-2">
                    <Input
                      label={t('checkout.note')}
                      placeholder="Ghi chú đơn hàng (không bắt buộc)"
                      value={formData.note}
                      onChange={(e) => handleInputChange('note', e.target.value)}
                      leftIcon={<FileText size={18} />}
                    />
                  </div>
                </div>

                <div className="mt-6 flex justify-end">
                  <Button size="lg" onClick={() => {
                    if (!formData.fullName || !formData.phone || !formData.address || !formData.city || !formData.district) {
                      toast.error(t('validation.required') || 'Vui lòng điền đầy đủ thông tin giao hàng');
                      return;
                    }
                    if (!validate()) {
                      toast.error(t('validation.checkErrors') || 'Vui lòng kiểm tra lại thông tin');
                      return;
                    }
                    setStep(2);
                  }}>
                    {t('checkout.continue')}
                  </Button>
                </div>
              </div>
            )}

            {/* Step 2: Payment */}
            {step === 2 && (
              <div className="bg-primary rounded-xl p-6">
                <h2 className="text-xl font-bold mb-6 flex items-center gap-2">
                  <CreditCard size={24} />
                  {t('checkout.paymentMethod')}
                </h2>

                <div className="space-y-4">
                    {[
                      { value: PaymentMethod.COD, label: 'Thanh toán khi nhận hàng (COD)', icon: '💵' },
                      { value: 'ZALOPAY', label: 'ZaloPay', icon: '💳' },
                      { value: 'MOMO', label: 'Ví MoMo', icon: '📱' },
                      { value: 'VNPAY', label: 'VNPAY QR', icon: '🏦' },
                      { value: PaymentMethod.BANK_TRANSFER, label: 'Chuyển khoản ngân hàng', icon: '🏛️' },
                    ].map((method) => (
                      <label
                        key={method.value}
                        className={`flex items-center gap-4 p-4 border-2 rounded-xl cursor-pointer transition-colors ${paymentMethod === method.value ? 'border-primary bg-primary/5' : 'hover:border-primary/50'
                          }`}
                      >
                        <input
                          type="radio"
                          name="payment"
                          value={method.value}
                          checked={paymentMethod === method.value}
                          onChange={(e) => setPaymentMethod(e.target.value)}
                          className="w-5 h-5 text-primary"
                        />
                        <div className="flex-1">
                          <div className="flex items-center gap-3">
                            <span className="text-2xl">{method.icon}</span>
                            <span className="font-medium">{method.label}</span>
                          </div>
                          {paymentMethod === PaymentMethod.BANK_TRANSFER && method.value === PaymentMethod.BANK_TRANSFER && (
                            <motion.div
                              initial={{ height: 0, opacity: 0 }}
                              animate={{ height: 'auto', opacity: 1 }}
                              className="mt-3 p-4 bg-tertiary rounded-lg text-xs space-y-1"
                            >
                              <p className="font-bold text-primary">{t('checkout.bankInfo')}</p>
                              <p>{t('common.fields.bankName')}: <strong>{t('checkout.bankName')}</strong></p>
                              <p>{t('common.fields.bankAccount')}: <strong>{t('checkout.accountNumber')}</strong></p>
                              <p>{t('common.fields.accountHolder') || 'Chủ tài khoản'}: <strong>{t('checkout.accountHolder')}</strong></p>
                              <p>{t('common.fields.note')}: <strong>{t('checkout.transferContent')}</strong></p>
                          </motion.div>
                        )}
                      </div>
                    </label>
                  ))}
                </div>

                <div className="mt-6 flex justify-between">
                  <Button variant="outline" onClick={() => setStep(1)}>
                    {t('common.back')}
                  </Button>
                  <Button size="lg" onClick={() => setStep(3)}>
                    {t('checkout.continue')}
                  </Button>
                </div>
              </div>
            )}

            {/* Step 3: Confirm */}
            {step === 3 && (
              <div className="bg-primary rounded-xl p-6">
                <h2 className="text-xl font-bold mb-6">{t('checkout.confirmOrder')}</h2>

                {/* Shipping Summary */}
                <div className="mb-6 p-4 bg-secondary rounded-lg">
                  <h3 className="font-semibold mb-2">{t('checkout.shippingInfo')}</h3>
                  <p>{formData.fullName}</p>
                  <p>{formData.phone}</p>
                  <p>{formData.address}, {formData.district}, {formData.city}</p>
                </div>

                {/* Payment Summary */}
                <div className="mb-6 p-4 bg-secondary rounded-lg">
                  <h3 className="font-semibold mb-2">{t('checkout.paymentMethod')}</h3>
                  <p className="capitalize">{paymentMethod}</p>
                </div>

                {/* Order Items */}
                <div className="space-y-3 mb-6">
                  {items.map((item) => (
                    <div key={item.code} className="flex items-center gap-4">
                      <div className="w-16 h-16 rounded-lg overflow-hidden bg-secondary flex-shrink-0">
                        <CustomImage
                          src={item.product.image}
                          alt={item.product.name}
                          className="w-full h-full object-cover"
                        />
                      </div>
                      <div className="flex-1">
                        <p className="font-medium line-clamp-1">{item.product.name}</p>
                        <p className="text-sm text-tertiary">x{item.quantity}</p>
                      </div>
                      <p className="font-bold">{formatCurrency(item.product.price * item.quantity)}</p>
                    </div>
                  ))}
                </div>

                <div className="mt-6 flex justify-between">
                  <Button variant="outline" onClick={() => setStep(2)}>
                    {t('common.back')}
                  </Button>
                  <Button size="lg" onClick={handleSubmit} isLoading={isProcessing}>
                    {t('checkout.placeOrder')}
                  </Button>
                </div>
              </div>
            )}
          </div>

          {/* Order Summary */}
          <div className="lg:col-span-1">
            <div className="bg-primary rounded-xl p-6 sticky top-24">
              <h2 className="text-lg font-bold mb-4">{t('cart.orderSummary')}</h2>

              <div className="space-y-3 mb-4 max-h-64 overflow-y-auto">
                {items.map((item) => (
                  <div key={item.code} className="flex gap-3 text-sm">
                    <div className="w-12 h-12 rounded overflow-hidden bg-secondary flex-shrink-0">
                      <CustomImage
                        src={item.product.image}
                        alt={item.product.name}
                        className="w-full h-full object-cover"
                      />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="font-medium truncate">{item.product.name}</p>
                      <p className="text-tertiary">x{item.quantity}</p>
                    </div>
                    <p className="font-medium">{formatCurrency(item.product.price * item.quantity)}</p>
                  </div>
                ))}
              </div>

              <hr className="my-4" />

              <div className="space-y-2">
                {/* Voucher Input */}
                <div className="flex gap-2">
                  <div className="relative flex-1">
                    <Input
                      placeholder={t('checkout.voucherPlaceholder') || 'Mã giảm giá'}
                      value={voucherCode}
                      onChange={(e) => setVoucherCode(e.target.value.toUpperCase())}
                      className="w-full"
                    />
                    <button 
                      onClick={() => setShowCouponSelector(true)}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-indigo-600 hover:text-indigo-700 font-bold text-xs"
                    >
                      {t('checkout.viewCoupons', 'Danh sách')}
                    </button>
                  </div>
                  <Button size="sm" onClick={handleApplyVoucher} disabled={!voucherCode || !!appliedVoucher}>
                    {t('common.apply')}
                  </Button>
                </div>
                {appliedVoucher && (
                    <motion.div 
                      initial={{ opacity: 0, scale: 0.95 }}
                      animate={{ opacity: 1, scale: 1 }}
                      className="flex justify-between items-center bg-emerald-50 dark:bg-emerald-900/20 border border-emerald-100 dark:border-emerald-800 p-3 rounded-xl"
                    >
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-full bg-emerald-100 dark:bg-emerald-800 flex items-center justify-center text-emerald-600">
                          <Ticket size={16} />
                        </div>
                        <div>
                          <p className="text-[10px] font-bold text-emerald-600 uppercase tracking-wider">{t('checkout.appliedVoucher', 'Mã áp dụng')}</p>
                          <p className="text-sm font-bold text-slate-900 dark:text-white">{appliedVoucher.code}</p>
                        </div>
                      </div>
                      <button 
                        onClick={() => { setAppliedVoucher(null); setVoucherCode(''); }} 
                        className="p-1 hover:bg-emerald-100 rounded-full text-slate-400 transition-colors"
                      >
                        <X size={16} />
                      </button>
                    </motion.div>
                )}

                {/* Coupon Selector Modal */}
                <CouponSelector 
                  isOpen={showCouponSelector}
                  onClose={() => setShowCouponSelector(false)}
                  currentSubtotal={subtotal}
                  onSelect={(code) => {
                    setVoucherCode(code);
                    setShowCouponSelector(false);
                    // Automatically trigger apply after selection
                    setTimeout(() => handleApplyVoucher(), 100);
                  }}
                />

                <div className="flex justify-between mt-4">
                  <span className="text-secondary">{t('cart.subtotal')}</span>
                  <span>{formatCurrency(subtotal)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-secondary">{t('cart.shipping')}</span>
                  <span className={shippingFee === 0 ? 'text-success' : ''}>
                    {shippingFee === 0 ? t('cart.free') : formatCurrency(shippingFee)}
                  </span>
                </div>

                {/* Discount Row */}
                {discountAmount > 0 && (
                  <div className="flex justify-between text-success">
                    <span>{t('cart.discount')}</span>
                    <span>-{formatCurrency(discountAmount)}</span>
                  </div>
                )}

                <hr />
                <div className="flex justify-between text-lg font-bold">
                  <span>{t('cart.total')}</span>
                  <span className="text-error">{formatCurrency(finalTotal)}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Login Required Modal */}
      <Modal
        isOpen={showLoginModal}
        onClose={() => { }} // Block closing by clicking outside? Modal 'closeOnOverlayClick' defaults true.
        // If mandatory, we should disable closing.
        // CheckoutPage usually implies Intent to Checkout. If they cancel, they go back to Cart?
        title={t('checkout.loginRequired') || 'Đăng nhập để thanh toán'}
        showCloseButton={false}
        closeOnOverlayClick={false}
        closeOnEsc={false}
        footer={
          <div className="flex gap-4 w-full justify-end">
            <Button variant="ghost" onClick={() => setIsGuestCheckout(true)}>
              {t('checkout.guestCheckout') || 'Mua hàng không cần đăng nhập'}
            </Button>
            <Link to="/register" state={{ from: '/checkout' }}>
              <Button variant="outline">{t('auth.register')}</Button>
            </Link>
            <Link to="/auth/login" state={{ from: '/checkout' }}>
              <Button>{t('auth.login')}</Button>
            </Link>
          </div>
        }
      >
        <div className="text-secondary">
          <p className="mb-4">
            {t('checkout.loginMessage') || 'Vui lòng đăng nhập hoặc đăng ký tài khoản để tiếp tục thanh toán.'}
          </p>
          <ul className="list-disc list-inside space-y-1 text-sm text-tertiary">
            <li>Tra cứu đơn hàng dễ dàng</li>
            <li>Tích điểm thành viên</li>
            <li>Lưu địa chỉ giao hàng</li>
          </ul>
        </div>
      </Modal>

      {/* Address Selection Modal */}
      <Modal
        isOpen={showAddressModal}
        onClose={() => setShowAddressModal(false)}
        title={t('checkout.selectAddress', 'Chọn địa chỉ giao hàng')}
        size="xl"
      >
        <div className="space-y-4 max-h-[60vh] overflow-y-auto pr-2">
          {savedAddresses.map((addr) => (
            <div 
              key={addr.code}
              className="p-4 border-2 border-secondary/5 rounded-2xl hover:border-indigo-500/50 hover:bg-indigo-50/10 cursor-pointer transition-all group"
              onClick={() => {
                setFormData({
                  fullName: addr.fullName || user?.fullName || '',
                  phone: addr.phone || user?.phone || '',
                  email: user?.email || '',
                  address: addr.addressLine,
                  city: addr.city,
                  district: addr.state || '',
                  ward: addr.postalCode || '',
                });
                setShowAddressModal(false);
              }}
            >
              <div className="flex justify-between items-start mb-2">
                <div className="flex items-center gap-2">
                  <span className="font-bold text-primary">{addr.fullName || user?.fullName}</span>
                  {addr.isDefault && <Badge color="success" className="text-[10px] uppercase">{t('common.default')}</Badge>}
                  {addr.category && <Badge variant="outline" className="text-[10px] uppercase">{addr.category}</Badge>}
                </div>
                <div className="text-indigo-600 opacity-0 group-hover:opacity-100 transition-opacity">
                  <ChevronRight size={18} />
                </div>
              </div>
              <p className="text-sm text-secondary mb-1">{addr.phone || user?.phone}</p>
              <p className="text-sm text-tertiary leading-relaxed">
                {addr.addressLine}{addr.city ? `, ${addr.city}` : ''}
              </p>
            </div>
          ))}
        </div>
      </Modal>
    </div>
  );
};

export default CheckoutPage;
