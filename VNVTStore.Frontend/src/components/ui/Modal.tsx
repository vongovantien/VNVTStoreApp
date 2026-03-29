import { memo, useEffect, useCallback } from 'react';
import type { ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { X } from 'lucide-react';
import { cn } from '@/utils/cn';
import { Button } from './Button';

// ============ Modal Types ============
export type ModalSize = 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | '4xl' | '5xl' | '6xl' | '7xl' | 'full';

// ============ Modal Props Interface ============
export interface ModalProps {
  /** Modal open state */
  isOpen: boolean;
  /** Close handler */
  onClose: () => void;
  title?: ReactNode;
  /** Modal size */
  size?: ModalSize;
  /** Show close button */
  showCloseButton?: boolean;
  /** Close on overlay click */
  closeOnOverlayClick?: boolean;
  /** Close on escape key */
  closeOnEsc?: boolean;
  /** Center content vertically */
  centered?: boolean;
  /** Footer content */
  footer?: ReactNode;
  /** Children */
  children: ReactNode;
  /** Custom className for content */
  className?: string;
}

// ============ Size Styles ============
const sizeStyles: Record<ModalSize, string> = {
  sm: 'max-w-sm',
  md: 'max-w-md',
  lg: 'max-w-lg',
  xl: 'max-w-xl',
  '2xl': 'max-w-2xl',
  '3xl': 'max-w-3xl',
  '4xl': 'max-w-4xl',
  '5xl': 'max-w-5xl',
  '6xl': 'max-w-6xl',
  '7xl': 'max-w-7xl',
  full: 'max-w-full mx-4',
};

// ============ Modal Component ============
export const Modal = memo(
  ({
    isOpen,
    onClose,
    title,
    size = 'md',
    showCloseButton = true,
    closeOnOverlayClick = true,
    closeOnEsc = true,
    centered = true,
    footer,
    children,
    className,
  }: ModalProps) => {
    // Handle escape key
    const handleEscKey = useCallback(
      (e: KeyboardEvent) => {
        if (closeOnEsc && e.key === 'Escape') {
          onClose();
        }
      },
      [closeOnEsc, onClose]
    );

    // Add/remove escape key listener
    useEffect(() => {
      if (isOpen) {
        document.addEventListener('keydown', handleEscKey);
        document.body.style.overflow = 'hidden';
      }
      return () => {
        document.removeEventListener('keydown', handleEscKey);
        document.body.style.overflow = '';
      };
    }, [isOpen, handleEscKey]);

    // Handle overlay click
    const handleOverlayClick = useCallback(() => {
      if (closeOnOverlayClick) {
        onClose();
      }
    }, [closeOnOverlayClick, onClose]);

    const modalContent = (
      <AnimatePresence>
        {isOpen && (
          <div className="fixed inset-0 z-50 overflow-y-auto">
            {/* Overlay */}
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              transition={{ duration: 0.2 }}
              className="fixed inset-0 bg-black/50 backdrop-blur-sm"
              onClick={handleOverlayClick}
            />

            {/* Modal container */}
            <div
              className={cn(
                'flex min-h-full p-4',
                centered ? 'items-center justify-center' : 'items-start justify-center pt-20'
              )}
            >
              <motion.div
                initial={{ opacity: 0, scale: 0.95, y: 10 }}
                animate={{ opacity: 1, scale: 1, y: 0 }}
                exit={{ opacity: 0, scale: 0.95, y: 10 }}
                transition={{ duration: 0.2 }}
                className={cn(
                  'relative w-full bg-primary rounded-2xl shadow-2xl',
                  sizeStyles[size],
                  className
                )}
                onClick={(e) => e.stopPropagation()}
              >
                {/* Header */}
                {(title || showCloseButton) && (
                  <div className="flex items-center justify-between px-6 py-5 border-b border-border">
                    {title && (
                      <h3 className="text-xl font-bold text-text-primary tracking-tight">{title}</h3>
                    )}
                    {showCloseButton && (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={onClose}
                        className="ml-auto -mr-2 rounded-full w-10 h-10 p-0 hover:bg-bg-tertiary transition-colors"
                        aria-label="Close modal"
                      >
                        <X size={22} className="text-text-secondary" />
                      </Button>
                    )}
                  </div>
                )}

                {/* Body */}
                <div className="px-6 py-6 max-h-[70vh] overflow-y-auto custom-scrollbar">{children}</div>

                {/* Footer */}
                {footer && (
                  <div className="flex items-center justify-end gap-3 px-6 py-5 border-t border-border bg-bg-primary rounded-b-2xl">
                    {footer}
                  </div>
                )}
              </motion.div>
            </div>
          </div>
        )}
      </AnimatePresence>
    );

    // Render using portal
    return createPortal(modalContent, document.body);
  }
);

Modal.displayName = 'Modal';

export default Modal;
