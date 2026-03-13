import React from 'react';
import { AlertCircle, RefreshCw } from 'lucide-react';

interface InlineErrorProps {
  error: Error;
  resetErrorBoundary: () => void;
}

export const InlineError: React.FC<InlineErrorProps> = ({ error, resetErrorBoundary }) => {
  return (
    <div className="flex items-center gap-2 text-red-600 bg-red-50 px-3 py-2 rounded-md border border-red-100 text-sm">
      <AlertCircle size={14} />
      <span className="flex-1 truncate">{error.message}</span>
      <button 
        onClick={resetErrorBoundary} 
        className="p-1 hover:bg-red-100 rounded-full transition-colors"
        title="Thử lại"
      >
        <RefreshCw size={12} />
      </button>
    </div>
  );
};
