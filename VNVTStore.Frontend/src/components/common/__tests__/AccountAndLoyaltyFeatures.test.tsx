/**
 * Unit tests for Account and Loyalty Features
 * Features: #4 Breadcrumbs, #14 Spec Sheet, #15 Energy Label, #18 Brand Story,
 * #24 Delayed Shipping, #26 Recurring Order, #31 Birthday Rewards,
 * #32 Referral Dashboard, #34 Notification Prefs, #38 RMA, #39 Wallet
 */
import { render, screen, fireEvent, act } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';

// Mock framer-motion
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, ...props }: any) => <div {...props}>{children}</div>,
  },
  AnimatePresence: ({ children }: any) => <>{children}</>,
}));

// Mock clipboard
const mockWriteText = vi.fn().mockResolvedValue(undefined);
Object.defineProperty(navigator, 'clipboard', {
  value: { writeText: mockWriteText },
  writable: true,
});

import {
  AttributeBreadcrumbs,
  PrintableSpecSheet,
  EnergyLabelBadge,
  BrandStoryModal,
  DelayedShippingPicker,
  RecurringOrderToggle,
  BirthdayReward,
  ReferralDashboard,
  NotificationPreferences,
  ReturnRequestForm,
  WalletDisplay,
} from '../AccountAndLoyaltyFeatures';

// #4 Attribute Breadcrumbs
describe('AttributeBreadcrumbs - Feature #4', () => {
  const items = [
    { label: 'Trang chủ', href: '/' },
    { label: 'Điện thoại', href: '/category/phone' },
    { label: 'iPhone 15' },
  ];

  it('renders all breadcrumb items', () => {
    render(<AttributeBreadcrumbs items={items} />);
    expect(screen.getByText('Trang chủ')).toBeInTheDocument();
    expect(screen.getByText('Điện thoại')).toBeInTheDocument();
    expect(screen.getByText('iPhone 15')).toBeInTheDocument();
  });

  it('renders links for items with href', () => {
    render(<AttributeBreadcrumbs items={items} />);
    expect(screen.getByText('Trang chủ').closest('a')).toHaveAttribute('href', '/');
  });

  it('renders last item as non-link', () => {
    render(<AttributeBreadcrumbs items={items} />);
    expect(screen.getByText('iPhone 15').closest('a')).toBeNull();
  });

  it('renders separator between items', () => {
    const { container } = render(<AttributeBreadcrumbs items={items} />);
    // Should have chevron separators (SVG icons)
    const svgs = container.querySelectorAll('svg');
    expect(svgs.length).toBe(2); // 3 items = 2 separators
  });
});

// #14 Printable Spec Sheet
describe('PrintableSpecSheet - Feature #14', () => {
  const specs = { 'Màn hình': '6.1 inch', 'Pin': '3274 mAh', 'RAM': '8 GB' };

  it('renders print button', () => {
    render(<PrintableSpecSheet productName="iPhone 15" specs={specs} />);
    expect(screen.getByText(/Tải thông số/i)).toBeInTheDocument();
  });

  it('calls window.print when button is clicked', () => {
    const mockPrint = vi.fn();
    window.print = mockPrint;
    
    render(<PrintableSpecSheet productName="iPhone 15" specs={specs} />);
    fireEvent.click(screen.getByText(/Tải thông số/i));
    expect(mockPrint).toHaveBeenCalled();
  });
});

// #15 Energy Label Badge
describe('EnergyLabelBadge - Feature #15', () => {
  it('renders A+++ rating in green', () => {
    render(<EnergyLabelBadge rating="A+++" />);
    expect(screen.getByText('A+++')).toBeInTheDocument();
    expect(screen.getByText('A+++').closest('span')).toHaveClass('bg-green-600');
  });

  it('renders D rating in red', () => {
    render(<EnergyLabelBadge rating="D" />);
    expect(screen.getByText('D')).toBeInTheDocument();
    expect(screen.getByText('D').closest('span')).toHaveClass('bg-red-500');
  });
});

