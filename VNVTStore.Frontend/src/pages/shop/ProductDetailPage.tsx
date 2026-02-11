import { useState, useMemo, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ProductDetailType } from '@/types';
import { useTranslation } from 'react-i18next';
import {
  ChevronRight,
  Loader2,
  AlertCircle,
} from 'lucide-react';
import { Button } from '@/components/ui';
import { ProductCard } from '@/components/common/ProductCard';
import { useCartStore, useWishlistStore, useCompareStore, useToast } from '@/store';
import { useProduct, useProducts } from '@/hooks';

import { ImageGallery } from '@/components/common/ImageGallery';
import { ProductInfo } from './components/ProductInfo';
import { ProductTabs } from './components/ProductTabs';

// ============ Product Detail Page ============
export const ProductDetailPage = () => {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  
  const [quantity, setQuantity] = useState(1);
  const [isAddingToCart, setIsAddingToCart] = useState(false); // New state
  const [activeTab, setActiveTab] = useState<'description' | 'specs' | 'units' | 'variants' | 'images' | 'reviews'>('description');
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [lightboxOpen, setLightboxOpen] = useState(false);

  // Store actions
  const addToCart = useCartStore((state) => state.addItem);
  const { addItem: addToWishlist, removeItem: removeFromWishlist, isInWishlist } = useWishlistStore();
  const { addItem: addToCompare, isInCompare } = useCompareStore();
  const { success, error: toastError } = useToast();

  // Fetch product from API
  const { data: product, isLoading, isError, error } = useProduct(id || '');


  // Fetch related products (same category)
  const { data: relatedData } = useProducts({
    pageIndex: 1,
    pageSize: 8, // Fetch more for better matching
    enabled: !!product?.categoryCode,
  });

  const relatedProducts = useMemo(
    () => (relatedData?.products || []).filter((p) => p.code !== id).slice(0, 4),
    [relatedData?.products, id]
  );

  // Group: RELATION for accessories/cross-selling
  const relations = useMemo(() => 
    product?.details?.filter(d => d.detailType === ProductDetailType.RELATION) || [],
    [product?.details]
  );

  // Derived states
  const isWishlisted = product ? isInWishlist(product.code) : false;
  const isCompared = product ? isInCompare(product.code) : false;
  const hasFixedPrice = product ? (product.price > 0) : false;
  const images = (product ? (product.images?.length ? product.images : (product.image ? [product.image] : [])) : []).filter(img => img && img.length > 0);

  const stockStatus = useMemo(() => {
    if (!product) return null;
    const stock = product.stock || 0;
    const minStock = product.minStockLevel || 5; // Default threshold if not set
    
    if (stock <= 0) return { label: t('product.outOfStock'), color: 'error' as const, text: 'text-red-600' };
    if (stock <= minStock) return { label: t('product.lowStock') || 'Sắp hết hàng', color: 'warning' as const, text: 'text-orange-600' };
    return { label: `${t('product.inStock')} (${stock})`, color: 'success' as const, text: 'text-green-600' };
  }, [product, t]);

  // Handlers
  const handleAddToCart = useCallback(async () => {
    if (product && hasFixedPrice) {
        setIsAddingToCart(true);
        try {
            void addToCart(product, quantity);
            success(t('product.addToCartSuccess') || 'Đã thêm vào giỏ hàng');
        } catch (err) {
            console.error(err);
            toastError(t('product.addToCartError') || 'Có lỗi xảy ra');
        } finally {
            setIsAddingToCart(false);
        }
    }
  }, [product, hasFixedPrice, quantity, addToCart, success, toastError, t]);

  const handleWishlistToggle = useCallback(() => {
    if (product) {
      if (isWishlisted) {
        removeFromWishlist(product.code);
      } else {
        addToWishlist(product);
      }
    }
  }, [product, isWishlisted, addToWishlist, removeFromWishlist]);

  const handleCompareToggle = useCallback(() => {
    if (product) {
      addToCompare(product);
    }
  }, [product, addToCompare]);



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
          <Link to={`/products?category=${product.categoryCode}`} className="text-secondary hover:text-primary">
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
            <ImageGallery 
              images={images} 
              altPrefix={product.name} 
              selectedIndex={selectedIndex}
              onSelectIndex={setSelectedIndex}
              lightboxOpen={lightboxOpen}
              setLightboxOpen={setLightboxOpen}
            />
          </div>

          {/* Product Info */}
          <div className="space-y-6 lg:col-span-3">
            <ProductInfo 
              product={product}
              hasFixedPrice={hasFixedPrice}
              stockStatus={stockStatus}
              quantity={quantity}
              setQuantity={setQuantity}
              handleAddToCart={handleAddToCart}
              isAddingToCart={isAddingToCart}
              isWishlisted={isWishlisted}
              handleWishlistToggle={handleWishlistToggle}
              isCompared={isCompared}
              handleCompareToggle={handleCompareToggle}
            />
          </div>
        </div>

        {/* Tabs */}
        <ProductTabs 
          product={product}
          activeTab={activeTab}
          setActiveTab={setActiveTab}
          images={images}
          relations={relations}
          setSelectedIndex={setSelectedIndex}
          setLightboxOpen={setLightboxOpen}
        />



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
