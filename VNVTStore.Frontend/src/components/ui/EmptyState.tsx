import { memo } from 'react';
import type { ReactNode } from 'react';
import { cn } from '@/utils/cn';
import { Button } from './Button';

export interface EmptyStateProps {
  /** Icon element (e.g. a lucide icon) */
  icon?: ReactNode;
  title: string;
  description?: string;
  /** Primary action button label */
  actionLabel?: string;
  /** Primary action handler */
  onAction?: () => void;
  /** Secondary action label */
  secondaryLabel?: string;
  onSecondary?: () => void;
  className?: string;
  /** Size variant */
  size?: 'sm' | 'md' | 'lg';
}

const sizeMap = {
  sm: { icon: 'w-10 h-10 text-2xl', title: 'text-base', desc: 'text-sm', wrap: 'py-8' },
  md: { icon: 'w-14 h-14 text-3xl', title: 'text-lg', desc: 'text-sm', wrap: 'py-12' },
  lg: { icon: 'w-20 h-20 text-4xl', title: 'text-xl', desc: 'text-base', wrap: 'py-16' },
};

export const EmptyState = memo(({
  icon,
  title,
  description,
  actionLabel,
  onAction,
  secondaryLabel,
  onSecondary,
  className,
  size = 'md',
}: EmptyStateProps) => {
  const s = sizeMap[size];

  return (
    <div className={cn('flex flex-col items-center justify-center text-center px-4', s.wrap, className)}>
      {icon && (
        <div className={cn(
          'flex items-center justify-center rounded-2xl bg-bg-tertiary text-text-tertiary mb-4',
          s.icon,
        )}>
          {icon}
        </div>
      )}

      <h3 className={cn('font-semibold text-text-primary mb-1', s.title)}>{title}</h3>

      {description && (
        <p className={cn('text-text-secondary max-w-sm', s.desc)}>{description}</p>
      )}

      {(actionLabel || secondaryLabel) && (
        <div className="flex items-center gap-3 mt-5">
          {secondaryLabel && onSecondary && (
            <Button variant="outline" size="sm" onClick={onSecondary}>
              {secondaryLabel}
            </Button>
          )}
          {actionLabel && onAction && (
            <Button size="sm" onClick={onAction}>
              {actionLabel}
            </Button>
          )}
        </div>
      )}
    </div>
  );
});

EmptyState.displayName = 'EmptyState';
export default EmptyState;
