import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Building2, Phone, Mail, RefreshCw, Edit, Trash2, Eye } from 'lucide-react';
import { Button, Badge, Modal, Input, ConfirmDialog } from '@/components/ui';
import { useToast } from '@/store';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { supplierService, type SupplierDto } from '@/services';
import { DataTable, type DataTableColumn } from '@/components/common';

import { useEntityManager } from '@/hooks';

type Supplier = SupplierDto;

export const SuppliersPage = () => {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const toast = useToast();

  // Fetch suppliers
  const { data: suppliersData, isLoading, isError, isFetching, error, refetch } = useQuery({
    queryKey: ['suppliers'],
    queryFn: () => supplierService.getAll(),
    select: (response) => response.data?.items || [],
  });

  const suppliers = (suppliersData || []) as Supplier[];

  // Modal State via useCrud
  const {
    isFormOpen,
    editingItem: editingSupplier,
    itemToDelete: supplierToDelete,
    isLoading: isSubmitting,
    isDeleting,
    openCreate,
    openEdit,
    closeForm,
    confirmDelete,
    cancelDelete,
    create: createSupplier,
    update: updateSupplier,
    delete: deleteSupplier
  } = useEntityManager<Supplier, Partial<Supplier>, Partial<Supplier>>({
    service: supplierService as any, // casting if types slightly mismatch, usually supplierService matches EntityService
    queryKey: ['suppliers'],
  });

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

  // View State
  const [viewingSupplier, setViewingSupplier] = useState<Supplier | null>(null);

  const handleOpenCreate = () => {
    openCreate();
    resetForm();
  };

  const handleOpenEdit = (supplier: Supplier) => {
    openEdit(supplier);
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
  };

  const handleOpenView = (supplier: Supplier) => {
    setViewingSupplier(supplier);
  };

  const handleSubmit = useCallback((e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.name?.trim()) return;

    if (editingSupplier) {
      updateSupplier(editingSupplier.code, formData);
    } else {
      createSupplier(formData);
    }
  }, [formData, editingSupplier, createSupplier, updateSupplier]);

  const handleDelete = () => {
    if (supplierToDelete) {
      deleteSupplier(supplierToDelete.code);
    }
  };

  const updateFormField = (field: keyof Supplier, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleReset = useCallback(() => {
    refetch(); // Force API refetch
  }, [refetch]);

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
        <Badge color={supplier.isActive ? 'success' : 'error'} size="sm">
          {supplier.isActive ? t('common.active') : t('common.inactive')}
        </Badge>
      ),
      sortable: true,
      className: 'text-center',
      headerClassName: 'text-center',
    },
    {
      id: 'actions',
      header: t('admin.columns.action'),
      accessor: (supplier) => (
        <div className="flex items-center justify-center gap-2">
          <button
            className="p-2 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-lg transition-colors text-slate-500"
            title={t('admin.actions.view')}
            onClick={() => handleOpenView(supplier)}
          >
            <Eye size={16} />
          </button>
          <button
            className="p-2 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded-lg transition-colors text-blue-600"
            title={t('admin.actions.edit')}
            onClick={() => handleOpenEdit(supplier)}
          >
            <Edit size={16} />
          </button>
          <button
            className="p-2 hover:bg-rose-50 dark:hover:bg-rose-900/20 rounded-lg transition-colors text-rose-600"
            title={t('admin.actions.delete')}
            onClick={() => confirmDelete(supplier)}
          >
            <Trash2 size={16} />
          </button>
        </div>
      ),
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
        onAdd={handleOpenCreate}
        onEdit={handleOpenEdit}
        onView={handleOpenView}
        onDelete={confirmDelete}
        onReset={handleReset}
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

      {/* View Modal */}
      <Modal
        isOpen={!!viewingSupplier}
        onClose={() => setViewingSupplier(null)}
        title={t('admin.actions.view') + ' ' + t('admin.suppliers')}
        size="lg"
      >
        {viewingSupplier && (
          <div className="space-y-6">
            <div className="flex items-center gap-4">
              <div className="w-16 h-16 rounded-lg bg-blue-100 flex items-center justify-center text-blue-600">
                <Building2 size={32} />
              </div>
              <div>
                <h2 className="text-xl font-bold text-slate-800 dark:text-white">{viewingSupplier.name}</h2>
                <div className="flex items-center gap-4 mt-1 text-sm text-slate-500">
                  {viewingSupplier.contactPerson && <span>{viewingSupplier.contactPerson}</span>}
                  {viewingSupplier.taxCode && <span>MST: {viewingSupplier.taxCode}</span>}
                  <Badge color={viewingSupplier.isActive ? 'success' : 'error'} size="sm">
                    {viewingSupplier.isActive ? t('common.active') : t('common.inactive')}
                  </Badge>
                </div>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-4">
                <h3 className="font-semibold text-slate-800 dark:text-white border-b pb-2">{t('admin.columns.contact')}</h3>
                <div className="space-y-3">
                  <div className="flex items-start gap-3">
                    <Mail className="w-5 h-5 text-slate-400 mt-0.5" />
                    <div>
                      <p className="text-sm font-medium text-slate-700 dark:text-slate-200">Email</p>
                      <p className="text-sm text-slate-500">{viewingSupplier.email || '-'}</p>
                    </div>
                  </div>
                  <div className="flex items-start gap-3">
                    <Phone className="w-5 h-5 text-slate-400 mt-0.5" />
                    <div>
                      <p className="text-sm font-medium text-slate-700 dark:text-slate-200">Phone</p>
                      <p className="text-sm text-slate-500">{viewingSupplier.phone || '-'}</p>
                    </div>
                  </div>
                  <div className="flex items-start gap-3">
                    <Building2 className="w-5 h-5 text-slate-400 mt-0.5" />
                    <div>
                      <p className="text-sm font-medium text-slate-700 dark:text-slate-200">Address</p>
                      <p className="text-sm text-slate-500">{viewingSupplier.address || '-'}</p>
                    </div>
                  </div>
                </div>
              </div>

              <div className="space-y-4">
                <h3 className="font-semibold text-slate-800 dark:text-white border-b pb-2">{t('admin.columns.bankAccount')}</h3>
                <div className="space-y-3">
                  <div className="flex items-start gap-3">
                    <div className="w-5 h-5" />
                    <div>
                      <p className="text-sm font-medium text-slate-700 dark:text-slate-200">Bank Name</p>
                      <p className="text-sm text-slate-500">{viewingSupplier.bankName || '-'}</p>
                    </div>
                  </div>
                  <div className="flex items-start gap-3">
                    <div className="w-5 h-5" />
                    <div>
                      <p className="text-sm font-medium text-slate-700 dark:text-slate-200">Account Number</p>
                      <p className="text-sm text-slate-500">{viewingSupplier.bankAccount || '-'}</p>
                    </div>
                  </div>
                </div>

                {viewingSupplier.notes && (
                  <div className="mt-6">
                    <h3 className="font-semibold text-slate-800 dark:text-white border-b pb-2">{t('admin.columns.notes')}</h3>
                    <p className="text-sm text-slate-500 mt-2">{viewingSupplier.notes}</p>
                  </div>
                )}
              </div>
            </div>

            <div className="flex justify-end pt-4">
              <Button onClick={() => setViewingSupplier(null)}>
                {t('common.close')}
              </Button>
            </div>
          </div>
        )}
      </Modal>

      {/* Form Modal */}
      <Modal
        isOpen={isFormOpen}
        onClose={closeForm}
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
              onClick={closeForm}
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
        onClose={cancelDelete}
        onConfirm={handleDelete}
        title={t('admin.actions.delete')}
        message={t('common.confirmDelete', { count: 1 })}
        confirmText={t('common.delete')}
        isLoading={isDeleting}
      />
    </div>
  );
};

export default SuppliersPage;
