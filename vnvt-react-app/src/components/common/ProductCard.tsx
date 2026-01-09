import { memo, useCallback, useMemo } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Heart, ShoppingCart, Star, Eye, Scale, Phone } from 'lucide-react';
import { cn } from '@/utils/cn';
import { Button, Badge } from '@/components/ui';
import { useCartStore, useWishlistStore, useCompareStore, useToast } from '@/store';
import { formatCurrency } from '@/utils/format';
import type { Product } from '@/types';
import CustomImage from '@/components/common/Image';

// ============ ProductCard Props Interface ============
export interface ProductCardProps {
  /** Product data */
  product: Product;
  /** Show quick view button */
  showQuickView?: boolean;
  /** Card layout variant */
  variant?: 'grid' | 'list' | 'compact';
  /** Enable hover effects */
  hoverable?: boolean;
  /** Show actions on hover only */
  actionsOnHover?: boolean;
  /** Custom className */
  className?: string;
  /** Quick view handler */
  onQuickView?: (product: Product) => void;
}

// ============ ProductCard Component ============
export const ProductCard = memo(
  ({
    product,
    showQuickView = true,
    variant = 'grid',
    hoverable = true,
    actionsOnHover = true,
    className,
    onQuickView,
  }: ProductCardProps) => {
    const { t } = useTranslation();
    
    // Store actions
    const addToCart = useCartStore((state) => state.addItem);
    const { addItem: addToWishlist, removeItem: removeFromWishlist, isInWishlist } = useWishlistStore();
    const { addItem: addToCompare, removeItem: removeFromCompare, isInCompare } = useCompareStore();
    const toast = useToast();

    // Derived states
    const isWishlisted = isInWishlist(product.id);
    const isCompared = isInCompare(product.id);
    const hasFixedPrice = product.price > 0;
    const hasDiscount = product.discount && product.discount > 0;
    const isLowStock = product.stock <= 5 && product.stock > 0;
    const isOutOfStock = product.stock === 0;

    // Handlers with useCallback for memoization
    const handleWishlistToggle = useCallback(
      (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        isWishlisted ? removeFromWishlist(product.id) : addToWishlist(product);
      },
      [isWishlisted, product, addToWishlist, removeFromWishlist]
    );

    const handleCompareToggle = useCallback(
      (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        isCompared ? removeFromCompare(product.id) : addToCompare(product);
      },
      [isCompared, product, addToCompare, removeFromCompare]
    );

    const handleAddToCart = useCallback(
      (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        if (hasFixedPrice && !isOutOfStock) {
          addToCart(product);
          toast.success(`${product.name} đã được thêm vào giỏ hàng`);
        }
      },
      [hasFixedPrice, isOutOfStock, product, addToCart, toast]
    );

    const handleQuickView = useCallback(
      (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        onQuickView?.(product);
      },
      [product, onQuickView]
    );

    // Render stars
    const stars = useMemo(
      () =>
        Array.from({ length: 5 }).map((_, i) => (
          <Star
            key={i}
            size={12}
            className={cn(
              i < Math.floor(product.rating)
                ? 'fill-yellow-400 text-yellow-400'
                : 'fill-gray-200 text-gray-200'
            )}
          />
        )),
      [product.rating]
    );

    // List variant
    if (variant === 'list') {
      return (
        <div className={cn(
          'flex gap-4 p-4 bg-white rounded-2xl shadow-sm border border-slate-100',
          hoverable && 'hover:shadow-lg transition-all duration-300',
          className
        )}>
          {/* Image */}
          <Link to={`/product/${product.id}`} className="flex-shrink-0">
            <div className="relative w-40 h-40 rounded-lg overflow-hidden bg-secondary">
              <CustomImage
                src={product.image}
                alt={product.name}
                className="w-full h-full object-cover"
                loading="lazy"
              />
              {product.isNew && <Badge color="success" className="absolute top-2 left-2">{t('product.new')}</Badge>}
              {hasDiscount && <Badge color="error" className="absolute top-2 left-2">{`-${product.discount}%`}</Badge>}
            </div>
          </Link>

          {/* Content */}
          <div className="flex-1 flex flex-col justify-between">
            <div>
              <Link to={`/products?category=${product.categoryId}`} className="text-xs text-primary font-medium uppercase tracking-wide">
                {product.category}
              </Link>
              <Link to={`/product/${product.id}`}>
                <h3 className="font-semibold text-primary mt-1 hover:text-primary-dark transition-colors">
                  {product.name}
                </h3>
              </Link>
              <p className="text-sm text-secondary mt-1 line-clamp-2">{product.description}</p>
              <div className="flex items-center gap-2 mt-2">
                <div className="flex">{stars}</div>
                <span className="text-xs text-tertiary">({product.reviewCount})</span>
              </div>
            </div>

            <div className="flex items-center justify-between mt-4">
              {/* Price */}
              <div className="flex items-center gap-2">
                {hasFixedPrice ? (
                  <>
                    <span className="text-lg font-bold text-error">{formatCurrency(product.price)}</span>
                    {product.originalPrice && (
                      <span className="text-sm text-tertiary line-through">{formatCurrency(product.originalPrice)}</span>
                    )}
                  </>
                ) : (
                  <span className="text-base font-semibold text-primary">{t('product.contactForPrice')}</span>
                )}
              </div>

              {/* Actions */}
              <div className="flex gap-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={handleWishlistToggle}
                  className={cn(isWishlisted && 'text-secondary')}
                >
                  <Heart size={18} fill={isWishlisted ? 'currentColor' : 'none'} />
                </Button>
                {hasFixedPrice ? (
                  <Button size="sm" onClick={handleAddToCart} disabled={isOutOfStock}>
                    <ShoppingCart size={16} />
                    {t('product.addToCart')}
                  </Button>
                ) : (
                  <Button size="sm" variant="outline" leftIcon={<Phone size={16} />}>
                    {t('product.requestQuote')}
                  </Button>
                )}
              </div>
            </div>
          </div>
        </div>
      );
    }

    // Grid variant (default)
    return (
      <motion.div
        className={cn(
          'group relative bg-white rounded-2xl overflow-hidden shadow-sm border border-slate-100 flex flex-col',
          hoverable && 'hover:-translate-y-1 hover:shadow-xl transition-all duration-300',
          className
        )}
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3 }}
      >
        <Link to={`/product/${product.id}`} className="flex-1 flex flex-col">
            {/* Image Container */}
          <div className="relative aspect-square overflow-hidden bg-slate-50 rounded-t-2xl">
            <CustomImage
              src={product.image}
              alt={product.name}
              className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105"
              loading="lazy"
            />

            {/* Badges */}
            <div className="absolute top-2 left-2 flex flex-col gap-1">
              {product.isNew && <Badge color="success" size="sm">{t('product.new')}</Badge>}
              {hasDiscount && <Badge color="error" size="sm">{`-${product.discount}%`}</Badge>}
              {!hasFixedPrice && <Badge color="primary" size="sm">{t('product.contactForPrice')}</Badge>}
            </div>

            {/* Quick Actions */}
            <div
              className={cn(
                'absolute top-2 right-2 flex flex-col gap-1 transition-all duration-300',
                actionsOnHover && 'opacity-0 translate-x-2 group-hover:opacity-100 group-hover:translate-x-0'
              )}
            >
              <button
                onClick={handleWishlistToggle}
                className={cn(
                  'w-9 h-9 flex items-center justify-center bg-white rounded-full shadow-md transition-all hover:scale-110',
                  isWishlisted && 'text-rose-500'
                )}
              >
                <Heart size={16} fill={isWishlisted ? 'currentColor' : 'none'} />
              </button>
              <button
                onClick={handleCompareToggle}
                className={cn(
                  'w-9 h-9 flex items-center justify-center bg-white rounded-full shadow-md transition-all hover:scale-110',
                  isCompared && 'bg-indigo-600 text-white'
                )}
              >
                <Scale size={16} />
              </button>
              {showQuickView && (
                <button
                  onClick={handleQuickView}
                  className="w-9 h-9 flex items-center justify-center bg-white rounded-full shadow-md transition-all hover:scale-110"
                >
                  <Eye size={16} />
                </button>
              )}
            </div>
          </div>

          {/* Content */}
          <div className="p-4 flex flex-col flex-1">
            <span className="text-xs text-indigo-600 font-semibold uppercase tracking-wide">
              {product.category}
            </span>
            <h3 className="font-semibold text-slate-800 mt-1 line-clamp-2 group-hover:text-indigo-600 transition-colors">
              {product.name}
            </h3>

            {/* Rating */}
            <div className="flex items-center gap-1 mt-2">
              {stars}
              <span className="text-xs text-tertiary ml-1">({product.reviewCount})</span>
            </div>

            {/* Price */}
            <div className="mt-auto pt-3 flex items-center gap-2">
              {hasFixedPrice ? (
                <>
                  <span className="text-lg font-bold text-error">{formatCurrency(product.price)}</span>
                  {product.originalPrice && product.originalPrice > product.price && (
                    <span className="text-sm text-tertiary line-through">{formatCurrency(product.originalPrice)}</span>
                  )}
                </>
              ) : (
                <span className="text-base font-semibold text-primary">{t('product.contactForPrice')}</span>
              )}
            </div>

            {/* Stock warning */}
            {isLowStock && (
              <span className="text-xs text-warning font-medium mt-1">
                {t('product.lowStock', { count: product.stock })}
              </span>
            )}
            {isOutOfStock && (
              <span className="text-xs text-error font-medium mt-1">{t('product.outOfStock')}</span>
            )}
          </div>
        </Link>

        {/* Add to Cart Button */}
        <div className="p-4 pt-0">
          {hasFixedPrice ? (
            <Button
              fullWidth
              onClick={handleAddToCart}
              disabled={isOutOfStock}
              leftIcon={<ShoppingCart size={16} />}
            >
              {isOutOfStock ? t('product.outOfStock') : t('product.addToCart')}
            </Button>
          ) : (
            <Button
              fullWidth
              variant="outline"
              leftIcon={<Phone size={16} />}
              onClick={(e) => {
                e.preventDefault();
                window.location.href = `/quote-request/${product.id}`;
              }}
            >
              {t('product.requestQuote')}
            </Button>
          )}
        </div>
      </motion.div>
    );
  }
);

ProductCard.displayName = 'ProductCard';

export default ProductCard;
