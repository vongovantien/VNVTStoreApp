import React from 'react';
import { useAuthStore } from '@/store';

interface PermissionGateProps {
    permission: string;
    children: React.ReactNode;
    fallback?: React.ReactNode;
}

/**
 * Component to conditionally render children based on user permissions.
 */
export const PermissionGate: React.FC<PermissionGateProps> = ({ 
    permission, 
    children, 
    fallback = null 
}) => {
    const hasPermission = useAuthStore((state) => state.hasPermission(permission));

    if (!hasPermission) {
        return <>{fallback}</>;
    }

    return <>{children}</>;
};

export default PermissionGate;
