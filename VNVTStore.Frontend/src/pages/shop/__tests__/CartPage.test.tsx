import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { BrowserRouter } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import CartPage from '../CartPage';
import { useCartStore } from '@/store';

// Mock dependencies
vi.mock('@/store', () => ({
  useCartStore: vi.fn(),
  useAuthStore: {
    getState: () => ({ isAuthenticated: false })
  }
}));

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

describe('CartPage', () => {
  const mockItems = [
    {
      code: 'ITEM1',
      product: {
        code: 'PROD1',
        name: 'Product 1',
        price: 100000,
        image: 'img1.jpg',
        category: 'Cat 1'
      },
      quantity: 2
    },
    {
      code: 'ITEM2',
      product: {
        code: 'PROD2',
        name: 'Product 2',
        price: 200000,
        image: 'img2.jpg',
        category: 'Cat 2'
      },
      quantity: 1
    }
  ];

  const mockStore = {
    items: [],
    removeItem: vi.fn(),
    updateQuantity: vi.fn(),
    getTotal: vi.fn(() => 0),
    clearCart: vi.fn(),
    isLoading: false
  };

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useCartStore).mockReturnValue(mockStore);
  });

  it('renders empty cart message when no items', () => {
    render(
      <BrowserRouter>
        <CartPage />
      </BrowserRouter>
    );

    expect(screen.getByText('cart.empty')).toBeInTheDocument();
  });

  it('renders cart items correctly', () => {
    const total = 400000; // 100k*2 + 200k*1
    vi.mocked(useCartStore).mockReturnValue({
      ...mockStore,
      items: mockItems,
      getTotal: () => total
    });

    render(
      <BrowserRouter>
        <CartPage />
      </BrowserRouter>
    );

    expect(screen.getByText('Product 1')).toBeInTheDocument();
    expect(screen.getByText('Product 2')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument(); // Quantity of Item 1
    expect(screen.getByText('1')).toBeInTheDocument(); // Quantity of Item 2
  });

  it('calls updateQuantity when plus/minus buttons are clicked', () => {
    const updateQuantity = vi.fn();
    vi.mocked(useCartStore).mockReturnValue({
      ...mockStore,
      items: mockItems,
      updateQuantity
    });

    render(
      <BrowserRouter>
        <CartPage />
      </BrowserRouter>
    );

    const plusButtons = screen.getAllByRole('button').filter(b => b.querySelector('svg')?.classList.contains('lucide-plus'));
    fireEvent.click(plusButtons[0]);
    expect(updateQuantity).toHaveBeenCalledWith('ITEM1', 3);

    const minusButtons = screen.getAllByRole('button').filter(b => b.querySelector('svg')?.classList.contains('lucide-minus'));
    fireEvent.click(minusButtons[0]);
    expect(updateQuantity).toHaveBeenCalledWith('ITEM1', 1);
  });

  it('calls removeItem when remove button is clicked', () => {
    const removeItem = vi.fn();
    vi.mocked(useCartStore).mockReturnValue({
      ...mockStore,
      items: mockItems,
      removeItem
    });

    render(
      <BrowserRouter>
        <CartPage />
      </BrowserRouter>
    );

    const removeButtons = screen.getAllByRole('button').filter(b => b.querySelector('svg')?.classList.contains('lucide-x'));
    fireEvent.click(removeButtons[0]);
    expect(removeItem).toHaveBeenCalledWith('ITEM1');
  });

  it('calls clearCart when clear button is clicked', () => {
    const clearCart = vi.fn();
    vi.mocked(useCartStore).mockReturnValue({
      ...mockStore,
      items: mockItems,
      clearCart
    });

    render(
      <BrowserRouter>
        <CartPage />
      </BrowserRouter>
    );

    fireEvent.click(screen.getByText('cart.clearCart'));
    expect(clearCart).toHaveBeenCalled();
  });
});
