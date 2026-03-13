/**
 * B2B & Wholesale Features:
 * #62: Bulk Order Form (CSV-like quick entry)
 * #63: VAT/Tax ID Input
 * #65: Wholesale Login Gate
 * #66: Multiple Shipping Addresses
 * #68: Net 30 Payment Terms
 * #69: Quick Reorder from SKU List
 * #70: Download Catalog PDF-like view
 * #67: Sales Rep Assignment display
 * 
 * Admin Features:
 * #91: Admin Dashboard Widgets
 * #93: Abandoned Cart List
 * #94: Low Stock Alerts
 * #98: Bulk Product Edit
 * #99: Banner Manager
 * #100: System Health Status
 * #92: Real-time Visitor Counter display
 * #95: User Activity Log
 * #96: Sales Heatmap (table format)
 */

import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  Upload, Building2, MapPin, Plus, Trash2, Clock, 
  Users, AlertTriangle, Activity, 
  ShoppingCart, Eye, Globe, Check 
} from 'lucide-react';

// ============ #62 Bulk Order Form ============
interface BulkOrderItem {
  sku: string;
  quantity: number;
}

export const BulkOrderForm = ({ onSubmit }: { onSubmit: (items: BulkOrderItem[]) => void }) => {
  const { t } = useTranslation();
  const [items, setItems] = useState<BulkOrderItem[]>([{ sku: '', quantity: 1 }]);

  const addRow = () => setItems([...items, { sku: '', quantity: 1 }]);
  const removeRow = (idx: number) => setItems(items.filter((_, i) => i !== idx));
  const updateRow = (idx: number, field: keyof BulkOrderItem, value: string | number) => {
    const updated = [...items];
    const item = updated[idx];
    if (field === 'sku') {
      item.sku = value as string;
    } else if (field === 'quantity') {
      item.quantity = value as number;
    }
    setItems(updated);
  };

  const handlePaste = (e: React.ClipboardEvent) => {
    const text = e.clipboardData.getData('text');
    const lines = text.split('\n').filter(l => l.trim());
    if (lines.length > 1) {
      e.preventDefault();
      const parsed = lines.map(line => {
        const [sku, qty] = line.split(/[,\t]/);
        return { sku: sku?.trim() || '', quantity: parseInt(qty) || 1 };
      }).filter(i => i.sku);
      setItems(parsed);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="font-bold flex items-center gap-2">
          <Upload size={18} /> {t('b2b.bulkOrder', 'Đặt hàng số lượng lớn')}
        </h3>
        <span className="text-xs text-tertiary">{t('b2b.pasteTip', 'Tip: Dán danh sách SKU từ CSV')}</span>
      </div>
      
      <div className="space-y-2">
        {items.map((item, idx) => (
          <div key={idx} className="flex gap-2">
            <input
              value={item.sku}
              onChange={(e) => updateRow(idx, 'sku', e.target.value)}
              onPaste={idx === 0 ? handlePaste : undefined}
              placeholder="Mã SKU"
              className="flex-1 px-3 py-2 bg-secondary border rounded-lg text-sm focus:outline-none focus:border-indigo-500"
            />
            <input
              type="number"
              value={item.quantity}
              onChange={(e) => updateRow(idx, 'quantity', parseInt(e.target.value) || 1)}
              min={1}
              className="w-20 px-3 py-2 bg-secondary border rounded-lg text-sm text-center focus:outline-none focus:border-indigo-500"
            />
            {items.length > 1 && (
              <button onClick={() => removeRow(idx)} className="p-2 text-error hover:bg-error/10 rounded-lg">
                <Trash2 size={16} />
              </button>
            )}
          </div>
        ))}
      </div>
      
      <div className="flex gap-2">
        <button onClick={addRow} className="flex items-center gap-1 px-3 py-2 text-sm text-indigo-600 hover:bg-indigo-50 rounded-lg">
          <Plus size={14} /> {t('b2b.addRow', 'Thêm dòng')}
        </button>
        <button 
          onClick={() => onSubmit(items.filter(i => i.sku.trim()))} 
          className="ml-auto px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700"
        >
          {t('b2b.submit', 'Gửi đơn hàng')}
        </button>
      </div>
    </div>
  );
};

// ============ #63 VAT/Tax ID Input ============
export const VATInput = ({ 
  value, 
  onChange,
  companyName,
  onCompanyNameChange 
}: { 
  value: string; 
  onChange: (v: string) => void;
  companyName: string;
  onCompanyNameChange: (v: string) => void;
}) => {
  const { t } = useTranslation();
  const [isCompany, setIsCompany] = useState(!!value);

  return (
    <div className="border rounded-xl overflow-hidden">
      <button
        onClick={() => setIsCompany(!isCompany)}
        className="w-full flex items-center gap-3 p-4 hover:bg-hover transition-colors"
      >
        <div className={`w-5 h-5 rounded border-2 flex items-center justify-center ${isCompany ? 'bg-indigo-600 border-indigo-600' : 'border-gray-300'}`}>
          {isCompany && <Check size={12} className="text-white" />}
        </div>
        <Building2 size={18} className="text-indigo-500" />
        <span className="text-sm font-medium">{t('checkout.companyInvoice', 'Xuất hóa đơn công ty')}</span>
      </button>
      <AnimatePresence>
        {isCompany && (
          <motion.div initial={{ height: 0 }} animate={{ height: 'auto' }} exit={{ height: 0 }} className="overflow-hidden">
            <div className="p-4 pt-0 space-y-3">
              <div>
                <label className="text-xs text-tertiary mb-1 block">{t('checkout.companyName', 'Tên công ty')}</label>
                <input value={companyName} onChange={(e) => onCompanyNameChange(e.target.value)} className="w-full px-3 py-2 bg-secondary border rounded-lg text-sm focus:outline-none focus:border-indigo-500" placeholder="Công ty TNHH..." />
              </div>
              <div>
                <label className="text-xs text-tertiary mb-1 block">{t('checkout.vatId', 'Mã số thuế')}</label>
                <input value={value} onChange={(e) => onChange(e.target.value)} className="w-full px-3 py-2 bg-secondary border rounded-lg text-sm focus:outline-none focus:border-indigo-500" placeholder="0123456789" />
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

// ============ #66 Multiple Shipping Addresses ============
interface SavedAddress {
  id: string;
  label: string;
  name: string;
  phone: string;
  address: string;
  isDefault: boolean;
}

export const AddressManager = ({ 
  addresses, 
  selected, 
  onSelect,
  onAdd,
  onDelete 
}: { 
  addresses: SavedAddress[];
  selected: string;
  onSelect: (id: string) => void;
  onAdd: (addr: Omit<SavedAddress, 'id'>) => void;
  onDelete: (id: string) => void;
}) => {
  const { t } = useTranslation();
  const [showForm, setShowForm] = useState(false);
  const [newAddr, setNewAddr] = useState({ label: '', name: '', phone: '', address: '', isDefault: false });

  return (
    <div className="space-y-3">
      <h4 className="font-semibold flex items-center gap-2">
        <MapPin size={16} /> {t('checkout.savedAddresses', 'Địa chỉ đã lưu')}
      </h4>
      {addresses.map((addr) => (
        <div
          key={addr.id}
          onClick={() => onSelect(addr.id)}
          className={`p-3 rounded-xl border-2 cursor-pointer transition-all ${
            selected === addr.id ? 'border-indigo-500 bg-indigo-50/50' : 'border-transparent bg-secondary hover:border-gray-300'
          }`}
        >
          <div className="flex items-center justify-between mb-1">
            <span className="text-sm font-semibold">{addr.label} {addr.isDefault && <span className="text-xs text-indigo-600">(Mặc định)</span>}</span>
            <button onClick={(e) => { e.stopPropagation(); onDelete(addr.id); }} className="text-tertiary hover:text-error">
              <Trash2 size={14} />
            </button>
          </div>
          <p className="text-sm text-secondary">{addr.name} • {addr.phone}</p>
          <p className="text-xs text-tertiary mt-0.5">{addr.address}</p>
        </div>
      ))}
      {!showForm ? (
        <button onClick={() => setShowForm(true)} className="flex items-center gap-1.5 text-sm text-indigo-600 hover:underline">
          <Plus size={14} /> {t('checkout.addAddress', 'Thêm địa chỉ mới')}
        </button>
      ) : (
        <div className="p-4 bg-secondary rounded-xl space-y-3">
          <input value={newAddr.label} onChange={(e) => setNewAddr({ ...newAddr, label: e.target.value })} placeholder="Nhãn (VD: Nhà, Văn phòng)" className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:border-indigo-500" />
          <div className="grid grid-cols-2 gap-3">
            <input value={newAddr.name} onChange={(e) => setNewAddr({ ...newAddr, name: e.target.value })} placeholder="Họ tên" className="px-3 py-2 border rounded-lg text-sm focus:outline-none focus:border-indigo-500" />
            <input value={newAddr.phone} onChange={(e) => setNewAddr({ ...newAddr, phone: e.target.value })} placeholder="Số điện thoại" className="px-3 py-2 border rounded-lg text-sm focus:outline-none focus:border-indigo-500" />
          </div>
          <input value={newAddr.address} onChange={(e) => setNewAddr({ ...newAddr, address: e.target.value })} placeholder="Địa chỉ đầy đủ" className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:border-indigo-500" />
          <div className="flex gap-2">
            <button onClick={() => { onAdd(newAddr); setShowForm(false); setNewAddr({ label: '', name: '', phone: '', address: '', isDefault: false }); }} className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium">Lưu</button>
            <button onClick={() => setShowForm(false)} className="px-4 py-2 text-tertiary text-sm">Hủy</button>
          </div>
        </div>
      )}
    </div>
  );
};

// ============ #91 Admin Dashboard Widget ============
interface DashboardWidget {
  title: string;
  value: string | number;
  change?: number;
  icon: React.ReactNode;
  color: string;
}

export const AdminDashboardWidgets = ({ widgets }: { widgets: DashboardWidget[] }) => (
  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
    {widgets.map((w, i) => (
      <motion.div
        key={i}
        initial={{ y: 20, opacity: 0 }}
        animate={{ y: 0, opacity: 1 }}
        transition={{ delay: i * 0.1 }}
        className="bg-primary rounded-xl border p-5 hover:shadow-lg transition-shadow"
      >
        <div className="flex items-start justify-between mb-3">
          <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${w.color}`}>
            {w.icon}
          </div>
          {w.change !== undefined && (
            <span className={`text-xs font-semibold px-2 py-0.5 rounded-full ${
              w.change >= 0 ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
            }`}>
              {w.change >= 0 ? '+' : ''}{w.change}%
            </span>
          )}
        </div>
        <div className="text-2xl font-bold">{w.value}</div>
        <div className="text-xs text-tertiary mt-1">{w.title}</div>
      </motion.div>
    ))}
  </div>
);

// ============ #93 Abandoned Cart List (Admin) ============
interface AbandonedCart {
  userId: string;
  userName: string;
  items: number;
  total: number;
  lastActivity: string;
}

export const AbandonedCartList = ({ carts }: { carts: AbandonedCart[] }) => {
  const { t } = useTranslation();
  const formatPrice = (p: number) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p);

  return (
    <div className="bg-primary rounded-xl border overflow-hidden">
      <div className="p-4 border-b flex items-center justify-between">
        <h3 className="font-bold flex items-center gap-2">
          <ShoppingCart size={18} /> {t('admin.abandonedCarts', 'Giỏ hàng bị bỏ')}
        </h3>
        <span className="text-xs bg-amber-100 text-amber-700 px-2 py-0.5 rounded-full font-medium">{carts.length}</span>
      </div>
      <div className="divide-y">
        {carts.map((cart) => (
          <div key={cart.userId} className="p-4 flex items-center justify-between hover:bg-hover transition-colors">
            <div>
              <p className="text-sm font-medium">{cart.userName}</p>
              <p className="text-xs text-tertiary">{cart.items} sản phẩm • {formatPrice(cart.total)}</p>
            </div>
            <div className="text-right">
              <p className="text-xs text-tertiary flex items-center gap-1"><Clock size={12} /> {cart.lastActivity}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

// ============ #94 Low Stock Alerts ============
interface LowStockItem {
  code: string;
  name: string;
  stock: number;
  threshold: number;
}

export const LowStockAlerts = ({ items }: { items: LowStockItem[] }) => {
  const { t } = useTranslation();
  if (items.length === 0) return null;

  return (
    <div className="bg-primary rounded-xl border overflow-hidden">
      <div className="p-4 border-b flex items-center justify-between">
        <h3 className="font-bold flex items-center gap-2 text-amber-600">
          <AlertTriangle size={18} /> {t('admin.lowStock', 'Sắp hết hàng')}
        </h3>
        <span className="text-xs bg-amber-100 text-amber-700 px-2 py-0.5 rounded-full font-medium">{items.length}</span>
      </div>
      <div className="divide-y">
        {items.map((item) => (
          <div key={item.code} className="p-4 flex items-center justify-between hover:bg-hover transition-colors">
            <div>
              <p className="text-sm font-medium">{item.name}</p>
              <p className="text-xs text-tertiary">SKU: {item.code}</p>
            </div>
            <div className="text-right">
              <span className={`text-sm font-bold ${item.stock <= 5 ? 'text-error' : 'text-amber-600'}`}>
                {item.stock} / {item.threshold}
              </span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

// ============ #100 System Health Status ============
interface HealthCheck {
  service: string;
  status: 'healthy' | 'degraded' | 'down';
  responseTime?: number;
  lastCheck: string;
}

export const SystemHealthStatus = ({ checks }: { checks: HealthCheck[] }) => {
  const statusColors = {
    healthy: 'bg-green-500',
    degraded: 'bg-amber-500',
    down: 'bg-red-500',
  };

  const overallStatus = checks.every(c => c.status === 'healthy')
    ? 'healthy'
    : checks.some(c => c.status === 'down')
    ? 'down'
    : 'degraded';

  return (
    <div className="bg-primary rounded-xl border overflow-hidden">
      <div className="p-4 border-b flex items-center justify-between">
        <h3 className="font-bold flex items-center gap-2">
          <Activity size={18} /> Trạng thái hệ thống
        </h3>
        <div className="flex items-center gap-2">
          <div className={`w-2.5 h-2.5 rounded-full ${statusColors[overallStatus]} animate-pulse`} />
          <span className="text-xs font-medium capitalize">{overallStatus}</span>
        </div>
      </div>
      <div className="divide-y">
        {checks.map((check) => (
          <div key={check.service} className="p-4 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className={`w-2 h-2 rounded-full ${statusColors[check.status]}`} />
              <span className="text-sm font-medium">{check.service}</span>
            </div>
            <div className="flex items-center gap-4">
              {check.responseTime && (
                <span className="text-xs text-tertiary">{check.responseTime}ms</span>
              )}
              <span className="text-xs text-tertiary">{check.lastCheck}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

// ============ #92 Real-time Visitor Counter ============
export const VisitorCounter = ({ count }: { count: number }) => (
  <div className="flex items-center gap-2 text-xs">
    <div className="flex items-center gap-1.5">
      <div className="w-2 h-2 rounded-full bg-green-500 animate-pulse" />
      <Eye size={12} className="text-tertiary" />
    </div>
    <span className="text-tertiary">
      <span className="font-semibold text-primary">{count}</span> người đang xem
    </span>
  </div>
);

// ============ #95 User Activity Log ============
interface ActivityEntry {
  user: string;
  action: string;
  target: string;
  timestamp: string;
}

export const UserActivityLog = ({ entries }: { entries: ActivityEntry[] }) => (
  <div className="bg-primary rounded-xl border overflow-hidden">
    <div className="p-4 border-b">
      <h3 className="font-bold flex items-center gap-2">
        <Users size={18} /> Nhật ký hoạt động
      </h3>
    </div>
    <div className="divide-y max-h-80 overflow-y-auto">
      {entries.map((entry, i) => (
        <div key={i} className="p-3 flex items-start gap-3 hover:bg-hover transition-colors">
          <div className="w-8 h-8 bg-indigo-100 rounded-full flex items-center justify-center shrink-0 text-xs font-bold text-indigo-600">
            {entry.user.charAt(0).toUpperCase()}
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm">
              <span className="font-medium">{entry.user}</span>{' '}
              <span className="text-tertiary">{entry.action}</span>{' '}
              <span className="font-medium text-accent-primary">{entry.target}</span>
            </p>
            <p className="text-xs text-tertiary mt-0.5">{entry.timestamp}</p>
          </div>
        </div>
      ))}
    </div>
  </div>
);

// ============ #96 Sales by Region (Table) ============
interface RegionSale {
  region: string;
  orders: number;
  revenue: number;
  percentage: number;
}

export const SalesByRegion = ({ regions }: { regions: RegionSale[] }) => {
  const formatPrice = (p: number) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p);

  return (
    <div className="bg-primary rounded-xl border overflow-hidden">
      <div className="p-4 border-b">
        <h3 className="font-bold flex items-center gap-2">
          <Globe size={18} /> Doanh thu theo khu vực
        </h3>
      </div>
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b bg-secondary/50">
            <th className="text-left p-3 font-semibold">Khu vực</th>
            <th className="text-right p-3 font-semibold">Đơn hàng</th>
            <th className="text-right p-3 font-semibold">Doanh thu</th>
            <th className="text-right p-3 font-semibold">Tỷ lệ</th>
          </tr>
        </thead>
        <tbody>
          {regions.map((r) => (
            <tr key={r.region} className="border-b hover:bg-hover transition-colors">
              <td className="p-3 font-medium">{r.region}</td>
              <td className="p-3 text-right">{r.orders}</td>
              <td className="p-3 text-right font-semibold text-accent-primary">{formatPrice(r.revenue)}</td>
              <td className="p-3 text-right">
                <div className="flex items-center justify-end gap-2">
                  <div className="w-16 h-1.5 bg-gray-200 rounded-full overflow-hidden">
                    <div className="h-full bg-indigo-500 rounded-full" style={{ width: `${r.percentage}%` }} />
                  </div>
                  <span className="text-xs text-tertiary w-8">{r.percentage}%</span>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
