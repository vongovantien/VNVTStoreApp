import type { Meta, StoryObj } from '@storybook/react';
import { DataTable } from './DataTable';
import { Button } from '@/components/ui'; // Assuming default export or part of ui index
import { Edit, Trash, Eye } from 'lucide-react';

const meta = {
  title: 'Common/DataTable',
  component: DataTable,
  parameters: {
    layout: 'padded',
  },
  tags: ['autodocs'],
  argTypes: {
    isLoading: { control: 'boolean' },
    showToolbar: { control: 'boolean' },
    title: { control: 'text' },
  },
} satisfies Meta<typeof DataTable>;

export default meta;
type Story = StoryObj<typeof meta>;

interface User {
  id: string;
  name: string;
  email: string;
  role: string;
  status: string;
}

const data: User[] = [
  { id: '1', name: 'John Doe', email: 'john@example.com', role: 'Admin', status: 'Active' },
  { id: '2', name: 'Jane Smith', email: 'jane@example.com', role: 'User', status: 'Inactive' },
  { id: '3', name: 'Bob Johnson', email: 'bob@example.com', role: 'User', status: 'Active' },
  { id: '4', name: 'Alice Brown', email: 'alice@example.com', role: 'Manager', status: 'Active' },
  { id: '5', name: 'Charlie Wilson', email: 'charlie@example.com', role: 'User', status: 'Pending' },
];

const columns = [
  { id: 'name', header: 'Name', accessor: 'name' as const, sortable: true },
  { id: 'email', header: 'Email', accessor: 'email' as const, sortable: true },
  { id: 'role', header: 'Role', accessor: 'role' as const, sortable: true },
  {
    id: 'status',
    header: 'Status',
    accessor: (row: User) => (
      <span className={`px-2 py-1 rounded-full text-xs font-semibold ${
        row.status === 'Active' ? 'bg-green-100 text-green-800' :
        row.status === 'Inactive' ? 'bg-red-100 text-red-800' :
        'bg-yellow-100 text-yellow-800'
      }`}>
        {row.status}
      </span>
    ),
  },
  {
    id: 'actions',
    header: 'Actions',
    accessor: (row: User) => (
      <div className="flex gap-2">
        <Button size="sm" variant="ghost" onClick={() => alert(`View ${row.name}`)}><Eye size={16} /></Button>
        <Button size="sm" variant="ghost" onClick={() => alert(`Edit ${row.name}`)}><Edit size={16} /></Button>
        <Button size="sm" variant="ghost" className="text-red-500 hover:text-red-700" onClick={() => alert(`Delete ${row.name}`)}><Trash size={16} /></Button>
      </div>
    ),
  },
];

export const Default: Story = {
  args: {
    columns,
    data,
    keyField: 'id',
    title: 'Users List',
    showToolbar: true,
  },
};

export const Loading: Story = {
  args: {
    columns,
    data: [],
    keyField: 'id',
    title: 'Users List',
    isLoading: true,
  },
};

export const Empty: Story = {
  args: {
    columns,
    data: [],
    keyField: 'id',
    title: 'Users List',
    emptyMessage: 'No users found.',
  },
};