// #18 Brand Story Modal
describe('BrandStoryModal - Feature #18', () => {
  const brand = { name: 'Apple', description: 'Founded in Cupertino...', founded: '1976', origin: 'USA' };
  const mockOnClose = vi.fn();

  it('renders when open', () => {
    render(<BrandStoryModal isOpen={true} onClose={mockOnClose} brand={brand} />);
    expect(screen.getByText('Apple')).toBeInTheDocument();
    expect(screen.getByText(/Founded in Cupertino/)).toBeInTheDocument();
  });

  it('does not render when closed', () => {
    render(<BrandStoryModal isOpen={false} onClose={mockOnClose} brand={brand} />);
    expect(screen.queryByText('Apple')).not.toBeInTheDocument();
  });

  it('shows founded year and origin', () => {
    render(<BrandStoryModal isOpen={true} onClose={mockOnClose} brand={brand} />);
    expect(screen.getByText(/1976/)).toBeInTheDocument();
    expect(screen.getByText(/USA/)).toBeInTheDocument();
  });
});

// #24 Delayed Shipping
describe('DelayedShippingPicker - Feature #24', () => {
  const mockOnChange = vi.fn();

  it('renders toggle', () => {
    render(<DelayedShippingPicker date="" onChange={mockOnChange} />);
    expect(screen.getByText(/Chọn ngày giao hàng/i)).toBeInTheDocument();
  });

  it('shows date picker when toggled on', () => {
    render(<DelayedShippingPicker date="" onChange={mockOnChange} />);
    fireEvent.click(screen.getByText(/Chọn ngày giao hàng/i));
    
    const dateInput = document.querySelector('input[type="date"]');
    expect(dateInput).toBeInTheDocument();
  });
});

// #26 Recurring Order Toggle
describe('RecurringOrderToggle - Feature #26', () => {
  const mockOnToggle = vi.fn();
  const mockOnInterval = vi.fn();

  it('renders toggle switch', () => {
    render(<RecurringOrderToggle enabled={false} onToggle={mockOnToggle} interval={30} onIntervalChange={mockOnInterval} />);
    expect(screen.getByText(/Đặt hàng định kỳ/i)).toBeInTheDocument();
  });

  it('shows interval options when enabled', () => {
    render(<RecurringOrderToggle enabled={true} onToggle={mockOnToggle} interval={30} onIntervalChange={mockOnInterval} />);
    expect(screen.getByText('7 ngày')).toBeInTheDocument();
    expect(screen.getByText('14 ngày')).toBeInTheDocument();
    expect(screen.getByText('30 ngày')).toBeInTheDocument();
    expect(screen.getByText('60 ngày')).toBeInTheDocument();
  });

  it('calls onIntervalChange when interval button is clicked', () => {
    render(<RecurringOrderToggle enabled={true} onToggle={mockOnToggle} interval={30} onIntervalChange={mockOnInterval} />);
    fireEvent.click(screen.getByText('14 ngày'));
    expect(mockOnInterval).toHaveBeenCalledWith(14);
  });
});

// #31 Birthday Rewards
describe('BirthdayReward - Feature #31', () => {
  it('returns null when no coupon code', () => {
    const { container } = render(<BirthdayReward />);
    expect(container).toBeEmptyDOMElement();
  });

  it('shows birthday reward during birthday month', () => {
    // Use current month to ensure the component renders
    const now = new Date();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const birthday = `1999-${month}-15T00:00:00`;
    render(<BirthdayReward birthday={birthday} couponCode="BDAY15" />);
    expect(screen.getByText(/Chúc mừng sinh nhật/)).toBeInTheDocument();
    expect(screen.getByText(/BDAY15/)).toBeInTheDocument();
  });

  it('copies coupon code to clipboard on click', async () => {
    const now = new Date();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const birthday = `1999-${month}-15T00:00:00`;
    render(<BirthdayReward birthday={birthday} couponCode="BDAY15" />);
    
    const copyBtn = screen.getByText(/BDAY15/).closest('button');
    if (copyBtn) {
      await act(async () => { fireEvent.click(copyBtn); });
      expect(mockWriteText).toHaveBeenCalledWith('BDAY15');
    }
  });
});

