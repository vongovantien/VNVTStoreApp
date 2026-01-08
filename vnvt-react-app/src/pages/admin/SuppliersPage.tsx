import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Edit2, Trash2, Building2, Phone, Mail, Loader2, AlertCircle, Search } from 'lucide-react';
import { Button, Badge, Modal, Input, ConfirmDialog, Pagination } from '@/components/ui';
import { useToast } from '@/store';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/services/api';

interface Supplier {
  code: string;
  name: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  address?: string;
  taxCode?: string;
  bankAccount?: string;
  bankName?: string;
  notes?: string;
  isActive: boolean;
  createdAt?: string;
}

const supplierService = {
  getAll: () => api.get<Supplier[]>('/suppliers'),
  create: (data: Partial<Supplier>) => api.post<Supplier>('/suppliers', data),
  update: (code: string, data: Partial<Supplier>) => api.put<Supplier>(`/suppliers/${code}`, data),
  delete: (code: string) => api.delete(`/suppliers/${code}`),
};

export const SuppliersPage = () => {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const toast = useToast();

  // Pagination & Search
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const pageSize = 10;

  // Fetch suppliers
  const { data: suppliersData, isLoading, isError, isFetching } = useQuery({
    queryKey: ['suppliers'],
    queryFn: () => supplierService.getAll(),
    select: (response) => response.data || [],
  });

  const suppliers = suppliersData || [];
  
  // Filter by search
  const filteredSuppliers = suppliers.filter((s: Supplier) =>
    s.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    s.email?.toLowerCase().includes(searchQuery.toLowerCase()) ||
    s.phone?.includes(searchQuery)
  );

  // Paginate
  const totalItems = filteredSuppliers.length;
  const totalPages = Math.ceil(totalItems / pageSize);
  const paginatedSuppliers = filteredSuppliers.slice(
    (currentPage - 1) * pageSize,
    currentPage * pageSize
  );

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
    mutationFn: (data: Partial<Supplier>) => supplierService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['suppliers'] });
      toast.success('Tạo nhà cung cấp thành công!');
      setIsFormOpen(false);
      resetForm();
    },
    onError: () => toast.error('Không thể tạo nhà cung cấp'),
  });

  const updateMutation = useMutation({
    mutationFn: (data: { code: string; payload: Partial<Supplier> }) =>
      supplierService.update(data.code, data.payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['suppliers'] });
      toast.success('Cập nhật nhà cung cấp thành công!');
      setIsFormOpen(false);
      setEditingSupplier(null);
      resetForm();
    },
    onError: () => toast.error('Không thể cập nhật nhà cung cấp'),
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => supplierService.delete(code),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['suppliers'] });
      toast.success('Xóa nhà cung cấp thành công!');
      setSupplierToDelete(null);
    },
    onError: () => toast.error('Không thể xóa nhà cung cấp'),
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

  const showError = isError && !isLoading;
  const isSubmitting = createMutation.isPending || updateMutation.isPending;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-bold">{t('admin.suppliers') || 'Nhà cung cấp'}</h1>
        <Button leftIcon={<Plus size={20} />} onClick={openCreateModal}>
          Thêm nhà cung cấp
        </Button>
      </div>

      {/* Search */}
      <div className="bg-primary rounded-xl p-4 border">
        <div className="max-w-md">
          <Input
            placeholder="Tìm kiếm nhà cung cấp..."
            leftIcon={<Search size={18} />}
            value={searchQuery}
            onChange={(e) => { setSearchQuery(e.target.value); setCurrentPage(1); }}
          />
        </div>
      </div>

      {/* Table */}
      <div className="bg-primary rounded-xl overflow-hidden shadow-sm border relative min-h-[400px]">
        {/* Loading overlay */}
        {(isLoading || isFetching) && (
          <div className="absolute inset-0 bg-primary/70 flex items-center justify-center z-10">
            <div className="flex items-center gap-3">
              <Loader2 className="w-6 h-6 animate-spin text-indigo-600" />
              <span className="text-secondary text-sm">Đang tải...</span>
            </div>
          </div>
        )}

        {/* Error overlay */}
        {showError && (
          <div className="absolute inset-0 bg-primary flex items-center justify-center z-10">
            <div className="text-center">
              <AlertCircle className="w-12 h-12 mx-auto mb-4 text-red-500" />
              <h3 className="font-semibold mb-2">Có lỗi xảy ra</h3>
            </div>
          </div>
        )}

        <div className="overflow-x-auto">
          <table className="w-full min-w-[800px]">
            <thead className="bg-secondary border-b">
              <tr>
                <th className="px-4 py-3 text-left text-sm font-semibold">Tên</th>
                <th className="px-4 py-3 text-left text-sm font-semibold">Người liên hệ</th>
                <th className="px-4 py-3 text-left text-sm font-semibold">Email</th>
                <th className="px-4 py-3 text-left text-sm font-semibold">Điện thoại</th>
                <th className="px-4 py-3 text-center text-sm font-semibold">Status</th>
                <th className="px-4 py-3 text-right text-sm font-semibold">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {paginatedSuppliers.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-4 py-12 text-center text-secondary">
                    <Building2 className="w-12 h-12 mx-auto mb-4 text-gray-300" />
                    <p>Chưa có nhà cung cấp nào</p>
                  </td>
                </tr>
              ) : (
                paginatedSuppliers.map((supplier: Supplier) => (
                  <tr key={supplier.code} className="border-b last:border-0 hover:bg-secondary/50 transition-colors">
                    <td className="px-4 py-4">
                      <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-lg bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center">
                          <Building2 className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
                        </div>
                        <div>
                          <p className="font-medium">{supplier.name}</p>
                          {supplier.taxCode && (
                            <p className="text-xs text-tertiary">MST: {supplier.taxCode}</p>
                          )}
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-4 text-secondary">{supplier.contactPerson || '-'}</td>
                    <td className="px-4 py-4">
                      {supplier.email && (
                        <a href={`mailto:${supplier.email}`} className="flex items-center gap-1 text-indigo-600 hover:underline text-sm">
                          <Mail size={14} />
                          {supplier.email}
                        </a>
                      )}
                    </td>
                    <td className="px-4 py-4">
                      {supplier.phone && (
                        <a href={`tel:${supplier.phone}`} className="flex items-center gap-1 text-sm">
                          <Phone size={14} />
                          {supplier.phone}
                        </a>
                      )}
                    </td>
                    <td className="px-4 py-4 text-center">
                      <Badge color={supplier.isActive ? 'success' : 'gray'} size="sm" variant="outline">
                        {supplier.isActive ? 'Active' : 'Inactive'}
                      </Badge>
                    </td>
                    <td className="px-4 py-4 text-right">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => openEditModal(supplier)}
                          className="p-2 hover:bg-secondary rounded-lg transition-colors"
                          title="Sửa"
                        >
                          <Edit2 size={16} className="text-primary" />
                        </button>
                        <button
                          onClick={() => setSupplierToDelete(supplier)}
                          className="p-2 hover:bg-error/10 rounded-lg transition-colors"
                          title="Xóa"
                        >
                          <Trash2 size={16} className="text-error" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            totalItems={totalItems}
            pageSize={pageSize}
            onPageChange={setCurrentPage}
          />
        )}
      </div>

      {/* Create/Edit Modal */}
      <Modal
        isOpen={isFormOpen}
        onClose={() => { setIsFormOpen(false); setEditingSupplier(null); resetForm(); }}
        title={editingSupplier ? 'Sửa nhà cung cấp' : 'Thêm nhà cung cấp mới'}
        size="lg"
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input
              label="Tên nhà cung cấp *"
              placeholder="Nhập tên"
              value={formData.name}
              onChange={(e) => updateFormField('name', e.target.value)}
              required
            />
            <Input
              label="Người liên hệ"
              placeholder="Nhập tên người liên hệ"
              value={formData.contactPerson}
              onChange={(e) => updateFormField('contactPerson', e.target.value)}
            />
            <Input
              label="Email"
              type="email"
              placeholder="email@example.com"
              value={formData.email}
              onChange={(e) => updateFormField('email', e.target.value)}
            />
            <Input
              label="Số điện thoại"
              placeholder="0123 456 789"
              value={formData.phone}
              onChange={(e) => updateFormField('phone', e.target.value)}
            />
            <Input
              label="Mã số thuế"
              placeholder="Nhập MST"
              value={formData.taxCode}
              onChange={(e) => updateFormField('taxCode', e.target.value)}
            />
            <Input
              label="Địa chỉ"
              placeholder="Nhập địa chỉ"
              value={formData.address}
              onChange={(e) => updateFormField('address', e.target.value)}
            />
            <Input
              label="Số tài khoản"
              placeholder="Nhập STK"
              value={formData.bankAccount}
              onChange={(e) => updateFormField('bankAccount', e.target.value)}
            />
            <Input
              label="Ngân hàng"
              placeholder="Tên ngân hàng"
              value={formData.bankName}
              onChange={(e) => updateFormField('bankName', e.target.value)}
            />
          </div>
          <Input
            label="Ghi chú"
            placeholder="Ghi chú (tùy chọn)"
            value={formData.notes}
            onChange={(e) => updateFormField('notes', e.target.value)}
          />
          <div className="flex justify-end gap-3 pt-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => { setIsFormOpen(false); setEditingSupplier(null); resetForm(); }}
            >
              Hủy
            </Button>
            <Button type="submit" isLoading={isSubmitting} disabled={!formData.name?.trim()}>
              {editingSupplier ? 'Cập nhật' : 'Tạo mới'}
            </Button>
          </div>
        </form>
      </Modal>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!supplierToDelete}
        onClose={() => setSupplierToDelete(null)}
        onConfirm={handleDelete}
        title="Xác nhận xóa"
        message={
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Bạn có chắc chắn muốn xóa nhà cung cấp <strong className="text-gray-900 dark:text-white">"{supplierToDelete?.name}"</strong>?
          </p>
        }
        confirmText="Xóa"
        variant="danger"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
};

export default SuppliersPage;
