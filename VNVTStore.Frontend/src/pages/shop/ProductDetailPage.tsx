import { useState, useMemo, useCallback, memo, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import {
  ChevronRight,
  Heart,
  ShoppingCart,
  Share2,
  Truck,
  Shield,
  RefreshCw,
  Star,
  Minus,
  Plus,
  Phone,
  Scale,
  Loader2,
  AlertCircle,
  X,
} from 'lucide-react';
import { Button, Badge } from '@/components/ui';
import { ProductCard } from '@/components/common/ProductCard';
import SharedImage from '@/components/common/Image';
import { useCartStore, useWishlistStore, useCompareStore, useToast } from '@/store';
import { useProduct, useProducts } from '@/hooks';
import { formatCurrency } from '@/utils/format';
import { reviewService, type ReviewDto } from '@/services';

// ============ Image Gallery Component ============
interface ImageGalleryProps {
  images: string[];
  productName: string;
}

// ============ Image Lightbox Component ============
const ImageLightbox = ({
  images,
  initialIndex,
  isOpen,
  onClose,
}: {
  images: string[];
  initialIndex: number;
  isOpen: boolean;
  onClose: () => void;
}) => {
  const [index, setIndex] = useState(initialIndex);

  // Sync internal index when initialIndex changes or lightbox opens
  useEffect(() => {
    if (isOpen) setIndex(initialIndex);
  }, [initialIndex, isOpen]);

  // Keyboard navigation
  useEffect(() => {
    if (!isOpen) return;
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
      if (e.key === 'ArrowLeft') setIndex((prev) => (prev > 0 ? prev - 1 : prev));
      if (e.key === 'ArrowRight') setIndex((prev) => (prev < images.length - 1 ? prev + 1 : prev));
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose, images.length]);

  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/90 backdrop-blur-sm"
          onClick={onClose}
        >
          {/* Close Button */}
          <button
            onClick={onClose}
            className="absolute top-4 right-4 p-2 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors z-50"
          >
            <X size={32} />
          </button>

          {/* Main Image Container */}
          <div className="relative w-full h-full flex items-center justify-center p-4 pointer-events-none">
            <motion.div
              key={index}
              initial={{ opacity: 0, scale: 0.9 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ type: "spring", stiffness: 300, damping: 30 }}
              className="relative max-w-full max-h-full pointer-events-auto"
              onClick={(e) => e.stopPropagation()} 
            >
              <SharedImage
                src={images[index]}
                alt={`Gallery image ${index + 1}`}
                className="max-w-full max-h-[90vh] object-contain select-none shadow-2xl"
              />
            </motion.div>

            {/* Prev Button */}
            {index > 0 && (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  setIndex(index - 1);
                }}
                className="absolute left-4 top-1/2 -translate-y-1/2 p-3 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors pointer-events-auto"
              >
                <ChevronRight className="rotate-180" size={40} />
              </button>
            )}

            {/* Next Button */}
            {index < images.length - 1 && (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  setIndex(index + 1);
                }}
                className="absolute right-4 top-1/2 -translate-y-1/2 p-3 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors pointer-events-auto"
              >
                <ChevronRight size={40} />
              </button>
            )}
            
            {/* Image Counter */}
            <div className="absolute top-4 left-4 px-3 py-1 bg-black/50 text-white rounded-full text-sm font-medium pointer-events-auto">
                {index + 1} / {images.length}
            </div>

            {/* Thumbnails Strip */}
            <div className="absolute bottom-4 left-0 right-0 flex justify-center gap-2 px-4 pointer-events-none">
              <div className="flex gap-2 p-2 bg-black/50 rounded-xl overflow-x-auto max-w-full pointer-events-auto">
                {images.map((img, i) => (
                  <button
                    key={i}
                    onClick={(e) => {
                      e.stopPropagation();
                      setIndex(i);
                    }}
                    className={`w-12 h-12 flex-shrink-0 rounded-lg overflow-hidden border-2 transition-all ${
                      i === index ? 'border-primary scale-110' : 'border-transparent opacity-50 hover:opacity-100'
                    }`}
                  >
                    <SharedImage 
                      src={img} 
                      alt={`Thumbnail ${i + 1}`} 
                      className="w-full h-full object-cover" 
                    />
                  </button>
                ))}
              </div>
            </div>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};

