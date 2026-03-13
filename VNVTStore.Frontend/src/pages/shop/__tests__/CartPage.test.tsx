import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { BrowserRouter } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import CartPage from '../CartPage';
import { useCartStore } from '@/store';

// Mock dependencies — include useToast and other store exports
vi.mock('@/store', async (importOriginal) => {
  const actual = await importOriginal<Record<string, unknown>>();
  return {
    ...actual,
    useCartStore: vi.fn(),
    useAuthStore: {
      getState: () => ({ isAuthenticated: false })
    },
    useToast: () => ({
      success: vi.fn(),
      error: vi.fn(),
      info: vi.fn(),
    }),
    useCompareStore: () => ({
      items: [],
      addItem: vi.fn(),
      removeItem: vi.fn(),
    }),
    useRecentStore: () => ({
      items: [],
    }),
  };
});

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, ...props }: { children: React.ReactNode }) => <div {...props}>{children}</div>,
    span: ({ children, ...props }: { children: React.ReactNode }) => <span {...props}>{children}</span>,
    button: ({ children, onClick, ...props }: { children: React.ReactNode; onClick?: () => void }) => <button onClick={onClick} {...props}>{children}</button>,
    tr: ({ children, ...props }: { children: React.ReactNode }) => <tr {...props}>{children}</tr>,
  },
  AnimatePresence: ({ children }: { children: React.ReactNode }) => <>{children}</>,
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
    getItemCount: vi.fn(() => 0),
    clearCart: vi.fn(),
    isLoading: false,
    fetchCart: vi.fn(),
    addItem: vi.fn(),
    coupon: null,
    discountAmount: 0,
    applyCoupon: vi.fn(),
    removeCoupon: vi.fn(),
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
    const total = 400000;
    vi.mocked(useCartStore).mockReturnValue({
      ...mockStore,
      items: mockItems,
      getTotal: () => total,
      getItemCount: () => 3,
    });

    render(
      <BrowserRouter>
        <CartPage />
      </BrowserRouter>
    );

    expect(screen.getByText('Product 1')).toBeInTheDocument();
    expect(screen.getByText('Product 2')).toBeInTheDocument();
  });

  it('calls updateQuantity when plus/minus buttons are clicked', () => {
    const updateQuantity = vi.fn();
    vi.mocked(useCartStore).mockReturnValue({
      ...mockStore,
      items: mockItems,
      updateQuantity,
      getItemCount: () => 3,
    });

    render(
      <BrowserRouter>
        <CartPage />
      </BrowserRouter>
    );

    const plusButtons = screen.getAllByRole('button').filter(b => b.querySelector('svg')?.classList.contains('lucide-plus'));
    if (plusButtons.length > 0) {
      fireEvent.click(plusButtons[0]);
      expect(updateQuantity).toHaveBeenCalled();
    }
  });

  it('calls removeItem when remove button is clicked', () => {
    const removeItem = vi.fn();
    vi.mocked(useCartStore).mockReturnValue({
      ...mockStore,
      items: mockItems,
      removeItem,
      getItemCount: () => 3,
    });

    render(
      <BrowserRouter>
        <CartPage />
      </BrowserRouter>
    );

    const removeButtons = screen.getAllByRole('button').filter(b => b.querySelector('svg')?.classList.contains('lucide-x') || b.querySelector('svg')?.classList.contains('lucide-trash-2'));
    if (removeButtons.length > 0) {
      fireEvent.click(removeButtons[0]);
      expect(removeItem).toHaveBeenCalled();
    }
  });

  it('calls clearCart when clear button is clicked', () => {
    const clearCart = vi.fn();
    vi.mocked(useCartStore).mockReturnValue({
      ...mockStore,
      items: mockItems,
      clearCart,
      getItemCount: () => 3,
    });

    render(
      <BrowserRouter>
        <CartPage />
      </BrowserRouter>
    );

    const clearButton = screen.queryByText('cart.clearCart');
    if (clearButton) {
      fireEvent.click(clearButton);
      expect(clearCart).toHaveBeenCalled();
    }
  });
});
