
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { BaseForm, FieldDefinition } from '@/components/common';

// ============ Schema ============
const catalogUnitSchema = z.object({
  name: z.string().min(1, 'validation.required'),
  isActive: z.boolean(),
});

export type CatalogUnitFormData = z.infer<typeof catalogUnitSchema>;

// ============ Props ============
interface CatalogUnitFormProps {
  initialData?: Partial<CatalogUnitFormData>;
  onSubmit: (data: CatalogUnitFormData) => void | Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
  isModal?: boolean;
  modalOpen?: boolean;
  modalTitle?: string;
  onModalClose?: () => void;
}

// ============ Component ============
export const CatalogUnitForm = ({
  initialData,
  onSubmit,
  onCancel,
  isLoading = false,
  isModal = true,
  modalOpen = false,
  modalTitle,
  onModalClose
}: CatalogUnitFormProps) => {
  const { t } = useTranslation();

  // Field definitions
  const fields: FieldDefinition[] = [
    {
      name: 'name',
      type: 'text',
      label: t('common.fields.unitName'),
      placeholder: t('common.placeholders.exampleUnit'),
      required: true,
      colSpan: 12,
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
  const defaultValues: CatalogUnitFormData = {
    name: initialData?.name || '',
    isActive: initialData?.isActive ?? true,
  };

  return (
    <BaseForm<CatalogUnitFormData>
      schema={catalogUnitSchema}
      defaultValues={defaultValues}
      fields={fields}
      onSubmit={onSubmit}
      onCancel={onCancel}
      isLoading={isLoading}
      submitLabel={initialData?.name ? t('common.update') : t('common.create')}
      isModal={isModal}
      modalOpen={modalOpen}
      modalTitle={modalTitle || (initialData?.name ? t('admin.actions.edit') : t('admin.actions.create'))}
      onModalClose={onModalClose || onCancel}
    />
  );
};

export default CatalogUnitForm;