const ImageGallery = memo(({ images, productName }: ImageGalleryProps) => {
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [lightboxOpen, setLightboxOpen] = useState(false);

  return (
    <div className="space-y-4">
      {/* Main Image */}
      <div 
        className="aspect-[4/3] max-h-[500px] rounded-2xl overflow-hidden bg-secondary border border-tertiary cursor-zoom-in relative group"
        onClick={() => setLightboxOpen(true)}
      >
        <motion.div
            key={selectedIndex}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ duration: 0.3 }}
            className="w-full h-full"
        >
            <SharedImage
                src={images[selectedIndex]}
                alt={productName}
                className="w-full h-full object-contain bg-white transition-transform duration-300 group-hover:scale-105"
            />
        </motion.div>
        
        {/* Hover Hint */}
        <div className="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity bg-black/10 pointer-events-none">
            <span className="bg-black/60 text-white text-xs px-3 py-1.5 rounded-full backdrop-blur-sm">
                Click to expand
            </span>
        </div>
      </div>

      {/* Thumbnails */}
      {images.length > 1 && (
        <div className="flex gap-2 overflow-x-auto pb-2">
          {images.map((img, index) => (
            <button
              key={index}
              onClick={() => setSelectedIndex(index)}
              className={`w-20 h-20 flex-shrink-0 rounded-lg overflow-hidden border-2 transition-all ${index === selectedIndex ? 'border-primary' : 'border-transparent opacity-70 hover:opacity-100'
                }`}
            >
              <SharedImage src={img} alt={`${productName} ${index + 1}`} className="w-full h-full object-cover" />
            </button>
          ))}
        </div>
      )}

      {/* Lightbox */}
      <ImageLightbox 
        images={images}
        initialIndex={selectedIndex}
        isOpen={lightboxOpen}
        onClose={() => setLightboxOpen(false)}
      />
    </div>
  );
});

ImageGallery.displayName = 'ImageGallery';

