import { useState } from 'react'; // Feature 12
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Minus, Plus, X, ShoppingBag, ArrowRight, Trash2, Gift, Scale, Truck, Ticket, Tag } from 'lucide-react'; // Added icons
import { Button } from '@/components/ui';
import { CouponSelector } from '@/components/common/CouponSelector';
import SharedImage from '@/components/common/Image';
import { useCartStore, useToast } from '@/store';
import { formatCurrency, formatNumber } from '@/utils/format'; // Added formatNumber

import { useSEO } from '@/hooks/useSEO';

export const CartPage = () => {
  const { t } = useTranslation();
  
  useSEO({
    title: 'Giỏ hàng',
    noindex: true,
  });
  const { items, removeItem, updateQuantity, getTotal, clearCart, isLoading, coupon, discountAmount, applyCoupon, removeCoupon } = useCartStore();
  const [isCouponDrawerOpen, setIsCouponDrawerOpen] = useState(false);
  const { error, success } = useToast();
  
  // Feature 12: Gift Wrapping State
  const [isGiftWrapped, setIsGiftWrapped] = useState(false);
  const GIFT_WRAP_FEE = 20000;

  const total = getTotal();
  const shippingFee = total >= 500000 ? 0 : 30000;
  
  // Feature 15: Cart Weight Estimator (Mock 500g per item if missing)
  const totalWeight = items.reduce((acc, item) => acc + ((item.product.weight || 0.5) * item.quantity), 0);
  
  const grandTotal = total + shippingFee + (isGiftWrapped ? GIFT_WRAP_FEE : 0) - discountAmount;
  const freeShipProgress = Math.min((total / 500000) * 100, 100);

  const handleApplyCoupon = async (code: string) => {
      try {
          await applyCoupon(code);
          success(t('cart.couponApplied', 'Áp dụng mã giảm giá thành công'));
          setIsCouponDrawerOpen(false);
      } catch (err: unknown) {
          error((err as Error).message || t('cart.couponError', 'Lỗi áp dụng mã giảm giá'));
      }
  };

  if (isLoading && items.length === 0) {
    return (
        <div className="min-h-screen bg-secondary flex items-center justify-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
        </div>
    );
  }

  if (items.length === 0) {
    return (
      <div className="min-h-screen bg-secondary">
        <div className="container mx-auto px-4 py-16 text-center">
          <div className="w-24 h-24 mx-auto bg-tertiary rounded-full flex items-center justify-center mb-6">
            <ShoppingBag size={48} className="text-tertiary" />
          </div>
          <h1 className="text-2xl font-bold mb-4">{t('cart.empty')}</h1>
          <p className="text-secondary mb-8">{t('cart.emptyMessage')}</p>
          <Link to="/products">
            <Button size="lg">{t('cart.continueShopping')}</Button>
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-secondary">
      <div className="container mx-auto px-4 py-8">
        <h1 className="text-2xl md:text-3xl font-bold mb-8">{t('cart.title')}</h1>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Cart Items */}
          <div className="lg:col-span-2 space-y-4">
            {/* Header - Desktop */}
            <div className="hidden md:grid grid-cols-12 gap-4 p-4 bg-primary rounded-xl font-semibold text-secondary">
              <div className="col-span-6">{t('cart.product')}</div>
              <div className="col-span-2 text-center">{t('cart.price')}</div>
              <div className="col-span-2 text-center">{t('cart.quantity')}</div>
              <div className="col-span-2 text-right">{t('cart.subtotal')}</div>
            </div>

            {/* Items */}
            {items.map((item) => (
              <motion.div
                key={item.code}
                layout
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, x: -20 }}
                className="grid grid-cols-1 md:grid-cols-12 gap-4 p-4 bg-primary rounded-xl items-center"
              >
                {/* Product */}
                <div className="md:col-span-6 flex gap-4">
                  <Link to={`/product/${item.product.code}`} className="flex-shrink-0">
                    <SharedImage
                      src={item.product.image}
                      alt={item.product.name}
                      className="w-20 h-20 object-cover rounded-lg"
                    />
                  </Link>
                  <div className="flex-1 min-w-0">
                    <Link
                      to={`/product/${item.product.code}`}
                      className="font-medium text-primary hover:text-primary-dark transition-colors line-clamp-2"
                    >
                      {item.product.name}
                    </Link>
                    <p className="text-sm text-tertiary">{item.product.category}</p>
                    {/* Mobile price */}
                    <p className="md:hidden text-error font-bold mt-1">
                      {formatCurrency(item.product.price)}
                    </p>
                  </div>
                </div>

                {/* Price - Desktop */}
                <div className="hidden md:block md:col-span-2 text-center font-medium text-error">
                  {formatCurrency(item.product.price)}
                </div>

                {/* Quantity */}
                <div className="md:col-span-2 flex justify-center">
                  <div className="flex items-center border rounded-lg">
                    <button
                      onClick={() => updateQuantity(item.code, Math.max(1, item.quantity - 1))}
                      className="p-2 hover:bg-hover transition-colors"
                    >
                      <Minus size={16} />
                    </button>
                    <span className="w-12 text-center font-medium">{item.quantity}</span>
                    <button
                      onClick={() => updateQuantity(item.code, item.quantity + 1)}
                      className="p-2 hover:bg-hover transition-colors"
                    >
                      <Plus size={16} />
                    </button>
                  </div>
                </div>

                {/* Subtotal & Remove */}
                <div className="md:col-span-2 flex items-center justify-between md:justify-end gap-4">
                  <span className="font-bold text-primary">
                    {formatCurrency(item.product.price * item.quantity)}
                  </span>
                  <button
                    onClick={() => removeItem(item.code)}
                    className="p-2 text-tertiary hover:text-error transition-colors"
                  >
                    <X size={18} />
                  </button>
                </div>
              </motion.div>
            ))}

            {/* Actions */}
            <div className="flex flex-col sm:flex-row justify-between gap-4 pt-4">
              <Link to="/products">
                <Button variant="outline">{t('cart.continueShopping')}</Button>
              </Link>
              <Button variant="ghost" onClick={clearCart} leftIcon={<Trash2 size={18} />}>
                {t('cart.clearCart')}
              </Button>
            </div>
          </div>

          {/* Summary */}
          <div className="lg:col-span-1">
            <div className="bg-primary rounded-xl p-6 sticky top-24">
              <h2 className="text-lg font-bold mb-4">{t('cart.orderSummary')}</h2>

              <div className="space-y-3 mb-6">
                <div className="flex justify-between text-secondary">
                  <span>{t('cart.subtotal')}</span>
                  <span>{formatCurrency(total)}</span>
                </div>
                <div className="flex justify-between text-secondary">
                  <span>{t('cart.shipping')}</span>
                  <span className={shippingFee === 0 ? 'text-success' : ''}>
                    {shippingFee === 0 ? t('cart.free') : formatCurrency(shippingFee)}
                  </span>
                </div>

                {/* Feature 12: Gift Wrapping Toggle */}
                <div className="flex items-center justify-between py-2 border-t border-dashed">
                  <div className="flex items-center gap-2">
                    <Gift size={16} className="text-indigo-500" />
                    <label htmlFor="gift-wrap" className="text-sm cursor-pointer select-none text-secondary hover:text-primary">
                      {t('cart.giftWrap', 'Gói quà (+20k)')}
                    </label>
                  </div>
                  <input 
                    type="checkbox" 
                    id="gift-wrap" 
                    checked={isGiftWrapped}
                    onChange={(e) => setIsGiftWrapped(e.target.checked)}
                    className="w-4 h-4 rounded text-indigo-600 focus:ring-indigo-500 cursor-pointer"
                  />
                </div>
                {isGiftWrapped && (
                  <div className="flex justify-between text-sm text-indigo-600">
                    <span>{t('cart.giftWrapFee', 'Phí gói quà')}</span>
                    <span>{formatCurrency(GIFT_WRAP_FEE)}</span>
                  </div>
                )}
                
                {/* Discount Row */}
                {discountAmount > 0 && (
                    <div className="flex justify-between text-sm text-success font-medium py-2 border-t border-dashed border-success/30">
                        <span className="flex items-center gap-1">
                            <Tag size={14} />
                            {t('cart.discount', 'Đã giảm')} ({coupon?.code})
                        </span>
                        <span>-{formatCurrency(discountAmount)}</span>
                    </div>
                )}

                {/* Feature 15: Weight Estimator */}
                <div className="flex items-center justify-between text-xs text-slate-400 pt-1">
                   <div className="flex items-center gap-1">
                     <Scale size={12} />
                     <span>{t('cart.estimatedWeight', 'Ước tính trọng lượng')}:</span>
                   </div>
                   <span>{formatNumber(totalWeight)} kg</span>
                </div>

                <div className="mb-4">
                  <div className="flex justify-between text-xs mb-1">
                    <span className="font-medium text-secondary">{t('cart.freeShipProgress', 'Tiến độ Freeship')}</span>
                    <span className="text-primary font-bold">{Math.round(freeShipProgress)}%</span>
                  </div>
                  <div className="h-2 w-full bg-slate-100 rounded-full overflow-hidden">
                    <motion.div 
                      initial={{ width: 0 }}
                      animate={{ width: `${freeShipProgress}%` }}
                      className={`h-full rounded-full ${freeShipProgress === 100 ? 'bg-success' : 'bg-primary'}`}
                    />
                  </div>
                  {total < 500000 ? (
                    <p className="text-xs text-tertiary mt-1">
                      {t('cart.freeShippingNote', { amount: formatCurrency(500000 - total) })}
                    </p>
                  ) : (
                    <p className="text-xs text-success font-bold mt-1 flex items-center gap-1">
                      <Truck size={12} /> {t('cart.freeShippingQualified', 'Đơn hàng đã được miễn phí vận chuyển!')}
                    </p>
                  )}
                </div>
                <hr />
                <div className="flex justify-between text-lg font-bold">
                  <span>{t('cart.total')}</span>
                  <span className="text-error">{formatCurrency(grandTotal)}</span>
                </div>
              </div>

              {/* Coupon Selector */}
              <div className="mb-6">
                {coupon ? (
                     <div className="bg-indigo-50 dark:bg-indigo-900/20 border border-indigo-100 dark:border-indigo-800 rounded-lg p-3 flex justify-between items-center">
                         <div className="flex items-center gap-2">
                             <Ticket size={18} className="text-indigo-600" />
                             <div>
                                 <div className="font-bold text-indigo-700 dark:text-indigo-400 text-sm">{coupon.code}</div>
                                 <div className="text-[10px] text-indigo-500">{t('cart.couponApplied')}</div>
                             </div>
                         </div>
                         <button onClick={removeCoupon} className="text-slate-400 hover:text-rose-500 p-1">
                             <X size={16} />
                         </button>
                     </div>
                ) : (
                    <Button 
                        variant="outline" 
                        fullWidth 
                        leftIcon={<Ticket size={18} />}
                        onClick={() => setIsCouponDrawerOpen(true)}
                        className="border-dashed"
                    >
                        {t('cart.selectCoupon', 'Chọn hoặc nhập mã giảm giá')}
                    </Button>
                )}
              </div>

              <Link to="/checkout">
                <Button fullWidth size="lg" rightIcon={<ArrowRight size={20} />}>
                  {t('cart.checkout')}
                </Button>
              </Link>

              {/* Payment methods */}
              <div className="mt-6 text-center">
                <p className="text-xs text-tertiary mb-2">{t('cart.securePayment')}</p>
                <div className="flex justify-center gap-2">
                  {['Visa', 'MC', 'ZaloPay', 'MoMo'].map((method) => (
                    <span key={method} className="px-2 py-1 bg-secondary rounded text-xs font-medium">
                      {method}
                    </span>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <CouponSelector 
        isOpen={isCouponDrawerOpen} 
        onClose={() => setIsCouponDrawerOpen(false)} 
        onSelect={handleApplyCoupon}
        currentSubtotal={total}
      />
    </div>
  );
};

export default CartPage;
