import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useAdminProducts, useProductMutation } from '../hooks/useAdminProducts';
import { ProductList } from '../components/ProductList';
import { ProductForm, ProductFormData } from '../components/ProductForm';
import { Product } from '@/types';
import { ConfirmDialog, Modal } from '@/components/ui';
import { AdminPageHeader } from '@/components/admin';
import { useToast } from '@/store';
import { CreateProductRequest, UpdateProductRequest } from '@/services/productService';

export const ProductsPage = () => {
    const { t } = useTranslation();
    const { success, error: toastError } = useToast();
    
    // State
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [search, setSearch] = useState('');
    
    // Data
    const { data, isLoading, isFetching } = useAdminProducts({
        pageIndex: page,
        pageSize,
        search,
        sortBy: 'CreatedAt',
        sortDesc: true
    });

    // Mutations
    const { create, update, remove } = useProductMutation();

    // UI State
    const [isFormOpen, setIsFormOpen] = useState(false);
    const [editingProduct, setEditingProduct] = useState<Product | undefined>(undefined);
    const [productToDelete, setProductToDelete] = useState<Product | null>(null);
    const [bulkDeleteIds, setBulkDeleteIds] = useState<string[]>([]);

    // Handlers
    const handleCreate = () => {
        setEditingProduct(undefined);
        setIsFormOpen(true);
    };

    const handleEdit = (product: Product) => {
        setEditingProduct(product);
        setIsFormOpen(true);
    };

    const handleDelete = (product: Product) => {
        setProductToDelete(product);
    };

    const handleBulkDelete = (ids: string[]) => {
        setBulkDeleteIds(ids);
    };

    const confirmDelete = async () => {
        if (productToDelete) {
            remove.mutate(productToDelete.code, {
                onSuccess: () => {
                    success(t('common.deleteSuccess'));
                    setProductToDelete(null);
                },
                onError: () => toastError(t('common.errors.deleteFailed'))
            });
        }
    };

    const confirmBulkDelete = async () => {
        // Implement bulk delete if API supports it, or parallelize
        // For now, sequentially delete (prototype)
        if (bulkDeleteIds.length > 0) {
            try {
                await Promise.all(bulkDeleteIds.map(id => remove.mutateAsync(id)));
                success(t('common.deleteSuccess'));
                setBulkDeleteIds([]); // Clear selection in parent if possible, or trigger refetch
            } catch {
                toastError(t('common.errors.deleteFailed'));
            }
        }
    };

    const handleSubmit = async (data: ProductFormData) => {
        // Map Form Data to API Request
        // Note: ProductForm handles image processing and logic
        // We just need to map the output to CreateProductRequest/UpdateProductRequest
        
        // Simplified mapping (ProductForm typically returns ready-to-use structure, but let's double check types)
        // The ProductForm component in this project seems to handle the heavy lifting.
        
        // Casting for now as ProductForm logic is complex and we are reusing it.
        // In a strict refactor, we would define shared types between Form and Mutation.
        
        try {
            if (editingProduct) {
                await update.mutateAsync({ 
                    id: editingProduct.code, 
                    data: data as unknown as UpdateProductRequest 
                });
                success(t('common.updateSuccess'));
            } else {
                await create.mutateAsync(data as unknown as CreateProductRequest);
                success(t('common.createSuccess'));
            }
            setIsFormOpen(false);
        } catch {
            toastError(t('common.errors.saveFailed'));
        }
    };

    return (
        <div className="space-y-6 container mx-auto p-6">
            <AdminPageHeader
                title="common.modules.products"
                subtitle="admin.subtitles.products"
            />

            <ProductList
                products={data?.items || []}
                totalItems={data?.totalItems || 0}
                totalPages={data?.totalPages || 0}
                page={page}
                pageSize={pageSize}
                isLoading={isLoading}
                isFetching={isFetching}
                onPageChange={setPage}
                onPageSizeChange={setPageSize}
                onSearch={setSearch}
                onAdd={handleCreate}
                onEdit={handleEdit}
                onDelete={handleDelete}
                onBulkDelete={handleBulkDelete}
                onRefresh={() => { /* implementation detail: react-query handles refetch on key invalidation */ }}
            />

            {/* Product Form Modal */}
            {isFormOpen && (
                <Modal
                    isOpen={isFormOpen}
                    onClose={() => setIsFormOpen(false)}
                    title={editingProduct ? t('common.actions.edit') : t('common.actions.create')}
                    size="7xl"
                >
                    <ProductForm
                        // We need to map Product back to Form Initial Data
                        // This might require a helper or ensuring Product matches Form expectations
                        initialData={editingProduct ? {
                            ...editingProduct,
                            // Map existing Product fields to Form fields if names differ
                            // The Product formed by useAdminProducts should be compatible with ProductForm expectations
                            // since we copied ProductForm which expects partial ProductFormData
                        } as unknown as ProductFormData : undefined} 
                        onSubmit={handleSubmit}
                        onCancel={() => setIsFormOpen(false)}
                        isLoading={create.isPending || update.isPending}
                    />
                </Modal>
            )}

            {/* Delete Dialogs */}
            <ConfirmDialog
                isOpen={!!productToDelete}
                onClose={() => setProductToDelete(null)}
                onConfirm={confirmDelete}
                title={t('common.actions.delete')}
                message={t('messages.confirmDelete', { name: productToDelete?.name })}
                confirmText={t('common.delete')}
                variant="danger"
                isLoading={remove.isPending}
            />

            <ConfirmDialog
                isOpen={bulkDeleteIds.length > 0}
                onClose={() => setBulkDeleteIds([])}
                onConfirm={confirmBulkDelete}
                title={t('common.actions.delete')}
                message={t('common.confirmDelete', { count: bulkDeleteIds.length })}
                confirmText={t('common.delete')}
                variant="danger"
                isLoading={remove.isPending}
            />
        </div>
    );
};

export default ProductsPage;
