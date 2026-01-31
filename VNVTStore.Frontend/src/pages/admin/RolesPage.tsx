import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Shield, Edit, Trash2, Plus, Info, CheckCircle2, XCircle } from 'lucide-react';
import { Button, Badge, Modal, ConfirmDialog } from '@/components/ui';
import { roleService, CreateRoleRequest, UpdateRoleRequest } from '@/services/roleService';
import { useEntityManager, useRoles } from '@/hooks';
import type { Role } from '@/types';
import { DataTable, type DataTableColumn } from '@/components/common/DataTable';
import { AdminPageHeader } from '@/components/admin';
import { PaginationDefaults, SortDirection } from '@/constants';
import { RoleForm, RoleFormData } from './forms/RoleForm';

export const RolesPage = () => {
    const { t, i18n } = useTranslation();

    // State
    const [currentPage, setCurrentPage] = useState<number>(PaginationDefaults.PAGE_INDEX);
    const [pageSize, setPageSize] = useState<number>(PaginationDefaults.PAGE_SIZE);
    const [searchQuery, setSearchQuery] = useState<string>('');

    // Sorting
    const [sortField, setSortField] = useState<string>('name');
    const [sortDir, setSortDir] = useState<SortDirection>(SortDirection.ASC);

    // Data Fetching
    const {
        data: result,
        isLoading,
        isFetching,
        isError,
        error,
        refetch
    } = useRoles({
        pageIndex: currentPage,
        pageSize,
        search: searchQuery || undefined,
        sortBy: sortField,
        sortDesc: sortDir === SortDirection.DESC
    });
    
    const roles = result?.data?.items || [];
    const totalItems = result?.data?.totalItems || 0;
    const totalPages = Math.ceil(totalItems / pageSize) || 1;

    // Entity Manager (for CRUD state)
    const {
        isFormOpen,
        editingItem: editingRole,
        itemToDelete: roleToDelete,
        isDeleting,
        openCreate,
        openEdit,
        closeForm,
        confirmDelete,
        cancelDelete,
        createMutation,
        updateMutation,
        deleteMutation
    } = useEntityManager<Role, CreateRoleRequest, UpdateRoleRequest>({
        service: roleService,
        queryKey: ['roles']
    });

    // Handlers
    const handleSort = (field: string, dir: 'asc' | 'desc') => {
        setSortField(field);
        setSortDir(dir as SortDirection);
        setCurrentPage(PaginationDefaults.PAGE_INDEX);
    };

    const handleCreate = async (data: RoleFormData) => {
        try {
            await createMutation.mutateAsync({
                name: data.name,
                description: data.description,
                isActive: data.isActive,
                permissionCodes: data.permissionCodes
            });
        } catch (err) {
            // Error handled by hook
        }
    };

    const handleUpdate = async (data: RoleFormData) => {
        if (!editingRole) return;
        try {
            await updateMutation.mutateAsync({
                id: editingRole.code,
                data: {
                    name: data.name,
                    description: data.description,
                    isActive: data.isActive,
                    permissionCodes: data.permissionCodes
                }
            });
        } catch (err) {
            // Error handled by hook
        }
    };

    const handleDelete = async () => {
        if (roleToDelete) {
            deleteMutation.mutate(roleToDelete.code);
        }
    };

    // Column Definitions
    const columns: DataTableColumn<Role>[] = [
        {
            id: 'code',
            header: t('common.fields.code'),
            accessor: 'code',
            sortable: true
        },
        {
            id: 'name',
            header: t('common.fields.name'),
            accessor: (role) => (
                <div>
                    <p className="font-medium text-sm text-slate-700 dark:text-slate-200">{role.name}</p>
                    <p className="text-xs text-slate-500 line-clamp-1">{role.description}</p>
                </div>
            ),
            sortable: true
        },
        {
            id: 'permissions',
            header: t('rbac.permissions'),
            accessor: (role) => (
                <div className="flex flex-wrap gap-1 max-w-xs">
                    {role.permissions?.slice(0, 5).map(p => (
                        <Badge key={p.code} size="sm" variant="outline" color="primary">{p.name}</Badge>
                    ))}
                    {role.permissions?.length > 5 && (
                        <Badge size="sm" variant="outline">+{role.permissions.length - 5}</Badge>
                    )}
                </div>
            )
        },
        {
            id: 'status',
            header: t('common.fields.status'),
            accessor: (role) => (
                <Badge
                    color={role.isActive ? 'success' : 'error'}
                    size="sm"
                    variant="soft"
                >
                    {role.isActive ? t('common.status.active') : t('common.status.inactive')}
                </Badge>
            ),
            className: 'text-center',
            headerClassName: 'text-center'
        },
        {
            id: 'actions',
            header: '',
            accessor: (role) => (
                <div className="flex justify-end gap-2">
                    <Button
                        size="sm"
                        variant="ghost"
                        onClick={() => openEdit(role)}
                        title={t('common.actions.edit')}
                    >
                        <Edit size={16} />
                    </Button>
                    <Button
                        size="sm"
                        variant="ghost"
                        onClick={() => confirmDelete(role)}
                        disabled={role.code === 'ADMIN'} // Protected role
                        title={t('common.actions.delete')}
                        className="text-rose-500 hover:text-rose-600 hover:bg-rose-50"
                    >
                        <Trash2 size={16} />
                    </Button>
                </div>
            ),
            className: 'text-right'
        }
    ];

    const roleInitialData = useMemo(() => editingRole ? {
        name: editingRole.name,
        description: editingRole.description,
        isActive: editingRole.isActive,
        permissionCodes: editingRole.permissions?.map(p => p.code) || []
    } : undefined, [editingRole]);

    return (
        <div className="space-y-6">
            <AdminPageHeader
                title="rbac.roles.title"
                subtitle="rbac.roles.subtitle"
            />

            <DataTable
                columns={columns}
                data={roles}
                keyField="code"
                isLoading={isLoading}
                isFetching={isFetching}
                onAdd={() => openCreate()}
                onRefresh={() => refetch()}
                error={isError ? (error as Error) : null}

                // Sorting
                externalSortField={sortField}
                externalSortDir={sortDir}
                onExternalSort={handleSort}

                // Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                totalItems={totalItems}
                pageSize={pageSize}
                onPageChange={setCurrentPage}
                onPageSizeChange={(size) => {
                    setPageSize(size);
                    setCurrentPage(PaginationDefaults.PAGE_INDEX);
                }}

                // Search
                onSearch={setSearchQuery}
            />

            {/* Form Modal */}
            <Modal
                isOpen={isFormOpen}
                onClose={closeForm}
                title={editingRole ? t('rbac.roles.edit') : t('rbac.roles.create')}
                size="4xl"
            >
                <RoleForm
                    initialData={roleInitialData}
                    onSubmit={editingRole ? handleUpdate : handleCreate}
                    onCancel={closeForm}
                    isLoading={createMutation.isPending || updateMutation.isPending}
                    isEdit={!!editingRole}
                />
            </Modal>

            {/* Delete Confirmation */}
            <ConfirmDialog
                isOpen={!!roleToDelete}
                onClose={cancelDelete}
                onConfirm={handleDelete}
                title={t('common.actions.delete')}
                message={t('rbac.roles.deleteConfirm', { name: roleToDelete?.name })}
                confirmText={t('common.actions.delete')}
                variant="danger"
                isLoading={isDeleting}
            />
        </div>
    );
};

export default RolesPage;
