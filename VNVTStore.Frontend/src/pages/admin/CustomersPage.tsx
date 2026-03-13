import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Mail, Phone, Users, AlertTriangle, Key, LogIn } from 'lucide-react';
import { Button, Modal, Badge, ConfirmDialog, Input, Select, Switch } from '@/components/ui';
import { useToast, useAuthStore } from '@/store';
import { formatDate } from '@/utils/format';
import { DataTable, type DataTableColumn, CommonColumns } from '@/components/common/DataTable';
import { AdminPageHeader } from '@/components/admin';
import { useEntityManager, type EntityService } from '@/hooks';
import { customerService, type CustomerDto, type CreateCustomerRequest, type UpdateCustomerRequest } from '@/services';
import { useQuery, useMutation } from '@tanstack/react-query';
import { PaginationDefaults, SortDirection } from '@/constants';
import { StatsCards, StatItem } from '@/components/admin/StatsCards';
import { USER_LIST_FIELDS } from '@/constants/fieldConstants';
import { REGEX } from '@/constants/regex';

const isPasswordValid = (password: string) => !password || REGEX.PASSWORD.test(password);

const CustomersPage = () => {
  const { t } = useTranslation();
  const [selectedCustomer, setSelectedCustomer] = useState<CustomerDto | null>(null);
  const [resettingCustomer, setResettingCustomer] = useState<CustomerDto | null>(null);
  const [newPassword, setNewPassword] = useState('');

  // State for Fetching
  const [pageIndex, setPageIndex] = useState(PaginationDefaults.PAGE_INDEX);
  const [pageSize, setPageSize] = useState(PaginationDefaults.PAGE_SIZE);
  const [sortField, setSortField] = useState('createdAt');
  const [sortDir, setSortDir] = useState<SortDirection>(SortDirection.DESC);
  const [filters, setFilters] = useState<Record<string, string>>({ role: 'customer' });

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
      searchField: filters.search ? 'fullName' : '',
      fields: USER_LIST_FIELDS
    })
  });

  // Fetch Stats
  const { data: statsData, isLoading: isStatsLoading } = useQuery({
      queryKey: ['customer-stats'],
      queryFn: () => customerService.getStats(),
      staleTime: 60000, 
  });

  const stats: StatItem[] = [
      {
          label: t('admin.stats.totalCustomers'),
          value: statsData?.total || 0,
          icon: <Users size={24} />,
          color: 'blue',
          loading: isStatsLoading
      },
      {
          label: t('admin.stats.notActivated'),
          value: statsData?.unverified || 0,
          icon: <AlertTriangle size={24} />,
          color: 'rose',
          loading: isStatsLoading
      },
      {
          label: t('admin.stats.activeCustomers'),
          value: statsData?.active || 0,
          icon: <Users size={24} />,
          color: 'emerald',
          loading: isStatsLoading
      }
  ];

  const items = customerResponse?.data?.items || [];
  const totalCount = customerResponse?.data?.totalItems || PaginationDefaults.TOTAL_ITEMS;
  const totalPages = Math.ceil(totalCount / pageSize) || PaginationDefaults.TOTAL_PAGES;

  // Entity Manager
  const {
      isFormOpen,
      editingItem: editingCustomer,
      itemToDelete: customerToDelete,
      isLoading: isSubmitting,
      isDeleting,
      openCreate,
      openEdit,
      closeForm,
      confirmDelete,
      cancelDelete,
      create: createCustomer,
      update: updateCustomer,
      delete: deleteCustomer
  } = useEntityManager<CustomerDto, CreateCustomerRequest, UpdateCustomerRequest>({
    service: customerService as unknown as EntityService<CustomerDto, CreateCustomerRequest, UpdateCustomerRequest>,
    queryKey: ['customers', pageIndex, pageSize, sortField, sortDir, filters],
  });

  const toast = useToast();
  // Bulk Delete State
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [itemsToDelete, setItemsToDelete] = useState<CustomerDto[] | null>(null);

  const bulkDeleteMutation = useMutation({
    mutationFn: (codes: string[]) => customerService.deleteMultiple(codes),
    onSuccess: () => {
      toast.success(t('common.deleteSuccess'));
      setItemsToDelete(null);
      refetch();
    },
    onError: (err: unknown) => {
      toast.error((err as Error).message || t('common.deleteError'));
    }
  });

  const handleBulkDelete = (items: CustomerDto[]) => {
    setItemsToDelete(items);
  };

  const confirmBulkDelete = () => {
    if (itemsToDelete) {
      bulkDeleteMutation.mutate(itemsToDelete.map(i => i.code));
    }
  };

  // Form State
  const [formData, setFormData] = useState({
      username: '',
      email: '',
      password: '',
      fullName: '',
      phone: '',
      role: 'customer',
      isActive: true,
  });

  const resetForm = () => setFormData({
      username: '',
      email: '',
      password: '',
      fullName: '',
      phone: '',
      role: 'customer',
      isActive: true,
  });

  const handleOpenCreate = () => {
      resetForm();
      openCreate();
  };

  const handleOpenEdit = (customer: CustomerDto) => {
      setFormData({
          username: customer.username || '',
          email: customer.email,
          password: '',
          fullName: customer.fullName || '',
          phone: customer.phone || '',
          role: customer.role || 'customer',
          isActive: customer.isActive
      });
      openEdit(customer);
  };

  const handleSubmit = (e: React.FormEvent) => {
      e.preventDefault();
      
      if (editingCustomer) {
          updateCustomer(editingCustomer.code, {
              fullName: formData.fullName,
              email: formData.email,
              phone: formData.phone,
              role: formData.role,
              isActive: formData.isActive,
              password: formData.password || ''
          });
      } else {
          createCustomer({
              username: formData.username,
              email: formData.email,
              password: formData.password,
              fullName: formData.fullName,
              phone: formData.phone,
              role: formData.role,
              isActive: formData.isActive
          });
      }
  };

  const handleResetPasswordPrompt = (customer: CustomerDto) => {
      setResettingCustomer(customer);
      setNewPassword('');
  };

  const confirmResetPassword = async () => {
      if (!resettingCustomer || !newPassword) return;
      try {
          await customerService.update(resettingCustomer.code, { password: newPassword });
          toast.success(t('messages.saveSuccess'));
          setResettingCustomer(null);
      } catch {
          toast.error(t('messages.error'));
      }
  };


  const columns: DataTableColumn<CustomerDto>[] = [
    {
      id: 'fullName',
      header: t('common.fields.customer'),
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
      header: t('common.fields.contact'),
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
      id: 'role',
      header: t('common.fields.role'),
      accessor: (customer) => (
        <Badge color={customer.role === 'admin' ? 'error' : 'info'}>
          {t(`admin.types.${customer.role.toLowerCase()}`, customer.role)}
        </Badge>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    },
    CommonColumns.createStatusColumn(t),
    {
      id: 'isEmailVerified',
      header: t('common.fields.emailVerified', 'Email Verified'),
      accessor: (customer) => (
        <Badge color={customer.isEmailVerified ? 'success' : 'error'} variant="outline">
          {customer.isEmailVerified ? t('common.status.verified') : t('common.status.unactivated')}
        </Badge>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    },
    {
      id: 'lastLogin',
      header: t('common.fields.lastLogin', 'Last Login'),
      accessor: (customer) => <span className="text-slate-500">{customer.lastLogin ? formatDate(customer.lastLogin) : t('common.never', 'Never')}</span>,
      sortable: true
    },
    {
      id: 'joinDate',
      header: t('common.fields.joinDate'),
      accessor: (customer) => <span className="text-slate-500">{formatDate(customer.createdAt)}</span>,
      sortable: true
    },

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
    // Reset to starting state: just customers sorted by createdAt desc
    setFilters({ role: 'customer' }); 
    setPageIndex(PaginationDefaults.PAGE_INDEX);
    setSortField('createdAt');
    setSortDir(SortDirection.DESC);
    refetch();
  };

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="admin.sidebar.customers"
        subtitle="admin.subtitles.customers"
      />

      <StatsCards stats={stats} />

      <DataTable
        columns={columns}
        data={items}
        keyField="code"
        enableSelection
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        isLoading={isLoading}
        onBulkDelete={handleBulkDelete}
        onView={(customer) => setSelectedCustomer(customer)}
        onEdit={handleOpenEdit}
        onDelete={confirmDelete}
        onAdd={handleOpenCreate}
        onRefresh={refetch}
        initialFilters={{ role: 'customer' }}
        renderRowActions={(customer) => (
          <div className="flex items-center gap-1">
            <button
              className="p-1.5 text-slate-500 hover:text-blue-600 hover:bg-blue-50 rounded transition-colors"
              title={t('admin.actions.loginAs', 'Login As')}
              onClick={async (e) => {
                e.stopPropagation();
                if (window.confirm(t('messages.confirmImpersonate', { name: customer.fullName || customer.username }) || `Login as ${customer.fullName || customer.username}?`)) {
                  try {
                    const { impersonate } = useAuthStore.getState();
                    await impersonate(customer.code);
                    toast.success(t('messages.impersonateSuccess', 'Switched to user session'));
                    window.location.href = '/'; // Redirect to shop home
                  } catch {
                    toast.error(t('messages.error'));
                  }
                }
              }}
            >
              <LogIn size={18} />
            </button>
            <button
              className="p-1.5 text-slate-500 hover:text-amber-600 hover:bg-amber-50 rounded transition-colors"
              title={t('admin.actions.resetPassword')}
              onClick={() => handleResetPasswordPrompt(customer)}
            >
              <Key size={18} />
            </button>
          </div>
        )}

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
            label: t('common.fields.customer'),
            type: 'text',
            placeholder: t('common.placeholders.search')
          },
          {
            id: 'email',
            label: 'Email',
            type: 'text',
          },
          {
            id: 'phone',
            label: t('common.fields.phone'),
            type: 'text',
          },
          {
            id: 'role',
            label: t('common.fields.role'),
            type: 'select',
            options: [
              { value: 'customer', label: t('admin.types.customer') },
              { value: 'admin', label: t('admin.types.admin') },
              { value: 'staff', label: t('admin.types.staff') }
            ]
          }
        ]}

        // Pagination
        currentPage={pageIndex}
        totalItems={totalCount}
        totalPages={totalPages}
        pageSize={pageSize}
        onPageChange={setPageIndex}
        onPageSizeChange={(size) => {
          setPageSize(size);
          setPageIndex(PaginationDefaults.PAGE_INDEX);
        }}

        // Visibility
        enableColumnVisibility={true}
        exportFilename="customers_export"
        onExportAllData={async () => {
          const response = await customerService.search({ pageIndex: 1, pageSize: 10000 });
          return (response.data?.items || []) as unknown as CustomerDto[];
        }}
        emptyMessage={t('common.noResults')}
      />

      {/* Customer Detail Modal */}
      <Modal
        isOpen={!!selectedCustomer}
        onClose={() => setSelectedCustomer(null)}
        title={t('admin.actions.view') + ' ' + t('common.fields.customer')}
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
                <div className="flex items-center gap-2 mt-1">
                    <Badge color={selectedCustomer.isActive ? 'success' : 'secondary'}>
                    {selectedCustomer.isActive ? t('common.status.active') : t('common.status.inactive')}
                    </Badge>
                     <Badge color={selectedCustomer.role === 'admin' ? 'error' : 'info'}>{t(`admin.types.${selectedCustomer.role.toLowerCase()}`, selectedCustomer.role)}</Badge>
                </div>
              </div>
            </div>

            <div className="space-y-3">
              <h3 className="font-semibold text-slate-800 dark:text-white">{t('common.fields.contact')}</h3>
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
                <p className="flex items-center gap-2">
                  <span className="font-semibold w-24">{t('common.fields.username')}:</span>
                  {selectedCustomer.username}
                </p>
                <p className="flex items-center gap-2">
                  <span className="font-semibold w-24">{t('common.fields.address')}:</span>
                  {selectedCustomer.address ? `${selectedCustomer.address}, ${selectedCustomer.ward}, ${selectedCustomer.district}, ${selectedCustomer.city}` : t('common.actions.none')}
                </p>
              </div>
            </div>

            <div className="flex gap-3">
              <Button fullWidth>
                {t('common.fields.email')}
              </Button>
            </div>
             <div className="flex justify-end pt-4">
              <Button onClick={() => setSelectedCustomer(null)} variant="outline">
                {t('common.close')}
              </Button>
            </div>
          </div>
        )}
      </Modal>


      {/* Customer Create/Edit Modal */}
      <Modal
        isOpen={isFormOpen}
        onClose={closeForm}
        title={editingCustomer ? t('common.actions.edit') + ' ' + t('common.fields.customer') : t('common.actions.create') + ' ' + t('common.fields.customer')}
        size="lg"
      >
        <form onSubmit={handleSubmit} className="space-y-4">
            {!editingCustomer && (
                <Input
                    label={t('common.fields.username')}
                    initialData={promotionInitialData}
                    value={formData.username}
                    onChange={e => setFormData({...formData, username: e.target.value})}
                    isRequired
                    placeholder={t('common.placeholders.enterName')}
                />
            )}
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                 <Input
                    label={t('common.fields.customer')}
                    value={formData.fullName}
                    onChange={e => setFormData({...formData, fullName: e.target.value})}
                    placeholder={t('common.placeholders.enterName')}
                />
                 <Input
                    label={t('common.fields.email')}
                    type="email"
                    value={formData.email}
                    onChange={e => setFormData({...formData, email: e.target.value})}
                    isRequired
                    placeholder="example@email.com"
                />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                 <Input
                    label={t('common.fields.phone')}
                    value={formData.phone}
                    onChange={e => setFormData({...formData, phone: e.target.value})}
                    placeholder={t('common.placeholders.enterPhone')}
                />
                 <Select
                    label={t('common.fields.role')}
                    value={formData.role}
                    onChange={e => setFormData({...formData, role: e.target.value})}
                    options={[
                        { value: 'customer', label: t('admin.types.customer') },
                        { value: 'admin', label: t('admin.types.admin') },
                        { value: 'staff', label: t('admin.types.staff') }
                    ]}
                />
            </div>

              <div className="space-y-1">
                 <Input
                    label={editingCustomer ? t('common.fields.newPassword') : t('common.fields.password')}
                    type="password"
                    value={formData.password}
                    onChange={e => setFormData({...formData, password: e.target.value})}
                    isRequired={!editingCustomer}
                    placeholder={editingCustomer ? t('common.hints.leaveBlankKeepCurrent') : t('common.placeholders.enterPassword')}
                    error={(formData.password && !isPasswordValid(formData.password)) ? t('validation.weakPassword') : ''}
                />
                <p className="text-xs text-gray-500 dark:text-slate-400 px-1">
                    {t('common.hints.passwordRule')}
                </p>
              </div>

                <div className="p-4 bg-gray-50 dark:bg-slate-800/50 rounded-lg">
                     <Switch
                        label={t('common.fields.status')}
                        description={t('admin.statusHint', 'Bật để tài khoản hoạt động')}
                        checked={formData.isActive}
                        onChange={(checked) => setFormData({...formData, isActive: checked})}
                     />
                </div>

            <div className="flex justify-end gap-3 pt-4">
                <Button type="button" variant="ghost" onClick={closeForm}>
                    {t('common.cancel')}
                </Button>
                <Button type="submit" isLoading={isSubmitting}>
                    {editingCustomer ? t('common.save') : t('common.create')}
                </Button>
            </div>
        </form>
      </Modal>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!customerToDelete}
        onClose={cancelDelete}
        title={t('common.actions.delete')}
        message={t('messages.confirmDelete')}
        confirmText={t('common.delete')}
        onConfirm={() => customerToDelete && deleteCustomer(customerToDelete.code)}
        isLoading={isDeleting}
      />
      
      {/* Bulk Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!itemsToDelete}
        onClose={() => setItemsToDelete(null)}
        onConfirm={confirmBulkDelete}
        title={t('common.actions.delete')}
        message={t('common.confirmDelete', { count: itemsToDelete?.length || 0 })}
        confirmText={t('common.delete')}
        isLoading={bulkDeleteMutation.isPending}
      />

      <Modal
        isOpen={!!resettingCustomer}
        onClose={() => setResettingCustomer(null)}
        title={t('common.actions.resetPasswordTitle', { name: resettingCustomer?.fullName || resettingCustomer?.username })}
        size="sm"
      >
         <div className="space-y-4">
            <div className="space-y-1">
                <Input
                    label={t('common.fields.newPassword')}
                    type="password"
                    value={newPassword}
                    onChange={e => setNewPassword(e.target.value)}
                    placeholder={t('common.placeholders.enterPassword')}
                    isRequired
                    error={(newPassword && !isPasswordValid(newPassword)) ? t('validation.weakPassword') : ''}
                />
                <p className="text-xs text-gray-500 dark:text-slate-400 px-1">
                    {t('common.hints.passwordRule')}
                </p>
            </div>
            <div className="flex justify-end gap-3">
                <Button variant="ghost" onClick={() => setResettingCustomer(null)}>{t('common.cancel')}</Button>
                <Button onClick={confirmResetPassword} disabled={!newPassword || !isPasswordValid(newPassword)}>{t('common.update')}</Button>
            </div>
        </div>
      </Modal>
    </div>
  );
};

export default CustomersPage;
