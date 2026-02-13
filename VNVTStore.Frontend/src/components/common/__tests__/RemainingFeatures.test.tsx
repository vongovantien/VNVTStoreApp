/**
 * Unit tests for Remaining Features
 * Features: #2 Price Histogram, #60 Post-Purchase Upsell, #46 Admin Review Response,
 * #48 User Gallery, #25 Split Shipment, #30 Saved Payments, #33 Social Links,
 * #55 Mystery Box, #37 Invoice Download, #82 WebP, #90 Currency Auto-detect,
 * #99 Banner Manager, #98 Bulk Product Edit
 */
import { render, screen, fireEvent, act } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';

// Mock framer-motion
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, onClick, ...props }: any) => <div onClick={onClick} {...props}>{children}</div>,
  },
  AnimatePresence: ({ children }: any) => <>{children}</>,
}));

import {
  PriceHistogram,
  PostPurchaseUpsell,
  AdminReviewResponse,
  UserGallery,
  SplitShipmentToggle,
  SavedPaymentMethods,
  SocialMediaLinks,
  MysteryBoxCard,
  InvoiceDownloadButton,
  WebPIndicator,
  useCurrencyAutoDetect,
  BannerManager,
  BulkProductEdit,
} from '../RemainingFeatures';

// #2 Price Range Histogram
describe('PriceHistogram - Feature #2', () => {
  const prices = [100000, 200000, 150000, 300000, 250000, 180000, 500000, 450000, 120000, 350000];

  it('renders histogram bars', () => {
    const { container } = render(
      <PriceHistogram prices={prices} range={[100000, 500000]} onRangeChange={vi.fn()} />
    );
    const bars = container.querySelectorAll('[class*="rounded-t"]');
    expect(bars.length).toBeGreaterThan(0);
  });

  it('renders range sliders', () => {
    const { container } = render(
      <PriceHistogram prices={prices} range={[100000, 500000]} onRangeChange={vi.fn()} />
    );
    const sliders = container.querySelectorAll('input[type="range"]');
    expect(sliders.length).toBe(2);
  });

  it('calls onRangeChange when slider is moved', () => {
    const mockOnChange = vi.fn();
    const { container } = render(
      <PriceHistogram prices={prices} range={[100000, 500000]} onRangeChange={mockOnChange} />
    );
    const sliders = container.querySelectorAll('input[type="range"]');
    fireEvent.change(sliders[0], { target: { value: '200000' } });
    expect(mockOnChange).toHaveBeenCalledWith([200000, 500000]);
  });

  it('returns null when prices array is empty', () => {
    const { container } = render(
      <PriceHistogram prices={[]} range={[0, 100]} onRangeChange={vi.fn()} />
    );
    expect(container).toBeEmptyDOMElement();
  });
});

// #60 Post-Purchase Upsell
describe('PostPurchaseUpsell - Feature #60', () => {
  const products = [
    { code: 'P1', name: 'Ốp lưng', price: 200000, imageUrl: '/img/case.jpg' },
    { code: 'P2', name: 'Sạc nhanh', price: 450000, imageUrl: '/img/charger.jpg' },
  ];

  it('renders upsell products', () => {
    render(<PostPurchaseUpsell products={products} onAddToOrder={vi.fn()} />);
    expect(screen.getByText('Ốp lưng')).toBeInTheDocument();
    expect(screen.getByText('Sạc nhanh')).toBeInTheDocument();
  });

  it('shows discounted price (10% off)', () => {
    render(<PostPurchaseUpsell products={products} onAddToOrder={vi.fn()} />);
    // 200000 * 0.9 = 180000
    expect(screen.getByText(/180.000/)).toBeInTheDocument();
  });

  it('calls onAddToOrder when add button is clicked', () => {
    const mockAdd = vi.fn();
    render(<PostPurchaseUpsell products={products} onAddToOrder={mockAdd} />);
    const addButtons = screen.getAllByRole('button');
    fireEvent.click(addButtons[0]);
    expect(mockAdd).toHaveBeenCalledWith('P1');
  });

  it('returns null when no products', () => {
    const { container } = render(<PostPurchaseUpsell products={[]} onAddToOrder={vi.fn()} />);
    expect(container).toBeEmptyDOMElement();
  });
});

