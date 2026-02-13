/**
 * Unit tests for SearchAutocomplete component
 * Features: #6 Search Autocomplete with Thumbnails, #7 Recent Search History, #8 Trending Search Terms
 */
import { render, screen, fireEvent, act, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';

// Mock framer-motion
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, onClick, ...props }: React.HTMLAttributes<HTMLDivElement>) => <div onClick={onClick} {...props}>{children}</div>,
    li: ({ children, onClick, ...props }: React.HTMLAttributes<HTMLLIElement>) => <li onClick={onClick} {...props}>{children}</li>,
  },
  AnimatePresence: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

// Mock useDebounce
vi.mock('@/hooks/useDebounce', () => ({
  useDebounce: (v: string) => v,
}));

// Mock productService
vi.mock('@/services/productService', () => ({
  productService: {
    getProducts: vi.fn().mockResolvedValue({
      data: [
        { code: 'P1', name: 'iPhone 15 Pro', price: 29990000, imageUrl: '/img/iphone.jpg' },
        { code: 'P2', name: 'iPad Air', price: 19990000, imageUrl: '/img/ipad.jpg' },
      ],
    }),
  },
}));

// Mock react-router-dom
const mockNavigate = vi.fn();
vi.mock('react-router-dom', () => ({
  useNavigate: vi.fn(() => mockNavigate),
}));

// Mock lucide-react
vi.mock('lucide-react', () => ({
  Search: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>Search</span>,
  X: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>X</span>,
  Clock: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>Clock</span>,
  TrendingUp: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>TrendingUp</span>,
  ArrowRight: (props: React.SVGProps<SVGSVGElement>) => <span {...(props as Record<string, unknown>)}>ArrowRight</span>,
}));

import { SearchAutocomplete } from '../SearchAutocomplete';

describe('SearchAutocomplete', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  // Feature #6: Search Autocomplete with Thumbnails
  describe('#6 Search Autocomplete with Thumbnails', () => {
    it('renders the search input', () => {
      render(<SearchAutocomplete />);
      const input = screen.getByPlaceholderText(/Tìm kiếm sản phẩm/);
      expect(input).toBeInTheDocument();
    });

    it('shows dropdown on focus', () => {
      render(<SearchAutocomplete />);
      const input = screen.getByPlaceholderText(/Tìm kiếm sản phẩm/);
      fireEvent.focus(input);
      
      // Dropdown should be visible (even if empty, it shows trending/recent)
      // The dropdown container should be in the DOM
      const dropdown = document.querySelector('[class*="absolute"][class*="shadow"]');
      expect(dropdown).toBeTruthy();
    });
  });

  // Feature #7: Recent Search History
  describe('#7 Recent Search History', () => {
    it('stores search terms in localStorage on Enter', async () => {
      render(<SearchAutocomplete />);
      const input = screen.getByPlaceholderText(/Tìm kiếm sản phẩm/);
      
      fireEvent.change(input, { target: { value: 'iPhone' } });
      fireEvent.keyDown(input, { key: 'Enter' });

      const stored = localStorage.getItem('vnvt_recent_searches');
      expect(stored).not.toBeNull();
      if (stored) {
        const parsed = JSON.parse(stored);
        expect(parsed).toContain('iPhone');
      }
    });

    it('navigates to search results on Enter', () => {
      render(<SearchAutocomplete />);
      const input = screen.getByPlaceholderText(/Tìm kiếm sản phẩm/);
      
      fireEvent.change(input, { target: { value: 'iPhone' } });
      fireEvent.keyDown(input, { key: 'Enter' });
      
      expect(mockNavigate).toHaveBeenCalledWith(expect.stringContaining('/products?search=iPhone'));
    });

    it('shows recent searches on focus with pre-populated data', () => {
      localStorage.setItem('vnvt_recent_searches', JSON.stringify(['iPhone', 'iPad']));
      
      render(<SearchAutocomplete />);
      const input = screen.getByPlaceholderText(/Tìm kiếm sản phẩm/);
      fireEvent.focus(input);

      expect(screen.getByText('iPhone')).toBeInTheDocument();
      expect(screen.getByText('iPad')).toBeInTheDocument();
    });

    it('clears recent searches when clear all is clicked', () => {
      localStorage.setItem('vnvt_recent_searches', JSON.stringify(['test search']));
      
      render(<SearchAutocomplete />);
      const input = screen.getByPlaceholderText(/Tìm kiếm sản phẩm/);
      fireEvent.focus(input);

      const clearBtn = screen.getByText(/Xóa tất cả/);
      fireEvent.click(clearBtn);

      expect(localStorage.getItem('vnvt_recent_searches')).toBeNull();
    });
  });

  // Feature #8: Trending Search Terms
  describe('#8 Trending Search Terms', () => {
    it('shows trending terms section on focus', () => {
      render(<SearchAutocomplete />);
      const input = screen.getByPlaceholderText(/Tìm kiếm sản phẩm/);
      fireEvent.focus(input);

      expect(screen.getByText(/Xu hướng tìm kiếm/)).toBeInTheDocument();
    });
  });
});
