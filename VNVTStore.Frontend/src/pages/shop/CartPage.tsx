import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Minus, Plus, X, ShoppingBag, ArrowRight, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui';
import { useCartStore } from '@/store';
import { formatCurrency } from '@/utils/format';

export const CartPage = () => {
  const { t } = useTranslation();
  const { items, removeItem, updateQuantity, getTotal, clearCart, isLoading } = useCartStore();
  const total = getTotal();
  const shippingFee = total >= 500000 ? 0 : 30000;
  const grandTotal = total + shippingFee;

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
                key={item.id}
                layout
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, x: -20 }}
                className="grid grid-cols-1 md:grid-cols-12 gap-4 p-4 bg-primary rounded-xl items-center"
              >
                {/* Product */}
                <div className="md:col-span-6 flex gap-4">
                  <Link to={`/product/${item.product.id}`} className="flex-shrink-0">
                    <img
                      src={item.product.image}
                      alt={item.product.name}
                      className="w-20 h-20 object-cover rounded-lg"
                    />
                  </Link>
                  <div className="flex-1 min-w-0">
                    <Link
                      to={`/product/${item.product.id}`}
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
                      onClick={() => updateQuantity(item.id, Math.max(1, item.quantity - 1))}
                      className="p-2 hover:bg-secondary transition-colors"
                    >
                      <Minus size={16} />
                    </button>
                    <span className="w-12 text-center font-medium">{item.quantity}</span>
                    <button
                      onClick={() => updateQuantity(item.id, item.quantity + 1)}
                      className="p-2 hover:bg-secondary transition-colors"
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
                    onClick={() => removeItem(item.id)}
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
                {total < 500000 && (
                  <p className="text-xs text-tertiary">
                    {t('cart.freeShippingNote', { amount: formatCurrency(500000 - total) })}
                  </p>
                )}
                <hr />
                <div className="flex justify-between text-lg font-bold">
                  <span>{t('cart.total')}</span>
                  <span className="text-error">{formatCurrency(grandTotal)}</span>
                </div>
              </div>

              {/* Coupon */}
              <div className="mb-6">
                <div className="flex gap-2">
                  <input
                    type="text"
                    placeholder={t('cart.couponPlaceholder')}
                    className="flex-1 px-4 py-2 border rounded-lg text-sm focus:outline-none focus:border-primary"
                  />
                  <Button variant="outline" size="sm">
                    {t('cart.apply')}
                  </Button>
                </div>
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
    </div>
  );
};

export default CartPage;
