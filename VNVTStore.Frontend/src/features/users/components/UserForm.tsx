import React from 'react';
import { z } from 'zod';
import { User } from '../types/User';
import { DynamicForm } from '@/components/shared/DynamicForm/DynamicForm'; // Importing from shared
import { FieldConfig, FieldType } from '@/shared/constants/form-config';

const userSchema = z.object({
  fullName: z.string().min(2, 'Name must be at least 2 characters'),
  email: z.string().email('Invalid email address'),
  role: z.enum(['ADMIN', 'MANAGER', 'USER']),
  status: z.enum(['ACTIVE', 'INACTIVE', 'BANNED']),
});

type UserFormData = z.infer<typeof userSchema>;

interface UserFormProps {
  initialData?: User | undefined; // Fix exactOptionalPropertyTypes compatibility
  onSubmit: (data: UserFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
}

const userFields: FieldConfig<UserFormData>[] = [
  {
    name: 'fullName',
    type: FieldType.TEXT,
    label: 'Full Name',
    placeholder: 'Ex: John Doe',
    className: 'col-span-2',
  },
  {
    name: 'email',
    type: FieldType.TEXT, // Should add email type support later
    label: 'Email Address',
    placeholder: 'john@example.com',
    className: 'col-span-2',
  },
  {
    name: 'role',
    type: FieldType.TEXT, // TODO: Add SELECT support to DynamicForm
    label: 'Role (ADMIN, MANAGER, USER)',
    placeholder: 'USER',
  },
  {
    name: 'status',
    type: FieldType.TEXT, // TODO: Add SELECT support
    label: 'Status (ACTIVE, INACTIVE)',
    placeholder: 'ACTIVE',
  },
];

export const UserForm: React.FC<UserFormProps> = ({ initialData, onSubmit, onCancel, isLoading }) => {
  return (
    <div className="p-6 bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-800 shadow-sm">
      <h3 className="text-lg font-semibold mb-6">
        {initialData ? 'Edit User' : 'Create New User'}
      </h3>
      <DynamicForm<UserFormData>
        schema={userSchema}
        defaultValues={initialData ? {
            fullName: initialData.fullName,
            email: initialData.email,
            role: initialData.role,
            status: initialData.status
        } : {
            role: 'USER',
            status: 'ACTIVE'
        }}
        fields={userFields}
        onSubmit={onSubmit}
        onCancel={onCancel}
        isLoading={isLoading ?? false}
        submitLabel={initialData ? 'Update User' : 'Create User'}
      />
    </div>
  );
};
