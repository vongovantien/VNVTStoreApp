/**
 * Feature #2: Price Range Histogram
 * Feature #60: Post-Purchase Upsell
 * Feature #97: Export Orders (already exists, extend with more formats)
 * Feature #46: Admin Response to Review
 * Feature #48: User Gallery (UGC)
 * Feature #25: Split Shipment UI
 * Feature #30: Saved Payment Methods (display, no real payment)
 * Feature #33: Social Media Links
 * Feature #28: Social Login During Checkout (display only)
 * Feature #55: Mystery Box / Lucky Dip
 * Feature #37: Invoice Download (client-side)
 * Feature #82: WebP image optimization indicator
 * Feature #90: Currency Auto-detect
 * Feature #99: Banner Manager (drag-and-drop)
 * Feature #98: Bulk Product Edit (inline editing)
 */

import { useState, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import {
  BarChart, ShoppingBag, Download, Reply, Camera, Package,
  CreditCard, LinkIcon, Box, FileText, ImageIcon, Globe,
  Image, Edit, GripVertical, Plus, Trash2, Check, X
} from 'lucide-react';

// ============ #2 Price Range Histogram ============
export const PriceHistogram = ({ prices, range, onRangeChange }: {
  prices: number[];
  range: [number, number];
  onRangeChange: (r: [number, number]) => void;
}) => {
  const buckets = useMemo(() => {
    if (prices.length === 0) return [];
    const min = Math.min(...prices);
    const max = Math.max(...prices);
    const bucketCount = 20;
    const bucketSize = (max - min) / bucketCount || 1;
    const counts = new Array(bucketCount).fill(0);
    prices.forEach(p => {
      const idx = Math.min(Math.floor((p - min) / bucketSize), bucketCount - 1);
      counts[idx]++;
    });
    const maxCount = Math.max(...counts);
    return counts.map((c, i) => ({
      count: c,
      height: maxCount > 0 ? (c / maxCount) * 100 : 0,
      min: min + i * bucketSize,
      max: min + (i + 1) * bucketSize,
    }));
  }, [prices]);

  if (buckets.length === 0) return null;

  return (
    <div className="space-y-2">
      <div className="flex items-end gap-px h-16">
        {buckets.map((b, i) => {
          const inRange = b.min >= range[0] && b.max <= range[1];
          return (
            <div
              key={i}
              className={`flex-1 rounded-t-sm transition-colors ${inRange ? 'bg-indigo-500' : 'bg-gray-200 dark:bg-gray-700'}`}
              style={{ height: `${Math.max(b.height, 4)}%` }}
              title={`${Math.round(b.min).toLocaleString()} - ${Math.round(b.max).toLocaleString()}: ${b.count} sản phẩm`}
            />
          );
        })}
      </div>
      <div className="flex items-center gap-2">
        <input
          type="range"
          min={buckets[0]?.min || 0}
          max={buckets[buckets.length - 1]?.max || 100000000}
          step={10000}
          value={range[0]}
          onChange={(e) => onRangeChange([Number(e.target.value), range[1]])}
          className="flex-1 accent-indigo-600"
        />
        <input
          type="range"
          min={buckets[0]?.min || 0}
          max={buckets[buckets.length - 1]?.max || 100000000}
          step={10000}
          value={range[1]}
          onChange={(e) => onRangeChange([range[0], Number(e.target.value)])}
          className="flex-1 accent-indigo-600"
        />
      </div>
    </div>
  );
};

// ============ #60 Post-Purchase Upsell ============
export const PostPurchaseUpsell = ({ 
  products,
  onAddToOrder 
}: { 
  products: Array<{ code: string; name: string; price: number; imageUrl?: string }>;
  onAddToOrder: (code: string) => void;
}) => {
  const { t } = useTranslation();
  const formatPrice = (p: number) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p);

  if (products.length === 0) return null;

  return (
    <div className="bg-gradient-to-r from-indigo-50 to-purple-50 dark:from-indigo-950/30 dark:to-purple-950/30 rounded-2xl p-6 border">
      <h3 className="text-lg font-bold mb-1 flex items-center gap-2">
        <ShoppingBag size={20} className="text-indigo-600" />
        {t('order.upsell', 'Thêm vào đơn hàng — Giảm 10%!')}
      </h3>
      <p className="text-sm text-tertiary mb-4">{t('order.upsellDesc', 'Thêm ngay trước khi đơn hàng được xử lý')}</p>
      <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
        {products.map((p) => (
          <div key={p.code} className="bg-primary rounded-xl p-3 border">
            <div className="text-sm font-medium truncate mb-1">{p.name}</div>
            <div className="flex items-center justify-between">
              <span className="text-sm font-bold text-accent-primary">{formatPrice(p.price * 0.9)}</span>
              <button onClick={() => onAddToOrder(p.code)} className="p-1.5 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 text-xs">
                <Plus size={14} />
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

// ============ #46 Admin Response to Review ============
export const AdminReviewResponse = ({ 
  reviewId,
  existingResponse,
  onSubmit 
}: { 
  reviewId: string;
  existingResponse?: string;
  onSubmit: (response: string) => void;
}) => {
  const [response, setResponse] = useState(existingResponse || '');
  const [editing, setEditing] = useState(!existingResponse);

  if (!editing && existingResponse) {
    return (
      <div className="pl-6 mt-2 border-l-2 border-indigo-400">
        <div className="flex items-center gap-1.5 text-xs font-semibold text-indigo-600 mb-1">
          <Reply size={12} /> Phản hồi từ cửa hàng
        </div>
        <p className="text-sm text-secondary">{existingResponse}</p>
        <button onClick={() => setEditing(true)} className="text-xs text-tertiary hover:text-primary mt-1">Chỉnh sửa</button>
      </div>
    );
  }

  return (
    <div className="pl-6 mt-2 border-l-2 border-indigo-400">
      <textarea
        value={response}
        onChange={(e) => setResponse(e.target.value)}
        rows={2}
        className="w-full px-3 py-2 bg-secondary border rounded-lg text-sm focus:outline-none focus:border-indigo-500 resize-none"
        placeholder="Viết phản hồi cho đánh giá này..."
      />
      <div className="flex gap-2 mt-2">
        <button onClick={() => { onSubmit(response); setEditing(false); }} className="px-3 py-1.5 bg-indigo-600 text-white rounded-lg text-xs">Gửi</button>
        {existingResponse && <button onClick={() => { setEditing(false); setResponse(existingResponse); }} className="px-3 py-1.5 text-tertiary text-xs">Hủy</button>}
      </div>
    </div>
  );
};

// ============ #48 User Gallery (UGC) ============
export const UserGallery = ({ productCode }: { productCode: string }) => {
  const [images, setImages] = useState<string[]>(() => {
    const stored = localStorage.getItem(`vnvt_ugc_${productCode}`);
    return stored ? JSON.parse(stored) : [];
  });

  const handleUpload = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files) return;
    Array.from(files).forEach(file => {
      const reader = new FileReader();
      reader.onload = () => {
        setImages(prev => {
          const updated = [...prev, reader.result as string];
          localStorage.setItem(`vnvt_ugc_${productCode}`, JSON.stringify(updated));
          return updated;
        });
      };
      reader.readAsDataURL(file);
    });
  }, [productCode]);

  return (
    <div className="space-y-3">
      <h4 className="font-semibold flex items-center gap-2">
        <Camera size={16} /> Ảnh từ khách hàng ({images.length})
      </h4>
      <div className="flex flex-wrap gap-2">
        {images.map((img, i) => (
          <div key={i} className="w-20 h-20 rounded-lg overflow-hidden border">
            <img src={img} alt="" className="w-full h-full object-cover" />
          </div>
        ))}
        <label className="w-20 h-20 rounded-lg border-2 border-dashed flex items-center justify-center cursor-pointer hover:bg-hover transition-colors">
          <Plus size={20} className="text-tertiary" />
          <input type="file" accept="image/*" multiple onChange={handleUpload} className="hidden" />
        </label>
      </div>
    </div>
  );
};

// ============ #25 Split Shipment Toggle ============
export const SplitShipmentToggle = ({ 
  enabled, 
  onToggle 
}: { 
  enabled: boolean; 
  onToggle: (v: boolean) => void;
}) => (
  <div className="flex items-center justify-between p-4 border rounded-xl">
    <div className="flex items-center gap-2">
      <Package size={16} className="text-indigo-500" />
      <div>
        <span className="text-sm font-medium">Giao hàng nhiều đợt</span>
        <p className="text-xs text-tertiary">Nhận hàng có sẵn trước, còn lại giao sau</p>
      </div>
    </div>
    <button
      onClick={() => onToggle(!enabled)}
      className={`w-10 h-6 rounded-full transition-colors relative ${enabled ? 'bg-indigo-500' : 'bg-gray-300'}`}
    >
      <div className={`absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all ${enabled ? 'left-[18px]' : 'left-0.5'}`} />
    </button>
  </div>
);

// ============ #30 Saved Payment Methods (display) ============
export const SavedPaymentMethods = ({ 
  methods,
  selected,
  onSelect 
}: { 
  methods: Array<{ id: string; type: 'visa' | 'mastercard' | 'momo' | 'zalopay'; last4: string; expiry: string }>;
  selected: string;
  onSelect: (id: string) => void;
}) => (
  <div className="space-y-2">
    {methods.map((m) => (
      <div
        key={m.id}
        onClick={() => onSelect(m.id)}
        className={`flex items-center gap-3 p-3 rounded-xl border-2 cursor-pointer transition-all ${
          selected === m.id ? 'border-indigo-500 bg-indigo-50/50' : 'border-transparent bg-secondary hover:border-gray-300'
        }`}
      >
        <CreditCard size={20} className="text-indigo-500" />
        <div className="flex-1">
          <p className="text-sm font-medium capitalize">{m.type} •••• {m.last4}</p>
          <p className="text-xs text-tertiary">Hết hạn: {m.expiry}</p>
        </div>
        {selected === m.id && <Check size={16} className="text-indigo-600" />}
      </div>
    ))}
  </div>
);

// ============ #33 Social Media Links ============
export const SocialMediaLinks = ({ links }: { links: Record<string, string> }) => {
  const icons: Record<string, string> = {
    facebook: '📘', instagram: '📷', tiktok: '🎵', youtube: '📺', zalo: '💬'
  };
  return (
    <div className="flex gap-2">
      {Object.entries(links).map(([platform, url]) => (
        <a
          key={platform}
          href={url}
          target="_blank"
          rel="noopener noreferrer"
          className="w-10 h-10 bg-secondary rounded-xl flex items-center justify-center hover:bg-hover transition-colors text-lg"
          title={platform}
        >
          {icons[platform] || <LinkIcon size={16} />}
        </a>
      ))}
    </div>
  );
};

// ============ #55 Mystery Box / Lucky Dip ============
export const MysteryBoxCard = ({ 
  price, 
  category, 
  onBuy 
}: { 
  price: number; 
  category: string; 
  onBuy: () => void;
}) => {
  const formatPrice = (p: number) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p);

  return (
    <motion.div
      whileHover={{ scale: 1.02 }}
      className="bg-gradient-to-br from-purple-600 via-pink-500 to-orange-400 rounded-2xl p-6 text-white cursor-pointer shadow-xl"
      onClick={onBuy}
    >
      <div className="text-center">
        <div className="text-4xl mb-3">🎁</div>
        <h3 className="text-lg font-bold mb-1">Hộp bí ẩn</h3>
        <p className="text-white/70 text-sm mb-3">Danh mục: {category}</p>
        <div className="text-2xl font-bold mb-1">{formatPrice(price)}</div>
        <p className="text-white/60 text-xs">Giá trị thực lên đến {formatPrice(price * 3)}!</p>
        <button className="mt-4 px-6 py-2 bg-white/20 rounded-xl font-medium hover:bg-white/30 transition-colors">
          Mua ngay!
        </button>
      </div>
    </motion.div>
  );
};

// ============ #37 Invoice Download (client-side) ============
export const InvoiceDownloadButton = ({ 
  orderCode,
  onDownload 
}: { 
  orderCode: string;
  onDownload: () => void;
}) => (
  <button onClick={onDownload} className="flex items-center gap-2 px-4 py-2 border rounded-lg text-sm hover:bg-hover transition-colors">
    <FileText size={16} /> Tải hóa đơn
  </button>
);

// ============ #82 WebP Indicator ============
export const WebPIndicator = ({ isWebP }: { isWebP: boolean }) => {
  if (!isWebP) return null;
  return (
    <span className="absolute top-2 right-2 px-1.5 py-0.5 bg-green-500 text-white text-[8px] font-bold rounded uppercase">
      WebP
    </span>
  );
};

// ============ #90 Currency Auto-detect ============
export const useCurrencyAutoDetect = () => {
  const lang = navigator.language || 'vi-VN';
  const detected = lang.startsWith('vi') ? 'VND' : 'USD';
  return detected;
};

// ============ #99 Banner Manager (Admin) ============
interface BannerItem {
  id: string;
  title: string;
  imageUrl: string;
  link: string;
  active: boolean;
}

export const BannerManager = ({ 
  banners,
  onReorder,
  onToggle,
  onDelete 
}: { 
  banners: BannerItem[];
  onReorder: (from: number, to: number) => void;
  onToggle: (id: string) => void;
  onDelete: (id: string) => void;
}) => (
  <div className="space-y-2">
    {banners.map((banner, idx) => (
      <div key={banner.id} className="flex items-center gap-3 p-3 bg-primary rounded-xl border hover:shadow-sm transition-shadow">
        <GripVertical size={16} className="text-tertiary cursor-grab" />
        <div className="w-16 h-10 rounded bg-secondary overflow-hidden shrink-0">
          {banner.imageUrl && <img src={banner.imageUrl} alt="" className="w-full h-full object-cover" />}
        </div>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium truncate">{banner.title}</p>
          <p className="text-xs text-tertiary truncate">{banner.link}</p>
        </div>
        <button onClick={() => onToggle(banner.id)} className={`w-8 h-5 rounded-full transition-colors relative ${banner.active ? 'bg-green-500' : 'bg-gray-300'}`}>
          <div className={`absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-all ${banner.active ? 'left-3.5' : 'left-0.5'}`} />
        </button>
        <button onClick={() => onDelete(banner.id)} className="p-1 text-tertiary hover:text-error"><Trash2 size={14} /></button>
      </div>
    ))}
  </div>
);

// ============ #98 Bulk Product Edit (inline) ============
export const BulkProductEdit = ({ 
  products,
  onSave 
}: { 
  products: Array<{ code: string; name: string; price: number; stock: number }>;
  onSave: (updated: typeof products) => void;
}) => {
  const [items, setItems] = useState(products);
  const [changed, setChanged] = useState<Set<string>>(new Set());

  const updateField = (code: string, field: string, value: string | number) => {
    setItems(prev => prev.map(p => p.code === code ? { ...p, [field]: value } : p));
    setChanged(prev => new Set(prev).add(code));
  };

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b bg-secondary/50">
            <th className="text-left p-3 font-semibold">SKU</th>
            <th className="text-left p-3 font-semibold">Tên sản phẩm</th>
            <th className="text-right p-3 font-semibold">Giá</th>
            <th className="text-right p-3 font-semibold">Tồn kho</th>
          </tr>
        </thead>
        <tbody>
          {items.map((p) => (
            <tr key={p.code} className={`border-b ${changed.has(p.code) ? 'bg-yellow-50' : 'hover:bg-hover'}`}>
              <td className="p-3 font-mono text-xs">{p.code}</td>
              <td className="p-3">
                <input value={p.name} onChange={(e) => updateField(p.code, 'name', e.target.value)} className="bg-transparent border-b border-transparent hover:border-gray-300 focus:border-indigo-500 outline-none w-full" />
              </td>
              <td className="p-3 text-right">
                <input type="number" value={p.price} onChange={(e) => updateField(p.code, 'price', Number(e.target.value))} className="bg-transparent border-b border-transparent hover:border-gray-300 focus:border-indigo-500 outline-none w-24 text-right" />
              </td>
              <td className="p-3 text-right">
                <input type="number" value={p.stock} onChange={(e) => updateField(p.code, 'stock', Number(e.target.value))} className="bg-transparent border-b border-transparent hover:border-gray-300 focus:border-indigo-500 outline-none w-20 text-right" />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {changed.size > 0 && (
        <div className="p-3 border-t flex items-center justify-between">
          <span className="text-sm text-tertiary">{changed.size} sản phẩm đã thay đổi</span>
          <button onClick={() => { onSave(items); setChanged(new Set()); }} className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700">
            Lưu thay đổi
          </button>
        </div>
      )}
    </div>
  );
};
