import React, { useMemo } from 'react';
import { motion } from 'framer-motion';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { Phone, Minus, Plus, ShoppingCart, Heart, Scale, Share2, Star, Zap } from 'lucide-react';
import { Button, Badge, type BadgeColor } from '@/components/ui';
import { useToast } from '@/store';
import { formatCurrency } from '@/utils/format';
import { ProductLogistics } from './ProductLogistics';
import type { Product } from '@/types';

interface ProductInfoProps {
  product: Product;
  hasFixedPrice: boolean;
  stockStatus: { label: string; color: BadgeColor; text: string } | null;
  quantity: number;
  setQuantity: (q: number) => void;
  handleAddToCart: () => void;
  handleBuyNow: () => void;
  isAddingToCart: boolean;
  isWishlisted: boolean;
  handleWishlistToggle: () => void;
  isCompared: boolean;
  handleCompareToggle: () => void;
}

export const ProductInfo: React.FC<ProductInfoProps> = ({
  product,
  hasFixedPrice,
  stockStatus,
  quantity,
  setQuantity,
  handleAddToCart,
  handleBuyNow,
  isAddingToCart,
  isWishlisted,
  handleWishlistToggle,
  isCompared,
  handleCompareToggle,
}) => {
  const { t } = useTranslation();
  const { success } = useToast();

  // Stars renderer
  const renderStars = useMemo(() => {
    if (!product) return null;
    const ratingValue = product.averageRating ?? product.rating ?? 0;
    return Array.from({ length: 5 }).map((_, i) => (
      <Star
        key={i}
        size={18}
        className={i < Math.floor(ratingValue) ? 'fill-yellow-400 text-yellow-400' : 'text-gray-300'}
      />
    ));
  }, [product]);

  return (
    <div className="space-y-6 lg:col-span-3">
      {/* Badges */}
      <div className="flex flex-wrap gap-2">
        {product.isNew && <Badge color="success">{t('product.new')}</Badge>}
        {product.discount && product.discount > 0 && (
          <Badge color="error">-{product.discount}%</Badge>
        )}
        {!hasFixedPrice && <Badge color="primary">{t('product.contactForPrice')}</Badge>}
        {stockStatus && stockStatus.color !== 'success' && (
          <Badge color={stockStatus.color}>{stockStatus.label}</Badge>
        )}
      </div>

      {/* Title */}
      <h1 className="text-2xl md:text-3xl font-bold text-primary">{product.name}</h1>

      {/* Rating & Reviews */}
      <div className="flex items-center gap-4 flex-wrap">
        <div className="flex items-center gap-1">{renderStars}</div>
        <span className="text-secondary">
          {product.averageRating?.toFixed(1) ?? '0.0'} ({product.reviewCount} {t('product.reviews')})
        </span>
        <span className="text-tertiary">|</span>
        <span className={`font-medium ${stockStatus?.text || ''}`}>
          {stockStatus?.label}
        </span>
      </div>

      {/* Price & Urgency */}
      <div className="py-6 border-y bg-slate-50/50 dark:bg-slate-800/30 rounded-2xl px-6">
        {hasFixedPrice ? (
          <div className="flex flex-col gap-4">
            <div className="flex items-baseline gap-3">
              <span className="text-4xl font-black text-rose-600 dark:text-rose-500 tracking-tight">
                {formatCurrency(product.price)}
              </span>
              {product.originalPrice && product.originalPrice > product.price && (
                <span className="text-xl text-slate-400 line-through font-medium">
                  {formatCurrency(product.originalPrice)}
                </span>
              )}
            </div>
            
            {/* Sale Progress Bar (Mocked Logic) */}
            {product.discount && product.discount > 0 && (
              <div className="space-y-2">
                <div className="flex justify-between items-center text-xs font-bold uppercase tracking-wider text-slate-500">
                  <span className="flex items-center gap-1.5 text-rose-600">
                    <Zap size={14} className="fill-current" />
                    {t('product.flashSale', 'Siêu ưu đãi kết thúc sau')}
                  </span>
                  <span className="text-slate-900 dark:text-slate-200">23:54:12</span>
                </div>
                <div className="h-2 w-full bg-slate-200 dark:bg-slate-700 rounded-full overflow-hidden">
                  <motion.div 
                    initial={{ width: 0 }}
                    animate={{ width: '75%' }}
                    transition={{ duration: 1, ease: "easeOut" }}
                    className="h-full bg-gradient-to-r from-rose-500 to-amber-500 rounded-full"
                  />
                </div>
                <div className="flex justify-between text-[10px] text-slate-400 font-bold uppercase">
                  <span>{t('product.itemsSold', 'Đã bán: 15')}</span>
                  <span>{t('product.itemsRemaining', 'Còn lại: 5')}</span>
                </div>
              </div>
            )}

            {product.wholesalePrice && (
              <div className="flex items-center gap-2 text-sm text-secondary pt-2 border-t border-slate-100 dark:border-slate-700">
                <Badge color="secondary" className="font-semibold uppercase tracking-tighter">{t('product.wholesalePrice', 'Giá sỉ')}</Badge>
                <span className="font-bold text-slate-900 dark:text-white">{formatCurrency(product.wholesalePrice)}</span>
                <span className="text-[11px] opacity-70">({t('product.contactForBestPrice', 'Liên hệ để nhận ưu đãi tốt nhất')})</span>
              </div>
            )}

            {/* Stock Urgency Alert */}
            {(product.stockQuantity ?? product.stock) > 0 && (product.stockQuantity ?? product.stock) <= 10 && (
              <motion.div 
                initial={{ opacity: 0, x: -10 }}
                animate={{ opacity: 1, x: 0 }}
                className="flex items-center gap-2 text-rose-600 bg-rose-50 dark:bg-rose-900/20 px-3 py-2 rounded-lg border border-rose-100 dark:border-rose-900/30 text-xs font-bold"
              >
                <div className="w-2 h-2 rounded-full bg-rose-600 animate-pulse" />
                {t('product.lowStockAlert', 'Chỉ còn {{count}} sản phẩm trong kho - Đặt ngay!', { count: product.stockQuantity ?? product.stock })}
              </motion.div>
            )}
          </div>
        ) : (
          <div className="flex items-center gap-3 py-2">
            <Phone className="text-indigo-600" />
            <span className="text-2xl font-bold text-slate-900 dark:text-white">{t('product.contactForPrice')}</span>
          </div>
        )}
      </div>

      {/* Core Info Details */}
      <div className="grid grid-cols-2 gap-y-2 text-sm">
        <div className="text-tertiary">{t('product.code', 'Mã sản phẩm')}:</div>
        <div className="text-primary font-medium">{product.code}</div>
        
        <div className="text-tertiary">{t('product.unit', 'Đơn vị tính')}:</div>
        <div className="text-primary">{product.baseUnit || t('product.defaultUnit', 'Cái')}</div>
        
        <div className="text-tertiary">{t('product.binLocation', 'Vị trí kho')}:</div>
        <div className="text-primary">{product.binLocation || t('common.updating', 'Đang cập nhật')}</div>
      </div>

      {/* Description */}
      <p className="text-secondary leading-relaxed line-clamp-3">{product.description}</p>

      {/* Product Logistics */}
      <ProductLogistics product={product} />

      {/* Quantity & Add to Cart */}
      {hasFixedPrice && (
        <div className="flex flex-col sm:flex-row gap-4">
          {/* Quantity */}
          <div className="flex items-center border rounded-lg bg-white">
            <button
              onClick={() => setQuantity(Math.max(1, quantity - 1))}
              className="p-3 hover:bg-hover transition-colors"
            >
              <Minus size={18} />
            </button>
            <span className="w-16 text-center font-semibold">{quantity}</span>
            <button
              onClick={() => setQuantity(Math.min(product.stockQuantity ?? product.stock, quantity + 1))}
              disabled={quantity >= (product.stockQuantity ?? product.stock)}
              className={`p-3 transition-colors ${quantity >= (product.stockQuantity ?? product.stock) ? 'opacity-50 cursor-not-allowed' : 'hover:bg-hover'}`}
            >
              <Plus size={18} />
            </button>
          </div>

          {/* Actions */}
          <div className="flex-1 flex flex-col sm:flex-row gap-3">
            <Button
              size="lg"
              variant="outline"
              className="flex-1 border-slate-900 text-slate-900 hover:bg-hover"
              onClick={handleAddToCart}
              disabled={(product.stockQuantity ?? product.stock) === 0 || isAddingToCart}
              isLoading={isAddingToCart}
              leftIcon={!isAddingToCart && <ShoppingCart size={20} />}
            >
              {t('product.addToCart')}
            </Button>
            <Button
              size="lg"
              className="flex-1 bg-slate-900 hover:bg-black text-white"
              onClick={handleBuyNow}
              disabled={(product.stockQuantity ?? product.stock) === 0 || isAddingToCart}
            >
              {t('product.buyNow', 'Mua ngay')}
            </Button>
          </div>
        </div>
      )}

      {/* Contact for Quote */}
      {!hasFixedPrice && (
        <Link to={`/quote-request/${product.code}`}>
          <Button size="lg" fullWidth leftIcon={<Phone size={20} />}>
            {t('product.requestQuote')}
          </Button>
        </Link>
      )}

      {/* Secondary Actions */}
      <div className="flex flex-wrap gap-3">
        <Button
          variant={isWishlisted ? 'primary' : 'outline'}
          onClick={handleWishlistToggle}
          leftIcon={<Heart size={18} fill={isWishlisted ? 'currentColor' : 'none'} />}
        >
          {isWishlisted ? t('product.inWishlist') : t('product.addToWishlist')}
        </Button>
        <Button
          variant={isCompared ? 'primary' : 'outline'}
          onClick={handleCompareToggle}
          leftIcon={<Scale size={18} />}
        >
          {t('product.compare')}
        </Button>
        <Button 
          variant="ghost" 
          leftIcon={<Share2 size={18} />}
          onClick={() => {
            if (navigator.share) {
              navigator.share({
                title: product.name,
                text: product.description,
                url: window.location.href,
              }).catch(() => {});
            } else {
              navigator.clipboard.writeText(window.location.href);
              success(t('common.copiedToClipboard', 'Đã sao chép liên kết'));
            }
          }}
        >
          {t('product.share')}
        </Button>
      </div>
    </div>
  );
};
