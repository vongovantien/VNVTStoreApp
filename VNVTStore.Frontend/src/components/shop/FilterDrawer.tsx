import { memo } from 'react';
import type { ReactNode } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X, SlidersHorizontal } from 'lucide-react';
import { createPortal } from 'react-dom';
import { cn } from '@/utils/cn';
import { Button } from '@/components/ui';
import { useTranslation } from 'react-i18next';

interface FilterDrawerProps {
  isOpen: boolean;
  onClose: () => void;
  children: ReactNode;
  activeFilterCount?: number;
  onClearAll?: () => void;
}

/**
 * Mobile bottom-sheet filter drawer.
 * Renders via portal so it sits above all page content.
 * On desktop (lg+) this component is unused — the inline sidebar takes over.
 */
export const FilterDrawer = memo(({
  isOpen,
  onClose,
  children,
  activeFilterCount = 0,
  onClearAll,
}: FilterDrawerProps) => {
  const { t } = useTranslation();

  const content = (
    <AnimatePresence>
      {isOpen && (
        <>
          {/* Backdrop */}
          <motion.div
            key="backdrop"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.2 }}
            className="fixed inset-0 z-[200] bg-black/40 backdrop-blur-sm lg:hidden"
            onClick={onClose}
          />

          {/* Sheet */}
          <motion.div
            key="sheet"
            initial={{ y: '100%' }}
            animate={{ y: 0 }}
            exit={{ y: '100%' }}
            transition={{ type: 'spring', damping: 30, stiffness: 300 }}
            className={cn(
              'fixed bottom-0 left-0 right-0 z-[201] lg:hidden',
              'bg-bg-primary rounded-t-2xl shadow-2xl',
              'flex flex-col max-h-[85dvh]',
            )}
          >
            {/* Handle */}
            <div className="flex justify-center pt-3 pb-1 flex-shrink-0">
              <div className="w-10 h-1 rounded-full bg-border" />
            </div>

            {/* Header */}
            <div className="flex items-center justify-between px-5 py-3 border-b border-border flex-shrink-0">
              <div className="flex items-center gap-2">
                <SlidersHorizontal size={18} className="text-accent" />
                <h3 className="font-semibold text-text-primary">
                  {t('filter.filters', 'Bộ lọc')}
                  {activeFilterCount > 0 && (
                    <span className="ml-2 inline-flex items-center justify-center w-5 h-5 rounded-full bg-accent text-white text-[10px] font-bold">
                      {activeFilterCount}
                    </span>
                  )}
                </h3>
              </div>
              <div className="flex items-center gap-2">
                {activeFilterCount > 0 && onClearAll && (
                  <Button variant="ghost" size="xs" onClick={onClearAll} className="text-error">
                    {t('filter.clearAll', 'Xóa tất cả')}
                  </Button>
                )}
                <button
                  onClick={onClose}
                  className="p-1.5 rounded-lg hover:bg-bg-tertiary transition-colors text-text-secondary"
                  aria-label="Đóng bộ lọc"
                >
                  <X size={20} />
                </button>
              </div>
            </div>

            {/* Scrollable content */}
            <div className="flex-1 overflow-y-auto overscroll-contain px-5 py-4">
              {children}
            </div>

            {/* Apply button */}
            <div className="px-5 py-4 border-t border-border flex-shrink-0 bg-bg-primary">
              <Button fullWidth onClick={onClose}>
                {t('filter.applyFilters', 'Áp dụng bộ lọc')}
                {activeFilterCount > 0 && ` (${activeFilterCount})`}
              </Button>
            </div>
          </motion.div>
        </>
      )}
    </AnimatePresence>
  );

  return createPortal(content, document.body);
});

FilterDrawer.displayName = 'FilterDrawer';
export default FilterDrawer;
