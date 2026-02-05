import React from 'react';

import { cn } from '@/utils/cn';

export interface StatItem {
  label: string;
  value: number | string;
  icon: React.ReactNode;
  color?: 'blue' | 'rose' | 'emerald' | 'amber' | 'purple' | 'cyan' | 'indigo';
  loading?: boolean;
}

interface StatsCardsProps {
  stats: StatItem[];
  className?: string;
}

const colorMap = {
  blue: { bg: 'bg-blue-50', text: 'text-blue-600' },
  rose: { bg: 'bg-rose-50', text: 'text-rose-600' },
  emerald: { bg: 'bg-emerald-50', text: 'text-emerald-600' },
  amber: { bg: 'bg-amber-50', text: 'text-amber-600' },
  purple: { bg: 'bg-purple-50', text: 'text-purple-600' },
  cyan: { bg: 'bg-cyan-50', text: 'text-cyan-600' },
  indigo: { bg: 'bg-indigo-50', text: 'text-indigo-600' },
};

export const StatsCards = ({ stats, className }: StatsCardsProps) => {
  return (
    <div className={cn("grid grid-cols-1 md:grid-cols-3 gap-4", className)}>
      {stats.map((stat, index) => {
        const theme = stat.color && colorMap[stat.color] ? colorMap[stat.color] : colorMap.blue;
        
        return (
          <div key={index} className="bg-white dark:bg-slate-800 p-4 rounded-xl shadow-sm border border-slate-100 dark:border-slate-700 flex items-center gap-4 transition-all hover:shadow-md">
            <div className={cn("w-12 h-12 rounded-lg flex items-center justify-center transition-colors", theme.bg, theme.text)}>
              {stat.icon}
            </div>
            <div>
              <p className="text-sm text-slate-500 dark:text-slate-400">{stat.label}</p>
              {stat.loading ? (
                <div className="h-7 w-16 bg-slate-100 dark:bg-slate-700 animate-pulse rounded mt-1" />
              ) : (
                <p className="text-xl font-bold text-slate-800 dark:text-slate-100">{stat.value}</p>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
};
