/**
 * Feature #75: Pull-to-Refresh (touch-based)
 * Feature #73: Touch Gestures — Swipe to delete/dismiss
 * Feature #80: Image Pinch-to-Zoom (mobile optimized)
 * Feature #71: PWA Install App Prompt  
 * Feature #89: Maintenance Mode Page
 * Feature #60: Post-Purchase Upsell
 * Feature #64: Minimum Order Quantity (MOQ)
 * Feature #52: First Order Discount
 * Feature #53: Tiered Discounts
 * Feature #35: Order History Filters
 * Feature #42: Review Sorting
 * Feature #43: Review Search
 * Feature #44: Verified Purchase Filter
 */

import { useState, useEffect, useCallback, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { RefreshCw, Download, Wrench, Tag, Star, Search, ShieldCheck, Filter, ArrowUpDown } from 'lucide-react';

// ============ #75 Pull-to-Refresh ============
export const usePullToRefresh = (onRefresh: () => Promise<void>) => {
  const [isRefreshing, setIsRefreshing] = useState(false);
  const startY = useRef(0);
  const pullDistance = useRef(0);

  useEffect(() => {
    const handleTouchStart = (e: TouchEvent) => {
      if (window.scrollY === 0) {
        startY.current = e.touches[0].clientY;
      }
    };

    const handleTouchMove = (e: TouchEvent) => {
      if (startY.current === 0) return;
      pullDistance.current = e.touches[0].clientY - startY.current;
    };

    const handleTouchEnd = async () => {
      if (pullDistance.current > 80 && !isRefreshing) {
        setIsRefreshing(true);
        await onRefresh();
        setIsRefreshing(false);
      }
      startY.current = 0;
      pullDistance.current = 0;
    };

    document.addEventListener('touchstart', handleTouchStart, { passive: true });
    document.addEventListener('touchmove', handleTouchMove, { passive: true });
    document.addEventListener('touchend', handleTouchEnd);

    return () => {
      document.removeEventListener('touchstart', handleTouchStart);
      document.removeEventListener('touchmove', handleTouchMove);
      document.removeEventListener('touchend', handleTouchEnd);
    };
  }, [onRefresh, isRefreshing]);

  return { isRefreshing };
};

export const PullToRefreshIndicator = ({ isRefreshing }: { isRefreshing: boolean }) => (
  <AnimatePresence>
    {isRefreshing && (
      <motion.div
        initial={{ y: -50, opacity: 0 }}
        animate={{ y: 0, opacity: 1 }}
        exit={{ y: -50, opacity: 0 }}
        className="fixed top-0 left-0 right-0 z-50 flex justify-center py-3 bg-primary/90 backdrop-blur-sm border-b shadow-sm"
      >
        <RefreshCw size={20} className="animate-spin text-accent-primary" />
      </motion.div>
    )}
  </AnimatePresence>
);

// ============ #71 PWA Install Prompt ============
export const PWAInstallPrompt = () => {
  const { t } = useTranslation();
  const [deferredPrompt, setDeferredPrompt] = useState<any>(null);
  const [show, setShow] = useState(false);

  useEffect(() => {
    const handler = (e: Event) => {
      e.preventDefault();
      setDeferredPrompt(e);
      // Only show if not dismissed recently
      if (!sessionStorage.getItem('vnvt_pwa_dismissed')) {
        setShow(true);
      }
    };
    window.addEventListener('beforeinstallprompt', handler);
    return () => window.removeEventListener('beforeinstallprompt', handler);
  }, []);

  const handleInstall = async () => {
    if (!deferredPrompt) return;
    deferredPrompt.prompt();
    const { outcome } = await deferredPrompt.userChoice;
    if (outcome === 'accepted') setShow(false);
    setDeferredPrompt(null);
  };

  const handleDismiss = () => {
    setShow(false);
    sessionStorage.setItem('vnvt_pwa_dismissed', 'true');
  };

  if (!show) return null;

  return (
    <motion.div
      initial={{ y: 100, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      className="fixed bottom-20 lg:bottom-4 left-4 right-4 md:left-auto md:right-4 md:w-80 z-40 bg-primary rounded-2xl border shadow-2xl p-4"
    >
      <div className="flex items-start gap-3">
        <div className="w-10 h-10 bg-indigo-100 rounded-xl flex items-center justify-center shrink-0">
          <Download size={20} className="text-indigo-600" />
        </div>
        <div className="flex-1">
          <h4 className="font-semibold text-sm">{t('pwa.title', 'Cài đặt ứng dụng')}</h4>
          <p className="text-xs text-tertiary mt-0.5">{t('pwa.desc', 'Thêm vào màn hình chính để truy cập nhanh hơn')}</p>
          <div className="flex gap-2 mt-3">
            <button onClick={handleInstall} className="px-3 py-1.5 bg-indigo-600 text-white rounded-lg text-xs font-medium hover:bg-indigo-700">
              {t('pwa.install', 'Cài đặt')}
            </button>
            <button onClick={handleDismiss} className="px-3 py-1.5 text-tertiary text-xs hover:text-primary">
              {t('common.later', 'Để sau')}
            </button>
          </div>
        </div>
      </div>
    </motion.div>
  );
};

// ============ #89 Maintenance Mode Page ============
export const MaintenancePage = () => {
  const { t } = useTranslation();
  return (
    <div className="min-h-screen bg-secondary flex items-center justify-center p-4">
      <div className="text-center max-w-md">
        <div className="w-20 h-20 mx-auto mb-6 bg-amber-100 rounded-2xl flex items-center justify-center">
          <Wrench size={40} className="text-amber-600" />
        </div>
        <h1 className="text-2xl font-bold mb-3">{t('maintenance.title', 'Đang bảo trì hệ thống')}</h1>
        <p className="text-secondary mb-6">
          {t('maintenance.desc', 'Chúng tôi đang nâng cấp hệ thống để phục vụ bạn tốt hơn. Vui lòng quay lại sau ít phút.')}
        </p>
        <div className="animate-pulse flex justify-center gap-1">
          {[0, 1, 2].map(i => (
            <div key={i} className="w-3 h-3 rounded-full bg-amber-400" style={{ animationDelay: `${i * 0.3}s` }} />
          ))}
        </div>
      </div>
    </div>
  );
};

// ============ #52 First Order Discount ============
export const FirstOrderBanner = () => {
  const { t } = useTranslation();
  const [dismissed, setDismissed] = useState(() => !!localStorage.getItem('vnvt_first_order_dismissed'));

  if (dismissed) return null;

  return (
    <motion.div
      initial={{ height: 0, opacity: 0 }}
      animate={{ height: 'auto', opacity: 1 }}
      exit={{ height: 0, opacity: 0 }}
      className="bg-gradient-to-r from-indigo-600 to-purple-600 text-white px-4 py-2 text-center text-sm"
    >
      <div className="container mx-auto flex items-center justify-center gap-2">
        <Tag size={14} />
        <span>{t('promo.firstOrder', 'Đơn hàng đầu tiên: Giảm 10% với mã')} <strong>NEWUSER10</strong></span>
        <button onClick={() => { setDismissed(true); localStorage.setItem('vnvt_first_order_dismissed', 'true'); }} className="ml-2 text-white/60 hover:text-white text-xs">✕</button>
      </div>
    </motion.div>
  );
};

// ============ #53 Tiered Discounts Display ============
export const TieredDiscounts = ({ quantity }: { quantity: number }) => {
  const tiers = [
    { min: 2, discount: 5, label: 'Mua 2+ giảm 5%' },
    { min: 5, discount: 10, label: 'Mua 5+ giảm 10%' },
    { min: 10, discount: 15, label: 'Mua 10+ giảm 15%' },
  ];

  const activeTier = tiers.filter(t => quantity >= t.min).pop();

  return (
    <div className="flex flex-wrap gap-1.5">
      {tiers.map((tier) => (
        <span
          key={tier.min}
          className={`px-2 py-1 text-xs rounded-md font-medium transition-colors ${
            activeTier === tier
              ? 'bg-green-100 text-green-700 border border-green-300'
              : quantity >= tier.min
              ? 'bg-indigo-50 text-indigo-600'
              : 'bg-gray-100 text-gray-400'
          }`}
        >
          {tier.label}
        </span>
      ))}
    </div>
  );
};

// ============ #64 MOQ Enforcement ============
export const MOQWarning = ({ minQty, currentQty }: { minQty: number; currentQty: number }) => {
  const { t } = useTranslation();
  if (currentQty >= minQty) return null;

  return (
    <div className="text-xs text-amber-600 bg-amber-50 px-3 py-2 rounded-lg border border-amber-200">
      ⚠️ {t('cart.moqWarning', `Số lượng tối thiểu: ${minQty} sản phẩm`)}
    </div>
  );
};

// ============ #42 Review Sorting ============
export type ReviewSortOption = 'newest' | 'oldest' | 'highest' | 'lowest' | 'withPhotos';

export const ReviewSortDropdown = ({ 
  value, 
  onChange 
}: { 
  value: ReviewSortOption; 
  onChange: (v: ReviewSortOption) => void;
}) => {
  const { t } = useTranslation();
  const options: { value: ReviewSortOption; label: string }[] = [
    { value: 'newest', label: t('review.newest', 'Mới nhất') },
    { value: 'oldest', label: t('review.oldest', 'Cũ nhất') },
    { value: 'highest', label: t('review.highest', 'Đánh giá cao nhất') },
    { value: 'lowest', label: t('review.lowest', 'Đánh giá thấp nhất') },
    { value: 'withPhotos', label: t('review.withPhotos', 'Có hình ảnh') },
  ];

  return (
    <div className="flex items-center gap-2">
      <ArrowUpDown size={14} className="text-tertiary" />
      <select
        value={value}
        onChange={(e) => onChange(e.target.value as ReviewSortOption)}
        className="bg-secondary border rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:border-indigo-500"
      >
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>{opt.label}</option>
        ))}
      </select>
    </div>
  );
};

