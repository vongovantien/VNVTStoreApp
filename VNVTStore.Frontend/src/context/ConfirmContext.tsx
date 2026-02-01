import React, { createContext, useContext, useState, useCallback, useRef } from 'react';
import { AlertCircle, CheckCircle2, XCircle } from 'lucide-react';
import { cn } from '@/utils/cn';

interface ConfirmOptions {
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'info' | 'danger' | 'warning' | 'success';
}

interface ConfirmContextType {
  confirm: (options: ConfirmOptions) => Promise<boolean>;
}

const ConfirmContext = createContext<ConfirmContextType | undefined>(undefined);

export const useConfirm = () => {
  const context = useContext(ConfirmContext);
  if (!context) {
    throw new Error('useConfirm must be used within a ConfirmProvider');
  }
  return context;
};

export const ConfirmProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [options, setOptions] = useState<ConfirmOptions>({ message: '' });
  const resolveRef = useRef<(value: boolean) => void>(() => {});

  const confirm = useCallback((options: ConfirmOptions) => {
    setOptions(options);
    setIsOpen(true);
    return new Promise<boolean>((resolve) => {
      resolveRef.current = resolve;
    });
  }, []);

  const handleConfirm = useCallback(() => {
    setIsOpen(false);
    resolveRef.current(true);
  }, []);

  const handleCancel = useCallback(() => {
    setIsOpen(false);
    resolveRef.current(false);
  }, []);

  return (
    <ConfirmContext.Provider value={{ confirm }}>
      {children}
      {isOpen && (
        <div className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/50 backdrop-blur-sm animate-in fade-in duration-200">
          <div className="bg-white dark:bg-slate-900 rounded-xl shadow-2xl w-full max-w-md mx-4 p-6 scale-100 animate-in zoom-in-95 duration-200 border border-slate-200 dark:border-slate-800">
            <div className="flex flex-col items-center text-center gap-4">
              {options.type === 'danger' && (
                <div className="w-12 h-12 rounded-full bg-red-100 dark:bg-red-900/30 flex items-center justify-center text-red-600 dark:text-red-400">
                  <XCircle size={28} />
                </div>
              )}
              {options.type === 'warning' && (
                 <div className="w-12 h-12 rounded-full bg-amber-100 dark:bg-amber-900/30 flex items-center justify-center text-amber-600 dark:text-amber-400">
                  <AlertCircle size={28} />
                </div>
              )}
              {(!options.type || options.type === 'info') && (
                 <div className="w-12 h-12 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center text-blue-600 dark:text-blue-400">
                  <AlertCircle size={28} />
                </div>
              )}
               {options.type === 'success' && (
                 <div className="w-12 h-12 rounded-full bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center text-emerald-600 dark:text-emerald-400">
                  <CheckCircle2 size={28} />
                </div>
              )}

              <div className="space-y-2">
                <h3 className="text-xl font-semibold text-slate-900 dark:text-slate-100">
                  {options.title || 'Xác nhận'}
                </h3>
                <p className="text-slate-500 dark:text-slate-400 text-sm leading-relaxed">
                  {options.message}
                </p>
              </div>

              <div className="flex gap-3 w-full mt-2">
                <button
                  onClick={handleCancel}
                  className="flex-1 px-4 py-2.5 rounded-lg border border-slate-200 dark:border-slate-700 font-medium text-slate-700 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors focus:ring-2 focus:ring-slate-200 focus:outline-none"
                >
                  {options.cancelText || 'Hủy'}
                </button>
                <button
                  onClick={handleConfirm}
                  className={cn(
                    "flex-1 px-4 py-2.5 rounded-lg font-medium text-white shadow-lg shadow-blue-500/20 transition-all active:scale-95 focus:ring-2 focus:ring-offset-2 focus:outline-none",
                    options.type === 'danger' ? "bg-red-600 hover:bg-red-700 focus:ring-red-500" :
                    options.type === 'warning' ? "bg-amber-600 hover:bg-amber-700 focus:ring-amber-500" :
                    "bg-blue-600 hover:bg-blue-700 focus:ring-blue-500"
                  )}
                >
                  {options.confirmText || 'Đồng ý'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </ConfirmContext.Provider>
  );
};
