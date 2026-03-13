import { AlertTriangle, RefreshCw, Home, ChevronLeft } from 'lucide-react';
import { Button } from '@/components/ui';
import { motion } from 'framer-motion';

interface ErrorFallbackProps {
  error: Error;
  resetErrorBoundary: () => void;
}

export const ErrorFallback: React.FC<ErrorFallbackProps> = ({ error, resetErrorBoundary }) => {
  return (
    <motion.div 
      initial={{ opacity: 0, scale: 0.9 }}
      animate={{ opacity: 1, scale: 1 }}
      className="max-w-md w-full p-8 rounded-[2rem] bg-white/70 backdrop-blur-2xl border border-white/40 shadow-[0_20px_50px_rgba(0,0,0,0.05)] flex flex-col items-center text-center space-y-6 relative overflow-hidden"
    >
      {/* Decorative background gradients */}
      <div className="absolute -top-10 -right-10 w-32 h-32 bg-primary/10 rounded-full blur-2xl flex-shrink-0" />
      <div className="absolute -bottom-10 -left-10 w-32 h-32 bg-accent/10 rounded-full blur-2xl flex-shrink-0" />

      <div className="w-20 h-20 bg-gradient-to-br from-red-50 to-red-100 rounded-2xl flex items-center justify-center text-red-500 shadow-inner relative z-10">
        <AlertTriangle size={38} strokeWidth={1.5} />
      </div>
      
      <div className="space-y-2 relative z-10">
        <h3 className="text-2xl font-bold text-slate-900 tracking-tight">Hệ thống gặp sự cố</h3>
        <p className="text-slate-500 text-sm leading-relaxed max-w-[280px] mx-auto font-medium">
          {error.message || 'Chúng tôi không thể tải nội dung này ngay bây giờ. Vui lòng thử lại sau.'}
        </p>
      </div>

      <div className="flex flex-col w-full gap-3 pt-2 relative z-10">
        <Button 
          variant="primary" 
          size="lg" 
          rounded 
          onClick={resetErrorBoundary}
          className="w-full shadow-lg shadow-primary/20 hover:scale-[1.02] active:scale-[0.98] transition-all bg-primary text-white"
          leftIcon={<RefreshCw size={18} />}
        >
          Thử lại ngay
        </Button>
        
        <div className="flex gap-3">
            <Button 
                variant="outline" 
                size="md" 
                rounded
                onClick={() => window.history.back()}
                className="flex-1 bg-white/50 hover:bg-white border-slate-200 text-slate-600"
                leftIcon={<ChevronLeft size={16} />}
            >
                Quay lại
            </Button>
            <Button 
                variant="outline" 
                size="md" 
                rounded
                onClick={() => window.location.href = '/'}
                className="flex-1 bg-white/50 hover:bg-white border-slate-200 text-slate-600"
                leftIcon={<Home size={16} />}
            >
                Trang chủ
            </Button>
        </div>
      </div>

      {error.stack && (
        <div className="pt-4 w-full relative z-10">
            <details className="text-left group">
                <summary className="text-[10px] text-slate-400 cursor-pointer hover:text-slate-600 transition-colors uppercase tracking-widest font-bold flex items-center justify-center list-none">
                    <span className="group-open:rotate-180 transition-transform mr-1 inline-block">▼</span>
                    Chi tiết kỹ thuật
                </summary>
                <div className="mt-3 p-4 bg-slate-900/90 rounded-2xl text-[10px] text-slate-300 overflow-x-auto max-h-40 scrollbar-thin scrollbar-thumb-white/10 backdrop-blur-md border border-white/5">
                    <pre className="font-mono">{error.stack}</pre>
                </div>
            </details>
        </div>
      )}
    </motion.div>
  );
};
