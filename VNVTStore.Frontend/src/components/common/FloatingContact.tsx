import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { ArrowUp, MessageCircle, X as CloseIcon } from 'lucide-react';
import { cn } from '@/utils/cn';
import { ChatWidget } from './ChatWidget';
import { useQuery } from '@tanstack/react-query';
import { systemConfigService } from '@/services/systemConfigService';

// Default social link configs (used as fallback if not set in admin settings)
const DEFAULT_PHONE        = '0901234567';
const DEFAULT_FACEBOOK_URL = 'https://www.facebook.com/vnvtstore';
const DEFAULT_TIKTOK_URL   = 'https://www.tiktok.com/@vnvtstore';
const DEFAULT_MESSENGER_URL = 'https://m.me/vnvtstore';
const DEFAULT_ZALO_NUMBER  = '0901234567';
const DEFAULT_MAPS_URL     = 'https://maps.google.com/?q=VNVT+Store';

// ─── Inline SVG icons (brand logos, không có trong lucide) ──────────────────
const PhoneWaveIcon = () => (
  <svg viewBox="0 0 24 24" fill="currentColor" width="22" height="22">
    <path d="M6.6 10.8c1.4 2.8 3.8 5.1 6.6 6.6l2.2-2.2c.3-.3.7-.4 1-.2 1.1.4 2.3.6 3.6.6.6 0 1 .4 1 1V20c0 .6-.4 1-1 1C10.6 21 3 13.4 3 4c0-.6.4-1 1-1h3.5c.6 0 1 .4 1 1 0 1.3.2 2.5.6 3.6.1.3 0 .7-.2 1L6.6 10.8z"/>
  </svg>
);

const FacebookIcon = () => (
  <svg viewBox="0 0 24 24" fill="currentColor" width="22" height="22">
    <path d="M24 12.073C24 5.405 18.627 0 12 0S0 5.405 0 12.073C0 18.1 4.388 23.094 10.125 24v-8.437H7.078v-3.49h3.047V9.41c0-3.025 1.792-4.697 4.533-4.697 1.312 0 2.686.236 2.686.236v2.97h-1.513c-1.491 0-1.956.93-1.956 1.874v2.25h3.328l-.532 3.49h-2.796V24C19.612 23.094 24 18.1 24 12.073z"/>
  </svg>
);

const TikTokIcon = () => (
  <svg viewBox="0 0 24 24" fill="currentColor" width="20" height="20">
    <path d="M19.59 6.69a4.83 4.83 0 01-3.77-4.25V2h-3.45v13.67a2.89 2.89 0 01-2.88 2.5 2.89 2.89 0 01-2.89-2.89 2.89 2.89 0 012.89-2.89c.28 0 .54.04.79.1V9.01a6.34 6.34 0 00-.79-.05 6.34 6.34 0 00-6.34 6.34 6.34 6.34 0 006.34 6.34 6.34 6.34 0 006.33-6.34V9.05a8.16 8.16 0 004.77 1.52V7.12a4.85 4.85 0 01-1-.43z"/>
  </svg>
);

const MessengerIcon = () => (
  <svg viewBox="0 0 24 24" fill="currentColor" width="22" height="22">
    <path d="M12 0C5.4 0 0 5.1 0 11.4c0 3.6 1.8 6.8 4.6 8.9V24l4.2-2.3c1.1.3 2.1.5 3.2.5 6.6 0 12-5.1 12-11.4C24 5.1 18.6 0 12 0zm1.2 15.4l-3-3.2-6 3.2 6.5-6.9 3.1 3.2 5.9-3.2-6.5 6.9z"/>
  </svg>
);

const ZaloIcon = () => (
  <svg viewBox="0 0 50 50" fill="currentColor" width="22" height="22">
    <path d="M25 2C12.3 2 2 12.3 2 25s10.3 23 23 23 23-10.3 23-23S37.7 2 25 2zm-5.3 31.6H17v-9.5h2.7v9.5zm-1.4-10.7c-.9 0-1.5-.6-1.5-1.4s.6-1.4 1.5-1.4 1.5.6 1.5 1.4-.6 1.4-1.5 1.4zm14.2 10.7h-2.7v-5c0-1.3-.5-2.1-1.6-2.1-.9 0-1.4.6-1.6 1.2-.1.2-.1.5-.1.7v5.2H24v-6.3c0-1.1 0-2-.1-2.8h2.4l.1 1.2c.6-.9 1.5-1.5 2.8-1.5 1.9 0 3.4 1.2 3.4 3.9v5.5z"/>
  </svg>
);

const MapsIcon = () => (
  <svg viewBox="0 0 24 24" width="22" height="22">
    <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z" fill="#EA4335"/>
    <path d="M12 2v7.5c1.38 0 2.5 1.12 2.5 2.5S13.38 14.5 12 14.5V22s7-7.75 7-13c0-3.87-3.13-7-7-7z" fill="#34A853" opacity=".6"/>
  </svg>
);