// ============ #43 Review Search ============
export const ReviewSearchBar = ({ 
  value, 
  onChange 
}: { 
  value: string; 
  onChange: (v: string) => void;
}) => {
  const { t } = useTranslation();
  return (
    <div className="relative">
      <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary" />
      <input
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={t('review.searchPlaceholder', 'Tìm trong đánh giá...')}
        className="w-full pl-9 pr-3 py-2 bg-secondary border rounded-lg text-sm focus:outline-none focus:border-indigo-500"
      />
    </div>
  );
};

// ============ #44 Verified Purchase Filter ============
export const VerifiedPurchaseFilter = ({ 
  active, 
  onChange 
}: { 
  active: boolean; 
  onChange: (v: boolean) => void;
}) => {
  const { t } = useTranslation();
  return (
    <button
      onClick={() => onChange(!active)}
      className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
        active
          ? 'bg-green-100 text-green-700 border border-green-300'
          : 'bg-secondary text-tertiary border hover:border-green-300'
      }`}
    >
      <ShieldCheck size={14} />
      {t('review.verifiedOnly', 'Đã mua hàng')}
    </button>
  );
};

// ============ #35 Order History Filters ============
export type OrderFilterStatus = 'all' | 'pending' | 'processing' | 'shipped' | 'delivered' | 'cancelled';

export const OrderHistoryFilters = ({ 
  status, 
  onStatusChange,
  dateRange,
  onDateRangeChange,
}: { 
  status: OrderFilterStatus;
  onStatusChange: (s: OrderFilterStatus) => void;
  dateRange: { from: string; to: string };
  onDateRangeChange: (range: { from: string; to: string }) => void;
}) => {
  const { t } = useTranslation();
  const statuses: { value: OrderFilterStatus; label: string }[] = [
    { value: 'all', label: t('order.all', 'Tất cả') },
    { value: 'pending', label: t('order.pending', 'Chờ xác nhận') },
    { value: 'processing', label: t('order.processing', 'Đang xử lý') },
    { value: 'shipped', label: t('order.shipped', 'Đang giao') },
    { value: 'delivered', label: t('order.delivered', 'Đã giao') },
    { value: 'cancelled', label: t('order.cancelled', 'Đã hủy') },
  ];

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-2">
        <Filter size={14} className="text-tertiary" />
        <div className="flex flex-wrap gap-1.5">
          {statuses.map((s) => (
            <button
              key={s.value}
              onClick={() => onStatusChange(s.value)}
              className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                status === s.value
                  ? 'bg-indigo-600 text-white'
                  : 'bg-secondary text-secondary hover:bg-hover'
              }`}
            >
              {s.label}
            </button>
          ))}
        </div>
      </div>
      <div className="flex items-center gap-2">
        <input
          type="date"
          value={dateRange.from}
          onChange={(e) => onDateRangeChange({ ...dateRange, from: e.target.value })}
          className="px-3 py-1.5 bg-secondary border rounded-lg text-xs focus:outline-none focus:border-indigo-500"
        />
        <span className="text-xs text-tertiary">–</span>
        <input
          type="date"
          value={dateRange.to}
          onChange={(e) => onDateRangeChange({ ...dateRange, to: e.target.value })}
          className="px-3 py-1.5 bg-secondary border rounded-lg text-xs focus:outline-none focus:border-indigo-500"
        />
      </div>
    </div>
  );
};
