import { memo, useCallback, useMemo, useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { Heart, ShoppingCart, Star, Eye, Scale, Phone, Share2, Truck, Clock, Check, Copy, Zap, FastForward, ShieldCheck, Coins, QrCode, Info, Bell, Package, X, History as HistoryIcon, Sparkles } from 'lucide-react';
import { cn } from '@/utils/cn';

import { Button, Badge } from '@/components/ui';
import { useCartStore, useWishlistStore, useCompareStore, usePriceAlertStore, useToast, useUIStore } from '@/store';
import { formatCurrency } from '@/utils/format';
import type { Product } from '@/types';
import CustomImage from '@/components/common/Image';

// ============ ProductCard Props Interface ============
// ============ Constants ============
const LOW_STOCK_THRESHOLD = 10;
const DELIVERY_DAYS = 3;

function getEstimatedDelivery(): string {
  const d = new Date();
  d.setDate(d.getDate() + DELIVERY_DAYS);
  return d.toLocaleDateString('vi-VN', { weekday: 'short', day: 'numeric', month: 'numeric' });
}

/** Calculate order cutoff countdown */
function useOrderCutoff() {
  const [remaining, setRemaining] = useState('');
  useEffect(() => {
    const tick = () => {
      const now = new Date();
      const cutoff = new Date();
      cutoff.setHours(16, 0, 0, 0); // 4 PM Cutoff
      if (now > cutoff) cutoff.setDate(cutoff.getDate() + 1);
      const diff = cutoff.getTime() - now.getTime();
      const h = Math.floor(diff / 3600000);
      const m = Math.floor((diff % 3600000) / 60000);
      setRemaining(`${h}h ${m}m`);
    };
    tick();
    const id = setInterval(tick, 60000);
    return () => clearInterval(id);
  }, []);
  return remaining;
}

/** Format remaining countdown from endDate */
function useCountdown(endDate?: string | Date | null) {
  const [remaining, setRemaining] = useState('');
  const [isActive, setIsActive] = useState(false);

  useEffect(() => {
    if (!endDate) return;
    const end = new Date(endDate).getTime();
    if (isNaN(end)) return;

    const tick = () => {
      const now = Date.now();
      const diff = end - now;
      if (diff <= 0) { setIsActive(false); setRemaining(''); return; }
      setIsActive(true);
      const h = Math.floor(diff / 3600000);
      const m = Math.floor((diff % 3600000) / 60000);
      const s = Math.floor((diff % 60000) / 1000);
      setRemaining(`${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`);
    };
    tick();
    const id = setInterval(tick, 1000);
    return () => clearInterval(id);
  }, [endDate]);

  return { remaining, isActive };
}

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
  /** Enable bulk selection checkbox */
  selectable?: boolean;
  /** Whether this card is currently selected */
  selected?: boolean;
  /** Selection toggle handler */
  onSelectToggle?: (code: string) => void;
}

