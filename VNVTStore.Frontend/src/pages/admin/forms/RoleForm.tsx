import React, { useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { Shield, Info, Menu as MenuIcon } from 'lucide-react';
import { Button, Input, Textarea, Badge, Loading } from '@/components/ui';
import { usePermissions, useMenus } from '@/hooks';
import { Menu } from '@/types';

export interface RoleFormData {
    name: string;
    description?: string;
    isActive: boolean;
    permissionCodes: string[];
    menuCodes: string[];
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
            permissionCodes: [],
            menuCodes: []
        }
    });

    const selectedPermissionCodes = watch('permissionCodes');
    const selectedMenuCodes = watch('menuCodes');
    
    // Fetch all permissions
    const { data: permissionsResult, isLoading: isPermissionsLoading } = usePermissions();
    const permissions = permissionsResult?.data || [];

    // Fetch all menus
    const { data: menusResult, isLoading: isMenusLoading } = useMenus();
    const menus = menusResult?.data || [];

    // Group permissions by module
    const groupedPermissions = permissions.reduce((acc, curr) => {
        if (!acc[curr.module]) acc[curr.module] = [];
        acc[curr.module].push(curr);
        return acc;
    }, {} as Record<string, Permission[]>);

    // Group menus by groupName
    const groupedMenus = menus.reduce((acc, curr) => {
        if (!acc[curr.groupName]) acc[curr.groupName] = [];
        acc[curr.groupName].push(curr);
        return acc;
    }, {} as Record<string, Menu[]>);

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

    const toggleMenu = useCallback((code: string) => {
        const current = [...selectedMenuCodes];
        const index = current.indexOf(code);
        if (index > -1) {
            current.splice(index, 1);
        } else {
            current.push(code);
        }
        setValue('menuCodes', current);
    }, [selectedMenuCodes, setValue]);

    const togglePermissionModule = useCallback((module: string) => {
        const modulePerms = groupedPermissions[module].map(p => p.code);
        const allSelected = modulePerms.every(code => selectedPermissionCodes.includes(code));
        
        let newSelection = [...selectedPermissionCodes];
        if (allSelected) {
            newSelection = newSelection.filter(code => !modulePerms.includes(code));
        } else {
            modulePerms.forEach(code => {
                if (!newSelection.includes(code)) newSelection.push(code);
            });
        }
        setValue('permissionCodes', newSelection);
    }, [groupedPermissions, selectedPermissionCodes, setValue]);

    const toggleMenuGroup = useCallback((groupName: string) => {
        const groupMenus = groupedMenus[groupName].map(m => m.code);
        const allSelected = groupMenus.every(code => selectedMenuCodes.includes(code));
        
        let newSelection = [...selectedMenuCodes];
        if (allSelected) {
            newSelection = newSelection.filter(code => !groupMenus.includes(code));
        } else {
            groupMenus.forEach(code => {
                if (!newSelection.includes(code)) newSelection.push(code);
            });
        }
        setValue('menuCodes', newSelection);
    }, [groupedMenus, selectedMenuCodes, setValue]);

    const isModuleAllSelected = useCallback((module: string) => {
        return groupedPermissions[module]?.every(p => selectedPermissionCodes.includes(p.code)) ?? false;
    }, [groupedPermissions, selectedPermissionCodes]);

    const isModuleAnySelected = useCallback((module: string) => {
        return groupedPermissions[module]?.some(p => selectedPermissionCodes.includes(p.code)) ?? false;
    }, [groupedPermissions, selectedPermissionCodes]);

    const isMenuGroupAllSelected = useCallback((groupName: string) => {
        return groupedMenus[groupName]?.every(m => selectedMenuCodes.includes(m.code)) ?? false;
    }, [groupedMenus, selectedMenuCodes]);

    const isMenuGroupAnySelected = useCallback((groupName: string) => {
        return groupedMenus[groupName]?.some(m => selectedMenuCodes.includes(m.code)) ?? false;
    }, [groupedMenus, selectedMenuCodes]);

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            {/* General Info */}
            <div className="space-y-4">
                <h3 className="text-lg font-semibold flex items-center gap-2 border-b pb-2">
                    <Info size={18} />
                    {t('common.fields.info')}
                </h3>
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <Input
                        label={t('common.fields.name')}
                        {...register('name', { required: t('validation.required') })}
                        error={errors.name?.message}
                        placeholder={t('rbac.roles.namePlaceholder')}
                    />

                    <div className="flex items-center gap-2 pt-6">
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

                <Textarea
                    label={t('common.fields.description')}
                    {...register('description')}
                    placeholder={t('rbac.roles.descriptionPlaceholder')}
                    rows={2}
                />
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Menu Access */}
                <div className="space-y-4">
                    <h3 className="text-lg font-semibold flex items-center gap-2 border-b pb-2">
                        <MenuIcon size={18} />
                        {t('rbac.menuAccess')}
                        <Badge size="sm" color="primary" className="ml-auto">
                            {selectedMenuCodes.length} {t('common.selected')}
                        </Badge>
                    </h3>

                    {isMenusLoading ? (
                        <div className="flex justify-center py-8">
                            <Loading />
                        </div>
                    ) : (
                        <div className="max-h-[300px] overflow-y-auto pr-2 space-y-3">
                            {Object.entries(groupedMenus).map(([groupName, menuItems]) => (
                                <div key={groupName} className="border rounded-lg p-3 bg-slate-50 dark:bg-slate-800/50">
                                    <div className="flex items-center justify-between mb-2">
                                        <div className="flex items-center gap-2">
                                            <input
                                                type="checkbox"
                                                checked={isMenuGroupAllSelected(groupName)}
                                                onChange={() => toggleMenuGroup(groupName)}
                                                className="w-4 h-4 text-indigo-600 border-gray-300 rounded focus:ring-indigo-500"
                                            />
                                            <span className="font-bold text-slate-800 dark:text-slate-200">{groupName}</span>
                                        </div>
                                        <Badge size="sm" variant="soft" color={isMenuGroupAllSelected(groupName) ? 'success' : isMenuGroupAnySelected(groupName) ? 'info' : 'secondary'}>
                                            {menuItems.filter(m => selectedMenuCodes.includes(m.code)).length} / {menuItems.length}
                                        </Badge>
                                    </div>
                                    <div className="grid grid-cols-1 gap-1 pl-6">
                                        {menuItems.map(menu => (
                                            <label key={menu.code} className="flex items-center gap-2 py-1 cursor-pointer hover:bg-slate-100 dark:hover:bg-slate-700/50 rounded px-1 group">
                                                <input
                                                    type="checkbox"
                                                    checked={selectedMenuCodes.includes(menu.code)}
                                                    onChange={() => toggleMenu(menu.code)}
                                                    className="w-3.5 h-3.5 text-indigo-600 border-gray-300 rounded focus:ring-indigo-500"
                                                />
                                                <span className="text-sm text-slate-600 dark:text-slate-400 group-hover:text-slate-900 dark:group-hover:text-slate-200">
                                                    {menu.name}
                                                </span>
                                            </label>
                                        ))}
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                {/* Permissions */}
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
                            <Loading />
                        </div>
                    ) : (
                        <div className="max-h-[300px] overflow-y-auto pr-2 space-y-3">
                            {Object.entries(groupedPermissions).map(([module, perms]) => (
                                <div key={module} className="border rounded-lg p-3 bg-slate-50 dark:bg-slate-800/50">
                                    <div className="flex items-center justify-between mb-2">
                                        <div className="flex items-center gap-2">
                                            <input
                                                type="checkbox"
                                                checked={isModuleAllSelected(module)}
                                                onChange={() => togglePermissionModule(module)}
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
