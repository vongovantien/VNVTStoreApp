import React from 'react';
import { AlertTriangle, Home, RefreshCw } from 'lucide-react';
import { Button } from '@/components/ui';

interface GlobalErrorProps {
  error: Error;
  resetErrorBoundary: () => void;
}

export const GlobalError: React.FC<GlobalErrorProps> = ({ error, resetErrorBoundary }) => {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-slate-50 dark:bg-slate-900 p-4">
      <div className="max-w-md w-full bg-white dark:bg-slate-800 rounded-2xl shadow-xl p-8 text-center border border-slate-100 dark:border-slate-700">
        <div className="w-16 h-16 bg-red-100 dark:bg-red-900/30 rounded-full flex items-center justify-center text-red-600 mx-auto mb-6">
          <AlertTriangle size={32} />
        </div>
        
        <h2 className="text-2xl font-bold text-slate-900 dark:text-white mb-2">
          Đã có sự cố xảy ra
        </h2>
        
        <p className="text-slate-500 dark:text-slate-400 mb-6">
          Chúng tôi rất tiếc vì sự bất tiện này. Ứng dụng đã gặp phải một lỗi không mong muốn.
        </p>

        <div className="bg-slate-100 dark:bg-slate-900 rounded-lg p-4 mb-6 text-left overflow-auto max-h-40">
          <code className="text-xs text-red-500 font-mono break-all">
            {error.message || 'Unknown error occurred'}
          </code>
        </div>

        <div className="flex gap-3 justify-center">
          <Button 
            variant="outline" 
            onClick={() => window.location.href = '/'}
            leftIcon={<Home size={16} />}
          >
            Trang chủ
          </Button>
          <Button 
            onClick={resetErrorBoundary}
            leftIcon={<RefreshCw size={16} />}
          >
            Thử lại
          </Button>
        </div>
      </div>
    </div>
  );
};