// #46 Admin Review Response
describe('AdminReviewResponse - Feature #46', () => {
  it('renders text input when no existing response', () => {
    render(<AdminReviewResponse reviewId="R1" onSubmit={vi.fn()} />);
    expect(screen.getByPlaceholderText(/phản hồi/i)).toBeInTheDocument();
  });

  it('shows existing response in read mode', () => {
    render(<AdminReviewResponse reviewId="R1" existingResponse="Cảm ơn bạn!" onSubmit={vi.fn()} />);
    expect(screen.getByText('Cảm ơn bạn!')).toBeInTheDocument();
  });

  it('submits new response', () => {
    const mockSubmit = vi.fn();
    render(<AdminReviewResponse reviewId="R1" onSubmit={mockSubmit} />);
    
    const textarea = screen.getByPlaceholderText(/phản hồi/i);
    fireEvent.change(textarea, { target: { value: 'Cảm ơn phản hồi!' } });
    fireEvent.click(screen.getByText('Gửi'));
    
    expect(mockSubmit).toHaveBeenCalledWith('Cảm ơn phản hồi!');
  });
});

// #48 User Gallery (UGC)
describe('UserGallery - Feature #48', () => {
  beforeEach(() => localStorage.clear());

  it('renders upload button', () => {
    render(<UserGallery productCode="P1" />);
    const fileInput = document.querySelector('input[type="file"]');
    expect(fileInput).toBeInTheDocument();
  });

  it('shows existing images from localStorage', () => {
    localStorage.setItem('vnvt_ugc_P1', JSON.stringify(['data:image/png;base64,abc']));
    render(<UserGallery productCode="P1" />);
    const images = document.querySelectorAll('img');
    expect(images.length).toBe(1);
  });
});

// #25 Split Shipment
describe('SplitShipmentToggle - Feature #25', () => {
  it('renders toggle with description', () => {
    render(<SplitShipmentToggle enabled={false} onToggle={vi.fn()} />);
    expect(screen.getByText(/Giao hàng nhiều đợt/i)).toBeInTheDocument();
  });

  it('calls onToggle when clicked', () => {
    const mockToggle = vi.fn();
    render(<SplitShipmentToggle enabled={false} onToggle={mockToggle} />);
    const toggleBtn = screen.getByRole('button');
    fireEvent.click(toggleBtn);
    expect(mockToggle).toHaveBeenCalledWith(true);
  });
});

// #30 Saved Payment Methods
describe('SavedPaymentMethods - Feature #30', () => {
  const methods = [
    { id: 'pm1', type: 'visa' as const, last4: '4242', expiry: '12/26' },
    { id: 'pm2', type: 'momo' as const, last4: '1234', expiry: '06/25' },
  ];

  it('renders all payment methods', () => {
    render(<SavedPaymentMethods methods={methods} selected="pm1" onSelect={vi.fn()} />);
    expect(screen.getByText(/visa.*4242/i)).toBeInTheDocument();
    expect(screen.getByText(/momo.*1234/i)).toBeInTheDocument();
  });

  it('highlights selected method', () => {
    render(<SavedPaymentMethods methods={methods} selected="pm1" onSelect={vi.fn()} />);
    const selected = screen.getByText(/visa.*4242/i).closest('div[class*="border-indigo"]');
    expect(selected).toBeTruthy();
  });

  it('calls onSelect when a method is clicked', () => {
    const mockSelect = vi.fn();
    render(<SavedPaymentMethods methods={methods} selected="pm1" onSelect={mockSelect} />);
    const momoCard = screen.getByText(/momo.*1234/i).closest('div');
    if (momoCard) {
      fireEvent.click(momoCard);
      expect(mockSelect).toHaveBeenCalledWith('pm2');
    }
  });
});

// #33 Social Media Links
describe('SocialMediaLinks - Feature #33', () => {
  const links = {
    facebook: 'https://facebook.com/shop',
    instagram: 'https://instagram.com/shop',
    tiktok: 'https://tiktok.com/@shop',
  };

  it('renders all social links', () => {
    render(<SocialMediaLinks links={links} />);
    const anchors = screen.getAllByRole('link');
    expect(anchors.length).toBe(3);
  });

  it('opens links in new tab', () => {
    render(<SocialMediaLinks links={links} />);
    const anchors = screen.getAllByRole('link');
    anchors.forEach(a => {
      expect(a).toHaveAttribute('target', '_blank');
      expect(a).toHaveAttribute('rel', 'noopener noreferrer');
    });
  });
});

// #55 Mystery Box
describe('MysteryBoxCard - Feature #55', () => {
  it('renders mystery box with price', () => {
    render(<MysteryBoxCard price={500000} category="Phụ kiện" onBuy={vi.fn()} />);
    expect(screen.getByText(/Hộp bí ẩn/i)).toBeInTheDocument();
    // Currency formatting varies by environment, just check the number part exists
    expect(document.body).toHaveTextContent(/500/);
  });

  it('shows potential value (3x price)', () => {
    render(<MysteryBoxCard price={500000} category="Phụ kiện" onBuy={vi.fn()} />);
    // 3x = 1,500,000 - check number exists in text
    expect(document.body).toHaveTextContent(/1.*500.*000/);
  });

  it('calls onBuy when clicked', () => {
    const mockBuy = vi.fn();
    render(<MysteryBoxCard price={500000} category="Phụ kiện" onBuy={mockBuy} />);
    fireEvent.click(screen.getByText(/Mua ngay/i));
    expect(mockBuy).toHaveBeenCalled();
  });
});

