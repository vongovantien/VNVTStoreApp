import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { ShoppingBag, Users, DollarSign, ArrowUpRight, TrendingUp, TrendingDown, Package, Loader2 } from 'lucide-react';
import { RevenueChart, AdminPageHeader } from '@/components/admin';
import { formatCurrency, getStatusColor, getStatusText } from '@/utils/format';
import { dashboardService } from '@/services';
import { useQuery } from '@tanstack/react-query';
import { useAdminOrders } from '@/hooks';
import { PageSize, PaginationDefaults } from '@/constants';
import { Badge } from '@/components/ui';
import { StatCardSkeleton } from '@/components/ui/Skeleton';

// ============ Stat Card Component ============
interface StatCardProps {
  title: string;
  value: string | number;
  change?: number;
  icon: React.ElementType;
  color: string;
}

const StatCard = ({ title, value, change, icon: Icon, color }: StatCardProps) => {
  const { t } = useTranslation();
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once: true }}
      transition={{ duration: 0.4, ease: [0.22, 1, 0.36, 1] }}
      className="bg-bg-primary rounded-xl p-6 border border-border shadow-sm"
    >
      <div className="flex items-start justify-between">
        <div>
          <p className="text-sm text-secondary mb-1">{title}</p>
          <p className="text-2xl font-bold">{value}</p>
          {typeof change === 'number' && (
            <p className={`flex items-center gap-1 text-sm mt-2 ${change >= 0 ? 'text-success' : 'text-error'}`}>
              {change >= 0 ? <TrendingUp size={14} /> : <TrendingDown size={14} />}
              {change >= 0 ? '+' : ''}{change}% {t('admin.stats.vsLastMonth')}
            </p>
          )}
        </div>
        <div className={`w-12 h-12 rounded-xl flex items-center justify-center ${color}`}>
          <Icon size={24} className="text-white" />
        </div>
      </div>
    </motion.div>
  );
};

