import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useNavigate } from 'react-router-dom';
import { ShoppingCart, Heart, Scale, X, Minus, Plus, Eye, Star } from 'lucide-react';

import { Modal, Button, Badge } from '@/components/ui';
import { useCartStore, useWishlistStore, useCompareStore, useToast } from '@/store';
import { formatCurrency } from '@/utils/format';
import CustomImage from '@/components/common/Image';
import type { Product } from '@/types';
import { cn } from '@/utils/cn';

interface QuickViewModalProps {
  product: Product | null;
  isOpen: boolean;
  onClose: () => void;
}

export const QuickViewModal: React.FC<QuickViewModalProps> = ({
  product,
  isOpen,
  onClose,
}) => {
  const { t } = useTranslation();
  const [quantity, setQuantity] = useState(1);
  const [isAddingToCart, setIsAddingToCart] = useState(false);
  const toast = useToast();
  const navigate = useNavigate();

  const addToCart = useCartStore((state) => state.addItem);
  const { addItem: addToWishlist, removeItem: removeFromWishlist, isInWishlist } = useWishlistStore();
  const { addItem: addToCompare, isInCompare } = useCompareStore();

  useEffect(() => {
    if (isOpen) {
      setQuantity(1);
    }
  }, [isOpen]);

  if (!product) return null;

  const isWishlisted = isInWishlist(product.code);
  const isCompared = isInCompare(product.code);
  const hasFixedPrice = product.price > 0;
  const isOutOfStock = (product.stockQuantity ?? product.stock) === 0;

  const handleAddToCart = async () => {
    if (hasFixedPrice && !isOutOfStock) {
      setIsAddingToCart(true);
      try {
        await addToCart(product, quantity);
        toast.success(t('product.addToCartSuccess') || 'Đã thêm vào giỏ hàng');
        onClose();
      } catch {
        toast.error(t('product.addToCartError') || 'Có lỗi xảy ra');
      } finally {
        setIsAddingToCart(false);
      }
    }
  };

  const handleBuyNow = async () => {
    if (hasFixedPrice && !isOutOfStock) {
      setIsAddingToCart(true);
      try {
        await addToCart(product, quantity);
        toast.success(t('product.addToCartSuccess') || 'Đã thêm vào giỏ hàng');
        onClose();
        navigate('/checkout');
      } catch {
        toast.error(t('product.addToCartError') || 'Có lỗi xảy ra');
      } finally {
        setIsAddingToCart(false);
      }
    }
  };

  const renderStars = () => {
    const ratingValue = product.averageRating ?? product.rating ?? 0;
    return Array.from({ length: 5 }).map((_, i) => (
      <Star
        key={i}
        size={14}
        className={cn(
          i < Math.floor(ratingValue) ? 'fill-yellow-400 text-yellow-400' : 'text-gray-200'
        )}
      />
    ));
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={product.name}
      size="xl"
      showCloseButton={false}
    >
      <div className="relative">
        <button
          onClick={onClose}
          className="absolute -top-12 right-0 p-2 text-white hover:bg-white/10 rounded-full transition-colors z-50 md:-right-4"
        >
          <X size={24} />
        </button>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          {/* Image Section */}
          <div className="relative aspect-[4/5] rounded-2xl overflow-hidden bg-slate-50 border border-slate-100">
            <CustomImage
              src={product.image}
              alt={product.name}
              className="w-full h-full object-cover"
            />
            <div className="absolute top-4 left-4 flex flex-col gap-2">
              {product.isNew && (
                <Badge className="bg-red-500 text-white border-0 shadow-sm">
                  {t('product.new', 'New')}
                </Badge>
              )}
              {product.isFeatured && (
                <Badge className="bg-rose-600 text-white border-0 shadow-sm">
                  {t('product.featured', 'HOT')}
                </Badge>
              )}
              {product.discount && product.discount > 0 && (
                <Badge className="bg-orange-500 text-white border-0 shadow-sm">
                  -{product.discount}%
                </Badge>
              )}
            </div>
          </div>

          {/* Info Section */}
          <div className="flex flex-col h-full py-2">
            <div>
              <div className="text-xs font-bold text-indigo-500 uppercase tracking-wider mb-2">
                {product.category}
              </div>
              <h2 className="text-2xl font-bold text-slate-900 mb-3 leading-tight">
                {product.name}
              </h2>

              <div className="flex items-center gap-2 mb-6">
                <div className="flex">{renderStars()}</div>
                <span className="text-xs text-slate-400 font-medium">
                  ({product.reviewCount} {t('product.reviews')})
                </span>
                <span className="text-slate-300 mx-2">|</span>
                <span className={cn(
                  "text-xs font-bold uppercase tracking-wide",
                  isOutOfStock ? "text-rose-500" : "text-emerald-500"
                )}>
                  {isOutOfStock ? t('product.outOfStock') : t('product.inStock')}
                </span>
              </div>

              <div className="mb-6">
                {hasFixedPrice ? (
                  <div className="flex items-baseline gap-3">
                    <span className="text-3xl font-bold text-slate-900">
                      {formatCurrency(product.price)}
                    </span>
                    {product.originalPrice && product.originalPrice > product.price && (
                      <span className="text-lg text-slate-400 line-through">
                        {formatCurrency(product.originalPrice)}
                      </span>
                    )}
                  </div>
                ) : (
                  <span className="text-xl font-semibold text-indigo-600">
                    {t('product.contactForPrice')}
                  </span>
                )}
              </div>

              <p className="text-slate-500 text-sm leading-relaxed mb-8 line-clamp-4">
                {product.description}
              </p>
            </div>

            <div className="mt-auto space-y-6">
              {hasFixedPrice && !isOutOfStock && (
                <div className="flex items-center gap-4">
                  <div className="flex items-center h-12 border border-slate-200 rounded-xl bg-white overflow-hidden shadow-sm">
                    <button
                      onClick={() => setQuantity(Math.max(1, quantity - 1))}
                      className="w-12 h-full flex items-center justify-center hover:bg-slate-50 text-slate-600 transition-colors"
                    >
                      <Minus size={16} />
                    </button>
                    <span className="w-10 text-center font-bold text-slate-900">{quantity}</span>
                    <button
                      onClick={() => setQuantity(Math.min(product.stockQuantity ?? product.stock ?? 999, quantity + 1))}
                      className="w-12 h-full flex items-center justify-center hover:bg-slate-50 text-slate-600 transition-colors border-l border-slate-100"
                    >
                      <Plus size={16} />
                    </button>
                  </div>

                  <div className="flex-1 flex flex-col sm:flex-row gap-3">
                    <Button
                      size="lg"
                      variant="outline"
                      className="flex-1 border-slate-900 text-slate-900 hover:bg-slate-50"
                      onClick={handleAddToCart}
                      isLoading={isAddingToCart}
                      disabled={isOutOfStock}
                      leftIcon={!isAddingToCart && <ShoppingCart size={20} />}
                    >
                      {t('product.addToCart')}
                    </Button>
                    <Button
                      size="lg"
                      className="flex-1 bg-slate-900 hover:bg-black text-white"
                      onClick={handleBuyNow}
                      disabled={isOutOfStock || isAddingToCart}
                    >
                      {t('product.buyNow', 'Mua ngay')}
                    </Button>
                  </div>
                </div>
              )}

              <div className="flex items-center gap-4 pt-4 border-t border-slate-100">
                <button
                  onClick={() => isWishlisted ? removeFromWishlist(product.code) : addToWishlist(product)}
                  className={cn(
                    "flex-1 flex items-center justify-center gap-2 py-2 text-sm font-semibold rounded-lg transition-colors border",
                    isWishlisted 
                      ? "bg-rose-50 border-rose-100 text-rose-600" 
                      : "bg-white border-slate-200 text-slate-600 hover:bg-slate-50"
                  )}
                >
                  <Heart size={18} fill={isWishlisted ? 'currentColor' : 'none'} />
                  {isWishlisted ? t('product.inWishlist') : t('product.addToWishlist')}
                </button>
                <button
                  onClick={() => addToCompare(product)}
                  className={cn(
                    "flex-1 flex items-center justify-center gap-2 py-2 text-sm font-semibold rounded-lg transition-colors border",
                    isCompared 
                      ? "bg-indigo-50 border-indigo-100 text-indigo-600" 
                      : "bg-white border-slate-200 text-slate-600 hover:bg-slate-50"
                  )}
                >
                  <Scale size={18} />
                  {t('product.compare')}
                </button>
              </div>

              <Link
                to={`/product/${product.code}`}
                onClick={onClose}
                className="flex items-center justify-center gap-2 text-indigo-600 font-bold text-sm hover:translate-x-1 transition-transform group"
              >
                {t('product.viewDetails', 'Xem chi tiết đầy đủ')}
                <Eye size={16} className="group-hover:scale-110 transition-transform" />
              </Link>
            </div>
          </div>
        </div>
      </div>
    </Modal>
  );
};
