/**
 * Feature #50: Live Sales Popups
 * "Someone in Hanoi just bought X" — no third-party needed, uses random data.
 */
import { useState, useEffect, useCallback } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { ShoppingBag, X } from 'lucide-react';

const CITIES = [
  'Hà Nội', 'TP. Hồ Chí Minh', 'Đà Nẵng', 'Hải Phòng', 'Cần Thơ',
  'Nha Trang', 'Huế', 'Biên Hòa', 'Vũng Tàu', 'Bắc Ninh',
  'Thái Nguyên', 'Nam Định', 'Thanh Hóa', 'Buôn Ma Thuột', 'Quy Nhơn'
];

const FIRST_NAMES = [
  'Nguyễn', 'Trần', 'Lê', 'Phạm', 'Hoàng', 'Huỳnh', 'Phan', 'Vũ', 'Đặng', 'Bùi'
];

const PRODUCTS_SAMPLE = [
  'Tai nghe Bluetooth', 'Ốp lưng điện thoại', 'Cáp sạc nhanh', 'Chuột không dây',
  'Bàn phím cơ', 'Loa mini', 'Pin dự phòng', 'Miếng dán màn hình', 'USB Flash 64GB',
  'Đèn LED bàn'
];

const pickRandom = <T,>(arr: T[]): T => arr[Math.floor(Math.random() * arr.length)];

const POPUP_KEY = 'vnvt_sales_popup_disabled';

export const LiveSalesPopup = () => {
  const [notification, setNotification] = useState<{ city: string; name: string; product: string; minutes: number } | null>(null);
  const [dismissed, setDismissed] = useState(false);

  const showNotification = useCallback(() => {
    if (sessionStorage.getItem(POPUP_KEY)) return;
    setNotification({
      city: pickRandom(CITIES),
      name: `${pickRandom(FIRST_NAMES)} ***`,
      product: pickRandom(PRODUCTS_SAMPLE),
      minutes: Math.floor(Math.random() * 30) + 1,
    });
    setDismissed(false);

    // Auto-hide after 5 seconds
    setTimeout(() => setDismissed(true), 5000);
  }, []);

  useEffect(() => {
    // First popup after 15 seconds
    const initialTimer = setTimeout(showNotification, 15000);

    // Repeat every 30-60 seconds
    const interval = setInterval(() => {
      showNotification();
    }, (Math.random() * 30 + 30) * 1000);

    return () => {
      clearTimeout(initialTimer);
      clearInterval(interval);
    };
  }, [showNotification]);

  const handleDismiss = () => {
    setDismissed(true);
    sessionStorage.setItem(POPUP_KEY, 'true');
  };

  return (
    <AnimatePresence>
      {notification && !dismissed && (
        <motion.div
          initial={{ x: -100, opacity: 0 }}
          animate={{ x: 0, opacity: 1 }}
          exit={{ x: -100, opacity: 0 }}
          transition={{ type: 'spring', damping: 20 }}
          className="fixed bottom-20 lg:bottom-6 left-4 z-40 max-w-xs bg-primary rounded-xl border shadow-2xl overflow-hidden"
        >
          <div className="flex items-start gap-3 p-3 pr-8">
            <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center shrink-0">
              <ShoppingBag size={20} className="text-green-600" />
            </div>
            <div className="min-w-0">
              <p className="text-sm font-medium text-primary leading-tight">
                <span className="font-bold">{notification.name}</span> ở {notification.city}
              </p>
              <p className="text-xs text-secondary mt-0.5 truncate">
                vừa mua <span className="font-semibold text-accent-primary">{notification.product}</span>
              </p>
              <p className="text-[10px] text-tertiary mt-1">
                {notification.minutes} phút trước
              </p>
            </div>
          </div>
          <button
            onClick={handleDismiss}
            className="absolute top-2 right-2 p-0.5 text-tertiary hover:text-primary rounded transition-colors"
          >
            <X size={14} />
          </button>
          <div className="h-0.5 bg-green-500 animate-pulse" />
        </motion.div>
      )}
    </AnimatePresence>
  );
};
