import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Heart, ShoppingCart, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui';
import { ProductCard } from '@/components/common/ProductCard';
import { useWishlistStore, useCartStore } from '@/store';

export const WishlistContent = () => {
  const { t } = useTranslation();
  const { items, clearWishlist } = useWishlistStore();
  const addToCart = useCartStore((state) => state.addItem);

  const handleAddAllToCart = () => {
    items.forEach((product) => {
      if (product.price > 0) {
        addToCart(product);
      }
    });
  };

  if (items.length === 0) {
    return (
      <div className="bg-primary rounded-xl p-6 border shadow-sm min-h-[400px] flex flex-col items-center justify-center text-center">
        <div className="w-20 h-20 bg-tertiary/10 rounded-full flex items-center justify-center mb-6">
          <Heart size={40} className="text-tertiary" />
        </div>
        <h3 className="text-xl font-bold mb-2">{t('wishlist.empty')}</h3>
        <p className="text-secondary mb-6">{t('wishlist.emptyMessage')}</p>
        <Link to="/products">
          <Button size="lg">{t('common.browseProducts')}</Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="bg-primary rounded-xl p-6 border shadow-sm">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
          <div>
            <h2 className="text-xl font-bold flex items-center gap-2">
              <Heart className="text-tertiary" size={24} />
              {t('wishlist.title')}
            </h2>
            <p className="text-secondary text-sm mt-1">
              {items.length} {t('wishlist.items')}
            </p>
          </div>
          <div className="flex gap-3">
            <Button
              variant="outline"
              size="sm"
              onClick={handleAddAllToCart}
              leftIcon={<ShoppingCart size={16} />}
            >
              {t('wishlist.addAllToCart')}
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={clearWishlist}
              leftIcon={<Trash2 size={16} />}
            >
              {t('wishlist.clearAll')}
            </Button>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {items.map((product) => (
            <motion.div
              key={product.code}
              layout
              initial={{ opacity: 0, scale: 0.9 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.9 }}
            >
              <ProductCard product={product} />
            </motion.div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default WishlistContent;
