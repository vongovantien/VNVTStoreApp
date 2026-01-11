import { memo, type ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { AlertTriangle, Loader2, CheckCircle, Info } from 'lucide-react';
import { cn } from '@/utils/cn';

export interface ConfirmDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  message: string | ReactNode;
  confirmText?: string;
  cancelText?: string;
  variant?: 'danger' | 'warning' | 'info' | 'success';
  isLoading?: boolean;
  icon?: ReactNode;
  hideCancel?: boolean; // For info-only dialogs
}

const variantStyles = {
  danger: {
    iconBg: 'bg-red-100 dark:bg-red-500/10',
    iconColor: 'text-red-600 dark:text-red-400',
    confirmBtn: 'bg-red-600 hover:bg-red-500 dark:bg-red-500 dark:hover:bg-red-400',
  },
  warning: {
    iconBg: 'bg-yellow-100 dark:bg-yellow-500/10',
    iconColor: 'text-yellow-600 dark:text-yellow-400',
    confirmBtn: 'bg-yellow-600 hover:bg-yellow-500 dark:bg-yellow-500 dark:hover:bg-yellow-400',
  },
  info: {
    iconBg: 'bg-blue-100 dark:bg-blue-500/10',
    iconColor: 'text-blue-600 dark:text-blue-400',
    confirmBtn: 'bg-blue-600 hover:bg-blue-500 dark:bg-blue-500 dark:hover:bg-blue-400',
  },
  success: {
    iconBg: 'bg-green-100 dark:bg-green-500/10',
    iconColor: 'text-green-600 dark:text-green-400',
    confirmBtn: 'bg-indigo-600 hover:bg-indigo-500 dark:bg-indigo-500 dark:hover:bg-indigo-400',
  },
};

export const ConfirmDialog = memo(({
  isOpen,
  onClose,
  onConfirm,
  title,
  message,
  confirmText = 'Xác nhận',
  cancelText = 'Hủy',
  variant = 'danger',
  isLoading = false,
  icon,
  hideCancel = false,
}: ConfirmDialogProps) => {
  const styles = variantStyles[variant];

  // Default icon based on variant
  const defaultIcon = variant === 'success'
    ? <CheckCircle className={cn('size-6', styles.iconColor)} />
    : variant === 'info'
      ? <Info className={cn('size-6', styles.iconColor)} />
      : <AlertTriangle className={cn('size-6', styles.iconColor)} />;

  const dialogContent = (
    <AnimatePresence>
      {isOpen && (
        <div className="fixed inset-0 z-50 overflow-y-auto">
          {/* Backdrop */}
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.2 }}
            className="fixed inset-0 bg-gray-500/75 dark:bg-gray-900/80"
            onClick={onClose}
          />

          {/* Dialog container */}
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <motion.div
              initial={{ opacity: 0, scale: 0.95, y: 10 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.95, y: 10 }}
              transition={{ duration: 0.2 }}
              className={cn(
                'relative transform overflow-hidden rounded-lg text-left shadow-xl sm:my-8 sm:w-full sm:max-w-lg',
                'bg-white dark:bg-slate-800',
                'dark:ring-1 dark:ring-slate-700'
              )}
              onClick={(e) => e.stopPropagation()}
            >
              {/* Content */}
              <div className="bg-white dark:bg-gray-800 px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                <div className="sm:flex sm:items-start">
                  {/* Icon */}
                  <div className={cn(
                    'mx-auto flex size-12 shrink-0 items-center justify-center rounded-full sm:mx-0 sm:size-10',
                    styles.iconBg
                  )}>
                    {icon || defaultIcon}
                  </div>

                  {/* Text content */}
                  <div className="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left">
                    <h3 className="text-base font-semibold text-gray-900 dark:text-white">
                      {title}
                    </h3>
                    <div className="mt-2">
                      {typeof message === 'string' ? (
                        <p className="text-sm text-gray-500 dark:text-gray-400">
                          {message}
                        </p>
                      ) : (
                        message
                      )}
                    </div>
                  </div>
                </div>
              </div>

              {/* Actions */}
              <div className="bg-gray-50 dark:bg-slate-800/50 px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6 dark:border-t dark:border-slate-700">
                <button
                  type="button"
                  onClick={onConfirm}
                  disabled={isLoading}
                  className={cn(
                    'inline-flex w-full justify-center rounded-md px-3 py-2 text-sm font-semibold text-white shadow-sm sm:ml-3 sm:w-auto',
                    'disabled:opacity-50 disabled:cursor-not-allowed',
                    styles.confirmBtn
                  )}
                >
                  {isLoading ? (
                    <>
                      <Loader2 size={16} className="animate-spin mr-2" />
                      Đang xử lý...
                    </>
                  ) : (
                    confirmText
                  )}
                </button>
                {!hideCancel && (
                  <button
                    type="button"
                    onClick={onClose}
                    disabled={isLoading}
                    className={cn(
                      'mt-3 inline-flex w-full justify-center rounded-md px-3 py-2 text-sm font-semibold shadow-sm sm:mt-0 sm:w-auto',
                      'bg-white text-gray-900 ring-1 ring-inset ring-gray-300 hover:bg-gray-50',
                      'dark:bg-slate-700 dark:text-white dark:ring-slate-600 dark:hover:bg-slate-600',
                      'disabled:opacity-50 disabled:cursor-not-allowed'
                    )}
                  >
                    {cancelText}
                  </button>
                )}
              </div>
            </motion.div>
          </div>
        </div>
      )}
    </AnimatePresence>
  );

  return createPortal(dialogContent, document.body);
});

ConfirmDialog.displayName = 'ConfirmDialog';

export default ConfirmDialog;
