import { useTranslation } from 'react-i18next';
import { z } from 'zod';
import { Upload, X, Edit, Trash2 } from 'lucide-react';
import { Button, Input, Badge, ConfirmDialog, Modal, Checkbox } from '@/components/ui';
import { useState, useMemo, useEffect } from 'react';
import { createPortal } from 'react-dom';
import { getImageUrl } from '@/utils/format';
import { useToast } from '@/store';
import { BaseForm, FieldGroup } from '@/components/common';
import { useDropzone } from 'react-dropzone';
import { UseFormReturn, Controller } from 'react-hook-form';
import LazySelect, { LazySelectOption } from '@/components/ui/LazySelect';
import { categoryService, supplierService, unitService, brandService } from '@/services';
import { SearchCondition } from '@/services/baseService';
import { UnitForm, UnitFormData } from './UnitForm';
import { DataTable, DataTableColumn } from '@/components/common';
import { useQuery } from '@tanstack/react-query';

// Local interface for Product Unit (different from Catalog Unit)
export interface ProductUnitDto {
    code?: string;
    unitName: string;
    conversionRate: number;
    price: number;
    isActive: boolean;
    isBaseUnit?: boolean;
    productCode?: string;
}

const productSchema = z.object({
  name: z.string().min(3),
  categoryCode: z.string().min(1),
  price: z.number().min(0),
  wholesalePrice: z.number().min(0).optional(),
  costPrice: z.number().min(0).optional(),
  stockQuantity: z.number().int().min(0),
  description: z.string().optional(),
  brandCode: z.string().optional(),
  baseUnit: z.string().optional(),
  minStockLevel: z.number().optional(),
  binLocation: z.string().optional(),
  vatRate: z.number().optional(),
  countryOfOrigin: z.string().optional(),
  supplierCode: z.string().optional(),
  images: z.array(z.string()).optional(),
  isActive: z.boolean().optional(),
  weight: z.number().optional(),
  color: z.string().optional(),
  power: z.string().optional(),
  voltage: z.string().optional(),
  material: z.string().optional(),
  size: z.string().optional(),
  details: z.array(z.object({
    detailType: z.enum(['SPEC', 'LOGISTICS', 'RELATION', 'IMAGE']),
    specName: z.string().min(1),
    specValue: z.string().min(1)
  })).optional(),
  code: z.string().optional(),
  unitsSection: z.any().optional(),
  productUnits: z.array(z.any()).optional(),
});

export type ProductFormData = z.infer<typeof productSchema>;

interface ProductDetail {
  detailType: 'SPEC' | 'LOGISTICS' | 'RELATION' | 'IMAGE';
  specName: string;
  specValue: string;
}

