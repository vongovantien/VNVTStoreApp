/**
 * Feature #51: Exit Intent Popup
 * Shows a coupon/discount offer when user's cursor leaves the browser viewport.
 */
import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { X, Gift, Copy, Check } from 'lucide-react';

const EXIT_POPUP_KEY = 'vnvt_exit_popup_shown';
const COUPON_CODE = 'WELCOME10';

export const ExitIntentPopup = () => {
  const { t } = useTranslation();
  const [show, setShow] = useState(false);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    // Don't show if already shown in this session
    if (sessionStorage.getItem(EXIT_POPUP_KEY)) return;

    const handleMouseLeave = (e: MouseEvent) => {
      if (e.clientY <= 0) {
        setShow(true);
        sessionStorage.setItem(EXIT_POPUP_KEY, 'true');
        document.removeEventListener('mouseout', handleMouseLeave);
      }
    };

    // Delay adding listener to avoid triggering on page load
    const timer = setTimeout(() => {
      document.addEventListener('mouseout', handleMouseLeave);
    }, 5000);

    return () => {
      clearTimeout(timer);
      document.removeEventListener('mouseout', handleMouseLeave);
    };
  }, []);

  const handleCopy = useCallback(() => {
    navigator.clipboard.writeText(COUPON_CODE);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }, []);

  return (
    <AnimatePresence>
      {show && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          className="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4"
          onClick={() => setShow(false)}
        >
          <motion.div
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            exit={{ scale: 0.8, opacity: 0 }}
            transition={{ type: 'spring', damping: 20 }}
            onClick={(e) => e.stopPropagation()}
            className="relative w-full max-w-md bg-gradient-to-br from-indigo-600 via-purple-600 to-pink-500 rounded-3xl p-8 text-white shadow-2xl"
          >
            <button onClick={() => setShow(false)} className="absolute top-4 right-4 p-1 hover:bg-white/20 rounded-full transition-colors">
              <X size={20} />
            </button>

            <div className="text-center">
              <div className="w-16 h-16 mx-auto mb-4 bg-white/20 rounded-2xl flex items-center justify-center">
                <Gift size={32} />
              </div>
              <h2 className="text-2xl font-bold mb-2">
                {t('popup.exitTitle', 'Khoan đã! 🎁')}
              </h2>
              <p className="text-white/80 mb-6">
                {t('popup.exitDesc', 'Nhận ngay mã giảm giá 10% cho đơn hàng đầu tiên!')}
              </p>

              <div className="bg-white/10 rounded-2xl p-4 mb-6 border border-white/20">
                <p className="text-sm text-white/60 mb-2">{t('popup.yourCode', 'Mã giảm giá của bạn')}</p>
                <div className="flex items-center justify-center gap-3">
                  <span className="text-2xl font-mono font-bold tracking-[0.3em]">{COUPON_CODE}</span>
                  <button
                    onClick={handleCopy}
                    className="p-2 bg-white/20 rounded-lg hover:bg-white/30 transition-colors"
                  >
                    {copied ? <Check size={18} /> : <Copy size={18} />}
                  </button>
                </div>
              </div>

              <button
                onClick={() => setShow(false)}
                className="w-full py-3 bg-white text-indigo-700 font-bold rounded-xl hover:bg-white/90 transition-colors"
              >
                {t('popup.continueShopping', 'Tiếp tục mua sắm')}
              </button>
              <button
                onClick={() => setShow(false)}
                className="mt-3 text-sm text-white/50 hover:text-white/80 transition-colors"
              >
                {t('popup.noThanks', 'Không, cảm ơn')}
              </button>
            </div>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};
