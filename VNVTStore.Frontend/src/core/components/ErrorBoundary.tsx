import { Component, ErrorInfo, ReactNode } from 'react';
import { Button } from '@/components/ui';
import { AlertTriangle, RefreshCcw } from 'lucide-react';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
  onReset?: () => void;
}

interface State {
  hasError: boolean;
  error?: Error | undefined; // Fix exactOptionalPropertyTypes
}

export class ErrorBoundary extends Component<Props, State> {
  public state: State = {
    hasError: false,
    error: undefined,
  };

  public static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Uncaught error:', error, errorInfo);
  }

  private handleReset = () => {
    this.setState({ hasError: false, error: undefined });
    this.props.onReset?.();
  };

  public render() {
    if (this.state.hasError) {
      return (
        this.props.fallback || (
          <div className="flex flex-col items-center justify-center min-h-[400px] p-6 border-2 border-dashed border-rose-200 rounded-xl bg-rose-50/50 dark:bg-rose-900/10 dark:border-rose-900/20">
            <div className="p-4 bg-white dark:bg-slate-800 rounded-full shadow-sm mb-4">
              <AlertTriangle className="w-8 h-8 text-rose-500" />
            </div>
            <h3 className="text-xl font-semibold text-slate-900 dark:text-slate-100 mb-2">
              Đã có lỗi xảy ra!
            </h3>
            <p className="text-slate-500 dark:text-slate-400 text-center max-w-md mb-6">
              {this.state.error?.message || 'Hệ thống gặp sự cố không mong muốn. Vui lòng thử lại.'}
            </p>
            <Button 
              variant="primary" 
              leftIcon={<RefreshCcw size={16} />}
              onClick={this.handleReset}
            >
              Thử lại
            </Button>
          </div>
        )
      );
    }

    return this.props.children;
  }
}
