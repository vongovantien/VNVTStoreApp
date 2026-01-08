import { memo } from 'react';
import { createPortal } from 'react-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { CheckCircle, XCircle, AlertTriangle, Info, X } from 'lucide-react';
import { cn } from '@/utils/cn';
import { useToastStore, type ToastType } from '@/store/toastStore';

const toastStyles: Record<ToastType, { 
  bg: string; 
  icon: typeof CheckCircle; 
  iconColor: string;
  borderColor: string;
}> = {
  success: {
    bg: 'bg-slate-800 dark:bg-slate-800',
    icon: CheckCircle,
    iconColor: 'text-green-400',
    borderColor: 'border-green-500/30',
  },
  error: {
    bg: 'bg-slate-800 dark:bg-slate-800',
    icon: XCircle,
    iconColor: 'text-red-400',
    borderColor: 'border-red-500/30',
  },
  warning: {
    bg: 'bg-slate-800 dark:bg-slate-800',
    icon: AlertTriangle,
    iconColor: 'text-yellow-400',
    borderColor: 'border-yellow-500/30',
  },
  info: {
    bg: 'bg-slate-800 dark:bg-slate-800',
    icon: Info,
    iconColor: 'text-blue-400',
    borderColor: 'border-blue-500/30',
  },
};

export const ToastContainer = memo(() => {
  const { toasts, removeToast } = useToastStore();

  const content = (
    <div className="fixed top-4 right-4 z-[100] flex flex-col gap-3 max-w-md w-full pointer-events-none">
      <AnimatePresence mode="popLayout">
        {toasts.map((toast) => {
          const style = toastStyles[toast.type];
          const Icon = style.icon;

          return (
            <motion.div
              key={toast.id}
              layout
              initial={{ opacity: 0, x: 100, scale: 0.9 }}
              animate={{ opacity: 1, x: 0, scale: 1 }}
              exit={{ opacity: 0, x: 100, scale: 0.9 }}
              transition={{ type: 'spring', damping: 25, stiffness: 300 }}
              className={cn(
                'pointer-events-auto flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg border',
                style.bg,
                style.borderColor
              )}
            >
              <Icon className={cn('w-5 h-5 flex-shrink-0', style.iconColor)} />
              <p className="flex-1 text-sm text-white">{toast.message}</p>
              <button
                onClick={() => removeToast(toast.id)}
                className="p-1 hover:bg-white/10 rounded transition-colors flex-shrink-0"
              >
                <X size={16} className="text-gray-400" />
              </button>
            </motion.div>
          );
        })}
      </AnimatePresence>
    </div>
  );

  return createPortal(content, document.body);
});

ToastContainer.displayName = 'ToastContainer';

export default ToastContainer;
