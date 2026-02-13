/**
 * User Account & Loyalty Features:
 * #31: Birthday Rewards display
 * #32: Referral Dashboard
 * #33: Social Media Links in Profile (display only)
 * #34: Notification Preference Center
 * #37: Downloadable Invoice (client-side PDF-like view)
 * #38: RMA/Return Request Form
 * #39: Wallet/Store Credit Display
 * #40: Avatar Maker (already exists, this extends)
 * #46: Admin Response to Review
 * #48: User Gallery (UGC)  
 * #14: PDF Spec Sheet (printable view)
 * #15: Energy Label Badge
 * #18: Brand Story Modal
 * #4: Attribute-Based Breadcrumbs
 * #24: Delayed Shipping Selection
 * #25: Split Shipment (UI toggle)
 * #26: Recurring Order Toggle (Subscribe & Save)
 * #30: Saved Credit Card (display only, mock)
 */

import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  Gift, Users, Link2, Bell, FileText, RotateCcw, Wallet, 
  Reply, Camera, ChevronRight, Printer, Zap, BookOpen,
  CalendarDays, Package, RefreshCw, CreditCard, Cake,
  Copy, Check, X
} from 'lucide-react';

// ============ #4 Attribute-Based Breadcrumbs ============
interface BreadcrumbItem {
  label: string;
  href?: string;
}

export const AttributeBreadcrumbs = ({ items }: { items: BreadcrumbItem[] }) => (
  <nav className="flex items-center gap-1.5 text-sm text-secondary flex-wrap">
    {items.map((item, i) => (
      <span key={i} className="flex items-center gap-1.5">
        {i > 0 && <ChevronRight size={12} className="text-tertiary" />}
        {item.href ? (
          <a href={item.href} className="hover:text-accent-primary transition-colors">{item.label}</a>
        ) : (
          <span className="text-primary font-medium">{item.label}</span>
        )}
      </span>
    ))}
  </nav>
);