// ─── Button config ────────────────────────────────────────────────────────────
interface ContactButton {
  id: string;
  label: string;
  icon: React.ReactNode;
  /** bg class for the button */
  bg: string;
  /** ring / shadow pulse class */
  ring: string;
  href?: string;
  onClick?: () => void;
}

// ─── Tooltip ─────────────────────────────────────────────────────────────────
const Tooltip = ({ label }: { label: string }) => (
  <motion.div
    initial={{ opacity: 0, x: 8 }}
    animate={{ opacity: 1, x: 0 }}
    exit={{ opacity: 0, x: 8 }}
    transition={{ duration: 0.15 }}
    className="absolute right-[calc(100%+12px)] top-1/2 -translate-y-1/2 whitespace-nowrap bg-slate-900 text-white text-xs font-medium px-2.5 py-1.5 rounded-lg shadow-lg pointer-events-none select-none"
  >
    {label}
    {/* Arrow */}
    <span className="absolute right-[-5px] top-1/2 -translate-y-1/2 border-4 border-transparent border-l-slate-900" />
  </motion.div>
);

// ─── Single button ────────────────────────────────────────────────────────────
const SocialBtn = ({ btn, delay }: { btn: ContactButton; delay: number }) => {
  const [hovered, setHovered] = useState(false);

  const inner = (
    <motion.button
      type="button"
      whileHover={{ scale: 1.12 }}
      whileTap={{ scale: 0.93 }}
      initial={{ opacity: 0, x: 40 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ delay, type: 'spring', stiffness: 300, damping: 20 }}
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
      onClick={btn.onClick}
      className={cn(
        'relative w-11 h-11 rounded-full flex items-center justify-center text-white shadow-lg',
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2',
        btn.bg, btn.ring,
      )}
      aria-label={btn.label}
    >
      {btn.icon}
      <AnimatePresence>{hovered && <Tooltip label={btn.label} />}</AnimatePresence>
    </motion.button>
  );

  if (btn.href) {
    return (
      <a href={btn.href} target="_blank" rel="noopener noreferrer" tabIndex={-1}>
        {inner}
      </a>
    );
  }
  return inner;
};

