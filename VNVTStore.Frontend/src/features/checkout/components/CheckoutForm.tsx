import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { useToast } from '@/store';
import { formatCurrency } from '@/utils/format';

import { DynamicForm } from '@/components/shared/DynamicForm/DynamicForm';
import { FieldConfig, FieldType } from '@/shared/constants/form-config';
import { useCheckoutStore } from '../store/checkoutStore';
import { useCheckoutMutation } from '../hooks/useCheckoutMutation';

// Validation Schema
const checkoutSchema = z.object({
  fullName: z.string().min(2, 'Vui lòng nhập họ tên (tối thiểu 2 ký tự)'),
  phoneNumber: z.string().regex(/^(0|\+84)[3|5|7|8|9][0-9]{8}$/, 'Số điện thoại không hợp lệ'),
  address: z.string().min(5, 'Vui lòng nhập địa chỉ chi tiết'),
  note: z.string().optional(),
});

type CheckoutFormData = z.infer<typeof checkoutSchema>;

// Form Configuration
const checkoutFields: FieldConfig<CheckoutFormData>[] = [
  {
    name: 'fullName',
    type: FieldType.TEXT,
    label: 'checkout.fullName',
    placeholder: 'Nguyễn Văn A',
  },
  {
    name: 'phoneNumber',
    type: FieldType.TEXT,
    label: 'checkout.phoneNumber', 
    placeholder: '0912345678',
  },
  {
    name: 'address',
    type: FieldType.TEXTAREA,
    label: 'checkout.address',
    placeholder: 'Số nhà, đường, phường/xã...',
  },
  {
    name: 'note',
    type: FieldType.TEXTAREA,
    label: 'checkout.note',
    placeholder: 'Giao giờ hành chính...',
  },
];



// ... (imports remain)

export const CheckoutForm = () => {
  const { t } = useTranslation();
  const { session, closeCheckout, clearSession } = useCheckoutStore();
  const { success, error: toastError } = useToast();
  
  // Use Mutation Hook
  const { mutate: submitOrder, isPending: isSubmitting } = useCheckoutMutation();

  if (!session) return null;

  const { product, quantity } = session;
  const totalAmount = product.price * quantity;

  // Transform fields with translation (remains same)
  const translatedFields = checkoutFields.map(field => ({
    ...field,
    label: t(field.label, field.label === 'checkout.fullName' ? 'Họ và tên' : 
             field.label === 'checkout.phoneNumber' ? 'Số điện thoại' :
             field.label === 'checkout.address' ? 'Địa chỉ nhận hàng' : 'Ghi chú (tùy chọn)')
  }));

  const onSubmit = (data: CheckoutFormData) => {
    submitOrder(
      {
        customer: {
          fullName: data.fullName,
          phoneNumber: data.phoneNumber,
          address: data.address,
          note: data.note,
        },
        items: [
          {
            productId: product.id,
            quantity: quantity,
            price: product.price,
          },
        ]
      },
      {
        onSuccess: () => {
          success('Đặt hàng thành công! Chúng tôi sẽ liên hệ sớm.');
          clearSession();
        },
        onError: (err) => {
          toastError(err instanceof Error ? err.message : 'Đặt hàng thất bại');
        },
      }
    );
  };

  const renderHeader = () => (
      <div className="bg-slate-50 dark:bg-slate-900 rounded-lg p-4 flex gap-4">
        {product.image && (
          <img 
            src={product.image} 
            alt={product.name} 
            className="w-16 h-16 object-cover rounded-md border border-slate-200 dark:border-slate-700"
          />
        )}
        <div className="flex-1">
          <h4 className="font-medium text-slate-900 dark:text-white line-clamp-1">{product.name}</h4>
          <div className="text-sm text-slate-500 mt-1 flex justify-between">
            <span>{quantity} x {formatCurrency(product.price)}</span>
            <span className="font-bold text-rose-600">{formatCurrency(totalAmount)}</span>
          </div>
        </div>
      </div>
  );

  return (
    <DynamicForm<CheckoutFormData>
      schema={checkoutSchema}
      fields={translatedFields}
      onSubmit={onSubmit}
      isLoading={isSubmitting}
      submitLabel={t('checkout.confirm', 'Xác nhận đặt hàng')}
      cancelLabel={t('common.cancel', 'Hủy')}
      onCancel={closeCheckout}
      renderHeader={renderHeader}
    />
  );
};
