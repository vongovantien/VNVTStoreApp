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
  shadow: string;
}> = {
  success: {
    bg: 'bg-white/90 dark:bg-slate-900/90 backdrop-blur-xl',
    icon: CheckCircle,
    iconColor: 'text-emerald-500',
    borderColor: 'border-emerald-500/20',
    shadow: 'shadow-[0_8px_30px_rgb(16,185,129,0.15)]',
  },
  error: {
    bg: 'bg-white/90 dark:bg-slate-900/90 backdrop-blur-xl',
    icon: XCircle,
    iconColor: 'text-rose-500',
    borderColor: 'border-rose-500/20',
    shadow: 'shadow-[0_8px_30px_rgb(244,63,94,0.15)]',
  },
  warning: {
    bg: 'bg-white/90 dark:bg-slate-900/90 backdrop-blur-xl',
    icon: AlertTriangle,
    iconColor: 'text-amber-500',
    borderColor: 'border-amber-500/20',
    shadow: 'shadow-[0_8px_30px_rgb(245,158,11,0.15)]',
  },
  info: {
    bg: 'bg-white/90 dark:bg-slate-900/90 backdrop-blur-xl',
    icon: Info,
    iconColor: 'text-sky-500',
    borderColor: 'border-sky-500/20',
    shadow: 'shadow-[0_8px_30px_rgb(14,165,233,0.15)]',
  },
};

export const ToastContainer = memo(() => {
  const { toasts, removeToast } = useToastStore();

  const content = (
    <div className="fixed bottom-6 right-6 z-[200] flex flex-col gap-4 max-w-sm w-full pointer-events-none">
      <AnimatePresence mode="popLayout">
        {toasts.map((toast) => {
          const style = toastStyles[toast.type];
          const Icon = style.icon;

          return (
            <motion.div
              key={toast.id}
              layout
              initial={{ opacity: 0, y: 20, scale: 0.9, x: 20 }}
              animate={{ opacity: 1, y: 0, scale: 1, x: 0 }}
              exit={{ opacity: 0, scale: 0.9, transition: { duration: 0.2 } }}
              transition={{ type: 'spring', damping: 20, stiffness: 300 }}
              className={cn(
                'pointer-events-auto group flex items-start gap-4 px-5 py-4 rounded-2xl border transition-all duration-300',
                style.bg,
                style.borderColor,
                style.shadow
              )}
            >
              <div className={cn('p-2 rounded-xl bg-slate-50 dark:bg-slate-800/50 flex-shrink-0')}>
                <Icon className={cn('w-5 h-5', style.iconColor)} />
              </div>
              <div className="flex-1 pt-1">
                <p className="text-sm font-semibold text-slate-900 dark:text-white leading-tight">
                    {toast.type === 'success' ? 'Thành công!' : 
                     toast.type === 'error' ? 'Lỗi hệ thống' : 
                     toast.type === 'warning' ? 'Cảnh báo' : 'Thông tin'}
                </p>
                <p className="text-xs text-slate-500 dark:text-slate-400 mt-1 line-clamp-2">{toast.message}</p>
              </div>
              <button
                onClick={() => removeToast(toast.id)}
                className="p-2 opacity-0 group-hover:opacity-100 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-xl transition-all duration-300 text-slate-400"
              >
                <X size={16} />
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
