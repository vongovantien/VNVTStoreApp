
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { BaseForm, FieldDefinition } from '@/components/common';

// ============ Schema with Validation ============
const supplierSchema = z.object({
  name: z.string().min(1, 'Tên nhà cung cấp là bắt buộc'),
  contactPerson: z.string().optional(),
  email: z.string().email('Email không hợp lệ').optional().or(z.literal('')),
  phone: z.string().regex(/^[0-9]{10,11}$/, 'Số điện thoại phải có 10-11 số').optional().or(z.literal('')),
  address: z.string().optional(),
  taxCode: z.string().optional(),
  bankAccount: z.string().optional(),
  bankName: z.string().optional(),
  notes: z.string().optional(),
  isActive: z.boolean(),
});

export type SupplierFormData = z.infer<typeof supplierSchema>;

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
      label: t('admin.columns.name'),
      placeholder: t('admin.placeholders.supplierName', 'Nhập tên nhà cung cấp'),
      required: true,
      colSpan: 12,
    },
    {
      name: 'contactPerson',
      type: 'text',
      label: t('admin.columns.contactPerson'),
      placeholder: t('admin.placeholders.contactPerson', 'Người liên hệ'),
      colSpan: 12,
    },
    {
      name: 'email',
      type: 'email',
      label: t('admin.columns.email'),
      placeholder: 'example@email.com',
      colSpan: 6,
    },
    {
      name: 'phone',
      type: 'phone',
      label: t('admin.columns.phone'),
      placeholder: '0901234567',
      colSpan: 6,
    },
    {
      name: 'address',
      type: 'text',
      label: t('admin.columns.address'),
      placeholder: t('admin.placeholders.address', 'Địa chỉ'),
      colSpan: 12,
    },
    {
      name: 'taxCode',
      type: 'text',
      label: t('admin.columns.taxCode'),
      placeholder: t('admin.placeholders.taxCode', 'Mã số thuế'),
      colSpan: 6,
    },
    {
      name: 'bankName',
      type: 'text',
      label: t('admin.columns.bankName'),
      placeholder: t('admin.placeholders.bankName', 'Tên ngân hàng'),
      colSpan: 6,
    },
    {
      name: 'bankAccount',
      type: 'text',
      label: t('admin.columns.bankAccount'),
      placeholder: t('admin.placeholders.bankAccount', 'Số tài khoản'),
      colSpan: 12,
    },
    {
      name: 'notes',
      type: 'textarea',
      label: t('admin.columns.notes'),
      placeholder: t('admin.placeholders.notes', 'Ghi chú'),
      rows: 3,
      colSpan: 12,
    },
    {
      name: 'isActive',
      type: 'switch',
      label: t('admin.columns.status'),
      description: t('admin.statusHint', 'Bật để nhà cung cấp hoạt động'),
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
      schema={supplierSchema}
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
