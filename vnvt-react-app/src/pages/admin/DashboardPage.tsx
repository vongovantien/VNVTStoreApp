import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import {
  DollarSign,
  ShoppingBag,
  Users,
  Package,
  TrendingUp,
  TrendingDown,
  ArrowUpRight,
  FileText,
  Loader2,
} from 'lucide-react';
import { RevenueChart } from './components/RevenueChart';
import { formatCurrency, getStatusColor, getStatusText } from '@/utils/format';
import { dashboardService } from '@/services';
import { useQuery } from '@tanstack/react-query';
import { useAdminOrders } from '@/hooks';

// ============ Stat Card Component ============
interface StatCardProps {
  title: string;
  value: string | number;
  change?: number;
  icon: React.ElementType;
  color: string;
}

const StatCard = ({ title, value, change, icon: Icon, color }: StatCardProps) => (
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
            {change >= 0 ? '+' : ''}{change}% so với tháng trước
          </p>
        )}
      </div>
      <div className={`w-12 h-12 rounded-xl flex items-center justify-center ${color}`}>
        <Icon size={24} className="text-white" />
      </div>
    </div>
  </motion.div>
);

// ============ Dashboard Page ============
export const DashboardPage = () => {
  const { t } = useTranslation();

  const { data: statsResponse } = useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: () => dashboardService.getStats(),
    staleTime: 30000, // 30 seconds - prevents double calls
  });

  // Fetch recent orders
  const { data: ordersData, isLoading: ordersLoading } = useAdminOrders({ pageIndex: 1, pageSize: 5 });
  const recentOrders = ordersData?.orders || [];

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
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t('admin.dashboard')}</h1>
        <p className="text-secondary">
          {new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}
        </p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
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
      </div>

      {/* Charts & Tables Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Revenue Chart Placeholder */}
        <div className="lg:col-span-2 bg-primary rounded-xl p-6 shadow-sm border">
          <h2 className="font-bold mb-4">{t('admin.revenueChart')}</h2>
          <div className="h-64">
            <RevenueChart />
          </div>
        </div>

        {/* Top Products */}
        <div className="bg-primary rounded-xl p-6">
          <h2 className="font-bold mb-4">{t('admin.topProducts')}</h2>
          <div className="space-y-4">
            {[
              { name: 'Máy lọc nước RO Kangaroo', sales: 128, revenue: 1150000000 },
              { name: 'Máy giặt Samsung Inverter', sales: 95, revenue: 1187500000 },
              { name: 'Nồi chiên không dầu Philips', sales: 210, revenue: 1047900000 },
            ].map((product, index) => (
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
                  <th className="pb-3">{t('admin.orderCode')}</th>
                  <th className="pb-3">{t('admin.customer')}</th>
                  <th className="pb-3">{t('admin.total')}</th>
                  <th className="pb-3">{t('admin.status')}</th>
                </tr>
              </thead>
              <tbody>
                {ordersLoading ? (
                    <tr><td colSpan={4} className="py-8 text-center"><Loader2 className="animate-spin mx-auto" /></td></tr>
                ) : recentOrders.length === 0 ? (
                    <tr><td colSpan={4} className="py-4 text-center text-secondary">Chưa có đơn hàng</td></tr>
                ) : (recentOrders.map((order) => (
                  <tr key={order.code} className="border-b last:border-0">
                    <td className="py-3 font-medium">{order.code}</td>
                    <td className="py-3">{order.shippingName || order.userCode}</td>
                    <td className="py-3 text-error font-medium">{formatCurrency(order.finalAmount)}</td>
                    <td className="py-3">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium bg-${getStatusColor(order.status)}/20 text-${getStatusColor(order.status)}`}>
                        {getStatusText(order.status)}
                      </span>
                    </td>
                  </tr>
                )))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Pending Quotes */}
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
          <div className="space-y-3">
            {[
              { customer: 'Công ty ABC', product: 'Điều hòa Daikin', date: '2 giờ trước' },
              { customer: 'Nguyễn Văn B', product: 'Robot hút bụi Ecovacs', date: '5 giờ trước' },
              { customer: 'Công ty XYZ', product: 'Máy lọc không khí', date: '1 ngày trước' },
            ].map((quote, index) => (
              <div key={index} className="flex items-center gap-3 p-3 bg-secondary rounded-lg">
                <FileText size={20} className="text-primary" />
                <div className="flex-1 min-w-0">
                  <p className="font-medium">{quote.customer}</p>
                  <p className="text-sm text-tertiary truncate">{quote.product}</p>
                </div>
                <span className="text-xs text-tertiary">{quote.date}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default DashboardPage;
