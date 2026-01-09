import { memo } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { X, Minus, Plus, ShoppingBag, ArrowRight } from 'lucide-react';
import { cn } from '@/utils/cn';
import { Button } from '@/components/ui';
import { useCartStore, useUIStore } from '@/store';
import { formatCurrency } from '@/utils/format';

export const CartDrawer = memo(() => {
  const { t } = useTranslation();
  const { cartOpen, setCartOpen } = useUIStore();
  const { items, removeItem, updateQuantity, getTotal, clearCart } = useCartStore();

  const total = getTotal();

  return (
    <AnimatePresence>
      {cartOpen && (
        <>
          {/* Overlay */}
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/50 z-50"
            onClick={() => setCartOpen(false)}
          />

          {/* Drawer */}
          <motion.div
            initial={{ x: '100%' }}
            animate={{ x: 0 }}
            exit={{ x: '100%' }}
            transition={{ type: 'tween', duration: 0.3 }}
            className="fixed top-0 right-0 bottom-0 w-full max-w-md bg-primary z-50 flex flex-col shadow-2xl"
          >
            {/* Header */}
            <div className="flex items-center justify-between p-4 border-b">
              <h2 className="text-lg font-bold flex items-center gap-2">
                <ShoppingBag size={20} />
                {t('cart.title')} ({items.length})
              </h2>
              <Button variant="ghost" size="sm" onClick={() => setCartOpen(false)}>
                <X size={24} />
              </Button>
            </div>

            {/* Cart Items */}
            {items.length === 0 ? (
              <div className="flex-1 flex flex-col items-center justify-center text-center p-8">
                <div className="w-24 h-24 bg-secondary rounded-full flex items-center justify-center mb-4">
                  <ShoppingBag size={48} className="text-tertiary" />
                </div>
                <h3 className="text-lg font-semibold text-primary mb-2">{t('cart.empty')}</h3>
                <p className="text-secondary mb-6">{t('cart.emptyMessage')}</p>
                <Button onClick={() => setCartOpen(false)}>
                  {t('cart.continueShopping')}
                </Button>
              </div>
            ) : (
              <div className="flex-1 overflow-y-auto p-4 space-y-4">
                {items.map((item) => (
                  <motion.div
                    key={item.id}
                    layout
                    initial={{ opacity: 0, x: 20 }}
                    animate={{ opacity: 1, x: 0 }}
                    exit={{ opacity: 0, x: -20 }}
                    className="flex gap-4 p-3 bg-secondary rounded-xl"
                  >
                    {/* Image */}
                    <Link
                      to={`/product/${item.product.id}`}
                      onClick={() => setCartOpen(false)}
                      className="flex-shrink-0"
                    >
                      <img
                        src={item.product.image}
                        alt={item.product.name}
                        className="w-20 h-20 object-cover rounded-lg"
                      />
                    </Link>

                    {/* Info */}
                    <div className="flex-1 min-w-0">
                      <Link
                        to={`/product/${item.product.id}`}
                        onClick={() => setCartOpen(false)}
                        className="font-medium text-primary hover:text-primary-dark transition-colors line-clamp-2"
                      >
                        {item.product.name}
                      </Link>
                      <p className="text-error font-bold mt-1">
                        {formatCurrency(item.product.price)}
                      </p>

                      {/* Quantity controls */}
                      <div className="flex items-center justify-between mt-2">
                        <div className="flex items-center border rounded-lg overflow-hidden">
                          <button
                            onClick={() => updateQuantity(item.id, Math.max(1, item.quantity - 1))}
                            className="p-1.5 hover:bg-tertiary transition-colors"
                            disabled={item.quantity <= 1}
                          >
                            <Minus size={14} />
                          </button>
                          <span className="px-3 text-sm font-medium">{item.quantity}</span>
                          <button
                            onClick={() => updateQuantity(item.id, item.quantity + 1)}
                            className="p-1.5 hover:bg-tertiary transition-colors"
                          >
                            <Plus size={14} />
                          </button>
                        </div>

                        <button
                          onClick={() => removeItem(item.id)}
                          className="text-tertiary hover:text-error transition-colors"
                        >
                          <X size={18} />
                        </button>
                      </div>
                    </div>
                  </motion.div>
                ))}
              </div>
            )}

            {/* Footer */}
            {items.length > 0 && (
              <div className="border-t p-4 space-y-4 bg-secondary/50">
                {/* Subtotal */}
                <div className="flex items-center justify-between">
                  <span className="text-secondary">{t('cart.subtotal')}</span>
                  <span className="text-xl font-bold text-primary">{formatCurrency(total)}</span>
                </div>

                {/* Actions */}
                <div className="space-y-2">
                  <Button
                    fullWidth
                    rightIcon={<ArrowRight size={18} />}
                    onClick={() => {
                      setCartOpen(false);
                      window.location.href = '/checkout';
                    }}
                  >
                    {t('cart.checkout')}
                  </Button>
                  <Link
                    to="/cart"
                    onClick={() => setCartOpen(false)}
                    className="block text-center text-sm text-primary hover:underline"
                  >
                    {t('cart.title')}
                  </Link>
                </div>

                {/* Clear cart */}
                <button
                  onClick={clearCart}
                  className="w-full text-center text-xs text-tertiary hover:text-error transition-colors"
                >
                  Xóa tất cả
                </button>
              </div>
            )}
          </motion.div>
        </>
      )}
    </AnimatePresence>
  );
});

CartDrawer.displayName = 'CartDrawer';

export default CartDrawer;
