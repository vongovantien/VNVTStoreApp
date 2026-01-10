import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Building2, Phone, Mail, RefreshCw } from 'lucide-react';
import { Button, Badge, Modal, Input, ConfirmDialog } from '@/components/ui';
import { useToast } from '@/store';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { supplierService, type SupplierDto } from '@/services';
import { DataTable, type DataTableColumn } from '@/components/common';

type Supplier = SupplierDto;

export const SuppliersPage = () => {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const toast = useToast();

  // Fetch suppliers
  const { data: suppliersData, isLoading, isError, isFetching, error } = useQuery({
    queryKey: ['suppliers'],
    queryFn: () => supplierService.getAll(),
    select: (response) => response.data?.items || [],
  });

  const suppliers = (suppliersData || []) as Supplier[];

  // Modal State
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingSupplier, setEditingSupplier] = useState<Supplier | null>(null);
  const [supplierToDelete, setSupplierToDelete] = useState<Supplier | null>(null);

  // Form State
  const [formData, setFormData] = useState<Partial<Supplier>>({
    name: '',
    contactPerson: '',
    email: '',
    phone: '',
    address: '',
    taxCode: '',
    bankAccount: '',
    bankName: '',
    notes: '',
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: (data: Partial<Supplier>) => supplierService.create(data as any),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['suppliers'] });
      toast.success(t('messages.createSuccess'));
      setIsFormOpen(false);
      resetForm();
    },
    onError: (error: Error) => toast.error(error.message || t('messages.createError')),
  });

  const updateMutation = useMutation({
    mutationFn: (data: { code: string; payload: Partial<Supplier> }) =>
      supplierService.update(data.code, data.payload as SupplierDto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['suppliers'] });
      toast.success(t('messages.updateSuccess'));
      setIsFormOpen(false);
      setEditingSupplier(null);
      resetForm();
    },
    onError: (error: Error) => toast.error(error.message || t('messages.updateError')),
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => supplierService.delete(code),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['suppliers'] });
      toast.success(t('messages.deleteSuccess'));
      setSupplierToDelete(null);
    },
    onError: (error: Error) => toast.error(error.message || t('messages.deleteError')),
  });

  const resetForm = () => {
    setFormData({
      name: '',
      contactPerson: '',
      email: '',
      phone: '',
      address: '',
      taxCode: '',
      bankAccount: '',
      bankName: '',
      notes: '',
    });
  };

  const openCreateModal = () => {
    setEditingSupplier(null);
    resetForm();
    setIsFormOpen(true);
  };

  const openEditModal = (supplier: Supplier) => {
    setEditingSupplier(supplier);
    setFormData({
      name: supplier.name,
      contactPerson: supplier.contactPerson || '',
      email: supplier.email || '',
      phone: supplier.phone || '',
      address: supplier.address || '',
      taxCode: supplier.taxCode || '',
      bankAccount: supplier.bankAccount || '',
      bankName: supplier.bankName || '',
      notes: supplier.notes || '',
    });
    setIsFormOpen(true);
  };

  const handleSubmit = useCallback((e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.name?.trim()) return;

    if (editingSupplier) {
      updateMutation.mutate({ code: editingSupplier.code, payload: formData });
    } else {
      createMutation.mutate(formData);
    }
  }, [formData, editingSupplier, createMutation, updateMutation]);

  const handleDelete = () => {
    if (supplierToDelete) {
      deleteMutation.mutate(supplierToDelete.code);
    }
  };

  const updateFormField = (field: keyof Supplier, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const isSubmitting = createMutation.isPending || updateMutation.isPending;

  // Column definitions for DataTable
  const columns: DataTableColumn<Supplier>[] = [
    {
      id: 'name',
      header: t('admin.columns.name'),
      accessor: (supplier) => (
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-lg bg-secondary flex items-center justify-center">
            <Building2 size={20} className="text-primary" />
          </div>
          <div>
            <p className="font-medium">{supplier.name}</p>
            {supplier.contactPerson && (
              <p className="text-xs text-tertiary">{supplier.contactPerson}</p>
            )}
          </div>
        </div>
      ),
      sortable: true,
    },
    {
      id: 'email',
      header: t('admin.columns.email'),
      accessor: (supplier) => supplier.email ? (
        <div className="flex items-center gap-2 text-secondary">
          <Mail size={14} /> {supplier.email}
        </div>
      ) : '-',
      sortable: true,
    },
    {
      id: 'phone',
      header: t('admin.columns.phone'),
      accessor: (supplier) => supplier.phone ? (
        <div className="flex items-center gap-2 text-secondary">
          <Phone size={14} /> {supplier.phone}
        </div>
      ) : '-',
    },
    {
      id: 'isActive',
      header: t('admin.columns.status'),
      accessor: (supplier) => (
        <Badge color={supplier.isActive ? 'success' : 'secondary'} size="sm">
          {supplier.isActive ? t('common.active') : t('common.inactive')}
        </Badge>
      ),
      sortable: true,
      className: 'text-center',
      headerClassName: 'text-center',
    },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-800 dark:text-slate-100">{t('admin.suppliers')}</h1>

      <DataTable
        columns={columns}
        data={suppliers}
        keyField="code"
        isLoading={isLoading}
        isFetching={isFetching}
        error={isError ? (error as Error) : null}
        onAdd={openCreateModal}
        onEdit={openEditModal}
        onDelete={(supplier) => setSupplierToDelete(supplier)}
        exportFilename="suppliers_export"
        searchOptions={[
          { label: t('admin.columns.name'), value: 'name' },
          { label: t('admin.columns.email'), value: 'email' },
          { label: t('admin.columns.phone'), value: 'phone' },
          { label: t('admin.columns.contactPerson'), value: 'contactPerson' },
          { label: t('admin.columns.taxCode'), value: 'taxCode' },
        ]}
        advancedFilterDefs={[
          {
            id: 'name',
            label: t('admin.columns.name'),
            type: 'text',
            placeholder: 'Tên nhà cung cấp...'
          },
          {
            id: 'contactPerson',
            label: t('admin.columns.contactPerson'),
            type: 'text',
          },
          {
            id: 'email',
            label: t('admin.columns.email'),
            type: 'text',
          },
          {
            id: 'phone',
            label: t('admin.columns.phone'),
            type: 'text',
          },
          {
            id: 'taxCode',
            label: t('admin.columns.taxCode'),
            type: 'text',
          },
          {
            id: 'isActive',
            label: t('admin.columns.status'),
            type: 'select',
            options: [
              { value: 'true', label: t('common.active') },
              { value: 'false', label: t('common.inactive') }
            ]
          }
        ]}
        enableColumnVisibility={true}
        emptyMessage={t('common.noResults')}
      />

      {/* Form Modal */}
      <Modal
        isOpen={isFormOpen}
        onClose={() => { setIsFormOpen(false); setEditingSupplier(null); }}
        title={editingSupplier ? t('admin.actions.edit') : t('admin.actions.create')}
        size="lg"
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            label={t('admin.columns.name') + ' *'}
            placeholder={t('admin.columns.name')}
            value={formData.name}
            onChange={(e) => updateFormField('name', e.target.value)}
            required
          />
          <Input
            label={t('admin.columns.contactPerson')}
            placeholder={t('admin.columns.contactPerson')}
            value={formData.contactPerson}
            onChange={(e) => updateFormField('contactPerson', e.target.value)}
          />
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input
              label={t('admin.columns.email')}
              type="email"
              placeholder={t('admin.columns.email')}
              value={formData.email}
              onChange={(e) => updateFormField('email', e.target.value)}
            />
            <Input
              label={t('admin.columns.phone')}
              placeholder={t('admin.columns.phone')}
              value={formData.phone}
              onChange={(e) => updateFormField('phone', e.target.value)}
            />
          </div>
          <Input
            label={t('admin.columns.address')}
            placeholder={t('admin.columns.address')}
            value={formData.address}
            onChange={(e) => updateFormField('address', e.target.value)}
          />
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input
              label={t('admin.columns.taxCode')}
              placeholder={t('admin.columns.taxCode')}
              value={formData.taxCode}
              onChange={(e) => updateFormField('taxCode', e.target.value)}
            />
            <Input
              label={t('admin.columns.bankName')}
              placeholder={t('admin.columns.bankName')}
              value={formData.bankName}
              onChange={(e) => updateFormField('bankName', e.target.value)}
            />
          </div>
          <Input
            label={t('admin.columns.bankAccount')}
            placeholder={t('admin.columns.bankAccount')}
            value={formData.bankAccount}
            onChange={(e) => updateFormField('bankAccount', e.target.value)}
          />
          <Input
            label={t('admin.columns.notes')}
            placeholder={t('admin.columns.notes')}
            value={formData.notes}
            onChange={(e) => updateFormField('notes', e.target.value)}
          />
          <div className="flex justify-end gap-3 pt-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => { setIsFormOpen(false); setEditingSupplier(null); }}
            >
              {t('common.cancel')}
            </Button>
            <Button type="submit" isLoading={isSubmitting}>
              {editingSupplier ? t('common.update') : t('common.create')}
            </Button>
          </div>
        </form>
      </Modal>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!supplierToDelete}
        onClose={() => setSupplierToDelete(null)}
        onConfirm={handleDelete}
        title={t('admin.actions.delete')}
        message={t('common.confirmDelete', { count: 1 })}
        confirmText={t('common.delete')}
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
};

export default SuppliersPage;
