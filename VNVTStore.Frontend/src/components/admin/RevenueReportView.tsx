import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import {
  TrendingUp,
  TrendingDown,
  DollarSign,
  ShoppingBag,
  CreditCard,
  Download,
  Calendar,
  Layers,
  CheckCircle2,
  XCircle,
  Clock,
  Tag,
  Percent,
  RefreshCw,
  BarChart3
} from 'lucide-react';
import {
  ResponsiveContainer,
  ComposedChart,
  Area,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  PieChart,
  Pie,
  Cell
} from 'recharts';
import { dashboardService, type RevenueReportDto } from '@/services/dashboardService';
import { formatCurrency, formatDate } from '@/utils/format';
import { Button, Badge } from '@/components/ui';

const PIE_COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ec4899', '#8b5cf6', '#06b6d4'];

export const RevenueReportView: React.FC = () => {
  // Preset selector
  const [selectedPreset, setSelectedPreset] = useState<'7d' | '30d' | 'this_month' | 'this_quarter' | 'this_year' | 'custom'>('30d');
  const [groupBy, setGroupBy] = useState<'day' | 'month'>('day');

  // Custom date state
  const [startDate, setStartDate] = useState(() => {
    const d = new Date();
    d.setDate(d.getDate() - 30);
    return d.toISOString().split('T')[0];
  });
  const [endDate, setEndDate] = useState(() => new Date().toISOString().split('T')[0]);

  const handlePresetChange = (preset: '7d' | '30d' | 'this_month' | 'this_quarter' | 'this_year' | 'custom') => {
    setSelectedPreset(preset);
    const end = new Date();
    const start = new Date();

    if (preset === '7d') {
      start.setDate(end.getDate() - 7);
      setGroupBy('day');
    } else if (preset === '30d') {
      start.setDate(end.getDate() - 30);
      setGroupBy('day');
    } else if (preset === 'this_month') {
      start.setDate(1);
      setGroupBy('day');
    } else if (preset === 'this_quarter') {
      const currentQuarter = Math.floor(end.getMonth() / 3);
      start.setMonth(currentQuarter * 3, 1);
      setGroupBy('month');
    } else if (preset === 'this_year') {
      start.setMonth(0, 1);
      setGroupBy('month');
    }

    if (preset !== 'custom') {
      setStartDate(start.toISOString().split('T')[0]);
      setEndDate(end.toISOString().split('T')[0]);
    }
  };

  const { data: reportRes, isLoading, refetch, isFetching } = useQuery({
    queryKey: ['revenue-report', startDate, endDate, groupBy],
    queryFn: () => dashboardService.getRevenueReport({ startDate, endDate, groupBy }),
    staleTime: 60000
  });

  const report: RevenueReportDto = reportRes?.data || {
    summary: {
      totalRevenue: 0,
      totalOrders: 0,
      averageOrderValue: 0,
      completedOrders: 0,
      cancelledOrders: 0,
      pendingOrders: 0,
      totalDiscount: 0,
      revenueChangeVsPreviousPeriod: 0,
      ordersChangeVsPreviousPeriod: 0
    },
    timeline: [],
    topProducts: [],
    paymentMethods: [],
    orderStatuses: []
  };

  const { summary, timeline, topProducts, paymentMethods, orderStatuses } = report;

  // Export to CSV function
  const handleExportCsv = () => {
    if (!timeline || timeline.length === 0) return;

    let csvContent = 'data:text/csv;charset=utf-8,';
    csvContent += 'Bao cao doanh thu tu ' + startDate + ' den ' + endDate + '\n\n';
    csvContent += 'Thoi gian,Doanh thu (VND),Tong don hang,Don thanh cong,Don bi huy\n';

    timeline.forEach(row => {
      csvContent += `${row.label},${row.revenue},${row.totalOrders},${row.completedOrders},${row.cancelledOrders}\n`;
    });

    csvContent += `\nTong doanh thu,${summary.totalRevenue}\n`;
    csvContent += `Tong don hang,${summary.totalOrders}\n`;
    csvContent += `Gia tri trung binh don,${Math.round(summary.averageOrderValue)}\n`;

    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', `Bao_cao_doanh_thu_${startDate}_${endDate}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <div className="space-y-6">
      {/* Controls Bar */}
      <div className="bg-bg-primary rounded-xl p-4 border border-border shadow-sm flex flex-wrap items-center justify-between gap-4">
        {/* Presets */}
        <div className="flex flex-wrap items-center gap-1.5">
          <span className="text-xs font-semibold text-secondary mr-2 flex items-center gap-1">
            <Calendar size={14} /> Kỳ báo cáo:
          </span>
          {[
            { id: '7d', label: '7 ngày qua' },
            { id: '30d', label: '30 ngày qua' },
            { id: 'this_month', label: 'Tháng này' },
            { id: 'this_quarter', label: 'Quý này' },
            { id: 'this_year', label: 'Năm nay' },
            { id: 'custom', label: 'Tùy chọn' }
          ].map(p => (
            <button
              key={p.id}
              onClick={() => handlePresetChange(p.id as any)}
              className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors ${
                selectedPreset === p.id
                  ? 'bg-primary text-white shadow-sm'
                  : 'bg-bg-secondary text-secondary hover:bg-secondary'
              }`}
            >
              {p.label}
            </button>
          ))}
        </div>

        {/* Date Inputs & Actions */}
        <div className="flex items-center gap-3">
          {selectedPreset === 'custom' && (
            <div className="flex items-center gap-2 text-xs">
              <input
                type="date"
                value={startDate}
                onChange={e => setStartDate(e.target.value)}
                className="px-2.5 py-1.5 bg-bg-secondary rounded-lg border border-border text-xs focus:outline-none focus:border-primary"
              />
              <span className="text-secondary">-</span>
              <input
                type="date"
                value={endDate}
                onChange={e => setEndDate(e.target.value)}
                className="px-2.5 py-1.5 bg-bg-secondary rounded-lg border border-border text-xs focus:outline-none focus:border-primary"
              />
            </div>
          )}

          {/* Group By selector */}
          <div className="flex items-center bg-bg-secondary rounded-lg p-0.5 border border-border text-xs">
            <button
              onClick={() => setGroupBy('day')}
              className={`px-2.5 py-1 rounded-md transition-colors ${
                groupBy === 'day' ? 'bg-primary text-white font-medium' : 'text-secondary'
              }`}
            >
              Ngày
            </button>
            <button
              onClick={() => setGroupBy('month')}
              className={`px-2.5 py-1 rounded-md transition-colors ${
                groupBy === 'month' ? 'bg-primary text-white font-medium' : 'text-secondary'
              }`}
            >
              Tháng
            </button>
          </div>

          <Button
            size="sm"
            variant="outline"
            onClick={handleExportCsv}
            className="flex items-center gap-1.5 text-xs"
            title="Xuất file CSV"
          >
            <Download size={14} />
            Xuất CSV
          </Button>

          <button
            onClick={() => refetch()}
            className="p-2 hover:bg-secondary rounded-lg text-secondary hover:text-primary transition-colors"
            title="Làm mới"
          >
            <RefreshCw size={15} className={isFetching ? 'animate-spin' : ''} />
          </button>
        </div>
      </div>

      {/* KPI Metric Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
        {/* Total Revenue */}
        <div className="bg-bg-primary rounded-xl p-5 border border-border shadow-sm">
          <div className="flex items-center justify-between mb-2">
            <span className="text-xs font-medium text-secondary">Tổng doanh thu</span>
            <div className="w-8 h-8 rounded-lg bg-emerald-500/10 text-emerald-500 flex items-center justify-center">
              <DollarSign size={18} />
            </div>
          </div>
          <p className="text-xl font-bold text-text-primary">{formatCurrency(summary.totalRevenue)}</p>
          <div className="mt-2 flex items-center gap-1 text-xs">
            {summary.revenueChangeVsPreviousPeriod >= 0 ? (
              <span className="text-success flex items-center font-medium">
                <TrendingUp size={13} className="mr-0.5" />+{summary.revenueChangeVsPreviousPeriod}%
              </span>
            ) : (
              <span className="text-error flex items-center font-medium">
                <TrendingDown size={13} className="mr-0.5" />{summary.revenueChangeVsPreviousPeriod}%
              </span>
            )}
            <span className="text-tertiary">so với kỳ trước</span>
          </div>
        </div>

        {/* Total Orders */}
        <div className="bg-bg-primary rounded-xl p-5 border border-border shadow-sm">
          <div className="flex items-center justify-between mb-2">
            <span className="text-xs font-medium text-secondary">Tổng đơn hàng</span>
            <div className="w-8 h-8 rounded-lg bg-blue-500/10 text-blue-500 flex items-center justify-center">
              <ShoppingBag size={18} />
            </div>
          </div>
          <p className="text-xl font-bold text-text-primary">{summary.totalOrders}</p>
          <div className="mt-2 flex items-center gap-1 text-xs">
            {summary.ordersChangeVsPreviousPeriod >= 0 ? (
              <span className="text-success flex items-center font-medium">
                <TrendingUp size={13} className="mr-0.5" />+{summary.ordersChangeVsPreviousPeriod}%
              </span>
            ) : (
              <span className="text-error flex items-center font-medium">
                <TrendingDown size={13} className="mr-0.5" />{summary.ordersChangeVsPreviousPeriod}%
              </span>
            )}
            <span className="text-tertiary">so với kỳ trước</span>
          </div>
        </div>

        {/* Average Order Value (AOV) */}
        <div className="bg-bg-primary rounded-xl p-5 border border-border shadow-sm">
          <div className="flex items-center justify-between mb-2">
            <span className="text-xs font-medium text-secondary">Giá trị TB/Đơn (AOV)</span>
            <div className="w-8 h-8 rounded-lg bg-purple-500/10 text-purple-500 flex items-center justify-center">
              <BarChart3 size={18} />
            </div>
          </div>
          <p className="text-xl font-bold text-text-primary">{formatCurrency(summary.averageOrderValue)}</p>
          <p className="mt-2 text-xs text-tertiary">
            Tính trên {summary.completedOrders} đơn hoàn thành
          </p>
        </div>

        {/* Delivery Completion Rate */}
        <div className="bg-bg-primary rounded-xl p-5 border border-border shadow-sm">
          <div className="flex items-center justify-between mb-2">
            <span className="text-xs font-medium text-secondary">Tỉ lệ hoàn thành</span>
            <div className="w-8 h-8 rounded-lg bg-teal-500/10 text-teal-500 flex items-center justify-center">
              <CheckCircle2 size={18} />
            </div>
          </div>
          <p className="text-xl font-bold text-text-primary">
            {summary.totalOrders > 0
              ? `${Math.round((summary.completedOrders / summary.totalOrders) * 100)}%`
              : '0%'}
          </p>
          <div className="mt-2 flex items-center gap-2 text-xs text-secondary">
            <span className="text-emerald-500 font-medium">{summary.completedOrders} xong</span>
            <span>•</span>
            <span className="text-rose-500 font-medium">{summary.cancelledOrders} hủy</span>
          </div>
        </div>

        {/* Discounts / Promotions */}
        <div className="bg-bg-primary rounded-xl p-5 border border-border shadow-sm">
          <div className="flex items-center justify-between mb-2">
            <span className="text-xs font-medium text-secondary">Chiết khấu / Giảm giá</span>
            <div className="w-8 h-8 rounded-lg bg-amber-500/10 text-amber-500 flex items-center justify-center">
              <Tag size={18} />
            </div>
          </div>
          <p className="text-xl font-bold text-text-primary">{formatCurrency(summary.totalDiscount)}</p>
          <p className="mt-2 text-xs text-tertiary">Từ mã khuyến mãi & coupon</p>
        </div>
      </div>

      {/* Main Charts: Revenue & Order Count Composed Chart */}
      <div className="bg-bg-primary rounded-xl p-6 border border-border shadow-sm">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h3 className="font-bold text-base text-text-primary">Biểu đồ Doanh thu & Lượng đơn hàng</h3>
            <p className="text-xs text-secondary">
              Xu hướng tăng trưởng từ {formatDate(startDate)} đến {formatDate(endDate)}
            </p>
          </div>
          <div className="flex items-center gap-4 text-xs">
            <span className="flex items-center gap-1.5">
              <span className="w-3 h-3 rounded-full bg-indigo-600 inline-block" /> Doanh thu (VND)
            </span>
            <span className="flex items-center gap-1.5">
              <span className="w-3 h-3 rounded-full bg-emerald-400 inline-block" /> Đơn hoàn thành
            </span>
          </div>
        </div>

        <div className="h-80 w-full">
          {timeline.length === 0 ? (
            <div className="w-full h-full flex items-center justify-center text-secondary text-sm">
              Chưa có dữ liệu trong khoảng thời gian này
            </div>
          ) : (
            <ResponsiveContainer width="100%" height="100%">
              <ComposedChart data={timeline} margin={{ top: 10, right: 10, left: 10, bottom: 0 }}>
                <defs>
                  <linearGradient id="revenueGrad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#4f46e5" stopOpacity={0.4} />
                    <stop offset="95%" stopColor="#4f46e5" stopOpacity={0.0} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" vertical={false} opacity={0.2} />
                <XAxis dataKey="label" tickLine={false} tick={{ fontSize: 11, fill: '#64748b' }} dy={8} />
                <YAxis
                  yAxisId="left"
                  tickLine={false}
                  tick={{ fontSize: 11, fill: '#64748b' }}
                  tickFormatter={val => `${val / 1000000}M`}
                  width={55}
                />
                <YAxis
                  yAxisId="right"
                  orientation="right"
                  tickLine={false}
                  tick={{ fontSize: 11, fill: '#64748b' }}
                  width={30}
                />
                <Tooltip
                  formatter={(value: any, name: string) => [
                    name === 'revenue' ? formatCurrency(Number(value)) : value,
                    name === 'revenue' ? 'Doanh thu' : name === 'completedOrders' ? 'Đơn thành công' : 'Tổng đơn'
                  ]}
                  contentStyle={{ borderRadius: '12px', border: 'none', boxShadow: '0 8px 16px -2px rgba(0,0,0,0.1)' }}
                />
                <Area
                  yAxisId="left"
                  type="monotone"
                  dataKey="revenue"
                  stroke="#4f46e5"
                  strokeWidth={2.5}
                  fill="url(#revenueGrad)"
                />
                <Bar
                  yAxisId="right"
                  dataKey="completedOrders"
                  fill="#10b981"
                  radius={[4, 4, 0, 0]}
                  barSize={16}
                />
              </ComposedChart>
            </ResponsiveContainer>
          )}
        </div>
      </div>

      {/* Distribution Row: Payment Methods & Order Statuses */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Payment Methods */}
        <div className="bg-bg-primary rounded-xl p-6 border border-border shadow-sm">
          <h3 className="font-bold text-base text-text-primary mb-1">Phương thức thanh toán</h3>
          <p className="text-xs text-secondary mb-4">Cơ cấu doanh thu theo cổng thanh toán</p>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 items-center">
            <div className="h-52">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={paymentMethods}
                    dataKey="revenue"
                    nameKey="method"
                    cx="50%"
                    cy="50%"
                    innerRadius={45}
                    outerRadius={75}
                    paddingAngle={3}
                  >
                    {paymentMethods.map((_, index) => (
                      <Cell key={`cell-${index}`} fill={PIE_COLORS[index % PIE_COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip formatter={(val: any) => formatCurrency(Number(val))} />
                </PieChart>
              </ResponsiveContainer>
            </div>
            <div className="space-y-2.5">
              {paymentMethods.map((item, idx) => (
                <div key={item.method} className="flex items-center justify-between text-xs">
                  <div className="flex items-center gap-2">
                    <span
                      className="w-3 h-3 rounded-full flex-shrink-0"
                      style={{ backgroundColor: PIE_COLORS[idx % PIE_COLORS.length] }}
                    />
                    <span className="font-medium text-text-primary">{item.method}</span>
                  </div>
                  <div className="text-right">
                    <p className="font-semibold">{formatCurrency(item.revenue)}</p>
                    <p className="text-tertiary">{item.orderCount} đơn ({item.percentage}%)</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Top Selling Products in Period */}
        <div className="bg-bg-primary rounded-xl p-6 border border-border shadow-sm">
          <h3 className="font-bold text-base text-text-primary mb-1">Top sản phẩm doanh số cao</h3>
          <p className="text-xs text-secondary mb-4">Sản phẩm đóng góp doanh thu lớn nhất trong kỳ</p>
          {topProducts.length === 0 ? (
            <p className="text-sm text-secondary py-8 text-center">Chưa có dữ liệu sản phẩm bán ra</p>
          ) : (
            <div className="space-y-3">
              {topProducts.slice(0, 5).map((p, idx) => (
                <div key={idx} className="flex items-center justify-between text-sm py-1.5 border-b border-border/50 last:border-0">
                  <div className="flex items-center gap-3 min-w-0 pr-2">
                    <span className="w-6 h-6 rounded-lg bg-bg-secondary text-primary font-bold text-xs flex items-center justify-center flex-shrink-0">
                      {idx + 1}
                    </span>
                    <div className="truncate">
                      <p className="font-medium text-text-primary truncate">{p.name}</p>
                      <p className="text-xs text-tertiary">Đã bán: {p.sales} sản phẩm</p>
                    </div>
                  </div>
                  <span className="font-semibold text-emerald-600 flex-shrink-0">
                    {formatCurrency(p.revenue)}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Detailed Timeline Table */}
      <div className="bg-bg-primary rounded-xl border border-border shadow-sm overflow-hidden">
        <div className="p-4 border-b border-border flex items-center justify-between">
          <h3 className="font-bold text-base text-text-primary">Bảng kê chi tiết theo dòng thời gian</h3>
          <span className="text-xs text-tertiary">{timeline.length} mốc dữ liệu</span>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="bg-bg-secondary text-xs text-tertiary uppercase">
              <tr>
                <th className="py-3 px-4 font-semibold">Thời gian</th>
                <th className="py-3 px-4 font-semibold text-right">Doanh thu</th>
                <th className="py-3 px-4 font-semibold text-center">Tổng đơn</th>
                <th className="py-3 px-4 font-semibold text-center">Đơn thành công</th>
                <th className="py-3 px-4 font-semibold text-center">Đơn hủy</th>
                <th className="py-3 px-4 font-semibold text-right">Tỉ lệ hoàn thành</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {timeline.map((row, idx) => {
                const rate = row.totalOrders > 0 ? Math.round((row.completedOrders / row.totalOrders) * 100) : 0;
                return (
                  <tr key={idx} className="hover:bg-bg-secondary/40 transition-colors">
                    <td className="py-3 px-4 font-medium text-text-primary">{row.label}</td>
                    <td className="py-3 px-4 text-right font-semibold text-emerald-600">
                      {formatCurrency(row.revenue)}
                    </td>
                    <td className="py-3 px-4 text-center">{row.totalOrders}</td>
                    <td className="py-3 px-4 text-center font-medium text-teal-600">{row.completedOrders}</td>
                    <td className="py-3 px-4 text-center font-medium text-rose-500">{row.cancelledOrders}</td>
                    <td className="py-3 px-4 text-right">
                      <span className={`inline-block px-2 py-0.5 rounded-full text-xs font-semibold ${
                        rate >= 80 ? 'bg-emerald-500/10 text-emerald-600' : rate >= 50 ? 'bg-amber-500/10 text-amber-600' : 'bg-rose-500/10 text-rose-600'
                      }`}>
                        {rate}%
                      </span>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default RevenueReportView;
