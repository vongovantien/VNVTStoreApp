import { useTranslation } from 'react-i18next';
import { Tag } from 'lucide-react';
import { DataTable, type DataTableColumn, CommonColumns } from '@/components/common';
import { type BrandDto } from '@/services/brandService';
import { getImageUrl } from '@/utils/format';
import { TableActions } from '@/components/ui';

interface BrandListProps {
    brands: BrandDto[];
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
    onEdit: (brand: BrandDto) => void;
    onDelete: (brand: BrandDto) => void;
    onBulkDelete: (items: BrandDto[]) => void;
    onView: (brand: BrandDto) => void;
    onRefresh: () => void;
    onImport: (file: File) => Promise<void>;
    onExport: () => Promise<BrandDto[]>;
    selectedIds: Set<string>;
    onSelectionChange: (ids: Set<string>) => void;
}

export const BrandList = ({
    brands,
    isLoading,
    pagination,
    onPageChange,
    onPageSizeChange,
    onAdd,
    onEdit,
    onDelete,
    onBulkDelete,
    onView,
    onRefresh,
    onImport,
    onExport,
    selectedIds,
    onSelectionChange
}: BrandListProps) => {
    const { t } = useTranslation();

    const columns: DataTableColumn<BrandDto>[] = [
        {
            id: 'logo',
            header: t('common.fields.image'),
            accessor: (brand) => brand.logoUrl ? (
                <img src={getImageUrl(brand.logoUrl)} alt={brand.name} className="w-10 h-10 object-contain rounded-md border border-gray-100" />
            ) : (
                <div className="w-10 h-10 rounded-md bg-gray-50 flex items-center justify-center text-gray-400">
                    <Tag size={20} />
                </div>
            ),
            className: 'w-[120px]'
        },
        {
            id: 'name',
            header: t('common.fields.name'),
            accessor: 'name',
            sortable: true,
        },
        {
            id: 'description',
            header: t('common.fields.description'),
            accessor: 'description',
            className: 'hidden md:table-cell max-w-[300px] truncate'
        },
        CommonColumns.createStatusColumn(t),
        {
            id: 'actions',
            header: '',
            className: 'w-[100px]',
            accessor: (brand) => (
                <TableActions
                    onView={() => onView(brand)}
                    onEdit={() => onEdit(brand)}
                    onDelete={() => onDelete(brand)}
                />
            )
        }
    ];

    return (
        <DataTable
            data={brands}
            columns={columns}
            isLoading={isLoading}
            searchPlaceholder={t('common.placeholders.search')}
            onAdd={onAdd}
            onRefresh={onRefresh}
            onImport={onImport}
            importTemplateUrl="/brands/template" // Assuming standard endpoint
            importTitle={t('common.importData')}
            onExportAllData={onExport}
            keyField="code"
            enableSelection
            selectedIds={selectedIds}
            onSelectionChange={onSelectionChange}
            onBulkDelete={(ids) => {
                const selectedBrands = brands.filter(b => ids.has(b.code));
                onBulkDelete(selectedBrands);
            }}

            // Server-side props
            currentPage={pagination.pageIndex}
            pageSize={pagination.pageSize}
            totalItems={pagination.totalItems}
            totalPages={pagination.totalPages}
            onPageChange={onPageChange}
            onPageSizeChange={onPageSizeChange}
        />
    );
};
