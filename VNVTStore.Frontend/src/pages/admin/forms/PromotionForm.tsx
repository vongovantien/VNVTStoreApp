import React, { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Save, X } from 'lucide-react';
import { Button, Input, Select } from '@/components/ui';
import { CreatePromotionRequest } from '@/services/promotionService';
import { useProducts } from '@/hooks';
import { PaginationDefaults } from '@/constants';

// Need a multi-select component for products. Using a simple select for now or custom?
// Since we want "Reuse", we should check if we have a multi-select or use a simple one for now.
// We will use a simple multi-select adaptation or just a comma separate input or list.
// For "Flash Sale", selecting products is key.
// Let's us a simple implementation for now to stick to existing UI components.

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
  onSubmit: (data: PromotionFormData) => Promise<void>;
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

  const { register, control, handleSubmit, watch, setFocus, setValue, formState: { errors } } = useForm<PromotionFormData>({
    resolver: zodResolver(promotionSchema),
    defaultValues: {
      code: '',
      name: '',
      description: '',
      discountType: 'PERCENTAGE',
      discountValue: 0,
      minOrderAmount: null,
      maxDiscountAmount: null,
      isActive: true,
      productCodes: [],
      ...initialData,
      startDate: initialData?.startDate ? new Date(initialData.startDate) : new Date(),
      endDate: initialData?.endDate ? new Date(initialData.endDate) : new Date(new Date().setDate(new Date().getDate() + 7)),
    }
  });

  // Use Products hook for Flash Sale product selection
  // To avoid huge load, we might need search. For now load all (paginated default) or search?
  // Let's implement a simple product selector later or just input codes for reused MVP.
  // Actually, let's load first 100 products for selection.
  const { data: productsData } = useProducts({ pageSize: 100, pageIndex: PaginationDefaults.PAGE_INDEX });
  const products = productsData?.products || [];

  const discountType = watch('discountType');

  // Focus name on mount
  useEffect(() => {
    setFocus('name');
  }, [setFocus]);

  const handleFormSubmit = async (data: any) => {
    await onSubmit({
      ...data,
      // Ensure dates are sent/handled as objects or strings? 
      // Service expects string ISO. Form uses Date objects.
      // We will conversion in the Page component or here.
      // Schema output is Date.
    });
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Basic Info */}
        <div className="space-y-4">
          <Input
            label={t('common.fields.code')}
            {...register('code')}
            error={errors.code?.message}
            placeholder={t('common.placeholders.enterCode')}
            disabled={!!initialData?.code} // Code immutable on edit
          />

          <Input
            label={t('common.fields.name')}
            {...register('name')}
            error={errors.name?.message}
            placeholder={t('common.placeholders.enterName')}
          />

          <Controller
            name="discountType"
            control={control}
            render={({ field }) => (
              <Select
                label={t('common.fields.discountType')}
                {...field}
                options={[
                  { value: 'PERCENTAGE', label: 'Percentage (%)' },
                  { value: 'AMOUNT', label: 'Fixed Amount' },
                  // { value: 'FIXED_PRICE', label: 'Fixed Price' }
                ]}
                error={errors.discountType?.message}
              />
            )}
          />

          <Input
            label={t('common.fields.value')}
            type="number"
            {...register('discountValue', { valueAsNumber: true })}
            error={errors.discountValue?.message}
            min={0}
          />
        </div>

        {/* Limits & Dates */}
        <div className="space-y-4">
          <Input
            label={t('common.fields.minOrder')}
            type="number"
            {...register('minOrderAmount', { valueAsNumber: true })}
            error={errors.minOrderAmount?.message}
            placeholder={t('common.placeholders.enterPrice')}
          />

          {discountType === 'PERCENTAGE' && (
            <Input
              label={t('common.fields.maxDiscount')}
              type="number"
              {...register('maxDiscountAmount', { valueAsNumber: true })}
              error={errors.maxDiscountAmount?.message}
              placeholder={t('common.placeholders.enterPrice')}
            />
          )}

          <div className="grid grid-cols-2 gap-4">
            <Controller
              name="startDate"
              control={control}
              render={({ field }) => (
                <Input
                  label={t('common.fields.startDate')}
                  type="date"
                  value={field.value instanceof Date ? field.value.toISOString().split('T')[0] : field.value}
                  onChange={(e) => field.onChange(new Date(e.target.value))}
                  error={errors.startDate?.message}
                />
              )}
            />
            <Controller
              name="endDate"
              control={control}
              render={({ field }) => (
                <Input
                  label={t('common.fields.endDate')}
                  type="date"
                  value={field.value instanceof Date ? field.value.toISOString().split('T')[0] : field.value}
                  onChange={(e) => field.onChange(new Date(e.target.value))}
                  error={errors.endDate?.message}
                />
              )}
            />
          </div>

          <Input
            label={t('common.fields.usageLimit')}
            type="number"
            {...register('usageLimit', { valueAsNumber: true })}
            error={errors.usageLimit?.message}
            placeholder={t('common.placeholders.enterQuantity')}
          />
        </div>
      </div>

      {/* Description */}
      <div>
        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
          {t('common.fields.description')}
        </label>
        <textarea
          {...register('description')}
          className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary dark:bg-slate-900 dark:border-gray-700"
          rows={3}
        />
      </div>

      {/* Product Selection for Flash Sales (Optional) */}
      <div className="border-t pt-4">
        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          {t('messages.selectProductsFlashSale')}
        </label>
        <div className="h-48 overflow-y-auto border rounded-md p-2 bg-gray-50 dark:bg-slate-900">
          {products.length === 0 ? (
            <p className="text-gray-500 text-sm text-center p-4">{t('messages.noProductsAvailable')}</p>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
              {products.map(p => (
                <label key={p.code} className="flex items-center gap-2 text-sm p-1 hover:bg-gray-100 dark:hover:bg-slate-800 rounded cursor-pointer">
                  <input
                    type="checkbox"
                    value={p.code}
                    {...register('productCodes')}
                    className="rounded border-gray-300"
                  />
                  <span className="truncate">{p.name} - {p.code}</span>
                </label>
              ))}
            </div>
          )}
        </div>
        <p className="text-xs text-gray-500 mt-1">{t('messages.selectProductsInfo')}</p>
      </div>

      <div className="flex justify-end gap-3 pt-4 border-t">
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          disabled={isLoading}
        >
          <X size={16} className="mr-2" />
          {t('common.cancel')}
        </Button>
        <Button
          type="submit"
          disabled={isLoading}
        >
          <Save size={16} className="mr-2" />
          {t('common.save')}
        </Button>
      </div>
    </form>
  );
};
