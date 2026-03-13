import React from 'react';
import { User } from '../types/User';
import { DataTable, ColumnDef } from '@/components/shared/DataTable';
import { Button, Badge } from '@/components/ui';
import { Edit2, Trash2 } from 'lucide-react';
import { format } from 'date-fns';

 interface UserListProps {
  users: User[];
  isLoading: boolean;
  onEdit: (user: User) => void;
  onDelete: (id: string) => void;
  page?: number;
  totalPages?: number;
  onPageChange?: (page: number) => void;
}

export const UserList: React.FC<UserListProps> = ({ users, isLoading, onEdit, onDelete, page, totalPages, onPageChange }) => {
  const columns: ColumnDef<User>[] = [
    {
      header: 'User',
      className: 'w-[250px]',
      cell: (user) => (
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 rounded-full bg-slate-200 dark:bg-slate-700 flex items-center justify-center text-xs font-bold text-slate-600 dark:text-slate-300">
            {user.avatar ? <img src={user.avatar} className="w-full h-full rounded-full" /> : user.fullName.charAt(0)}
          </div>
          <div>
            <div className="font-medium text-slate-900 dark:text-slate-100">{user.fullName}</div>
            <div className="text-xs text-slate-500">{user.email}</div>
          </div>
        </div>
      ),
    },
    {
      header: 'Role',
      accessorKey: 'role',
      cell: (user) => (
        <Badge 
          variant="outline" 
          className={
            user.role === 'ADMIN' ? 'bg-purple-50 text-purple-700 border-purple-200' :
            user.role === 'MANAGER' ? 'bg-blue-50 text-blue-700 border-blue-200' :
            'bg-slate-50 text-slate-700 border-slate-200'
          }
        >
          {user.role}
        </Badge>
      ),
    },
    {
      header: 'Status',
      accessorKey: 'status',
      cell: (user) => (
        <Badge 
            variant="outline"
            className={
                user.status === 'ACTIVE' ? 'bg-green-50 text-green-700 border-green-200' :
                user.status === 'INACTIVE' ? 'bg-yellow-50 text-yellow-700 border-yellow-200' :
                'bg-red-50 text-red-700 border-red-200'
            }
        >
            {user.status}
        </Badge>
      )
    },
    {
      header: 'Created At',
      accessorKey: 'createdAt',
      cell: (user) => <span className="text-slate-500 text-sm">{format(new Date(user.createdAt), 'dd/MM/yyyy')}</span>
    },
    {
        header: 'Actions',
        className: 'text-right',
        cell: (user) => (
            <div className="flex justify-end gap-2">
                <Button size="xs" variant="ghost" onClick={(e) => { e.stopPropagation(); onEdit(user); }}>
                    <Edit2 size={14} className="text-slate-500" />
                </Button>
                <Button size="xs" variant="ghost" onClick={(e) => { e.stopPropagation(); onDelete(user.id); }}>
                    <Trash2 size={14} className="text-rose-500" />
                </Button>
            </div>
        )
    }
  ];

  return (
    <DataTable 
      data={users} 
      columns={columns} 
      isLoading={isLoading} 
      page={page || 1}
      totalPages={totalPages || 1}
      onPageChange={onPageChange || (() => {})}
    />
  );
};
