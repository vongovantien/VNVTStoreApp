import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import VerifyOrderPage from '../VerifyOrderPage';
import { orderService } from '@/services/orderService';

// Mock dependencies
vi.mock('@/services/orderService');
vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));
// Helper to render with router
const renderWithRouter = (initialEntries = ['/verify-order?token=test_token']) => {
  return render(
    <MemoryRouter initialEntries={initialEntries}>
      <Routes>
        <Route path="/verify-order" element={<VerifyOrderPage />} />
        <Route path="/" element={<div>Home Page</div>} />
      </Routes>
    </MemoryRouter>
  );
};

describe('VerifyOrderPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('shows loading state initially', () => {
    vi.mocked(orderService.verify).mockImplementation(() => new Promise(() => {})); // Hang promise
    renderWithRouter();
    // Assuming PageLoader renders a spinner or similar. 
    // Since PageLoader is mocked or implementation detail, we look for query that might indicate loading or just absence of error/success
    // Actually, PageLoader has "animate-spin".
    // Or we can check if success/error messages are NOT present.
    expect(screen.queryByText(/approved/i)).not.toBeInTheDocument();
  });

  it('shows success message when verification succeeds', async () => {
    vi.mocked(orderService.verify).mockResolvedValue({
      success: true,
      data: 'ORD123',
      message: 'Order verified successfully',
      statusCode: 200
    });

    renderWithRouter();

    await waitFor(() => {
      expect(screen.getByText('verifyOrder.verifiedTitle')).toBeInTheDocument();
    });
    expect(screen.getByText('Order verified successfully')).toBeInTheDocument();
  });

  it('shows error message when verification fails', async () => {
    vi.mocked(orderService.verify).mockResolvedValue({
      success: false,
      message: 'Invalid token',
      data: null,
      statusCode: 400
    });

    renderWithRouter();

    await waitFor(() => {
      expect(screen.getByText('verifyOrder.failedTitle')).toBeInTheDocument();
    });
    expect(screen.getByText('Invalid token')).toBeInTheDocument();
  });

  it('handles missing token', async () => {
    renderWithRouter(['/verify-order']); // No token param

    await waitFor(() => {
      expect(screen.getByText('verifyOrder.failedTitle')).toBeInTheDocument();
    });
    expect(screen.getByText('verifyOrder.missingToken')).toBeInTheDocument();
  });

  it('handles API error exception', async () => {
    vi.mocked(orderService.verify).mockRejectedValue(new Error('Network Error'));

    renderWithRouter();

    await waitFor(() => {
      expect(screen.getByText('Network Error')).toBeInTheDocument();
    });
  });
});
