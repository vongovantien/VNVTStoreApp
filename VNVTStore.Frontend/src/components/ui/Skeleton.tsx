import { memo } from 'react';
import { cn } from '@/utils/cn';

// ============ Base Skeleton ============
export interface SkeletonProps {
  className?: string;
}

export const Skeleton = memo(({ className }: SkeletonProps) => (
  <div className={cn('relative overflow-hidden rounded-md bg-bg-tertiary', className)}>
    <div className="absolute inset-0 -translate-x-full animate-[shimmer_1.4s_infinite] bg-gradient-to-r from-transparent via-white/20 to-transparent" />
  </div>
));

Skeleton.displayName = 'Skeleton';

// ============ Stat Card Skeleton ============
export const StatCardSkeleton = memo(() => (
  <div className="bg-bg-primary rounded-xl p-6 border border-border shadow-sm">
    <div className="flex items-start justify-between">
      <div className="flex-1">
        <Skeleton className="h-4 w-24 mb-3" />
        <Skeleton className="h-7 w-32 mb-3" />
        <Skeleton className="h-4 w-20" />
      </div>
      <Skeleton className="w-12 h-12 rounded-xl flex-shrink-0" />
    </div>
  </div>
));

StatCardSkeleton.displayName = 'StatCardSkeleton';

// ============ Table Skeleton ============
interface TableSkeletonProps {
  rows?: number;
  cols?: number;
  showHeader?: boolean;
}

export const TableSkeleton = memo(({ rows = 6, cols = 5, showHeader = true }: TableSkeletonProps) => (
  <div className="w-full border border-border rounded-xl overflow-hidden bg-bg-primary">
    {showHeader && (
      <div className="flex items-center gap-4 px-4 py-3 border-b border-border bg-bg-secondary">
        {Array.from({ length: cols }).map((_, i) => (
          <Skeleton key={i} className={cn('h-4', i === 0 ? 'w-32' : i === cols - 1 ? 'w-16 ml-auto' : 'flex-1')} />
        ))}
      </div>
    )}
    <div className="divide-y divide-border">
      {Array.from({ length: rows }).map((_, rowIdx) => (
        <div key={rowIdx} className="flex items-center gap-4 px-4 py-3.5">
          {Array.from({ length: cols }).map((_, colIdx) => (
            <Skeleton
              key={colIdx}
              className={cn(
                'h-4',
                colIdx === 0 ? 'w-8 rounded-full' : colIdx === 1 ? 'w-40' : colIdx === cols - 1 ? 'w-20 ml-auto' : 'flex-1',
              )}
            />
          ))}
        </div>
      ))}
    </div>
  </div>
));

TableSkeleton.displayName = 'TableSkeleton';

// ============ Form Skeleton ============
interface FormSkeletonProps {
  fields?: number;
  showTitle?: boolean;
}

export const FormSkeleton = memo(({ fields = 6, showTitle = true }: FormSkeletonProps) => (
  <div className="space-y-5">
    {showTitle && (
      <div className="pb-3 border-b border-border">
        <Skeleton className="h-6 w-48 mb-1.5" />
        <Skeleton className="h-4 w-72" />
      </div>
    )}
    <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
      {Array.from({ length: fields }).map((_, i) => (
        <div key={i} className={cn('flex flex-col gap-1.5', i === fields - 1 && fields % 2 !== 0 && 'sm:col-span-2')}>
          <Skeleton className="h-4 w-24" />
          <Skeleton className="h-10 w-full" />
        </div>
      ))}
    </div>
    <div className="flex justify-end gap-3 pt-2">
      <Skeleton className="h-10 w-24" />
      <Skeleton className="h-10 w-28" />
    </div>
  </div>
));

FormSkeleton.displayName = 'FormSkeleton';

// ============ Product Card Skeleton ============
export const ProductCardSkeleton = memo(() => (
  <div className="rounded-xl border border-border bg-bg-primary overflow-hidden">
    <Skeleton className="aspect-[4/5] w-full rounded-none" />
    <div className="p-4 space-y-2.5">
      <Skeleton className="h-3 w-16" />
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-3/4" />
      <div className="flex gap-0.5 mt-1">
        {Array.from({ length: 5 }).map((_, i) => <Skeleton key={i} className="h-3 w-3 rounded-full" />)}
      </div>
      <Skeleton className="h-5 w-28 mt-1" />
    </div>
  </div>
));

ProductCardSkeleton.displayName = 'ProductCardSkeleton';

// ============ Legacy aliases for backward compatibility ============
export const SkeletonText = memo(({ lines = 3, className }: { lines?: number; className?: string }) => (
  <div className={cn('space-y-2', className)}>
    {Array.from({ length: lines }).map((_, i) => (
      <Skeleton key={i} className={cn('h-4', i === lines - 1 ? 'w-3/4' : 'w-full')} />
    ))}
  </div>
));
SkeletonText.displayName = 'SkeletonText';

export const SkeletonAvatar = memo(({ size = 40 }: { size?: number }) => (
  <Skeleton className="rounded-full flex-shrink-0" style={{ width: size, height: size } as React.CSSProperties} />
));
SkeletonAvatar.displayName = 'SkeletonAvatar';

export const SkeletonCard = memo(({ className }: { className?: string }) => (
  <div className={cn('rounded-xl border border-border bg-bg-primary p-4 space-y-3', className)}>
    <Skeleton className="h-40 rounded-lg" />
    <Skeleton className="h-4 w-3/4" />
    <Skeleton className="h-4 w-1/2" />
    <Skeleton className="h-8 w-full rounded-lg" />
  </div>
));
SkeletonCard.displayName = 'SkeletonCard';