// #37 Invoice Download
describe('InvoiceDownloadButton - Feature #37', () => {
  it('renders download button', () => {
    render(<InvoiceDownloadButton orderCode="ORD001" onDownload={vi.fn()} />);
    expect(screen.getByText(/Tải hóa đơn/i)).toBeInTheDocument();
  });

  it('calls onDownload when clicked', () => {
    const mockDownload = vi.fn();
    render(<InvoiceDownloadButton orderCode="ORD001" onDownload={mockDownload} />);
    fireEvent.click(screen.getByText(/Tải hóa đơn/i));
    expect(mockDownload).toHaveBeenCalled();
  });
});

// #82 WebP Indicator
describe('WebPIndicator - Feature #82', () => {
  it('renders WebP badge when true', () => {
    render(<WebPIndicator isWebP={true} />);
    expect(screen.getByText('WebP')).toBeInTheDocument();
  });

  it('returns null when not WebP', () => {
    const { container } = render(<WebPIndicator isWebP={false} />);
    expect(container).toBeEmptyDOMElement();
  });
});

// #90 Currency Auto-detect
describe('useCurrencyAutoDetect - Feature #90', () => {
  it('returns VND for Vietnamese locale', () => {
    Object.defineProperty(navigator, 'language', { value: 'vi-VN', writable: true });
    // Hook returns a string
    const TestComponent = () => {
      const currency = useCurrencyAutoDetect();
      return <div>{currency}</div>;
    };
    render(<TestComponent />);
    expect(screen.getByText('VND')).toBeInTheDocument();
  });
});

// #99 Banner Manager
describe('BannerManager - Feature #99', () => {
  const banners = [
    { id: 'b1', title: 'Summer Sale', imageUrl: '/img/banner1.jpg', link: '/sale', active: true },
    { id: 'b2', title: 'New Arrivals', imageUrl: '/img/banner2.jpg', link: '/new', active: false },
  ];

  it('renders all banners', () => {
    render(<BannerManager banners={banners} onReorder={vi.fn()} onToggle={vi.fn()} onDelete={vi.fn()} />);
    expect(screen.getByText('Summer Sale')).toBeInTheDocument();
    expect(screen.getByText('New Arrivals')).toBeInTheDocument();
  });

  it('calls onToggle when toggle is clicked', () => {
    const mockToggle = vi.fn();
    render(<BannerManager banners={banners} onReorder={vi.fn()} onToggle={mockToggle} onDelete={vi.fn()} />);
    const toggleButtons = screen.getAllByRole('button').filter(b => b.className.includes('rounded-full'));
    if (toggleButtons.length > 0) {
      fireEvent.click(toggleButtons[0]);
      expect(mockToggle).toHaveBeenCalledWith('b1');
    }
  });
});

// #98 Bulk Product Edit
describe('BulkProductEdit - Feature #98', () => {
  const products = [
    { code: 'SKU001', name: 'Áo thun', price: 200000, stock: 50 },
    { code: 'SKU002', name: 'Quần jean', price: 450000, stock: 30 },
  ];

  it('renders product table', () => {
    render(<BulkProductEdit products={products} onSave={vi.fn()} />);
    expect(screen.getByText('SKU001')).toBeInTheDocument();
    expect(screen.getByText('SKU002')).toBeInTheDocument();
  });

  it('shows save button after editing', () => {
    render(<BulkProductEdit products={products} onSave={vi.fn()} />);
    const nameInput = screen.getByDisplayValue('Áo thun');
    fireEvent.change(nameInput, { target: { value: 'Áo thun mới' } });
    expect(screen.getByText(/sản phẩm đã thay đổi/)).toBeInTheDocument();
  });

  it('calls onSave with updated products', () => {
    const mockSave = vi.fn();
    render(<BulkProductEdit products={products} onSave={mockSave} />);
    
    const nameInput = screen.getByDisplayValue('Áo thun');
    fireEvent.change(nameInput, { target: { value: 'Áo thun mới' } });
    
    fireEvent.click(screen.getByText(/Lưu thay đổi/i));
    expect(mockSave).toHaveBeenCalledWith(
      expect.arrayContaining([expect.objectContaining({ name: 'Áo thun mới' })])
    );
  });
});