// ─── Main component ───────────────────────────────────────────────────────────
export const FloatingContact: React.FC = () => {
  const [showScrollTop, setShowScrollTop] = useState(false);
  const [isChatOpen, setIsChatOpen] = useState(false);
  const [isExpanded, setIsExpanded] = useState(false);

  // Fetch public contact settings from the Backend
  const { data: contactsRes } = useQuery({
    queryKey: ['public-contacts'],
    queryFn: () => systemConfigService.getPublicContacts(),
    staleTime: 5 * 60 * 1000, // cache for 5 minutes
  });

  const getContactValue = useCallback((code: string, fallback: string) => {
    return contactsRes?.data?.[code] || fallback;
  }, [contactsRes]);

  const phone = useMemo(() => getContactValue('CONTACT_PHONE', DEFAULT_PHONE), [getContactValue]);
  const facebookUrl = useMemo(() => getContactValue('CONTACT_FACEBOOK', DEFAULT_FACEBOOK_URL), [getContactValue]);
  const tiktokUrl = useMemo(() => getContactValue('CONTACT_TIKTOK', DEFAULT_TIKTOK_URL), [getContactValue]);
  const messengerUrl = useMemo(() => getContactValue('CONTACT_MESSENGER', DEFAULT_MESSENGER_URL), [getContactValue]);
  const zaloNumber = useMemo(() => getContactValue('CONTACT_ZALO', DEFAULT_ZALO_NUMBER), [getContactValue]);
  const mapsUrl = useMemo(() => getContactValue('CONTACT_MAPS', DEFAULT_MAPS_URL), [getContactValue]);

  const [progress, setProgress] = useState(0);

  // Show scroll-to-top only after scrolling 300px
  useEffect(() => {
    const onScroll = () => {
      const scrolled = window.scrollY;
      setShowScrollTop(scrolled > 300);
      
      const height = document.documentElement.scrollHeight - window.innerHeight;
      if (height > 0) {
        setProgress((scrolled / height) * 100);
      }
    };
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  const scrollToTop = useCallback(() => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }, []);

  const buttons: ContactButton[] = useMemo(() => [
    {
      id: 'phone',
      label: `Hotline: ${phone}`,
      icon: <PhoneWaveIcon />,
      bg: 'bg-orange-500 hover:bg-orange-600',
      ring: 'ring-orange-400',
      href: `tel:${phone}`,
    },
    {
      id: 'facebook',
      label: 'Facebook',
      icon: <FacebookIcon />,
      bg: 'bg-[#1877F2] hover:bg-[#166FE5]',
      ring: 'ring-blue-400',
      href: facebookUrl,
    },
    {
      id: 'tiktok',
      label: 'TikTok',
      icon: <TikTokIcon />,
      bg: 'bg-black hover:bg-slate-800',
      ring: 'ring-slate-400',
      href: tiktokUrl,
    },
    {
      id: 'messenger',
      label: 'Messenger',
      icon: <MessengerIcon />,
      bg: 'bg-gradient-to-b from-[#C632FB] via-[#4867FF] to-[#00C6FF] hover:opacity-90',
      ring: 'ring-purple-400',
      href: messengerUrl,
    },
    {
      id: 'zalo',
      label: 'Zalo',
      icon: <ZaloIcon />,
      bg: 'bg-[#0068FF] hover:bg-[#005CE6]',
      ring: 'ring-blue-400',
      href: zaloNumber.startsWith('http') ? zaloNumber : `https://zalo.me/${zaloNumber}`,
    },
    {
      id: 'maps',
      label: 'Xem bản đồ',
      icon: <MapsIcon />,
      bg: 'bg-white hover:bg-slate-50 !text-slate-700 border border-slate-200',
      ring: 'ring-slate-300',
      href: mapsUrl,
    },
  ], [phone, facebookUrl, tiktokUrl, messengerUrl, zaloNumber, mapsUrl]);

  return (
    <>
      {/* Sidebar bar */}
      <div
        className="fixed right-4 bottom-6 md:bottom-10 z-[102] flex flex-col items-center gap-2.5"
        aria-label="Liên hệ nhanh"
        role="complementary"
      >
        {/* Scroll to top */}
        <AnimatePresence>
          {showScrollTop && (
            <motion.button
              type="button"
              key="scroll-top"
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: 10 }}
              whileHover={{ scale: 1.12 }}
              whileTap={{ scale: 0.93 }}
              onClick={scrollToTop}
              className="relative w-11 h-11 rounded-full flex items-center justify-center bg-white border border-slate-200 text-slate-500 hover:text-primary shadow-lg transition-colors group"
              aria-label="Về đầu trang"
            >
              <svg className="absolute inset-0 w-11 h-11 -rotate-90">
                <circle
                  cx="22"
                  cy="22"
                  r="18"
                  stroke="currentColor"
                  strokeWidth="2"
                  fill="transparent"
                  className="text-slate-100 dark:text-slate-800"
                />
                <circle
                  cx="22"
                  cy="22"
                  r="18"
                  stroke="currentColor"
                  strokeWidth="2"
                  fill="transparent"
                  strokeDasharray={2 * Math.PI * 18}
                  strokeDashoffset={2 * Math.PI * 18 - (progress / 100) * 2 * Math.PI * 18}
                  className="text-primary transition-all duration-300"
                />
              </svg>
              <ArrowUp size={16} className="absolute text-slate-600 group-hover:-translate-y-0.5 transition-transform" />
            </motion.button>
          )}
        </AnimatePresence>

        {/* Social buttons - expandable list */}
        <AnimatePresence>
          {isExpanded && (
            <motion.div
              initial={{ opacity: 0, y: 20, scale: 0.95 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              exit={{ opacity: 0, y: 20, scale: 0.95 }}
              transition={{ duration: 0.2 }}
              className="flex flex-col items-center gap-2.5"
            >
              {buttons.map((btn, i) => (
                <SocialBtn key={btn.id} btn={btn} delay={i * 0.03} />
              ))}
            </motion.div>
          )}
        </AnimatePresence>

        {/* Main Toggle Button */}
        <motion.button
          type="button"
          onClick={() => setIsExpanded(!isExpanded)}
          whileHover={{ scale: 1.08 }}
          whileTap={{ scale: 0.95 }}
          className={cn(
            "w-12 h-12 rounded-full flex items-center justify-center text-white shadow-xl focus:outline-none transition-colors",
            isExpanded ? "bg-slate-700 hover:bg-slate-800" : "bg-gradient-to-tr from-indigo-600 to-violet-600 hover:opacity-90"
          )}
          aria-label={isExpanded ? "Đóng liên hệ" : "Liên hệ nhanh"}
        >
          {isExpanded ? (
            <CloseIcon size={20} className="animate-in spin-in-90 duration-200" />
          ) : (
            <div className="relative">
              <MessageCircle size={20} className="animate-pulse" />
              <span className="absolute -top-1 -right-1 flex h-2 w-2">
                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75"></span>
                <span className="relative inline-flex rounded-full h-2 w-2 bg-red-500"></span>
              </span>
            </div>
          )}
        </motion.button>
      </div>

      <ChatWidget isOpen={isChatOpen} onClose={() => setIsChatOpen(false)} />
    </>
  );
};

export default FloatingContact;
