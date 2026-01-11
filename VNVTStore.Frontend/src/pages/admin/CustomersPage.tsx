import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Mail, Phone, ShoppingBag, Eye, Edit, Trash2 } from 'lucide-react';
import { Button, Modal, Badge } from '@/components/ui';
import { formatCurrency, formatDate } from '@/utils/format';
import { DataTable, type DataTableColumn } from '@/components/common/DataTable';
import { useEntityManager } from '@/hooks';
import { customerService, type CustomerDto, type CreateCustomerRequest, type UpdateCustomerRequest } from '@/services';
import { useQuery } from '@tanstack/react-query';
import { PageSize, PaginationDefaults, SortDirection } from '@/constants';

export const CustomersPage = () => {
  const { t } = useTranslation();
  const [selectedCustomer, setSelectedCustomer] = useState<CustomerDto | null>(null);

  // State for Fetching
  const [pageIndex, setPageIndex] = useState(PaginationDefaults.PAGE_INDEX);
  const [pageSize, setPageSize] = useState(PageSize.DEFAULT);
  const [sortField, setSortField] = useState('createdAt');
  const [sortDir, setSortDir] = useState<SortDirection>(SortDirection.DESC);
  const [filters, setFilters] = useState<Record<string, string>>({});

  // Fetch Data
  const { data: customerResponse, isLoading, refetch } = useQuery({
    queryKey: ['customers', pageIndex, pageSize, sortField, sortDir, filters],
    queryFn: () => customerService.search({
      pageIndex,
      pageSize,
      sortBy: sortField,
      sortDesc: sortDir === SortDirection.DESC,
      filters: Object.entries(filters).map(([field, value]) => ({ field, value })),
      search: filters.search,
      searchField: filters.search ? 'fullName' : undefined // Basic search by name
    })
  });

  const items = customerResponse?.data?.items || [];
  const totalCount = customerResponse?.data?.totalItems || PaginationDefaults.TOTAL_ITEMS;
  const totalPages = Math.ceil(totalCount / pageSize) || PaginationDefaults.TOTAL_PAGES;

  // Entity Manager (Mutations only)
  const manager = useEntityManager<CustomerDto, CreateCustomerRequest, UpdateCustomerRequest>({
    service: customerService,
    queryKey: ['customers'],
  });

  const columns: DataTableColumn<CustomerDto>[] = [
    {
      id: 'fullName',
      header: t('admin.columns.customer'),
      accessor: (customer) => (
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-gradient-to-r from-blue-500 to-purple-500 flex items-center justify-center text-white font-bold shadow-sm">
            {customer.fullName?.charAt(0) || customer.username?.charAt(0)}
          </div>
          <div>
            <p className="font-medium text-slate-800 dark:text-slate-100">{customer.fullName || customer.username}</p>
            <p className="text-xs text-slate-500">{customer.username}</p>
          </div>
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
          {customer.phoneNumber && (
            <p className="text-sm flex items-center gap-2 text-slate-500 dark:text-slate-500">
              <Phone size={14} className="text-slate-400" />
              {customer.phoneNumber}
            </p>
          )}
        </div>
      )
    },
    {
      id: 'role',
      header: "Role", // ToDo: Localize
      accessor: (customer) => (
        <Badge color={customer.role === 'admin' ? 'error' : 'info'}>{customer.role}</Badge>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    },
    {
      id: 'isActive',
      header: t('admin.columns.status'),
      accessor: (customer) => (
        <Badge color={customer.isActive ? 'success' : 'secondary'}>
          {customer.isActive ? t('status.active') : t('status.inactive')}
        </Badge>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
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
        <div className="flex items-center justify-center gap-2">
          <button
            className="p-2 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-lg transition-colors text-slate-500"
            onClick={() => setSelectedCustomer(customer)}
            title={t('admin.actions.view')}
          >
            <Eye size={16} />
          </button>
          <button
            className="p-2 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded-lg transition-colors text-blue-600"
            onClick={() => manager.openEdit(customer)}
            title={t('admin.actions.edit')}
          >
            <Edit size={16} />
          </button>
          <button
            className="p-2 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors text-red-500"
            onClick={() => manager.confirmDelete(customer)}
            title={t('admin.actions.delete')}
          >
            <Trash2 size={16} />
          </button>
        </div>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    }
  ];

  const handleSort = (field: string, dir: 'asc' | 'desc') => {
    setSortField(field);
    setSortDir(dir as SortDirection);
  };

  const handleAdvancedSearch = (newFilters: Record<string, string>) => {
    setFilters(newFilters);
    setPageIndex(PaginationDefaults.PAGE_INDEX);
  };

  const handleReset = () => {
    setFilters({});
    setPageIndex(PaginationDefaults.PAGE_INDEX);
    setSortField('createdAt');
    setSortDir(SortDirection.DESC);
    refetch(); // Force API call
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-800 dark:text-slate-100">{t('admin.customers')}</h1>

      <DataTable
        columns={columns}
        data={items}
        keyField="code"
        isLoading={isLoading}

        // Sorting
        externalSortField={sortField}
        externalSortDir={sortDir}
        onExternalSort={handleSort}

        // Search & Filter
        onAdvancedSearch={handleAdvancedSearch}
        onReset={handleReset}
        advancedFilterDefs={[
          {
            id: 'fullName',
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
            id: 'phoneNumber',
            label: t('admin.columns.phone') || 'Số điện thoại',
            type: 'text',
          },
          {
            id: 'role',
            label: 'Role',
            type: 'select',
            options: [
              { value: 'customer', label: 'Customer' },
              { value: 'admin', label: 'Admin' }
            ]
          }
        ]}

        // Pagination
        currentPage={pageIndex}
        totalItems={totalCount}
        totalPages={totalPages}
        pageSize={pageSize}
        onPageChange={setPageIndex}
        onPageSizeChange={setPageSize}

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
                {selectedCustomer.fullName?.charAt(0) || selectedCustomer.username?.charAt(0)}
              </div>
              <div>
                <h2 className="text-xl font-bold text-slate-800 dark:text-white">{selectedCustomer.fullName}</h2>
                <p className="text-slate-500">{selectedCustomer.email}</p>
                <Badge color={selectedCustomer.isActive ? 'success' : 'secondary'} className="mt-1">
                  {selectedCustomer.isActive ? t('status.active') : t('status.inactive')}
                </Badge>
              </div>
            </div>

            <div className="space-y-3">
              <h3 className="font-semibold text-slate-800 dark:text-white">{t('admin.columns.contact')}</h3>
              <div className="space-y-2 text-slate-600 dark:text-slate-400">
                <p className="flex items-center gap-2">
                  <Mail size={16} className="text-blue-500" />
                  {selectedCustomer.email}
                </p>
                {selectedCustomer.phoneNumber && (
                  <p className="flex items-center gap-2">
                    <Phone size={16} className="text-slate-400" />
                    {selectedCustomer.phoneNumber}
                  </p>
                )}
                <p className="flex items-center gap-2">
                  <span className="font-semibold w-24">Username:</span>
                  {selectedCustomer.username}
                </p>
                <p className="flex items-center gap-2">
                  <span className="font-semibold w-24">Address:</span>
                  {selectedCustomer.address ? `${selectedCustomer.address}, ${selectedCustomer.ward}, ${selectedCustomer.district}, ${selectedCustomer.city}` : 'N/A'}
                </p>
              </div>
            </div>

            <div className="flex gap-3">
              <Button fullWidth>
                Email
              </Button>
            </div>
          </div>
        )}
      </Modal>


      {/* Customer Edit Modal */}
      <Modal
        isOpen={manager.isFormOpen}
        onClose={manager.closeForm}
        title={t('admin.actions.edit') + ' ' + t('admin.columns.customer')}
        size="md"
      >
        <form
          onSubmit={(e) => {
            e.preventDefault();
            const formData = new FormData(e.currentTarget);
            const updates: any = {
              isActive: formData.get('isActive') === 'true',
              // Add other fields if needed, e.g. role if backend supports it
            };
            // If backend supports role update via update endpoint
            // check CustomerDto structure.
            // For now just Status as it is common.
            if (manager.editingItem) {
              manager.updateMutation.mutate({
                id: manager.editingItem.code,
                data: updates
              });
            }
          }}
          className="space-y-4"
        >
          {manager.editingItem && (
            <>
              <div>
                <label className="block text-sm font-medium mb-1">{t('admin.columns.customer')}</label>
                <input className="w-full px-3 py-2 border rounded-md bg-slate-100 text-slate-500" disabled value={manager.editingItem.fullName || manager.editingItem.username} />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">Status</label>
                <select
                  name="isActive"
                  defaultValue={String(manager.editingItem.isActive)}
                  className="w-full px-3 py-2 border rounded-md dark:bg-slate-800 dark:border-slate-700"
                >
                  <option value="true">{t('status.active')}</option>
                  <option value="false">{t('status.inactive')}</option>
                </select>
              </div>

              <div className="flex justify-end gap-3 pt-4">
                <Button type="button" variant="outline" onClick={manager.closeForm}>
                  {t('common.cancel')}
                </Button>
                <Button type="submit" isLoading={manager.isLoading}>
                  {t('common.save')}
                </Button>
              </div>
            </>
          )}
        </form>
      </Modal>

      {/* Delete Confirmation */}
      <Modal
        isOpen={!!manager.itemToDelete}
        onClose={manager.cancelDelete}
        title={t('admin.actions.delete')}
        size="sm"
      >
        <div className="space-y-4">
          <p>{t('messages.confirmDelete')}</p>
          <div className="flex justify-end gap-3">
            <Button variant="outline" onClick={manager.cancelDelete}>
              {t('common.cancel')}
            </Button>
            <Button
              className="bg-red-600 hover:bg-red-700 text-white"
              onClick={() => manager.itemToDelete && manager.delete(manager.itemToDelete.code)}
            >
              {t('common.delete')}
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
};

export default CustomersPage;
