import React, { useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X } from 'lucide-react';
import { createPortal } from 'react-dom';
import { cn } from '@/utils/cn';

interface DrawerProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  children: React.ReactNode;
  position?: 'left' | 'right' | 'bottom';
  size?: 'sm' | 'md' | 'lg' | 'xl' | 'full';
  className?: string;
}

const variants = {
  left: {
    initial: { x: '-100%' },
    animate: { x: 0 },
    exit: { x: '-100%' },
  },
  right: {
    initial: { x: '100%' },
    animate: { x: 0 },
    exit: { x: '100%' },
  },
  bottom: {
    initial: { y: '100%' },
    animate: { y: 0 },
    exit: { y: '100%' },
  },
};

const sizes = {
  sm: 'max-w-sm',
  md: 'max-w-md',
  lg: 'max-w-lg',
  xl: 'max-w-xl',
  full: 'max-w-full',
};

export const Drawer: React.FC<DrawerProps> = ({
  isOpen,
  onClose,
  title,
  children,
  position = 'right',
  size = 'md',
  className,
}) => {
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  const drawerContent = (
    <AnimatePresence>
      {isOpen && (
        <>
          {/* Backdrop */}
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={onClose}
            className="fixed inset-0 bg-black/50 z-[9998] backdrop-blur-sm"
          />

          {/* Drawer Panel */}
          <motion.div
            variants={variants[position]}
            initial="initial"
            animate="animate"
            exit="exit"
            transition={{ type: 'spring', damping: 25, stiffness: 200 }}
            className={cn(
              'fixed bg-white dark:bg-slate-900 shadow-2xl z-[9999] flex flex-col',
              position === 'left' && 'top-0 bottom-0 left-0 h-full',
              position === 'right' && 'top-0 bottom-0 right-0 h-full',
              position === 'bottom' && 'left-0 right-0 bottom-0 rounded-t-2xl max-h-[90vh]',
              position !== 'bottom' && sizes[size],
              position !== 'bottom' ? 'w-full' : 'w-full',
              className
            )}
          >
            {/* Header */}
            <div className="flex items-center justify-between p-4 border-b border-slate-100 dark:border-slate-800">
              <h2 className="text-lg font-bold text-slate-900 dark:text-white truncate">
                {title}
              </h2>
              <button
                onClick={onClose}
                className="p-2 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-full transition-colors"
              >
                <X size={20} className="text-slate-500" />
              </button>
            </div>

            {/* Body */}
            <div className="flex-1 overflow-y-auto p-4 custom-scrollbar">
              {children}
            </div>
          </motion.div>
        </>
      )}
    </AnimatePresence>
  );

  return createPortal(drawerContent, document.body);
};