interface ProductFormProps {
  initialData?: Partial<ProductFormData> & { 
      productImages?: Array<{ imageUrl: string }>; 
      originalPrice?: number;
      categoryName?: string;
      supplierName?: string;
      brandName?: string;
  };
  onSubmit: (data: ProductFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
}

const ProductUnitsManager = ({ 
    baseUnitName, 
    baseUnitPrice,
    onLocalUnitsChange, 
    initialLocalUnits = [] 
}: { 
    baseUnitName?: string;
    baseUnitPrice?: number;
    onLocalUnitsChange?: (units: ProductUnitDto[]) => void;
    initialLocalUnits?: ProductUnitDto[];
}) => {
    const { t } = useTranslation();
    const [isFormOpen, setIsFormOpen] = useState(false);
    const [editingUnit, setEditingUnit] = useState<ProductUnitDto | null>(null);
    const [unitToDelete, setUnitToDelete] = useState<ProductUnitDto | null>(null);
    const [localUnits, setLocalUnits] = useState<ProductUnitDto[]>(initialLocalUnits);

    // Sync local state if initialLocalUnits changes (e.g. on reset or re-mount)
    useEffect(() => {
        if (initialLocalUnits.length > 0 && localUnits.length === 0) {
            setLocalUnits(initialLocalUnits);
        }
    }, [initialLocalUnits]);

    const allUnits = useMemo(() => {
        const baseRow: ProductUnitDto = {
            code: 'BASE_ROW',
            productCode: '',
            unitName: baseUnitName || 'Cái',
            conversionRate: 1,
            price: baseUnitPrice || 0,
            isBaseUnit: true,
            isActive: true
        };
        // Ensure localUnits doesn't contain the base row duplicated (filter by isBaseUnit or name+rate)
        const filteredLocal = localUnits.filter(u => !u.isBaseUnit && (u.conversionRate !== 1 || u.unitName !== baseUnitName));
        return [baseRow, ...filteredLocal];
    }, [baseUnitName, baseUnitPrice, localUnits]);

    const handleSave = async (data: UnitFormData) => {
        let newUnits: ProductUnitDto[];
        if (editingUnit) {
            newUnits = localUnits.map(u => u.code === editingUnit.code ? { ...u, ...data, unitName: data.unitName!, conversionRate: data.conversionRate, price: data.price, isActive: data.isActive } as ProductUnitDto : u);
        } else {
            const newUnit: ProductUnitDto = {
                code: `temp-${Date.now()}`,
                productCode: '',
                unitName: data.unitName,
                conversionRate: data.conversionRate,
                price: data.price,
                isActive: data.isActive,
                isBaseUnit: false
            };
            newUnits = [...localUnits, newUnit];
        }
        setLocalUnits(newUnits);
        onLocalUnitsChange?.(newUnits);
        console.log("Units updated locally:", newUnits); // Debug log to confirm no API call
        setIsFormOpen(false);
        setEditingUnit(null);
    };

    const handleDelete = () => {
        if (unitToDelete) {
            const newUnits = localUnits.filter(u => u.code !== unitToDelete.code);
            setLocalUnits(newUnits);
            onLocalUnitsChange?.(newUnits);
            setUnitToDelete(null);
        }
    };

    const columns: DataTableColumn<ProductUnitDto>[] = [
        {
            id: 'select',
            header: <Checkbox checked={true} disabled />,
            accessor: (u) => <Checkbox checked={true} disabled={u.code === 'BASE_ROW'} />,
            className: 'w-12'
        },
        { 
            id: 'name', 
            header: t('common.fields.unitName'), 
            accessor: (u) => (
                <div className="flex items-center gap-2">
                    <span className="font-medium text-slate-700 dark:text-slate-200">{u.unitName}</span>
                    {u.code === 'BASE_ROW' && (
                        <Badge size="sm" variant="outline" color="primary" className="text-[10px] uppercase font-bold py-0">{t('product.defaultUnit')}</Badge>
                    )}
                </div>
            ) 
        },
        { 
            id: 'rate', 
            header: t('common.fields.conversionRate'), 
            accessor: (u) => u.code === 'BASE_ROW' ? (
                <span className="text-xs text-secondary italic">{t('product.baseUnitLabel')}</span>
            ) : (
                <div className="flex items-center gap-2 text-sm text-slate-600 dark:text-slate-400">
                    <span>1 {u.unitName} = {u.conversionRate} {baseUnitName}</span>
                </div>
            )
        },
        { 
            id: 'price', 
            header: t('common.fields.price'), 
            accessor: (u) => (
                <span className="font-bold text-rose-600">
                    {new Intl.NumberFormat('vi-VN').format(u.price)} <span className="text-xs font-normal underline">đ</span>
                </span>
            ), 
            className: 'text-right' 
        },
        { 
            id: 'actions', 
            header: '', 
            accessor: (u) => u.code !== 'BASE_ROW' ? (
                <div className="flex justify-end gap-2">
                    <button type="button" onClick={() => { setEditingUnit(u); setIsFormOpen(true); }} className="p-1.5 text-slate-400 hover:text-blue-500 rounded-lg hover:bg-blue-50 transition-all"><Edit size={16} /></button>
                    <button type="button" onClick={() => setUnitToDelete(u)} className="p-1.5 text-slate-400 hover:text-red-500 rounded-lg hover:bg-red-50 transition-all"><Trash2 size={16} /></button>
                </div>
            ) : null,
            className: 'w-24'
        }
    ];

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center bg-slate-50 dark:bg-slate-900/50 p-4 rounded-xl border border-border-color border-dashed">
                <div className="space-y-1">
                    <p className="text-sm font-semibold text-primary">{t('admin.groups.units')}</p>
                    <p className="text-xs text-tertiary">Thiết lập các đơn vị quy đổi từ đơn vị gốc <strong>{baseUnitName}</strong></p>
                </div>
                <Button type="button" size="sm" variant="outline" onClick={() => { setEditingUnit(null); setIsFormOpen(true); }}>
                     + {t('admin.actions.addUnit')}
                </Button>
            </div>
            
            <div className="border border-border-color rounded-xl overflow-hidden shadow-sm bg-white dark:bg-slate-950">
                <DataTable 
                    data={allUnits} 
                    columns={columns} 
                    showToolbar={false} 
                    keyField="code" 
                    pageSize={100}
                />
            </div>

            {isFormOpen && (
                <UnitForm
                   initialData={editingUnit ? { ...editingUnit, productCode: 'CURRENT' } : { productCode: 'CURRENT', unitName: '', conversionRate: 1, price: baseUnitPrice || 0, isActive: true }}
                   onSubmit={handleSave}
                   onCancel={(e) => { e?.stopPropagation(); setIsFormOpen(false); setEditingUnit(null); }}
                   modalOpen={isFormOpen}
                   modalTitle={editingUnit ? t('admin.actions.edit') : t('admin.actions.create')}
                   isModal={true}
                   fixedProduct={true}
                />
            )}
             <ConfirmDialog
                isOpen={!!unitToDelete}
                onClose={() => setUnitToDelete(null)}
                onConfirm={handleDelete}
                title={t('admin.actions.delete')}
                message={t('messages.confirmDelete', { name: unitToDelete?.unitName })}
                confirmText={t('admin.actions.delete')}
                variant="danger"
            />
        </div>
    );
};

