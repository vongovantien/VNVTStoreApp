import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { Package, Calendar, ChevronRight, MapPin, Star } from 'lucide-react';
import SharedImage from '@/components/common/Image';
import { Button, Badge, Modal } from '@/components/ui';
import { formatDate, formatCurrency, getStatusColor, getStatusText } from '@/utils/format';
import { orderService, type OrderDto, type OrderItemDto } from '@/services/orderService';
import ReviewForm from '@/components/reviews/ReviewForm';
import { useAuthStore } from '@/store';

const OrdersContent = () => {
  const { t } = useTranslation();
  const [orders, setOrders] = useState<OrderDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedItem, setSelectedItem] = useState<OrderItemDto | null>(null);
  const { user } = useAuthStore();

  const fetchOrders = () => {
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
  };

  useEffect(() => {
    fetchOrders();
  }, []);

  const handleReviewSuccess = () => {
    setSelectedItem(null);
    // Optionally refresh orders if needed (e.g. to hide review button if functionality added later) or show success toast
  };

  if (loading) return <div>{t('common.loading')}</div>;

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-bold">{t('account.orders')}</h2>

      {orders.length === 0 && <p className="text-secondary">{t('review.no_orders')}</p>}

      {orders.map((order) => (
        <div key={order.code} className="bg-primary rounded-xl p-4 border border-secondary/20">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2 mb-4">
            <div>
              <Link to={`/account/orders/${order.code}`} className="font-bold text-primary hover:underline">
                #{order.code}
              </Link>
              <p className="text-sm text-tertiary">{formatDate(order.orderDate)}</p>
            </div>
            <span
              className={`px-3 py-1 rounded-full text-xs font-semibold bg-${getStatusColor(order.status)}/20 text-${getStatusColor(order.status)}`}
            >
              {t(getStatusText(order.status))}
            </span>
          </div>

          <div className="space-y-2 mb-4">
             {order.orderItems?.map((item, index) => (
              <div key={index} className="flex items-center gap-3 py-2 border-b border-secondary/10 last:border-0">
                <SharedImage
                  src={item.productImage}
                  alt={item.productName}
                  className="h-16 w-16 rounded-md object-cover flex-shrink-0"
                />
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium truncate">{item.productName}</p>
                  <div className="flex justify-between items-center mt-1">
                      <p className="text-xs text-tertiary">x{item.quantity}</p>
                      
                      {order.status === 'Delivered' && (
                        <Button 
                            variant="outline" 
                            size="xs" 
                            leftIcon={<Star size={12}/>}
                            onClick={() => setSelectedItem(item)}
                        >
                            {t('review.write')}
                        </Button>
                      )}
                  </div>
                </div>
                <p className="text-sm font-medium flex-shrink-0">{formatCurrency(item.priceAtOrder * item.quantity)}</p>
              </div>
            ))}
          </div>

          <div className="flex justify-between items-center pt-4 border-t">
            <span className="text-sm text-secondary">{t('cart.total')}</span>
            <span className="font-bold text-error">{formatCurrency(order.totalAmount)}</span>
          </div>
        </div>
      ))}

      {/* Review Modal */}
      <Modal
        isOpen={!!selectedItem}
        onClose={() => setSelectedItem(null)}
        title={t('review.write_review')}
        size="lg"
      >
        {selectedItem && user && (
            <ReviewForm
                orderItemCode={selectedItem.code} 
                productName={selectedItem.productName}
                productImage={selectedItem.productImage}
                userCode={user.code}
                onSuccess={handleReviewSuccess}
                onCancel={() => setSelectedItem(null)}
            />
        )}
      </Modal>
    </div>
  );
};

export default OrdersContent;
