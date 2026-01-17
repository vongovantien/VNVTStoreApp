
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { BaseForm, FieldDefinition } from '@/components/common';
import { useCategories } from '@/hooks';

// ============ Schema ============
const categorySchema = z.object({
  name: z.string().min(1, 'Tên danh mục là bắt buộc'),
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
}: CategoryFormProps) => {
  const { t } = useTranslation();
  const { data: categories = [] } = useCategories();

  // Build parent category options
  const parentOptions = [
    { value: '', label: t('common.none', 'Không có') },
    ...categories
      .filter((c) => c.code !== excludeCode) // Exclude self by code
      .map((c) => ({ value: c.code, label: c.name })),
  ];

  // Field definitions
  const fields: FieldDefinition[] = [
    {
      name: 'imageUrl',
      type: 'image',
      label: t('admin.columns.image'),
      colSpan: 12,
    },
    {
      name: 'name',
      type: 'text',
      label: t('admin.columns.name'),
      placeholder: t('admin.placeholders.categoryName', 'Nhập tên danh mục'),
      required: true,
      colSpan: 12,
    },
    {
      name: 'description',
      type: 'textarea',
      label: t('admin.columns.description'),
      placeholder: t('admin.placeholders.categoryDescription', 'Mô tả danh mục'),
      rows: 3,
      colSpan: 12,
    },
    {
      name: 'parentCode',
      type: 'select',
      label: t('admin.columns.parentCategory'),
      options: parentOptions,
      colSpan: 12,
    },
    {
      name: 'isActive',
      type: 'switch',
      label: t('admin.columns.status'),
      description: t('admin.statusHint', 'Bật để hiển thị danh mục'),
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
    />
  );
};

export default CategoryForm;
