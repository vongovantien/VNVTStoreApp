
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import type { TFunction } from 'i18next';
import { BaseForm, FieldDefinition } from '@/components/common';
import { useCategories } from '@/hooks';
import { contentWeaverService } from '@/services/ContentWeaverService';
import { Sparkles } from 'lucide-react';
import { Button } from '@/components/ui';

// ============ Schema ============
const createCategorySchema = (t: TFunction) => z.object({
  name: z.string().min(1, t('validation.required')), // Handle validation correctly
  description: z.string().optional(),
  parentCode: z.string().optional(),
  imageURL: z.string().optional(),
  isActive: z.boolean(),
});

export type CategoryFormData = z.infer<ReturnType<typeof createCategorySchema>>;

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
      .filter((c: {code: string; name: string}) => c.code !== excludeCode) // Exclude self by code
      .map((c: {code: string; name: string}) => ({ value: c.code, label: c.name })),
  ];

  // Field definitions
  const fields: FieldDefinition[] = [
    {
      name: 'imageURL',
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
    imageURL: initialData?.imageURL || '',
    isActive: initialData?.isActive ?? true,
  };

  return (
    <BaseForm<CategoryFormData>
      schema={createCategorySchema(t)}
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
      imageBaseUrl={imageBaseUrl || ''}
      renderBefore={(form) => (
        <div className="mb-4 p-4 bg-accent/5 border border-accent/20 rounded-2xl flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-accent/10 rounded-xl text-accent">
              <Sparkles size={20} />
            </div>
            <div>
              <p className="text-sm font-bold text-text-primary">Autonomous Weaver</p>
              <p className="text-xs text-text-secondary">Generate recursive localized descriptions.</p>
            </div>
          </div>
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() => {
              const name = form.getValues('name');
              const generated = contentWeaverService.generateDescription(name || 'Danh mục', 'default');
              form.setValue('description', generated);
            }}
            disabled={!form.watch('name')}
            leftIcon={<Sparkles size={14} />}
            className="neon-border-sm"
          >
            Auto-generate
          </Button>
        </div>
      )}
    />
  );
};

export default CategoryForm;
