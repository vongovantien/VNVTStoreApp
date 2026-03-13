import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ClipboardList, Package, Truck } from 'lucide-react';
import { useQueryClient, useMutation } from '@tanstack/react-query';
import { AdminPageHeader } from '@/components/admin';
import { StatsCards } from '@/components/admin/StatsCards';
import { OrderList } from '../components/OrderList';
import { OrderDetailModal } from '../components/OrderDetailModal';
import { useAdminOrders, useOrderStats, ADMIN_ORDER_KEYS } from '../hooks/useAdminOrders';
import { orderService, OrderDto } from '@/services/orderService';
import { useToast } from '@/store';
import { PaginationDefaults } from '@/constants';

export const OrdersPage = () => {
    const { t } = useTranslation();
    const { success, error: toastError } = useToast();
    const queryClient = useQueryClient();

    // State
    const [page, setPage] = useState(PaginationDefaults.PAGE_INDEX);
    const [pageSize, setPageSize] = useState(PaginationDefaults.PAGE_SIZE);
    const [selectedOrder, setSelectedOrder] = useState<OrderDto | null>(null);

    // Data Fetching
    const { data, isLoading, isFetching, refetch } = useAdminOrders({
        pageIndex: page,
        pageSize,
        sortBy: 'OrderDate',
        sortDesc: true
    });

    const { data: stats, isLoading: isStatsLoading } = useOrderStats();

    // Mutations
    const updateStatusMutation = useMutation({
        mutationFn: ({ code, status }: { code: string; status: string }) => 
            orderService.updateStatus(code, status),
        onSuccess: (_, variables) => {
            success(t('common.updateSuccess'));
            queryClient.invalidateQueries({ queryKey: ADMIN_ORDER_KEYS.all });
            
            // Update local state if modal is open
            if (selectedOrder && selectedOrder.code === variables.code) {
                setSelectedOrder({ ...selectedOrder, status: variables.status });
            }
        },
        onError: () => toastError(t('common.errors.updateFailed'))
    });

    // Handlers
    const handleUpdateStatus = (code: string, status: string) => {
        updateStatusMutation.mutate({ code, status });
    };

    const handlePrint = () => {
        window.print();
    };

    const handleExport = async () => {
         const response = await orderService.search({ pageIndex: 1, pageSize: 10000 });
         return (response.data?.items || []) as OrderDto[];
    };

    return (
        <div className="space-y-6">
            <AdminPageHeader
                title="admin.sidebar.orders"
                subtitle="admin.subtitles.orders"
            />

            <StatsCards stats={[
                {
                    label: t('admin.stats.totalOrders'),
                    value: stats?.total || 0,
                    icon: <ClipboardList size={24} />,
                    color: 'blue',
                    loading: isStatsLoading
                },
                {
                    label: t('admin.stats.pendingOrders'),
                    value: stats?.pending || 0,
                    icon: <Package size={24} />,
                    color: 'amber',
                    loading: isStatsLoading
                },
                {
                    label: t('admin.stats.shippingOrders'),
                    value: stats?.shipping || 0,
                    icon: <Truck size={24} />,
                    color: 'purple',
                    loading: isStatsLoading
                }
            ]} />

            <OrderList
                orders={data?.orders || []}
                totalItems={data?.totalItems || 0}
                totalPages={data?.totalPages || 0}
                page={page}
                pageSize={pageSize}
                isLoading={isLoading}
                isFetching={isFetching}
                onPageChange={setPage}
                onPageSizeChange={setPageSize}
                onView={setSelectedOrder}
                onUpdateStatus={handleUpdateStatus}
                onPrint={handlePrint}
                onRefresh={refetch}
                onExport={handleExport}
            />

            <OrderDetailModal
                order={selectedOrder}
                onClose={() => setSelectedOrder(null)}
                onUpdateStatus={handleUpdateStatus}
                onPrint={handlePrint}
            />
        </div>
    );
};

export default OrdersPage;
