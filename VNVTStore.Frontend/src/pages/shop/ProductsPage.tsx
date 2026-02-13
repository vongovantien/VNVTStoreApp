import { useState, useMemo, useCallback, useEffect, memo } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import {
  Grid,
  List,
  SlidersHorizontal,
  ChevronRight,
  Star,
  AlertCircle,
  Search,
  X,
  Heart,
  ShoppingCart,
  History,
  Scale,
  CheckSquare,
  Package,
  Mic,
  Camera,
  ArrowUp,
  Globe,
  ChevronDown,
  HelpCircle,
  Sparkles,
} from 'lucide-react';
import { ProductCard } from '@/components/common/ProductCard';
import { ProductSkeleton } from '@/components/common/ProductSkeleton';
import { Button, Badge } from '@/components/ui';
import { useDebounce, useProducts, useInfiniteProducts, useCategories, useIntersectionObserver } from '@/hooks';
import { cn } from '@/utils/cn';
import { useCompareStore, useCartStore, useWishlistStore, useToast, useUIStore } from '@/store';
import { useRecentStore } from '@/store/recentStore';
import { CompareTable } from '@/components/shop/CompareTable';
import { formatCurrency } from '@/utils/format';
import CustomImage from '@/components/common/Image';
import { useSEO } from '@/hooks/useSEO';

import type { CategoryDto } from '@/services/productService';

