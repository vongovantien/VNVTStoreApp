import { useState, useMemo, useCallback, memo } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import {
  Grid,
  List,
  SlidersHorizontal,
  ChevronDown,
  ChevronRight,
  Star,
  Loader2,
  AlertCircle,
  Search,
} from 'lucide-react';
import { ProductCard } from '@/components/common/ProductCard';
import { ProductSkeleton } from '@/components/common/ProductSkeleton';
import { Button, Badge } from '@/components/ui';
import { useDebounce, useProducts, useCategories } from '@/hooks';

import type { CategoryDto } from '@/services/productService';

// ============ Filter Sidebar Component ============
interface FilterSidebarProps {
  categories: CategoryDto[];
  selectedCategories: string[];
  onCategoryToggle: (id: string) => void;
  selectedBrands: string[];
  onBrandToggle: (brand: string) => void;
  priceRange: [number, number];
  onPriceRangeChange: (range: [number, number]) => void;
  priceType: 'all' | 'fixed' | 'contact';
  onPriceTypeChange: (type: 'all' | 'fixed' | 'contact') => void;
  selectedRating: number | null;
  onRatingChange: (rating: number | null) => void;
  brands: string[];
  onClearAll: () => void;
  hasActiveFilters: boolean;
}

const FilterSidebar = memo(({
  categories,
  selectedCategories,
  onCategoryToggle,
  selectedBrands,
  onBrandToggle,
  priceRange,
  onPriceRangeChange,
  priceType,
  onPriceTypeChange,
  selectedRating,
  onRatingChange,
  brands,
  onClearAll,
  hasActiveFilters,
}: FilterSidebarProps) => {
  const { t } = useTranslation();

  const priceShortcuts = [
    { label: t('filter.priceUnder1M'), range: [0, 1000000] as [number, number] },
    { label: t('filter.price1to5M'), range: [1000000, 5000000] as [number, number] },
    { label: t('filter.price5to10M'), range: [5000000, 10000000] as [number, number] },
    { label: t('filter.priceOver10M'), range: [10000000, 100000000] as [number, number] },
  ];

  return (
    <aside className="w-80 bg-primary rounded-xl p-4 h-fit sticky top-24 border shadow-sm z-10 shrink-0">
      <div className="flex justify-between items-center mb-4 pb-3 border-b">
        <h3 className="flex items-center gap-2 font-semibold text-primary">
          <SlidersHorizontal size={18} />
          {t('filter.title')}
        </h3>
        {hasActiveFilters && (
          <button onClick={onClearAll} className="text-sm text-error hover:underline">
            {t('filter.clearAll')}
          </button>
        )}
      </div>

      {/* Categories */}
      <div className="mb-6">
        <h4 className="text-sm font-semibold mb-3">{t('filter.category')}</h4>
        <div className="space-y-2 max-h-48 overflow-y-auto custom-scrollbar pr-1">
          {categories.map((cat) => (
            <div
              key={cat.code}
              onClick={() => onCategoryToggle(cat.code)}
              className="flex items-center gap-3 cursor-pointer group"
            >
              <span
                className={`w-4 h-4 rounded border-2 flex items-center justify-center transition-colors ${selectedCategories.includes(cat.code) ? 'border-primary bg-accent-primary' : 'border-tertiary'
                  }`}
              >
                {selectedCategories.includes(cat.code) && (
                  <svg viewBox="0 0 12 9" className="w-2.5 h-2.5 text-white" fill="none" stroke="currentColor" strokeWidth="2">
                    <path d="M1 4L4.5 7.5L11 1" />
                  </svg>
                )}
              </span>
              <span className="flex-1 text-sm text-secondary group-hover:text-primary transition-colors">
                {cat.name}
              </span>
              {/* Product count not available in API yet */}
              {/* <span className="text-xs text-tertiary bg-secondary px-1.5 py-0.5 rounded">
                {cat.productCount}
              </span> */}
            </div>
          ))}
          {categories.length === 0 && (
             <div className="text-sm text-tertiary italic">Chưa có danh mục</div>
          )}
        </div>
      </div>

      {/* Price Type */}
      <div className="mb-6">
        <h4 className="text-sm font-semibold mb-3">{t('filter.priceType')}</h4>
        <div className="space-y-2">
          {(['all', 'fixed', 'contact'] as const).map((type) => (
            <div
              key={type}
              onClick={() => onPriceTypeChange(type)}
              className="flex items-center gap-3 cursor-pointer group"
            >
              <span className={`w-4 h-4 rounded-full border-2 flex items-center justify-center transition-colors ${priceType === type ? 'border-primary bg-accent-primary' : 'border-tertiary'
                }`}>
                {priceType === type && <span className="w-1.5 h-1.5 bg-white rounded-full" />}
              </span>
              <span className="text-sm text-secondary group-hover:text-primary transition-colors">
                {type === 'all' ? t('common.all') : type === 'fixed' ? t('filter.fixedPrice') : t('filter.contactPrice')}
              </span>
            </div>
          ))}
        </div>
      </div>

      {/* Price Range */}
      <div className="mb-6">
        <h4 className="text-sm font-semibold mb-3">{t('filter.priceRange')}</h4>
        <div className="flex gap-2 mb-3">
          <input
            type="number"
            placeholder={t('filter.from')}
            value={priceRange[0] || ''}
            onChange={(e) => onPriceRangeChange([Number(e.target.value) || 0, priceRange[1]])}
            className="flex-1 w-full min-w-0 px-2 py-2 border rounded-lg text-sm text-center focus:outline-none focus:border-primary"
          />
          <span className="self-center text-tertiary">-</span>
          <input
            type="number"
            placeholder={t('filter.to')}
            value={priceRange[1] === 100000000 ? '' : priceRange[1]}
            onChange={(e) => onPriceRangeChange([priceRange[0], Number(e.target.value) || 100000000])}
            className="flex-1 w-full min-w-0 px-2 py-2 border rounded-lg text-sm text-center focus:outline-none focus:border-primary"
          />
        </div>
        <div className="flex flex-wrap gap-2">
          {priceShortcuts.map((s) => (
            <button
              key={s.label}
              onClick={() => onPriceRangeChange(s.range)}
              className={`px-2 py-1 text-xs border rounded-lg transition-colors ${priceRange[0] === s.range[0] && priceRange[1] === s.range[1]
                ? 'bg-accent-primary text-white border-primary'
                : 'border-tertiary text-secondary hover:border-primary'
                }`}
            >
              {s.label}
            </button>
          ))}
        </div>
      </div>

      {/* Brands */}
      <div className="mb-6">
        <h4 className="text-sm font-semibold mb-3">{t('filter.brand')}</h4>
        <div className="space-y-2">
          {brands.map((brand) => (
            <div
              key={brand}
              onClick={() => onBrandToggle(brand)}
              className="flex items-center gap-3 cursor-pointer group"
            >
              <span
                className={`w-4 h-4 rounded border-2 flex items-center justify-center transition-colors ${selectedBrands.includes(brand) ? 'border-primary bg-accent-primary' : 'border-tertiary'
                  }`}
              >
                {selectedBrands.includes(brand) && (
                  <svg viewBox="0 0 12 9" className="w-2.5 h-2.5 text-white" fill="none" stroke="currentColor" strokeWidth="2">
                    <path d="M1 4L4.5 7.5L11 1" />
                  </svg>
                )}
              </span>
              <span className="text-sm text-secondary group-hover:text-primary transition-colors">
                {brand}
              </span>
            </div>
          ))}
        </div>
      </div>

      {/* Rating */}
      <div>
        <h4 className="text-sm font-semibold mb-3">{t('filter.rating')}</h4>
        <div className="space-y-2">
          {[5, 4, 3, 2, 1].map((rating) => (
            <button
              key={rating}
              onClick={() => onRatingChange(selectedRating === rating ? null : rating)}
              className={`flex items-center gap-2 w-full px-2 py-1.5 rounded-lg text-xs transition-colors ${selectedRating === rating ? 'bg-indigo-50 dark:bg-indigo-900/30 text-accent' : 'hover:bg-tertiary'
                }`}
            >
              {Array.from({ length: 5 }).map((_, i) => (
                <Star
                  key={i}
                  size={12}
                  className={i < rating ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300'}
                />
              ))}
              <span className="text-secondary">{t('filter.ratingUp')}</span>
            </button>
          ))}
        </div>
      </div>
    </aside>
  );
});

FilterSidebar.displayName = 'FilterSidebar';

// ============ Products Page Component ============
export const ProductsPage = () => {
  const { t } = useTranslation();
  const [searchParams, setSearchParams] = useSearchParams();
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [showFilters, setShowFilters] = useState(true);
  const [sortBy, setSortBy] = useState('newest');
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 12;

  // Filters state
  const [priceRange, setPriceRange] = useState<[number, number]>([0, 100000000]); // Filter States
  const [selectedCategories, setSelectedCategories] = useState<string[]>(
    searchParams.get('category') ? [searchParams.get('category')!] : []
  );
  const [selectedBrands, setSelectedBrands] = useState<string[]>([]);
  const [selectedRating, setSelectedRating] = useState<number | null>(null);
  const [priceType, setPriceType] = useState<'all' | 'fixed' | 'contact'>('all');

  const searchQuery = searchParams.get('search') || '';
  const categorySlug = searchParams.get('category') || '';
  const debouncedSearch = useDebounce(searchQuery, 300);

  // Map sortBy to API format
  const getSortConfig = () => {
    switch (sortBy) {
      case 'price-asc':
        return { field: 'price', dir: 'asc' as const };
      case 'price-desc':
        return { field: 'price', dir: 'desc' as const };
      case 'newest':
      default:
        return { field: 'createdAt', dir: 'desc' as const };
    }
  };

  // Fetch products from API
  const sortConfig = getSortConfig();
  // Fetch Categories
  const { data: categories = [] } = useCategories();

  // Fetch Products
  const {
    data: productsData,
    isLoading,
    isError,
    error,
  } = useProducts({
    pageIndex: currentPage,
    pageSize,
    search: debouncedSearch || undefined,
    sortField: sortConfig.field,
    sortDir: sortConfig.dir,
    category: categorySlug,
  });

  // Get unique brands from fetched products
  const brands = useMemo(() => {
    const brandSet = new Set(
      (productsData?.products || []).map((p) => p.brand).filter(Boolean)
    );
    return Array.from(brandSet) as string[];
  }, [productsData?.products]);

  // Apply client-side filters (price, category, brand, rating)
  const filteredProducts = useMemo(() => {
    let products = [...(productsData?.products || [])];

    if (categorySlug) {
      products = products.filter((p) => p.categoryId === categorySlug);
    }

    if (selectedCategories.length > 0) {
      products = products.filter((p) => selectedCategories.includes(p.categoryId));
    }

    if (selectedBrands.length > 0) {
      products = products.filter((p) => p.brand && selectedBrands.includes(p.brand));
    }

    products = products.filter(
      (p) => p.price >= priceRange[0] && (p.price <= priceRange[1] || p.price === 0)
    );

    if (priceType === 'fixed') {
      products = products.filter((p) => p.price > 0);
    } else if (priceType === 'contact') {
      products = products.filter((p) => p.price === 0);
    }

    if (selectedRating) {
      products = products.filter((p) => p.rating >= selectedRating);
    }

    // Client-side sorting for rating/bestseller (not supported by API)
    switch (sortBy) {
      case 'rating':
        products.sort((a, b) => b.rating - a.rating);
        break;
      case 'bestseller':
        products.sort((a, b) => b.reviewCount - a.reviewCount);
        break;
    }

    return products;
  }, [productsData?.products, categorySlug, selectedCategories, selectedBrands, priceRange, priceType, selectedRating, sortBy]);

  // Handlers
  const handleCategoryToggle = useCallback((id: string) => {
    setSelectedCategories((prev) => {
        const next = prev.includes(id)
            ? prev.filter((c) => c !== id)
            : [...prev, id];
        
        // Optional: Update URL to reflect single category selection (common pattern)
        // If we want multi-select in URL, we'd join them. For now let's assume single or primary category in URL
        if (next.length > 0) {
            setSearchParams(params => {
                params.set('category', next[next.length -1]); // Set the most recently selected
                return params;
            });
        } else {
             setSearchParams(params => {
                params.delete('category');
                return params;
            });
        }
        return next;
    });
    setCurrentPage(1);
  }, [setSearchParams]);

  const handleBrandToggle = useCallback((brand: string) => {
    setSelectedBrands((prev) =>
      prev.includes(brand) ? prev.filter((b) => b !== brand) : [...prev, brand]
    );
  }, []);

  const clearFilters = useCallback(() => {
    setSelectedCategories([]);
    setSelectedBrands([]);
    setPriceRange([0, 100000000]);
    setSelectedRating(null);
    setPriceType('all');
    setSearchParams({});
  }, [setSearchParams]);

  const hasActiveFilters =
    selectedCategories.length > 0 ||
    selectedBrands.length > 0 ||
    selectedRating !== null ||
    priceType !== 'all' ||
    priceRange[0] > 0 ||
    priceRange[1] < 100000000;

  return (
    <div className="min-h-screen bg-secondary">
      {/* Breadcrumb */}
      <div className="bg-primary border-b py-3">
        <div className="container mx-auto px-4 flex items-center gap-2 text-sm">
          <Link to="/" className="text-secondary hover:text-accent transition-colors">
            {t('common.home')}
          </Link>
          <ChevronRight size={14} className="text-tertiary" />
          <span className="text-primary font-medium">{t('common.products')}</span>
          {categorySlug && (
            <>
              <ChevronRight size={14} className="text-tertiary" />
              <span className="text-primary font-medium">
                {categories.find((c) => c.code === categorySlug)?.name || categorySlug}
              </span>
            </>
          )}
        </div>
      </div>

      <div className="container mx-auto px-4 py-6">
        <div className="flex gap-6">
          {/* Filters */}
          <AnimatePresence>
            {showFilters && (
              <motion.div
                initial={{ x: -20, opacity: 0 }}
                animate={{ x: 0, opacity: 1 }}
                exit={{ x: -20, opacity: 0 }}
                className="hidden lg:block"
              >
                <FilterSidebar
                  categories={categories}
                  selectedCategories={selectedCategories}
                  onCategoryToggle={handleCategoryToggle}
                  selectedBrands={selectedBrands}
                  onBrandToggle={handleBrandToggle}
                  priceRange={priceRange}
                  onPriceRangeChange={setPriceRange}
                  priceType={priceType}
                  onPriceTypeChange={setPriceType}
                  selectedRating={selectedRating}
                  onRatingChange={setSelectedRating}
                  brands={brands}
                  onClearAll={clearFilters}
                  hasActiveFilters={hasActiveFilters}
                />
              </motion.div>
            )}
          </AnimatePresence>

          {/* Products */}
          <div className="flex-1">
            {/* Toolbar */}
            <div className="flex justify-between items-center p-4 bg-primary rounded-xl mb-6 border shadow-sm">
              <div className="flex items-center gap-4">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setShowFilters(!showFilters)}
                  leftIcon={<SlidersHorizontal size={18} />}
                  className="hidden lg:flex"
                >
                  {showFilters ? t('filter.hideFilter') : t('filter.showFilter')}
                </Button>
                
                {/* Search Input */}
                <div className="relative flex-1 max-w-xs">
                  <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary" />
                  <input
                    type="text"
                    placeholder={t('common.search')}
                    value={searchParams.get('search') || ''}
                    onChange={(e) => {
                      const newParams = new URLSearchParams(searchParams);
                      if (e.target.value) {
                        newParams.set('search', e.target.value);
                      } else {
                        newParams.delete('search');
                      }
                      setSearchParams(newParams);
                    }}
                    className="w-full pl-10 pr-4 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 bg-secondary"
                  />
                </div>
                
                <span className="text-sm text-secondary">
                  {t('filter.showResults', { count: filteredProducts.length })}
                </span>
              </div>

              <div className="flex items-center gap-4">
                {/* Sort */}
                <div className="relative flex items-center gap-2">
                  <span className="text-sm text-secondary hidden sm:inline">{t('filter.sortBy')}:</span>
                  <select
                    value={sortBy}
                    onChange={(e) => setSortBy(e.target.value)}
                    className="appearance-none px-4 py-2 pr-8 border rounded-lg text-sm focus:outline-none focus:border-primary cursor-pointer"
                  >
                    <option value="newest">{t('filter.newest')}</option>
                    <option value="price-asc">{t('filter.priceAsc')}</option>
                    <option value="price-desc">{t('filter.priceDesc')}</option>
                    <option value="rating">{t('filter.topRated')}</option>
                    <option value="bestseller">{t('filter.bestSeller')}</option>
                  </select>
                  <ChevronDown size={16} className="absolute right-2 text-tertiary pointer-events-none" />
                </div>

                {/* View Toggle */}
                <div className="flex border rounded-lg overflow-hidden">
                  <button
                    onClick={() => setViewMode('grid')}
                    className={`p-2 transition-colors ${viewMode === 'grid' ? 'bg-accent-primary text-white' : 'hover:bg-tertiary'
                      }`}
                  >
                    <Grid size={18} />
                  </button>
                  <button
                    onClick={() => setViewMode('list')}
                    className={`p-2 transition-colors ${viewMode === 'list' ? 'bg-accent-primary text-white' : 'hover:bg-tertiary'
                      }`}
                  >
                    <List size={18} />
                  </button>
                </div>
              </div>
            </div>

            {/* Active Filters */}
            {hasActiveFilters && (
              <div className="flex flex-wrap gap-2 mb-6">
                {selectedCategories.map((catId) => {
                  const cat = categories.find((c) => c.code === catId);
                  return (
                    <Badge
                      key={catId}
                      color="primary"
                      variant="soft"
                      closable
                      onClose={() => handleCategoryToggle(catId)}
                    >
                      {cat?.name}
                    </Badge>
                  );
                })}
                {selectedBrands.map((brand) => (
                  <Badge
                    key={brand}
                    color="primary"
                    variant="soft"
                    closable
                    onClose={() => handleBrandToggle(brand)}
                  >
                    {brand}
                  </Badge>
                ))}
                {priceType !== 'all' && (
                  <Badge
                    color="primary"
                    variant="soft"
                    closable
                    onClose={() => setPriceType('all')}
                  >
                    {priceType === 'fixed' ? t('filter.fixedPrice') : t('filter.contactPrice')}
                  </Badge>
                )}
              </div>
            )}

            {/* Products Grid */}
            {isLoading ? (
              <div
                className={`grid gap-6 ${viewMode === 'list'
                  ? 'grid-cols-1'
                  : 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3'
                  }`}
              >
                {Array.from({ length: pageSize }).map((_, i) => (
                  <div key={i} className="h-[400px]">
                    <ProductSkeleton />
                  </div>
                ))}
              </div>
            ) : isError ? (
              <div className="text-center py-16 bg-primary rounded-xl">
                <AlertCircle className="w-12 h-12 mx-auto mb-4 text-red-500" />
                <h3 className="text-xl font-semibold mb-2">Có lỗi xảy ra</h3>
                <p className="text-secondary mb-6">
                  {error instanceof Error ? error.message : 'Không thể tải danh sách sản phẩm'}
                </p>
                <Button onClick={() => window.location.reload()}>Thử lại</Button>
              </div>
            ) : filteredProducts.length > 0 ? (
              <div
                className={`grid gap-6 ${viewMode === 'list'
                  ? 'grid-cols-1'
                  : 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3'
                  }`}
              >
                {filteredProducts.map((product) => (
                  <ProductCard
                    key={product.id}
                    product={product}
                    variant={viewMode === 'list' ? 'list' : 'grid'}
                  />
                ))}
              </div>
            ) : (
              <div className="text-center py-16 bg-primary rounded-xl">
                <div className="text-6xl mb-4">🔍</div>
                <h3 className="text-xl font-semibold mb-2">{t('common.noResults')}</h3>
                <p className="text-secondary mb-6">Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm</p>
                <Button onClick={clearFilters}>{t('filter.clearAll')}</Button>
              </div>
            )}

            {/* Pagination */}
            {!isLoading && productsData && productsData.totalPages > 1 && (
              <div className="flex justify-center gap-2 mt-8">
                <button
                  onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                  disabled={!productsData.hasPreviousPage}
                  className="w-10 h-10 flex items-center justify-center border rounded-lg text-secondary hover:border-primary disabled:opacity-50"
                >
                  ‹
                </button>
                {Array.from({ length: Math.min(5, productsData.totalPages) }, (_, i) => {
                  const pageNum = i + 1;
                  return (
                    <button
                      key={pageNum}
                      onClick={() => setCurrentPage(pageNum)}
                      className={`w-10 h-10 flex items-center justify-center border rounded-lg font-medium transition-colors ${currentPage === pageNum
                          ? 'bg-accent-primary text-white border-primary'
                          : 'hover:border-primary'
                        }`}
                    >
                      {pageNum}
                    </button>
                  );
                })}
                {productsData.totalPages > 5 && (
                  <>
                    <span className="w-10 h-10 flex items-center justify-center text-tertiary">...</span>
                    <button
                      onClick={() => setCurrentPage(productsData.totalPages)}
                      className={`w-10 h-10 flex items-center justify-center border rounded-lg ${currentPage === productsData.totalPages
                          ? 'bg-accent-primary text-white border-primary'
                          : 'hover:border-primary'
                        }`}
                    >
                      {productsData.totalPages}
                    </button>
                  </>
                )}
                <button
                  onClick={() => setCurrentPage((p) => Math.min(productsData.totalPages, p + 1))}
                  disabled={!productsData.hasNextPage}
                  className="w-10 h-10 flex items-center justify-center border rounded-lg text-secondary hover:border-primary disabled:opacity-50"
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

export default ProductsPage;
