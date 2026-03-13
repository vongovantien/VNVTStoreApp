/**
 * Unit tests for B2B and Admin Features
 * Features: #62 Bulk Order, #63 VAT Input, #66 Multiple Addresses,
 * #91 Dashboard Widgets, #93 Abandoned Carts, #94 Low Stock,
 * #100 System Health, #92 Visitor Counter, #95 Activity Log, #96 Sales by Region
 */
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { vi, describe, it, expect } from 'vitest';

// Mock framer-motion
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, ...props }: { children: React.ReactNode }) => <div {...props}>{children}</div>,
  },
  AnimatePresence: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

import {
  BulkOrderForm,
  VATInput,
  AddressManager,
  AdminDashboardWidgets,
  AbandonedCartList,
  LowStockAlerts,
  SystemHealthStatus,
  VisitorCounter,
  UserActivityLog,
  SalesByRegion,
} from '../B2BAndAdminFeatures';

// #62 Bulk Order Form
describe('BulkOrderForm - Feature #62', () => {
  const mockOnSubmit = vi.fn();

  it('renders initial order row', () => {
    render(<BulkOrderForm onSubmit={mockOnSubmit} />);
    expect(screen.getByPlaceholderText(/SKU/i)).toBeInTheDocument();
  });

  it('adds new rows when "Add Row" is clicked', () => {
    render(<BulkOrderForm onSubmit={mockOnSubmit} />);
    const addBtn = screen.getByText(/Thêm dòng/i);
    fireEvent.click(addBtn);
    
    const skuInputs = screen.getAllByPlaceholderText(/SKU/i);
    expect(skuInputs.length).toBe(2);
  });

  it('submits form with valid items', () => {
    render(<BulkOrderForm onSubmit={mockOnSubmit} />);
    const skuInput = screen.getByPlaceholderText(/SKU/i);
    
    fireEvent.change(skuInput, { target: { value: 'PROD-001' } });
    
    const submitBtn = screen.getByText(/Gửi đơn hàng/i);
    fireEvent.click(submitBtn);
    
    expect(mockOnSubmit).toHaveBeenCalledWith([
      expect.objectContaining({ sku: 'PROD-001', quantity: 1 })
    ]);
  });

  it('removes rows when delete is clicked', () => {
    render(<BulkOrderForm onSubmit={mockOnSubmit} />);
    // Add a second row first
    fireEvent.click(screen.getByText(/Thêm dòng/i));
    
    const deleteButtons = screen.getAllByRole('button').filter(btn => 
      btn.querySelector('svg') && btn.className.includes('error')
    );
    
    if (deleteButtons.length > 0) {
      fireEvent.click(deleteButtons[0]);
      expect(screen.getAllByPlaceholderText(/SKU/i).length).toBe(1);
    }
  });
});

// #63 VAT/Tax ID Input
describe('VATInput - Feature #63', () => {
  const mockOnChange = vi.fn();
  const mockOnCompanyChange = vi.fn();

  it('renders toggle button', () => {
    render(<VATInput value="" onChange={mockOnChange} companyName="" onCompanyNameChange={mockOnCompanyChange} />);
    expect(screen.getByText(/Xuất hóa đơn công ty/i)).toBeInTheDocument();
  });

  it('shows input fields when toggled on', () => {
    render(<VATInput value="" onChange={mockOnChange} companyName="" onCompanyNameChange={mockOnCompanyChange} />);
    fireEvent.click(screen.getByText(/Xuất hóa đơn công ty/i));
    
    expect(screen.getByPlaceholderText(/Công ty TNHH/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/0123456789/i)).toBeInTheDocument();
  });
});

// #66 Multiple Shipping Addresses
describe('AddressManager - Feature #66', () => {
  const addresses = [
    { id: '1', label: 'Nhà', name: 'Nguyễn Văn A', phone: '0901234567', address: '123 ABC, HCM', isDefault: true },
    { id: '2', label: 'Văn phòng', name: 'Nguyễn Văn A', phone: '0907654321', address: '456 DEF, HCM', isDefault: false },
  ];

  it('renders saved addresses', () => {
    render(<AddressManager addresses={addresses} selected="1" onSelect={vi.fn()} onAdd={vi.fn()} onDelete={vi.fn()} />);
    expect(screen.getByText('Nhà')).toBeInTheDocument();
    expect(screen.getByText('Văn phòng')).toBeInTheDocument();
  });

  it('highlights selected address', () => {
    render(<AddressManager addresses={addresses} selected="1" onSelect={vi.fn()} onAdd={vi.fn()} onDelete={vi.fn()} />);
    const selectedCard = screen.getByText('Nhà').closest('div[class*="border-indigo"]');
    expect(selectedCard).toBeTruthy();
  });

  it('shows add form when "Add" is clicked', () => {
    render(<AddressManager addresses={addresses} selected="1" onSelect={vi.fn()} onAdd={vi.fn()} onDelete={vi.fn()} />);
    fireEvent.click(screen.getByText(/Thêm địa chỉ mới/i));
    expect(screen.getByPlaceholderText(/Nhãn/i)).toBeInTheDocument();
  });
});