// ============ Product Detail Page ============
export const ProductDetailPage = () => {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  // Mock attributes
  const sizes = ['S', 'M', 'L', 'XL'];
  const colors = ['Black', 'White', 'Blue', 'Red'];
  
  const [quantity, setQuantity] = useState(1);
  const [isAddingToCart, setIsAddingToCart] = useState(false); // New state
  const [selectedSize, setSelectedSize] = useState<string>('M'); // Default
  const [selectedColor, setSelectedColor] = useState<string>('Black'); // Default
  const [activeTab, setActiveTab] = useState<'description' | 'specs' | 'reviews'>('description');
  const [reviews, setReviews] = useState<ReviewDto[]>([]);

  // Store actions
  const addToCart = useCartStore((state) => state.addItem);
  const { addItem: addToWishlist, removeItem: removeFromWishlist, isInWishlist } = useWishlistStore();
  const { addItem: addToCompare, isInCompare } = useCompareStore();
  const { success, error: toastError } = useToast();

  // Fetch product from API
  const { data: product, isLoading, isError, error } = useProduct(id || '');

  // Fetch Reviews
  useEffect(() => {
      if (id) {
          reviewService.search({ search: id, searchField: 'productCode' }).then(res => {
              if (res.success && res.data) {
                  setReviews(res.data.items);
              }
          });
      }
  }, [id]);

  // Fetch related products (same category)
  const { data: relatedData } = useProducts({
    pageIndex: 1,
    pageSize: 4,
    enabled: !!product?.categoryId,
  });

  const relatedProducts = useMemo(
    () => (relatedData?.products || []).filter((p) => p.code !== id).slice(0, 4),
    [relatedData?.products, id]
  );

  // Derived states
  const isWishlisted = product ? isInWishlist(product.code) : false;
  const isCompared = product ? isInCompare(product.code) : false;
  const hasFixedPrice = product ? product.price > 0 : false;
  const images = product ? (product.images?.length ? product.images : [product.image]) : [];

  // Handlers
  const handleAddToCart = useCallback(async () => {
    if (product && hasFixedPrice) {
        setIsAddingToCart(true);
        try {
            await addToCart(product, quantity, { size: selectedSize, color: selectedColor });
            success(t('product.addToCartSuccess') || 'Đã thêm vào giỏ hàng');
        } catch (err) {
            console.error(err);
            toastError(t('product.addToCartError') || 'Có lỗi xảy ra');
        } finally {
            setIsAddingToCart(false);
        }
    }
  }, [product, hasFixedPrice, quantity, addToCart, selectedSize, selectedColor, success, toastError]);

  const handleWishlistToggle = useCallback(() => {
    if (product) {
      isWishlisted ? removeFromWishlist(product.code) : addToWishlist(product);
    }
  }, [product, isWishlisted, addToWishlist, removeFromWishlist]);

  const handleCompareToggle = useCallback(() => {
    if (product) {
      addToCompare(product);
    }
  }, [product, addToCompare]);

  // Stars renderer
  const renderStars = useMemo(() => {
    if (!product) return null;
    return Array.from({ length: 5 }).map((_, i) => (
      <Star
        key={i}
        size={18}
        className={i < Math.floor(product.rating) ? 'fill-yellow-400 text-yellow-400' : 'text-gray-300'}
      />
    ));
  }, [product]);

  // Loading state
  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader2 className="w-8 h-8 animate-spin text-accent" />
        <span className="ml-3 text-secondary">Đang tải sản phẩm...</span>
      </div>
    );
  }

  // Error state
  if (isError) {
    return (
      <div className="container mx-auto px-4 py-16 text-center">
        <AlertCircle className="w-12 h-12 mx-auto mb-4 text-red-500" />
        <h1 className="text-2xl font-bold mb-4">Có lỗi xảy ra</h1>
        <p className="text-secondary mb-6">
          {error instanceof Error ? error.message : 'Không thể tải thông tin sản phẩm'}
        </p>
        <Link to="/products">
          <Button>{t('common.backToProducts')}</Button>
        </Link>
      </div>
    );
  }

  // Not found state
  if (!product) {
    return (
      <div className="container mx-auto px-4 py-16 text-center">
        <h1 className="text-2xl font-bold mb-4">{t('common.notFound')}</h1>
        <Link to="/products">
          <Button>{t('common.backToProducts')}</Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-secondary">
      {/* Breadcrumb */}
      <div className="bg-primary border-b py-3">
        <div className="container mx-auto px-4 flex items-center gap-2 text-sm flex-wrap">
          <Link to="/" className="text-secondary hover:text-primary">{t('common.home')}</Link>
          <ChevronRight size={14} className="text-tertiary" />
          <Link to="/products" className="text-secondary hover:text-primary">{t('common.products')}</Link>
          <ChevronRight size={14} className="text-tertiary" />
          <Link to={`/products?category=${product.categoryId}`} className="text-secondary hover:text-primary">
            {product.category}
          </Link>
          <ChevronRight size={14} className="text-tertiary" />
          <span className="text-primary font-medium truncate max-w-[200px]">{product.name}</span>
        </div>
      </div>

      <div className="container mx-auto px-4 py-8">
        {/* Main Content */}
        <div className="grid grid-cols-1 lg:grid-cols-5 gap-8 mb-12">
          {/* Image Gallery */}
          <div className="lg:col-span-2">
            <ImageGallery images={images} productName={product.name} />
          </div>

          {/* Product Info */}
          <div className="space-y-6 lg:col-span-3">
            {/* Badges */}
            <div className="flex flex-wrap gap-2">
              {product.isNew && <Badge color="success">{t('product.new')}</Badge>}
              {product.discount && product.discount > 0 && (
                <Badge color="error">-{product.discount}%</Badge>
              )}
              {!hasFixedPrice && <Badge color="primary">{t('product.contactForPrice')}</Badge>}
            </div>

            {/* Title */}
            <h1 className="text-2xl md:text-3xl font-bold text-primary">{product.name}</h1>

            {/* Rating & Reviews */}
            <div className="flex items-center gap-4 flex-wrap">
              <div className="flex items-center gap-1">{renderStars}</div>
              <span className="text-secondary">
                {product.rating} ({product.reviewCount} {t('product.reviews')})
              </span>
              <span className="text-tertiary">|</span>
              <span className="text-success font-medium">
                {product.stock > 0 ? `${t('product.inStock')} (${product.stock})` : t('product.outOfStock')}
              </span>
            </div>

            {/* Price */}
            <div className="py-4 border-y">
              {hasFixedPrice ? (
                <div className="flex items-baseline gap-3">
                  <span className="text-3xl font-bold text-error">{formatCurrency(product.price)}</span>
                  {product.originalPrice && product.originalPrice > product.price && (
                    <span className="text-xl text-tertiary line-through">
                      {formatCurrency(product.originalPrice)}
                    </span>
                  )}
                </div>
              ) : (
                <div className="flex items-center gap-3">
                  <Phone className="text-primary" />
                  <span className="text-xl font-semibold text-primary">{t('product.contactForPrice')}</span>
                </div>
              )}
            </div>

            {/* Description */}
            <p className="text-secondary leading-relaxed">{product.description}</p>

            {/* Attributes Selection */}
            {hasFixedPrice && (
                <div className="space-y-4 mb-6">
                    {/* Size */}
                    <div>
                        <span className="font-medium text-primary block mb-2">{t('product.size') || 'Size'}</span>
                        <div className="flex flex-wrap gap-2">
                            {sizes.map(size => (
                                <button
                                    key={size}
                                    onClick={() => setSelectedSize(size)}
                                    className={`px-3 py-1 border rounded-md text-sm transition-colors ${
                                        selectedSize === size 
                                            ? 'border-primary bg-primary text-secondary' 
                                            : 'border-gray-200 hover:border-primary'
                                    }`}
                                >
                                    {size}
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Color */}
                    <div>
                        <span className="font-medium text-primary block mb-2">{t('product.color') || 'Color'}</span>
                        <div className="flex flex-wrap gap-2">
                            {colors.map(color => (
                                <button
                                    key={color}
                                    onClick={() => setSelectedColor(color)}
                                    className={`px-3 py-1 border rounded-md text-sm transition-colors ${
                                        selectedColor === color 
                                            ? 'border-primary bg-primary text-secondary' 
                                            : 'border-gray-200 hover:border-primary'
                                    }`}
                                >
                                    {color}
                                </button>
                            ))}
                        </div>
                    </div>
                </div>
            )}

            {/* Quantity & Add to Cart */}
            {hasFixedPrice && (
              <div className="flex flex-col sm:flex-row gap-4">
                {/* Quantity */}
                <div className="flex items-center border rounded-lg">
                  <button
                    onClick={() => setQuantity(Math.max(1, quantity - 1))}
                    className="p-3 hover:bg-secondary transition-colors"
                  >
                    <Minus size={18} />
                  </button>
                  <span className="w-16 text-center font-semibold">{quantity}</span>
                  <button
                    onClick={() => setQuantity(Math.min(product.stock, quantity + 1))}
                    disabled={quantity >= product.stock}
                    className={`p-3 transition-colors ${quantity >= product.stock ? 'opacity-50 cursor-not-allowed' : 'hover:bg-secondary'}`}
                  >
                    <Plus size={18} />
                  </button>
                </div>

                {/* Add to Cart */}
                <Button
                  size="lg"
                  className="flex-1"
                  onClick={handleAddToCart}
                  disabled={product.stock === 0 || isAddingToCart}
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

            {/* Features */}
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 pt-4">
              {[
                { icon: Truck, title: t('product.freeShipping'), desc: t('product.freeShippingDesc') },
                { icon: Shield, title: t('product.warranty'), desc: t('product.warrantyDesc') },
                { icon: RefreshCw, title: t('product.returns'), desc: t('product.returnsDesc') },
              ].map((feature, i) => (
                <div key={i} className="flex items-start gap-3 p-3 bg-secondary rounded-lg">
                  <feature.icon size={20} className="text-primary flex-shrink-0 mt-0.5" />
                  <div>
                    <p className="font-medium text-sm">{feature.title}</p>
                    <p className="text-xs text-tertiary">{feature.desc}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Tabs */}
        <div className="bg-primary rounded-xl p-6 mb-12">
          <div className="flex border-b mb-6 overflow-x-auto">
            {(['description', 'specs', 'reviews'] as const).map((tab) => (
              <button
                key={tab}
                onClick={() => setActiveTab(tab)}
                className={`px-6 py-3 font-medium whitespace-nowrap transition-colors ${activeTab === tab
                    ? 'text-primary border-b-2 border-primary'
                    : 'text-secondary hover:text-primary'
                  }`}
              >
                {t(`product.tab.${tab}`)}
              </button>
            ))}
          </div>

          {/* Tab Content */}
          {activeTab === 'description' && (
            <div className="prose max-w-none">
              <p className="text-secondary leading-relaxed">{product.description}</p>
            </div>
          )}

          {activeTab === 'specs' && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {Object.entries(product.specifications || { Thương_hiệu: product.brand, Danh_mục: product.category }).map(
                ([key, value]) => (
                  <div key={key} className="flex border-b py-2">
                    <span className="w-1/3 font-medium text-secondary">{key.replace(/_/g, ' ')}</span>
                    <span className="flex-1 text-primary">{value as string}</span>
                  </div>
                )
              )}
            </div>
          )}

          {activeTab === 'reviews' && (
            <div className="space-y-6">
              {reviews.length > 0 ? (
                reviews.map((review) => (
                  <div key={review.code} className="border-b pb-6 last:border-0">
                    <div className="flex items-center gap-3 mb-2">
                      <div className="w-10 h-10 rounded-full bg-gradient-to-r from-primary to-purple-500 flex items-center justify-center text-white font-bold">
                        {review.userName?.charAt(0) || 'U'}
                      </div>
                      <div>
                        <p className="font-medium">{review.userName || 'Người dùng ẩn danh'}</p>
                        <div className="flex gap-1">
                          {Array.from({ length: 5 }).map((_, i) => (
                            <Star
                              key={i}
                              size={12}
                              className={i < review.rating ? 'fill-yellow-400 text-yellow-400' : 'text-gray-300'}
                            />
                          ))}
                        </div>
                      </div>
                    </div>
                    <p className="text-secondary">{review.comment}</p>
                  </div>
                ))
              ) : (
                <p className="text-center text-tertiary py-8">{t('product.noReviews')}</p>
              )}
            </div>
          )}
        </div>

        {/* Related Products */}
        {relatedProducts.length > 0 && (
          <div>
            <h2 className="text-2xl font-bold mb-6">{t('product.relatedProducts')}</h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
              {relatedProducts.map((p) => (
                <ProductCard key={p.code} product={p} />
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ProductDetailPage;
