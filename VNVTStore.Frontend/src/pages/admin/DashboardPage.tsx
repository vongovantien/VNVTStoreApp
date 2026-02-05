import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { ShoppingBag, Users, DollarSign, ArrowUpRight, TrendingUp, TrendingDown, Package, Loader2 } from 'lucide-react';
import { RevenueChart, AdminPageHeader } from '@/components/admin';
import { formatCurrency, getStatusColor, getStatusText } from '@/utils/format';
import { dashboardService } from '@/services';
import { useQuery } from '@tanstack/react-query';
import { useAdminOrders } from '@/hooks';
import { PageSize, PaginationDefaults } from '@/constants';

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
      animate={{ opacity: 1, y: 0 }}
      className="bg-primary rounded-xl p-6"
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
           Array(4).fill(0).map((_, i) => (
             <div key={i} className="bg-primary rounded-xl p-6 h-32 animate-pulse" />
           ))
        ) : statsError ? (
           <div className="col-span-4 p-4 bg-red-50 text-red-500 rounded-xl">
             Failed to load dashboard statistics. Please try again.
           </div>
        ) : (
          <>
            <StatCard
              title={t('admin.stats.revenue')}
              value={formatCurrency(stats.totalRevenue)}
              change={stats.revenueChange}
              icon={DollarSign}
              color="bg-gradient-to-r from-green-500 to-emerald-500"
            />
            <StatCard
              title={t('admin.stats.orders')}
              value={stats.totalOrders.toLocaleString()}
              change={stats.ordersChange}
              icon={ShoppingBag}
              color="bg-gradient-to-r from-blue-500 to-cyan-500"
            />
            <StatCard
              title={t('admin.stats.customers')}
              value={stats.totalCustomers.toLocaleString()}
              change={stats.customersChange}
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
        <div className="lg:col-span-2 bg-primary rounded-xl p-6 shadow-sm border">
          <h2 className="font-bold mb-4">{t('admin.revenueChart')}</h2>
          <div className="h-64">
            <RevenueChart data={stats.revenueChart} />
          </div>
        </div>

        {/* Top Products */}
        <div className="bg-primary rounded-xl p-6">
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
                  <p className="text-sm text-tertiary">{product.sales} {t('admin.sold')}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Recent Orders & Pending Quotes */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Orders */}
        <div className="bg-primary rounded-xl p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-bold">{t('admin.recentOrders')}</h2>
            <a href="/admin/orders" className="text-sm text-primary flex items-center gap-1 hover:underline">
              {t('common.viewAll')} <ArrowUpRight size={14} />
            </a>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="text-left text-sm text-tertiary border-b">
                  <th className="pb-3">{t('common.fields.orderCode')}</th>
                  <th className="pb-3">{t('common.fields.customer')}</th>
                  <th className="pb-3">{t('common.fields.total')}</th>
                  <th className="pb-3">{t('common.fields.status')}</th>
                </tr>
              </thead>
              <tbody>
                {ordersLoading ? (
                  <tr><td colSpan={4} className="py-8 text-center"><Loader2 className="animate-spin mx-auto" /></td></tr>
                ) : recentOrders.length === 0 ? (
                  <tr><td colSpan={4} className="py-4 text-center text-secondary">{t('common.noData')}</td></tr>
                ) : (recentOrders.map((order) => (
                  <tr key={order.code} className="border-b last:border-0">
                    <td className="py-3 font-medium">{order.code}</td>
                    <td className="py-3">{order.shippingName || order.userCode}</td>
                    <td className="py-3 text-error font-medium">{formatCurrency(order.finalAmount)}</td>
                    <td className="py-3">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium bg-${getStatusColor(order.status)}/20 text-${getStatusColor(order.status)}`}>
                        {t(getStatusText(order.status))}
                      </span>
                    </td>
                  </tr>
                )))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Pending Quotes (Note: Currently we only show count, list is dummy or requires real fetch) */}
        <div className="bg-primary rounded-xl p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-bold flex items-center gap-2">
              {t('admin.quoteRequests')}
              <span className="px-2 py-0.5 bg-error/20 text-error text-xs rounded-full">
                {stats.pendingQuotes} {t('admin.pending')}
              </span>
            </h2>
            <a href="/admin/quotes" className="text-sm text-primary flex items-center gap-1 hover:underline">
              {t('common.viewAll')} <ArrowUpRight size={14} />
            </a>
          </div>
          <p className="text-sm text-secondary">
             {t('common.messages.checkQuotesPage')}
          </p>
        </div>
      </div>
    </div>
  );
};

export default DashboardPage;
