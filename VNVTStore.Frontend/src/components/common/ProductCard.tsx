import { memo, useCallback, useMemo, useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import {
  Heart, ShoppingCart, Star, Eye, Scale,
  Phone, Share2, Clock, Check, FastForward, Bell, X,
} from 'lucide-react';
import { cn } from '@/utils/cn';
import { Button, Badge } from '@/components/ui';
import {
  useCartStore, useWishlistStore, useCompareStore,
  usePriceAlertStore, useToast, useUIStore,
} from '@/store';
import { formatCurrency } from '@/utils/format';
import type { Product } from '@/types';
import CustomImage from '@/components/common/Image';
import { SizeGuideDrawer } from '@/components/common/SizeGuideDrawer';

// ============ Constants ============
const LOW_STOCK_THRESHOLD = 10;
const DELIVERY_DAYS = 3;
const NEW_PRODUCT_DAYS = 7;

function getEstimatedDelivery(): string {
  const d = new Date();
  d.setDate(d.getDate() + DELIVERY_DAYS);
  return d.toLocaleDateString('vi-VN', { weekday: 'short', day: 'numeric', month: 'numeric' });
}

/** Countdown hook — shared between cards via prop, not re-created per-card */
export function useCountdown(endDate?: string | Date | null) {
  const [remaining, setRemaining] = useState('');
  const [isActive, setIsActive] = useState(false);

  useEffect(() => {
    if (!endDate) return;
    const end = new Date(endDate).getTime();
    if (isNaN(end)) return;
    const tick = () => {
      const diff = end - Date.now();
      if (diff <= 0) { setIsActive(false); setRemaining(''); return; }
      setIsActive(true);
      const h = Math.floor(diff / 3600000);
      const m = Math.floor((diff % 3600000) / 60000);
      const s = Math.floor((diff % 60000) / 1000);
      setRemaining(
        `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
      );
    };
    tick();
    const id = setInterval(tick, 1000);
    return () => clearInterval(id);
  }, [endDate]);

  return { remaining, isActive };
}

// ============ Props ============
export interface ProductCardProps {
  product: Product;
  showQuickView?: boolean;
  variant?: 'grid' | 'list' | 'compact';
  hoverable?: boolean;
  className?: string;
  onQuickView?: (product: Product) => void;
  selectable?: boolean;
  selected?: boolean;
  onSelectToggle?: (code: string) => void;
}

// ============ Hover Action Button ============
interface ActionBtnProps {
  onClick: (e: React.MouseEvent) => void;
  title: string;
  active?: boolean;
  activeClassName?: string;
  children: React.ReactNode;
}

const ActionBtn = ({ onClick, title, active, activeClassName, children }: ActionBtnProps) => (
  <button
    onClick={onClick}
    title={title}
    className={cn(
      'w-9 h-9 flex items-center justify-center bg-white rounded-full shadow-md',
      'transition-transform hover:scale-110 text-slate-600',
      active && activeClassName,
    )}
  >
    {children}
  </button>
);

// ============ Compact Variant ============
const CompactCard = ({ product, className }: Pick<ProductCardProps, 'product' | 'className'>) => (
  <Link
    to={`/product/${product.code}`}
    className={cn('flex items-center gap-3 p-2 rounded-lg border border-border bg-bg-primary hover:bg-bg-tertiary transition-colors', className)}
  >
    <CustomImage src={product.image} alt={product.name} className="w-12 h-12 rounded object-cover flex-shrink-0" />
    <div className="min-w-0">
      <p className="text-sm font-medium text-text-primary truncate">{product.name}</p>
      <p className="text-sm font-bold text-accent">{formatCurrency(product.price)}</p>
    </div>
  </Link>
);

// ============ List Variant ============
const ListCard = memo(({ product, className, hoverable }: Pick<ProductCardProps, 'product' | 'className' | 'hoverable'>) => {
  const { t } = useTranslation();
  const addToCart = useCartStore((s) => s.addItem);
  const { addItem: addToWishlist, removeItem: removeFromWishlist, isInWishlist } = useWishlistStore();
  const toast = useToast();
  const isWishlisted = isInWishlist(product.code);
  const stockQty = product.stockQuantity ?? product.stock ?? 0;
  const isOutOfStock = stockQty === 0;
  const hasDiscount = Boolean(product.discount && product.discount > 0);

  const handleWishlist = useCallback((e: React.MouseEvent) => {
    e.preventDefault(); e.stopPropagation();
    isWishlisted ? removeFromWishlist(product.code) : addToWishlist(product);
  }, [isWishlisted, product, addToWishlist, removeFromWishlist]);

  const handleAddToCart = useCallback(async (e: React.MouseEvent) => {
    e.preventDefault(); e.stopPropagation();
    if (product.price > 0 && !isOutOfStock) {
      await addToCart(product);
      toast.success(`${product.name} ${t('cart.addedToCart', 'đã được thêm vào giỏ hàng')}`);
    }
  }, [product, isOutOfStock, addToCart, toast, t]);

  return (
    <div className={cn(
      'flex gap-4 p-4 rounded-2xl border border-border bg-bg-primary transition-all duration-300',
      hoverable && 'hover:shadow-lg',
      className,
    )}>
      <Link to={`/product/${product.code}`} className="flex-shrink-0">
        <div className="relative w-36 h-36 rounded-lg overflow-hidden bg-bg-secondary">
          <CustomImage src={product.image} alt={product.name} className="w-full h-full object-cover" loading="lazy" />
          {hasDiscount && <Badge color="error" className="absolute top-2 left-2">-{product.discount}%</Badge>}
        </div>
      </Link>
      <div className="flex-1 flex flex-col justify-between min-w-0">
        <div>
          <Link to={`/products?category=${product.categoryCode}`} className="text-xs font-bold uppercase tracking-wide text-accent">
            {product.category}
          </Link>
          <Link to={`/product/${product.code}`}>
            <h3 className="font-semibold mt-1 text-text-primary hover:text-accent transition-colors line-clamp-2">
              {product.name}
            </h3>
          </Link>
          {product.description && (
            <p className="text-sm mt-1 line-clamp-2 text-text-secondary">{product.description}</p>
          )}
        </div>
        <div className="flex items-center justify-between mt-3">
          <div>
            {product.price > 0 ? (
              <div className="flex items-baseline gap-2">
                <span className="text-lg font-bold text-error">{formatCurrency(product.price)}</span>
                {product.originalPrice && (
                  <span className="text-sm text-text-tertiary line-through">{formatCurrency(product.originalPrice)}</span>
                )}
              </div>
            ) : (
              <span className="text-base font-semibold text-accent">{t('product.contactForPrice')}</span>
            )}
          </div>
          <div className="flex gap-2">
            <Button variant="ghost" size="sm" onClick={handleWishlist}>
              <Heart size={18} fill={isWishlisted ? 'currentColor' : 'none'} className={isWishlisted ? 'text-rose-500' : ''} />
            </Button>
            {product.price > 0 ? (
              <Button size="sm" onClick={handleAddToCart} disabled={isOutOfStock}>
                <ShoppingCart size={16} className="mr-1.5" />
                {isOutOfStock ? t('product.outOfStock') : t('product.addToCart')}
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
});

// ============ Grid Card (default) ============
const GridCard = memo(({
  product, hoverable, className, showQuickView, onQuickView, selectable, selected, onSelectToggle,
}: Omit<ProductCardProps, 'variant'>) => {
  const { t } = useTranslation();
  const addToCart = useCartStore((s) => s.addItem);
  const { addItem: addToWishlist, removeItem: removeFromWishlist, isInWishlist } = useWishlistStore();
  const { addItem: addToCompare, removeItem: removeFromCompare, isInCompare } = useCompareStore();
  const { toggleAlert, isWatched } = usePriceAlertStore();
  const toast = useToast();
  const { setQuickViewProduct } = useUIStore();

  const stockQty = product.stockQuantity ?? product.stock ?? 0;
  const isOutOfStock = stockQty === 0;
  const isLowStock = stockQty > 0 && stockQty <= LOW_STOCK_THRESHOLD;
  const hasDiscount = Boolean(product.discount && product.discount > 0);
  const hasFixedPrice = product.price > 0;
  const isWishlisted = isInWishlist(product.code);
  const isCompared = isInCompare(product.code);
  const isWatchingPrice = isWatched(product.code);

  const [showSizeGuide, setShowSizeGuide] = useState(false);
  const [showCopied, setShowCopied] = useState(false);

  const isNew = useMemo(() => {
    if (product.isNew) return true;
    if (product.createdAt) {
      const cutoff = new Date();
      cutoff.setDate(cutoff.getDate() - NEW_PRODUCT_DAYS);
      return new Date(product.createdAt) >= cutoff;
    }
    return false;
  }, [product.isNew, product.createdAt]);

  // Flash sale countdown
  const { remaining: countdown, isActive: hasCountdown } = useCountdown(product.promotionEndDate);

  // Multi-image hover cycling
  const allImages = useMemo(() => (product.images && product.images.length > 1 ? product.images : []), [product.images]);
  const [hoverImgIdx, setHoverImgIdx] = useState(0);
  const hoverInterval = useRef<ReturnType<typeof setInterval> | null>(null);

  const startImageCycle = useCallback(() => {
    if (allImages.length <= 1) return;
    hoverInterval.current = setInterval(() => setHoverImgIdx((p) => (p + 1) % allImages.length), 1200);
  }, [allImages.length]);

  const stopImageCycle = useCallback(() => {
    if (hoverInterval.current) { clearInterval(hoverInterval.current); hoverInterval.current = null; }
    setHoverImgIdx(0);
  }, []);

  const deliveryDate = useMemo(() => getEstimatedDelivery(), []);

  // Variant color dots
  const variantColors = useMemo(() => {
    if (!product.variants?.length) return [];
    try {
      return product.variants
        .map((v) => {
          const attrs = typeof v.attributes === 'string' ? JSON.parse(v.attributes) : v.attributes;
          return attrs?.color || attrs?.Color || null;
        })
        .filter(Boolean)
        .slice(0, 5) as string[];
    } catch { return []; }
  }, [product.variants]);

  // Stars
  const stars = useMemo(() => {
    const rating = product.averageRating ?? product.rating ?? 0;
    return Array.from({ length: 5 }, (_, i) => (
      <Star key={i} size={12} className={cn(i < Math.floor(rating) ? 'fill-yellow-400 text-yellow-400' : 'fill-gray-200 text-gray-200')} />
    ));
  }, [product.averageRating, product.rating]);

  // Handlers
  const handleWishlist = useCallback((e: React.MouseEvent) => {
    e.preventDefault(); e.stopPropagation();
    isWishlisted ? removeFromWishlist(product.code) : addToWishlist(product);
  }, [isWishlisted, product, addToWishlist, removeFromWishlist]);

  const handleCompare = useCallback((e: React.MouseEvent) => {
    e.preventDefault(); e.stopPropagation();
    isCompared ? removeFromCompare(product.code) : addToCompare(product);
  }, [isCompared, product, addToCompare, removeFromCompare]);

  const handleAddToCart = useCallback(async (e: React.MouseEvent) => {
    e.preventDefault(); e.stopPropagation();
    if (!hasFixedPrice || isOutOfStock) return;
    try {
      await addToCart(product);
      toast.success(`${product.name} ${t('cart.addedToCart', 'đã được thêm vào giỏ hàng')}`);
    } catch {
      toast.error(t('product.addToCartError', 'Có lỗi xảy ra'));
    }
  }, [hasFixedPrice, isOutOfStock, product, addToCart, toast, t]);

  const handleBuyNow = useCallback((e: React.MouseEvent) => {
    e.preventDefault(); e.stopPropagation();
    addToCart(product);
    window.location.href = '/checkout';
  }, [product, addToCart]);

  const handleQuickView = useCallback((e: React.MouseEvent) => {
    e.preventDefault(); e.stopPropagation();
    onQuickView ? onQuickView(product) : setQuickViewProduct(product);
  }, [product, onQuickView, setQuickViewProduct]);

  const handleShare = useCallback((e: React.MouseEvent) => {
    e.preventDefault(); e.stopPropagation();
    navigator.clipboard.writeText(`${window.location.origin}/product/${product.code}`).then(() => {
      setShowCopied(true);
      setTimeout(() => setShowCopied(false), 2000);
    });
  }, [product.code]);

  const handlePriceAlert = useCallback((e: React.MouseEvent) => {
    e.preventDefault(); e.stopPropagation();
    toggleAlert(product.code);
    toast.success(isWatchingPrice ? t('product.unwatchSuccess') : t('product.watchSuccess'));
  }, [product.code, toggleAlert, isWatchingPrice, toast, t]);

  const handleSelectToggle = useCallback((e: React.MouseEvent) => {
    e.preventDefault(); e.stopPropagation();
    onSelectToggle?.(product.code);
  }, [product.code, onSelectToggle]);

  return (
    <motion.div
      className={cn(
        'group relative rounded-xl overflow-hidden transition-all duration-300 border',
        'bg-bg-primary border-border hover:shadow-lg hover:border-border',
        hoverable && 'hover:-translate-y-0.5',
        className,
      )}
      initial={{ opacity: 0, scale: 0.97 }}
      whileInView={{ opacity: 1, scale: 1 }}
      viewport={{ once: true }}
      transition={{ duration: 0.3 }}
    >
      <Link to={`/product/${product.code}`} className="block relative">
        {/* Image */}
        <div
          className="relative aspect-[4/5] overflow-hidden bg-bg-secondary"
          onMouseEnter={startImageCycle}
          onMouseLeave={stopImageCycle}
        >
          {/* Video on hover */}
          {product.videoURL && (
            <video src={product.videoURL} autoPlay muted loop playsInline
              className="absolute inset-0 w-full h-full object-cover z-10 opacity-0 group-hover:opacity-100 transition-opacity duration-300 pointer-events-none"
            />
          )}

          <CustomImage
            src={allImages.length > 1 ? allImages[hoverImgIdx] : product.image}
            alt={product.name}
            className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-105"
            loading="lazy"
          />

          {/* Image dots */}
          {allImages.length > 1 && (
            <div className="absolute bottom-12 left-1/2 -translate-x-1/2 flex gap-1 z-10 opacity-0 group-hover:opacity-100 transition-opacity">
              {allImages.map((_img, idx) => (
                <span key={idx} className={cn('w-1.5 h-1.5 rounded-full transition-all', idx === hoverImgIdx ? 'bg-white scale-125' : 'bg-white/50')} />
              ))}
            </div>
          )}

          {/* Dim overlay */}
          <div className="absolute inset-0 bg-black/5 opacity-0 group-hover:opacity-100 transition-opacity duration-300" />

          {/* Bulk select */}
          {selectable && (
            <button onClick={handleSelectToggle}
              className={cn('absolute top-3 left-3 w-6 h-6 rounded border-2 flex items-center justify-center z-30 transition-all',
                selected ? 'bg-accent border-accent text-white' : 'bg-white/80 border-slate-300 hover:border-accent')}
            >
              {selected && <Check size={14} />}
            </button>
          )}

          {/* Badges */}
          <div className={cn('absolute top-3 flex flex-col gap-1.5 z-10', selectable ? 'left-12' : 'left-3')}>
            {isNew && <Badge color="error">NEW</Badge>}
            {product.isFeatured && <Badge color="error">HOT</Badge>}
            {hasDiscount && <Badge color="warning">-{product.discount}%</Badge>}
            {isLowStock && (
              <Badge color="warning" className="animate-pulse">
                🔥 {t('product.justLeft', 'Còn {{count}}!', { count: stockQty })}
              </Badge>
            )}
          </div>

          {/* Flash sale timer */}
          {hasCountdown && (
            <div className="absolute top-3 right-3 z-10 flex items-center gap-1 bg-red-600 text-white px-2 py-1 rounded-lg text-xs font-mono font-bold shadow">
              <Clock size={11} />
              {countdown}
            </div>
          )}

          {/* Hover action buttons */}
          <div className={cn('absolute right-3 flex flex-col gap-2 translate-x-10 opacity-0 group-hover:translate-x-0 group-hover:opacity-100 transition-all duration-300 z-20', hasCountdown ? 'top-14' : 'top-3')}>
            <ActionBtn onClick={handleWishlist} title={t('product.addToWishlist')} active={isWishlisted} activeClassName="text-rose-500">
              <Heart size={17} fill={isWishlisted ? 'currentColor' : 'none'} />
            </ActionBtn>
            <ActionBtn onClick={handleCompare} title={t('product.compare')} active={isCompared} activeClassName="bg-accent text-white hover:text-white">
              <Scale size={17} />
            </ActionBtn>
            {showQuickView && (
              <ActionBtn onClick={handleQuickView} title={t('common.view')}>
                <Eye size={17} />
              </ActionBtn>
            )}
            <ActionBtn onClick={handlePriceAlert} title={isWatchingPrice ? t('product.unwatch') : t('product.watch')} active={isWatchingPrice} activeClassName="text-amber-500">
              <Bell size={17} fill={isWatchingPrice ? 'currentColor' : 'none'} />
            </ActionBtn>
            <ActionBtn onClick={handleShare} title={t('product.share')} className="relative">
              {showCopied ? <Check size={17} className="text-success" /> : <Share2 size={17} />}
              <AnimatePresence>
                {showCopied && (
                  <motion.span initial={{ opacity: 0, x: -8 }} animate={{ opacity: 1, x: -44 }} exit={{ opacity: 0 }}
                    className="absolute right-full mr-1 bg-slate-900 text-white text-[10px] px-2 py-0.5 rounded whitespace-nowrap">
                    {t('product.copied', 'Đã copy!')}
                  </motion.span>
                )}
              </AnimatePresence>
            </ActionBtn>
          </div>

          {/* Add to cart / Buy now overlay */}
          <div className="absolute inset-x-4 bottom-4 translate-y-full opacity-0 group-hover:translate-y-0 group-hover:opacity-100 transition-all duration-300 z-20">
            {hasFixedPrice ? (
              <div className="flex gap-2">
                <Button className="flex-1 bg-slate-900/90 hover:bg-black text-white backdrop-blur-sm border-0 h-10 px-2 text-xs"
                  onClick={handleAddToCart} disabled={isOutOfStock}>
                  <ShoppingCart size={15} className="mr-1.5" />
                  {isOutOfStock ? t('product.outOfStock') : t('product.addToCart', 'Thêm giỏ')}
                </Button>
                <Button className="flex-1 bg-orange-500/90 hover:bg-orange-600 text-white backdrop-blur-sm border-0 h-10 px-2 text-xs"
                  onClick={handleBuyNow} disabled={isOutOfStock}>
                  <FastForward size={15} className="mr-1.5" />
                  {t('common.buyNow', 'Mua ngay')}
                </Button>
              </div>
            ) : (
              <Button className="w-full bg-white/90 hover:bg-white text-text-primary border-0 backdrop-blur-sm"
                onClick={(e) => { e.preventDefault(); window.location.href = `/quote-request/${product.code}`; }}>
                <Phone size={16} className="mr-2" />
                {t('product.requestQuote', 'Báo giá')}
              </Button>
            )}
          </div>
        </div>

        {/* Card Body */}
        <div className="p-4 pt-3">
          {/* Category */}
          <div className="text-[11px] font-bold uppercase tracking-wider text-accent mb-1">
            {product.category}
          </div>

          {/* Title */}
          <h3 className="text-[14px] font-medium leading-snug line-clamp-2 min-h-[2.4em] text-text-primary group-hover:text-accent transition-colors mb-2">
            {product.name}
          </h3>

          {/* Rating */}
          <div className="flex items-center gap-1 mb-2">
            <div className="flex gap-0.5">{stars}</div>
            <span className="text-xs text-text-tertiary ml-1">({product.reviewCount})</span>
          </div>

          {/* Variant color dots */}
          {variantColors.length > 0 && (
            <div className="flex items-center gap-1.5 mb-2">
              {variantColors.map((color, i) => (
                <span key={i} className="w-4 h-4 rounded-full border border-border shadow-inner cursor-pointer hover:scale-125 transition-transform"
                  style={{ backgroundColor: color }} title={color} />
              ))}
              {product.variants && product.variants.length > 5 && (
                <span className="text-[10px] text-text-tertiary">+{product.variants.length - 5}</span>
              )}
            </div>
          )}

          {/* Flash sale progress bar */}
          {hasCountdown && product.promotionOriginalQuantity && (
            <div className="mb-3">
              <div className="flex justify-between text-[10px] mb-1">
                <span className="font-bold text-orange-600">🔥 Đang bán chạy</span>
                <span className="text-text-tertiary">Đã bán {product.soldCount ?? 0}</span>
              </div>
              <div className="h-1.5 w-full rounded-full overflow-hidden bg-bg-tertiary">
                <motion.div
                  initial={{ width: 0 }}
                  animate={{ width: `${Math.min(100, ((product.soldCount ?? 0) / product.promotionOriginalQuantity) * 100)}%` }}
                  className="h-full rounded-full bg-gradient-to-r from-orange-500 to-red-600"
                />
              </div>
            </div>
          )}

          {/* Price */}
          <div className="mt-auto">
            {hasFixedPrice ? (
              <>
                <div className="flex items-baseline gap-2">
                  <span className="text-[17px] font-bold text-slate-900 dark:text-white">
                    {formatCurrency(product.price)}
                  </span>
                  {product.originalPrice && product.originalPrice > product.price && (
                    <span className="text-sm text-text-tertiary line-through">
                      {formatCurrency(product.originalPrice)}
                    </span>
                  )}
                </div>
                {/* Wholesale callout */}
                {product.wholesalePrice && (
                  <p className="text-[10px] text-success font-medium mt-0.5">
                    Mua sỉ từ {formatCurrency(product.wholesalePrice)}
                  </p>
                )}
              </>
            ) : (
              <span className="text-base font-semibold text-accent">{t('product.contactForPrice')}</span>
            )}
          </div>

          {/* Estimated delivery */}
          {hasFixedPrice && !isOutOfStock && (
            <p className="mt-2 text-[10px] text-text-tertiary">
              🚚 Giao dự kiến: <span className="font-medium text-text-secondary">{deliveryDate}</span>
            </p>
          )}

          {/* Size guide */}
          <button
            onClick={(e) => { e.preventDefault(); e.stopPropagation(); setShowSizeGuide(true); }}
            className="mt-2 text-[10px] text-accent hover:underline"
          >
            {t('product.sizeGuide', 'Hướng dẫn chọn size')}
          </button>
        </div>
      </Link>

      <SizeGuideDrawer
        category={showSizeGuide ? (product.category ?? 'general') : undefined}
        onClose={() => setShowSizeGuide(false)}
      />
    </motion.div>
  );
});

// ============ Main ProductCard Export ============
export const ProductCard = memo(({ variant = 'grid', ...props }: ProductCardProps) => {
  if (variant === 'compact') return <CompactCard product={props.product} className={props.className} />;
  if (variant === 'list')    return <ListCard hoverable={props.hoverable} product={props.product} className={props.className} />;
  return <GridCard {...props} />;
});

ProductCard.displayName = 'ProductCard';
export default ProductCard;
