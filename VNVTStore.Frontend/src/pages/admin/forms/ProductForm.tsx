import { useState, useMemo } from 'react';
import { createPortal } from 'react-dom';
import { useTranslation } from 'react-i18next';
import { z } from 'zod';
import { X } from 'lucide-react';
import { BaseForm, FieldGroup } from '@/components/common';
import { Controller, UseFormReturn } from 'react-hook-form';
import LazySelect from '@/components/ui/LazySelect';
import { ProductUnitsManager, ProductUnitDto } from '@/components/common/ProductUnitsManager';
import { ProductVariantManager, ProductVariantData } from '@/components/common/ProductVariantManager';
import { ProductImage } from '@/types';
import { getApiRoot } from '@/utils/config';
import { getImageUrl } from '@/utils/format';

// Extracted Components
import { ImageUploadField } from './ImageUploadField';
import { ProductPricingSection } from './ProductPricingSection';
import { ProductSpecsSection } from './ProductSpecsSection';
import { useProductFormOptions } from './useProductFormOptions';

export const productSchema = z.object({
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
  productUnits: z.array(z.unknown()).optional(),
  variants: z.array(z.unknown()).optional(),
});

export type ProductFormData = z.infer<typeof productSchema>;

interface ProductFormProps {
  initialData?: (Partial<ProductFormData> & { 
      productImages?: ProductImage[] | undefined; 
      originalPrice?: number | undefined;
      categoryName?: string | undefined;
      supplierName?: string | undefined;
      brandName?: string | undefined;
      productUnits?: ProductUnitDto[] | undefined;
      variants?: ProductVariantData[] | undefined;
      imageURL?: string | undefined;
  }) | undefined;
  onSubmit: (data: ProductFormData) => void;
  onCancel: () => void;
  isLoading?: boolean | undefined;
}

export const ProductForm = ({ initialData, onSubmit, onCancel, isLoading }: ProductFormProps) => {
  const { t } = useTranslation();
  const { fetchCategories, fetchSuppliers, fetchUnits, fetchBrands } = useProductFormOptions(t);

  const [localUnits, setLocalUnits] = useState<ProductUnitDto[]>((initialData?.productUnits as ProductUnitDto[]) || []);
  const [localVariants, setLocalVariants] = useState<ProductVariantData[]>((initialData?.variants as ProductVariantData[]) || []);
  const [isUploading, setIsUploading] = useState(false);
  const [previewImage, setPreviewImage] = useState<string | null>(null);

  const handleSubmit = async (data: ProductFormData) => {
    try {
        const root = getApiRoot();
        const processedImages = data.images?.map(img => {
            if (img.startsWith('data:')) return img;
            if (root && img.startsWith(root)) return img.substring(root.length);
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
            productUnits: [baseRow, ...conversionUnits],
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
        const prodImages = initialData?.productImages?.map(img => getImageUrl(img.imageURL || (img as any).imageUrl));
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
                render: (f: unknown) => (
                    <ImageUploadField 
                        form={f as UseFormReturn<ProductFormData>} 
                        t={t} 
                        isUploading={isUploading} 
                        setIsUploading={setIsUploading} 
                        setPreviewImage={setPreviewImage} 
                    />
                )
            }
        ]
    },
    {
        title: t('admin.groups.pricing', 'Pricing'),
        fields: [
            {
                name: 'pricingSection', type: 'custom', label: '', colSpan: 12,
                render: (f: unknown) => <ProductPricingSection form={f as UseFormReturn<ProductFormData>} t={t} />
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
                render: (f: unknown) => (
                    <Controller control={(f as UseFormReturn<ProductFormData>).control} name="baseUnit" render={({ field, fieldState }) => (
                        <LazySelect {...field} value={field.value || ''} onChange={field.onChange} label={t('common.fields.unit', 'Unit')} error={fieldState.error?.message} queryKeyPrefix="units" fetchFn={fetchUnits} placeholder={t('common.placeholders.select', 'Select...')} initialLabel={field.value} />
                    )} />
                )
            },
            { name: 'binLocation', type: 'text', label: t('common.fields.location'), colSpan: 4, placeholder: t('common.placeholders.enterLocation', 'Kệ A...') }
        ]
    },
    {
        title: t('admin.groups.specs', 'Specifications'),
        fields: [
            {
                name: 'details', type: 'custom', label: t('common.fields.specs'), colSpan: 12,
                render: (f: unknown) => <ProductSpecsSection form={f as UseFormReturn<ProductFormData>} t={t} />
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
                            onChange={setLocalUnits}
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
                            onChange={setLocalVariants}
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
                render: (f: unknown) => (
                    <Controller control={(f as UseFormReturn<ProductFormData>).control} name="categoryCode" render={({ field, fieldState }) => (
                        <LazySelect {...field} value={field.value || ''} onChange={field.onChange} label={t('common.fields.category', 'Category')} error={fieldState.error?.message} required queryKeyPrefix="categories" fetchFn={fetchCategories} placeholder={t('common.placeholders.select', 'Select...')} initialLabel={initialData?.categoryName} />
                    )} />
                )
            },
            { 
                name: 'brandCode', type: 'custom', label: t('common.fields.brand'), colSpan: 12, 
                render: (f: unknown) => (
                    <Controller control={(f as UseFormReturn<ProductFormData>).control} name="brandCode" render={({ field, fieldState }) => (
                        <LazySelect {...field} value={field.value || ''} onChange={field.onChange} label={t('common.fields.brand', 'Brand')} error={fieldState.error?.message} queryKeyPrefix="brands" fetchFn={fetchBrands} placeholder={t('common.placeholders.select', 'Select...')} initialLabel={initialData?.brandName} />
                    )} />
                )
            },
            { 
                name: 'supplierCode', type: 'custom', label: t('common.fields.supplier'), colSpan: 12, 
                render: (f: unknown) => (
                    <Controller control={(f as UseFormReturn<ProductFormData>).control} name="supplierCode" render={({ field, fieldState }) => (
                        <LazySelect {...field} value={field.value || ''} onChange={field.onChange} label={t('common.fields.supplier', 'Supplier')} error={fieldState.error?.message} queryKeyPrefix="suppliers" fetchFn={fetchSuppliers} placeholder={t('common.placeholders.select', 'Select...')} initialLabel={initialData?.supplierName} />
                    )} />
                )
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
