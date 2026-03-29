import { useState, useMemo, useEffect } from 'react';
import { createPortal } from 'react-dom';
import { useTranslation } from 'react-i18next';
import { z } from 'zod';
import { Upload, X } from 'lucide-react';
import { Button, Input, Badge } from '@/components/ui';
import { getImageUrl } from '@/utils/format';
import { BaseForm, FieldGroup } from '@/components/common';
import { useDropzone } from 'react-dropzone';
import { Controller, UseFormReturn } from 'react-hook-form';
import LazySelect from '@/components/ui/LazySelect';
import { categoryService, supplierService, unitService, brandService } from '@/services';
import { SearchCondition } from '@/services/baseService';
import { ProductUnitsManager, ProductUnitDto } from '@/components/common/ProductUnitsManager';
import { ProductVariantManager, ProductVariantData } from '@/components/common/ProductVariantManager';
import { ProductImage } from '@/types';

const productSchema = z.object({
  name: z.string().min(3, { message: 'validation.productNameMin' }),
  categoryCode: z.string().min(1, { message: 'validation.categoryRequired' }),
  price: z.number().min(0, { message: 'validation.priceMin' }),
  wholesalePrice: z.number().min(0, { message: 'validation.priceMin' }).optional(),
  costPrice: z.number().min(0, { message: 'validation.priceMin' }).optional(),
  stockQuantity: z.number().int({ message: 'validation.integer' }).min(0, { message: 'validation.stockMin' }),
  description: z.string().optional(),
  brandCode: z.string().optional(),
  baseUnit: z.string().optional(),
  minStockLevel: z.number().optional(),
  binLocation: z.string().optional(),
  vatRate: z.number().optional(),
  countryOfOrigin: z.string().optional(),
  supplierCode: z.string().optional(),
  weight: z.number().optional(),
  color: z.string().optional(),
  power: z.string().optional(),
  voltage: z.string().optional(),
  material: z.string().optional(),
  size: z.string().optional(),
  images: z.array(z.string()).optional(),
  isActive: z.boolean().optional(),
  details: z.array(z.object({
    detailType: z.enum(['SPEC', 'LOGISTICS', 'RELATION', 'IMAGE']),
    specName: z.string().min(1, { message: 'validation.required' }),
    specValue: z.string().min(1, { message: 'validation.required' })
  })).optional(),
  code: z.string().optional(),
  unitsSection: z.unknown().optional(),
  productUnits: z.array(z.object({
    code: z.string().optional(),
    productCode: z.string().optional(),
    unitName: z.string(),
    conversionRate: z.number(),
    price: z.number(),
    isBaseUnit: z.boolean(),
    isActive: z.boolean().optional()
  })).optional(),
  variants: z.array(z.object({
    code: z.string().optional(),
    productCode: z.string().optional(),
    sku: z.string(),
    attributes: z.union([z.string(), z.record(z.unknown())]), // Attributes can be JSON string or object
    price: z.number(),
    stockQuantity: z.number(),
    isActive: z.boolean().optional()
  })).optional(),
});

export type ProductFormData = z.infer<typeof productSchema>;

interface ProductDetail {
  detailType: 'SPEC' | 'LOGISTICS' | 'RELATION' | 'IMAGE';
  specName: string;
  specValue: string;
}

export interface ProductInitialData extends Partial<ProductFormData> {
  productImages?: ProductImage[];
  originalPrice?: number;
  categoryName?: string;
  supplierName?: string;
  brandName?: string;
  productUnits?: ProductUnitDto[];
  variants?: ProductVariantData[];
  imageURL?: string;
}