// ============ #14 Printable Spec Sheet ============
export const PrintableSpecSheet = ({ 
  productName, 
  specs 
}: { 
  productName: string; 
  specs: Record<string, string>;
}) => {
  const handlePrint = () => window.print();

  return (
    <div>
      <button onClick={handlePrint} className="flex items-center gap-2 px-4 py-2 border rounded-lg text-sm hover:bg-hover transition-colors">
        <Printer size={16} /> Tải thông số kỹ thuật
      </button>
      {/* Print-only section */}
      <div className="hidden print:block p-8">
        <h1 className="text-2xl font-bold mb-4">{productName}</h1>
        <table className="w-full border-collapse">
          <tbody>
            {Object.entries(specs).map(([key, val]) => (
              <tr key={key} className="border-b">
                <td className="py-2 pr-4 font-medium text-gray-600 w-1/3">{key}</td>
                <td className="py-2">{val}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

// ============ #15 Energy Label Badge ============
export const EnergyLabelBadge = ({ rating }: { rating: 'A+++' | 'A++' | 'A+' | 'A' | 'B' | 'C' | 'D' }) => {
  const colors: Record<string, string> = {
    'A+++': 'bg-green-600', 'A++': 'bg-green-500', 'A+': 'bg-green-400',
    'A': 'bg-lime-500', 'B': 'bg-yellow-500', 'C': 'bg-orange-500', 'D': 'bg-red-500',
  };
  return (
    <span className={`inline-flex items-center gap-1 px-2 py-0.5 text-xs font-bold text-white rounded ${colors[rating]}`}>
      <Zap size={10} /> {rating}
    </span>
  );
};

// ============ #18 Brand Story Modal ============
export const BrandStoryModal = ({ 
  isOpen, 
  onClose, 
  brand 
}: { 
  isOpen: boolean; 
  onClose: () => void; 
  brand: { name: string; description: string; founded?: string; origin?: string };
}) => (
  <AnimatePresence>
    {isOpen && (
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        exit={{ opacity: 0 }}
        className="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 backdrop-blur-sm p-4"
        onClick={onClose}
      >
        <motion.div
          initial={{ scale: 0.9 }}
          animate={{ scale: 1 }}
          exit={{ scale: 0.9 }}
          onClick={(e) => e.stopPropagation()}
          className="w-full max-w-md bg-primary rounded-2xl shadow-2xl overflow-hidden"
        >
          <div className="p-6">
            <div className="flex items-center justify-between mb-4">
              <div>
                <h2 className="text-xl font-bold flex items-center gap-2">
                  <BookOpen size={20} className="text-indigo-600" /> {brand.name}
                </h2>
                {brand.founded && <p className="text-xs text-tertiary mt-0.5">Thành lập: {brand.founded}</p>}
              </div>
              <button onClick={onClose} className="p-1 hover:bg-hover rounded-lg"><X size={18} /></button>
            </div>
            <p className="text-sm text-secondary leading-relaxed">{brand.description}</p>
            {brand.origin && (
              <p className="text-xs text-tertiary mt-3">📍 Xuất xứ: {brand.origin}</p>
            )}
          </div>
        </motion.div>
      </motion.div>
    )}
  </AnimatePresence>
);

// ============ #24 Delayed Shipping Selection ============
export const DelayedShippingPicker = ({ 
  date, 
  onChange 
}: { 
  date: string; 
  onChange: (d: string) => void;
}) => {
  const { t } = useTranslation();
  const [delayed, setDelayed] = useState(!!date);
  const minDate = new Date();
  minDate.setDate(minDate.getDate() + 1);

  return (
    <div className="border rounded-xl overflow-hidden">
      <button onClick={() => setDelayed(!delayed)} className="w-full flex items-center gap-3 p-4 hover:bg-hover transition-colors">
        <div className={`w-5 h-5 rounded border-2 flex items-center justify-center ${delayed ? 'bg-indigo-600 border-indigo-600' : 'border-gray-300'}`}>
          {delayed && <Check size={12} className="text-white" />}
        </div>
        <CalendarDays size={18} className="text-indigo-500" />
        <span className="text-sm font-medium">{t('checkout.delayShipping', 'Chọn ngày giao hàng')}</span>
      </button>
      <AnimatePresence>
        {delayed && (
          <motion.div initial={{ height: 0 }} animate={{ height: 'auto' }} exit={{ height: 0 }} className="overflow-hidden">
            <div className="p-4 pt-0">
              <input
                type="date"
                value={date}
                onChange={(e) => onChange(e.target.value)}
                min={minDate.toISOString().split('T')[0]}
                className="w-full px-3 py-2 bg-secondary border rounded-lg text-sm focus:outline-none focus:border-indigo-500"
              />
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

// ============ #26 Recurring Order Toggle ============
export const RecurringOrderToggle = ({ 
  enabled, 
  onToggle,
  interval,
  onIntervalChange 
}: { 
  enabled: boolean; 
  onToggle: (v: boolean) => void;
  interval: number;
  onIntervalChange: (days: number) => void;
}) => {
  const { t } = useTranslation();
  return (
    <div className="border rounded-xl p-4">
      <div className="flex items-center justify-between mb-3">
        <div className="flex items-center gap-2">
          <RefreshCw size={16} className="text-green-600" />
          <span className="text-sm font-medium">{t('cart.subscribe', 'Đặt hàng định kỳ — Tiết kiệm 5%')}</span>
        </div>
        <button
          onClick={() => onToggle(!enabled)}
          className={`w-10 h-6 rounded-full transition-colors relative ${enabled ? 'bg-green-500' : 'bg-gray-300'}`}
        >
          <div className={`absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all ${enabled ? 'left-[18px]' : 'left-0.5'}`} />
        </button>
      </div>
      {enabled && (
        <div className="flex gap-2">
          {[7, 14, 30, 60].map(days => (
            <button
              key={days}
              onClick={() => onIntervalChange(days)}
              className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                interval === days ? 'bg-green-100 text-green-700 border border-green-300' : 'bg-secondary text-secondary hover:bg-hover'
              }`}
            >
              {days} ngày
            </button>
          ))}
        </div>
      )}
    </div>
  );
};

// ============ #31 Birthday Rewards ============
export const BirthdayReward = ({ birthday, couponCode }: { birthday?: string; couponCode?: string }) => {
  const { t } = useTranslation();
  const [copied, setCopied] = useState(false);
  
  if (!couponCode) return null;

  const isBirthdayMonth = birthday ? new Date(birthday).getMonth() === new Date().getMonth() : false;
  if (!isBirthdayMonth) return null;

  return (
    <div className="bg-gradient-to-r from-pink-50 to-purple-50 dark:from-pink-950/30 dark:to-purple-950/30 rounded-xl p-4 border border-pink-200">
      <div className="flex items-center gap-3">
        <div className="w-10 h-10 bg-pink-100 rounded-xl flex items-center justify-center">
          <Cake size={20} className="text-pink-600" />
        </div>
        <div className="flex-1">
          <h4 className="text-sm font-bold text-pink-800 dark:text-pink-300">🎂 Chúc mừng sinh nhật!</h4>
          <p className="text-xs text-pink-600 dark:text-pink-400">Giảm 15% cho đơn hàng trong tháng sinh nhật</p>
        </div>
        <button
          onClick={() => { navigator.clipboard.writeText(couponCode); setCopied(true); setTimeout(() => setCopied(false), 2000); }}
          className="flex items-center gap-1 px-3 py-1.5 bg-pink-600 text-white rounded-lg text-xs font-medium hover:bg-pink-700"
        >
          {copied ? <Check size={12} /> : <Copy size={12} />}
          {copied ? 'Đã sao chép' : couponCode}
        </button>
      </div>
    </div>
  );
};

// ============ #32 Referral Dashboard ============
export const ReferralDashboard = ({ referralCode, referralCount, totalEarnings }: { referralCode: string; referralCount: number; totalEarnings: number }) => {
  const [copied, setCopied] = useState(false);
  const formatPrice = (p: number) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p);

  return (
    <div className="bg-primary rounded-xl border p-5">
      <h3 className="font-bold flex items-center gap-2 mb-4">
        <Users size={18} /> Chương trình giới thiệu
      </h3>
      <div className="grid grid-cols-2 gap-4 mb-4">
        <div className="bg-secondary rounded-lg p-3 text-center">
          <div className="text-2xl font-bold text-indigo-600">{referralCount}</div>
          <div className="text-xs text-tertiary">Bạn bè đã mời</div>
        </div>
        <div className="bg-secondary rounded-lg p-3 text-center">
          <div className="text-2xl font-bold text-green-600">{formatPrice(totalEarnings)}</div>
          <div className="text-xs text-tertiary">Thu nhập từ giới thiệu</div>
        </div>
      </div>
      <div className="flex items-center gap-2 bg-secondary rounded-lg p-3">
        <span className="text-sm font-mono flex-1">{referralCode}</span>
        <button
          onClick={() => { navigator.clipboard.writeText(referralCode); setCopied(true); setTimeout(() => setCopied(false), 2000); }}
          className="p-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
        >
          {copied ? <Check size={14} /> : <Copy size={14} />}
        </button>
      </div>
    </div>
  );
};

// ============ #34 Notification Preference Center ============
interface NotifPref { email: boolean; push: boolean; sms: boolean; }
type NotifTopic = 'orders' | 'promotions' | 'reviews' | 'priceDrops' | 'restocks';

export const NotificationPreferences = ({ 
  prefs, 
  onChange 
}: { 
  prefs: Record<NotifTopic, NotifPref>;
  onChange: (topic: NotifTopic, channel: keyof NotifPref, value: boolean) => void;
}) => {
  const topics: { key: NotifTopic; label: string; icon: React.ReactNode }[] = [
    { key: 'orders', label: 'Đơn hàng', icon: <Package size={16} /> },
    { key: 'promotions', label: 'Khuyến mãi', icon: <Gift size={16} /> },
    { key: 'reviews', label: 'Đánh giá', icon: <Reply size={16} /> },
    { key: 'priceDrops', label: 'Giảm giá', icon: <Zap size={16} /> },
    { key: 'restocks', label: 'Hàng về', icon: <RefreshCw size={16} /> },
  ];

  return (
    <div className="space-y-2">
      <div className="grid grid-cols-4 gap-2 px-3 pb-2 border-b text-xs font-semibold text-tertiary">
        <span></span>
        <span className="text-center">Email</span>
        <span className="text-center">Push</span>
        <span className="text-center">SMS</span>
      </div>
      {topics.map(({ key, label, icon }) => (
        <div key={key} className="grid grid-cols-4 gap-2 items-center px-3 py-2 hover:bg-hover rounded-lg">
          <span className="flex items-center gap-2 text-sm font-medium">{icon} {label}</span>
          {(['email', 'push', 'sms'] as const).map(channel => (
            <div key={channel} className="flex justify-center">
              <button
                onClick={() => onChange(key, channel, !prefs[key][channel])}
                className={`w-8 h-5 rounded-full transition-colors relative ${prefs[key][channel] ? 'bg-indigo-500' : 'bg-gray-300'}`}
              >
                <div className={`absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-all ${prefs[key][channel] ? 'left-3.5' : 'left-0.5'}`} />
              </button>
            </div>
          ))}
        </div>
      ))}
    </div>
  );
};

// ============ #38 RMA/Return Request Form ============
export const ReturnRequestForm = ({ 
  orderCode,
  onSubmit 
}: { 
  orderCode: string;
  onSubmit: (data: { reason: string; details: string }) => void;
}) => {
  const { t } = useTranslation();
  const [reason, setReason] = useState('');
  const [details, setDetails] = useState('');

  const reasons = [
    'Sản phẩm lỗi',
    'Sai sản phẩm',
    'Không đúng mô tả',
    'Đổi ý / Không cần nữa',
    'Khác',
  ];

  return (
    <div className="space-y-4 p-4 bg-secondary rounded-xl">
      <h4 className="font-semibold flex items-center gap-2">
        <RotateCcw size={16} /> {t('order.returnRequest', 'Yêu cầu trả hàng')} — #{orderCode}
      </h4>
      <div>
        <label className="text-xs text-tertiary mb-1 block">Lý do trả hàng</label>
        <select value={reason} onChange={(e) => setReason(e.target.value)} className="w-full px-3 py-2 border rounded-lg text-sm bg-primary focus:outline-none focus:border-indigo-500">
          <option value="">Chọn lý do...</option>
          {reasons.map(r => <option key={r} value={r}>{r}</option>)}
        </select>
      </div>
      <div>
        <label className="text-xs text-tertiary mb-1 block">Chi tiết</label>
        <textarea value={details} onChange={(e) => setDetails(e.target.value)} rows={3} className="w-full px-3 py-2 border rounded-lg text-sm bg-primary focus:outline-none focus:border-indigo-500 resize-none" placeholder="Mô tả chi tiết vấn đề..." />
      </div>
      <button onClick={() => onSubmit({ reason, details })} disabled={!reason} className="w-full py-2.5 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 disabled:opacity-50">
        Gửi yêu cầu
      </button>
    </div>
  );
};

// ============ #39 Wallet/Store Credit Display ============
export const WalletDisplay = ({ balance, transactions }: { 
  balance: number; 
  transactions: Array<{ type: 'credit' | 'debit'; amount: number; description: string; date: string }>;
}) => {
  const formatPrice = (p: number) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p);

  return (
    <div className="bg-primary rounded-xl border overflow-hidden">
      <div className="bg-gradient-to-r from-indigo-600 to-purple-600 p-5 text-white">
        <div className="flex items-center gap-2 text-white/70 text-sm mb-1">
          <Wallet size={16} /> Số dư ví
        </div>
        <div className="text-3xl font-bold">{formatPrice(balance)}</div>
      </div>
      <div className="divide-y">
        {transactions.slice(0, 5).map((tx, i) => (
          <div key={i} className="p-3 flex items-center justify-between">
            <div>
              <p className="text-sm">{tx.description}</p>
              <p className="text-xs text-tertiary">{tx.date}</p>
            </div>
            <span className={`text-sm font-semibold ${tx.type === 'credit' ? 'text-green-600' : 'text-error'}`}>
              {tx.type === 'credit' ? '+' : '-'}{formatPrice(tx.amount)}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
};
