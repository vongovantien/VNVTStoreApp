import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { BrowserRouter } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import CheckoutPage from '../CheckoutPage';
import { useCartStore, useAuthStore } from '@/store';
import { useCheckoutStore } from '@/store/checkoutStore';
import { orderService } from '@/services/orderService';

// Mock dependencies
vi.mock('@/store', () => ({
  useCartStore: vi.fn(),
  useAuthStore: vi.fn(),
  useToast: vi.fn(() => ({
    success: vi.fn(),
    error: vi.fn(),
  })),
}));

vi.mock('@/store/checkoutStore', () => ({
  useCheckoutStore: vi.fn(),
}));

vi.mock('@/services/orderService', () => ({
  orderService: {
    create: vi.fn(),
  },
}));

vi.mock('@/services/paymentService', () => ({
  paymentService: {
    create: vi.fn(),
  },
}));

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

describe('CheckoutPage', () => {
  const mockItems = [
    {
      code: 'ITEM1',
      product: { code: 'PROD1', name: 'Product 1', price: 100000, image: 'img1.jpg' },
      quantity: 1
    }
  ];

  const mockCheckoutStore = {
    step: 1,
    setStep: vi.fn(),
    formData: {
      fullName: '',
      phone: '',
      email: '',
      address: '',
      city: '',
      district: '',
      ward: '',
      note: ''
    },
    setFormData: vi.fn(),
    paymentMethod: 'COD',
    setPaymentMethod: vi.fn(),
    voucherCode: '',
    setVoucherCode: vi.fn(),
    resetCheckout: vi.fn()
  };

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useCartStore).mockReturnValue({
      items: mockItems,
      getTotal: () => 100000,
      clearCart: vi.fn(),
      fetchCart: vi.fn()
    });
    vi.mocked(useAuthStore).mockReturnValue({
      user: null,
      isAuthenticated: false
    });
    vi.mocked(useCheckoutStore).mockReturnValue(mockCheckoutStore);
  });

  it('renders shipping step initially', () => {
    render(
      <BrowserRouter>
        <CheckoutPage />
      </BrowserRouter>
    );

    expect(screen.getByText(/checkout.shippingInfo/i)).toBeInTheDocument();
    expect(screen.getByText(/checkout.fullName/i)).toBeInTheDocument();
  });

  it('shows validation error if required fields are missing in step 1', async () => {
    render(
      <BrowserRouter>
        <CheckoutPage />
      </BrowserRouter>
    );

    const continueButton = screen.getByText('checkout.continue');
    fireEvent.click(continueButton);

    // Should stay in step 1 and show errors (or toast)
    expect(mockCheckoutStore.setStep).not.toHaveBeenCalled();
  });

  it('proceeds to step 2 when form is valid', async () => {
    vi.mocked(useCheckoutStore).mockReturnValue({
      ...mockCheckoutStore,
      formData: {
        fullName: 'John Doe',
        phone: '0901234567',
        email: 'john@example.com',
        address: '123 Street',
        city: 'hcm',
        district: 'q1',
        ward: '',
        note: ''
      }
    });

    render(
      <BrowserRouter>
        <CheckoutPage />
      </BrowserRouter>
    );

    const continueButton = screen.getByText('checkout.continue');
    fireEvent.click(continueButton);

    expect(mockCheckoutStore.setStep).toHaveBeenCalledWith(2);
  });

  it('submits order successfully in step 3', async () => {
    vi.mocked(useCheckoutStore).mockReturnValue({
      ...mockCheckoutStore,
      step: 3,
      formData: {
        fullName: 'John Doe',
        phone: '0901234567',
        email: 'john@example.com',
        address: '123 Street',
        city: 'hcm',
        district: 'q1',
        ward: '',
        note: ''
      }
    });

    vi.mocked(orderService.create).mockResolvedValue({
      success: true,
      data: { code: 'ORD123' }
    });

    render(
      <BrowserRouter>
        <CheckoutPage />
      </BrowserRouter>
    );

    const placeOrderButton = screen.getByText('checkout.placeOrder');
    fireEvent.click(placeOrderButton);

    await waitFor(() => {
      expect(orderService.create).toHaveBeenCalled();
    });
  });
});
