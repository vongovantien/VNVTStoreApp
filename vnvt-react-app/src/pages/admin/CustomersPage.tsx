import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Mail, Phone, ShoppingBag, Eye } from 'lucide-react';
import { Button, Modal } from '@/components/ui';
import { formatCurrency, formatDate } from '@/utils/format';
import { DataTable, type DataTableColumn } from '@/components/common/DataTable';

interface Customer {
  id: string;
  email: string;
  name: string;
  phone: string;
  role: 'customer';
  createdAt: string;
  orders: number;
  totalSpent: number;
}

export const CustomersPage = () => {
  const { t } = useTranslation();
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);

  // Mock Data (Assuming hook or API call would be here)
  const customersData: Customer[] = [
    { id: 'user-2', email: 'customer@email.com', name: 'Khách Hàng Demo', phone: '0909123456', role: 'customer' as const, createdAt: '2024-01-10', orders: 5, totalSpent: 25000000 },
    { id: 'user-3', email: 'nguyenvana@email.com', name: 'Nguyễn Văn A', phone: '0901234567', role: 'customer' as const, createdAt: '2024-01-15', orders: 12, totalSpent: 85000000 },
    { id: 'user-4', email: 'tranthib@email.com', name: 'Trần Thị B', phone: '0912345678', role: 'customer' as const, createdAt: '2024-01-20', orders: 3, totalSpent: 15000000 },
  ];

  const columns: DataTableColumn<Customer>[] = [
    {
      id: 'name',
      header: t('admin.columns.customer'),
      accessor: (customer) => (
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-gradient-to-r from-blue-500 to-purple-500 flex items-center justify-center text-white font-bold shadow-sm">
            {customer.name.charAt(0)}
          </div>
          <p className="font-medium text-slate-800 dark:text-slate-100">{customer.name}</p>
        </div>
      ),
      sortable: true
    },
    {
      id: 'contact',
      header: t('admin.columns.contact'),
      accessor: (customer) => (
        <div className="space-y-1">
          <p className="text-sm flex items-center gap-2 text-slate-600 dark:text-slate-400">
            <Mail size={14} className="text-blue-400" />
            {customer.email}
          </p>
          {customer.phone && (
            <p className="text-sm flex items-center gap-2 text-slate-500 dark:text-slate-500">
              <Phone size={14} className="text-slate-400" />
              {customer.phone}
            </p>
          )}
        </div>
      )
    },
    {
      id: 'orders',
      header: t('admin.orders'),
      accessor: (customer) => (
        <div className="flex items-center justify-center gap-1.5">
          <ShoppingBag size={16} className="text-emerald-500" />
          <span className="font-medium">{customer.orders}</span>
        </div>
      ),
      className: 'text-center',
      headerClassName: 'text-center',
      sortable: true
    },
    {
      id: 'totalSpent',
      header: t('admin.columns.totalSpent'),
      accessor: (customer) => (
        <span className="font-semibold text-emerald-600">{formatCurrency(customer.totalSpent)}</span>
      ),
      className: 'text-right',
      headerClassName: 'text-right',
      sortable: true
    },
    {
      id: 'joinDate',
      header: t('admin.columns.joinDate'),
      accessor: (customer) => <span className="text-slate-500">{formatDate(customer.createdAt)}</span>,
      sortable: true
    },
    {
      id: 'action',
      header: t('admin.columns.action'),
      accessor: (customer) => (
        <div className="flex items-center justify-center">
          <button
            className="p-2 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-lg transition-colors text-slate-500"
            onClick={() => setSelectedCustomer(customer)}
            title="Xem chi tiết"
          >
            <Eye size={16} />
          </button>
        </div>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    }
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-800 dark:text-slate-100">{t('admin.customers')}</h1>

      <DataTable
        columns={columns}
        data={customersData}
        keyField="id"
        // Pass fake loading false
        isLoading={false}

        // Search & Filter
        onAdvancedSearch={() => { }} // Client side filtering not strictly implemented for mock, but prop enables panel
        advancedFilterDefs={[
          {
            id: 'name',
            label: t('admin.columns.customer'),
            type: 'text',
            placeholder: 'Tên khách hàng...'
          },
          {
            id: 'email',
            label: 'Email',
            type: 'text',
          },
          {
            id: 'phone',
            label: t('admin.columns.phone') || 'Số điện thoại',
            type: 'text',
          },
          {
            id: 'orders',
            label: t('admin.orders'),
            type: 'number',
            placeholder: 'Số đơn hàng từ...'
          },
          {
            id: 'totalSpent',
            label: t('admin.columns.totalSpent'),
            type: 'number',
            placeholder: 'Chi tiêu từ...'
          },
          {
            id: 'joinDate',
            label: t('admin.columns.joinDate'),
            type: 'date'
          }
        ]}

        // Visibility
        enableColumnVisibility={true}
        exportFilename="customers_export"
        emptyMessage={t('common.noResults')}
      />

      {/* Customer Detail Modal */}
      <Modal
        isOpen={!!selectedCustomer}
        onClose={() => setSelectedCustomer(null)}
        title={t('admin.actions.view') + ' ' + t('admin.columns.customer')}
        size="md"
      >
        {selectedCustomer && (
          <div className="space-y-6">
            <div className="flex items-center gap-4">
              <div className="w-16 h-16 rounded-full bg-gradient-to-r from-blue-500 to-purple-500 flex items-center justify-center text-white font-bold text-2xl shadow-md">
                {selectedCustomer.name.charAt(0)}
              </div>
              <div>
                <h2 className="text-xl font-bold text-slate-800 dark:text-white">{selectedCustomer.name}</h2>
                <p className="text-slate-500">{selectedCustomer.email}</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="bg-slate-50 dark:bg-slate-800 rounded-lg p-4 text-center border border-slate-100 dark:border-slate-700">
                <p className="text-2xl font-bold text-slate-800 dark:text-white">{selectedCustomer.orders}</p>
                <p className="text-sm text-slate-500">{t('admin.orders')}</p>
              </div>
              <div className="bg-slate-50 dark:bg-slate-800 rounded-lg p-4 text-center border border-slate-100 dark:border-slate-700">
                <p className="text-xl font-bold text-emerald-600">{formatCurrency(selectedCustomer.totalSpent)}</p>
                <p className="text-sm text-slate-500">{t('admin.columns.totalSpent')}</p>
              </div>
            </div>

            <div className="space-y-3">
              <h3 className="font-semibold text-slate-800 dark:text-white">{t('admin.columns.contact')}</h3>
              <div className="space-y-2 text-slate-600 dark:text-slate-400">
                <p className="flex items-center gap-2">
                  <Mail size={16} className="text-blue-500" />
                  {selectedCustomer.email}
                </p>
                {selectedCustomer.phone && (
                  <p className="flex items-center gap-2">
                    <Phone size={16} className="text-slate-400" />
                    {selectedCustomer.phone}
                  </p>
                )}
              </div>
            </div>

            <div className="flex gap-3">
              <Button fullWidth variant="outline">
                {t('admin.actions.view') + ' ' + t('admin.orders')}
              </Button>
              <Button fullWidth>
                Email
              </Button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
};

export default CustomersPage;
