import { OrderDto } from '@/services/orderService';
import { formatCurrency, formatDate } from '@/utils/format';
import { useTranslation } from 'react-i18next';

interface InvoiceTemplateProps {
  order: OrderDto;
}

export const InvoiceTemplate = ({ order }: InvoiceTemplateProps) => {
  const { t } = useTranslation();

  // Default shop info (Could be moved to a config/context)
  const shopInfo = {
    name: 'Cửa hàng Điện Nước VNVT',
    address: '123 Đường Điện Nước, TP.HCM',
    phone: '0909 123 456',
    email: 'contact@vnvtstore.com',
  };

  return (
    <div
      id="invoice-print-area"
      className="hidden print:block p-8 max-w-[210mm] mx-auto bg-white text-black"
    >
      {/* Header */}
      <div className="flex justify-between items-start mb-8 border-b pb-4">
        <div>
          <h1 className="text-2xl font-bold uppercase mb-2">{shopInfo.name}</h1>
          <p className="text-sm">{shopInfo.address}</p>
          <p className="text-sm">
            {t('common.fields.phone')}: {shopInfo.phone}
          </p>
          <p className="text-sm">
            {t('common.fields.email')}: {shopInfo.email}
          </p>
        </div>
        <div className="text-right">
          <h2 className="text-3xl font-bold text-gray-800 uppercase tracking-wide mb-2">
            {t('admin.actions.invoice') || 'HÓA ĐƠN'}
          </h2>
          <p className="text-sm">#{order.code}</p>
          <p className="text-sm">{formatDate(order.orderDate)}</p>
        </div>
      </div>

      {/* Customer & Order Info */}
      <div className="flex justify-between mb-8 gap-8">
        <div className="flex-1">
          <h3 className="font-bold border-b mb-2 pb-1 uppercase text-sm">
            {t('admin.order.customerInfo')}
          </h3>
          <p className="font-semibold">{order.shippingName || order.userCode}</p>
          <p>{order.shippingPhone}</p>
          <p className="whitespace-pre-wrap">{order.shippingAddress}</p>
        </div>
        <div className="flex-1 text-right">{/* Can add Shipment info here if needed */}</div>
      </div>

      {/* Items Table */}
      <table className="w-full mb-8 border-collapse">
        <thead>
          <tr className="bg-gray-100 border-b-2 border-gray-300">
            <th className="text-left py-2 px-2">#</th>
            <th className="text-left py-2 px-2">{t('common.products')}</th>
            <th className="text-right py-2 px-2">{t('common.fields.quantity')}</th>
            <th className="text-right py-2 px-2">{t('common.fields.price')}</th>
            <th className="text-right py-2 px-2">{t('common.fields.total')}</th>
          </tr>
        </thead>
        <tbody>
          {order.orderItems?.map((item, index) => (
            <tr key={index} className="border-b border-gray-200">
              <td className="py-2 px-2">{index + 1}</td>
              <td className="py-2 px-2">
                <p className="font-medium">{item.productName}</p>
                <p className="text-xs text-gray-500">{item.productCode}</p>
              </td>
              <td className="text-right py-2 px-2">{item.quantity}</td>
              <td className="text-right py-2 px-2">{formatCurrency(item.priceAtOrder)}</td>
              <td className="text-right py-2 px-2 font-medium">
                {formatCurrency(item.priceAtOrder * item.quantity)}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* Summary */}
      <div className="flex justify-end mb-12">
        <div className="w-1/2 space-y-2">
          <div className="flex justify-between border-b border-gray-100 pb-1">
            <span className="text-gray-600">{t('admin.order.subtotal')}</span>
            <span>{formatCurrency(order.totalAmount)}</span>
          </div>
          <div className="flex justify-between border-b border-gray-100 pb-1">
            <span className="text-gray-600">{t('admin.order.shippingFee')}</span>
            <span>{formatCurrency(order.shippingFee)}</span>
          </div>
          {order.discountAmount > 0 && (
            <div className="flex justify-between border-b border-gray-100 pb-1 text-green-600">
              <span>{t('admin.order.discount')}</span>
              <span>-{formatCurrency(order.discountAmount)}</span>
            </div>
          )}
          <div className="flex justify-between font-bold text-xl pt-2">
            <span>{t('admin.order.grandTotal')}</span>
            <span>{formatCurrency(order.finalAmount)}</span>
          </div>
        </div>
      </div>

      {/* Footer */}
      <div className="text-center text-sm text-gray-500 mt-auto pt-8 border-t">
        <p>{t('messages.thankYouForChosingUs') || 'Cảm ơn quý khách đã mua hàng!'}</p>
        <p>Website: vnvtstore.com</p>
      </div>
    </div>
  );
};