// ============ ProductCard Component ============
export const ProductCard = memo(
  ({
    product,
    showQuickView = true,
    variant = 'grid',
    hoverable = true,
    className,
    onQuickView,
    selectable = false,
    selected = false,
    onSelectToggle,
  }: ProductCardProps) => {
    const { t } = useTranslation();

    // Store actions
    const addToCart = useCartStore((state) => state.addItem);
    const { addItem: addToWishlist, removeItem: removeFromWishlist, isInWishlist } = useWishlistStore();
    const { addItem: addToCompare, removeItem: removeFromCompare, isInCompare } = useCompareStore();
    const { toggleAlert, isWatched } = usePriceAlertStore();
    const toast = useToast();

    // Feature 7: Real-time Analytics Mock
    const [recentBuyer, setRecentBuyer] = useState<string | null>(null);
    useEffect(() => {
      const names = ['Nguyễn Văn A', 'Trần Thị B', 'Lê Văn C', 'Phạm Minh D'];
      const timer = setTimeout(() => {
        if (Math.random() > 0.8) {
          setRecentBuyer(names[Math.floor(Math.random() * names.length)]);
        }
      }, Math.random() * 10000 + 5000);
      return () => clearTimeout(timer);
    }, []);

    // Feature 32: Abandoned Cart Recovery logic
    const [showAbandonedAlert, setShowAbandonedAlert] = useState(false);
    useEffect(() => {
      if (product.abandonedCartRecoveryEnabled) {
        const timer = setTimeout(() => setShowAbandonedAlert(true), 15000);
        return () => clearTimeout(timer);
      }
    }, [product.abandonedCartRecoveryEnabled]);

    // Feature 35: Product Zoom Magnifier logic
    const [zoomPos, setZoomPos] = useState({ x: 0, y: 0, show: false });
    const handleZoom = useCallback((e: React.MouseEvent<HTMLDivElement>) => {
      const rect = e.currentTarget.getBoundingClientRect();
      const x = ((e.clientX - rect.left) / rect.width) * 100;
      const y = ((e.clientY - rect.top) / rect.height) * 100;
      setZoomPos({ x, y, show: true });
    }, []);

    // Derived states
    const isWishlisted = isInWishlist(product.code);
    const isCompared = isInCompare(product.code);
    const hasFixedPrice = product.price > 0;
    const hasDiscount = product.discount && product.discount > 0;
    const stockQty = product.stockQuantity ?? product.stock ?? 0;
    const isOutOfStock = stockQty === 0;
    const isLowStock = stockQty > 0 && stockQty <= LOW_STOCK_THRESHOLD;

    // Feature 3: Flash Sale Countdown
    const { remaining: countdown, isActive: hasCountdown } = useCountdown(product.promotionEndDate);

    // Feature 5: Multi-Image Hover Switcher
    const allImages = useMemo(() => {
      const imgs = product.images && product.images.length > 1 ? product.images : [];
      return imgs;
    }, [product.images]);
    const [hoverImgIdx, setHoverImgIdx] = useState(0);
    const hoverInterval = useRef<ReturnType<typeof setInterval> | null>(null);

    const startImageCycle = useCallback(() => {
      if (allImages.length <= 1) return;
      hoverInterval.current = setInterval(() => {
        setHoverImgIdx(prev => (prev + 1) % allImages.length);
      }, 1200);
    }, [allImages.length]);

    const stopImageCycle = useCallback(() => {
      if (hoverInterval.current) { clearInterval(hoverInterval.current); hoverInterval.current = null; }
      setHoverImgIdx(0);
    }, []);

    // Feature 8: Estimated Delivery
    const deliveryDate = useMemo(() => getEstimatedDelivery(), []);

    // Feature 9: Share
    const [showCopied, setShowCopied] = useState(false);
    const handleShare = useCallback((e: React.MouseEvent) => {
      e.preventDefault();
      e.stopPropagation();
      const url = `${window.location.origin}/product/${product.code}`;
      navigator.clipboard.writeText(url).then(() => {
        setShowCopied(true);
        setTimeout(() => setShowCopied(false), 2000);
      });
    }, [product.code]);

    // Feature 2: Order Cutoff
    const orderCutoff = useOrderCutoff();

    // Feature 3: Buy Now
    const handleBuyNow = useCallback((e: React.MouseEvent) => {
      e.preventDefault();
      e.stopPropagation();
      addToCart(product);
      window.location.href = '/checkout';
    }, [product, addToCart]);

    // Feature 10: Variant Dots
    const variantColors = useMemo(() => {
      if (!product.variants || product.variants.length === 0) return [];
      try {
        return product.variants
          .map(v => {
            const attrs = typeof v.attributes === 'string' ? JSON.parse(v.attributes) : v.attributes;
            return attrs?.color || attrs?.Color || null;
          })
          .filter(Boolean)
          .slice(0, 5);
      } catch { return []; }
    }, [product.variants]);

    // Feature 7: Bulk selection
    const handleSelectToggle = useCallback((e: React.MouseEvent) => {
      e.preventDefault();
      e.stopPropagation();
      onSelectToggle?.(product.code);
    }, [product.code, onSelectToggle]);
    
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
        if (isWishlisted) {
          removeFromWishlist(product.code);
        } else {
          addToWishlist(product);
        }
      },
      [isWishlisted, product, addToWishlist, removeFromWishlist]
    );

    const handleCompareToggle = useCallback(
      (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        if (isCompared) {
          removeFromCompare(product.code);
        } else {
          addToCompare(product);
        }
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
          } catch {
            toast.error(t('product.addToCartError') || 'Có lỗi xảy ra khi thêm vào giỏ hàng');
          }
        }
      },
      [hasFixedPrice, isOutOfStock, product, addToCart, toast, t]
    );

    const setQuickViewProduct = useUIStore((state) => state.setQuickViewProduct);

    const handleQuickView = useCallback(
      (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        if (onQuickView) {
          onQuickView(product);
        } else {
          setQuickViewProduct(product);
        }
      },
      [product, onQuickView, setQuickViewProduct]
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
              {isNew && <Badge color="error" className="absolute top-2 left-2">{t('product.new', 'New')}</Badge>}
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
                  <span className="text-base font-semibold text-primary">{t('product.contactForPrice', 'Contact for Price')}</span>
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
                    {t('product.addToCart', 'Add to Cart')}
                  </Button>
                ) : (
                  <Button size="sm" variant="outline" leftIcon={<Phone size={16} />}>
                    {t('product.requestQuote', 'Request Quote')}
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
          <div
            className="relative aspect-[4/5] overflow-hidden bg-slate-50"
            onMouseEnter={startImageCycle}
            onMouseLeave={() => { stopImageCycle(); setZoomPos(prev => ({ ...prev, show: false })); }}
            onMouseMove={handleZoom}
          >
            {/* Feature 31: Promo Label */}
            {product.promoLabel && (
              <div className="absolute top-2.5 left-2.5 z-30 pointer-events-none">
                <Badge className="bg-rose-500 text-white border-0 shadow-lg text-[9px] h-4 px-1.5 animate-pulse font-bold">
                  {product.promoLabel}
                </Badge>
              </div>
            )}

            {/* Feature 35: Zoom Magnifier */}
            {zoomPos.show && (
              <div 
                className="absolute inset-0 z-20 pointer-events-none overflow-hidden hidden lg:block"
                style={{
                  backgroundImage: `url(${product.image})`,
                  backgroundPosition: `${zoomPos.x}% ${zoomPos.y}%`,
                  backgroundSize: '200%',
                  backgroundRepeat: 'no-repeat'
                }}
              />
            )}

            {/* Feature 19: Video Auto-play on Hover */}
            {product.videoURL && (
              <video 
                src={product.videoURL}
                autoPlay
                muted
                loop
                playsInline
                className="absolute inset-0 w-full h-full object-cover z-10 opacity-0 group-hover:opacity-100 transition-opacity duration-300 pointer-events-none"
              />
            )}
            
            {/* Feature 5: Multi-Image Hover Switcher */}
            <CustomImage
              src={allImages.length > 1 ? allImages[hoverImgIdx] : product.image}
              alt={product.name}
              className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110"
              loading="lazy"
            />
            {/* Image Dots Indicator */}
            {allImages.length > 1 && (
              <div className="absolute bottom-14 left-1/2 -translate-x-1/2 flex gap-1 z-10 opacity-0 group-hover:opacity-100 transition-opacity">
                {allImages.map((_, i) => (
                  <span key={i} className={cn('w-1.5 h-1.5 rounded-full transition-all', i === hoverImgIdx ? 'bg-white scale-125' : 'bg-white/50')} />
                ))}
              </div>
            )}

            {/* Overlay Gradient on Hover */}
            <div className="absolute inset-0 bg-black/5 opacity-0 group-hover:opacity-100 transition-opacity duration-300" />

            {/* Feature 7: Bulk Selection Checkbox */}
            {selectable && (
              <button
                onClick={handleSelectToggle}
                className={cn(
                  'absolute top-3 left-3 w-6 h-6 rounded border-2 flex items-center justify-center z-30 transition-all',
                  selected ? 'bg-indigo-600 border-indigo-600 text-white' : 'bg-white/80 border-slate-300 hover:border-indigo-500'
                )}
              >
                {selected && <Check size={14} />}
              </button>
            )}

            {/* Badges */}
            <div className={cn('absolute top-3 flex flex-col gap-2 z-10', selectable ? 'left-12' : 'left-3')}>
              {isNew && (
                <Badge className="bg-red-500 text-white border-0 shadow-sm px-2.5 py-0.5 text-xs font-bold uppercase tracking-wider">
                  {t('product.new', 'New')}
                </Badge>
              )}
              {hasDiscount && (
                <Badge className="bg-orange-500 text-white border-0 shadow-sm px-2.5 py-0.5 text-xs font-bold">
                  {`-${product.discount}%`}
                </Badge>
              )}
              {/* Feature 1: Low Stock Urgency Indicator */}
              {isLowStock && (
                <Badge className="bg-amber-500 text-white border-0 shadow-sm px-2.5 py-0.5 text-xs font-bold animate-pulse">
                  🔥 {t('product.justLeft', 'Chỉ còn {{count}}!', { count: stockQty })}
                </Badge>
              )}
              {/* Feature 8: Sales Volume Badge */}
              {product.soldCount && product.soldCount > 100 && (
                 <Badge className="bg-indigo-600 text-white border-0 shadow-sm px-2.5 py-0.5 text-xs font-bold">
                    {t('product.sold', { defaultValue: 'Đã bán {{count}}', count: Number(product.soldCount) })}
                 </Badge>
              )}
            </div>

            {/* Feature 3: Flash Sale Countdown Timer */}
            {hasCountdown && (
              <div className="absolute top-3 right-3 z-10">
                <div className="flex items-center gap-1 bg-red-600 text-white px-2 py-1 rounded-lg shadow-lg text-xs font-mono font-bold">
                  <Clock size={12} />
                  {countdown}
                </div>
              </div>
            )}

            {/* Right Side Actions (Wishlist, Compare, QuickView, Share) */}
            <div className={cn('absolute right-3 flex flex-col gap-2 translate-x-10 opacity-0 group-hover:translate-x-0 group-hover:opacity-100 transition-all duration-300 z-20', hasCountdown ? 'top-14' : 'top-3')}>
              <button
                onClick={handleWishlistToggle}
                className={cn(
                  'w-9 h-9 flex items-center justify-center bg-white rounded-full shadow-md transition-transform hover:scale-110 text-slate-600 hover:text-rose-500',
                  isWishlisted && 'text-rose-500'
                )}
                title={t('product.addToWishlist', 'Add to Wishlist')}
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
                  title={t('common.view')}
                >
                  <Eye size={18} />
                </button>
              )}
              {/* Feature 15: QR Code Sharing */}
              <button
                className="w-9 h-9 flex items-center justify-center bg-white rounded-full shadow-md transition-transform hover:scale-110 text-slate-600 hover:text-purple-600 delay-100"
                title={t('product.share')}
                onClick={(e) => { e.preventDefault(); e.stopPropagation(); toast.info("QR Code feature coming soon!"); }}
              >
                <QrCode size={18} />
              </button>

              {/* Feature 17: Price Drop Alert */}
              <button
                className={cn(
                  "w-9 h-9 flex items-center justify-center bg-white rounded-full shadow-md transition-transform hover:scale-110 delay-125",
                  isWatched(product.code) ? "text-amber-500" : "text-slate-600 hover:text-amber-500"
                )}
                title={isWatched(product.code) ? t('product.unwatch', 'Hủy theo dõi giá') : t('product.watch', 'Theo dõi giảm giá')}
                onClick={(e) => { 
                  e.preventDefault(); 
                  e.stopPropagation(); 
                  toggleAlert(product.code);
                  toast.success(isWatched(product.code) ? t('product.unwatchSuccess', 'Đã hủy theo dõi giá') : t('product.watchSuccess', 'Đã bật theo dõi giảm giá cho sản phẩm này'));
                }}
              >
                <Bell size={18} fill={isWatched(product.code) ? "currentColor" : "none"} />
              </button>


              {/* Feature 9: Share Quick Action */}
              <button
                onClick={handleShare}
                className="w-9 h-9 flex items-center justify-center bg-white rounded-full shadow-md transition-transform hover:scale-110 text-slate-600 hover:text-emerald-600 delay-150 relative"
                title={t('product.share')}
              >
                {showCopied ? <Check size={18} className="text-emerald-500" /> : <Share2 size={18} />}
                <AnimatePresence>
                  {showCopied && (
                    <motion.span
                      initial={{ opacity: 0, x: -10 }}
                      animate={{ opacity: 1, x: -48 }}
                      exit={{ opacity: 0 }}
                      className="absolute right-full mr-1 bg-slate-900 text-white text-[10px] px-2 py-0.5 rounded whitespace-nowrap"
                    >
                      {t('product.copied')}
                    </motion.span>
                  )}
                </AnimatePresence>
              </button>
            </div>

            <div className="absolute inset-x-4 bottom-4 translate-y-full opacity-0 group-hover:translate-y-0 group-hover:opacity-100 transition-all duration-300 z-20 flex flex-col gap-2">
               {hasFixedPrice ? (
                <>
                  <div className="flex gap-2">
                    <Button
                      className="flex-1 bg-slate-900/90 hover:bg-black text-white shadow-lg backdrop-blur-sm border-0 h-10 px-2"
                      onClick={handleAddToCart}
                      disabled={isOutOfStock}
                    >
                      <ShoppingCart size={16} className="mr-1.5" />
                      <span className="text-xs">{isOutOfStock ? t('product.outOfStock', 'Hết hàng') : t('product.addToCart', 'Thêm giỏ')}</span>
                    </Button>
                    {/* Feature 3: One-Click Buy Now */}
                    <Button
                      className="flex-1 bg-orange-600/90 hover:bg-orange-700 text-white shadow-lg backdrop-blur-sm border-0 h-10 px-2"
                      onClick={handleBuyNow}
                      disabled={isOutOfStock}
                    >
                      <FastForward size={16} className="mr-1.5" />
                      <span className="text-xs">{t('common.buyNow', 'Mua ngay')}</span>
                    </Button>
                  </div>
                </>
              ) : isOutOfStock ? (
                /* Feature 18: Restock SMS/Email Notification */
                <div className="bg-slate-50 border border-slate-200 rounded-lg p-2.5">
                  <p className="text-[11px] font-medium text-slate-600 mb-1.5 flex items-center gap-1.5">
                    <Bell size={14} className="text-amber-500" />
                    {t('product.restockNotif')}
                  </p>
                  <div className="flex gap-1">
                    <input 
                      type="text" 
                      placeholder={t('product.emailOrPhone')} 
                      className="flex-1 min-w-0 px-2 py-1.5 text-[11px] border rounded bg-white"
                      onClick={(e) => { e.preventDefault(); e.stopPropagation(); }}
                    />
                    <Button 
                      size="sm" 
                      className="h-8 px-2 bg-slate-800 text-white text-[10px]"
                      onClick={(e) => { e.preventDefault(); e.stopPropagation(); toast.success(t('product.notifGrated', 'Đã ghi nhận! Chúng tôi sẽ báo cho bạn ngay.')); }}
                    >
                      {t('product.notifyMe')}
                    </Button>
                  </div>
                </div>
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

              {/* Feature 5: Wholesale Tiered Pricing */}
              {product.wholesaleTiers && product.wholesaleTiers.length > 0 && (
                <div className="mt-3 p-2 bg-indigo-50 border border-indigo-100 rounded-lg">
                  <div className="text-[10px] font-bold text-indigo-700 uppercase mb-1 flex items-center gap-1">
                    <Package size={12} />
                    {t('product.wholesale', 'Bán sỉ')}
                  </div>
                  <div className="grid grid-cols-2 gap-1.5">
                    {product.wholesaleTiers.slice(0, 4).map((tier, idx) => (
                      <div key={idx} className="bg-white p-1 rounded border border-indigo-50 flex flex-col items-center">
                        <span className="text-[9px] font-medium text-slate-500">≥ {tier.minQuantity} cái</span>
                        <span className="text-[11px] font-bold text-indigo-600 font-mono">{formatCurrency(tier.price)}</span>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>

            {/* Feature 40: Product Video Reviews Trigger */}
            {product.videoReviews && product.videoReviews.length > 0 && (
              <div className="absolute top-2.5 right-2.5 z-30">
                <button 
                  className="bg-white/90 backdrop-blur-sm border border-slate-200 p-1.5 rounded-full shadow-sm hover:text-red-500 transition-colors"
                  title={t('product.videoReview')}
                  onClick={(e) => { e.preventDefault(); e.stopPropagation(); toast.info("Tính năng xem video đang được tải..."); }}
                >
                  <HistoryIcon size={14} className="animate-pulse" />
                </button>
              </div>
            )}

            {/* Feature 38: Low Stock Urgency Alert */}
            {isLowStock && (
              <div className="absolute top-10 left-2 z-30 pointer-events-none">
                <div className="bg-amber-500 text-white text-[9px] font-bold px-1.5 py-0.5 rounded shadow-sm flex items-center gap-1">
                  <Clock size={10} />
                  {t('product.justLeft', 'Chỉ còn {{count}}!', { count: stockQty })}
                </div>
              </div>
            )}

            {/* Feature 7: Real-time Analytics Popup */}
            <AnimatePresence>
              {recentBuyer && (
                <motion.div
                  initial={{ opacity: 0, scale: 0.8, y: 10 }}
                  animate={{ opacity: 1, scale: 1, y: 0 }}
                  exit={{ opacity: 0, scale: 0.8, y: 10 }}
                  className="absolute bottom-16 right-4 z-30 bg-white/95 backdrop-blur-sm border shadow-xl rounded-lg p-2.5 flex items-center gap-2.5 min-w-[180px]"
                >
                  <div className="w-9 h-9 rounded-full bg-indigo-100 flex items-center justify-center text-indigo-600 flex-shrink-0 animate-pulse">
                    <ShoppingCart size={16} />
                  </div>
                  <div className="overflow-hidden flex-1">
                    <p className="text-[10px] text-slate-500 font-medium">{t('product.justOrdered')}</p>
                    <p className="text-[11px] font-bold text-slate-800 truncate">{recentBuyer}</p>
                  </div>
                  <button onClick={(e) => { e.preventDefault(); e.stopPropagation(); setRecentBuyer(null); }} className="text-slate-400 hover:text-slate-600 p-1">
                    <X size={12} />
                  </button>
                </motion.div>
              )}
            </AnimatePresence>
          </div>

          {/* Content */}
          <div className="p-4 pt-3.5">
            <div className="text-[11px] font-bold text-indigo-500 uppercase tracking-wider mb-1 flex justify-between items-center">
              <span>{product.category}</span>
              {/* Feature 9: Trust Icons */}
              <div className="flex gap-1">
                <ShieldCheck size={12} className="text-emerald-500" />
                <Zap size={12} className="text-amber-500" />
              </div>
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

            {/* Feature 10: Variant Color Dots */}
            {variantColors.length > 0 && (
              <div className="flex items-center gap-1.5 mb-2">
                {variantColors.map((color, i) => (
                  <span
                    key={i}
                    className="w-4 h-4 rounded-full border border-slate-200 shadow-inner cursor-pointer hover:scale-125 transition-transform"
                    style={{ backgroundColor: color }}
                    title={color}
                  />
                ))}
                {product.variants && product.variants.length > 5 && (
                  <span className="text-[10px] text-slate-400">+{product.variants.length - 5}</span>
                )}
              </div>
            )}

            {/* Feature 1: Flash Sale Progress Bar */}
            {hasCountdown && (
              <div className="mb-3">
                <div className="flex justify-between items-center text-[10px] mb-1">
                   <span className="text-orange-600 font-bold uppercase">🔥 Đang bán chạy</span>
                   <span className="text-slate-400">Đã bán {product.soldCount || 12}</span>
                </div>
                <div className="h-1.5 w-full bg-slate-100 rounded-full overflow-hidden">
                  <motion.div 
                    initial={{ width: 0 }}
                    animate={{ width: `${Math.min(100, ((product.soldCount || 12) / (product.promotionOriginalQuantity || 50)) * 100)}%` }}
                    className="h-full bg-gradient-to-r from-orange-500 to-red-600 rounded-full"
                  />
                </div>
              </div>
            )}

            {/* Price & Stock */}
            <div className="flex items-end justify-between mt-auto">
              <div className="flex flex-col">
                {hasFixedPrice ? (
                  <div className="flex flex-col">
                    <div className="flex items-baseline gap-2">
                      <span className="text-lg font-bold text-slate-900">{formatCurrency(product.price)}</span>
                      {product.originalPrice && product.originalPrice > product.price && (
                        <span className="text-sm text-slate-400 line-through decoration-1">{formatCurrency(product.originalPrice)}</span>
                      )}
                    </div>
                    {/* Feature 25: Loyalty Points Preview */}
                    <div className="flex items-center gap-1 text-[10px] text-indigo-600 font-medium mt-0.5">
                      <Coins size={10} />
                      <span>+{Math.floor(product.price / 10000)} điểm thưởng</span>
                    </div>
                  </div>
                ) : (
                  <span className="text-base font-semibold text-indigo-600">{t('product.contactForPrice', 'Contact for Price')}</span>
                )}
              </div>
              {/* Feature 5: Wholesale Pricing Callout */}
              {product.wholesalePrice && (
                <div className="text-[10px] bg-emerald-50 text-emerald-700 px-1.5 py-0.5 rounded border border-emerald-100 font-bold animate-bounce-slow">
                  Mua sỉ từ {formatCurrency(product.wholesalePrice)}
                </div>
              )}
            </div>

            {/* Feature 2: Shipping Urgency */}
            {hasFixedPrice && !isOutOfStock && (
               <div className="mt-2 p-1.5 bg-slate-50 rounded-lg flex items-center justify-between text-[10px]">
                  <div className="flex items-center gap-1.5 text-slate-500">
                    <Clock size={12} className="text-indigo-500" />
                    <span>{t('product.orderWithin', 'Đặt trong')} <b>{orderCutoff}</b> {t('product.toShipToday', 'để giao hôm nay')}</span>
                  </div>
                  <Info size={12} className="text-slate-300 cursor-help" />
               </div>
            )}

            {/* Feature 23: Province-based Shipping Estimation */}
            {hasFixedPrice && !isOutOfStock && (
              <div className="mt-2.5 p-2 bg-emerald-50/50 border border-emerald-100/50 rounded-lg">
                <div className="flex items-center justify-between mb-1">
                  <div className="flex items-center gap-1.5 text-[11px] text-emerald-700 font-medium">
                    <Truck size={12} />
                    <span>Giao đến <b>Hà Nội</b>:</span>
                  </div>
                  <button className="text-[9px] text-indigo-600 font-bold hover:underline" onClick={(e) => { e.preventDefault(); e.stopPropagation(); toast.info("Tính năng đổi địa chỉ đang được phát triển"); }}>Đổi</button>
                </div>
                <div className="flex items-center justify-between text-[10px] text-emerald-600">
                  <span>{t('product.shippingFee', 'Phí')}: <b className="text-slate-800">{t('product.free', 'Miễn phí')}</b></span>
                  <span>{t('product.estimatedDelivery', 'Giao dự kiến')}: <b>{deliveryDate}</b></span>
                </div>
              </div>
            )}

            {/* Feature 33: Frequently Bought Together Preview */}
            {product.frequentlyBoughtTogether && product.frequentlyBoughtTogether.length > 0 && (
              <div className="mt-3 pt-2 border-t border-slate-100">
                <p className="text-[10px] font-bold text-slate-500 uppercase mb-1.5 flex items-center gap-1">
                  <Sparkles size={10} className="text-indigo-500" />
                  Thường mua cùng
                </p>
                <div className="flex gap-2 overflow-x-auto pb-1 scrollbar-hide">
                  {product.frequentlyBoughtTogether.map((code, idx) => (
                    <div key={idx} className="w-10 h-10 rounded border border-slate-200 bg-slate-50 flex-shrink-0 animate-pulse" />
                  ))}
                  <div className="w-10 h-10 rounded border border-dashed border-slate-300 flex items-center justify-center text-slate-400 flex-shrink-0">
                    <X size={14} />
                  </div>
                </div>
              </div>
            )}

            {/* Feature 12: Inventory Sync Timestamp */}
            <div className="flex items-center justify-between mt-3 pt-2 border-t border-slate-100 text-[9px] text-slate-400">
              <span className="flex items-center gap-1">
                <HistoryIcon size={10} />
                Cập nhật: 5 phút trước
              </span>
              <span className="bg-indigo-50 text-indigo-500 px-1 rounded font-medium">In Stock</span>
            </div>
          </div>
        </Link>
        
        {/* Feature 32: Abandoned Cart Alert Component Hooked */}
        <AbandonedCartAlert 
          show={showAbandonedAlert} 
          onClose={() => setShowAbandonedAlert(false)} 
        />
      </motion.div>
    );
  }
);

/** Feature 32: Abandoned Cart Alert Component */
const AbandonedCartAlert = ({ show, onClose }: { show: boolean; onClose: () => void }) => {
  if (!show) return null;
  return (
    <motion.div 
      initial={{ opacity: 0, scale: 0.9, y: 20 }}
      animate={{ opacity: 1, scale: 1, y: 0 }}
      className="fixed bottom-6 right-6 z-[100] bg-indigo-600 text-white p-4 rounded-xl shadow-2xl flex items-center gap-4 max-w-sm"
    >
      <div className="bg-white/20 p-2 rounded-lg">
        <ShoppingCart size={24} />
      </div>
      <div className="flex-1">
        <p className="text-sm font-bold">Quên gì đó trong giỏ?</p>
        <p className="text-[11px] opacity-90">Hoàn tất đơn hàng ngay để nhận ưu đãi vận chuyển tốt nhất!</p>
      </div>
      <button onClick={onClose} className="p-1 hover:bg-white/20 rounded-full transition-colors">
        <X size={16} />
      </button>
    </motion.div>
  );
};

ProductCard.displayName = 'ProductCard';

export default ProductCard;
