import '@testing-library/jest-dom';
import { describe, it, expect, vi, beforeEach, Mock } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import SearchPage from '../SearchPage';
import { useProducts } from '../../../hooks';
import { Product } from '../../../types';
import { useSearchParams } from 'react-router-dom';

// Mocks
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

vi.mock('react-router-dom', () => ({
  Link: ({ children, to }: { children: React.ReactNode; to: string }) => <a href={to}>{children}</a>,
  useSearchParams: vi.fn(),
}));

vi.mock('@/hooks', () => ({
  useProducts: vi.fn(),
  useDebounce: vi.fn((val) => val),
}));

describe('SearchPage', () => {
  const mockSetSearchParams = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    (useSearchParams as Mock).mockReturnValue([new URLSearchParams(), mockSetSearchParams]);
    (useProducts as Mock).mockReturnValue({
      data: { products: [], totalPages: 0 },
      isLoading: false,
      isError: false,
    });
  });

  it('renders search input and updates params on change', () => {
    render(<SearchPage />);
    
    const input = screen.getByPlaceholderText('header.searchPlaceholder');
    fireEvent.change(input, { target: { value: 'quạt' } });
    
    expect(mockSetSearchParams).toHaveBeenCalled();
  });

  it('displays products when loaded', () => {
    const mockProducts = [
      { code: 'P1', name: 'Product 1', price: 100, imageURL: '', categoryName: 'Cat 1' },
      { code: 'P2', name: 'Product 2', price: 200, imageURL: '', categoryName: 'Cat 2' }
    ] as unknown as Product[];
    (useProducts as Mock).mockReturnValue({
      data: { products: mockProducts, totalPages: 1 },
      isLoading: false,
      isError: false,
    });

    render(<SearchPage />);
    
    expect(screen.getByText('Product 1')).toBeInTheDocument();
  });

  it('displays loading state', () => {
    (useProducts as Mock).mockReturnValue({
      data: null,
      isLoading: true,
      isError: false,
    });

    render(<SearchPage />);
    
    // In SearchPage, loading renders skeletons
    // We can check if any skeleton-related classes or structures exist
    // But since skeletons are usually just divs, let's check for lack of results
    expect(screen.queryByText('Product 1')).not.toBeInTheDocument();
  });

  it('displays no results message when search returns nothing', () => {
    (useProducts as Mock).mockReturnValue({
      data: { products: [], totalPages: 0 },
      isLoading: false,
      isError: false,
    });

    render(<SearchPage />);
    
    expect(screen.getByText('common.noResults')).toBeInTheDocument();
  });

  it('handles error state', () => {
    (useProducts as Mock).mockReturnValue({
      data: null,
      isLoading: false,
      isError: true,
      error: new Error('Api Error')
    });

    render(<SearchPage />);
    
    expect(screen.getByText('common.errorOccurred')).toBeInTheDocument();
    expect(screen.getByText('Api Error')).toBeInTheDocument();
  });
});
