import { useState } from 'react';
import { useUsers, useUserMutation } from '../hooks/useUsers';
import { UserList } from '../components/UserList';
import { UserForm } from '../components/UserForm';
import { User, UserPayload } from '../types/User';
import { Button } from '@/components/ui';
import { Plus } from 'lucide-react';
import { useToast } from '@/store';
import { ErrorBoundary } from '@/core/components/ErrorBoundary';

const UsersPageContent = () => {
    const [page, setPage] = useState(1);
    const pageSize = 10;
  
    // Use POST search pattern
    const { data, isLoading, isError } = useUsers({ 
        pageIndex: page, 
        pageSize,
        // searching: [] // Removed: baseService handles filters via SearchParams
    });
    
    // Map PagedResult response
    const users = data?.items || [];
    const totalPages = data?.totalPages || 1;

    const { create, update, delete: deleteUser } = useUserMutation();
    const { success, error: toastError } = useToast();
  
    const [isFormOpen, setIsFormOpen] = useState(false);
    const [editingUser, setEditingUser] = useState<User | undefined>(undefined);

    if (isError) {
        throw new Error('Failed to load users list. Please try again.');
    }

    const handleCreate = () => {
        setEditingUser(undefined);
        setIsFormOpen(true);
    };

    const handleEdit = (user: User) => {
        setEditingUser(user);
        setIsFormOpen(true);
    };

    const handleDelete = (id: string) => {
        if (confirm('Are you sure you want to delete this user?')) {
            deleteUser.mutate(id, {
                onSuccess: () => success('User deleted successfully'),
                onError: () => toastError('Failed to delete user')
            });
        }
    };

    const handleSubmit = (data: UserPayload) => {
        if (editingUser) {
            update.mutate({ id: editingUser.id, payload: data }, {
                onSuccess: () => {
                    success('User updated successfully');
                    setIsFormOpen(false);
                },
                onError: () => toastError('Failed to update user')
            });
        } else {
            create.mutate(data, {
                onSuccess: () => {
                    success('User created successfully');
                    setIsFormOpen(false);
                },
                onError: () => toastError('Failed to create user')
            });
        }
    };

    return (
        <div className="container mx-auto p-6 space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold text-slate-900 dark:text-white">User Management</h1>
                    <p className="text-slate-500 dark:text-slate-400">Manage system users and their roles.</p>
                </div>
                {!isFormOpen && (
                    <Button onClick={handleCreate} leftIcon={<Plus size={18} />}>
                        Add User
                    </Button>
                )}
            </div>

            {isFormOpen ? (
                <UserForm 
                    initialData={editingUser}
                    onSubmit={handleSubmit}
                    onCancel={() => setIsFormOpen(false)}
                    isLoading={create.isPending || update.isPending}
                />
            ) : (
                <UserList 
                    users={users} 
                    isLoading={isLoading} 
                    onEdit={handleEdit} 
                    onDelete={handleDelete}
                    page={page}
                    totalPages={totalPages}
                    onPageChange={setPage}
                />
            )}
        </div>
    );
};

export const UsersPage = () => {
    return (
        <ErrorBoundary>
            <UsersPageContent />
        </ErrorBoundary>
    );
};

export default UsersPage;
