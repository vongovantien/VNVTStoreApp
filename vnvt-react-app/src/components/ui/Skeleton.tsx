import { memo, useMemo } from 'react';
import { cn } from '@/utils/cn';

// ============ Skeleton Props Interface ============
export interface SkeletonProps {
  /** Width (CSS value or Tailwind class) */
  width?: string | number;
  /** Height (CSS value or Tailwind class) */
  height?: string | number;
  /** Circle shape */
  circle?: boolean;
  /** Custom className */
  className?: string;
  /** Number of lines (for text) */
  lines?: number;
  /** Animation type */
  animation?: 'pulse' | 'wave' | 'none';
}

// ============ Skeleton Component ============
export const Skeleton = memo(
  ({
    width,
    height,
    circle = false,
    className,
    lines = 1,
    animation = 'pulse',
  }: SkeletonProps) => {
    const baseClasses = useMemo(
      () =>
        cn(
          'bg-gray-200',
          animation === 'pulse' && 'animate-pulse',
          animation === 'wave' &&
            'relative overflow-hidden before:absolute before:inset-0 before:-translate-x-full before:animate-[shimmer_1.5s_infinite] before:bg-gradient-to-r before:from-transparent before:via-white/60 before:to-transparent',
          circle ? 'rounded-full' : 'rounded-lg',
          className
        ),
      [circle, animation, className]
    );

    const style = useMemo(
      () => ({
        width: typeof width === 'number' ? `${width}px` : width,
        height: typeof height === 'number' ? `${height}px` : height,
      }),
      [width, height]
    );

    if (lines > 1) {
      return (
        <div className="flex flex-col gap-2">
          {Array.from({ length: lines }).map((_, i) => (
            <div
              key={i}
              className={cn(baseClasses, i === lines - 1 && 'w-3/4')}
              style={{ height: height || 16 }}
            />
          ))}
        </div>
      );
    }

    return <div className={baseClasses} style={style} />;
  }
);

Skeleton.displayName = 'Skeleton';

// ============ Skeleton Presets ============
export const SkeletonText = memo(({ lines = 3 }: { lines?: number }) => (
  <Skeleton lines={lines} height={14} />
));

export const SkeletonAvatar = memo(
  ({ size = 40 }: { size?: number }) => <Skeleton circle width={size} height={size} />
);

export const SkeletonCard = memo(() => (
  <div className="bg-primary rounded-xl p-4 space-y-4">
    <Skeleton height={200} />
    <Skeleton height={20} width="60%" />
    <Skeleton height={14} lines={2} />
    <div className="flex gap-2">
      <Skeleton height={10} width={60} />
      <Skeleton height={10} width={80} />
    </div>
  </div>
));

SkeletonText.displayName = 'SkeletonText';
SkeletonAvatar.displayName = 'SkeletonAvatar';
SkeletonCard.displayName = 'SkeletonCard';

export default Skeleton;