// ============ Filter Sidebar Component ============
interface FilterSidebarProps {
  categories: CategoryDto[];
  selectedCategories: string[];
  onCategoryToggle: (code: string) => void;
  selectedBrands: string[];
  onBrandToggle: (brand: string) => void;
  priceRange: [number, number];
  onPriceRangeChange: (range: [number, number]) => void;
  priceType: 'all' | 'fixed' | 'contact';
  onPriceTypeChange: (type: 'all' | 'fixed' | 'contact') => void;
  selectedRating: number | null;
  onRatingChange: (rating: number | null) => void;
  brands: string[];
  selectedAttributes: Record<string, string[]>;
  onAttributeToggle: (attr: string, value: string) => void;
  availableAttributes: { name: string; values: string[] }[];
  onClearAll: () => void;
  hasActiveFilters: boolean;
  /** Feature 4: In-Stock Only filter */
  inStockOnly: boolean;
  onInStockOnlyChange: (val: boolean) => void;
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
  selectedAttributes,
  onAttributeToggle,
  availableAttributes,
  onClearAll,
  hasActiveFilters,
  inStockOnly,
  onInStockOnlyChange,
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
            <div className="text-sm text-tertiary italic">{t('common.noCategories')}</div>
          )}
        </div>
      </div>

      {/* Feature 4: In-Stock Only Toggle */}
      <div className="mb-6">
        <div
          onClick={() => onInStockOnlyChange(!inStockOnly)}
          className="flex items-center justify-between cursor-pointer group p-2 rounded-lg hover:bg-hover transition-colors"
        >
          <div className="flex items-center gap-2">
            <Package size={16} className="text-emerald-600" />
            <span className="text-sm font-medium text-secondary group-hover:text-primary">
              {t('filter.inStockOnly', 'Chỉ còn hàng')}
            </span>
          </div>
          <div className={cn(
            'w-10 h-5 rounded-full relative transition-colors',
            inStockOnly ? 'bg-emerald-500' : 'bg-slate-200'
          )}>
            <span className={cn(
              'absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-all',
              inStockOnly ? 'left-5' : 'left-0.5'
            )} />
          </div>
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
        <div className="flex flex-wrap gap-2">
          {brands.map((brand) => (
            <button
              key={brand}
              onClick={() => onBrandToggle(brand)}
              className={cn(
                "px-3 py-1.5 text-xs border rounded-full transition-all active:scale-95",
                selectedBrands.includes(brand)
                  ? "bg-slate-900 text-white border-slate-900 shadow-sm"
                  : "border-slate-200 text-slate-600 hover:border-slate-900"
              )}
            >
              {brand}
            </button>
          ))}
        </div>
      </div>

      {/* Dynamic Attributes (Color, Size, Power, etc.) */}
      {availableAttributes.map((attr) => (
        <div key={attr.name} className="mb-6">
          <h4 className="text-sm font-semibold mb-3 flex justify-between items-center">
            {t(`product.${attr.name.toLowerCase()}`, attr.name)}
            {selectedAttributes[attr.name]?.length > 0 && (
              <span className="text-[10px] bg-indigo-100 text-indigo-600 px-1.5 py-0.5 rounded-full">
                {selectedAttributes[attr.name].length}
              </span>
            )}
          </h4>
          <div className="space-y-2 max-h-40 overflow-y-auto custom-scrollbar pr-1">
            {attr.values.map((val) => (
              <div
                key={val}
                onClick={() => onAttributeToggle(attr.name, val)}
                className="flex items-center gap-3 cursor-pointer group"
              >
                <div className={cn(
                  "w-4 h-4 rounded border flex items-center justify-center transition-all",
                  selectedAttributes[attr.name]?.includes(val)
                    ? "bg-slate-900 border-slate-900"
                    : "border-slate-200 group-hover:border-slate-400"
                )}>
                  {selectedAttributes[attr.name]?.includes(val) && (
                    <svg viewBox="0 0 12 9" className="w-2 h-2 text-white" fill="none" stroke="currentColor" strokeWidth="3">
                      <path d="M1 4L4.5 7.5L11 1" />
                    </svg>
                  )}
                </div>
                <span className={cn(
                  "text-sm transition-colors",
                  selectedAttributes[attr.name]?.includes(val) ? "text-slate-900 font-medium" : "text-slate-500 group-hover:text-slate-900"
                )}>
                  {val}
                </span>
              </div>
            ))}
          </div>
        </div>
      ))}

      {/* Rating */}
      <div>
        <h4 className="text-sm font-semibold mb-3">{t('filter.rating')}</h4>
        <div className="space-y-2">
          {[5, 4, 3, 2, 1].map((rating) => (
            <button
              key={rating}
              onClick={() => onRatingChange(selectedRating === rating ? null : rating)}
              className={cn(
                "flex items-center gap-2 w-full px-3 py-2 rounded-xl text-xs transition-all",
                selectedRating === rating 
                  ? "bg-slate-900 text-white shadow-md shadow-slate-200" 
                  : "bg-slate-50 text-slate-600 hover:bg-hover"
              )}
            >
              <div className="flex">
                {Array.from({ length: 5 }).map((_, i) => (
                  <Star
                    key={i}
                    size={12}
                    className={i < rating 
                      ? cn("fill-current", selectedRating === rating ? "text-yellow-400" : "text-amber-500") 
                      : cn(selectedRating === rating ? "text-slate-600" : "text-slate-200")}
                  />
                ))}
              </div>
              <span className="font-semibold">{rating === 5 ? t('filter.ratingTop', '5 sao') : t('filter.ratingUp', 'Trở lên')}</span>
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

  // SEO — dynamic title based on filters
  const categoryParam = searchParams.get('category');
  const searchParam = searchParams.get('search');
  const seoTitle = searchParam
    ? `Tìm kiếm "${searchParam}"`
    : categoryParam
    ? `Danh mục ${categoryParam}`
    : 'Tất cả sản phẩm';
  useSEO({
    title: seoTitle,
    description: 'Khám phá bộ sưu tập đồ gia dụng cao cấp tại VNVT Store. Lọc theo danh mục, thương hiệu, giá cả.',
    canonicalPath: '/products',
    keywords: 'sản phẩm, đồ gia dụng, thiết bị nhà bếp, mua sắm online, VNVT Store',
  });

  // Filters state
  const [priceRange, setPriceRange] = useState<[number, number]>([0, 100000000]); // Filter States
  const [selectedCategories, setSelectedCategories] = useState<string[]>(
    searchParams.get('category') ? searchParams.get('category')!.split(',') : []
  );
  const [selectedBrands, setSelectedBrands] = useState<string[]>([]);
  const [selectedRating, setSelectedRating] = useState<number | null>(null);
  const [priceType, setPriceType] = useState<'all' | 'fixed' | 'contact'>('all');
  const [selectedAttributes, setSelectedAttributes] = useState<Record<string, string[]>>({});
  const [inStockOnly, setInStockOnly] = useState(false);

  // Feature 7: Bulk Selection Mode
  const [bulkMode, setBulkMode] = useState(false);
  const [selectedCodes, setSelectedCodes] = useState<Set<string>>(new Set());
  const addToCart = useCartStore((state) => state.addItem);
  const toast = useToast();

  // Feature 6: Recently Viewed
  const [showRecent, setShowRecent] = useState(false);
  const viewedProducts = useRecentStore((s) => s.viewedProducts);

  // Feature 2: Floating Comparison Tray
  const compareItems = useCompareStore((s) => s.items);
  const isCompareOpen = useCompareStore((s) => s.isOpen);
  const setCompareOpen = useCompareStore((s) => s.setIsOpen);
  const removeFromCompare = useCompareStore((s) => s.removeItem);
  const clearCompare = useCompareStore((s) => s.clearCompare);
  const { addItem: addToWishlist } = useWishlistStore();

  // Feature 30: Dynamic Currency Converter
  const [currency, setCurrency] = useState<'VND' | 'USD'>('VND');
  const exchangeRate = 25000;

  const formatPrice = useCallback((price: number) => {
    if (currency === 'USD') {
      return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(price / exchangeRate);
    }
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }, [currency]);

  const handleAttributeToggle = useCallback((attr: string, value: string) => {
    setSelectedAttributes((prev) => {
      const current = prev[attr] || [];
      const next = current.includes(value)
        ? current.filter((v) => v !== value)
        : [...current, value];
      return { ...prev, [attr]: next };
    });
    setCurrentPage(1);
  }, []);

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

  // Fetch Products using Infinite Query
  const {
    data: infiniteData,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    isLoading,
    isError,
    error,
  } = useInfiniteProducts({
    pageSize,
    search: debouncedSearch || undefined,
    sortField: sortConfig.field,
    sortDir: sortConfig.dir,
    category: categorySlug,
    brands: selectedBrands,
    minPrice: priceRange[0],
    maxPrice: priceRange[1],
    rating: selectedRating || undefined,
    priceType: priceType,
    inStockOnly: inStockOnly,
  });

  const products = useMemo(() => {
    return infiniteData?.pages.flatMap((page) => page.products) || [];
  }, [infiniteData]);

  // Total items from first page info
  const totalItems = infiniteData?.pages[0]?.totalItems || 0;

  // Get unique brands from fetched products
  const brands = useMemo(() => {
    const brandSet = new Set(
      products.map((p) => p.brand).filter(Boolean)
    );
    return Array.from(brandSet) as string[];
  }, [products]);

  // Available attributes for filters
  const availableAttributes = useMemo(() => {
    const attrs: Record<string, Set<string>> = {
      'Color': new Set(),
      'Size': new Set(),
      'Power': new Set(),
      'Voltage': new Set(),
      'Material': new Set()
    };

    products.forEach(p => {
      if (p.color) attrs['Color'].add(p.color);
      if (p.size) attrs['Size'].add(p.size);
      if (p.power) attrs['Power'].add(p.power);
      if (p.voltage) attrs['Voltage'].add(p.voltage);
      if (p.material) attrs['Material'].add(p.material);
    });

    return Object.entries(attrs)
      .filter(([_, values]) => values.size > 0)
      .map(([name, values]) => ({
        name,
        values: Array.from(values).sort()
      }));
  }, [products]);

  // Apply client-side logic for features NOT supported by API yet
  // Currently: None (if Generic Search handles everything via 'In' or property checks)
  // However, since Brand/Rating are mocked on FE, passing them to API might return empty or error if fields don't exist.
  // QueryHelper uses reflection. If 'Brand' doesn't exist on TblProduct, it skips. UseProducts will send it.
  // Result: API returns ALL products (filter ignored). FE must filter client-side for these mocked fields.
  const filteredProducts = useMemo(() => {
    let result = [...products];

    // Client-side filtering for Multi-Category (if passed as separate params or single param in URL)
    // Controller logic handles "category" param. 
    // If selectedCategories has content, it overrides categorySlug.
    // So API receives it.
    // But we might have latency or mismatch.
    // Actually, if we trust API, we don't need this.
    // result = result.filter((p) => selectedCategories.includes(p.categoryId));

    // Client-side Brand (Mock Validation)
    if (selectedBrands.length > 0) {
       // Since backend might ignore 'Brand' if missing, we filter here to be safe for Mock UI
       result = result.filter((p) => p.brand && selectedBrands.includes(p.brand));
    }

    // Price is now Server-Side. Remove client filter.

    if (selectedRating) {
       // Since backend might ignore 'Rating', and data is mocked
       result = result.filter((p) => (p.rating || 0) >= selectedRating);
    }

    // Attribute filters
    Object.entries(selectedAttributes).forEach(([attr, values]) => {
      if (values.length > 0) {
        result = result.filter(p => {
          const productValue = (p as any)[attr.toLowerCase()];
          return productValue && values.includes(productValue);
        });
      }
    });

    // Client-side sorting for rating/bestseller (not supported by API)
    switch (sortBy) {
      case 'rating':
        result.sort((a, b) => (b.rating || 0) - (a.rating || 0));
        break;
      case 'bestseller':
        result.sort((a, b) => (b.reviewCount || 0) - (a.reviewCount || 0));
        break;
    }

    return result;
  }, [products, selectedBrands, selectedRating, sortBy, selectedAttributes]);

  // Feature 7: Bulk Selection Handlers
  const handleSelectToggle = useCallback((code: string) => {
    setSelectedCodes(prev => {
      const next = new Set(prev);
      if (next.has(code)) next.delete(code); else next.add(code);
      return next;
    });
  }, []);

  const handleBulkAddToCart = useCallback(async () => {
    const selectedProducts = products.filter(p => selectedCodes.has(p.code) && p.price > 0);
    let added = 0;
    for (const p of selectedProducts) {
      try { await addToCart(p); added++; } catch { /* skip */ }
    }
    if (added > 0) {
      toast.success(`Đã thêm ${added} sản phẩm vào giỏ hàng`);
      setSelectedCodes(new Set());
      setBulkMode(false);
    }
  }, [products, selectedCodes, addToCart, toast]);

  const handleBulkAddToWishlist = useCallback(() => {
    selectedCodes.forEach(code => {
      const prod = products.find(p => p.code === code);
      if (prod) addToWishlist(prod);
    });
    toast.success(`Đã thêm ${selectedCodes.size} sản phẩm vào danh sách yêu thích`);
    setSelectedCodes(new Set());
    setBulkMode(false);
  }, [selectedCodes, products, addToWishlist, toast]);

  const [observerRef, isIntersecting] = useIntersectionObserver({
    rootMargin: '200px',
  });

  useEffect(() => {
    if (isIntersecting && hasNextPage && !isFetchingNextPage) {
      fetchNextPage();
    }
  }, [isIntersecting, hasNextPage, isFetchingNextPage, fetchNextPage]);

  const handleSelectAll = useCallback(() => {
    if (selectedCodes.size === filteredProducts.length) {
      setSelectedCodes(new Set());
    } else {
      setSelectedCodes(new Set(filteredProducts.map(p => p.code)));
    }
  }, [filteredProducts, selectedCodes.size]);

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
          params.set('category', next.join(',')); // Join with comma for multiple selection
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
    setSelectedAttributes({});
    setInStockOnly(false);
    setSearchParams({});
  }, [setSearchParams]);

  const hasActiveFilters =
    selectedCategories.length > 0 ||
    selectedBrands.length > 0 ||
    selectedRating !== null ||
    priceType !== 'all' ||
    priceRange[0] > 0 ||
    priceRange[1] < 100000000 ||
    inStockOnly;

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
                  selectedAttributes={selectedAttributes}
                  onAttributeToggle={handleAttributeToggle}
                  availableAttributes={availableAttributes}
                  onClearAll={clearFilters}
                  hasActiveFilters={hasActiveFilters}
                  inStockOnly={inStockOnly}
                  onInStockOnlyChange={setInStockOnly}
                />
              </motion.div>
            )}
          </AnimatePresence>

          {/* Products */}
          <div className="flex-1 min-w-0">
            {/* Toolbar */}
            <div className="flex flex-wrap items-center justify-between gap-4 p-4 bg-primary rounded-xl mb-6 border shadow-sm">
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
                    className="w-full pl-10 pr-20 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 bg-secondary"
                  />
                  <div className="absolute right-2 top-1/2 -translate-y-1/2 flex items-center gap-1">
                    <button 
                      className="p-1.5 text-secondary hover:text-indigo-600 hover:bg-indigo-50 rounded-md transition-colors"
                      title="Tìm kiếm bằng giọng nói"
                      onClick={() => toast.info("Tính năng tìm kiếm bằng giọng nói đang được phát triển")}
                    >
                      <Mic size={16} />
                    </button>
                    <button 
                      className="p-1.5 text-secondary hover:text-indigo-600 hover:bg-indigo-50 rounded-md transition-colors"
                      title="Tìm kiếm bằng hình ảnh"
                      onClick={() => toast.info("Tính năng tìm kiếm bằng hình ảnh đang được phát triển")}
                    >
                      <Camera size={16} />
                    </button>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <span className="text-xs text-secondary font-medium uppercase tracking-wider">Tiền tệ:</span>
                  <div className="flex bg-secondary rounded-lg p-0.5 border">
                    <button
                      onClick={() => setCurrency('VND')}
                      className={cn(
                        "px-2 py-1 text-[10px] font-bold rounded-md transition-all",
                        currency === 'VND' ? "bg-white shadow-sm text-indigo-600" : "text-slate-400"
                      )}
                    >
                      VND
                    </button>
                    <button
                      onClick={() => setCurrency('USD')}
                      className={cn(
                        "px-2 py-1 text-[10px] font-bold rounded-md transition-all",
                        currency === 'USD' ? "bg-white shadow-sm text-indigo-600" : "text-slate-400"
                      )}
                    >
                      USD
                    </button>
                  </div>
                </div>

                <span className="text-sm text-secondary">
                  {t('filter.showResults', { count: products.length })}
                </span>

                {/* Feature 7: Bulk Mode Toggle */}
                <Button
                  variant={bulkMode ? 'primary' : 'ghost'}
                  size="sm"
                  onClick={() => { setBulkMode(!bulkMode); setSelectedCodes(new Set()); }}
                  leftIcon={<CheckSquare size={16} />}
                  className="ml-2"
                >
                  {bulkMode ? t('common.cancel', 'Hủy') : t('product.bulkSelect', 'Chọn nhiều')}
                </Button>
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
                    className={`p-2 transition-colors ${viewMode === 'grid' ? 'bg-accent-primary text-white' : 'hover:bg-hover'
                       }`}
                  >
                    <Grid size={18} />
                  </button>
                  <button
                    onClick={() => setViewMode('list')}
                    className={`p-2 transition-colors ${viewMode === 'list' ? 'bg-accent-primary text-white' : 'hover:bg-hover'
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

            {/* Feature 27: Related Category Chips */}
            <div className="flex flex-wrap items-center gap-3 mb-6">
              <span className="text-sm font-medium text-secondary whitespace-nowrap">Gợi ý:</span>
              {categories.slice(0, 8).map(cat => (
                <button
                  key={cat.code}
                  onClick={() => handleCategoryToggle(cat.code)}
                  className={cn(
                    "px-4 py-1.5 rounded-full text-sm font-medium transition-all whitespace-nowrap border",
                    selectedCategories.includes(cat.code)
                      ? "bg-indigo-600 text-white border-indigo-600 shadow-sm"
                      : "bg-white text-secondary hover:bg-indigo-50 hover:border-indigo-300 border-gray-200"
                  )}
                >
                  {cat.name}
                </button>
              ))}
            </div>

            {/* Products Grid */}
            {isLoading ? (
              <div
                className={`grid gap-6 ${viewMode === 'list'
                  ? 'grid-cols-1'
                  : 'grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-4'
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
                <h3 className="text-xl font-semibold mb-2">{t('common.errorOccurred')}</h3>
                <p className="text-secondary mb-6">
                  {error instanceof Error ? error.message : t('common.loadError')}
                </p>
                <Button onClick={() => window.location.reload()}>{t('common.retry')}</Button>
              </div>
            ) : products.length > 0 ? (
              <div
                className={`grid gap-6 ${viewMode === 'list'
                  ? 'grid-cols-1'
                  : 'grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-4'
                  }`}
              >
                {products.map((product) => (
                  <ProductCard
                    key={product.code}
                    product={product}
                    variant={viewMode === 'list' ? 'list' : 'grid'}
                    selectable={bulkMode}
                    selected={selectedCodes.has(product.code)}
                    onSelectToggle={handleSelectToggle}
                  />
                ))}  
              </div>
            ) : (
              <div className="text-center py-16 bg-primary rounded-xl">
                <div className="text-6xl mb-4">🔍</div>
                <h3 className="text-xl font-semibold mb-2">{t('common.noResults')}</h3>
                <p className="text-secondary mb-6">{t('common.noResultsHint')}</p>
                <Button onClick={clearFilters}>{t('filter.clearAll')}</Button>
              </div>
            )}

            {/* Infinite Scroll Loader */}
            <div ref={observerRef} className="py-10 flex justify-center">
               {isFetchingNextPage && (
                 <div className="flex flex-col items-center gap-2">
                    <div className="w-8 h-8 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin" />
                    <span className="text-sm text-secondary">Đang tải thêm sản phẩm...</span>
                 </div>
               )}
               {!hasNextPage && products.length > 0 && (
                 <p className="text-sm text-tertiary">Bạn đã xem hết {products.length} sản phẩm.</p>
               )}
            </div>
          </div>
        </div>
      </div>

      {/* Feature 6: Recently Viewed Drawer */}
      {viewedProducts.length > 0 && (
        <div className="bg-primary border-t">
          <div className="container mx-auto px-4">
            <button
              onClick={() => setShowRecent(!showRecent)}
              className="w-full flex items-center justify-between py-3 text-sm font-medium text-secondary hover:text-primary transition-colors"
            >
              <span className="flex items-center gap-2">
                <History size={16} />
                {t('product.recentlyViewed', 'Đã xem gần đây')} ({viewedProducts.length})
              </span>
              <ChevronDown size={16} className={cn('transition-transform', showRecent && 'rotate-180')} />
            </button>
            <AnimatePresence>
              {showRecent && (
                <motion.div
                  initial={{ height: 0, opacity: 0 }}
                  animate={{ height: 'auto', opacity: 1 }}
                  exit={{ height: 0, opacity: 0 }}
                  className="overflow-hidden"
                >
                  <div className="flex gap-4 pb-4 overflow-x-auto custom-scrollbar">
                    {viewedProducts.map((p) => (
                      <Link
                        key={p.code}
                        to={`/product/${p.code}`}
                        className="flex-shrink-0 w-32 group"
                      >
                        <div className="aspect-square rounded-lg overflow-hidden bg-slate-50 mb-2">
                          <CustomImage
                            src={p.image}
                            alt={p.name}
                            className="w-full h-full object-cover group-hover:scale-105 transition-transform"
                            loading="lazy"
                          />
                        </div>
                        <p className="text-xs font-medium text-primary line-clamp-2 group-hover:text-indigo-600 transition-colors">
                          {p.name}
                        </p>
                        <p className="text-xs font-bold text-slate-700 mt-0.5">
                          {p.price > 0 ? formatCurrency(p.price) : t('product.contactForPrice', 'Liên hệ')}
                        </p>
                      </Link>
                    ))}
                  </div>
                </motion.div>
              )}
            </AnimatePresence>
          </div>
        </div>
      )}

      {/* Feature 2: Floating Comparison Tray */}
      <AnimatePresence>
        {compareItems.length > 0 && (
          <motion.div
            initial={{ y: 100, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            exit={{ y: 100, opacity: 0 }}
            className="fixed bottom-0 left-0 right-0 bg-white/95 backdrop-blur-lg border-t border-slate-200 shadow-[0_-8px_30px_rgb(0,0,0,0.1)] z-50"
          >
            <div className="container mx-auto px-4 py-3 flex items-center gap-4">
              <div className="flex items-center gap-2 text-sm font-medium text-slate-700 shrink-0">
                <Scale size={18} className="text-indigo-600" />
                So sánh ({compareItems.length}/3)
              </div>
              <div className="flex-1 flex gap-3 overflow-x-auto">
                {compareItems.map((item) => (
                  <div key={item.code} className="flex items-center gap-2 bg-slate-50 rounded-lg px-3 py-1.5 shrink-0">
                    <div className="w-8 h-8 rounded overflow-hidden bg-slate-100">
                      <CustomImage src={item.image} alt={item.name} className="w-full h-full object-cover" />
                    </div>
                    <span className="text-xs font-medium max-w-[120px] truncate">{item.name}</span>
                    <button
                      onClick={() => removeFromCompare(item.code)}
                      className="text-slate-400 hover:text-red-500 transition-colors"
                    >
                      <X size={14} />
                    </button>
                  </div>
                ))}
              </div>
              <div className="flex gap-2 shrink-0">
                <Button variant="ghost" size="sm" onClick={clearCompare}>
                  {t('common.clear', 'Xóa')}
                </Button>
                <Button
                  size="sm"
                  disabled={compareItems.length < 2}
                  onClick={() => { /* navigate to compare page or open modal */ }}
                  leftIcon={<Scale size={14} />}
                >
                  So sánh ngay
                </Button>
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Feature 7: Bulk Add to Cart Floating Bar */}
      <AnimatePresence>
        {bulkMode && selectedCodes.size > 0 && (
          <motion.div
            initial={{ y: 100, opacity: 0 }}
            animate={{ y: compareItems.length > 0 ? -64 : 0, opacity: 1 }}
            exit={{ y: 100, opacity: 0 }}
            className="fixed bottom-0 left-0 right-0 bg-indigo-600 text-white z-50 shadow-lg"
          >
            <div className="container mx-auto px-4 py-3 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <button
                  onClick={handleSelectAll}
                  className="text-sm font-medium underline underline-offset-2 hover:text-indigo-200"
                >
                  {selectedCodes.size === filteredProducts.length ? 'Bỏ chọn tất cả' : 'Chọn tất cả'}
                </button>
                <span className="text-sm opacity-80">
                  Đã chọn {selectedCodes.size} sản phẩm
                </span>
              </div>
              <div className="flex gap-2">
                <Button
                  onClick={handleBulkAddToWishlist}
                  className="bg-indigo-500 text-white hover:bg-indigo-400"
                  variant="outline"
                  leftIcon={<Heart size={16} />}
                >
                  Yêu thích
                </Button>
                <Button
                  onClick={handleBulkAddToCart}
                  className="bg-white text-indigo-600 hover:bg-indigo-50"
                  leftIcon={<ShoppingCart size={16} />}
                >
                  Mục tiêu {selectedCodes.size} sản phẩm
                </Button>
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Feature 26: Back to Top Progress Circle */}
      <BackToTop />

      {/* Feature 21: Mini-Comparison Bar (Floating) */}
      <AnimatePresence>
        {compareItems.length > 0 && !isCompareOpen && (
          <motion.div
            initial={{ y: 100 }}
            animate={{ y: 0 }}
            exit={{ y: 100 }}
            className="fixed bottom-0 inset-x-0 z-40 p-4 bg-white/80 backdrop-blur-md border-t shadow-lg flex items-center justify-center gap-6"
          >
            <div className="flex -space-x-3">
              {compareItems.map(p => (
                <div key={p.code} className="w-12 h-12 rounded-full border-2 border-white overflow-hidden bg-white shadow-sm ring-1 ring-slate-100">
                  <CustomImage src={p.image} alt={p.name} className="w-full h-full object-cover" />
                </div>
              ))}
            </div>
            <div className="text-sm font-medium">
              <span className="text-indigo-600 font-bold">{compareItems.length}</span> sản phẩm trong danh sách so sánh
            </div>
            <div className="flex gap-2">
              <Button size="sm" variant="ghost" onClick={clearCompare}>Xóa hết</Button>
              <Button size="sm" variant="primary" onClick={() => setCompareOpen(true)}>So sánh ngay</Button>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Feature 20: Full-Page Comparison Modal */}
      <AnimatePresence>
        {isCompareOpen && (
          <CompareTable
            items={compareItems}
            onRemove={removeFromCompare}
            onAddToCart={addToCart}
            onClose={() => setCompareOpen(false)}
          />
        )}
      </AnimatePresence>

      {/* Feature 22: Interactive FAQ Section */}
      <section className="mt-20 pt-12 border-t">
        <div className="max-w-4xl mx-auto px-4">
          <div className="flex items-center gap-3 mb-8">
            <div className="p-2 bg-indigo-100 text-indigo-600 rounded-lg">
              <HelpCircle size={24} />
            </div>
            <div>
              <h2 className="text-2xl font-bold">Câu hỏi thường gặp</h2>
              <p className="text-secondary text-sm">Mọi thứ bạn cần biết về sản phẩm và dịch vụ của chúng tôi</p>
            </div>
          </div>
          <div className="space-y-4">
            <FAQItem 
              question="Chính sách bảo hành của VNVT Store như thế nào?" 
              answer="Tất cả sản phẩm chính hãng tại VNVT Store được bảo hành từ 12-24 tháng tùy dòng sản phẩm. Chúng tôi hỗ trợ 1 đổi 1 trong vòng 7 ngày nếu có lỗi từ nhà sản xuất."
            />
            <FAQItem 
              question="Tôi có thể kiểm tra hàng trước khi thanh toán không?" 
              answer="Có, VNVT Store khuyến khích khách hàng đồng kiểm cùng nhân viên giao hàng để đảm bảo sản phẩm đúng mẫu mã và không bị hư hỏng do vận chuyển."
            />
            <FAQItem 
              question="Làm sao để biết sản phẩm còn hàng?" 
              answer="Trạng thái tồn kho được cập nhật thời gian thực trên website. Nếu sản phẩm hiển thị 'Hết hàng', bạn có thể bật thông báo restock để nhận tin nhắn ngay khi hàng về."
            />
          </div>
        </div>
      </section>
    </div>
  );
};

const FAQItem = ({ question, answer }: { question: string, answer: string }) => {
  const [isOpen, setIsOpen] = useState(false);
  return (
    <div className="border rounded-xl bg-white overflow-hidden shadow-sm hover:border-indigo-200 transition-colors">
      <button 
        onClick={() => setIsOpen(!isOpen)}
        className="w-full p-4 flex justify-between items-center text-left font-medium text-slate-800"
      >
        <span>{question}</span>
        <ChevronDown size={18} className={cn("text-slate-400 transition-transform", isOpen && "rotate-180")} />
      </button>
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: "auto", opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            className="px-4 pb-4 text-sm text-slate-600 leading-relaxed"
          >
            {answer}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

const BackToTop = () => {
    const [isVisible, setIsVisible] = useState(false);
    const [progress, setProgress] = useState(0);

    useEffect(() => {
        const toggleVisibility = () => {
            const scrolled = window.scrollY;
            const threshold = 300;
            setIsVisible(scrolled > threshold);

            const height = document.documentElement.scrollHeight - window.innerHeight;
            if (height > 0) {
                setProgress((scrolled / height) * 100);
            }
        };

        window.addEventListener('scroll', toggleVisibility);
        return () => window.removeEventListener('scroll', toggleVisibility);
    }, []);

    const scrollToTop = () => {
        window.scrollTo({
            top: 0,
            behavior: 'smooth',
        });
    };

    return (
        <AnimatePresence>
            {isVisible && (
                <motion.button
                    initial={{ opacity: 0, scale: 0.5 }}
                    animate={{ opacity: 1, scale: 1 }}
                    exit={{ opacity: 0, scale: 0.5 }}
                    onClick={scrollToTop}
                    className="fixed bottom-8 right-8 z-50 p-0 w-12 h-12 bg-white rounded-full shadow-lg border border-gray-100 flex items-center justify-center group"
                >
                    <svg className="w-12 h-12 -rotate-90">
                        <circle
                            cx="24"
                            cy="24"
                            r="20"
                            stroke="currentColor"
                            strokeWidth="2.5"
                            fill="transparent"
                            className="text-gray-100"
                        />
                        <circle
                            cx="24"
                            cy="24"
                            r="20"
                            stroke="currentColor"
                            strokeWidth="2.5"
                            fill="transparent"
                            strokeDasharray={125.6}
                            strokeDashoffset={125.6 - (progress / 100) * 125.6}
                            className="text-indigo-600 transition-all duration-300"
                        />
                    </svg>
                    <div className="absolute inset-0 flex items-center justify-center text-secondary group-hover:text-indigo-600 transition-colors">
                        <ArrowUp size={20} />
                    </div>
                </motion.button>
            )}
        </AnimatePresence>
    );
};

export default ProductsPage;
