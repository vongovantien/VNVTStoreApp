import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, Eye, Truck, Check, X, Printer, Package, ChevronUp, ChevronDown, Loader2, Download } from 'lucide-react';
import { Button, Badge, Modal, Pagination } from '@/components/ui';
import { useAdminOrders, useUpdateOrderStatus } from '@/hooks';
import { formatCurrency, formatDate, getStatusColor, getStatusText } from '@/utils/format';
import type { OrderDto, OrderItemDto } from '@/services/orderService';
import { AdminToolbar } from '@/components/admin/AdminToolbar';
import { TableToolbar } from './components/TableToolbar';
import { exportToCSV } from '@/utils/export';

export const OrdersPage = () => {
  const { t } = useTranslation();
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [selectedOrder, setSelectedOrder] = useState<OrderDto | null>(null);
  const [currentPage, setCurrentPage] = useState(1);

  // Sorting
  type SortField = 'orderDate' | 'totalAmount' | 'status';
  type SortDirection = 'asc' | 'desc';
  const [sortField, setSortField] = useState<SortField>('orderDate');
  const [sortDir, setSortDir] = useState<SortDirection>('desc');

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDir(sortDir === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDir('desc');
    }
  };

  const SortIcon = ({ field }: { field: SortField }) => {
    if (sortField !== field) return null;
    return sortDir === 'asc' ? <ChevronUp size={14} /> : <ChevronDown size={14} />;
  };

  // Fetch orders
  const { data: ordersData, isLoading, isFetching } = useAdminOrders({
    pageIndex: currentPage,
    pageSize: 10,
    status: statusFilter !== 'all' ? statusFilter : undefined,
    search: searchQuery || undefined
  });
  
  const updateStatusMutation = useUpdateOrderStatus();

  const orders = ordersData?.orders || [];
  const totalPages = ordersData?.totalPages || 1;

  // Client-side sort (API might not support all sort fields)
  const sortedOrders = [...orders].sort((a, b) => {
    let comparison = 0;
    if (sortField === 'orderDate') {
      comparison = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
    } else if (sortField === 'totalAmount') {
      comparison = a.finalAmount - b.finalAmount;
    } else if (sortField === 'status') {
      comparison = a.status.localeCompare(b.status);
    }
    return sortDir === 'desc' ? -comparison : comparison;
  });

  const updateStatus = (orderId: string, newStatus: string) => {
    updateStatusMutation.mutate({ code: orderId, status: newStatus });
    if (selectedOrder && selectedOrder.code === orderId) {
      setSelectedOrder({ ...selectedOrder, status: newStatus });
    }
  };

  // Selection & Toolbar State
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [showSearch, setShowSearch] = useState(true);

  const handleExport = () => {
    exportToCSV(sortedOrders, 'orders_export');
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

      {/* Toolbar */}
      <div className="flex flex-col gap-4">
        <div className="flex justify-between items-center gap-4 bg-white dark:bg-slate-800 p-2 rounded-lg border shadow-sm">
          <AdminToolbar
            onSearchClick={() => setShowSearch(!showSearch)}
            onReset={() => {
              setSearchQuery('');
              setStatusFilter('all');
              setSortField('orderDate');
              setSortDir('desc');
              setCurrentPage(1);
              setSelectedIds(new Set());
            }}
            onExport={handleExport}
            isSearchActive={showSearch}
            selectedCount={selectedIds.size}
          />
        </div>

        {showSearch && (
          <TableToolbar
            searchQuery={searchQuery}
            onSearchChange={(val) => { setSearchQuery(val); setCurrentPage(1); }}
            searchField="all"
            onSearchFieldChange={() => {}}
            searchOptions={[
              { label: t('admin.columns.orderCode'), value: 'code' },
              { label: t('admin.columns.customer'), value: 'customer' },
            ]}
            selectedCount={selectedIds.size}
            onExport={handleExport}
          />
        )}

        {/* Status Filter */}
        <div className="flex gap-2">
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="px-4 py-2 border rounded-lg focus:outline-none focus:border-primary bg-primary"
          >
            <option value="all">{t('admin.filters.allStatus')}</option>
            <option value="pending">{t('admin.status.pending')}</option>
            <option value="confirmed">{t('admin.status.confirmed')}</option>
            <option value="processing">{t('admin.status.processing')}</option>
            <option value="shipping">{t('admin.status.shipping')}</option>
            <option value="delivered">{t('admin.status.delivered')}</option>
            <option value="cancelled">{t('admin.status.cancelled')}</option>
          </select>
          <Badge color="warning">{orders.filter(o => o.status === 'pending').length} chờ xác nhận</Badge>
          <Badge color="info">{orders.filter(o => o.status === 'shipping').length} đang giao</Badge>
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
              {isLoading ? (
                <tr>
                  <td colSpan={8} className="px-4 py-12 text-center">
                    <Loader2 className="w-8 h-8 mx-auto animate-spin" />
                  </td>
                </tr>
              ) : sortedOrders.length === 0 ? (
                <tr>
                  <td colSpan={8} className="px-4 py-12 text-center text-secondary">
                    Không có đơn hàng nào
                  </td>
                </tr>
              ) : (
                sortedOrders.map((order: OrderDto) => (
                <tr key={order.code} className="border-b last:border-0 hover:bg-secondary/50 transition-colors">
                  <td className="px-4 py-4 font-medium">{order.code}</td>
                  <td className="px-4 py-4">
                    <div>
                      <p className="font-medium">{order.shippingName || order.userCode}</p>
                      <p className="text-xs text-tertiary">{order.shippingPhone || '-'}</p>
                    </div>
                  </td>
                  <td className="px-4 py-4 text-center">{order.orderItems?.length || 0} sản phẩm</td>
                  <td className="px-4 py-4 text-right font-semibold text-error">
                    {formatCurrency(order.finalAmount)}
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
                          onClick={() => updateStatus(order.code, 'confirmed')}
                        >
                          <Check size={16} className="text-success" />
                        </button>
                      )}
                      
                      {order.status === 'confirmed' && (
                        <button 
                          className="p-2 hover:bg-primary/10 rounded-lg transition-colors" 
                          title="Giao hàng"
                          onClick={() => updateStatus(order.code, 'shipping')}
                        >
                          <Truck size={16} className="text-primary" />
                        </button>
                      )}

                      {order.status === 'shipping' && (
                        <button 
                          className="p-2 hover:bg-success/10 rounded-lg transition-colors" 
                          title="Đã giao"
                          onClick={() => updateStatus(order.code, 'delivered')}
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
                           onClick={() => updateStatus(order.code, 'cancelled')}
                         >
                           <X size={16} className="text-error" />
                         </button>
                      )}
                    </div>
                  </td>
                </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <div className="flex items-center justify-between p-4 border-t">
          <p className="text-sm text-secondary">Hiển thị {sortedOrders.length} / {ordersData?.totalItems || 0} đơn hàng</p>
          {totalPages > 1 && (
            <Pagination
              currentPage={currentPage}
              totalPages={totalPages}
              totalItems={ordersData?.totalItems || 0}
              pageSize={10}
              onPageChange={setCurrentPage}
            />
          )}
        </div>
      </div>

      {/* Order Detail Modal */}
      <Modal
        isOpen={!!selectedOrder}
        onClose={() => setSelectedOrder(null)}
        title={`Chi tiết đơn hàng ${selectedOrder?.code}`}
        size="lg"
        footer={
           selectedOrder && (
             <div className="flex justify-end gap-3">
               <Button variant="outline" onClick={() => setSelectedOrder(null)}>Đóng</Button>
               {selectedOrder.status === 'pending' && (
                 <Button onClick={() => updateStatus(selectedOrder.code, 'confirmed')}>Xác nhận đơn hàng</Button>
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
                <p className="font-medium">{selectedOrder.shippingName || selectedOrder.userCode}</p>
                <p className="text-secondary text-sm">{selectedOrder.shippingPhone || '-'}</p>
                <p className="text-secondary text-sm mt-1">{selectedOrder.shippingAddress || '-'}</p>
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
                {selectedOrder.orderItems?.map((item, index) => (
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
                <span>{formatCurrency(selectedOrder.totalAmount)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-secondary">Phí ship</span>
                <span>{formatCurrency(selectedOrder.shippingFee)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-secondary">Giảm giá</span>
                <span className="text-success">-{formatCurrency(selectedOrder.discountAmount || 0)}</span>
              </div>
              <div className="flex justify-between font-bold text-lg pt-2 border-t">
                <span>Tổng cộng</span>
                <span className="text-error">{formatCurrency(selectedOrder.finalAmount)}</span>
              </div>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
};

export default OrdersPage;
