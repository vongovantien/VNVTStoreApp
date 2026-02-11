import React, { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { Phone, Minus, Plus, ShoppingCart, Heart, Scale, Share2, Star } from 'lucide-react';
import { Button, Badge, type BadgeColor } from '@/components/ui';
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
  isAddingToCart,
  isWishlisted,
  handleWishlistToggle,
  isCompared,
  handleCompareToggle,
}) => {
  const { t } = useTranslation();

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

      {/* Price */}
      <div className="py-4 border-y">
        {hasFixedPrice ? (
          <div className="flex flex-col gap-2">
            <div className="flex items-baseline gap-3">
              <span className="text-3xl font-bold text-error">{formatCurrency(product.price)}</span>
              {product.originalPrice && product.originalPrice > product.price && (
                <span className="text-xl text-tertiary line-through">
                  {formatCurrency(product.originalPrice)}
                </span>
              )}
            </div>
            {product.wholesalePrice && (
              <div className="flex items-center gap-2 text-sm text-secondary">
                <Badge color="secondary" className="font-normal">{t('product.wholesalePrice', 'Giá sỉ')}</Badge>
                <span className="font-bold">{formatCurrency(product.wholesalePrice)}</span>
                <span className="text-[11px] opacity-70">({t('product.contactForBestPrice', 'Liên hệ để nhận ưu đãi tốt nhất')})</span>
              </div>
            )}
          </div>
        ) : (
          <div className="flex items-center gap-3">
            <Phone className="text-primary" />
            <span className="text-xl font-semibold text-primary">{t('product.contactForPrice')}</span>
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
              className="p-3 hover:bg-secondary transition-colors"
            >
              <Minus size={18} />
            </button>
            <span className="w-16 text-center font-semibold">{quantity}</span>
            <button
              onClick={() => setQuantity(Math.min(product.stockQuantity ?? product.stock, quantity + 1))}
              disabled={quantity >= (product.stockQuantity ?? product.stock)}
              className={`p-3 transition-colors ${quantity >= (product.stockQuantity ?? product.stock) ? 'opacity-50 cursor-not-allowed' : 'hover:bg-secondary'}`}
            >
              <Plus size={18} />
            </button>
          </div>

          {/* Add to Cart */}
          <Button
            size="lg"
            className="flex-1 bg-slate-900 hover:bg-black text-white"
            onClick={handleAddToCart}
            disabled={(product.stockQuantity ?? product.stock) === 0 || isAddingToCart}
            isLoading={isAddingToCart}
            leftIcon={!isAddingToCart && <ShoppingCart size={20} />}
          >
            {t('product.addToCart')}
          </Button>
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
        <Button variant="ghost" leftIcon={<Share2 size={18} />}>
          {t('product.share')}
        </Button>
      </div>
    </div>
  );
};
