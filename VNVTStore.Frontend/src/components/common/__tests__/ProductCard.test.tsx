import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { BrowserRouter } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import ProductCard from '../ProductCard';
import { useCartStore, useWishlistStore, useCompareStore, usePriceAlertStore, useUIStore, useToast } from '@/store';
import { Product } from '@/types';

// Mock dependencies
vi.mock('@/store', () => ({
  useCartStore: vi.fn(),
  useWishlistStore: vi.fn(),
  useCompareStore: vi.fn(),
  usePriceAlertStore: vi.fn(),
  useUIStore: vi.fn(),
  useToast: vi.fn(),
}));
// Local i18n mock removed to use global mock from setup.ts

const mockProduct: Product = {
  code: 'P001',
  name: 'Test Product',
  price: 100000,
  category: 'Test Category',
  categoryCode: 'C001',
  image: 'test.jpg',
  description: 'Test Description',
  stockQuantity: 10,  // using stockQuantity as per component logic
  averageRating: 4.5,
  reviewCount: 10,
  isNew: true,
  createdAt: new Date().toISOString()
};

describe('ProductCard', () => {
  const mockAddItem = vi.fn();
  const mockAddToWishlist = vi.fn();
  const mockRemoveFromWishlist = vi.fn();
  const mockAddToCompare = vi.fn();
  const mockRemoveFromCompare = vi.fn();
  const mockToastSuccess = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useCartStore).mockReturnValue(mockAddItem);
    vi.mocked(useWishlistStore).mockReturnValue({
      addItem: mockAddToWishlist,
      removeItem: mockRemoveFromWishlist,
      isInWishlist: (code: string) => code === 'WISH001',
    });
    vi.mocked(useCompareStore).mockReturnValue({
        addItem: mockAddToCompare,
        removeItem: mockRemoveFromCompare,
        isInCompare: (code: string) => code === 'COMP001',
    });
    vi.mocked(usePriceAlertStore).mockReturnValue({
        toggleAlert: vi.fn(),
        isWatched: () => false,
    });
    vi.mocked(useUIStore).mockReturnValue({
        setQuickViewProduct: vi.fn(),
    });
    vi.mocked(useToast).mockReturnValue({
        success: mockToastSuccess,
        error: vi.fn()
    });
  });

  it('renders product information correctly', () => {
    render(
      <BrowserRouter>
        <ProductCard product={mockProduct} />
      </BrowserRouter>
    );

    expect(screen.getByText('Test Product')).toBeInTheDocument();
    // Use regex to be flexible with separators and currency symbols
    expect(screen.getByText(/100[.,]000/)).toBeInTheDocument();
  });

  it('handles add to cart', async () => {
     render(
      <BrowserRouter>
        <ProductCard product={mockProduct} />
      </BrowserRouter>
    );
    
    // The button name in our mock will be the default text passed to t()
    const addToCartBtn = screen.getByRole('button', { name: /Thêm giỏ/i });
    fireEvent.click(addToCartBtn);

    await waitFor(() => {
        expect(mockAddItem).toHaveBeenCalledWith(mockProduct);
        expect(mockToastSuccess).toHaveBeenCalled();
    });
  });

  it('toggles wishlist', () => {
    render(
      <BrowserRouter>
        <ProductCard product={mockProduct} />
      </BrowserRouter>
    );

    const wishlistBtn = screen.getByTitle(/wishlist/i);
    fireEvent.click(wishlistBtn);

    // Initial mock returns false for isInWishlist('P001')
    expect(mockAddToWishlist).toHaveBeenCalledWith(mockProduct);
  });

  it('renders wholesale pricing when available', () => {
    const productWithWholesale: Product = {
      ...mockProduct,
      wholesaleTiers: [
        { minQuantity: 10, price: 90000 },
        { minQuantity: 50, price: 80000 }
      ]
    };

    render(
      <BrowserRouter>
        <ProductCard product={productWithWholesale} />
      </BrowserRouter>
    );

    expect(screen.getByText(/Bán sỉ/i)).toBeInTheDocument();
    expect(screen.getByText(/≥ 10 cái/i)).toBeInTheDocument();
  });

  it('renders shipping urgency banner', () => {
    render(
      <BrowserRouter>
        <ProductCard product={mockProduct} />
      </BrowserRouter>
    );

    expect(screen.getByText(/Đặt trong/i)).toBeInTheDocument();
  });

  it('triggers compare toggle', () => {
    render(
      <BrowserRouter>
        <ProductCard product={mockProduct} />
      </BrowserRouter>
    );

    // Using title because it's a small icon button
    const compareBtns = screen.getAllByTitle(/Compare/i);
    fireEvent.click(compareBtns[0]);
    expect(mockAddToCompare).toHaveBeenCalledWith(mockProduct);
  });
});
