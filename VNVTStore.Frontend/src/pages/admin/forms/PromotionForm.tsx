import React from 'react';
import { useTranslation } from 'react-i18next';
import { z } from 'zod';
import { BaseForm, FieldGroup } from '@/components/common';
import { useProducts } from '@/hooks';
import { PaginationDefaults } from '@/constants';
import { UseFormReturn } from 'react-hook-form';
import { Product } from '@/types';

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
  message: "End date must be after start date",
  path: ["endDate"]
});

type PromotionFormData = z.infer<typeof promotionSchema>;

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
  const { data: productsData } = useProducts({ pageSize: 100, pageIndex: PaginationDefaults.PAGE_INDEX });
  const products = productsData?.products || [];

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
      title: t('admin.groups.basicInfo'),
      fields: [
        { name: 'code', type: 'text', label: t('common.fields.code'), required: true, colSpan: 6, disabled: !!initialData?.code, placeholder: t('common.placeholders.enterCode') },
        { name: 'name', type: 'text', label: t('common.fields.name'), required: true, colSpan: 6, placeholder: t('common.placeholders.enterName') },
        { 
            name: 'discountType', 
            type: 'select', 
            label: t('common.fields.discountType'), 
            colSpan: 6,
            options: [
                { value: 'PERCENTAGE', label: t('admin.promotions.percentage') },
                { value: 'AMOUNT', label: t('admin.promotions.fixedAmount') },
                { value: 'FIXED_PRICE', label: t('admin.promotions.fixedPrice') }
            ] 
        },
        { name: 'discountValue', type: 'number', label: t('common.fields.value'), required: true, colSpan: 6, placeholder: t('common.placeholders.enterValue') },
        { name: 'description', type: 'textarea', label: t('common.fields.description'), colSpan: 12, rows: 2, placeholder: t('common.placeholders.enterDescription') }
      ]
    },
    {
        title: t('admin.groups.limitsAndDates'),
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
                            className="w-full px-4 py-2 text-sm border border-border-color rounded-xl focus:outline-none focus:ring-4 focus:ring-accent-primary/5 bg-primary transition-all text-primary"
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
                            className="w-full px-4 py-2 text-sm border border-border-color rounded-xl focus:outline-none focus:ring-4 focus:ring-accent-primary/5 bg-primary transition-all text-primary"
                            value={(form.watch('endDate') as Date)?.toISOString().split('T')[0]}
                            onChange={(e) => form.setValue('endDate', new Date(e.target.value))}
                        />
                    </div>
                )
            },
            { name: 'usageLimit', type: 'number', label: t('common.fields.usageLimit'), colSpan: 6, placeholder: t('common.placeholders.enterQuantity') },
            { name: 'isActive', type: 'switch', label: t('common.fields.status'), description: t('admin.statusHint'), colSpan: 6 }
        ]
    },
    {
        title: t('admin.groups.applicableProducts'),
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
                        <div className="space-y-2">
                           <div className="h-48 overflow-y-auto border border-border-color rounded-xl p-3 bg-slate-50 dark:bg-slate-900/50">
                                {products.length === 0 ? (
                                    <p className="text-tertiary text-sm text-center p-4">{t('messages.noProductsAvailable')}</p>
                                ) : (
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
                                        {products.map((p: Product) => (
                                            <label key={p.code} className="flex items-center gap-2 text-sm p-2 hover:bg-white dark:hover:bg-slate-800 rounded-lg cursor-pointer transition-colors border border-transparent hover:border-border-color shadow-sm shadow-transparent hover:shadow-gray-100">
                                                <input
                                                    type="checkbox"
                                                    checked={selectedCodes.includes(p.code)}
                                                    onChange={() => toggleProduct(p.code)}
                                                    className="rounded border-gray-300 text-primary focus:ring-primary w-4 h-4"
                                                />
                                                <span className="truncate font-medium">{p.name}</span>
                                                <span className="text-xs text-tertiary ml-auto">{p.code}</span>
                                            </label>
                                        ))}
                                    </div>
                                )}
                            </div>
                            <p className="text-xs text-tertiary italic">{t('messages.selectProductsInfo')}</p>
                        </div>
                    );
                }
            }
        ]
    }
  ];

  return (
    <div className="w-full">
        <div className="mb-6">
            <h1 className="text-2xl font-bold text-primary">
                {initialData?.code ? t('admin.titles.promotionEdit') : t('admin.titles.promotionAdd')}
            </h1>
            <p className="text-sm text-secondary">{t('admin.subtitles.promotions')}</p>
        </div>

        <BaseForm<PromotionFormData>
            schema={promotionSchema}
            defaultValues={defaultValues}
            fieldGroups={fieldGroups}
            onSubmit={onSubmit}
            onCancel={onCancel}
            isLoading={isLoading}
            submitLabel={t('common.save')}
            className="max-w-4xl"
        />
    </div>
  );
};

export default PromotionForm;
