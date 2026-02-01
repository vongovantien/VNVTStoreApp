import React from 'react';
import { cn } from '@/utils/cn';

interface TableSkeletonProps {
  columns: number;
  rows?: number;
  hasSelection?: boolean;
  hasActions?: boolean;
}

export const TableSkeleton: React.FC<TableSkeletonProps> = ({
  columns,
  rows = 5,
  hasSelection = true,
  hasActions = true,
}) => {
  return (
    <div className="w-full animate-pulse">
      {/* Header Skeleton */}
      <div className="bg-secondary border-b border-border flex items-center px-4 py-3">
        {hasSelection && (
          <div className="w-4 h-4 bg-gray-200 dark:bg-slate-700 rounded mr-4" />
        )}
        <div className="flex-1 flex gap-4">
          {[...Array(columns)].map((_, i) => (
            <div
              key={`h-${i}`}
              className={cn(
                "h-4 bg-gray-200 dark:bg-slate-700 rounded",
                i === 0 ? "w-1/4" : "flex-1"
              )}
            />
          ))}
        </div>
        {hasActions && <div className="w-[100px] h-4 bg-gray-200 dark:bg-slate-700 rounded ml-4" />}
      </div>

      {/* Rows Skeleton */}
      <div className="divide-y divide-border">
        {[...Array(rows)].map((_, rowIndex) => (
          <div key={`r-${rowIndex}`} className="flex items-center px-4 py-4">
            {hasSelection && (
              <div className="w-4 h-4 bg-gray-100 dark:bg-slate-800 rounded mr-4" />
            )}
            <div className="flex-1 flex gap-4">
              {[...Array(columns)].map((_, colIndex) => (
                <div
                  key={`c-${colIndex}`}
                  className={cn(
                    "h-4 bg-gray-100 dark:bg-slate-800 rounded",
                    colIndex === 0 ? "w-1/3" : "flex-1"
                  )}
                />
              ))}
            </div>
            {hasActions && (
              <div className="w-[100px] flex justify-center gap-2 ml-4">
                <div className="w-8 h-8 bg-gray-50 dark:bg-slate-800 rounded-md" />
                <div className="w-8 h-8 bg-gray-50 dark:bg-slate-800 rounded-md" />
                <div className="w-8 h-8 bg-gray-50 dark:bg-slate-800 rounded-md" />
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

export default TableSkeleton;
