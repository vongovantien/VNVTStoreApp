
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { BaseForm, FieldDefinition } from '@/components/common';
import { REGEX } from '@/constants/regex';

import type { TFunction } from 'i18next';

// ============ Schema Factory ============
const createSupplierSchema = (t: TFunction) => z.object({
  name: z.string().min(1, t('validation.required')), // will be handled correctly by i18n
  contactPerson: z.string().optional(),
  email: z.string().email(t('validation.invalidEmail')).optional().or(z.literal('')),
  phone: z.string().regex(REGEX.PHONE_SIMPLE, t('validation.invalidPhone')).optional().or(z.literal('')),
  address: z.string().optional(),
  taxCode: z.string().regex(REGEX.TAX_CODE, t('validation.invalidTaxCode')).optional().or(z.literal('')),
  bankAccount: z.string().optional(),
  bankName: z.string().optional(),
  notes: z.string().optional(),
  isActive: z.boolean(),
});

export type SupplierFormData = z.infer<ReturnType<typeof createSupplierSchema>>;

// ============ Props ============
interface SupplierFormProps {
  initialData?: Partial<SupplierFormData>;
  onSubmit: (data: SupplierFormData) => void | Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
  isModal?: boolean;
  modalOpen?: boolean;
  modalTitle?: string;
}

// ============ Component ============
export const SupplierForm = ({
  initialData,
  onSubmit,
  onCancel,
  isLoading = false,
  isModal = true,
  modalOpen = false,
  modalTitle,
}: SupplierFormProps) => {
  const { t } = useTranslation();

  // Field definitions with validation
  const fields: FieldDefinition[] = [
    {
      name: 'name',
      type: 'text',
      label: t('common.fields.name'),
      placeholder: t('common.placeholders.enterName'),
      required: true,
      colSpan: 12,
    },
    {
      name: 'contactPerson',
      type: 'text',
      label: t('common.fields.contactPerson'),
      placeholder: t('common.placeholders.enterName'),
      colSpan: 12,
    },
    {
      name: 'email',
      type: 'email',
      label: t('common.fields.email'),
      placeholder: t('common.placeholders.enterEmail'),
      colSpan: 6,
    },
    {
      name: 'phone',
      type: 'phone',
      label: t('common.fields.phone'),
      placeholder: t('common.placeholders.enterPhone'),
      colSpan: 6,
    },
    {
      name: 'address',
      type: 'text',
      label: t('common.fields.address'),
      placeholder: t('common.placeholders.enterAddress'),
      colSpan: 12,
    },
    {
      name: 'taxCode',
      type: 'text',
      label: t('common.fields.taxCode'),
      placeholder: t('common.placeholders.enterTaxCode'),
      colSpan: 6,
    },
    {
      name: 'bankName',
      type: 'text',
      label: t('common.fields.bankName'),
      placeholder: t('common.placeholders.enterBankName'),
      colSpan: 6,
    },
    {
      name: 'bankAccount',
      type: 'text',
      label: t('common.fields.bankAccount'),
      placeholder: t('common.placeholders.enterBankAccount'),
      colSpan: 12,
    },
    {
      name: 'notes',
      type: 'textarea',
      label: t('common.fields.note'),
      placeholder: t('common.placeholders.enterNote'),
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
  const defaultValues: SupplierFormData = {
    name: initialData?.name || '',
    contactPerson: initialData?.contactPerson || '',
    email: initialData?.email || '',
    phone: initialData?.phone || '',
    address: initialData?.address || '',
    taxCode: initialData?.taxCode || '',
    bankAccount: initialData?.bankAccount || '',
    bankName: initialData?.bankName || '',
    notes: initialData?.notes || '',
    isActive: initialData?.isActive ?? true,
  };

  return (
    <BaseForm<SupplierFormData>
      schema={createSupplierSchema(t)}
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

export default SupplierForm;
