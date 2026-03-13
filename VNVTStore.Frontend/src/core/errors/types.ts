export interface ErrorFallbackProps<T = unknown> {
    error: Error;
    resetErrorBoundary: () => void;
    context?: T;
}

export interface ErrorBoundaryProps<T = unknown> {
    children: React.ReactNode;
    fallback: React.ComponentType<ErrorFallbackProps<T>>;
    onReset?: () => void;
    onError?: (error: Error, info: React.ErrorInfo) => void;
    context?: T; // Data to pass to fallback
}

export interface ErrorBoundaryState {
    hasError: boolean;
    error: Error | null;
}
