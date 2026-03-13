import React, { Component, ErrorInfo, ReactNode } from 'react';

interface ErrorBoundaryProps<T = unknown> {
  children: ReactNode;
  fallback: React.ComponentType<{ error: Error; resetErrorBoundary: () => void; context?: T }>;
  onReset?: () => void;
  onError?: (error: Error, info: ErrorInfo) => void;
  context?: T; // Generic context data passed to fallback
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary<T = unknown> extends Component<ErrorBoundaryProps<T>, State> {
  constructor(props: ErrorBoundaryProps<T>) {
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
      const FallbackComponent = this.props.fallback;
      return (
        <FallbackComponent
          error={this.state.error}
          resetErrorBoundary={this.resetErrorBoundary}
          context={this.props.context}
        />
      );
    }

    return this.props.children;
  }
}
