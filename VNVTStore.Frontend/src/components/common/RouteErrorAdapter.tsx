import React from 'react';
import { useRouteError } from 'react-router-dom';
import { ErrorFallback } from './ErrorFallback';

export const RouteErrorAdapter: React.FC = () => {
  const error = useRouteError();
  
  const resetErrorBoundary = () => {
    window.location.href = '/';
  };

  return (
    <div className="min-h-[400px] flex items-center justify-center p-6">
      <ErrorFallback 
        error={error instanceof Error ? error : new Error('An unexpected routing error occurred')} 
        resetErrorBoundary={resetErrorBoundary} 
      />
    </div>
  );
};

export default RouteErrorAdapter;
