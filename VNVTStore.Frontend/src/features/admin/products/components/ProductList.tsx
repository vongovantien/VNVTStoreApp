import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui';
import { DataTable } from '@/components/common/DataTable';
import { Product } from '@/types';
import { useProductColumns } from '@/pages/admin/hooks/useProductColumns'; // Reuse existing columns logic

interface ProductListProps {
    products: Product[];
    totalItems: number;
    totalPages: number;
    page: number;
    pageSize: number;
    isLoading: boolean;
    isFetching: boolean;
    onPageChange: (page: number) => void;
    onPageSizeChange: (size: number) => void;
    onSearch: (query: string) => void;
    onAdd: () => void;
    onEdit: (product: Product) => void;
    onDelete: (product: Product) => void;
    onBulkDelete: (ids: string[]) => void;
    onRefresh: () => void;
}

export const ProductList: React.FC<ProductListProps> = ({
    products,
    totalItems,
    totalPages,
    page,
    pageSize,
    isLoading,
    isFetching,
    onPageChange,
    onPageSizeChange,
    onSearch,
    onAdd,
    onEdit,
    onDelete,
    onBulkDelete,
    onRefresh
}) => {
    const { t } = useTranslation();
    const columns = useProductColumns();
    const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

    const handleBulkDelete = () => {
        if (selectedIds.size > 0) {
            onBulkDelete(Array.from(selectedIds));
        }
    };

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center bg-white dark:bg-slate-900 p-4 rounded-lg shadow-sm border border-border-color">
                <div className="flex items-center gap-2">
                    <h2 className="text-lg font-semibold">{t('common.modules.products')}</h2>
                    <span className="bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400 px-2 py-0.5 rounded-full text-xs font-medium">
                        {totalItems}
                    </span>
                </div>
                <div className="flex gap-2">
                    {selectedIds.size > 0 && (
                        <Button variant="danger" size="sm" onClick={handleBulkDelete} leftIcon={<Trash2 size={16} />}>
                            {t('common.deleteSelected')} ({selectedIds.size})
                        </Button>
                    )}
                    <Button onClick={onAdd} leftIcon={<Plus size={16} />}>
                        {t('common.actions.create')}
                    </Button>
                </div>
            </div>

            <DataTable
                columns={columns}
                data={products}
                keyField="code"
                enableSelection
                selectedIds={selectedIds}
                onSelectionChange={setSelectedIds}
                isLoading={isLoading}
                isFetching={isFetching}
                
                // Pagination
                currentPage={page}
                totalPages={totalPages}
                totalItems={totalItems}
                pageSize={pageSize}
                onPageChange={onPageChange}
                onPageSizeChange={onPageSizeChange}

                // Actions
                onSearch={onSearch}
                onRefresh={onRefresh}
                onEdit={onEdit}
                onDelete={onDelete}
                
                // Styling
                className="bg-white dark:bg-slate-900 rounded-lg border border-border-color shadow-sm"
            />
        </div>
    );
};
