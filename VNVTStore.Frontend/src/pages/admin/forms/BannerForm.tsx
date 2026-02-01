import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { BaseForm, FieldDefinition } from '@/components/common';
import { createSchemas } from '@/utils/schemas';

// ============ Type ============
// Since bannerSchema is dynamic (localized), we can define a static interface 
// that matches it for prop typing.
export interface BannerFormData {
  title: string;
  content: string;
  linkUrl: string;
  linkText: string;
  imageURL: string;
  priority: number;
  isActive: boolean;
  [key: string]: any; // Required for FieldValues compatibility if we can't use 'any' then what?
}

// ============ Props ============
interface BannerFormProps {
  initialData?: Partial<BannerFormData>;
  onSubmit: (data: BannerFormData) => void | Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

// ============ Component ============
export const BannerForm = ({
  initialData,
  onSubmit,
  onCancel,
  isLoading = false,
}: BannerFormProps) => {
  const { t } = useTranslation();
  const { bannerSchema } = createSchemas(t);
  
  // Field definitions
  const fields: FieldDefinition[] = [
    {
      name: 'imageURL',
      type: 'image',
      label: t('common.fields.image'),
      colSpan: 12,
    },
    {
      name: 'title',
      type: 'text',
      label: t('common.fields.title'),
      placeholder: t('common.placeholders.enterTitle'),
      required: true,
      colSpan: 12,
    },
    {
      name: 'content',
      type: 'text',
      label: t('common.fields.content'),
      placeholder: t('common.placeholders.enterContent'),
      colSpan: 12,
    },
    {
      name: 'linkUrl',
      type: 'text',
      label: t('common.fields.linkUrl'),
      placeholder: t('common.placeholders.enterLinkUrl'),
      colSpan: 6,
    },
    {
      name: 'linkText',
      type: 'text',
      label: t('common.fields.linkText'),
      placeholder: t('common.placeholders.enterLinkText'),
      colSpan: 6,
    },
    {
      name: 'priority',
      type: 'number',
      label: t('common.fields.priority'),
      colSpan: 6,
    },
    {
      name: 'isActive',
      type: 'switch',
      label: t('common.status.active'),
      description: t('admin.statusHint'),
      colSpan: 6,
    },
  ];

  // Default values
  const defaultValues: BannerFormData = {
    title: initialData?.title || '',
    content: initialData?.content || '',
    linkUrl: initialData?.linkUrl || '',
    linkText: initialData?.linkText || t('common.placeholders.learnMore'),
    imageURL: initialData?.imageURL || '',
    priority: initialData?.priority || 0,
    isActive: initialData?.isActive ?? true,
  };

  return (
    <BaseForm<BannerFormData>
      schema={bannerSchema as any}
      defaultValues={defaultValues}
      fields={fields}
      onSubmit={onSubmit}
      onCancel={onCancel}
      isLoading={isLoading}
      submitLabel={initialData?.title ? t('common.save') : t('common.create')}
    />
  );
};

export default BannerForm;
