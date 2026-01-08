import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';
import { formatCurrency } from '@/utils/format';

const data = [
  { name: 'T1', revenue: 40000000 },
  { name: 'T2', revenue: 30000000 },
  { name: 'T3', revenue: 20000000 },
  { name: 'T4', revenue: 27800000 },
  { name: 'T5', revenue: 18900000 },
  { name: 'T6', revenue: 23900000 },
  { name: 'T7', revenue: 34900000 },
  { name: 'T8', revenue: 20000000 },
  { name: 'T9', revenue: 27800000 },
  { name: 'T10', revenue: 18900000 },
  { name: 'T11', revenue: 23900000 },
  { name: 'T12', revenue: 34900000 },
];

export const RevenueChart = () => {
  return (
    <div className="w-full h-full">
      <ResponsiveContainer width="100%" height="100%">
        <AreaChart
          data={data}
          margin={{
            top: 10,
            right: 10,
            left: 0,
            bottom: 0,
          }}
        >
          <defs>
            <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
              <stop offset="5%" stopColor="#4f46e5" stopOpacity={0.8} />
              <stop offset="95%" stopColor="#4f46e5" stopOpacity={0} />
            </linearGradient>
          </defs>
          <CartesianGrid strokeDasharray="3 3" vertical={false} opacity={0.3} />
          <XAxis 
            dataKey="name" 
            axisLine={false} 
            tickLine={false} 
            tick={{ fontSize: 12, fill: '#64748b' }} 
            dy={10}
          />
          <YAxis 
            axisLine={false} 
            tickLine={false} 
            tick={{ fontSize: 12, fill: '#64748b' }} 
            tickFormatter={(value) => `${value / 1000000}M`}
            width={40}
          />
          <Tooltip 
            contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
            formatter={(value: number) => [formatCurrency(value), 'Doanh thu']}
          />
          <Area
            type="monotone"
            dataKey="revenue"
            stroke="#4f46e5"
            fillOpacity={1}
            fill="url(#colorRevenue)"
          />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
};
