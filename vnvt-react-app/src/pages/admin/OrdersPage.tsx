import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, Eye, Truck, Check, X, Printer, Package } from 'lucide-react';
import { Button, Badge, Modal } from '@/components/ui';
import { mockOrders } from '@/data/mockData';
import { formatCurrency, formatDate, getStatusColor, getStatusText } from '@/utils/format';

export const OrdersPage = () => {
  const { t } = useTranslation();
  const [orders, setOrders] = useState(mockOrders);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [selectedOrder, setSelectedOrder] = useState<typeof mockOrders[0] | null>(null);

  const filteredOrders = orders.filter((order) => {
    const matchesSearch =
      order.orderNumber.toLowerCase().includes(searchQuery.toLowerCase()) ||
      order.customer.name.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesStatus = statusFilter === 'all' || order.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const updateStatus = (orderId: string, newStatus: string) => {
    setOrders(orders.map(o => o.id === orderId ? { ...o, status: newStatus as any } : o));
    if (selectedOrder && selectedOrder.id === orderId) {
      setSelectedOrder({ ...selectedOrder, status: newStatus as any });
    }
  };

  const handlePrintInvoice = () => {
    window.print();
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-bold">{t('admin.orders')}</h1>
        <div className="flex items-center gap-2">
          <Badge color="warning">{orders.filter(o => o.status === 'pending').length} chờ xác nhận</Badge>
          <Badge color="info">{orders.filter(o => o.status === 'shipping').length} đang giao</Badge>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-primary rounded-xl p-4">
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="flex-1 relative">
            <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary" />
            <input
              type="text"
              placeholder="Tìm theo mã đơn, tên khách hàng..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border rounded-lg focus:outline-none focus:border-primary bg-transparent"
            />
          </div>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="px-4 py-2 border rounded-lg focus:outline-none focus:border-primary bg-primary"
          >
            <option value="all">Tất cả trạng thái</option>
            <option value="pending">Chờ xác nhận</option>
            <option value="confirmed">Đã xác nhận</option>
            <option value="processing">Đang xử lý</option>
            <option value="shipping">Đang giao</option>
            <option value="delivered">Đã giao</option>
            <option value="cancelled">Đã hủy</option>
          </select>
        </div>
      </div>

      {/* Table */}
      <div className="bg-primary rounded-xl overflow-hidden shadow-sm border">
        <div className="overflow-x-auto">
          <table className="w-full min-w-[900px]">
            <thead className="bg-secondary border-b">
              <tr>
                <th className="px-4 py-3 text-left text-sm font-semibold">{t('admin.columns.orderCode')}</th>
                <th className="px-4 py-3 text-left text-sm font-semibold">{t('admin.columns.customer')}</th>
                <th className="px-4 py-3 text-center text-sm font-semibold">{t('admin.products')}</th>
                <th className="px-4 py-3 text-right text-sm font-semibold">{t('admin.columns.total')}</th>
                <th className="px-4 py-3 text-center text-sm font-semibold">{t('admin.columns.payment')}</th>
                <th className="px-4 py-3 text-center text-sm font-semibold">{t('admin.columns.status')}</th>
                <th className="px-4 py-3 text-left text-sm font-semibold">{t('admin.columns.date')}</th>
                <th className="px-4 py-3 text-center text-sm font-semibold">{t('admin.columns.action')}</th>
              </tr>
            </thead>
            <tbody>
              {filteredOrders.map((order) => (
                <tr key={order.id} className="border-b last:border-0 hover:bg-secondary/50 transition-colors">
                  <td className="px-4 py-4 font-medium">{order.orderNumber}</td>
                  <td className="px-4 py-4">
                    <div>
                      <p className="font-medium">{order.customer.name}</p>
                      <p className="text-xs text-tertiary">{order.customer.phone}</p>
                    </div>
                  </td>
                  <td className="px-4 py-4 text-center">{order.items.length} sản phẩm</td>
                  <td className="px-4 py-4 text-right font-semibold text-error">
                    {formatCurrency(order.total)}
                  </td>
                  <td className="px-4 py-4 text-center">
                    <Badge
                      color={order.paymentStatus === 'paid' ? 'success' : order.paymentStatus === 'pending' ? 'warning' : 'error'}
                      size="sm"
                    >
                      {order.paymentStatus === 'paid' ? 'Đã thanh toán' : order.paymentStatus === 'pending' ? 'Chờ thanh toán' : 'Thất bại'}
                    </Badge>
                  </td>
                  <td className="px-4 py-4 text-center">
                    <Badge color={getStatusColor(order.status) as any} size="sm">
                      {getStatusText(order.status)}
                    </Badge>
                  </td>
                  <td className="px-4 py-4 text-secondary text-sm">{formatDate(order.createdAt)}</td>
                  <td className="px-4 py-4">
                    <div className="flex items-center justify-center gap-2">
                      <button
                        className="p-2 hover:bg-secondary rounded-lg transition-colors"
                        onClick={() => setSelectedOrder(order)}
                        title="Xem chi tiết"
                      >
                        <Eye size={16} />
                      </button>
                      
                      {/* Workflow Actions */}
                      {order.status === 'pending' && (
                        <button 
                          className="p-2 hover:bg-success/10 rounded-lg transition-colors" 
                          title="Xác nhận đơn"
                          onClick={() => updateStatus(order.id, 'confirmed')}
                        >
                          <Check size={16} className="text-success" />
                        </button>
                      )}
                      
                      {order.status === 'confirmed' && (
                        <button 
                          className="p-2 hover:bg-primary/10 rounded-lg transition-colors" 
                          title="Giao hàng"
                          onClick={() => updateStatus(order.id, 'shipping')}
                        >
                          <Truck size={16} className="text-primary" />
                        </button>
                      )}

                      {order.status === 'shipping' && (
                        <button 
                          className="p-2 hover:bg-success/10 rounded-lg transition-colors" 
                          title="Đã giao"
                          onClick={() => updateStatus(order.id, 'delivered')}
                        >
                          <Package size={16} className="text-success" />
                        </button>
                      )}

                      {order.status === 'delivered' && (
                         <button 
                           className="p-2 hover:bg-secondary rounded-lg transition-colors" 
                           title="In hóa đơn"
                           onClick={handlePrintInvoice}
                         >
                           <Printer size={16} className="text-secondary" />
                         </button>
                      )}
                      
                      {order.status === 'pending' && (
                         <button 
                           className="p-2 hover:bg-error/10 rounded-lg transition-colors" 
                           title="Hủy đơn"
                           onClick={() => updateStatus(order.id, 'cancelled')}
                         >
                           <X size={16} className="text-error" />
                         </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <div className="flex items-center justify-between p-4 border-t">
          <p className="text-sm text-secondary">Hiển thị {filteredOrders.length} đơn hàng</p>
          <div className="flex gap-2">
            <button className="px-3 py-1 border rounded hover:bg-secondary disabled:opacity-50" disabled>
              Trước
            </button>
            <button className="px-3 py-1 bg-primary text-white rounded">1</button>
            <button className="px-3 py-1 border rounded hover:bg-secondary disabled:opacity-50" disabled>
              Sau
            </button>
          </div>
        </div>
      </div>

      {/* Order Detail Modal */}
      <Modal
        isOpen={!!selectedOrder}
        onClose={() => setSelectedOrder(null)}
        title={`Chi tiết đơn hàng ${selectedOrder?.orderNumber}`}
        size="lg"
        footer={
           selectedOrder && (
             <div className="flex justify-end gap-3">
               <Button variant="outline" onClick={() => setSelectedOrder(null)}>Đóng</Button>
               {selectedOrder.status === 'pending' && (
                 <Button onClick={() => updateStatus(selectedOrder.id, 'confirmed')}>Xác nhận đơn hàng</Button>
               )}
               {selectedOrder.status === 'delivered' && (
                 <Button leftIcon={<Printer size={16} />} onClick={handlePrintInvoice}>In hóa đơn</Button>
               )}
             </div>
           )
        }
      >
        {selectedOrder && (
          <div className="space-y-6">
            {/* Status */}
            <div className="flex items-center gap-4">
              <Badge color={getStatusColor(selectedOrder.status) as any}>
                {getStatusText(selectedOrder.status)}
              </Badge>
              <Badge color={selectedOrder.paymentStatus === 'paid' ? 'success' : 'warning'}>
                {selectedOrder.paymentStatus === 'paid' ? 'Đã thanh toán' : 'Chờ thanh toán'}
              </Badge>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* Customer Info */}
              <div className="bg-secondary rounded-lg p-4">
                <h3 className="font-semibold mb-2">Thông tin khách hàng</h3>
                <p className="font-medium">{selectedOrder.customer.name}</p>
                <p className="text-secondary text-sm">{selectedOrder.customer.phone}</p>
                <p className="text-secondary text-sm">{selectedOrder.customer.email}</p>
                <p className="text-secondary text-sm mt-1">{selectedOrder.customer.address}</p>
              </div>
              
               {/* Order Info */}
               <div className="bg-secondary rounded-lg p-4">
                <h3 className="font-semibold mb-2">Thông tin đơn hàng</h3>
                <p className="text-secondary text-sm">Ngày đặt: {formatDate(selectedOrder.createdAt)}</p>
                <p className="text-secondary text-sm">Phương thức: {selectedOrder.paymentMethod === 'cod' ? 'Thanh toán khi nhận hàng (COD)' : 'Chuyển khoản'}</p>
              </div>
            </div>

            {/* Items */}
            <div>
              <h3 className="font-semibold mb-3">Sản phẩm</h3>
              <div className="space-y-3 border rounded-lg p-3 bg-primary">
                {selectedOrder.items.map((item, index) => (
                  <div key={index} className="flex items-center gap-3 py-2 border-b last:border-0">
                    <img src={item.productImage} alt={item.productName} className="w-12 h-12 object-cover rounded border" />
                    <div className="flex-1">
                      <p className="font-medium text-sm">{item.productName}</p>
                      <p className="text-xs text-tertiary">Số lượng: {item.quantity}</p>
                    </div>
                    <p className="font-medium">{formatCurrency(item.price * item.quantity)}</p>
                  </div>
                ))}
              </div>
            </div>

            {/* Summary */}
            <div className="border-t pt-4 space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-secondary">Tạm tính</span>
                <span>{formatCurrency(selectedOrder.subtotal)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-secondary">Phí ship</span>
                <span>{formatCurrency(selectedOrder.shippingFee)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-secondary">Giảm giá</span>
                <span className="text-success">-{formatCurrency(selectedOrder.discount)}</span>
              </div>
              <div className="flex justify-between font-bold text-lg pt-2 border-t">
                <span>Tổng cộng</span>
                <span className="text-error">{formatCurrency(selectedOrder.total)}</span>
              </div>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
};

export default OrdersPage;
