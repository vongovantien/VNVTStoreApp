import { useState, useRef, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Globe, Check } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { cn } from '@/utils/cn';
import { Button } from '@/components/ui';

interface LanguageSwitcherProps {
  className?: string;
  variant?: 'ghost' | 'outline' | 'primary';
  showLabel?: boolean;
  align?: 'left' | 'right';
}

export const LanguageSwitcher = ({ 
  className, 
  variant = 'ghost', 
  showLabel = true,
  align = 'right'
}: LanguageSwitcherProps) => {
  const { i18n, t } = useTranslation();
  const [showMenu, setShowMenu] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  const languages = [
    { code: 'vi', label: 'Tiếng Việt', flag: '🇻🇳', short: 'VN' },
    { code: 'en', label: 'English', flag: '🇺🇸', short: 'US' }
  ];

  const currentLang = languages.find(l => (i18n.language || 'vi').startsWith(l.code)) || languages[0];

  const changeLanguage = (lang: string) => {
    i18n.changeLanguage(lang);
    localStorage.setItem('language', lang);
    setShowMenu(false);
  };

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setShowMenu(false);
      }
    };
    if (showMenu) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [showMenu]);

  return (
    <div className={cn('relative', className)} ref={containerRef}>
      <Button
        variant={variant}
        size="sm"
        onClick={() => setShowMenu(!showMenu)}
        className="flex items-center gap-2 px-2"
        title={t('common.switchLanguage', 'Switch Language')}
      >
        <Globe size={18} />
        {showLabel && (
          <span className="text-xs font-semibold uppercase">{currentLang.short}</span>
        )}
      </Button>

      <AnimatePresence>
        {showMenu && (
          <motion.div
            initial={{ opacity: 0, scale: 0.95, y: 10 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.95, y: 10 }}
            transition={{ duration: 0.1 }}
            className={cn(
              "absolute top-full mt-2 bg-white dark:bg-slate-800 rounded-lg shadow-xl border border-slate-200 dark:border-slate-700 p-1 min-w-[140px] z-[9999]",
              align === 'right' ? 'right-0' : 'left-0'
            )}
          >
            {languages.map((lang) => {
              const isActive = (i18n.language || 'vi').startsWith(lang.code);
              return (
                <button
                  key={lang.code}
                  onClick={() => changeLanguage(lang.code)}
                  className={cn(
                    'flex items-center justify-between w-full px-3 py-2 text-sm rounded-md transition-colors',
                    isActive
                      ? 'bg-indigo-100 dark:bg-indigo-900/50 text-indigo-700 dark:text-indigo-300'
                      : 'hover:bg-slate-100 dark:hover:bg-slate-700/50 text-slate-700 dark:text-slate-200'
                  )}
                >
                  <div className="flex items-center gap-2">
                    <span>{lang.flag}</span>
                    <span>{lang.label}</span>
                  </div>
                  {isActive && <Check size={14} />}
                </button>
              );
            })}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default LanguageSwitcher;
