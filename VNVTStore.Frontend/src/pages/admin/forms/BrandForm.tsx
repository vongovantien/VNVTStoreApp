import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { BaseForm, FieldDefinition } from '@/components/common';

// ============ Schema ============
const brandSchema = z.object({
  name: z.string().min(1, 'validation.required'),
  description: z.string().optional(),
  logoUrl: z.string().optional(),
  isActive: z.boolean(),
});

export type BrandFormData = z.infer<typeof brandSchema>;

// ============ Props ============
interface BrandFormProps {
  initialData?: Partial<BrandFormData>;
  onSubmit: (data: BrandFormData) => void | Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
  isModal?: boolean;
  modalOpen?: boolean;
  modalTitle?: string;
  imageBaseUrl?: string;
}

// ============ Component ============
export const BrandForm = ({
  initialData,
  onSubmit,
  onCancel,
  isLoading = false,
  isModal = true,
  modalOpen = false,
  modalTitle,
  imageBaseUrl,
}: BrandFormProps) => {
  const { t } = useTranslation();

  // Field definitions
  const fields: FieldDefinition[] = [
    {
      name: 'logoUrl',
      type: 'image',
      label: t('common.fields.image'),
      colSpan: 12,
    },
    {
      name: 'name',
      type: 'text',
      label: t('common.fields.name'),
      placeholder: t('common.placeholders.enterName'),
      required: true,
      colSpan: 12,
    },
    {
      name: 'description',
      type: 'textarea',
      label: t('common.fields.description'),
      placeholder: t('common.placeholders.enterDescription'),
      rows: 3,
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
  const defaultValues: BrandFormData = {
    name: initialData?.name || '',
    description: initialData?.description || '',
    logoUrl: initialData?.logoUrl || '',
    isActive: initialData?.isActive ?? true,
  };

  return (
    <BaseForm<BrandFormData>
      schema={brandSchema}
      defaultValues={defaultValues}
      fields={fields}
      onSubmit={onSubmit}
      onCancel={onCancel}
      isLoading={isLoading}
      submitLabel={initialData?.name ? t('common.update') : t('common.create')}
      isModal={isModal}
      modalOpen={modalOpen}
      modalTitle={modalTitle || (initialData?.name ? t('admin.actions.edit') : t('admin.actions.create'))}
      onModalClose={onCancel}
      imageBaseUrl={imageBaseUrl}
    />
  );
};

export default BrandForm;