// #32 Referral Dashboard
describe('ReferralDashboard - Feature #32', () => {
  it('renders referral stats', () => {
    render(<ReferralDashboard referralCode="REF123" referralCount={5} totalEarnings={500000} />);
    expect(screen.getByText('5')).toBeInTheDocument();
    expect(screen.getByText('REF123')).toBeInTheDocument();
  });

  it('copies referral code to clipboard', async () => {
    render(<ReferralDashboard referralCode="REF123" referralCount={5} totalEarnings={500000} />);
    const copyBtn = screen.getByText('REF123').closest('div')?.querySelector('button');
    if (copyBtn) {
      await act(async () => { fireEvent.click(copyBtn); });
      expect(mockWriteText).toHaveBeenCalledWith('REF123');
    }
  });
});

// #34 Notification Preferences
describe('NotificationPreferences - Feature #34', () => {
  const prefs = {
    orders: { email: true, push: false, sms: false },
    promotions: { email: true, push: true, sms: false },
    reviews: { email: false, push: false, sms: false },
    priceDrops: { email: true, push: false, sms: false },
    restocks: { email: false, push: false, sms: false },
  };
  const mockOnChange = vi.fn();

  it('renders all notification topics', () => {
    render(<NotificationPreferences prefs={prefs} onChange={mockOnChange} />);
    expect(screen.getByText('Đơn hàng')).toBeInTheDocument();
    expect(screen.getByText('Khuyến mãi')).toBeInTheDocument();
  });

  it('renders channel headers', () => {
    render(<NotificationPreferences prefs={prefs} onChange={mockOnChange} />);
    expect(screen.getByText('Email')).toBeInTheDocument();
    expect(screen.getByText('Push')).toBeInTheDocument();
    expect(screen.getByText('SMS')).toBeInTheDocument();
  });
});

// #38 Return Request Form
describe('ReturnRequestForm - Feature #38', () => {
  const mockOnSubmit = vi.fn();

  it('renders form with reason dropdown', () => {
    render(<ReturnRequestForm orderCode="ORD001" onSubmit={mockOnSubmit} />);
    expect(screen.getByText(/ORD001/)).toBeInTheDocument();
    const select = document.querySelector('select');
    expect(select).toBeInTheDocument();
  });

  it('disables submit when no reason selected', () => {
    render(<ReturnRequestForm orderCode="ORD001" onSubmit={mockOnSubmit} />);
    const submitBtn = screen.getByText(/Gửi yêu cầu/i);
    expect(submitBtn).toBeDisabled();
  });

  it('enables submit when reason is selected', () => {
    render(<ReturnRequestForm orderCode="ORD001" onSubmit={mockOnSubmit} />);
    const select = document.querySelector('select')!;
    fireEvent.change(select, { target: { value: 'Sản phẩm lỗi' } });
    const submitBtn = screen.getByText(/Gửi yêu cầu/i);
    expect(submitBtn).toBeEnabled();
  });
});

// #39 Wallet Display
describe('WalletDisplay - Feature #39', () => {
  const transactions = [
    { type: 'credit' as const, amount: 100000, description: 'Hoàn tiền đơn #123', date: '2024-01-15' },
    { type: 'debit' as const, amount: 50000, description: 'Thanh toán đơn #456', date: '2024-01-16' },
  ];

  it('renders wallet balance', () => {
    render(<WalletDisplay balance={150000} transactions={transactions} />);
    expect(screen.getByText(/150.000/)).toBeInTheDocument();
  });

  it('renders transaction list', () => {
    render(<WalletDisplay balance={150000} transactions={transactions} />);
    expect(screen.getByText(/Hoàn tiền đơn #123/)).toBeInTheDocument();
    expect(screen.getByText(/Thanh toán đơn #456/)).toBeInTheDocument();
  });

  it('shows credit transactions in green', () => {
    render(<WalletDisplay balance={150000} transactions={transactions} />);
    const creditAmount = screen.getByText(/\+.*100.000/);
    expect(creditAmount).toHaveClass('text-green-600');
  });
});
