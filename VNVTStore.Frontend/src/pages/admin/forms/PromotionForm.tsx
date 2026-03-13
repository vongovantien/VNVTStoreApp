import React from 'react';
import { useTranslation } from 'react-i18next';
import { z } from 'zod';
import { BaseForm, FieldGroup } from '@/components/common';
import { useProducts, useCategories } from '@/hooks';

import { UseFormReturn } from 'react-hook-form';
import { Product } from '@/types';
import { CategoryDto } from '@/services/productService';

const promotionSchema = z.object({
  code: z.string().min(3),
  name: z.string().min(3),
  description: z.string().optional(),
  discountType: z.enum(['PERCENTAGE', 'AMOUNT', 'FIXED_PRICE']),
  discountValue: z.number().min(0),
  minOrderAmount: z.number().optional().nullable(),
  maxDiscountAmount: z.number().optional().nullable(),
  startDate: z.date(),
  endDate: z.date(),
  usageLimit: z.number().optional().nullable(),
  isActive: z.boolean(),
  productCodes: z.array(z.string()).optional(),
}).refine(data => data.endDate > data.startDate, {
  message: "validation.endDateAfterStartDate",
  path: ["endDate"]
});

export type PromotionFormData = z.infer<typeof promotionSchema>;

interface PromotionFormProps {
  initialData?: Partial<PromotionFormData>;
  onSubmit: (data: PromotionFormData) => void | Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export const PromotionForm: React.FC<PromotionFormProps> = ({
  initialData,
  onSubmit,
  onCancel,
  isLoading
}) => {
  const { t } = useTranslation();
  
  // State for product filtering
  const [selectedCategory, setSelectedCategory] = React.useState<string>('all');
  const [productSearch, setProductSearch] = React.useState<string>('');

  // Fetch data
  const { data: productsData } = useProducts({ 
    pageSize: 1000, 
    pageIndex: 1,
    ...(selectedCategory !== 'all' ? { category: selectedCategory } : {}),
    ...(productSearch ? { search: productSearch } : {})
  });
  const products = productsData?.products || [];

  const { data: categoriesData } = useCategories({ enabled: true });
  const categories = categoriesData || [];

  const defaultValues: PromotionFormData = React.useMemo(() => ({
    code: initialData?.code || '',
    name: initialData?.name || '',
    description: initialData?.description || '',
    discountType: initialData?.discountType || 'PERCENTAGE',
    discountValue: initialData?.discountValue || 0,
    minOrderAmount: initialData?.minOrderAmount || null,
    maxDiscountAmount: initialData?.maxDiscountAmount || null,
    isActive: initialData?.isActive ?? true,
    productCodes: initialData?.productCodes || [],
    startDate: initialData?.startDate ? new Date(initialData.startDate) : new Date(),
    endDate: initialData?.endDate ? new Date(initialData.endDate) : new Date(new Date().setDate(new Date().getDate() + 7)),
  }), [initialData]);

  const fieldGroups: FieldGroup[] = [
    {
      title: t('admin.promotionForm.basicInfo'),
      fields: [
        { name: 'code', type: 'text', label: t('common.fields.code'), required: true, colSpan: 6, disabled: !!initialData?.code, placeholder: t('common.placeholders.enterCode') },
        { name: 'name', type: 'text', label: t('common.fields.name'), required: true, colSpan: 6, placeholder: t('common.placeholders.enterName') },
        { 
            name: 'discountType', 
            type: 'select', 
            label: t('common.fields.discountType'), 
            colSpan: 6,
            options: [
                { value: 'PERCENTAGE', label: t('admin.promotionForm.percentage') },
                { value: 'AMOUNT', label: t('admin.promotionForm.fixedAmount') },
                { value: 'FIXED_PRICE', label: t('admin.promotionForm.fixedPrice') }
            ] 
        },
        { name: 'discountValue', type: 'number', label: t('common.fields.value'), required: true, colSpan: 6, placeholder: t('common.placeholders.enterValue') },
        { name: 'description', type: 'textarea', label: t('common.fields.description'), colSpan: 12, rows: 2, placeholder: t('common.placeholders.enterDescription') }
      ]
    },
    {
        title: t('admin.promotionForm.limitsAndDates'),
        fields: [
            { name: 'minOrderAmount', type: 'number', label: t('common.fields.minOrder'), colSpan: 6, placeholder: t('common.placeholders.enterPrice') },
            { name: 'maxDiscountAmount', type: 'number', label: t('common.fields.maxDiscount'), colSpan: 6, placeholder: t('common.placeholders.enterPrice') },
            { 
                name: 'startDate', 
                type: 'custom', 
                label: t('common.fields.startDate'), 
                colSpan: 6,
                render: (form: UseFormReturn<PromotionFormData>) => (
                    <div className="flex flex-col gap-1 w-full">
                        <label className="text-sm font-bold text-primary">{t('common.fields.startDate')}</label>
                        <input 
                            type="date" 
                            className="w-full px-4 py-2 text-sm border border-border rounded-xl focus:outline-none focus:ring-2 focus:ring-accent/20 bg-primary transition-all text-primary"
                            value={(form.watch('startDate') as Date)?.toISOString().split('T')[0]}
                            onChange={(e) => form.setValue('startDate', new Date(e.target.value))}
                        />
                    </div>
                )
            },
            { 
                name: 'endDate', 
                type: 'custom', 
                label: t('common.fields.endDate'), 
                colSpan: 6,
                render: (form: UseFormReturn<PromotionFormData>) => (
                    <div className="flex flex-col gap-1 w-full">
                        <label className="text-sm font-bold text-primary">{t('common.fields.endDate')}</label>
                        <input 
                            type="date" 
                            className="w-full px-4 py-2 text-sm border border-border rounded-xl focus:outline-none focus:ring-2 focus:ring-accent/20 bg-primary transition-all text-primary"
                            value={(form.watch('endDate') as Date)?.toISOString().split('T')[0]}
                            onChange={(e) => form.setValue('endDate', new Date(e.target.value))}
                        />
                    </div>
                )
            },
            { name: 'usageLimit', type: 'number', label: t('common.fields.usageLimit'), colSpan: 6, placeholder: t('common.placeholders.enterQuantity') },
            { name: 'isActive', type: 'switch', label: t('common.fields.status'), description: t('admin.promotionForm.statusHint'), colSpan: 6 }
        ]
    },
    {
        title: t('admin.promotionForm.applicableProducts'),
        fields: [
            {
                name: 'productCodes',
                type: 'custom',
                label: t('messages.selectProductsFlashSale'),
                colSpan: 12,
                render: (form: UseFormReturn<PromotionFormData>) => {
                    const selectedCodes = form.watch('productCodes') || [];
                    const toggleProduct = (code: string) => {
                        const next = selectedCodes.includes(code)
                            ? selectedCodes.filter((c: string) => c !== code)
                            : [...selectedCodes, code];
                        form.setValue('productCodes', next);
                    };

                    return (
                        <div className="space-y-4">
                            {/* Filtering row */}
                            <div className="flex flex-wrap gap-3 p-3 bg-bg-secondary rounded-xl border border-border">
                                <div className="flex-1 min-w-[200px]">
                                    <input 
                                        type="text"
                                        placeholder={t('common.placeholders.search')}
                                        className="w-full px-3 py-1.5 text-xs border border-border rounded-lg focus:outline-none focus:ring-2 focus:ring-accent/20"
                                        value={productSearch}
                                        onChange={(e) => setProductSearch(e.target.value)}
                                    />
                                </div>
                                <select 
                                    className="px-3 py-1.5 text-xs border border-border rounded-lg focus:outline-none focus:ring-2 focus:ring-accent/20 bg-primary min-w-[150px]"
                                    value={selectedCategory}
                                    onChange={(e) => setSelectedCategory(e.target.value)}
                                >
                                    <option value="all">{t('common.allCategories')}</option>
                                    {categories.map((cat: CategoryDto) => (
                                        <option key={cat.code} value={cat.code}>{cat.name}</option>
                                    ))}
                                </select>
                                <div className="ml-auto text-xs font-medium text-text-secondary flex items-center bg-bg-primary px-3 rounded-lg border border-border">
                                    {t('common.selected')}: <span className="text-accent ml-1">{selectedCodes.length}</span>
                                </div>
                            </div>

                            <div className="h-64 overflow-y-auto border border-border rounded-xl p-3 bg-bg-secondary custom-scrollbar">
                                {products.length === 0 ? (
                                    <p className="text-text-tertiary text-sm text-center p-4">{t('messages.noProductsAvailable')}</p>
                                ) : (
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2">
                                        {products.map((p: Product) => (
                                            <label key={p.code} className="flex items-center gap-2 text-sm p-2 hover:bg-bg-primary rounded-lg cursor-pointer transition-colors border border-transparent hover:border-border shadow-sm group">
                                                <input
                                                    type="checkbox"
                                                    checked={selectedCodes.includes(p.code)}
                                                    onChange={() => toggleProduct(p.code)}
                                                    className="rounded border-border text-accent focus:ring-accent w-4 h-4"
                                                />
                                                <div className="flex flex-col min-w-0 flex-1">
                                                    <span className="truncate font-semibold group-hover:text-accent transition-colors">{p.name}</span>
                                                    <span className="text-[10px] text-text-tertiary">{p.code}</span>
                                                </div>
                                            </label>
                                        ))}
                                    </div>
                                )}
                            </div>
                            <p className="text-xs text-text-tertiary italic">{t('messages.selectProductsInfo')}</p>
                        </div>
                    );
                }
            }
        ]
    }
  ];

  return (
    <div className="w-full">
        <BaseForm<PromotionFormData>
            schema={promotionSchema}
            defaultValues={defaultValues}
            fieldGroups={fieldGroups}
            onSubmit={onSubmit}
            onCancel={onCancel}
            isLoading={isLoading ?? false}
            submitLabel={t('common.save')}
            className="max-w-4xl"
            layout="tabs"
        />
    </div>
  );
};

export default PromotionForm;
