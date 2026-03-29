import { useTranslation } from 'react-i18next';
import { Badge } from '@/components/ui';
import { DataTable, type DataTableColumn } from '@/components/common/DataTable';
import { type ExportColumn } from '@/utils/export';
import { OrderDto } from '@/services/orderService';
import { formatCurrency, formatDate, getStatusColor, getStatusText } from '@/utils/format';
import { Check, Truck, Package, X, Printer, Eye } from 'lucide-react';
import { OrderStatus } from '@/constants';

interface OrderListProps {
    orders: OrderDto[];
    totalItems: number;
    totalPages: number;
    page: number;
    pageSize: number;
    isLoading: boolean;
    isFetching: boolean;
    onPageChange: (page: number) => void;
    onPageSizeChange: (size: number) => void;
    onView: (order: OrderDto) => void;
    onUpdateStatus: (code: string, status: string) => void;
    onPrint: () => void;
    onRefresh: () => void;
    onExport: () => Promise<OrderDto[]>;
}

export const OrderList = ({
    orders,
    totalItems,
    totalPages,
    page,
    pageSize,
    isLoading,
    isFetching,
    onPageChange,
    onPageSizeChange,
    onView,
    onUpdateStatus,
    onPrint,
    onRefresh,
    onExport
}: OrderListProps) => {
    const { t } = useTranslation();

    const columns: DataTableColumn<OrderDto>[] = [
        {
            id: 'code',
            header: t('common.fields.orderCode'),
            accessor: (row) => <span className="font-medium text-primary hover:underline cursor-pointer" onClick={() => onView(row)}>{row.code}</span>,
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
            accessor: (row) => <span className="text-center block">{row.orderItems?.length || 0}</span>,
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
                <Badge color={row.paymentStatus === 'Completed' ? 'success' : row.paymentStatus === 'Pending' ? 'warning' : 'error'}>
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
                <Badge color={getStatusColor(row.status) as 'default' | 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info'} size="sm">
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

    return (
        <DataTable
            columns={columns}
            data={orders}
            keyField="code"
            isLoading={isLoading || isFetching}
            
            // Pagination
            currentPage={page}
            totalPages={totalPages}
            totalItems={totalItems}
            pageSize={pageSize}
            onPageChange={onPageChange}
            onPageSizeChange={onPageSizeChange}

            // Actions
            onRefresh={onRefresh}
            onView={onView}
            renderRowActions={(order) => (
                <div className="flex gap-1 justify-end">
                    <button 
                         className="p-1.5 hover:bg-slate-100 dark:hover:bg-slate-800 rounded text-slate-500"
                         title={t('admin.actions.view')}
                         onClick={() => onView(order)}
                    >
                        <Eye size={18} />
                    </button>
                    {order.status === OrderStatus.PENDING && (
                        <>
                            <button className="p-1.5 hover:bg-success/10 rounded text-success" title={t('admin.actions.confirmOrder')} onClick={() => onUpdateStatus(order.code, OrderStatus.CONFIRMED)}>
                                <Check size={18} />
                            </button>
                            <button className="p-1.5 hover:bg-error/10 rounded text-error" title={t('admin.actions.cancelOrder')} onClick={() => onUpdateStatus(order.code, OrderStatus.CANCELLED)}>
                                <X size={18} />
                            </button>
                        </>
                    )}
                    {order.status === OrderStatus.CONFIRMED && (
                        <button className="p-1.5 hover:bg-primary/10 rounded text-primary" title={t('admin.actions.shipOrder')} onClick={() => onUpdateStatus(order.code, OrderStatus.SHIPPING)}>
                            <Truck size={18} />
                        </button>
                    )}
                    {order.status === OrderStatus.SHIPPING && (
                        <button className="p-1.5 hover:bg-success/10 rounded text-success" title={t('admin.actions.markDelivered')} onClick={() => onUpdateStatus(order.code, OrderStatus.DELIVERED)}>
                            <Package size={18} />
                        </button>
                    )}
                    {order.status === OrderStatus.DELIVERED && (
                        <button className="p-1.5 hover:bg-secondary rounded text-secondary" title={t('admin.actions.printInvoice')} onClick={onPrint}>
                            <Printer size={18} />
                        </button>
                    )}
                </div>
            )}
            
            // Export
            exportFilename="orders_export"
            exportColumns={[
                { key: 'code', label: t('common.fields.orderCode'), width: 15 },
                { key: 'shippingName', label: t('common.fields.customer'), width: 20 },
                { key: 'shippingPhone', label: t('common.fields.phone'), width: 15 },
                { key: 'finalAmount', label: t('common.fields.finalAmount'), width: 15 },
                { key: 'status', label: t('common.fields.status'), width: 15 },
                { key: 'orderDate', label: t('common.fields.date'), width: 18 },
            ] as ExportColumn<OrderDto>[]}
            onExportAllData={onExport}

            className="bg-white dark:bg-slate-900 rounded-lg border border-border-color shadow-sm"
        />
    );
};
