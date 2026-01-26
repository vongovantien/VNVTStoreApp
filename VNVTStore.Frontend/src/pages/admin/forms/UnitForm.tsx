
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { BaseForm, FieldDefinition } from '@/components/common';
import { productService } from '@/services';
import { LazySelect } from '@/components/ui';
import { Controller } from 'react-hook-form';

// ============ Schema ============
const unitSchema = z.object({
  productCode: z.string().optional(),
  unitName: z.string().min(1),
  conversionRate: z.number().min(0),
  price: z.number().min(0),
  isActive: z.boolean(),
});

export type UnitFormData = z.infer<typeof unitSchema>;

// ============ Props ============
interface UnitFormProps {
  initialData?: Partial<UnitFormData>;
  onSubmit: (data: UnitFormData) => void | Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
  isModal?: boolean;
  modalOpen?: boolean;
  modalTitle?: string;
  fixedProduct?: boolean;
}

// ============ Component ============
export const UnitForm = ({
  initialData,
  onSubmit,
  onCancel,
  isLoading = false,
  isModal = true,
  modalOpen = false,
  modalTitle,
  fixedProduct = false,
}: UnitFormProps) => {
  const { t } = useTranslation();

  // Field definitions
  const fields: FieldDefinition[] = [
    {
      name: 'productCode',
      type: 'custom',
      label: t('common.fields.product'),
      required: true,
      colSpan: 12,
      disabled: fixedProduct,
      render: (form) => (
          fixedProduct ? <div className="hidden"></div> : (
         <div className="col-span-12">
            <label className="text-sm font-semibold text-primary mb-1 block">
               {t('common.fields.product')}
               <span className="text-red-500 ml-1">*</span>
            </label>
            <Controller
                control={form.control}
                name="productCode"
                render={({ field }) => (
                    <LazySelect
                        queryKeyPrefix="products"
                        value={field.value}
                        onChange={field.onChange}
                        disabled={fixedProduct}
                        fetchFn={async (params) => {
                            const res = await productService.search(params);
                            return {
                                items: (res.data?.items || []).map(p => ({
                                    value: p.code,
                                    label: p.name,
                                    subLabel: p.categoryName || p.code
                                })),
                                totalItems: res.data?.totalItems || 0
                            };
                        }}
                        placeholder={t('common.placeholders.selectProduct')}
                        error={form.formState.errors.productCode?.message as string}
                    />
                )}
            />
         </div>
          )
      )
    },
    {
      name: 'unitName',
      type: 'custom',
      label: t('common.fields.unitName'),
      required: true,
      colSpan: 12,
      render: (form) => (
         <div className="col-span-12">
            <label className="text-sm font-semibold text-primary mb-1 block">
               {t('common.fields.unitName')}
               <span className="text-red-500 ml-1">*</span>
            </label>
            <Controller
                control={form.control}
                name="unitName"
                render={({ field }) => (
                    <LazySelect
                        queryKeyPrefix="units-catalog"
                        value={field.value}
                        onChange={field.onChange}
                        fetchFn={async (params) => {
                            // Import unitService if not available or assume it's available in scope
                            // We need to ensure unitService is imported at top of file
                            const { unitService } = await import('@/services');
                            const { SearchCondition } = await import('@/services/baseService');

                            const res = await unitService.search({
                                ...params,
                                searchField: 'Name',
                                filters: [{ field: 'IsActive', value: true, operator: SearchCondition.Equal }]
                            });
                            return {
                                items: (res.data?.items || []).map(u => ({
                                    value: u.name, // Use Name as value since ProductUnit stores Name
                                    label: u.name
                                })),
                                totalItems: res.data?.totalItems || 0
                            };
                        }}
                        placeholder={t('common.placeholders.select')}
                        error={form.formState.errors.unitName?.message as string}
                        initialLabel={field.value}
                    />
                )}
            />
         </div>
      )
    },
    {
      name: 'conversionRate',
      type: 'number',
      label: t('common.fields.conversionRate'),
      required: true,
      colSpan: 6,
      min: 0,
    },
    {
      name: 'price',
      type: 'number',
      label: t('common.fields.price'),
      required: true,
      colSpan: 6,
      min: 0,
    },
    {
      name: 'isActive',
      type: 'switch',
      label: t('common.fields.status'),
      description: t('admin.statusHint'),
      colSpan: 12,
    },
  ];

  // Default values
  const defaultValues: UnitFormData = {
    productCode: initialData?.productCode || '',
    unitName: initialData?.unitName || '',
    conversionRate: initialData?.conversionRate || 1,
    price: initialData?.price || 0,
    isActive: initialData?.isActive ?? true,
  };

  return (
    <BaseForm<UnitFormData>
      schema={unitSchema}
      defaultValues={defaultValues}
      fields={fields}
      onSubmit={onSubmit}
      onCancel={onCancel}
      isLoading={isLoading}
      submitLabel={initialData?.unitName ? t('common.update') : t('common.create')}
      isModal={isModal}
      modalOpen={modalOpen}
      modalTitle={modalTitle || (initialData?.unitName ? t('admin.actions.edit') : t('admin.actions.create'))}
      onModalClose={onCancel}
    />
  );
};

export default UnitForm;
