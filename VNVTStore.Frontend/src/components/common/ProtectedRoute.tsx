
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuthStore } from '@/store';
import { UserRole } from '@/types';

interface ProtectedRouteProps {
  children?: React.ReactNode;
  allowedRoles?: UserRole[];
}

const ProtectedRoute = ({ children, allowedRoles }: ProtectedRouteProps) => {
  const { isAuthenticated, user } = useAuthStore();
  const location = useLocation();

  if (!isAuthenticated) {
    // Redirect to login if not authenticated
    return <Navigate to="/login" state={{ from: location.pathname }} replace />;
  }

  // Check for role-based access
  if (allowedRoles && user && !allowedRoles.includes(user.role)) {
    // Redirect to home (or 403 page) if user doesn't have permission
    return <Navigate to="/" replace />;
  }

  return children ? <>{children}</> : <Outlet />;
};

export default ProtectedRoute;
