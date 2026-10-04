import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { AnimatePresence, motion } from 'framer-motion';
import {
  Heart, ShoppingCart, Trash2,
  LayoutGrid, LayoutList, Grid2x2,
} from 'lucide-react';
import { Button } from '@/components/ui';
import { ProductCard } from '@/components/common/ProductCard';
import { useWishlistStore, useCartStore } from '@/store';
import { cn } from '@/utils/cn';

// ── View mode type ──────────────────────────────────────────────────────────
type ViewMode = 'grid2' | 'grid3' | 'grid4' | 'list';

const VIEW_OPTIONS: { mode: ViewMode; icon: React.ReactNode; label: string }[] = [
  { mode: 'grid2', icon: <Grid2x2 size={16} />,   label: '2 cột' },
  { mode: 'grid3', icon: <LayoutGrid size={16} />, label: '3 cột' },
  { mode: 'grid4', icon: <LayoutGrid size={14} />, label: '4 cột' },
  { mode: 'list',  icon: <LayoutList size={16} />, label: 'Danh sách' },
];

const GRID_CLASS: Record<ViewMode, string> = {
  grid2: 'grid grid-cols-1 sm:grid-cols-2 gap-4',
  grid3: 'grid grid-cols-2 sm:grid-cols-3 gap-4',
  grid4: 'grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3',
  list:  'flex flex-col gap-3',
};

// ── Component ────────────────────────────────────────────────────────────────
export const WishlistContent = () => {
  const { t } = useTranslation();
  const { items, clearWishlist } = useWishlistStore();
  const addToCart = useCartStore((s) => s.addItem);
  const [viewMode, setViewMode] = useState<ViewMode>('grid3');

  const handleAddAllToCart = () => {
    items.forEach((product) => {
      if (product.price > 0) addToCart(product);
    });
  };

  // ── Empty state ─────────────────────────────────────────────────────────
  if (items.length === 0) {
    return (
      <div className="bg-bg-primary rounded-xl p-6 border border-border shadow-sm min-h-[400px] flex flex-col items-center justify-center text-center">
        <div className="w-20 h-20 bg-bg-tertiary rounded-full flex items-center justify-center mb-6">
          <Heart size={40} className="text-text-tertiary" />
        </div>
        <h3 className="text-xl font-bold mb-2 text-text-primary">{t('wishlist.empty')}</h3>
        <p className="text-text-secondary mb-6">{t('wishlist.emptyMessage')}</p>
        <Link to="/products">
          <Button size="lg">{t('common.browseProducts')}</Button>
        </Link>
      </div>
    );
  }

  // ── Main ────────────────────────────────────────────────────────────────
  return (
    <div className="space-y-4">
      <div className="bg-bg-primary rounded-xl p-5 border border-border shadow-sm">

        {/* ── Header ── */}
        <div className="flex flex-wrap justify-between items-center gap-3 mb-5">
          {/* Title + count */}
          <div>
            <h2 className="text-lg font-bold flex items-center gap-2 text-text-primary">
              <Heart className="text-rose-500" size={20} />
              {t('wishlist.title')}
            </h2>
            <p className="text-text-tertiary text-xs mt-0.5">
              {items.length} {t('wishlist.items')}
            </p>
          </div>

          {/* Right: view toggle + actions */}
          <div className="flex items-center gap-2 flex-wrap">
            {/* View mode toggle */}
            <div className="flex items-center gap-0.5 bg-bg-secondary border border-border rounded-lg p-0.5">
              {VIEW_OPTIONS.map(({ mode, icon, label }) => (
                <button
                  key={mode}
                  onClick={() => setViewMode(mode)}
                  title={label}
                  className={cn(
                    'p-1.5 rounded-md transition-all text-text-secondary',
                    viewMode === mode
                      ? 'bg-bg-primary text-accent shadow-sm'
                      : 'hover:bg-bg-tertiary',
                  )}
                >
                  {icon}
                </button>
              ))}
            </div>

            {/* Add all to cart */}
            <Button
              variant="outline"
              size="sm"
              onClick={handleAddAllToCart}
              leftIcon={<ShoppingCart size={14} />}
            >
              {t('wishlist.addAllToCart')}
            </Button>

            {/* Clear all */}
            <Button
              variant="ghost"
              size="sm"
              onClick={clearWishlist}
              leftIcon={<Trash2 size={14} />}
              className="text-error hover:bg-error/10"
            >
              {t('wishlist.clearAll')}
            </Button>
          </div>
        </div>

        {/* ── Product grid / list ── */}
        <motion.div layout className={GRID_CLASS[viewMode]}>
          <AnimatePresence>
            {items.map((product) => (
              <motion.div
                key={product.code}
                layout
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                exit={{ opacity: 0, scale: 0.9 }}
                transition={{ duration: 0.2 }}
              >
                <ProductCard
                  product={product}
                  variant={viewMode === 'list' ? 'list' : 'grid'}
                  hoverable
                />
              </motion.div>
            ))}
          </AnimatePresence>
        </motion.div>
      </div>
    </div>
  );
};

export default WishlistContent;
