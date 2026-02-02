import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';

interface ProductFilterState {
  // UI State
  viewMode: 'grid' | 'list';
  showFilters: boolean;

  // Filter State
  sortBy: string;
  priceRange: [number, number];
  selectedCategories: string[];
  selectedBrands: string[];
  selectedRating: number | null;
  priceType: 'all' | 'fixed' | 'contact';
  searchQuery: string;

  // Actions
  setViewMode: (mode: 'grid' | 'list') => void;
  setShowFilters: (show: boolean) => void;
  setSortBy: (sort: string) => void;
  setPriceRange: (range: [number, number]) => void;
  setSelectedCategories: (categories: string[]) => void;
  toggleCategory: (category: string) => void;
  setSelectedBrands: (brands: string[]) => void;
  toggleBrand: (brand: string) => void;
  setSelectedRating: (rating: number | null) => void;
  setPriceType: (type: 'all' | 'fixed' | 'contact') => void;
  setSearchQuery: (query: string) => void;

  // Reset
  resetFilters: () => void;
}

const initialFilterState = {
  sortBy: 'newest',
  priceRange: [0, 100000000] as [number, number],
  selectedCategories: [],
  selectedBrands: [],
  selectedRating: null,
  priceType: 'all' as const,
  searchQuery: '',
};

export const useProductFilterStore = create<ProductFilterState>()(
  persist(
    (set) => ({
      // UI Defaults
      viewMode: 'grid',
      showFilters: true,

      // Filter Defaults
      ...initialFilterState,

      // Actions
      setViewMode: (viewMode) => set({ viewMode }),
      setShowFilters: (showFilters) => set({ showFilters }),
      setSortBy: (sortBy) => set({ sortBy }),
      setPriceRange: (priceRange) => set({ priceRange }),

      setSelectedCategories: (selectedCategories) => set({ selectedCategories }),
      toggleCategory: (category) =>
        set((state) => {
          const current = state.selectedCategories;
          const next = current.includes(category)
            ? current.filter((c) => c !== category)
            : [...current, category];
          return { selectedCategories: next };
        }),

      setSelectedBrands: (selectedBrands) => set({ selectedBrands }),
      toggleBrand: (brand) =>
        set((state) => {
          const current = state.selectedBrands;
          const next = current.includes(brand)
            ? current.filter((b) => b !== brand)
            : [...current, brand];
          return { selectedBrands: next };
        }),

      setSelectedRating: (selectedRating) => set({ selectedRating }),
      setPriceType: (priceType) => set({ priceType }),
      setSearchQuery: (searchQuery) => set({ searchQuery }),

      resetFilters: () => set({ ...initialFilterState }),
    }),
    {
      name: 'vnvt-product-filters',
      storage: createJSONStorage(() => localStorage),
      // Only persist UI settings and maybe sort/filters if desired.
      // Often users expect filters to reset on new session, but viewMode to persist.
      // Let's persist everything for a "sticky" experience, or partial.
      // For now, persisting everything as it enhances UX for returning users.
      partialize: (state) => ({
        viewMode: state.viewMode,
        showFilters: state.showFilters,
        // Uncomment to persist filters too
        // sortBy: state.sortBy
      }),
    }
  )
);
