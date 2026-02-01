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
    const isWishlisted = isInWishlist(product.code);
    const isCompared = isInCompare(product.code);
    const hasFixedPrice = product.price > 0;
    const hasDiscount = product.discount && product.discount > 0;
    const isLowStock = (product.stockQuantity ?? product.stock) <= 5 && (product.stockQuantity ?? product.stock) > 0;
    const isOutOfStock = (product.stockQuantity ?? product.stock) === 0;
    
    // Calculate isNew based on createdAt within NEW_PRODUCT_DAYS, or use product.isNew if provided
    const NEW_PRODUCT_DAYS = 7;
    const isNew = useMemo(() => {
      if (product.isNew) return true;
      if (product.createdAt) {
        const createdDate = new Date(product.createdAt);
        const cutoffDate = new Date();
        cutoffDate.setDate(cutoffDate.getDate() - NEW_PRODUCT_DAYS);
        return createdDate >= cutoffDate;
      }
      return false;
    }, [product.isNew, product.createdAt]);

    // Handlers with useCallback for memoization
    const handleWishlistToggle = useCallback(
      (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        isWishlisted ? removeFromWishlist(product.code) : addToWishlist(product);
      },
      [isWishlisted, product, addToWishlist, removeFromWishlist]
    );

    const handleCompareToggle = useCallback(
      (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        isCompared ? removeFromCompare(product.code) : addToCompare(product);
      },
      [isCompared, product, addToCompare, removeFromCompare]
    );

    const handleAddToCart = useCallback(
      async (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        if (hasFixedPrice && !isOutOfStock) {
          try {
            await addToCart(product);
            toast.success(`${product.name} đã được thêm vào giỏ hàng`);
          } catch (error) {
            toast.error(t('product.addToCartError') || 'Có lỗi xảy ra khi thêm vào giỏ hàng');
          }
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
        Array.from({ length: 5 }).map((_, i) => {
          const ratingValue = product.averageRating ?? product.rating ?? 0;
          return (
            <Star
              key={i}
              size={12}
              className={cn(
                i < Math.floor(ratingValue)
                  ? 'fill-yellow-400 text-yellow-400'
                  : 'fill-gray-200 text-gray-200'
              )}
            />
          );
        }),
      [product.averageRating, product.rating]
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
          <Link to={`/product/${product.code}`} className="flex-shrink-0">
            <div className="relative w-40 h-40 rounded-lg overflow-hidden bg-secondary">
              <CustomImage
                src={product.image}
                alt={product.name}
                className="w-full h-full object-cover"
                loading="lazy"
              />
              {isNew && <Badge color="error" className="absolute top-2 left-2">{t('product.new')}</Badge>}
              {hasDiscount && <Badge color="error" className="absolute top-2 left-2">{`-${product.discount}%`}</Badge>}
            </div>
          </Link>

          {/* Content */}
          <div className="flex-1 flex flex-col justify-between">
            <div>
              <Link to={`/products?category=${product.categoryCode}`} className="text-xs text-primary font-medium uppercase tracking-wide">
                {product.category}
              </Link>
              <Link to={`/product/${product.code}`}>
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
          'group relative bg-white rounded-xl overflow-hidden border border-slate-100/80 transition-all duration-500',
          hoverable && 'hover:shadow-[0_8px_30px_rgb(0,0,0,0.06)] hover:border-slate-200',
          className
        )}
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.3 }}
      >
        <Link to={`/product/${product.code}`} className="block relative">
          {/* Image Container */}
          <div className="relative aspect-[4/5] overflow-hidden bg-slate-50">
            <CustomImage
              src={product.image}
              alt={product.name}
              className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110"
              loading="lazy"
            />

            {/* Overlay Gradient on Hover */}
            <div className="absolute inset-0 bg-black/5 opacity-0 group-hover:opacity-100 transition-opacity duration-300" />

            {/* Badges */}
            <div className="absolute top-3 left-3 flex flex-col gap-2 z-10">
              {isNew && (
                <Badge className="bg-red-500 text-white border-0 shadow-sm px-2.5 py-0.5 text-xs font-bold uppercase tracking-wider">
                  {t('product.new')}
                </Badge>
              )}
              {hasDiscount && (
                <Badge className="bg-orange-500 text-white border-0 shadow-sm px-2.5 py-0.5 text-xs font-bold">
                  {`-${product.discount}%`}
                </Badge>
              )}
            </div>

            {/* Right Side Actions (Wishlist, Compare, QuickView) */}
            <div className="absolute top-3 right-3 flex flex-col gap-2 translate-x-10 opacity-0 group-hover:translate-x-0 group-hover:opacity-100 transition-all duration-300 z-20">
              <button
                onClick={handleWishlistToggle}
                className={cn(
                  'w-9 h-9 flex items-center justify-center bg-white rounded-full shadow-md transition-transform hover:scale-110 text-slate-600 hover:text-rose-500',
                  isWishlisted && 'text-rose-500'
                )}
                title={t('product.addToWishlist')}
              >
                <Heart size={18} fill={isWishlisted ? 'currentColor' : 'none'} className={isWishlisted ? "animate-pulse-once" : ""} />
              </button>
              <button
                onClick={handleCompareToggle}
                className={cn(
                  'w-9 h-9 flex items-center justify-center bg-white rounded-full shadow-md transition-transform hover:scale-110 text-slate-600 hover:text-indigo-600 delay-75',
                  isCompared && 'bg-indigo-600 text-white hover:text-white'
                )}
                title={t('product.compare')}
              >
                <Scale size={18} />
              </button>
              {showQuickView && (
                <button
                  onClick={handleQuickView}
                  className="w-9 h-9 flex items-center justify-center bg-white rounded-full shadow-md transition-transform hover:scale-110 text-slate-600 hover:text-indigo-600 delay-100"
                  title="Quick View"
                >
                  <Eye size={18} />
                </button>
              )}
            </div>

            {/* Bottom Action Area (Add to Cart) */}
            <div className="absolute inset-x-4 bottom-4 translate-y-full opacity-0 group-hover:translate-y-0 group-hover:opacity-100 transition-all duration-300 z-20">
               {hasFixedPrice ? (
                <Button
                  className="w-full bg-slate-900/90 hover:bg-black text-white shadow-lg backdrop-blur-sm border-0"
                  onClick={handleAddToCart}
                  disabled={isOutOfStock}
                >
                  <ShoppingCart size={18} className="mr-2" />
                  {isOutOfStock ? t('product.outOfStock') : t('product.addToCart')}
                </Button>
              ) : (
                <Button
                  className="w-full bg-white/90 hover:bg-white text-primary shadow-lg backdrop-blur-sm border-0"
                  onClick={(e) => {
                    e.preventDefault();
                    window.location.href = `/quote-request/${product.code}`;
                  }}
                >
                  <Phone size={18} className="mr-2" />
                  {t('product.requestQuote')}
                </Button>
              )}
            </div>
          </div>

          {/* Content */}
          <div className="p-4 pt-3.5">
            {/* Category */}
            <div className="text-[11px] font-bold text-indigo-500 uppercase tracking-wider mb-1 line-clamp-1">
              {product.category}
            </div>
            
            {/* Title */}
            <h3 className="text-[15px] font-medium text-slate-800 leading-snug line-clamp-2 min-h-[2.5em] group-hover:text-indigo-600 transition-colors mb-1.5">
              {product.name}
            </h3>

            {/* Rating */}
            <div className="flex items-center gap-1 mb-2">
              <div className="flex text-yellow-400 gap-0.5">
                {stars}
              </div>
              <span className="text-xs text-slate-400 font-medium ml-1">({product.reviewCount})</span>
            </div>

            {/* Price & Stock */}
            <div className="flex items-end justify-between mt-auto">
              <div className="flex flex-col">
                {hasFixedPrice ? (
                  <div className="flex items-baseline gap-2">
                    <span className="text-lg font-bold text-slate-900">{formatCurrency(product.price)}</span>
                    {product.originalPrice && product.originalPrice > product.price && (
                      <span className="text-sm text-slate-400 line-through decoration-1">{formatCurrency(product.originalPrice)}</span>
                    )}
                  </div>
                ) : (
                  <span className="text-base font-semibold text-indigo-600">{t('product.contactForPrice')}</span>
                )}
              </div>
            </div>
          </div>
        </Link>
      </motion.div>
    );
  }
);

ProductCard.displayName = 'ProductCard';

export default ProductCard;
