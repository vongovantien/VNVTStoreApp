import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { formatDate, formatCurrency, getStatusColor, getStatusText } from '@/utils/format';
import { orderService, type OrderDto } from '@/services/orderService';

const OrdersContent = () => {
  const { t } = useTranslation();
  const [orders, setOrders] = useState<OrderDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    orderService.getMyOrders({ pageIndex: 1, pageSize: 20 }).then(res => {
      if(res.success && res.data) {
          if (Array.isArray(res.data)) {
               setOrders(res.data as unknown as OrderDto[]);
          } else if (res.data.items) {
               setOrders(res.data.items);
          }
      }
      setLoading(false);
    });
  }, []);

  if (loading) return <div>{t('common.loading')}</div>;

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-bold">{t('account.orders')}</h2>

      {orders.length === 0 && <p className="text-secondary">Chưa có đơn hàng nào.</p>}

      {orders.map((order) => (
        <div key={order.code} className="bg-primary rounded-xl p-4 border border-secondary/20">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2 mb-4">
            <div>
              <p className="font-semibold">{order.code}</p>
              <p className="text-sm text-tertiary">{formatDate(order.createdAt)}</p>
            </div>
            <span
              className={`px-3 py-1 rounded-full text-xs font-semibold bg-${getStatusColor(order.status)}/20 text-${getStatusColor(order.status)}`}
            >
              {getStatusText(order.status)}
            </span>
          </div>

          <div className="space-y-2 mb-4">
             {order.orderItems?.map((item, index) => (
              <div key={index} className="flex items-center gap-3">
                 <img
                  src={item.productImage || 'https://via.placeholder.com/50'}
                  alt={item.productName}
                  className="w-12 h-12 object-cover rounded"
                />
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium truncate">{item.productName}</p>
                  <p className="text-xs text-tertiary">x{item.quantity}</p>
                </div>
                <p className="text-sm font-medium">{formatCurrency(item.price * item.quantity)}</p>
              </div>
            ))}
          </div>

          <div className="flex justify-between items-center pt-4 border-t">
            <span className="text-sm text-secondary">{t('cart.total')}</span>
            <span className="font-bold text-error">{formatCurrency(order.totalAmount)}</span>
          </div>
        </div>
      ))}
    </div>
  );
};

export default OrdersContent;
