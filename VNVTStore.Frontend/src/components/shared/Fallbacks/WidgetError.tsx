import React from 'react';
import { AlertCircle, RefreshCw } from 'lucide-react';
import { Button } from '@/components/ui';

interface WidgetErrorProps {
  error: Error;
  resetErrorBoundary: () => void;
  context?: {
    title?: string;
  };
}

export const WidgetError: React.FC<WidgetErrorProps> = ({ error, resetErrorBoundary, context }) => {
  return (
    <div className="flex flex-col items-center justify-center p-4 rounded-lg bg-slate-50 border border-slate-100 dark:bg-slate-800 dark:border-slate-700 min-h-[150px]">
      <div className="w-10 h-10 bg-red-100 dark:bg-red-900/30 rounded-full flex items-center justify-center text-red-500 mb-3">
        <AlertCircle size={20} />
      </div>
      <h4 className="text-sm font-medium text-slate-900 dark:text-slate-100 mb-1">
        {context?.title || 'Đã có lỗi xảy ra'}
      </h4>
      <p className="text-xs text-slate-500 dark:text-slate-400 text-center mb-3 px-2 line-clamp-2">
        {error.message}
      </p>
      <Button 
        variant="ghost" 
        size="sm" 
        onClick={resetErrorBoundary}
        className="h-7 text-xs hover:bg-white dark:hover:bg-slate-700 border border-transparent hover:border-slate-200"
      >
        <RefreshCw size={12} className="mr-1.5" />
        Thử lại
      </Button>
    </div>
  );
};
