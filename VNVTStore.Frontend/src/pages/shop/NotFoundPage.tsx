/**
 * Feature #88: Custom 404 Page with Product Recommendations
 * Feature #5: "No Results" Recommendations
 */
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { Home, Search, ArrowRight } from 'lucide-react';
import { useProducts } from '@/hooks';
import { ProductCard } from '@/components/common/ProductCard';
import { ProductSkeleton } from '@/components/common/ProductSkeleton';

import { useSEO } from '@/hooks/useSEO';

export const NotFoundPage = () => {
  const { t } = useTranslation();
  
  useSEO({
    title: 'Trang không tồn tại',
    noindex: true,
  });
  
  // Fetch some popular products as recommendations
  const { data: productsData, isLoading } = useProducts({
    pageIndex: 1,
    pageSize: 4,
    sortField: 'createdAt',
    sortDir: 'desc',
  });

  const products = productsData?.products || [];

  return (
    <div className="min-h-screen bg-secondary">
      <div className="container mx-auto px-4 py-16">
        {/* 404 Hero */}
        <div className="text-center mb-16">
          <div className="text-[120px] font-black bg-gradient-to-br from-indigo-500 via-purple-500 to-pink-500 bg-clip-text text-transparent leading-none mb-4 select-none">
            404
          </div>
          <h1 className="text-3xl font-bold mb-4">{t('notFound.title', 'Trang không tồn tại')}</h1>
          <p className="text-secondary max-w-md mx-auto mb-8">
            {t('notFound.desc', 'Xin lỗi, trang bạn đang tìm kiếm không tồn tại hoặc đã được di chuyển.')}
          </p>
          <div className="flex items-center justify-center gap-3">
            <Link 
              to="/" 
              className="flex items-center gap-2 px-6 py-3 bg-indigo-600 text-white rounded-xl hover:bg-indigo-700 transition-colors font-medium"
            >
              <Home size={18} /> {t('common.home', 'Trang chủ')}
            </Link>
            <Link 
              to="/products" 
              className="flex items-center gap-2 px-6 py-3 border rounded-xl hover:bg-hover transition-colors font-medium"
            >
              <Search size={18} /> {t('common.search', 'Tìm kiếm')}
            </Link>
          </div>
        </div>

        {/* Product Recommendations */}
        <div className="max-w-6xl mx-auto">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-xl font-bold">{t('notFound.recommendations', 'Có thể bạn quan tâm')}</h2>
            <Link to="/products" className="flex items-center gap-1 text-sm text-accent-primary hover:underline">
              {t('common.viewAll', 'Xem tất cả')} <ArrowRight size={14} />
            </Link>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
            {isLoading
              ? Array.from({ length: 4 }).map((_, i) => <ProductSkeleton key={i} />)
              : products.map((product) => (
                  <ProductCard key={product.code} product={product} />
                ))
            }
          </div>
        </div>
      </div>
    </div>
  );
};

export default NotFoundPage;
