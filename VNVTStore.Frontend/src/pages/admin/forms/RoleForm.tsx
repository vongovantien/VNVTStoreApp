import React, { useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { Shield, Info } from 'lucide-react';
import { Button, Input, Textarea, Badge } from '@/components/ui';
import { usePermissions } from '@/hooks';
import { Permission } from '@/types';

export interface RoleFormData {
    name: string;
    description?: string;
    isActive: boolean;
    permissionCodes: string[];
}

interface RoleFormProps {
    initialData?: RoleFormData;
    onSubmit: (data: RoleFormData) => Promise<void>;
    onCancel: () => void;
    isLoading?: boolean;
    isEdit?: boolean;
}

export const RoleForm: React.FC<RoleFormProps> = ({
    initialData,
    onSubmit,
    onCancel,
    isLoading,
    isEdit
}) => {
    const { t } = useTranslation();
    const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm<RoleFormData>({
        defaultValues: initialData || {
            name: '',
            description: '',
            isActive: true,
            permissionCodes: []
        }
    });

    const selectedPermissionCodes = watch('permissionCodes');
    
    // Fetch all permissions
    const { data: permissionsResult, isLoading: isPermissionsLoading } = usePermissions();

    const permissions = permissionsResult?.data || [];

    // Group permissions by module
    const groupedPermissions = permissions.reduce((acc, curr) => {
        if (!acc[curr.module]) acc[curr.module] = [];
        acc[curr.module].push(curr);
        return acc;
    }, {} as Record<string, Permission[]>);

    const togglePermission = useCallback((code: string) => {
        const current = [...selectedPermissionCodes];
        const index = current.indexOf(code);
        if (index > -1) {
            current.splice(index, 1);
        } else {
            current.push(code);
        }
        setValue('permissionCodes', current);
    }, [selectedPermissionCodes, setValue]);

    const toggleModule = useCallback((module: string) => {
        const modulePerms = groupedPermissions[module].map(p => p.code);
        const allSelected = modulePerms.every(code => selectedPermissionCodes.includes(code));
        
        let newSelection = [...selectedPermissionCodes];
        if (allSelected) {
            // Unselect all in module
            newSelection = newSelection.filter(code => !modulePerms.includes(code));
        } else {
            // Select all in module
            modulePerms.forEach(code => {
                if (!newSelection.includes(code)) newSelection.push(code);
            });
        }
        setValue('permissionCodes', newSelection);
    }, [groupedPermissions, selectedPermissionCodes, setValue]);

    const isModuleAllSelected = useCallback((module: string) => {
        return groupedPermissions[module].every(p => selectedPermissionCodes.includes(p.code));
    }, [groupedPermissions, selectedPermissionCodes]);

    const isModuleAnySelected = useCallback((module: string) => {
        return groupedPermissions[module].some(p => selectedPermissionCodes.includes(p.code));
    }, [groupedPermissions, selectedPermissionCodes]);

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Left Side: General Info */}
                <div className="space-y-4">
                    <h3 className="text-lg font-semibold flex items-center gap-2 border-b pb-2">
                        <Info size={18} />
                        {t('common.fields.info')}
                    </h3>
                    
                    <Input
                        label={t('common.fields.name')}
                        {...register('name', { required: t('validation.required') })}
                        error={errors.name?.message}
                        placeholder={t('rbac.roles.namePlaceholder')}
                    />

                    <Textarea
                        label={t('common.fields.description')}
                        {...register('description')}
                        placeholder={t('rbac.roles.descriptionPlaceholder')}
                        rows={4}
                    />

                    <div className="flex items-center gap-2">
                        <input
                            type="checkbox"
                            id="isActive"
                            {...register('isActive')}
                            className="w-4 h-4 text-indigo-600 border-gray-300 rounded focus:ring-indigo-500"
                        />
                        <label htmlFor="isActive" className="text-sm font-medium text-gray-700 dark:text-gray-300">
                            {t('common.fields.active')}
                        </label>
                    </div>
                </div>

                {/* Right Side: Permissions Selection */}
                <div className="space-y-4">
                    <h3 className="text-lg font-semibold flex items-center gap-2 border-b pb-2">
                        <Shield size={18} />
                        {t('rbac.permissions')}
                        <Badge size="sm" color="primary" className="ml-auto">
                            {selectedPermissionCodes.length} {t('common.selected')}
                        </Badge>
                    </h3>

                    {isPermissionsLoading ? (
                        <div className="flex justify-center py-8">
                            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
                        </div>
                    ) : (
                        <div className="max-h-[400px] overflow-y-auto pr-2 space-y-4">
                            {Object.entries(groupedPermissions).map(([module, perms]) => (
                                <div key={module} className="border rounded-lg p-3 bg-slate-50 dark:bg-slate-800/50">
                                    <div className="flex items-center justify-between mb-2">
                                        <div className="flex items-center gap-2">
                                            <input
                                                type="checkbox"
                                                checked={isModuleAllSelected(module)}
                                                onChange={() => toggleModule(module)}
                                                className="w-4 h-4 text-indigo-600 border-gray-300 rounded focus:ring-indigo-500"
                                            />
                                            <span className="font-bold text-slate-800 dark:text-slate-200">{module}</span>
                                        </div>
                                        <Badge size="sm" variant="soft" color={isModuleAllSelected(module) ? 'success' : isModuleAnySelected(module) ? 'info' : 'secondary'}>
                                            {perms.filter(p => selectedPermissionCodes.includes(p.code)).length} / {perms.length}
                                        </Badge>
                                    </div>
                                    <div className="grid grid-cols-1 gap-1 pl-6">
                                        {perms.map(p => (
                                            <label key={p.code} className="flex items-center gap-2 py-1 cursor-pointer hover:bg-slate-100 dark:hover:bg-slate-700/50 rounded px-1 group">
                                                <input
                                                    type="checkbox"
                                                    checked={selectedPermissionCodes.includes(p.code)}
                                                    onChange={() => togglePermission(p.code)}
                                                    className="w-3.5 h-3.5 text-indigo-600 border-gray-300 rounded focus:ring-indigo-500"
                                                />
                                                <span className="text-sm text-slate-600 dark:text-slate-400 group-hover:text-slate-900 dark:group-hover:text-slate-200">
                                                    {p.name}
                                                </span>
                                            </label>
                                        ))}
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>

            <div className="flex justify-end gap-3 pt-6 border-t">
                <Button type="button" variant="ghost" onClick={onCancel} disabled={isLoading}>
                    {t('common.actions.cancel')}
                </Button>
                <Button type="submit" isLoading={isLoading}>
                    {isEdit ? t('common.actions.save') : t('common.actions.create')}
                </Button>
            </div>
        </form>
    );
};
