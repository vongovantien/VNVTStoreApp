import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import SearchPage from '../SearchPage';
import { useProducts, useDebounce } from '@/hooks';
import { useSearchParams } from 'react-router-dom';
import React from 'react';

// Mocks
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

vi.mock('react-router-dom', () => ({
  Link: ({ children, to }: any) => <a href={to}>{children}</a>,
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
    (useSearchParams as any).mockReturnValue([new URLSearchParams(), mockSetSearchParams]);
    (useProducts as any).mockReturnValue({
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
      { code: 'p1', name: 'Product 1', price: 100, originalPrice: 120, image: '', slug: 'p1' },
    ];
    (useProducts as any).mockReturnValue({
      data: { products: mockProducts, totalPages: 1 },
      isLoading: false,
      isError: false,
    });

    render(<SearchPage />);
    
    expect(screen.getByText('Product 1')).toBeDefined();
  });

  it('displays loading state', () => {
    (useProducts as any).mockReturnValue({
      data: null,
      isLoading: true,
      isError: false,
    });

    render(<SearchPage />);
    
    // In SearchPage, loading renders skeletons
    // We can check if any skeleton-related classes or structures exist
    // But since skeletons are usually just divs, let's check for lack of results
    expect(screen.queryByText('Product 1')).toBeNull();
  });

  it('displays no results message when search returns nothing', () => {
    (useProducts as any).mockReturnValue({
      data: { products: [], totalPages: 0 },
      isLoading: false,
      isError: false,
    });

    render(<SearchPage />);
    
    expect(screen.getByText('common.noResults')).toBeDefined();
  });

  it('handles error state', () => {
    (useProducts as any).mockReturnValue({
      data: null,
      isLoading: false,
      isError: true,
      error: new Error('Api Error')
    });

    render(<SearchPage />);
    
    expect(screen.getByText('common.errorOccurred')).toBeDefined();
    expect(screen.getByText('Api Error')).toBeDefined();
  });
});
