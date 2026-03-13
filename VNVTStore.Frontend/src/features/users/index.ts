// Export only what is needed externally (Pages, Public Hooks)
export { default as UsersPage } from './pages/UsersPage';

// If other features need to fetch users (e.g., in a dropdown), we export the hook
export { useUsers } from './hooks/useUsers';

// Types might be needed for shared components
export type { User, UserRole, UserStatus } from './types/User';