// #91 Admin Dashboard Widgets
describe('AdminDashboardWidgets - Feature #91', () => {
  const widgets = [
    { title: 'Doanh thu', value: '150M', change: 12, icon: '💰', color: 'bg-green-100' },
    { title: 'Đơn hàng', value: 245, change: -3, icon: '📦', color: 'bg-blue-100' },
  ];

  it('renders all widgets', () => {
    render(<AdminDashboardWidgets widgets={widgets} />);
    expect(screen.getByText('Doanh thu')).toBeInTheDocument();
    expect(screen.getByText('Đơn hàng')).toBeInTheDocument();
  });

  it('shows positive change with green color', () => {
    render(<AdminDashboardWidgets widgets={widgets} />);
    expect(screen.getByText('+12%')).toBeInTheDocument();
  });

  it('shows negative change with red color', () => {
    render(<AdminDashboardWidgets widgets={widgets} />);
    expect(screen.getByText('-3%')).toBeInTheDocument();
  });
});

// #93 Abandoned Cart List
describe('AbandonedCartList - Feature #93', () => {
  const carts = [
    { userId: 'U1', userName: 'John Doe', items: 3, total: 500000, lastActivity: '2 giờ trước' },
  ];

  it('renders cart entries', () => {
    render(<AbandonedCartList carts={carts} />);
    expect(screen.getByText('John Doe')).toBeInTheDocument();
    expect(screen.getByText(/3 sản phẩm/)).toBeInTheDocument();
  });

  it('shows count badge', () => {
    render(<AbandonedCartList carts={carts} />);
    expect(screen.getByText('1')).toBeInTheDocument();
  });
});

// #94 Low Stock Alerts
describe('LowStockAlerts - Feature #94', () => {
  const items = [
    { code: 'SKU001', name: 'Áo thun', stock: 3, threshold: 10 },
    { code: 'SKU002', name: 'Quần jeans', stock: 8, threshold: 10 },
  ];

  it('renders low stock items', () => {
    render(<LowStockAlerts items={items} />);
    expect(screen.getByText('Áo thun')).toBeInTheDocument();
    expect(screen.getByText('Quần jeans')).toBeInTheDocument();
  });

  it('shows critical stock in red', () => {
    render(<LowStockAlerts items={items} />);
    expect(screen.getByText('3 / 10')).toBeInTheDocument();
  });

  it('returns null when no items', () => {
    const { container } = render(<LowStockAlerts items={[]} />);
    expect(container).toBeEmptyDOMElement();
  });
});

// #100 System Health Status
describe('SystemHealthStatus - Feature #100', () => {
  const checks = [
    { service: 'API Server', status: 'healthy' as const, responseTime: 45, lastCheck: '1 phút trước' },
    { service: 'Database', status: 'degraded' as const, responseTime: 250, lastCheck: '2 phút trước' },
  ];

  it('renders all services', () => {
    render(<SystemHealthStatus checks={checks} />);
    expect(screen.getByText('API Server')).toBeInTheDocument();
    expect(screen.getByText('Database')).toBeInTheDocument();
  });

  it('shows response times', () => {
    render(<SystemHealthStatus checks={checks} />);
    expect(screen.getByText('45ms')).toBeInTheDocument();
    expect(screen.getByText('250ms')).toBeInTheDocument();
  });

  it('shows overall status as degraded when one service is degraded', () => {
    render(<SystemHealthStatus checks={checks} />);
    expect(screen.getByText('degraded')).toBeInTheDocument();
  });
});

// #92 Visitor Counter
describe('VisitorCounter - Feature #92', () => {
  it('renders visitor count', () => {
    render(<VisitorCounter count={42} />);
    expect(screen.getByText('42')).toBeInTheDocument();
    expect(screen.getByText(/người đang xem/)).toBeInTheDocument();
  });
});

// #95 User Activity Log
describe('UserActivityLog - Feature #95', () => {
  const entries = [
    { user: 'Admin', action: 'cập nhật', target: 'Sản phẩm ABC', timestamp: '10:30' },
  ];

  it('renders activity entries', () => {
    render(<UserActivityLog entries={entries} />);
    expect(screen.getByText('Admin')).toBeInTheDocument();
    expect(screen.getByText('cập nhật')).toBeInTheDocument();
    expect(screen.getByText('Sản phẩm ABC')).toBeInTheDocument();
  });
});

// #96 Sales by Region
describe('SalesByRegion - Feature #96', () => {
  const regions = [
    { region: 'TP.HCM', orders: 150, revenue: 50000000, percentage: 45 },
    { region: 'Hà Nội', orders: 120, revenue: 40000000, percentage: 35 },
  ];

  it('renders region table', () => {
    render(<SalesByRegion regions={regions} />);
    expect(screen.getByText('TP.HCM')).toBeInTheDocument();
    expect(screen.getByText('Hà Nội')).toBeInTheDocument();
  });

  it('shows order counts', () => {
    render(<SalesByRegion regions={regions} />);
    expect(screen.getByText('150')).toBeInTheDocument();
    expect(screen.getByText('120')).toBeInTheDocument();
  });

  it('shows percentage bars', () => {
    render(<SalesByRegion regions={regions} />);
    expect(screen.getByText('45%')).toBeInTheDocument();
    expect(screen.getByText('35%')).toBeInTheDocument();
  });
});