// ============ Dashboard Page ============
export const DashboardPage = () => {
  const { t, i18n } = useTranslation();



  // Fetch recent orders
  const { data: ordersData, isLoading: ordersLoading } = useAdminOrders({ pageIndex: PaginationDefaults.PAGE_INDEX, pageSize: PageSize.SMALL });
  const recentOrders = ordersData?.orders || [];

  const { data: statsResponse, isLoading: statsLoading, isError: statsError, refetch } = useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: () => dashboardService.getStats(),
    staleTime: 30000,
  });

  const stats = statsResponse?.success && statsResponse?.data ? statsResponse.data : {
    totalRevenue: 0,
    revenueChange: 0,
    totalOrders: 0,
    ordersChange: 0,
    totalCustomers: 0,
    customersChange: 0,
    totalProducts: 0,
    pendingQuotes: 0
  };

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="admin.sidebar.dashboard"
        subtitle="admin.subtitles.dashboard"
        rightSection={
          <div className="flex items-center gap-3">
            <p className="text-secondary font-medium hidden sm:block">
              {new Date().toLocaleDateString(i18n.language, { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}
            </p>
            <button
              onClick={() => refetch()}
              disabled={statsLoading}
              className="p-2 hover:bg-secondary rounded-lg transition-colors text-primary"
              title={t('common.refresh')}
            >
              <Loader2 size={20} className={statsLoading ? 'animate-spin' : ''} />
            </button>
          </div>
        }
      />

      {/* Stats Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        {statsLoading ? (
          Array(4).fill(0).map((_, i) => <StatCardSkeleton key={i} />)
        ) : statsError ? (
           <div className="col-span-4 p-4 bg-error/10 text-error rounded-xl border border-error/20 text-sm">
             {t('admin.stats.loadError', 'Không thể tải số liệu. Vui lòng thử lại.')}
           </div>
        ) : (
          <>
            <StatCard
              title={t('admin.stats.revenue')}
              value={formatCurrency(stats.totalRevenue)}
              change={stats.revenueChange ?? 0}
              icon={DollarSign}
              color="bg-gradient-to-r from-green-500 to-emerald-500"
            />
            <StatCard
              title={t('admin.stats.orders')}
              value={stats.totalOrders.toLocaleString()}
              change={stats.ordersChange ?? 0}
              icon={ShoppingBag}
              color="bg-gradient-to-r from-blue-500 to-cyan-500"
            />
            <StatCard
              title={t('admin.stats.customers')}
              value={stats.totalCustomers.toLocaleString()}
              change={stats.customersChange ?? 0}
              icon={Users}
              color="bg-gradient-to-r from-purple-500 to-pink-500"
            />
            <StatCard
              title={t('admin.stats.products')}
              value={stats.totalProducts}
              icon={Package}
              color="bg-gradient-to-r from-orange-500 to-amber-500"
            />
          </>
        )}
      </div>

      {/* Charts & Tables Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Revenue Chart */}
        <div className="lg:col-span-2 bg-bg-primary rounded-xl p-6 shadow-sm border border-border">
          <h2 className="font-bold mb-4">{t('admin.revenueChart')}</h2>
          <div className="h-64">
            <RevenueChart data={stats.revenueChart ?? []} />
          </div>
        </div>

        {/* Top Products */}
        <div className="bg-bg-primary rounded-xl p-6 border border-border shadow-sm">
          <h2 className="font-bold mb-4">{t('admin.topProducts')}</h2>
          <div className="space-y-4">
            {(stats.topProducts || []).length === 0 ? (
                 <p className="text-secondary text-sm">{t('common.noData')}</p>
            ) : (stats.topProducts || []).map((product, index) => (
              <div key={index} className="flex items-center gap-3">
                <span className="w-8 h-8 rounded-lg bg-secondary flex items-center justify-center font-bold text-sm">
                  {index + 1}
                </span>
                <div className="flex-1 min-w-0">
                  <p className="font-medium truncate">{product.name}</p>
                  <p className="text-sm text-tertiary">{t('admin.sold', { count: product.sales })}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Recent Orders & Pending Quotes */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Orders */}
        <div className="bg-bg-primary rounded-xl p-6 border border-border shadow-sm">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-bold">{t('admin.recentOrders')}</h2>
            <a href="/admin/orders" className="text-sm text-accent flex items-center gap-1 hover:underline">
              {t('common.viewAll')} <ArrowUpRight size={14} />
            </a>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="text-left text-sm text-text-tertiary border-b border-border">
                  <th className="pb-3 font-medium">{t('common.fields.orderCode')}</th>
                  <th className="pb-3 font-medium">{t('common.fields.customer')}</th>
                  <th className="pb-3 font-medium">{t('common.fields.total')}</th>
                  <th className="pb-3 font-medium">{t('common.fields.status')}</th>
                </tr>
              </thead>
              <tbody>
                {ordersLoading ? (
                  <tr><td colSpan={4} className="py-8 text-center"><Loader2 className="animate-spin mx-auto text-text-tertiary" /></td></tr>
                ) : recentOrders.length === 0 ? (
                  <tr><td colSpan={4} className="py-4 text-center text-text-secondary text-sm">{t('common.noData')}</td></tr>
                ) : recentOrders.map((order) => (
                  <tr key={order.code} className="border-b border-border last:border-0 hover:bg-bg-secondary transition-colors">
                    <td className="py-3 font-medium text-sm">{order.code}</td>
                    <td className="py-3 text-sm text-text-secondary">{order.shippingName || order.userCode}</td>
                    <td className="py-3 font-semibold text-sm text-error">{formatCurrency(order.finalAmount)}</td>
                    <td className="py-3">
                      <Badge
                        color={getStatusColor(order.status) as 'warning' | 'success' | 'error' | 'info' | 'secondary' | 'default'}
                        size="sm"
                        variant="soft"
                      >
                        {t(getStatusText(order.status))}
                      </Badge>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Pending Quotes */}
        <div className="bg-bg-primary rounded-xl p-6 border border-border shadow-sm">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-bold flex items-center gap-2">
              {t('admin.quoteRequests')}
              {stats.pendingQuotes > 0 && (
                <span className="px-2 py-0.5 bg-error/10 text-error text-xs rounded-full font-semibold">
                  {stats.pendingQuotes} {t('admin.pending')}
                </span>
              )}
            </h2>
            <a href="/admin/quotes" className="text-sm text-accent flex items-center gap-1 hover:underline">
              {t('common.viewAll')} <ArrowUpRight size={14} />
            </a>
          </div>
          <p className="text-sm text-text-secondary">
             {t('common.messages.checkQuotesPage')}
          </p>
        </div>
      </div>
    </div>
  );
};

export default DashboardPage;