interface ProductFormProps {
  initialData?: ProductInitialData;
  onSubmit: (data: ProductFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
}



const ImageUploadField = ({ 
    form, 
    t, 
    setIsUploading, 
    isUploading, 
    setPreviewImage 
}: { 
    form: UseFormReturn<ProductFormData>; 
    t: (key: string) => string; 
    setIsUploading: (val: boolean) => void; 
    isUploading: boolean; 
    setPreviewImage: (val: string | null) => void; 
}) => {
    const images = form.watch('images') || [];

    const onDrop = async (acceptedFiles: File[]) => {
        if (acceptedFiles.length > 0) {
            setIsUploading(true);
            try {
                const promises = acceptedFiles.map((file) => new Promise<string>((resolve, reject) => {
                    const reader = new FileReader();
                    reader.onload = () => resolve(reader.result as string);
                    reader.onerror = reject;
                    reader.readAsDataURL(file);
                }));
                const results = await Promise.all(promises);
                const uniqueResults = results.filter(newImg => !images.includes(newImg));
                form.setValue('images', [...images, ...uniqueResults], { shouldValidate: true });
            } catch { 
                // Error handled
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
                <div className="w-12 h-12 bg-accent-primary/5 text-accent-primary rounded-full flex items-center justify-center mb-3"><Upload size={20} /></div>
                <p className="text-sm font-medium text-primary">{isUploading ? t('common.loading') : t('common.dragOrClick')}</p>
            </div>
            {images.length > 0 && (
                <div className="grid grid-cols-4 sm:grid-cols-6 gap-4">
                    {images.map((img: string, idx: number) => (
                        <div key={idx} className="relative aspect-square rounded-lg border border-border-color overflow-hidden group bg-white cursor-pointer" onClick={() => setPreviewImage(img)}>
                            <img src={img} className="w-full h-full object-cover" />
                            <button type="button" onClick={(e) => { e.stopPropagation(); form.setValue('images', images.filter((_, i) => i !== idx)); }} className="absolute inset-0 bg-black/40 text-white opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center backdrop-blur-sm"><X size={16} /></button>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export const ProductForm = ({ initialData, onSubmit, onCancel, isLoading }: ProductFormProps) => {
  const { t } = useTranslation();
  

  const [localUnits, setLocalUnits] = useState<ProductUnitDto[]>((initialData?.productUnits as ProductUnitDto[]) || []);
  const [localVariants, setLocalVariants] = useState<ProductVariantData[]>(initialData?.variants || []);

  useEffect(() => {
    if (initialData?.productUnits) {
        // eslint-disable-next-line
        setLocalUnits(initialData.productUnits as ProductUnitDto[]);
    }
    if (initialData?.variants) {
         
        setLocalVariants(initialData.variants);
    }
  }, [initialData]);

  const handleSubmit = async (data: ProductFormData) => {
    try {
        // Strip API base URL from existing images to send relative paths (fixes update issue)
        const apiBase = import.meta.env.VITE_API_URL || 'http://localhost:5176/api/v1';
        const root = apiBase.replace(/\/api\/v1\/?$/, '');
        
        const processedImages = data.images?.map(img => {
            if (img.startsWith('data:')) return img; // Keep new base64 uploads
            if (root && img.startsWith(root)) {
                 return img.substring(root.length); // Remove http://domain to get /uploads/...
            }
            return img;
        });

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
            images: processedImages,
            productUnits: [baseRow, ...conversionUnits].map(u => ({
                unitName: u.unitName,
                conversionRate: u.conversionRate,
                price: u.price,
                isBaseUnit: u.isBaseUnit,
                isActive: u.isActive
            })),
            variants: localVariants.map(v => ({
                sku: v.sku,
                attributes: typeof v.attributes === 'string' ? v.attributes : JSON.stringify(v.attributes),
                price: v.price,
                stockQuantity: v.stockQuantity
            }))
        };
        await onSubmit(payload as ProductFormData);
    } catch (err) {
        console.error("Failed to save product", err);
    }
  };

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
    images: (() => {
        const directImages = initialData?.images?.map(img => getImageUrl(img));
        if (directImages && directImages.length > 0) return directImages;
        
        const prodImages = initialData?.productImages?.map(img => getImageUrl(img.imageURL || (img as { imageURL?: string; imageUrl?: string }).imageUrl));
        if (prodImages && prodImages.length > 0) return prodImages;
        
        if (initialData?.imageURL) return [getImageUrl(initialData.imageURL)];
        
        return [];
    })(),
    code: initialData?.code || '',
    vatRate: initialData?.vatRate || 0,
    minStockLevel: initialData?.minStockLevel || 0,
    binLocation: initialData?.binLocation || '',
    countryOfOrigin: initialData?.countryOfOrigin || '',
    supplierCode: initialData?.supplierCode || '',
    variants: initialData?.variants || [],
  }), [initialData]);

  const [isUploading, setIsUploading] = useState(false);
  const [previewImage, setPreviewImage] = useState<string | null>(null);
  const specSuggestions = ["Công suất", "Điện áp", "Lưu lượng", "Phi (Ø)", "Đường kính", "Độ dày", "Chất liệu", "Màu sắc"];

  const fetchCategories = async (params: { pageIndex: number; pageSize: number; search?: string }) => {
      const res = await categoryService.search({
          pageIndex: params.pageIndex, pageSize: params.pageSize, search: params.search,
          searchField: 'Name', sortBy: 'Code',
          filters: [{ field: 'IsActive', value: true, operator: SearchCondition.Equal }],
          fields: ['Code', 'Name']
      });
      return { items: (res.data?.items || []).map(x => ({ value: x.code, label: x.name })), totalItems: res.data?.totalItems || 0 };
  };

  const fetchSuppliers = async (params: { pageIndex: number; pageSize: number; search?: string }) => {
      const res = await supplierService.search({
          pageIndex: params.pageIndex, pageSize: params.pageSize, search: params.search,
          searchField: 'Name', sortBy: 'Code',
          filters: [{ field: 'IsActive', value: true, operator: SearchCondition.Equal }],
          fields: ['Code', 'Name']
      });
      return { items: (res.data?.items || []).map(x => ({ value: x.code, label: x.name })), totalItems: res.data?.totalItems || 0 };
  };

  const fetchUnits = async (params: { pageIndex: number; pageSize: number; search?: string }) => {
    const res = await unitService.search({
        pageIndex: params.pageIndex, pageSize: params.pageSize, search: params.search,
        searchField: 'Name', sortBy: 'Code',
        filters: [{ field: 'IsActive', value: true, operator: SearchCondition.Equal }],
        fields: ['Code', 'Name']
    });
    return { items: (res.data?.items || []).map(x => ({ value: x.name, label: x.name })), totalItems: res.data?.totalItems || 0 };
  };

  const fetchBrands = async (params: { pageIndex: number; pageSize: number; search?: string }) => {
    const res = await brandService.search({
        pageIndex: params.pageIndex, pageSize: params.pageSize, search: params.search,
        searchField: 'Name', sortBy: 'Code',
        filters: [{ field: 'IsActive', value: true, operator: SearchCondition.Equal }],
        fields: ['Code', 'Name']
    });
    return { items: (res.data?.items || []).map(x => ({ value: x.code, label: x.name })), totalItems: res.data?.totalItems || 0 };
  };

  const fieldGroups: FieldGroup[] = [
    {
      title: t('admin.groups.general', 'General'),
      fields: [
        { name: 'name', type: 'text', label: t('common.fields.name', 'Name'), required: true, colSpan: 12, placeholder: t('common.placeholders.productName', 'Product Name') },
        { name: 'description', type: 'textarea', label: t('common.fields.description', 'Description'), colSpan: 12, placeholder: t('common.placeholders.productDescription', 'Product Description') }
      ]
    },
    {
        title: t('admin.groups.images', 'Images'),
        fields: [
            { 
                name: 'images', type: 'custom', label: t('common.fields.image'), colSpan: 12,
                render: (f: unknown) => {
                    const form = f as UseFormReturn<ProductFormData>;
                    return (
                        <ImageUploadField 
                            form={form} 
                            t={t} 
                            isUploading={isUploading} 
                            setIsUploading={setIsUploading} 
                            setPreviewImage={setPreviewImage} 
                        />
                    );
                }
            }
        ]
    },
    {
        title: t('admin.groups.pricing', 'Pricing'),
        fields: [
            {
                name: 'pricingSection', type: 'custom', label: '', colSpan: 12,
                render: (f: unknown) => {
                    const form = f as UseFormReturn<ProductFormData>;
                    const price = form.watch('price') || 0;
                    const wholesalePrice = form.watch('wholesalePrice') || 0;
                    const costPrice = form.watch('costPrice') || 0;
                    const vatRate = form.watch('vatRate') || 0;
                    
                    // Calculate profit
                    const profit = price - costPrice;
                    const profitMargin = price > 0 ? ((profit / price) * 100).toFixed(1) : '0';
                    
                    // Format number with thousand separators
                    const formatNumber = (num: number) => num.toLocaleString('vi-VN');
                    const parseNumber = (str: string) => {
                        const cleaned = str.replace(/[^\d]/g, '');
                        return cleaned ? parseInt(cleaned, 10) : 0;
                    };
                    
                    // Validation warnings
                    const warnings: string[] = [];
                    if (wholesalePrice > 0 && wholesalePrice >= price) {
                        warnings.push('⚠️ Giá bán sỉ phải nhỏ hơn Giá lẻ');
                    }
                    if (costPrice > 0 && costPrice >= price) {
                        warnings.push('⚠️ Giá vốn phải nhỏ hơn Giá bán');
                    }
                    
                    // Auto-calculate price from cost + VAT when cost or VAT changes
                    const handleCostChange = (newCost: number) => {
                        form.setValue('costPrice', newCost, { shouldValidate: true });
                        if (newCost > 0) {
                            const calculatedPrice = Math.round(newCost * (1 + vatRate / 100));
                            form.setValue('price', calculatedPrice, { shouldValidate: true });
                        }
                    };
                    
                    const handleVatChange = (newVat: number) => {
                        form.setValue('vatRate', newVat, { shouldValidate: true });
                        if (costPrice > 0) {
                            const calculatedPrice = Math.round(costPrice * (1 + newVat / 100));
                            form.setValue('price', calculatedPrice, { shouldValidate: true });
                        }
                    };
                    
                    return (
                        <div className="space-y-4">
                            <div className="grid grid-cols-12 gap-4">
                                {/* Giá vốn (INPUT - primary) */}
                                {/* Giá vốn (INPUT - primary) */}
                                <div className="col-span-3">
                                    <Input
                                        label={t('common.fields.costPrice', 'Cost Price')}
                                        isRequired
                                        value={formatNumber(costPrice)}
                                        onChange={(e) => handleCostChange(parseNumber(e.target.value))}
                                        placeholder={t('common.placeholders.enterPrice', 'Enter price...')}
                                        className="text-right"
                                    />
                                </div>
                                
                                {/* Giá bán sỉ (INPUT) */}
                                {/* Giá bán sỉ (INPUT) */}
                                <div className="col-span-3">
                                    <Input
                                        label={t('common.fields.wholesalePrice', 'Wholesale Price')}
                                        value={formatNumber(wholesalePrice)}
                                        onChange={(e) => form.setValue('wholesalePrice', parseNumber(e.target.value), { shouldValidate: true })}
                                        placeholder={t('common.placeholders.enterPrice', 'Enter price...')}
                                        className={`text-right ${wholesalePrice >= price && wholesalePrice > 0 ? 'border-yellow-500' : ''}`}
                                    />
                                </div>
                                
                                {/* VAT (INPUT) */}
                                {/* VAT (INPUT) */}
                                <div className="col-span-2">
                                    <Input
                                        label={t('common.fields.vatPercent', 'VAT (%)')}
                                        type="number"
                                        value={vatRate}
                                        onChange={(e) => handleVatChange(parseFloat(e.target.value) || 0)}
                                        placeholder="0"
                                        className="text-right"
                                    />
                                </div>
                                
                                {/* Giá bán lẻ (CALCULATED - can be overridden) */}
                                <div className="col-span-4">
                                    <Input
                                        id="product-price-input"
                                        data-testid="product-price-input"
                                        label={`${t('common.fields.price', 'Price')} bán`}
                                        value={formatNumber(price)}
                                        onChange={(e) => form.setValue('price', parseNumber(e.target.value), { shouldValidate: true })}
                                        placeholder={t('common.placeholders.enterPrice', 'Enter price...')}
                                        className="text-right font-semibold bg-green-50 dark:bg-green-900/20 border-green-300"
                                    />
                                </div>
                            </div>
                            
                            {/* Profit display */}
                            <div className="flex items-center gap-4 p-3 rounded-lg bg-slate-50 dark:bg-slate-900/50 border border-border-color">
                                <div className="flex-1">
                                    <span className="text-sm text-secondary">Lợi nhuận gộp:</span>
                                    <span className={`ml-2 font-semibold ${profit >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                                        {formatNumber(profit)} đ
                                    </span>
                                    <span className="ml-2 text-sm text-tertiary">({profitMargin}%)</span>
                                </div>
                            </div>
                            
                            {/* Validation warnings */}
                            {warnings.length > 0 && (
                                <div className="text-sm text-yellow-600 dark:text-yellow-400 space-y-1">
                                    {warnings.map((w, i) => <div key={i}>{w}</div>)}
                                </div>
                            )}
                        </div>
                    );
                }
            }
        ]
    },
    {
        title: t('admin.groups.inventory', 'Inventory'),
        fields: [
            { name: 'stockQuantity', type: 'number', label: t('common.fields.stock', 'Stock'), required: true, colSpan: 6, placeholder: t('common.placeholders.enterQuantity', 'Enter quantity...') },
            { name: 'code', type: 'text', label: t('common.fields.code', 'Code'), disabled: true, colSpan: 6, placeholder: t('common.placeholders.enterCode', 'Enter code...') },
            { name: 'minStockLevel', type: 'number', label: t('common.fields.minStock'), colSpan: 4, placeholder: '0' },
            { 
                name: 'baseUnit', type: 'custom', label: t('common.fields.unit'), colSpan: 4,
                render: (f: unknown) => {
                    const form = f as UseFormReturn<ProductFormData>;
                    return (
                        <Controller control={form.control} name="baseUnit" render={({ field, fieldState }) => (
                            <LazySelect {...field} value={field.value || ''} onChange={field.onChange} label={t('common.fields.unit', 'Unit')} error={fieldState.error?.message} queryKeyPrefix="units" fetchFn={fetchUnits} placeholder={t('common.placeholders.select', 'Select...')} initialLabel={field.value} />
                        )} />
                    );
                }
            },
            { name: 'binLocation', type: 'text', label: t('common.fields.location'), colSpan: 4, placeholder: t('common.placeholders.enterLocation', 'Kệ A...') }
        ]
    },
    {
        title: t('admin.groups.specs', 'Specifications'),
        fields: [
            {
                name: 'details', type: 'custom', label: t('common.fields.specs'), colSpan: 12,
                render: (f: unknown) => {
                    const form = f as UseFormReturn<ProductFormData>;
                    const details = form.watch('details') || [];
                    const addDetail = (type: 'SPEC' | 'LOGISTICS' | 'RELATION' | 'IMAGE') => form.setValue('details', [...details, { detailType: type, specName: '', specValue: '' }]);
                    const updateDetail = (idx: number, f: keyof ProductDetail, val: string) => {
                        const newDetails = [...details];
                        newDetails[idx] = { ...newDetails[idx], [f]: val } as ProductDetail;
                        form.setValue('details', newDetails);
                    };
                    return (
                        <div className="space-y-4">
                            <div className="flex gap-2 mb-2">
                                <Button type="button" size="sm" variant="outline" onClick={() => addDetail('SPEC')}>+ {t('common.fields.specs')}</Button>
                                <Button type="button" size="sm" variant="outline" onClick={() => addDetail('LOGISTICS')}>+ Logistics</Button>
                            </div>
                            <div className="space-y-3 bg-slate-50 dark:bg-slate-900/50 p-4 rounded-xl border border-border-color">
                                {details.length === 0 ? <p className="text-center text-sm text-tertiary py-4">{t('common.noData')}</p> : details.map((detail, idx: number) => {
                                    const dType = detail.detailType || 'SPEC';
                                    const badgeColor = dType === 'SPEC' ? 'primary' : dType === 'LOGISTICS' ? 'success' : 'warning';
                                    const badgeLabel = dType === 'SPEC' ? t('common.types.spec', 'SPEC') : dType === 'LOGISTICS' ? t('common.types.logistics', 'LOGIS') : t('common.types.tag', 'TAG');
                                    return (
                                    <div key={idx} className="flex items-center gap-3">
                                        <Badge color={badgeColor} className="w-20 justify-center">{badgeLabel}</Badge>
                                        <Input className="flex-1 h-9 text-sm" placeholder={t('common.fields.name')} value={detail.specName} onChange={(e) => updateDetail(idx, 'specName', e.target.value)} list={`spec-suggestions-${idx}`} /><datalist id={`spec-suggestions-${idx}`}>{specSuggestions.map(s => <option key={s} value={s} />)}</datalist>
                                        <Input className="flex-1 h-9 text-sm" placeholder={t('common.fields.value')} value={detail.specValue} onChange={(e) => updateDetail(idx, 'specValue', e.target.value)} />
                                        <button type="button" onClick={() => form.setValue('details', details.filter((_, i) => i !== idx))} className="text-slate-400 hover:text-red-500"><X size={18} /></button>
                                    </div>
                                    );
                                })}
                            </div>
                        </div>
                    );
                }
            }
        ]
    },
    {
        title: t('admin.groups.units', 'Units'),
        fields: [
            {
                name: 'unitsSection', type: 'custom', label: '', colSpan: 12,
                render: (f: unknown) => {
                    const form = f as UseFormReturn<ProductFormData>;
                    return (
                        <ProductUnitsManager 
                            baseUnitName={form.watch('baseUnit')} 
                            baseUnitPrice={form.watch('price')} 
                            units={localUnits}
                            onChange={(newUnits) => setLocalUnits(newUnits)}
                            fetchUnitOptions={fetchUnits}
                        />
                    );
                }
            }
        ]
    },
    {
        title: t('product.variants', 'Variants'),
        fields: [
            {
                name: 'variants', type: 'custom', label: '', colSpan: 12,
                render: (f: unknown) => {
                    const form = f as UseFormReturn<ProductFormData>;
                    return (
                        <ProductVariantManager 
                            basePrice={form.watch('price')}
                            productCode={form.watch('code')}
                            initialVariants={localVariants}
                            onChange={(newVariants) => setLocalVariants(newVariants)}
                        />
                    );
                }
            }
        ]
    }
  ];

  const fieldGroupsSidebar: FieldGroup[] = [
    {
        title: t('admin.groups.organization', 'Organization'),
        fields: [
            { name: 'isActive', type: 'switch', label: t('common.fields.status', 'Status'), description: t('admin.statusHint', 'Status hint'), colSpan: 12 },
            { 
                name: 'categoryCode', type: 'custom', label: t('common.fields.category'), required: true, colSpan: 12,
                render: (f: unknown) => {
                    const form = f as UseFormReturn<ProductFormData>;
                    return (
                        <Controller control={form.control} name="categoryCode" render={({ field, fieldState }) => (
                            <LazySelect {...field} value={field.value || ''} onChange={field.onChange} label={t('common.fields.category', 'Category')} error={fieldState.error?.message} required queryKeyPrefix="categories" fetchFn={fetchCategories} placeholder={t('common.placeholders.select', 'Select...')} initialLabel={initialData?.categoryName} />
                        )} />
                    );
                }
            },
            { 
                name: 'brandCode', type: 'custom', label: t('common.fields.brand'), colSpan: 12, 
                render: (f: unknown) => {
                    const form = f as UseFormReturn<ProductFormData>;
                    return (
                        <Controller control={form.control} name="brandCode" render={({ field, fieldState }) => (
                            <LazySelect {...field} value={field.value || ''} onChange={field.onChange} label={t('common.fields.brand', 'Brand')} error={fieldState.error?.message} queryKeyPrefix="brands" fetchFn={fetchBrands} placeholder={t('common.placeholders.select', 'Select...')} initialLabel={initialData?.brandName} />
                        )} />
                    );
                }
            },
            { 
                name: 'supplierCode', type: 'custom', label: t('common.fields.supplier'), colSpan: 12, 
                render: (f: unknown) => {
                    const form = f as UseFormReturn<ProductFormData>;
                    return (
                        <Controller control={form.control} name="supplierCode" render={({ field, fieldState }) => (
                            <LazySelect {...field} value={field.value || ''} onChange={field.onChange} label={t('common.fields.supplier', 'Supplier')} error={fieldState.error?.message} queryKeyPrefix="suppliers" fetchFn={fetchSuppliers} placeholder={t('common.placeholders.select', 'Select...')} initialLabel={initialData?.supplierName} />
                        )} />
                    );
                }
            },
            { name: 'countryOfOrigin', type: 'text', label: t('common.fields.origin'), colSpan: 12, placeholder: t('common.placeholders.enterOrigin', 'VD: Vietnam') }
        ]
    }
  ];

  return (
    <div className="w-full">
        <BaseForm<ProductFormData> schema={productSchema} defaultValues={defaultValues} fieldGroups={[...fieldGroups, ...fieldGroupsSidebar]} onSubmit={handleSubmit} onCancel={onCancel} isLoading={isLoading} layout="tabs" submitLabel={t('common.save')} />
        
        {previewImage && createPortal(
            <div className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/90 backdrop-blur-sm p-4 animate-in fade-in duration-200" onClick={() => setPreviewImage(null)}>
                <div className="relative max-w-full max-h-full flex flex-col items-center justify-center">
                    <img src={previewImage} alt="Full size preview" className="max-w-full max-h-[90vh] object-contain rounded-lg shadow-2xl" onClick={(e) => e.stopPropagation()} />
                    <button type="button" onClick={() => setPreviewImage(null)} className="absolute top-4 right-4 p-2 bg-white/10 text-white rounded-full hover:bg-white/20 transition-colors backdrop-blur-md"><X size={24} /></button>
                </div>
            </div>, document.body
        )}
    </div>
  );
};


export default ProductForm;
