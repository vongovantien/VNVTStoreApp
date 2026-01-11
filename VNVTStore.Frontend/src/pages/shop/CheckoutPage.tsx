import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ChevronRight, CreditCard, Truck, MapPin, Phone, User, Mail, FileText } from 'lucide-react';
import { Button, Input, Select, Modal } from '@/components/ui';
import { useCartStore, useAuthStore } from '@/store';
import { formatCurrency } from '@/utils/format';
import { orderService, type CreateOrderRequest } from '@/services/orderService';
import { paymentService } from '@/services/paymentService'; // Added import
import { PaymentMethod } from '@/constants';
import { useToast } from '@/store'; // Added useToast

export const CheckoutPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const toast = useToast(); // Added toast hook
  const { items, getTotal, clearCart, fetchCart } = useCartStore();
  const { user, isAuthenticated } = useAuthStore();
  const [showLoginModal, setShowLoginModal] = useState(false);

  useEffect(() => {
    if (!isAuthenticated) {
      setShowLoginModal(true);
    } else {
      setShowLoginModal(false);
    }
  }, [isAuthenticated]);

  const [step, setStep] = useState(1);
  const [paymentMethod, setPaymentMethod] = useState<string>(PaymentMethod.COD);
  const [isProcessing, setIsProcessing] = useState(false);

  const subtotal = getTotal();
  const shippingFee = subtotal >= 500000 ? 0 : 30000;
  const total = subtotal + shippingFee;

  const [formData, setFormData] = useState({
    fullName: user?.fullName || '',
    phone: user?.phone || '',
    email: user?.email || '',
    address: '',
    city: '',
    district: '',
    ward: '',
    note: '',
  });

  // Pre-fill if user has address (Logic omitted for brevity, could use userService.getMyAddresses())

  const handleInputChange = (field: string, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async () => {
    setIsProcessing(true);
    try {
      const orderData: CreateOrderRequest = {
        fullName: formData.fullName,
        phone: formData.phone,
        address: formData.address, // Should ideally include city/district/ward
        city: formData.city,
        district: formData.district,
        ward: formData.ward,
        note: formData.note,
        paymentMethod: paymentMethod
      };

      // 1. Create Order
      const orderRes = await orderService.create(orderData);

      if (orderRes.success && orderRes.data) {
        const orderCode = orderRes.data.code;
        toast.success(t('messages.orderSuccess') || 'Đặt hàng thành công!');

        // 2. Process Payment
        try {
          const totalAmount = orderRes.data.finalAmount || total;
          await paymentService.create({
            orderCode: orderCode,
            method: paymentMethod,
            amount: totalAmount
          });
          // Payment success (or pending for COD)
          // For online payment simulation, we might want to update status here or backend handles it
        } catch (paymentError) {
          console.error('Payment creation failed', paymentError);
          toast.error(t('messages.paymentError') || 'Có lỗi khi tạo thanh toán, vui lòng liên hệ CSKH.');
          // We still redirect because Order is created, just payment failed/pending
        }

        // 3. Clear Cart & Redirect
        await fetchCart();
        // Redirect to Orders page with success flag
        navigate('/account/orders?success=true');
      } else {
        toast.error(orderRes.message || t('messages.orderError') || 'Đặt hàng thất bại');
      }
    } catch (error: any) {
      console.error('Order failed', error);
      toast.error(error?.message || t('messages.generalError') || 'Có lỗi xảy ra');
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

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Input
                    label={t('checkout.fullName')}
                    placeholder="Nguyễn Văn A"
                    value={formData.fullName}
                    onChange={(e) => handleInputChange('fullName', e.target.value)}
                    leftIcon={<User size={18} />}
                    required
                  />
                  <Input
                    label={t('checkout.phone')}
                    placeholder="0901234567"
                    value={formData.phone}
                    onChange={(e) => handleInputChange('phone', e.target.value)}
                    leftIcon={<Phone size={18} />}
                    required
                  />
                  <div className="md:col-span-2">
                    <Input
                      label={t('checkout.email')}
                      type="email"
                      placeholder="email@example.com"
                      value={formData.email}
                      onChange={(e) => handleInputChange('email', e.target.value)}
                      leftIcon={<Mail size={18} />}
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
                  />
                  <div className="md:col-span-2">
                    <Input
                      label={t('checkout.address')}
                      placeholder="Số nhà, tên đường..."
                      value={formData.address}
                      onChange={(e) => handleInputChange('address', e.target.value)}
                      leftIcon={<MapPin size={18} />}
                      required
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
                  <Button size="lg" onClick={() => setStep(2)}>
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
                    { value: 'ZALOPAY', label: 'ZaloPay', icon: '💳' }, // Use Constants if available
                    { value: 'MOMO', label: 'Ví MoMo', icon: '📱' },
                    { value: 'VNPAY', label: 'VNPAY QR', icon: '🏦' },
                    { value: 'BANK', label: 'Chuyển khoản ngân hàng', icon: '🏛️' },
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
                      <span className="text-2xl">{method.icon}</span>
                      <span className="font-medium">{method.label}</span>
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
                    <div key={item.id} className="flex items-center gap-4">
                      <img
                        src={item.product.image}
                        alt={item.product.name}
                        className="w-16 h-16 object-cover rounded-lg"
                      />
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
                  <div key={item.id} className="flex gap-3 text-sm">
                    <img
                      src={item.product.image}
                      alt={item.product.name}
                      className="w-12 h-12 object-cover rounded"
                    />
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
                <div className="flex justify-between">
                  <span className="text-secondary">{t('cart.subtotal')}</span>
                  <span>{formatCurrency(subtotal)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-secondary">{t('cart.shipping')}</span>
                  <span className={shippingFee === 0 ? 'text-success' : ''}>
                    {shippingFee === 0 ? t('cart.free') : formatCurrency(shippingFee)}
                  </span>
                </div>
                <hr />
                <div className="flex justify-between text-lg font-bold">
                  <span>{t('cart.total')}</span>
                  <span className="text-error">{formatCurrency(total)}</span>
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
            <Link to="/cart">
              <Button variant="ghost">{t('common.back')}</Button>
            </Link>
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
    </div>
  );
};

export default CheckoutPage;
