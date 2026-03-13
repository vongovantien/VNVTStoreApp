import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { BrowserRouter } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import ProductDetailPage from '../ProductDetailPage';
import { useCartStore, useWishlistStore, useCompareStore, useRecentStore, usePriceAlertStore, useToast } from '@/store';
import { useProduct, useProducts } from '@/hooks';

// Mock dependencies
vi.mock('@/store', () => ({
  useCartStore: vi.fn(),
  useWishlistStore: vi.fn(),
  useCompareStore: vi.fn(),
  useRecentStore: vi.fn(),
  usePriceAlertStore: vi.fn(),
  useToast: vi.fn(),
}));

vi.mock('@/hooks', () => ({
  useProduct: vi.fn(),
  useProducts: vi.fn(),
}));

vi.mock('@/utils/format', () => ({
  formatCurrency: (value: number) => `${value} VND`,
  getImageUrl: (url: string) => url,
}));

vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal() as Record<string, unknown>;
  return {
    ...actual,
    useParams: () => ({ id: 'PROD1' }),
    Link: ({ children, to }: { children: React.ReactNode; to: string }) => <a href={to}>{children}</a>,
  };
});

// Using global i18n mock from setup.ts


vi.mock('@/components/common/ProductCard', () => ({
  ProductCard: () => <div data-testid="product-card">ProductCard</div>,
}));

vi.mock('@/components/common/Image', () => ({
  default: () => <img alt="test" />,
}));

vi.mock('@/components/reviews/ReviewsList', () => ({
  default: () => <div>ReviewsList</div>,
}));

vi.mock('@/components/reviews/ProductReviewButton', () => ({
  ProductReviewButton: () => <button>Review</button>,
}));

vi.mock('@/components/common/RecentlyViewed', () => ({
  RecentlyViewed: () => <div data-testid="recently-viewed">RecentlyViewed</div>,
}));

vi.mock('@/components/common/StickyCartBar', () => ({
  StickyCartBar: () => <div data-testid="sticky-cart-bar">StickyCartBar</div>,
}));

vi.mock('@/components/common', () => ({
  UpsellSection: () => <div data-testid="upsell-section">UpsellSection</div>,
}));

vi.mock('../components/ProductInfo', () => ({
  ProductInfo: ({ product, handleAddToCart }: { product: { name: string }, handleAddToCart: () => void }) => (
    <div>
      <h1>{product.name}</h1>
      <button onClick={handleAddToCart}>product.addToCart</button>
    </div>
  ),
}));

vi.mock('../components/ProductTabs', () => ({
  ProductTabs: () => <div>ProductTabs</div>,
}));

vi.mock('@/components/common/ImageGallery', () => ({
  ImageGallery: () => <div>ImageGallery</div>,
}));

vi.mock('../components/ProductQA', () => ({
  ProductQA: () => <div data-testid="product-qa">ProductQA</div>,
}));

describe('ProductDetailPage', () => {
  const mockProduct = {
    code: 'PROD1',
    name: 'Test Product',
    price: 100000,
    stock: 10,
    minStockLevel: 5,
    description: 'Test description',
    images: ['img1.jpg', 'img2.jpg'],
    categoryCode: 'CAT1',
    category: 'Category 1',
    details: [],
    variants: [],
    productUnits: [],
    reviewCount: 0,
    averageRating: 4.5
  };

  const mockAddToCart = vi.fn();
  const mockSuccess = vi.fn();
  const mockError = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    const mockCartState = {
      items: [],
      addItem: mockAddToCart,
      removeItem: vi.fn(),
      updateQuantity: vi.fn(),
      getTotal: () => 0,
      clearCart: vi.fn(),
      fetchCart: vi.fn()
    };

    vi.mocked(useCartStore).mockImplementation((selector: unknown) => {
       return typeof selector === 'function' ? selector(mockCartState) : mockCartState;
    });

    const mockWishlistState = {
      addItem: vi.fn(),
      removeItem: vi.fn(),
      isInWishlist: () => false
    };

    vi.mocked(useWishlistStore).mockImplementation((selector: unknown) => {
       return typeof selector === 'function' ? selector(mockWishlistState) : mockWishlistState;
    });

    const mockCompareState = {
      addItem: vi.fn(),
      isInCompare: () => false
    };

    vi.mocked(useCompareStore).mockImplementation((selector: unknown) => {
       return typeof selector === 'function' ? selector(mockCompareState) : mockCompareState;
    });

    vi.mocked(useToast).mockReturnValue({
      success: mockSuccess,
      error: mockError
    });

    vi.mocked(useProduct).mockReturnValue({
      data: mockProduct,
      isLoading: false,
      isError: false
    });
    
    vi.mocked(useProducts).mockReturnValue({
      data: { products: [] }
    });

    const mockRecentState = {
      addToRecent: vi.fn(),
    };

    vi.mocked(useRecentStore).mockImplementation((selector: unknown) => {
       return typeof selector === 'function' ? selector(mockRecentState) : mockRecentState;
    });

    const mockPriceAlertState = {
      toggleAlert: vi.fn(),
      isWatched: () => false,
    };

    vi.mocked(usePriceAlertStore).mockImplementation((selector: unknown) => {
       return typeof selector === 'function' ? selector(mockPriceAlertState) : mockPriceAlertState;
    });

    vi.mocked(useProducts).mockReturnValue({
      data: { products: [] }
    });
  });

  it('renders product details correctly', async () => {
    render(
      <BrowserRouter>
        <ProductDetailPage />
      </BrowserRouter>
    );
    
    expect(await screen.findByRole('heading', { name: /Test Product/i })).toBeInTheDocument();
    // expect(await screen.findByText(/100000 VND/i)).toBeDefined();
    // expect(await screen.findByText(/Test description/i)).toBeDefined();
  });

  it('handles add to cart correctly', async () => {
    render(
      <BrowserRouter>
        <ProductDetailPage />
      </BrowserRouter>
    );

    const addToCartButton = screen.getByText(/product.addToCart/i);
    fireEvent.click(addToCartButton);

    await waitFor(() => {
      expect(mockAddToCart).toHaveBeenCalledWith(mockProduct, 1);
      expect(mockSuccess).toHaveBeenCalled();
    });
  });

  it('shows loading state', () => {
    vi.mocked(useProduct).mockReturnValue({
        data: null,
        isLoading: true,
        isError: false
      });
  
      render(
        <BrowserRouter>
          <ProductDetailPage />
        </BrowserRouter>
      );
  
      expect(screen.getByText('Đang tải sản phẩm...')).toBeInTheDocument();
  });

  it('shows not found state', () => {
    vi.mocked(useProduct).mockReturnValue({
        data: null,
        isLoading: false,
        isError: false
      });
  
      render(
        <BrowserRouter>
          <ProductDetailPage />
        </BrowserRouter>
      );
  
      expect(screen.getByText('common.notFound')).toBeInTheDocument();
  });
});
