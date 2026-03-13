import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  ChevronRight,
  Loader2,
  AlertCircle,
} from 'lucide-react';
import { Button } from '@/components/ui';
import { ProductCard } from '@/components/common/ProductCard';
import { RecentlyViewed } from '@/components/common/RecentlyViewed';
import { ProductQA } from './components/ProductQA';
import { StickyCartBar } from '@/components/common/StickyCartBar';
import { UpsellSection } from '@/components/common';

import { ImageGallery } from '@/components/common/ImageGallery';
import { ProductInfo } from './components/ProductInfo';
import { ProductTabs } from './components/ProductTabs';
import { useSEO, useProductSchema, useBreadcrumbSchema } from '@/hooks/useSEO';
import { useProductDetail } from './hooks/useProductDetail';

// ============ Product Detail Page ============
export const ProductDetailPage = () => {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  
  const {
      product,
      isLoading,
      isError,
      error,
      quantity,
      setQuantity,
      isAddingToCart,
      activeTab,
      setActiveTab,
      selectedIndex,
      setSelectedIndex,
      lightboxOpen,
      setLightboxOpen,
      relatedProducts,
      relations,
      isWishlisted,
      isCompared,
      hasFixedPrice,
      images,
      stockStatus,
      handleAddToCart,
      handleBuyNow,
      handleWishlistToggle,
      handleCompareToggle
  } = useProductDetail(id);

  // SEO — Dynamic title, description, Open Graph, and Product Schema
  useSEO({
    title: product?.name || 'Chi tiết sản phẩm',
    description: product?.description?.substring(0, 160) || `Mua ${product?.name || 'sản phẩm'} chính hãng tại VNVT Store. Giá tốt, bảo hành, giao hàng toàn quốc.`,
    canonicalPath: product ? `/product/${id}` : undefined,
    ogImage: product?.image,
    ogType: 'product',
    keywords: product ? `${product.name}, ${product.brand || ''}, ${product.category || ''}, VNVT Store` : undefined,
  });
  useProductSchema(product ? {
    name: product.name,
    description: product.description,
    imageUrl: product.image,
    price: product.price,
    currency: 'VND',
    brand: product.brand,
    sku: product.code,
    rating: product.averageRating || product.rating,
    reviewCount: product.reviewCount,
    availability: (product.stock > 0 || (product.stockQuantity ?? 0) > 0) ? 'InStock' : 'OutOfStock',
  } : null);
  useBreadcrumbSchema(product ? [
    { name: 'Trang chủ', url: '/' },
    { name: 'Sản phẩm', url: '/products' },
    ...(product.category ? [{ name: product.category, url: `/products?category=${product.categoryCode}` }] : []),
    { name: product.name, url: `/product/${id}` },
  ] : []);



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
      <StickyCartBar 
        product={product}
        handleAddToCart={handleAddToCart}
        handleBuyNow={handleBuyNow}
        isAddingToCart={isAddingToCart}
        hasFixedPrice={hasFixedPrice}
      />
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
          <div className="lg:col-span-2 sticky top-24 self-start">
            <ImageGallery 
              images={images} 
              video={product.videoURL}
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
              handleBuyNow={handleBuyNow}
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
          <div className="mt-12">
            <h2 className="text-2xl font-bold mb-6">{t('product.relatedProducts')}</h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
              {relatedProducts.map((p) => (
                <ProductCard key={p.code} product={p} />
              ))}
            </div>
          </div>
        )}

        {/* Dynamic Upsell Section */}
        <UpsellSection currentProduct={product} />

        {/* Product Q&A */}
        <div id="product-qa" className="scroll-mt-28 mt-12">
            <ProductQA productCode={product.code} />
        </div>
      </div>
      
      {/* Recently Viewed - Fixed placement outside main container */}
      <RecentlyViewed />
    </div>
  );
};

export default ProductDetailPage;
