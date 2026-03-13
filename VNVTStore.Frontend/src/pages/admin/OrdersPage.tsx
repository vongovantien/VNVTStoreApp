import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Truck, Package, ClipboardList, Check, X, Printer } from 'lucide-react';
import defaultImage from '@/assets/default-image.png';
import { Button, Badge, Modal, ConfirmDialog } from '@/components/ui';
import { useAdminOrders, useUpdateOrderStatus } from '@/hooks';
import { formatCurrency, formatDate, getStatusColor, getStatusText } from '@/utils/format';
import { orderService, type OrderDto } from '@/services/orderService';
import { AdminPageHeader } from '@/components/admin';
import { DataTable } from '@/components/common';
import { DataTableColumn } from '@/components/common/DataTable';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { PaginationDefaults, OrderStatus } from '@/constants';
import { StatsCards } from '@/components/admin/StatsCards';
import { useQuery } from '@tanstack/react-query';

export const OrdersPage = () => {
  const { t } = useTranslation();
  const updateStatusMutation = useUpdateOrderStatus();

  // State
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedOrder, setSelectedOrder] = useState<OrderDto | null>(null);
  const [currentPage, setCurrentPage] = useState(PaginationDefaults.PAGE_INDEX);
  const [pageSize, setPageSize] = useState(PaginationDefaults.PAGE_SIZE);
  const [advancedFilters, setAdvancedFilters] = useState<Record<string, string>>({});



  const queryClient = useQueryClient();
  const [orderToDelete, setOrderToDelete] = useState<OrderDto | null>(null);

  const deleteMutation = useMutation({
    mutationFn: (id: string) => orderService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-orders'] }); // adapt key if needed
      setOrderToDelete(null);
    }
  });

  const handleDelete = () => {
    if (orderToDelete) {
      deleteMutation.mutate(orderToDelete.code);
    }
  };

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
      header: t('common.fields.orderCode'),
      accessor: (row) => <span className="font-medium">{row.code}</span>,
      sortable: true
    },
    {
      id: 'customer',
      header: t('common.fields.customer'),
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
      accessor: (row) => <span>{t('admin.order.productCount', { count: row.orderItems?.length || 0 })}</span>,
      className: 'text-center',
      headerClassName: 'text-center'
    },
    {
      id: 'total',
      header: t('common.fields.total'),
      accessor: (row) => <span className="font-semibold text-error">{formatCurrency(row.finalAmount)}</span>,
      className: 'text-right',
      headerClassName: 'text-right',
      sortable: true
    },
    {
      id: 'payment',
      header: t('common.fields.payment'),
      accessor: (row) => (
        <Badge
          color={row.paymentStatus === 'Completed' ? 'success' : row.paymentStatus === 'Pending' ? 'warning' : 'error'}
        >
          {row.paymentStatus === 'Completed' ? t('admin.payment.paid') : row.paymentStatus === 'Pending' ? t('admin.payment.pending') : t('admin.payment.failed')}
        </Badge>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    },
    {
      id: 'status',
      header: t('common.fields.status'),
      accessor: (row) => (
        <Badge color={getStatusColor(row.status) as "success" | "warning" | "error" | "info" | "primary" | "secondary"} size="sm">
          {t(getStatusText(row.status))}
        </Badge>
      ),
      className: 'text-center',
      headerClassName: 'text-center',
      sortable: true
    },
    {
      id: 'date',
      header: t('common.fields.date'),
      accessor: (row) => <span className="text-secondary text-sm">{formatDate(row.orderDate)}</span>,
      sortable: true
    }
  ];

  // Visible Columns State
  const [visibleColumns, setVisibleColumns] = useState<string[]>(columns.map(c => c.id));

  // Fetch Data
  const { data: ordersData, isLoading, isFetching, refetch } = useAdminOrders({
    pageIndex: currentPage,
    pageSize: pageSize,
    filters: {
      ...advancedFilters,
      ...(searchQuery ? { search: searchQuery } : {})
    }
  });

  const orders = ordersData?.orders || [];
  const totalPages = ordersData?.totalPages || 1;

  // Fetch Stats
  const { data: statsData, isLoading: isStatsLoading } = useQuery({
      queryKey: ['order-stats'],
      queryFn: () => orderService.getStats(),
      staleTime: 60000,
  });

  const handleAdvancedSearch = (filters: Record<string, string>) => {
    setAdvancedFilters(filters);
    setCurrentPage(PaginationDefaults.PAGE_INDEX);
  };

  const handleReset = () => {
    setAdvancedFilters({});
    setSearchQuery('');
    setCurrentPage(PaginationDefaults.PAGE_INDEX);
    setPageSize(PaginationDefaults.PAGE_SIZE);

  };

  // Selection & Toolbar State




  return (
    <div className="space-y-6">
      {/* Header */}
      <AdminPageHeader
        title="admin.sidebar.orders"
        subtitle="admin.subtitles.orders"
        rightSection={
          <div className="flex items-center gap-2">
            <Badge color="warning">{t('admin.order.pendingCount', { count: orders.filter(o => o.status === OrderStatus.PENDING).length })}</Badge>
            <Badge color="info">{t('admin.order.shippingCount', { count: orders.filter(o => o.status === OrderStatus.SHIPPING).length })}</Badge>
          </div>
        }
      />

    <StatsCards stats={[
        {
            label: t('admin.stats.totalOrders'),
            value: statsData?.total || 0,
            icon: <ClipboardList size={24} />,
            color: 'blue',
            loading: isStatsLoading
        },
        {
            label: t('admin.stats.pendingOrders'),
            value: statsData?.pending || 0,
            icon: <Package size={24} />,
            color: 'amber',
            loading: isStatsLoading
        },
        {
            label: t('admin.stats.shippingOrders'),
            value: statsData?.shipping || 0,
            icon: <Truck size={24} />,
            color: 'purple',
            loading: isStatsLoading
        }
    ]} />

      <DataTable
        columns={columns}
        data={orders}
        keyField="code"
        isLoading={isLoading || isFetching}
        
        // Pagination
        currentPage={currentPage}
        totalPages={totalPages}
        pageSize={pageSize}
        onPageChange={setCurrentPage}
        onPageSizeChange={(size) => {
          setPageSize(size);
          setCurrentPage(PaginationDefaults.PAGE_INDEX);
        }}
        
        onAdd={() => { }} // Placeholder
        onRefresh={refetch}
        onView={(order) => setSelectedOrder(order)}
        onEdit={(order) => setSelectedOrder(order)} // Map Edit to View for now as Order Edit is complex
        onDelete={(order) => setOrderToDelete(order)}
        renderRowActions={(order) => (
          <>

            {order.status === OrderStatus.PENDING && (
              <button
                className="p-1.5 hover:bg-success/10 rounded transition-colors text-success"
                title={t('admin.actions.confirmOrder')}
                onClick={() => updateStatus(order.code, OrderStatus.CONFIRMED)}
              >
                <Check size={18} />
              </button>
            )}
  
            {order.status === OrderStatus.CONFIRMED && (
              <button
                className="p-1.5 hover:bg-primary/10 rounded transition-colors text-primary"
                title={t('admin.actions.shipOrder')}
                onClick={() => updateStatus(order.code, OrderStatus.SHIPPING)}
              >
                <Truck size={18} />
              </button>
            )}
  
            {order.status === OrderStatus.SHIPPING && (
              <button
                className="p-1.5 hover:bg-success/10 rounded transition-colors text-success"
                title={t('admin.actions.markDelivered')}
                onClick={() => updateStatus(order.code, OrderStatus.DELIVERED)}
              >
                <Package size={18} />
              </button>
            )}
  
            {order.status === OrderStatus.DELIVERED && (
              <button
                className="p-1.5 hover:bg-secondary rounded transition-colors text-secondary"
                title={t('admin.actions.printInvoice')}
                onClick={handlePrintInvoice}
              >
                <Printer size={18} />
              </button>
            )}
  
            {order.status === OrderStatus.PENDING && (
              <button
                className="p-1.5 hover:bg-error/10 rounded transition-colors text-error"
                title={t('admin.actions.cancelOrder')}
                onClick={() => updateStatus(order.code, OrderStatus.CANCELLED)}
              >
                <X size={18} />
              </button>
            )}
          </>
        )}
        onReset={handleReset}
        exportFilename="orders_export"
        exportColumns={[
          { key: 'code', label: t('common.fields.orderCode'), width: 15 },
          { key: 'shippingName', label: t('common.fields.customer'), width: 20 },
          { key: 'shippingPhone', label: t('common.fields.phone'), width: 15 },
          { key: 'shippingAddress', label: t('common.fields.address'), width: 30 },
          { key: 'totalAmount', label: t('common.fields.total'), width: 15 },
          { key: 'finalAmount', label: t('common.fields.finalAmount'), width: 15 },
          { key: 'shippingFee', label: t('common.fields.shippingFee'), width: 12 },
          { key: 'discountAmount', label: t('common.fields.discount'), width: 12 },
          { key: 'status', label: t('common.fields.status'), width: 15 },
          { key: 'paymentMethod', label: t('common.fields.paymentMethod'), width: 18 },
          { key: 'paymentStatus', label: t('common.fields.payment'), width: 15 },
          { key: 'orderDate', label: t('common.fields.date'), width: 18 },
          { key: 'note', label: t('common.fields.note'), width: 25 },
        ]}
        onExportAllData={async () => {
          const response = await orderService.search({ pageIndex: 1, pageSize: 10000 });
          return response.data?.items || [];
        }}
        enableSelection={false}

        // Search & Filter
        onAdvancedSearch={handleAdvancedSearch}
        advancedFilterDefs={[
          {
            id: 'fromDate',
            label: t('admin.filters.fromDate'),
            type: 'date',
            placeholder: 'dd/mm/yyyy'
          },
          {
            id: 'toDate',
            label: t('admin.filters.toDate'),
            type: 'date',
            placeholder: 'dd/mm/yyyy'
          },
          {
            id: 'status',
            label: t('common.fields.status'),
            type: 'select',
            options: [
              { value: OrderStatus.PENDING, label: t('common.status.pending') },
              { value: OrderStatus.CONFIRMED, label: t('common.status.confirmed') },
              { value: OrderStatus.SHIPPING, label: t('common.status.shipping') },
              { value: OrderStatus.DELIVERED, label: t('common.status.delivered') },
              { value: OrderStatus.CANCELLED, label: t('common.status.cancelled') },
            ]
          },
          {
            id: 'paymentStatus',
            label: t('common.fields.payment'),
            type: 'select',
            options: [
              { value: 'Completed', label: t('common.status.paid') },
              { value: 'Pending', label: t('common.status.unpaid') },
            ]
          },
          {
            id: 'amountFrom',
            label: t('admin.filters.amountFrom'),
            type: 'number',
            placeholder: '0'
          },
          {
            id: 'amountTo',
            label: t('admin.filters.amountTo'),
            type: 'number',
            placeholder: '0'
          },
          {
            id: 'customer',
            label: t('common.fields.customer'),
            type: 'text',
            placeholder: t('common.placeholders.search')
          },
          {
            id: 'code',
            label: t('common.fields.orderCode'),
            type: 'text',
            placeholder: t('common.placeholders.enterCode')
          }
        ]}

        // Visibility
        visibleColumns={visibleColumns}
        onColumnVisibilityChange={setVisibleColumns}

        emptyMessage={t('common.noResults')}

        // Selection
        selectedIds={new Set()}
        onSelectionChange={() => {}}
      />

      <ConfirmDialog
        isOpen={!!orderToDelete}
        onClose={() => setOrderToDelete(null)}
        onConfirm={handleDelete}
        title={t('admin.actions.delete')}
        message={t('messages.confirmDelete')}
        isLoading={deleteMutation.isPending}
        variant="danger"
      />

      {/* Order Detail Modal */}
      <Modal
        isOpen={!!selectedOrder}
        onClose={() => setSelectedOrder(null)}
        title={`${t('common.actions.view')} ${t('common.fields.orderCode')}: ${selectedOrder?.code}`}
        size="lg"
        footer={
          selectedOrder && (
            <div className="flex justify-end gap-3">
              <Button variant="ghost" onClick={() => setSelectedOrder(null)}>{t('common.close')}</Button>
              {selectedOrder.status === OrderStatus.PENDING && (
                <Button variant="primary" onClick={() => updateStatus(selectedOrder.code, OrderStatus.CONFIRMED)}>{t('common.actions.confirmOrderFull')}</Button>
              )}
              {selectedOrder.status === OrderStatus.DELIVERED && (
                <Button variant="outline" leftIcon={<Printer size={16} />} onClick={handlePrintInvoice}>{t('common.actions.printInvoice')}</Button>
              )}
            </div>
          )
        }
      >
        {selectedOrder && (
          <div className="space-y-6">
            {/* Status */}
            <div className="flex items-center gap-4">
              <Badge color={getStatusColor(selectedOrder.status) as "success" | "warning" | "error" | "info" | "primary" | "secondary"}>
                {getStatusText(selectedOrder.status)}
              </Badge>
              <Badge color={selectedOrder.paymentStatus === 'Completed' ? 'success' : 'warning'}>
                {selectedOrder.paymentStatus === 'Completed' ? t('admin.payment.paid') : t('admin.payment.pending')}
              </Badge>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* Customer Info */}
              <div className="bg-secondary rounded-lg p-4">
                <h3 className="font-semibold mb-2">{t('admin.order.customerInfo')}</h3>
                <p className="font-medium">{selectedOrder.shippingName || selectedOrder.userCode}</p>
                <p className="text-secondary text-sm">{selectedOrder.shippingPhone || '-'}</p>
                <p className="text-secondary text-sm mt-1">{selectedOrder.shippingAddress || '-'}</p>
              </div>

              {/* Order Info */}
              <div className="bg-secondary rounded-lg p-4">
                <h3 className="font-semibold mb-2">{t('admin.order.orderInfo')}</h3>
                <p className="text-secondary text-sm">{t('admin.order.orderDate')}: {formatDate(selectedOrder.orderDate)}</p>
                <p className="text-secondary text-sm">{t('common.fields.paymentMethod')}: {selectedOrder.paymentMethod === 'cod' ? t('admin.order.paymentMethodCod') : t('admin.order.paymentMethodTransfer')}</p>
              </div>
            </div>

            {/* Items */}
            <div>
              <h3 className="font-semibold mb-3">{t('common.products')}</h3>
              <div className="space-y-3 border rounded-lg p-3 bg-primary">
                {selectedOrder.orderItems?.map((item, index) => (
                  <div key={index} className="flex items-center gap-3 py-2 border-b last:border-0">
                    <img 
                      src={item.productImage || defaultImage} 
                      alt={item.productName} 
                      className="w-12 h-12 object-cover rounded border"
                      onError={(e) => {
                        (e.target as HTMLImageElement).src = defaultImage;
                      }}
                    />
                    <div className="flex-1">
                      <p className="font-medium text-sm">{item.productName}</p>
                      <p className="text-xs text-tertiary">{t('common.fields.quantity')}: {item.quantity}</p>
                    </div>
                    <p className="font-medium">{formatCurrency(item.priceAtOrder * item.quantity)}</p>
                  </div>
                ))}
              </div>
            </div>

            {/* Summary */}
            <div className="border-t pt-4 space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-secondary">{t('admin.order.subtotal')}</span>
                <span>{formatCurrency(selectedOrder.totalAmount)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-secondary">{t('admin.order.shippingFee')}</span>
                <span>{formatCurrency(selectedOrder.shippingFee)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-secondary">{t('admin.order.discount')}</span>
                <span className="text-success">-{formatCurrency(selectedOrder.discountAmount || 0)}</span>
              </div>
              <div className="flex justify-between font-bold text-lg pt-2 border-t">
                <span>{t('admin.order.grandTotal')}</span>
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
