import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  ChevronRight,
  Search,
  Grid,
  List,
  AlertCircle
} from 'lucide-react';
import { ProductCard } from '@/components/common/ProductCard';
import { ProductSkeleton } from '@/components/common/ProductSkeleton';
import { Button } from '@/components/ui';
import { useDebounce, useProducts } from '@/hooks';

const SearchPage = () => {
  const { t } = useTranslation();
  const [searchParams, setSearchParams] = useSearchParams();
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 12;

  const searchQuery = searchParams.get('search') || '';
  const debouncedSearch = useDebounce(searchQuery, 300);

  const {
    data: productsData,
    isLoading,
    isError,
    error,
  } = useProducts({
    pageIndex: currentPage,
    pageSize,
    search: debouncedSearch || undefined,
  });

  const products = productsData?.products || [];

  return (
    <div className="min-h-screen bg-secondary">
      {/* Breadcrumb */}
      <div className="bg-primary border-b py-3">
        <div className="container mx-auto px-4 flex items-center gap-2 text-sm">
          <Link to="/" className="text-secondary hover:text-accent transition-colors">
            {t('common.home')}
          </Link>
          <ChevronRight size={14} className="text-tertiary" />
          <span className="text-primary font-medium">{t('common.search')}</span>
        </div>
      </div>

      <div className="container mx-auto px-4 py-8">
        <div className="flex flex-col gap-8">
          {/* Search Info & Bar */}
          <div className="bg-primary rounded-2xl p-6 border shadow-sm">
            <h1 className="text-2xl font-bold mb-6 flex items-center gap-3">
              <Search className="text-accent-primary" />
              {searchQuery ? (
                <span>{t('search.resultsFor') || 'Search results for'}: <span className="text-accent-primary">&quot;{searchQuery}&quot;</span></span>
              ) : (
                <span>{t('header.searchPlaceholder')}</span>
              )}
            </h1>

            <div className="flex flex-col md:flex-row gap-4 items-center">
              <div className="relative flex-1 w-full">
                <Search size={20} className="absolute left-4 top-1/2 -translate-y-1/2 text-tertiary" />
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => {
                    const params = new URLSearchParams(searchParams);
                    if (e.target.value) params.set('search', e.target.value);
                    else params.delete('search');
                    setSearchParams(params);
                  }}
                  placeholder={t('header.searchPlaceholder')}
                  className="w-full pl-12 pr-4 py-3 bg-secondary border border-transparent focus:border-indigo-500 rounded-xl outline-none transition-all"
                />
              </div>
              <div className="flex border rounded-xl overflow-hidden shrink-0">
                <button
                  onClick={() => setViewMode('grid')}
                  className={`p-3 transition-colors ${viewMode === 'grid' ? 'bg-indigo-600 text-white' : 'bg-primary hover:bg-secondary'}`}
                >
                  <Grid size={20} />
                </button>
                <button
                  onClick={() => setViewMode('list')}
                  className={`p-3 transition-colors ${viewMode === 'list' ? 'bg-indigo-600 text-white' : 'bg-primary hover:bg-secondary'}`}
                >
                  <List size={20} />
                </button>
              </div>
            </div>
          </div>

          {/* Results Area */}
          <div>
            {isLoading ? (
              <div className={`grid gap-6 ${viewMode === 'list' ? 'grid-cols-1' : 'grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4'}`}>
                {Array.from({ length: 8 }).map((_, i) => (
                  <ProductSkeleton key={i} />
                ))}
              </div>
            ) : isError ? (
              <div className="text-center py-20 bg-primary rounded-2xl border">
                <AlertCircle className="w-16 h-16 mx-auto mb-4 text-red-500" />
                <h2 className="text-xl font-bold mb-2">{t('common.errorOccurred')}</h2>
                <p className="text-secondary mb-6">{error instanceof Error ? error.message : t('common.loadError')}</p>
                <Button onClick={() => window.location.reload()}>{t('common.retry')}</Button>
              </div>
            ) : products.length > 0 ? (
              <div className={`grid gap-6 ${viewMode === 'list' ? 'grid-cols-1' : 'grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4'}`}>
                {products.map((product) => (
                  <ProductCard key={product.code} product={product} variant={viewMode === 'list' ? 'list' : 'grid'} />
                ))}
              </div>
            ) : (
              <div className="text-center py-20 bg-primary rounded-2xl border shadow-sm">
                <div className="text-7xl mb-6">🔍</div>
                <h2 className="text-2xl font-bold mb-2">{t('common.noResults')}</h2>
                <p className="text-secondary max-w-md mx-auto">
                  {t('common.noResultsHint') || "We couldn't find any products matching your search query. Please try different keywords."}
                </p>
                <Button variant="outline" className="mt-8" onClick={() => setSearchParams({})}>
                  {t('filter.clearAll')}
                </Button>
              </div>
            )}

            {/* Pagination */}
            {!isLoading && productsData && productsData.totalPages > 1 && (
              <div className="flex justify-center gap-2 mt-12">
                <button
                  onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                  disabled={!productsData.hasPreviousPage}
                  className="w-12 h-12 flex items-center justify-center border rounded-xl bg-primary text-secondary disabled:opacity-50"
                >
                  ‹
                </button>
                {Array.from({ length: productsData.totalPages }, (_, i) => i + 1).map((pageNum) => (
                  <button
                    key={pageNum}
                    onClick={() => setCurrentPage(pageNum)}
                    className={`w-12 h-12 flex items-center justify-center border rounded-xl font-bold transition-all ${currentPage === pageNum ? 'bg-indigo-600 text-white border-indigo-600 shadow-lg scale-110' : 'bg-primary hover:border-indigo-400'}`}
                  >
                    {pageNum}
                  </button>
                ))}
                <button
                  onClick={() => setCurrentPage((p) => Math.min(productsData.totalPages, p + 1))}
                  disabled={!productsData.hasNextPage}
                  className="w-12 h-12 flex items-center justify-center border rounded-xl bg-primary text-secondary disabled:opacity-50"
                >
                  ›
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default SearchPage;
