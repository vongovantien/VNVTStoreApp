import { useTranslation } from 'react-i18next';
import { Folder } from 'lucide-react';
import { Badge } from '@/components/ui';
import { DataTable, type DataTableColumn, CommonColumns } from '@/components/common';
import { type CategoryDto } from '@/services/productService';
import { getImageUrl } from '@/utils/format';
import { TableActions } from '@/components/ui';

interface CategoryListProps {
    categories: CategoryDto[];
    isLoading: boolean;
    pagination: {
        pageIndex: number;
        pageSize: number;
        totalItems: number;
        totalPages: number;
    };
    onPageChange: (page: number) => void;
    onPageSizeChange: (size: number) => void;
    onAdd: () => void;
    onEdit: (category: CategoryDto) => void;
    onDelete: (category: CategoryDto) => void;
    onView: (category: CategoryDto) => void;
    onRefresh: () => void;
    onImport: (file: File) => Promise<void>;
    onExport: () => Promise<CategoryDto[]>;
    selectedIds: Set<string>;
    onSelectionChange: (ids: Set<string>) => void;
}

export const CategoryList = ({
    categories,
    isLoading,
    pagination,
    onPageChange,
    onPageSizeChange,
    onAdd,
    onEdit,
    onDelete,
    onView,
    onRefresh,
    onImport,
    onExport,
    selectedIds,
    onSelectionChange
}: CategoryListProps) => {
    const { t } = useTranslation();

    const columns: DataTableColumn<CategoryDto>[] = [
        {
            id: 'imageURL',
            header: t('common.fields.image'),
            width: '120px',
            className: 'text-center',
            accessor: (category) => (
                <div className="flex flex-col items-center">
                    <div className="w-10 h-10 rounded-lg overflow-hidden bg-gray-100 dark:bg-slate-700 border border-gray-200 dark:border-slate-600">
                        {category.imageURL ? (
                            <img 
                                src={getImageUrl(category.imageURL)} 
                                alt={category.name} 
                                className="w-full h-full object-cover"
                            />
                        ) : (
                            <div className="w-full h-full flex items-center justify-center text-gray-400">
                                <Folder size={18} />
                            </div>
                        )}
                    </div>
                </div>
            )
        },
        {
            id: 'name',
            header: t('common.fields.name'),
            accessor: 'name',
            sortable: true
        },
        { 
            id: 'description',
            header: t('common.fields.description'),
            accessor: 'description',
            className: 'hidden md:table-cell text-gray-500' 
        },
        {
            id: 'parentCode',
            header: t('common.fields.parentCategory'),
            accessor: (category) => {
                if (!category.parentCode) return <span className="text-gray-400 italic">{t('common.none')}</span>;
                return (
                    <Badge variant="outline" className="gap-1">
                        <Folder size={10} className="mr-1" />
                        {category.parentCode} 
                    </Badge>
                );
            }
        },
        CommonColumns.createStatusColumn(t),
        {
            id: 'actions',
            header: t('common.fields.action'),
            width: '100px',
            className: 'w-[100px]',
            accessor: (category) => (
                <TableActions
                    onView={() => onView(category)}
                    onEdit={() => onEdit(category)}
                    onDelete={() => onDelete(category)}
                />
            )
        }
    ];

    return (
        <DataTable
            data={categories}
            columns={columns}
            isLoading={isLoading}
            searchPlaceholder={t('common.placeholders.search')}
            onAdd={onAdd}
            onRefresh={onRefresh}
            onImport={onImport}
            importTemplateUrl="/categories/template"
            importTitle={t('common.importData')}
            onExportAllData={onExport}
            keyField="code"
            enableSelection
            selectedIds={selectedIds}
            onSelectionChange={onSelectionChange}
            
            // Server-Side Pagination
            currentPage={pagination.pageIndex}
            totalItems={pagination.totalItems}
            totalPages={pagination.totalPages}
            pageSize={pagination.pageSize}
            onPageChange={onPageChange}
            onPageSizeChange={onPageSizeChange}
        />
    );
};
