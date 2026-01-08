import { memo, useEffect, useCallback, type ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { X } from 'lucide-react';
import { cn } from '@/utils/cn';
import { Button } from './Button';

// ============ Modal Types ============
export type ModalSize = 'sm' | 'md' | 'lg' | 'xl' | 'full';

// ============ Modal Props Interface ============
export interface ModalProps {
  /** Modal open state */
  isOpen: boolean;
  /** Close handler */
  onClose: () => void;
  /** Modal title */
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
                  <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200">
                    {title && (
                      <h3 className="text-lg font-semibold text-primary">{title}</h3>
                    )}
                    {showCloseButton && (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={onClose}
                        className="ml-auto -mr-2"
                        aria-label="Close modal"
                      >
                        <X size={20} />
                      </Button>
                    )}
                  </div>
                )}

                {/* Body */}
                <div className="px-6 py-4 max-h-[60vh] overflow-y-auto">{children}</div>

                {/* Footer */}
                {footer && (
                  <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-gray-200 bg-secondary/30 rounded-b-2xl">
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
