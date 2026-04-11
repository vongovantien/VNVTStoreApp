/**
 * Feature #6: Search Autocomplete with Thumbnails
 * Feature #7: Recent Search History (localStorage)
 * Feature #8: Trending Search Terms
 */
import { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Search, Clock, TrendingUp, X, ArrowRight } from 'lucide-react';
import { useDebounce } from '@/hooks';
import { productService, type ProductDto } from '@/services/productService';
import CustomImage from '@/components/common/Image';

const RECENT_SEARCHES_KEY = 'vnvt_recent_searches';
const MAX_RECENT = 8;

interface SearchResult {
  code: string;
  name: string;
  imageUrl?: string;
  price?: number;
}

export const SearchAutocomplete = ({ 
  onClose,
  className = '' 
}: { 
  onClose?: () => void;
  className?: string;
}) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [query, setQuery] = useState('');
  const [trendingTerms, setTrendingTerms] = useState<string[]>([]);
  const [results, setResults] = useState<SearchResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [showDropdown, setShowDropdown] = useState(false);
  const [recentSearches, setRecentSearches] = useState<string[]>([]);
  const inputRef = useRef<HTMLInputElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const debouncedQuery = useDebounce(query, 300);

  // Load recent searches and trending terms
  useEffect(() => {
    try {
      const stored = localStorage.getItem(RECENT_SEARCHES_KEY);
      if (stored) setRecentSearches(JSON.parse(stored));
    } catch { /* ignore */ }

    // Fetch trending terms from backend
    const fetchTrending = async () => {
      try {
        const res = await productService.getTrending(8);
        if (res.success && res.data) {
          setTrendingTerms(res.data.map((p: ProductDto) => p.name));
        }
      } catch {
        // Fallback to minimal set if backend fails
        setTrendingTerms(['điện thoại', 'laptop', 'tai nghe', 'sạc dự phòng']);
      }
    };
    fetchTrending();
  }, []);

  // Save recent search
  const saveRecentSearch = useCallback((term: string) => {
    if (!term.trim()) return;
    setRecentSearches(prev => {
      const updated = [term, ...prev.filter(s => s !== term)].slice(0, MAX_RECENT);
      localStorage.setItem(RECENT_SEARCHES_KEY, JSON.stringify(updated));
      return updated;
    });
  }, []);

  // Clear recent searches
  const clearRecentSearches = useCallback(() => {
    setRecentSearches([]);
    localStorage.removeItem(RECENT_SEARCHES_KEY);
  }, []);

  // Remove single recent search
  const removeRecentSearch = useCallback((term: string) => {
    setRecentSearches(prev => {
      const updated = prev.filter(s => s !== term);
      localStorage.setItem(RECENT_SEARCHES_KEY, JSON.stringify(updated));
      return updated;
    });
  }, []);

  // Fetch search results with thumbnails
  useEffect(() => {
    if (!debouncedQuery || debouncedQuery.length < 2) {
      setResults([]);
      return;
    }

    const fetchResults = async () => {
      setIsLoading(true);
      try {
        const res = await productService.search({
          pageIndex: 1,
          pageSize: 6,
          search: debouncedQuery,
          searchField: 'Name',
          fields: ['Code', 'Name', 'Price'],
        });
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        setResults(res.data?.items?.map((p: any) => ({
          code: p.code,
          name: p.name,
          imageUrl: p.imageUrl,
          price: p.price,
        })) || []);
      } catch {
        setResults([]);
      } finally {
        setIsLoading(false);
      }
    };
    fetchResults();
  }, [debouncedQuery]);

  // Handle submit
  const handleSubmit = useCallback((term: string) => {
    if (!term.trim()) return;
    saveRecentSearch(term);
    navigate(`/products?search=${encodeURIComponent(term)}`);
    setShowDropdown(false);
    onClose?.();
  }, [navigate, saveRecentSearch, onClose]);

  // Click outside to close
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
        setShowDropdown(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const formatPrice = (price: number) => 
    new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);

  return (
    <div ref={dropdownRef} className={`relative ${className}`}>
      {/* Search Input */}
      <div className="relative">
        <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary" />
        <input
          ref={inputRef}
          type="text"
          value={query}
          onChange={(e) => { setQuery(e.target.value); setShowDropdown(true); }}
          onFocus={() => setShowDropdown(true)}
          onKeyDown={(e) => { if (e.key === 'Enter') handleSubmit(query); }}
          placeholder={t('header.searchPlaceholder', 'Tìm kiếm sản phẩm...')}
          className="w-full pl-10 pr-10 py-3 !bg-transparent !border-0 !shadow-none !focus:ring-0 !focus:border-0 outline-none transition-all text-sm"
          autoComplete="off"
        />
        {query && (
          <button onClick={() => { setQuery(''); setResults([]); }} className="absolute right-3 top-1/2 -translate-y-1/2 text-tertiary hover:text-primary">
            <X size={16} />
          </button>
        )}
      </div>

      {/* Dropdown */}
      {showDropdown && (
        <div className="absolute top-full left-0 right-0 mt-2 bg-primary rounded-xl border shadow-2xl overflow-hidden z-50 max-h-[480px] overflow-y-auto">
          
          {/* Search Results with Thumbnails (#6) */}
          {query && results.length > 0 && (
            <div className="p-2">
              <h4 className="px-3 py-2 text-xs font-semibold text-tertiary uppercase tracking-wider">
                {t('search.results', 'Kết quả')}
              </h4>
              {results.map((item) => (
                <button
                  key={item.code}
                  onClick={() => { navigate(`/product/${item.code}`); setShowDropdown(false); onClose?.(); }}
                  className="w-full flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-hover transition-colors text-left"
                >
                  <div className="w-10 h-10 rounded-lg bg-secondary overflow-hidden shrink-0">
                    {item.imageUrl ? (
                      <CustomImage src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" />
                    ) : (
                      <div className="w-full h-full flex items-center justify-center text-tertiary text-xs">📦</div>
                    )}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium truncate">{item.name}</p>
                    {item.price && item.price > 0 && (
                      <p className="text-xs text-accent-primary font-semibold">{formatPrice(item.price)}</p>
                    )}
                  </div>
                  <ArrowRight size={14} className="text-tertiary shrink-0" />
                </button>
              ))}
              <button
                onClick={() => handleSubmit(query)}
                className="w-full px-3 py-2 text-sm text-accent-primary font-medium hover:bg-hover rounded-lg transition-colors text-center mt-1"
              >
                {t('search.viewAll', 'Xem tất cả kết quả')} →
              </button>
            </div>
          )}

          {/* Loading */}
          {query && isLoading && (
            <div className="p-6 text-center text-tertiary text-sm">
              <div className="animate-spin w-5 h-5 border-2 border-accent-primary border-t-transparent rounded-full mx-auto mb-2" />
              {t('common.loading', 'Đang tìm...')}
            </div>
          )}

          {/* No results */}
          {query && !isLoading && results.length === 0 && debouncedQuery.length >= 2 && (
            <div className="p-6 text-center text-tertiary text-sm">
              🔍 {t('common.noResults', 'Không tìm thấy sản phẩm nào')}
            </div>
          )}

          {/* Recent Searches (#7) */}
          {!query && recentSearches.length > 0 && (
            <div className="p-2">
              <div className="flex items-center justify-between px-3 py-2">
                <h4 className="text-xs font-semibold text-tertiary uppercase tracking-wider flex items-center gap-1.5">
                  <Clock size={12} /> {t('search.recent', 'Tìm kiếm gần đây')}
                </h4>
                <button onClick={clearRecentSearches} className="text-xs text-error hover:underline">
                  {t('filter.clearAll', 'Xóa tất cả')}
                </button>
              </div>
              {recentSearches.map((term) => (
                <div key={term} className="flex items-center group">
                  <button
                    onClick={() => { setQuery(term); handleSubmit(term); }}
                    className="flex-1 flex items-center gap-2 px-3 py-2 rounded-lg hover:bg-hover transition-colors text-left text-sm"
                  >
                    <Clock size={14} className="text-tertiary" />
                    {term}
                  </button>
                  <button
                    onClick={() => removeRecentSearch(term)}
                    className="p-1 opacity-0 group-hover:opacity-100 text-tertiary hover:text-error transition-all"
                  >
                    <X size={14} />
                  </button>
                </div>
              ))}
            </div>
          )}

          {/* Trending Searches (#8) */}
          {!query && (
            <div className="p-2 border-t">
              <h4 className="px-3 py-2 text-xs font-semibold text-tertiary uppercase tracking-wider flex items-center gap-1.5">
                <TrendingUp size={12} /> {t('search.trending', 'Xu hướng tìm kiếm')}
              </h4>
              <div className="flex flex-wrap gap-1.5 px-3 pb-2">
                {trendingTerms.map((term) => (
                  <button
                    key={term}
                    onClick={() => { setQuery(term); handleSubmit(term); }}
                    className="px-3 py-1.5 text-xs bg-secondary rounded-full hover:bg-accent-primary hover:text-white transition-all font-medium"
                  >
                    {term}
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
