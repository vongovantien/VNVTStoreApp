import React from 'react';
import { useAuthStore } from '@/store';

/**
 * Higher-Order Component to protect components based on user permissions.
 * @param WrappedComponent The component to wrap
 * @param permission The permission required to view the component
 * @param fallback Optional fallback UI (default: null)
 */
export const withPermission = <P extends object>(
  WrappedComponent: React.ComponentType<P>,
  permission: string,
  fallback: React.ReactNode = null
) => {
  // eslint-disable-next-line react/display-name
  return (props: P) => {
    const hasPermission = useAuthStore((state) => state.hasPermission(permission));

    if (!hasPermission) {
      return <>{fallback}</>;
    }

    return <WrappedComponent {...props} />;
  };
};
