import { Component, ErrorInfo, ReactNode } from 'react';
import { ErrorFallback } from './ErrorFallback';

interface Props {
  children: ReactNode;
  fallback?: ReactNode | ((props: { error: Error; resetErrorBoundary: () => void }) => ReactNode);
  onReset?: () => void;
  onError?: (error: Error, info: ErrorInfo) => void;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      hasError: false,
      error: null,
    };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    if (this.props.onError) {
      this.props.onError(error, info);
    }
    // Optimization: Log to error reporting service here
    console.error('ErrorBoundary caught an error:', error, info);
  }

  resetErrorBoundary = () => {
    if (this.props.onReset) {
      this.props.onReset();
    }
    this.setState({
      hasError: false,
      error: null,
    });
  };

  render() {
    if (this.state.hasError && this.state.error) {
      if (this.props.fallback) {
        if (typeof this.props.fallback === 'function') {
           return (this.props.fallback as (props: { error: Error; resetErrorBoundary: () => void }) => ReactNode)({
             error: this.state.error,
             resetErrorBoundary: this.resetErrorBoundary,
           });
        }
        return this.props.fallback;
      }
      return <ErrorFallback error={this.state.error} resetErrorBoundary={this.resetErrorBoundary} />;
    }

    return this.props.children;
  }
}
