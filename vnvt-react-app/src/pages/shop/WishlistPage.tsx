import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Heart, ShoppingCart, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui';
import { ProductCard } from '@/components/common/ProductCard';
import { useWishlistStore, useCartStore } from '@/store';

export const WishlistPage = () => {
  const { t } = useTranslation();
  const { items, removeItem, clearWishlist } = useWishlistStore();
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
      <div className="min-h-screen bg-secondary">
        <div className="container mx-auto px-4 py-16 text-center">
          <div className="w-24 h-24 mx-auto bg-tertiary rounded-full flex items-center justify-center mb-6">
            <Heart size={48} className="text-tertiary" />
          </div>
          <h1 className="text-2xl font-bold mb-4">{t('wishlist.empty')}</h1>
          <p className="text-secondary mb-8">{t('wishlist.emptyMessage')}</p>
          <Link to="/products">
            <Button size="lg">{t('common.browseProducts')}</Button>
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-secondary">
      <div className="container mx-auto px-4 py-8">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8">
          <div>
            <h1 className="text-2xl md:text-3xl font-bold">{t('wishlist.title')}</h1>
            <p className="text-secondary">{items.length} {t('wishlist.items')}</p>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" onClick={handleAddAllToCart} leftIcon={<ShoppingCart size={18} />}>
              {t('wishlist.addAllToCart')}
            </Button>
            <Button variant="ghost" onClick={clearWishlist} leftIcon={<Trash2 size={18} />}>
              {t('wishlist.clearAll')}
            </Button>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {items.map((product) => (
            <motion.div
              key={product.id}
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

export default WishlistPage;