export const ProductForm = ({ initialData, onSubmit, onCancel, isLoading }: ProductFormProps) => {
  const { t } = useTranslation();
  const { error: toastError } = useToast();

  const [localUnits, setLocalUnits] = useState<ProductUnitDto[]>(initialData?.productUnits as ProductUnitDto[] || []);

  useEffect(() => {
    if (initialData?.productUnits) {
        setLocalUnits(initialData.productUnits as ProductUnitDto[]);
    }
  }, [initialData]);

  const handleSubmit = async (data: ProductFormData) => {
    try {
        const baseRow: ProductUnitDto = {
            unitName: data.baseUnit || 'Cái',
            conversionRate: 1,
            price: data.price,
            isBaseUnit: true,
            isActive: true
        };
        
        const conversionUnits = localUnits.filter(u => !u.isBaseUnit && (u.unitName !== data.baseUnit || u.conversionRate !== 1));
        
        const payload = { 
            ...data, 
            productUnits: [baseRow, ...conversionUnits].map(u => ({
                unitName: u.unitName,
                conversionRate: u.conversionRate,
                price: u.price,
                isBaseUnit: u.isBaseUnit,
                isActive: u.isActive
            }))
        };
        
        await onSubmit(payload as any);
    } catch (err) {
        console.error("Failed to save product", err);
    }
  };

  // Schema is now defined at the top to be stable and used for type inference

  const defaultValues: ProductFormData = useMemo(() => ({
    name: initialData?.name || '',
    price: initialData?.price || 0,
    wholesalePrice: initialData?.wholesalePrice || 0,
    description: initialData?.description || '',
    brandCode: initialData?.brandCode || '',
    baseUnit: initialData?.baseUnit || 'Cái',
    isActive: initialData?.isActive ?? true,
    details: initialData?.details || [],
    categoryCode: initialData?.categoryCode || '',
    stockQuantity: initialData?.stockQuantity || 0,
    costPrice: initialData?.costPrice ?? initialData?.originalPrice ?? 0,
    images: initialData?.images?.map(img => getImageUrl(img)) || initialData?.productImages?.map(img => getImageUrl(img.imageUrl)) || [],
    code: initialData?.code || '',
    vatRate: initialData?.vatRate || 0,
    minStockLevel: initialData?.minStockLevel || 0,
    binLocation: initialData?.binLocation || '',
    countryOfOrigin: initialData?.countryOfOrigin || '',
    supplierCode: initialData?.supplierCode || '',
  }), [initialData]);

  const [isUploading, setIsUploading] = useState(false);
  const [previewImage, setPreviewImage] = useState<string | null>(null);
  const specSuggestions = ["Công suất", "Điện áp", "Lưu lượng", "Phi (Ø)", "Đường kính", "Độ dày", "Chất liệu", "Màu sắc"];

  // Fetch functions for LazySelect
  const fetchCategories = async (params: { pageIndex: number; pageSize: number; search?: string }) => {
      const res = await categoryService.search({
          pageIndex: params.pageIndex,
          pageSize: params.pageSize,
          search: params.search,
          searchField: 'Name',
          sortBy: 'Code',
          filters: [{ field: 'IsActive', value: true, operator: SearchCondition.Equal }],
          fields: ['Code', 'Name']
      });
      return {
          items: (res.data?.items || []).map(x => ({ value: x.code, label: x.name })),
          totalItems: res.data?.totalItems || 0
      };
  };

  const fetchSuppliers = async (params: { pageIndex: number; pageSize: number; search?: string }) => {
      const res = await supplierService.search({
          pageIndex: params.pageIndex,
          pageSize: params.pageSize,
          search: params.search,
          searchField: 'Name',
          sortBy: 'Code',
          filters: [{ field: 'IsActive', value: true, operator: SearchCondition.Equal }],
          fields: ['Code', 'Name']
      });
      return {
          items: (res.data?.items || []).map(x => ({ value: x.code, label: x.name })),
          totalItems: res.data?.totalItems || 0
      };
  };

  const fetchUnits = async (params: { pageIndex: number; pageSize: number; search?: string }) => {
    const res = await unitService.search({
        pageIndex: params.pageIndex,
        pageSize: params.pageSize,
        search: params.search,
        searchField: 'Name',
        sortBy: 'Code',
        filters: [{ field: 'IsActive', value: 'true', operator: SearchCondition.Equal }],
        fields: ['Code', 'Name']
    });
    return {
        // unitService returns UnitDto which now has 'name'
        items: (res.data?.items || []).map(x => ({ value: x.name, label: x.name })),
        totalItems: res.data?.totalItems || 0
    };
};

const fetchBrands = async (params: { pageIndex: number; pageSize: number; search?: string }) => {
    const res = await brandService.search({
        pageIndex: params.pageIndex,
        pageSize: params.pageSize,
        search: params.search,
        searchField: 'Name',
        sortBy: 'Code',
        filters: [{ field: 'IsActive', value: 'true', operator: SearchCondition.Equal }],
        fields: ['Code', 'Name']
    });
    return {
        items: (res.data?.items || []).map(x => ({ value: x.code, label: x.name })),
        totalItems: res.data?.totalItems || 0
    };
};

  const fieldGroups: FieldGroup[] = [
    {
      title: t('admin.groups.general'),
      fields: [
        { name: 'name', type: 'text', label: t('common.fields.name'), required: true, colSpan: 12, placeholder: t('common.placeholders.productName') },
        { name: 'description', type: 'textarea', label: t('common.fields.description'), colSpan: 12, placeholder: t('common.placeholders.productDescription') }
      ]
    },
    {
        title: t('admin.groups.images'),
        fields: [
            { 
                name: 'images', 
                type: 'custom', 
                label: t('common.fields.image'), 
                colSpan: 12,
                render: (form: UseFormReturn<ProductFormData>) => {
                    const images = form.watch('images') || [];
                    const onDrop = async (acceptedFiles: File[]) => {
                        if (acceptedFiles.length > 0) {
                            setIsUploading(true);
                            try {
                                const promises = acceptedFiles.map((file) => {
                                    return new Promise<string>((resolve, reject) => {
                                        const reader = new FileReader();
                                        reader.onload = () => resolve(reader.result as string);
                                        reader.onerror = reject;
                                        reader.readAsDataURL(file);
                                    });
                                });
                                const results = await Promise.all(promises);
                                const uniqueResults = results.filter(newImg => !images.includes(newImg));
                                form.setValue('images', [...images, ...uniqueResults], { shouldValidate: true });
                            } catch (err) {
                                toastError(t('common.messages.errorOccurred'));
                            } finally {
                                setIsUploading(false);
                            }
                        }
                    };

                    const { getRootProps, getInputProps, isDragActive } = useDropzone({ onDrop, accept: { 'image/*': [] } });

                    return (
                        <div className="space-y-4">
                            <div {...getRootProps()} className={`border-2 border-dashed rounded-xl p-8 flex flex-col items-center justify-center cursor-pointer transition-all bg-slate-50 dark:bg-slate-900/50 ${isDragActive ? 'border-accent-primary bg-accent-primary/5' : 'border-border-color hover:border-accent-primary/50'}`}>
                                <input {...getInputProps()} />
                                <div className="w-12 h-12 bg-accent-primary/5 text-accent-primary rounded-full flex items-center justify-center mb-3">
                                    <Upload size={20} />
                                </div>
                                <p className="text-sm font-medium text-primary">{isUploading ? t('common.loading') : t('common.dragOrClick')}</p>
                                <p className="text-xs text-tertiary mt-1">Hỗ trợ JPG, PNG, WEBP (Max 5MB)</p>
                            </div>
                            {images.length > 0 && (
                                <div className="grid grid-cols-4 sm:grid-cols-6 gap-4">
                                    {images.map((img: string, idx: number) => (
                                        <div key={idx} className="relative aspect-square rounded-lg border border-border-color overflow-hidden group bg-white cursor-pointer" onClick={() => setPreviewImage(img)}>
                                            <img src={img} className="w-full h-full object-cover" />
                                            <button 
                                                type="button" 
                                                onClick={(e) => { e.stopPropagation(); form.setValue('images', images.filter((_, i) => i !== idx)); }}
                                                className="absolute inset-0 bg-black/40 text-white opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center backdrop-blur-sm"
                                            >
                                                <X size={16} />
                                            </button>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    );
                }
            }
        ]
    },
    {
        title: t('admin.groups.pricing'),
        fields: [
            { name: 'price', type: 'number', label: t('common.fields.price'), required: true, colSpan: 3, placeholder: t('common.placeholders.enterPrice') },
            { name: 'wholesalePrice', type: 'number', label: t('common.fields.wholesalePrice'), colSpan: 3, placeholder: t('common.placeholders.enterPrice') },
            { name: 'costPrice', type: 'number', label: t('common.fields.costPrice'), colSpan: 3, placeholder: t('common.placeholders.enterPrice') },
            { name: 'vatRate', type: 'number', label: 'VAT (%)', colSpan: 3, placeholder: '0' }
        ]
    },
    {
        title: t('admin.groups.inventory'),
        fields: [
            { name: 'stockQuantity', type: 'number', label: t('common.fields.stock'), required: true, colSpan: 6, placeholder: t('common.placeholders.enterQuantity') },
            { name: 'code', type: 'text', label: t('common.fields.code'), disabled: true, colSpan: 6, placeholder: t('common.placeholders.enterCode') },
            { name: 'minStockLevel', type: 'number', label: t('common.fields.minStock'), colSpan: 4, placeholder: '0' },
            { 
                name: 'baseUnit', 
                type: 'custom', 
                label: t('common.fields.unit'), 
                colSpan: 4,
                render: (form: UseFormReturn<ProductFormData>) => (
                    <Controller
                        control={form.control}
                        name="baseUnit"
                        render={({ field, fieldState }) => (
                            <LazySelect
                                {...field}
                                value={field.value || ''}
                                onChange={field.onChange}
                                label={t('common.fields.unit')}
                                error={fieldState.error?.message}
                                queryKeyPrefix="units"
                                fetchFn={fetchUnits}
                                placeholder={t('common.placeholders.select')}
                                initialLabel={field.value} // For unit, value is the name
                            />
                        )}
                    />
                )
            },
            { name: 'binLocation', type: 'text', label: t('common.fields.location'), colSpan: 4, placeholder: 'Kệ A...' }
        ]
    },
    {
        title: t('admin.groups.specs'),
        fields: [
            {
                name: 'details',
                type: 'custom',
                label: t('common.fields.specs'),
                colSpan: 12,
                render: (form: UseFormReturn<ProductFormData>) => {
                    const details = form.watch('details') || [];
                    const addDetail = (type: 'SPEC' | 'LOGISTICS' | 'RELATION' | 'IMAGE') => {
                        form.setValue('details', [...details, { detailType: type, specName: '', specValue: '' }]);
                    };
                    const updateDetail = (idx: number, field: keyof ProductDetail, val: string) => {
                        const newDetails = [...details];
                        newDetails[idx] = { ...newDetails[idx], [field]: val } as ProductDetail;
                        form.setValue('details', newDetails);
                    };

                    return (
                        <div className="space-y-4">
                            <div className="flex gap-2 mb-2">
                                <Button type="button" size="sm" variant="outline" onClick={() => addDetail('SPEC')}>+ {t('common.fields.specs')}</Button>
                                <Button type="button" size="sm" variant="outline" onClick={() => addDetail('LOGISTICS')}>+ Logistics</Button>
                            </div>
                            <div className="space-y-3 bg-slate-50 dark:bg-slate-900/50 p-4 rounded-xl border border-border-color">
                                {details.length === 0 ? (
                                    <p className="text-center text-sm text-tertiary py-4">{t('common.noData')}</p>
                                ) : (
                                    details.map((detail, idx: number) => (
                                        <div key={idx} className="flex items-center gap-3">
                                            <Badge color={detail.detailType === 'SPEC' ? 'primary' : detail.detailType === 'LOGISTICS' ? 'success' : 'warning'} className="w-20 justify-center">
                                                {detail.detailType === 'SPEC' ? 'SPEC' : detail.detailType === 'LOGISTICS' ? 'LOGIS' : 'TAG'}
                                            </Badge>
                                            <Input className="flex-1 h-9 text-sm" placeholder={t('common.fields.name')} value={detail.specName} onChange={(e) => updateDetail(idx, 'specName', e.target.value)} list={`spec-suggestions-${idx}`} />
                                            <datalist id={`spec-suggestions-${idx}`}>
                                                {specSuggestions.map(s => <option key={s} value={s} />)}
                                            </datalist>
                                            <Input className="flex-1 h-9 text-sm" placeholder={t('common.fields.value')} value={detail.specValue} onChange={(e) => updateDetail(idx, 'specValue', e.target.value)} />
                                            <button type="button" onClick={() => form.setValue('details', details.filter((_, i) => i !== idx))} className="text-slate-400 hover:text-red-500">
                                                <X size={18} />
                                            </button>
                                        </div>
                                    ))
                                )}
                            </div>
                        </div>
                    );
                }
            }
        ]
    },
    {
        title: t('admin.groups.units'),
        fields: [
            {
                name: 'unitsSection',
                type: 'custom',
                label: '',
                colSpan: 12,
                render: (form: UseFormReturn<ProductFormData>) => (
                    <ProductUnitsManager 
                        baseUnitName={form.watch('baseUnit')}
                        baseUnitPrice={form.watch('price')}
                        initialLocalUnits={localUnits}
                        onLocalUnitsChange={setLocalUnits}
                    />
                )
            }
        ]
    },
  ];

  const fieldGroupsSidebar: FieldGroup[] = [
    {
        title: t('admin.groups.organization'),
        fields: [
            { name: 'isActive', type: 'switch', label: t('common.fields.status'), description: t('admin.statusHint'), colSpan: 12 },
            { 
                name: 'categoryCode', 
                type: 'custom', 
                label: t('common.fields.category'), 
                required: true, 
                colSpan: 12,
                render: (form: UseFormReturn<ProductFormData>) => (
                    <Controller
                        control={form.control}
                        name="categoryCode"
                        render={({ field, fieldState }) => (
                            <LazySelect
                                {...field}
                                value={field.value || ''}
                                onChange={field.onChange}
                                label={t('common.fields.category')}
                                error={fieldState.error?.message}
                                required
                                queryKeyPrefix="categories"
                                fetchFn={fetchCategories}
                                placeholder={t('common.placeholders.select')}
                                initialLabel={initialData?.categoryName} // We pass categoryName if available
                            />
                        )}
                    />
                )
            },
            { 
                name: 'brandCode', 
                type: 'custom', 
                label: t('common.fields.brand'), 
                colSpan: 12, 
                render: (form: UseFormReturn<ProductFormData>) => (
                    <Controller
                        control={form.control}
                        name="brandCode"
                        render={({ field, fieldState }) => (
                            <LazySelect
                                {...field}
                                value={field.value || ''}
                                onChange={field.onChange}
                                label={t('common.fields.brand')}
                                error={fieldState.error?.message}
                                queryKeyPrefix="brands"
                                fetchFn={fetchBrands}
                                placeholder={t('common.placeholders.select')}
                                initialLabel={initialData?.brandName}
                            />
                        )}
                    />
                )
            },
            { 
                name: 'supplierCode', 
                type: 'custom', 
                label: t('common.fields.supplier'), 
                colSpan: 12, 
                render: (form: UseFormReturn<ProductFormData>) => (
                    <Controller
                        control={form.control}
                        name="supplierCode"
                        render={({ field, fieldState }) => (
                            <LazySelect
                                {...field}
                                value={field.value || ''}
                                onChange={field.onChange}
                                label={t('common.fields.supplier')}
                                error={fieldState.error?.message}
                                queryKeyPrefix="suppliers"
                                fetchFn={fetchSuppliers}
                                placeholder={t('common.placeholders.select')}
                                initialLabel={initialData?.supplierName}
                            />
                        )}
                    />
                )
            },
            { name: 'countryOfOrigin', type: 'text', label: t('common.fields.origin'), colSpan: 12, placeholder: 'VD: Vietnam' }
        ]
    }
  ];

  return (
    <div className="w-full">
        <BaseForm<ProductFormData>
            schema={productSchema}
            defaultValues={defaultValues}
            fieldGroups={[...fieldGroups, ...fieldGroupsSidebar]}
            onSubmit={handleSubmit}
            onCancel={onCancel}
            isLoading={isLoading}
            layout="tabs"
            submitLabel={t('common.save')}
        />
        
        {previewImage && createPortal(
            <div className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/90 backdrop-blur-sm p-4 animate-in fade-in duration-200" onClick={() => setPreviewImage(null)}>
                <div className="relative max-w-full max-h-full flex flex-col items-center justify-center">
                    <img 
                        src={previewImage} 
                        alt="Full size preview" 
                        className="max-w-full max-h-[90vh] object-contain rounded-lg shadow-2xl" 
                        onClick={(e) => e.stopPropagation()} 
                    />
                    <button
                        type="button"
                        onClick={() => setPreviewImage(null)}
                        className="absolute top-4 right-4 p-2 bg-white/10 text-white rounded-full hover:bg-white/20 transition-colors backdrop-blur-md"
                    >
                        <X size={24} />
                    </button>
                </div>
            </div>,
            document.body
        )}
    </div>
  );
};

export default ProductForm;
