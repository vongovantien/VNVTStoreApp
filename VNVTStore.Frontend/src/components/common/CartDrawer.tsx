import { memo } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { X, Minus, Plus, ShoppingBag, ArrowRight, Truck, Info, Zap } from 'lucide-react';
import { Button } from '@/components/ui';
import SharedImage from '@/components/common/Image';
import { useCartStore, useUIStore } from '@/store';
import { formatCurrency } from '@/utils/format';
import { cn } from '@/utils/cn';


const FREE_SHIPPING_THRESHOLD = 2000000; // 2 million VND

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
            className="fixed inset-0 bg-black/50 z-[150]"
            onClick={() => setCartOpen(false)}
          />

          {/* Drawer */}
          <motion.div
            initial={{ x: '100%' }}
            animate={{ x: 0 }}
            exit={{ x: '100%' }}
            transition={{ type: 'tween', duration: 0.3 }}
            className="fixed top-0 right-0 bottom-0 w-full max-w-md bg-primary z-[150] flex flex-col shadow-2xl"
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
              <div className="flex-1 overflow-y-auto p-4 space-y-4 custom-scrollbar">
                {/* Feature 4: Free Shipping Meter */}
                <div className="bg-indigo-50 dark:bg-indigo-900/20 p-4 rounded-2xl border border-indigo-100 dark:border-indigo-800">
                   <div className="flex justify-between items-center mb-2">
                      <span className="text-xs font-bold text-indigo-700 dark:text-indigo-300 flex items-center gap-1.5">
                         <Truck size={14} />
                         {total >= FREE_SHIPPING_THRESHOLD ? 'Bạn được Miễn phí vận chuyển!' : `Mua thêm ${formatCurrency(FREE_SHIPPING_THRESHOLD - total)} để được Freeship`}
                      </span>
                      <span className="text-[10px] font-bold text-indigo-500">{Math.min(100, Math.round((total / FREE_SHIPPING_THRESHOLD) * 100))}%</span>
                   </div>
                   <div className="h-2 w-full bg-white dark:bg-slate-800 rounded-full overflow-hidden shadow-inner">
                      <motion.div 
                        initial={{ width: 0 }}
                        animate={{ width: `${Math.min(100, (total / FREE_SHIPPING_THRESHOLD) * 100)}%` }}
                        className={cn("h-full transition-all duration-500", total >= FREE_SHIPPING_THRESHOLD ? "bg-emerald-500" : "bg-indigo-500")}
                      />
                   </div>
                </div>

                {/* Feature 6: Wholesale Prompt */}
                {items.some(i => i.product.wholesalePrice) && (
                   <div className="bg-amber-50 dark:bg-amber-900/20 p-3 rounded-xl border border-amber-100 dark:border-amber-800 flex items-start gap-3">
                      <Zap size={16} className="text-amber-500 shrink-0 mt-0.5" />
                      <div>
                        <p className="text-xs font-bold text-amber-800 dark:text-amber-300">Ưu đãi giá sỉ!</p>
                        <p className="text-[10px] text-amber-600 dark:text-amber-400">Mua từ 10 sản phẩm cùng loại để nhận giá sỉ cực tốt.</p>
                      </div>
                   </div>
                )}
                {items.map((item) => (
                  <motion.div
                    key={item.code}
                    layout
                    initial={{ opacity: 0, x: 20 }}
                    animate={{ opacity: 1, x: 0 }}
                    exit={{ opacity: 0, x: -20 }}
                    className="flex gap-4 p-3 bg-secondary rounded-xl"
                  >
                    {/* Image */}
                    <Link
                      to={`/product/${item.product.code}`}
                      onClick={() => setCartOpen(false)}
                      className="flex-shrink-0"
                    >
                      <SharedImage
                        src={item.product.image}
                        alt={item.product.name}
                        className="w-20 h-20 object-cover rounded-lg"
                      />
                    </Link>

                    {/* Info */}
                    <div className="flex-1 min-w-0">
                      <Link
                        to={`/product/${item.product.code}`}
                        onClick={() => setCartOpen(false)}
                        className="font-medium text-primary hover:text-primary-dark transition-colors line-clamp-2"
                      >
                        {item.product.name}
                      </Link>
                      {(item.size || item.color) && (
                        <div className="flex flex-wrap gap-2 mt-1 text-xs text-secondary">
                          {item.size && (
                            <span className="bg-tertiary/20 px-1.5 py-0.5 rounded border">
                              {t('product.size') || 'Size'}: {item.size}
                            </span>
                          )}
                          {item.color && (
                            <span className="bg-tertiary/20 px-1.5 py-0.5 rounded border">
                              {t('product.color') || 'Color'}: {item.color}
                            </span>
                          )}
                        </div>
                      )}
                      <p className="text-error font-bold mt-1">
                        {formatCurrency(item.product.price)}
                      </p>

                      {/* Quantity controls */}
                      <div className="flex items-center justify-between mt-2">
                        <div className="flex items-center border rounded-lg overflow-hidden">
                          <button
                            onClick={() => updateQuantity(item.code, Math.max(1, item.quantity - 1))}
                            className="p-1.5 hover:bg-tertiary transition-colors"
                            disabled={item.quantity <= 1}
                          >
                            <Minus size={14} />
                          </button>
                          <span className="px-3 text-sm font-medium">{item.quantity}</span>
                          <button
                            onClick={() => updateQuantity(item.code, item.quantity + 1)}
                            className="p-1.5 hover:bg-tertiary transition-colors"
                          >
                            <Plus size={14} />
                          </button>
                        </div>

                        <button
                          onClick={() => removeItem(item.code)}
                          className="text-tertiary hover:text-error transition-colors"
                        >
                          <X size={18} />
                        </button>
                      </div>
                    </div>
                  </motion.div>
                ))}

                {/* Feature 10: Cross-sell Recommendations */}
                <div className="pt-4 border-t">
                  <h4 className="text-xs font-bold uppercase tracking-wider text-tertiary mb-3 flex items-center gap-2">
                    <Info size={14} />
                    {t('cart.recommendations', 'Thường được mua cùng')}
                  </h4>
                  <div className="flex gap-3 overflow-x-auto pb-2 custom-scrollbar">
                     {/* Mocked recommendations or fetch via hook */}
                     {[1,2,3].map(i => (
                       <div key={i} className="min-w-[120px] bg-secondary rounded-xl p-2 shrink-0 border border-transparent hover:border-indigo-200 transition-colors cursor-pointer">
                          <div className="aspect-square bg-white rounded-lg mb-2 overflow-hidden">
                             <img src={`https://picsum.photos/seed/${i+10}/200`} className="w-full h-full object-cover" />
                          </div>
                          <p className="text-[10px] font-bold line-clamp-1">Sản phẩm gợi ý #{i}</p>
                          <p className="text-[10px] text-indigo-600 font-bold">{formatCurrency(150000)}</p>
                       </div>
                     ))}
                  </div>
                </div>
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
