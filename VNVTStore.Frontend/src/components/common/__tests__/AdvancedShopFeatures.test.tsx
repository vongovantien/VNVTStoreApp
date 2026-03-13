/**
 * Unit tests for AdvancedShopFeatures components
 * Features: #71 PWA Install, #75 Pull-to-Refresh, #89 Maintenance Mode,
 * #52 First Order Discount, #53 Tiered Discounts, #64 MOQ Warning,
 * #42 Review Sorting, #43 Review Search, #44 Verified Purchase Filter,
 * #35 Order History Filters
 */
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi, describe, it, expect } from 'vitest';

// Mock framer-motion
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, ...props }: { children: React.ReactNode }) => <div {...props}>{children}</div>,
    button: ({ children, onClick, ...props }: { children: React.ReactNode; onClick?: () => void }) => <button onClick={onClick} {...props}>{children}</button>,
  },
  AnimatePresence: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

import {
  MaintenancePage,
  FirstOrderBanner,
  TieredDiscounts,
  MOQWarning,
  ReviewSortDropdown,
  ReviewSearchBar,
  VerifiedPurchaseFilter,
  OrderHistoryFilters,
} from '../AdvancedShopFeatures';

// Feature #89: Maintenance Mode Page
describe('MaintenancePage - Feature #89', () => {
  it('renders maintenance message', () => {
    render(<MaintenancePage />);
    expect(screen.getByText(/Bảo trì/i)).toBeInTheDocument();
  });

  it('displays an estimated return time or message', () => {
    render(<MaintenancePage />);
    expect(screen.getByText(/quay lại|sớm/i)).toBeInTheDocument();
  });
});

// Feature #52: First Order Discount
describe('FirstOrderBanner - Feature #52', () => {
  it('renders discount banner', () => {
    render(<FirstOrderBanner />);
    expect(screen.getByText(/Giảm|đơn đầu|%/i)).toBeInTheDocument();
  });

  it('displays discount amount', () => {
    render(<FirstOrderBanner />);
    // Should show a percentage or amount
    const text = document.body.textContent;
    expect(text).toMatch(/\d+%/);
  });
});

// Feature #53: Tiered Discounts
describe('TieredDiscounts - Feature #53', () => {
  it('renders all tier levels', () => {
    render(<TieredDiscounts quantity={1} />);
    expect(screen.getByText(/Mua 2\+/i)).toBeInTheDocument();
    expect(screen.getByText(/Mua 5\+/i)).toBeInTheDocument();
    expect(screen.getByText(/Mua 10\+/i)).toBeInTheDocument();
  });

  it('highlights active tier based on current quantity', () => {
    render(<TieredDiscounts quantity={7} />);
    // Tier 2 (5+) should be highlighted
    const activeTier = screen.getByText(/Mua 5\+/i);
    expect(activeTier.className).toMatch(/green|indigo/);
  });
});

// Feature #64: MOQ Warning
describe('MOQWarning - Feature #64', () => {
  it('renders warning when quantity is below MOQ', () => {
    render(<MOQWarning minQty={5} currentQty={3} />);
    expect(screen.getByText(/tối thiểu/i)).toBeInTheDocument();
  });

  it('does not render when quantity meets MOQ', () => {
    const { container } = render(<MOQWarning minQty={5} currentQty={5} />);
    expect(container).toBeEmptyDOMElement();
  });
});

// Feature #42: Review Sorting
describe('ReviewSortDropdown - Feature #42', () => {
  const mockOnChange = vi.fn();

  it('renders sort options', () => {
    render(<ReviewSortDropdown value="newest" onChange={mockOnChange} />);
    const select = screen.getByRole('combobox') || document.querySelector('select');
    expect(select).toBeInTheDocument();
  });

  it('calls onChange when a sort option is selected', () => {
    render(<ReviewSortDropdown value="newest" onChange={mockOnChange} />);
    const select = screen.getByRole('combobox') || document.querySelector('select')!;
    fireEvent.change(select, { target: { value: 'highest' } });
    expect(mockOnChange).toHaveBeenCalledWith('highest');
  });
});

// Feature #43: Review Search
describe('ReviewSearchBar - Feature #43', () => {
  const mockOnSearch = vi.fn();

  it('renders search input', () => {
    render(<ReviewSearchBar value="" onChange={mockOnSearch} />);
    const input = screen.getByPlaceholderText(/tìm|search|đánh giá/i);
    expect(input).toBeInTheDocument();
  });

  it('calls onChange when input changes', () => {
    render(<ReviewSearchBar value="" onChange={mockOnSearch} />);
    const input = screen.getByPlaceholderText(/tìm|search|đánh giá/i);
    fireEvent.change(input, { target: { value: 'great quality' } });
    expect(mockOnSearch).toHaveBeenCalledWith('great quality');
  });
});

// Feature #44: Verified Purchase Filter
describe('VerifiedPurchaseFilter - Feature #44', () => {
  const mockOnChange = vi.fn();

  it('renders the filter toggle', () => {
    render(<VerifiedPurchaseFilter active={false} onChange={mockOnChange} />);
    expect(screen.getByText(/Đã mua hàng/)).toBeInTheDocument();
  });

  it('calls onChange when clicked', () => {
    render(<VerifiedPurchaseFilter active={false} onChange={mockOnChange} />);
    const button = screen.getByRole('button');
    fireEvent.click(button);
    expect(mockOnChange).toHaveBeenCalledWith(true);
  });
});

// Feature #35: Order History Filters
describe('OrderHistoryFilters - Feature #35', () => {
  const mockOnChange = vi.fn();

  it('renders status filter options', () => {
    render(<OrderHistoryFilters status="all" dateRange={{ from: '', to: '' }} onStatusChange={mockOnChange} onDateRangeChange={vi.fn()} />);
    expect(screen.getByText(/Tất cả/)).toBeInTheDocument();
  });

  it('calls onStatusChange when status filter changes', () => {
    render(<OrderHistoryFilters status="all" dateRange={{ from: '', to: '' }} onStatusChange={mockOnChange} onDateRangeChange={vi.fn()} />);
    const pendingBtn = screen.getByText(/Chờ xác nhận/);
    fireEvent.click(pendingBtn);
    expect(mockOnChange).toHaveBeenCalledWith('pending');
  });
});
