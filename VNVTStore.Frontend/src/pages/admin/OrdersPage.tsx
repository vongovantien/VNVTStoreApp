import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, Eye, Truck, Check, X, Printer, Package, ChevronUp, ChevronDown, Loader2, Download, Trash2 } from 'lucide-react';
import defaultImage from '@/assets/default-image.png';
import { Button, Badge, Modal, Pagination, ConfirmDialog, TableActions } from '@/components/ui';
import { useAdminOrders, useUpdateOrderStatus } from '@/hooks';
import { formatCurrency, formatDate, getStatusColor, getStatusText } from '@/utils/format';
import { orderService, type OrderDto, type OrderItemDto } from '@/services/orderService';
import { AdminToolbar, ColumnVisibility, TableToolbar, AdminPageHeader } from '@/components/admin';
import { DataTable } from '@/components/common';
import { exportToExcel } from '@/utils/export';
import { DataTableColumn } from '@/components/common/DataTable';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { PageSize, PaginationDefaults, SortDirection } from '@/constants';

export const OrdersPage = () => {
  const { t } = useTranslation();
  const updateStatusMutation = useUpdateOrderStatus();

  // State
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedOrder, setSelectedOrder] = useState<OrderDto | null>(null);
  const [currentPage, setCurrentPage] = useState(PaginationDefaults.PAGE_INDEX);
  const [pageSize, setPageSize] = useState(PaginationDefaults.PAGE_SIZE);
  const [advancedFilters, setAdvancedFilters] = useState<Record<string, string>>({});

  // Sorting
  type SortField = 'orderDate' | 'totalAmount' | 'status';
  type SortDirection = 'asc' | 'desc';
  const [sortField, setSortField] = useState<SortField>('orderDate');
  const [sortDir, setSortDir] = useState<SortDirection>('desc');

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
          color={row.paymentStatus === 'paid' ? 'success' : row.paymentStatus === 'pending' ? 'warning' : 'error'}
          size="sm"
        >
          {row.paymentStatus === 'paid' ? t('admin.payment.paid') : row.paymentStatus === 'pending' ? t('admin.payment.pending') : t('admin.payment.failed')}
        </Badge>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    },
    {
      id: 'status',
      header: t('common.fields.status'),
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
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [showSearch, setShowSearch] = useState(true);

  // Sorting Handler
  const sortedOrders = [...orders].sort((a, b) => {
    let comparison = 0;
    if (sortField === 'orderDate') {
      comparison = new Date(a.orderDate).getTime() - new Date(b.orderDate).getTime();
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

  const handleExport = async () => {
    await exportToExcel(sortedOrders, 'orders_export');
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <AdminPageHeader
        title="admin.sidebar.orders"
        subtitle="admin.subtitles.orders"
        rightSection={
          <div className="flex items-center gap-2">
            <Badge color="warning">{t('admin.order.pendingCount', { count: orders.filter(o => o.status === 'pending').length })}</Badge>
            <Badge color="info">{t('admin.order.shippingCount', { count: orders.filter(o => o.status === 'shipping').length })}</Badge>
          </div>
        }
      />



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
        onView={(order) => setSelectedOrder(order)}
        onEdit={(order) => setSelectedOrder(order)} // Map Edit to View for now as Order Edit is complex
        onDelete={(order) => setOrderToDelete(order)}
        renderRowActions={(order) => (
          <>
            {order.status === 'pending' && (
              <button
                className="p-1.5 hover:bg-success/10 rounded transition-colors text-success"
                title={t('admin.actions.confirmOrder')}
                onClick={() => updateStatus(order.code, 'confirmed')}
              >
                <Check size={18} />
              </button>
            )}
  
            {order.status === 'confirmed' && (
              <button
                className="p-1.5 hover:bg-primary/10 rounded transition-colors text-primary"
                title={t('admin.actions.shipOrder')}
                onClick={() => updateStatus(order.code, 'shipping')}
              >
                <Truck size={18} />
              </button>
            )}
  
            {order.status === 'shipping' && (
              <button
                className="p-1.5 hover:bg-success/10 rounded transition-colors text-success"
                title={t('admin.actions.markDelivered')}
                onClick={() => updateStatus(order.code, 'delivered')}
              >
                <Package size={18} />
              </button>
            )}
  
            {order.status === 'delivered' && (
              <button
                className="p-1.5 hover:bg-secondary rounded transition-colors text-secondary"
                title={t('admin.actions.printInvoice')}
                onClick={handlePrintInvoice}
              >
                <Printer size={18} />
              </button>
            )}
  
            {order.status === 'pending' && (
              <button
                className="p-1.5 hover:bg-error/10 rounded transition-colors text-error"
                title={t('admin.actions.cancelOrder')}
                onClick={() => updateStatus(order.code, 'cancelled')}
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
          { key: 'createdAt', label: t('common.fields.date'), width: 18 },
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
              { value: 'pending', label: t('admin.status.pending') },
              { value: 'confirmed', label: t('admin.status.confirmed') },
              { value: 'shipping', label: t('admin.status.shipping') },
              { value: 'delivered', label: t('admin.status.delivered') },
              { value: 'cancelled', label: t('admin.status.cancelled') },
            ]
          },
          {
            id: 'paymentStatus',
            label: t('common.fields.payment'),
            type: 'select',
            options: [
              { value: 'paid', label: t('admin.status.paid') },
              { value: 'pending', label: t('admin.status.unpaid') },
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
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
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
        title={`${t('admin.actions.view')} ${t('common.fields.orderCode')}: ${selectedOrder?.code}`}
        size="lg"
        footer={
          selectedOrder && (
            <div className="flex justify-end gap-3">
              <Button variant="outline" onClick={() => setSelectedOrder(null)}>{t('common.close')}</Button>
              {selectedOrder.status === 'pending' && (
                <Button onClick={() => updateStatus(selectedOrder.code, 'confirmed')}>{t('admin.actions.confirmOrderFull')}</Button>
              )}
              {selectedOrder.status === 'delivered' && (
                <Button leftIcon={<Printer size={16} />} onClick={handlePrintInvoice}>{t('admin.actions.printInvoice')}</Button>
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
                {selectedOrder.paymentStatus === 'paid' ? t('admin.payment.paid') : t('admin.payment.pending')}
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
