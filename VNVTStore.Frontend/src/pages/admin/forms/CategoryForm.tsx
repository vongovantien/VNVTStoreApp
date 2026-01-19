
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { BaseForm, FieldDefinition } from '@/components/common';
import { useCategories } from '@/hooks';

// ============ Schema ============
const categorySchema = z.object({
  name: z.string().min(1, 'required'), // will be localized in component or handling
  description: z.string().optional(),
  parentCode: z.string().optional(),
  imageUrl: z.string().optional(),
  isActive: z.boolean(),
});

export type CategoryFormData = z.infer<typeof categorySchema>;

// ============ Props ============
interface CategoryFormProps {
  initialData?: Partial<CategoryFormData>;
  onSubmit: (data: CategoryFormData) => void | Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
  isModal?: boolean;
  modalOpen?: boolean;
  modalTitle?: string;
  excludeCode?: string;
  imageBaseUrl?: string;
}

// ============ Component ============
export const CategoryForm = ({
  initialData,
  onSubmit,
  onCancel,
  isLoading = false,
  isModal = true,
  modalOpen = false,
  modalTitle,
  excludeCode,
  imageBaseUrl,
}: CategoryFormProps) => {
  const { t } = useTranslation();
  const { data: categories = [] } = useCategories();

  // Build parent category options
  const parentOptions = [
    { value: '', label: t('common.actions.none') },
    ...categories
      .filter((c) => c.code !== excludeCode) // Exclude self by code
      .map((c) => ({ value: c.code, label: c.name })),
  ];

  // Field definitions
  const fields: FieldDefinition[] = [
    {
      name: 'imageUrl',
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
      name: 'parentCode',
      type: 'select',
      label: t('common.fields.parentCategory'),
      options: parentOptions,
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
  const defaultValues: CategoryFormData = {
    name: initialData?.name || '',
    description: initialData?.description || '',
    parentCode: initialData?.parentCode || '',
    imageUrl: initialData?.imageUrl || '',
    isActive: initialData?.isActive ?? true,
  };

  return (
    <BaseForm<CategoryFormData>
      schema={categorySchema}
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

export default CategoryForm;
