import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, Eye, Truck, Check, X, Printer, Package, ChevronUp, ChevronDown, Loader2, Download } from 'lucide-react';
import { Button, Badge, Modal, Pagination } from '@/components/ui';
import { useAdminOrders, useUpdateOrderStatus } from '@/hooks';
import { formatCurrency, formatDate, getStatusColor, getStatusText } from '@/utils/format';
import type { OrderDto, OrderItemDto } from '@/services/orderService';
import { AdminToolbar } from '@/components/admin/AdminToolbar';
import { ColumnVisibility } from '@/components/admin/ColumnVisibility';
import { DataTable } from '@/components/common';
import { TableToolbar } from './components/TableToolbar';
import { exportToCSV } from '@/utils/export';
import { DataTableColumn } from '@/components/common/DataTable';

export const OrdersPage = () => {
  const { t } = useTranslation();
  const updateStatusMutation = useUpdateOrderStatus();

  // State
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedOrder, setSelectedOrder] = useState<OrderDto | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [advancedFilters, setAdvancedFilters] = useState<Record<string, string>>({});

  // Sorting
  type SortField = 'orderDate' | 'totalAmount' | 'status';
  type SortDirection = 'asc' | 'desc';
  const [sortField, setSortField] = useState<SortField>('orderDate');
  const [sortDir, setSortDir] = useState<SortDirection>('desc');

  // Actions
  const updateStatus = (orderId: string, newStatus: string) => {
    updateStatusMutation.mutate({ code: orderId, status: newStatus });
    if (selectedOrder && selectedOrder.code === orderId) {
      setSelectedOrder({ ...selectedOrder, status: newStatus });
    }
  };

  const handlePrintInvoice = () => {
    window.print();
  };

  // Column Definitions
  const columns: DataTableColumn<OrderDto>[] = [
    {
      id: 'code',
      header: t('admin.columns.orderCode'),
      accessor: (row) => <span className="font-medium">{row.code}</span>,
      sortable: true
    },
    {
      id: 'customer',
      header: t('admin.columns.customer'),
      accessor: (row) => (
        <div>
          <p className="font-medium">{row.shippingName || row.userCode}</p>
          <p className="text-xs text-tertiary">{row.shippingPhone || '-'}</p>
        </div>
      )
    },
    {
      id: 'products',
      header: t('admin.products'),
      accessor: (row) => <span>{row.orderItems?.length || 0} sản phẩm</span>,
      className: 'text-center',
      headerClassName: 'text-center'
    },
    {
      id: 'total',
      header: t('admin.columns.total'),
      accessor: (row) => <span className="font-semibold text-error">{formatCurrency(row.finalAmount)}</span>,
      className: 'text-right',
      headerClassName: 'text-right',
      sortable: true
    },
    {
      id: 'payment',
      header: t('admin.columns.payment'),
      accessor: (row) => (
        <Badge
          color={row.paymentStatus === 'paid' ? 'success' : row.paymentStatus === 'pending' ? 'warning' : 'error'}
          size="sm"
        >
          {row.paymentStatus === 'paid' ? 'Đã thanh toán' : row.paymentStatus === 'pending' ? 'Chờ thanh toán' : 'Thất bại'}
        </Badge>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    },
    {
      id: 'status',
      header: t('admin.columns.status'),
      accessor: (row) => (
        <Badge color={getStatusColor(row.status) as any} size="sm">
          {getStatusText(row.status)}
        </Badge>
      ),
      className: 'text-center',
      headerClassName: 'text-center',
      sortable: true
    },
    {
      id: 'date',
      header: t('admin.columns.date'),
      accessor: (row) => <span className="text-secondary text-sm">{formatDate(row.createdAt)}</span>,
      sortable: true
    },
    {
      id: 'action',
      header: t('admin.columns.action'),
      className: 'text-center',
      headerClassName: 'text-center',
      accessor: (order) => (
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
      )
    }
  ];

  // Visible Columns State
  const [visibleColumns, setVisibleColumns] = useState<string[]>(columns.map(c => c.id));

  // Fetch Data
  const { data: ordersData, isLoading, isFetching } = useAdminOrders({
    pageIndex: currentPage,
    pageSize: 10,
    filters: {
      ...advancedFilters,
      ...(searchQuery ? { search: searchQuery } : {})
    }
  });

  const orders = ordersData?.orders || [];
  const totalPages = ordersData?.totalPages || 1;

  const handleAdvancedSearch = (filters: Record<string, string>) => {
    setAdvancedFilters(filters);
    setCurrentPage(1);
  };

  // Selection & Toolbar State
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [showSearch, setShowSearch] = useState(true);

  // Sorting Handler
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

  const SortIcon = ({ field }: { field: SortField }) => {
    if (sortField !== field) return null;
    return sortDir === 'asc' ? <ChevronUp size={14} /> : <ChevronDown size={14} />;
  };

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDir(sortDir === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDir('desc');
    }
  };

  const handleExport = () => {
    exportToCSV(sortedOrders, 'orders_export');
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



      <DataTable
        columns={columns}
        data={orders}
        keyField="code"
        isLoading={isLoading || isFetching}
        onAdd={() => { }}
        onEdit={(order) => setSelectedOrder(order)}
        exportFilename="orders_export"

        // Search & Filter
        onAdvancedSearch={handleAdvancedSearch}
        advancedFilterDefs={[
          {
            id: 'fromDate',
            label: t('admin.filters.fromDate') || 'Từ ngày',
            type: 'date',
            placeholder: 'dd/mm/yyyy'
          },
          {
            id: 'toDate',
            label: t('admin.filters.toDate') || 'Đến ngày',
            type: 'date',
            placeholder: 'dd/mm/yyyy'
          },
          {
            id: 'status',
            label: t('admin.columns.status'),
            type: 'select',
            options: [
              { value: 'pending', label: t('admin.status.pending') },
              { value: 'confirmed', label: t('admin.status.confirmed') },
              { value: 'shipping', label: t('admin.status.shipping') },
              { value: 'delivered', label: t('admin.status.delivered') },
              { value: 'cancelled', label: t('admin.status.cancelled') },
            ]
          },
          {
            id: 'paymentStatus',
            label: t('admin.columns.payment'),
            type: 'select',
            options: [
              { value: 'paid', label: t('admin.status.paid') || 'Đã thanh toán' },
              { value: 'pending', label: t('admin.status.unpaid') || 'Chưa thanh toán' },
            ]
          },
          {
            id: 'amountFrom',
            label: t('admin.filters.amountFrom') || 'Tổng tiền từ',
            type: 'number',
            placeholder: '0'
          },
          {
            id: 'amountTo',
            label: t('admin.filters.amountTo') || 'Tổng tiền đến',
            type: 'number',
            placeholder: '0'
          },
          {
            id: 'customer',
            label: t('admin.columns.customer'),
            type: 'text',
            placeholder: t('admin.placeholders.searchCustomer') || 'Nhập tên hoặc SĐT'
          },
          {
            id: 'code',
            label: t('admin.columns.orderCode'),
            type: 'text',
            placeholder: t('admin.placeholders.searchOrderCode') || 'Nhập mã đơn hàng'
          }
        ]}

        // Visibility
        visibleColumns={visibleColumns}
        onColumnVisibilityChange={setVisibleColumns}

        emptyMessage={t('common.noResults')}
      />

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
