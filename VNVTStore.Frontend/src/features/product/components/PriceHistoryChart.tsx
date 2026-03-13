import React, { useState } from 'react';
import { AreaChart, Area, XAxis, YAxis, Tooltip, ResponsiveContainer } from 'recharts';
import { TrendingUp, TrendingDown } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '@/components/ui';
import { usePriceHistory } from '../hooks/usePriceHistory';
import { formatCurrency } from '@/utils/format';

interface PriceHistoryChartProps {
  productId: string;
  currentPrice: number;
}

const PriceHistoryChart: React.FC<PriceHistoryChartProps> = ({ productId, currentPrice }) => {
  const { t } = useTranslation();
  const { data, isLoading, error } = usePriceHistory(productId, currentPrice);
  const [isOpen, setIsOpen] = useState(false);

  if (isLoading) return null; // Or a mini skeleton
  if (error || !data) return null;

  const priceDiff = data.currentPrice - data.averagePrice;
  const isGoodPrice = priceDiff <= 0;

  return (
    <div className="relative inline-block ml-2">
      <Button
        variant="ghost"
        size="sm"
        className={`h-6 px-2 text-xs gap-1 rounded-full ${
          isGoodPrice 
            ? 'bg-green-50 text-green-700 hover:bg-green-100 hover:text-green-800' 
            : 'bg-orange-50 text-orange-700 hover:bg-orange-100 hover:text-orange-800'
        }`}
        onClick={() => setIsOpen(!isOpen)}
      >
        {isGoodPrice ? <TrendingDown size={14} /> : <TrendingUp size={14} />}
        {isGoodPrice ? t('product.goodPrice', 'Giá tốt') : t('product.risingPrice', 'Đang tăng')}
      </Button>

      <AnimatePresence>
        {isOpen && (
          <>
            <div 
                className="fixed inset-0 z-40" 
                onClick={() => setIsOpen(false)} 
            />
            <motion.div
              initial={{ opacity: 0, scale: 0.95, y: 10 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.95, y: 10 }}
              className="absolute top-full left-0 mt-2 w-[320px] bg-white dark:bg-slate-800 rounded-xl shadow-xl border border-slate-100 dark:border-slate-700 p-4 z-50 origin-top-left"
            >
              <div className="flex justify-between items-start mb-4">
                <div>
                  <h4 className="font-semibold text-slate-900 dark:text-white">
                      {t('product.priceHistory', 'Lịch sử giá')}
                  </h4>
                  <p className="text-xs text-slate-500">6 tháng gần nhất</p>
                </div>
                <div className={`text-right ${isGoodPrice ? 'text-green-600' : 'text-orange-600'}`}>
                    <span className="text-lg font-bold">{formatCurrency(data.currentPrice)}</span>
                </div>
              </div>

              <div className="h-[180px] w-full -ml-4">
                <ResponsiveContainer width="100%" height="100%">
                  <AreaChart data={data.history}>
                    <defs>
                      <linearGradient id="colorPrice" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor={isGoodPrice ? "#22c55e" : "#f97316"} stopOpacity={0.1}/>
                        <stop offset="95%" stopColor={isGoodPrice ? "#22c55e" : "#f97316"} stopOpacity={0}/>
                      </linearGradient>
                    </defs>
                    <XAxis 
                        dataKey="date" 
                        hide 
                    />
                    <YAxis 
                        hide 
                        domain={['dataMin - 1000', 'dataMax + 1000']} 
                    />
                    <Tooltip 
                        contentStyle={{ 
                            backgroundColor: 'rgba(255, 255, 255, 0.9)', 
                            borderRadius: '8px', 
                            border: 'none', 
                            boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' 
                        }}
                        formatter={(value: number | undefined) => [formatCurrency(value || 0), 'Giá']}
                    />
                    <Area 
                        type="monotone" 
                        dataKey="price" 
                        stroke={isGoodPrice ? "#22c55e" : "#f97316"} 
                        strokeWidth={2}
                        fillOpacity={1} 
                        fill="url(#colorPrice)" 
                    />
                  </AreaChart>
                </ResponsiveContainer>
              </div>

              <div className="mt-2 pt-3 border-t border-slate-100 dark:border-slate-700 text-xs text-slate-500 flex justify-between">
                <span>Thấp nhất: <span className="font-medium text-slate-700 dark:text-slate-300">{formatCurrency(data.lowestPrice)}</span></span>
                <span>Cao nhất: <span className="font-medium text-slate-700 dark:text-slate-300">{formatCurrency(data.highestPrice)}</span></span>
              </div>
            </motion.div>
          </>
        )}
      </AnimatePresence>
    </div>
  );
};

import { ErrorBoundary } from '@/core/errors/ErrorBoundary';
import { WidgetError } from '@/components/shared/Fallbacks/WidgetError';

const PriceHistoryChartWrapper: React.FC<PriceHistoryChartProps> = (props) => (
  <ErrorBoundary fallback={WidgetError} context={{ title: 'Lịch sử giá' }}>
    <PriceHistoryChart {...props} />
  </ErrorBoundary>
);

export default PriceHistoryChartWrapper;
