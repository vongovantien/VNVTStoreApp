import React, { memo, useMemo } from 'react';
import type { ReactNode } from 'react';
import { cn } from '@/utils/cn';

// ============ Card Types ============
export type CardVariant = 'default' | 'elevated' | 'outline' | 'glass';

// ============ Card Props Interface ============
export interface CardProps {
  /** Card variant style */
  variant?: CardVariant;
  /** Padding size */
  padding?: 'none' | 'sm' | 'md' | 'lg';
  /** Enable hover effect */
  hoverable?: boolean;
  /** Enable click effect */
  clickable?: boolean;
  /** Custom className */
  className?: string;
  /** Children elements */
  children: ReactNode;
  /** Click handler */
  onClick?: () => void;
}

export interface CardHeaderProps {
  /** Title */
  title?: ReactNode;
  /** Subtitle */
  subtitle?: ReactNode;
  /** Action element (right side) */
  action?: ReactNode;
  /** Custom className */
  className?: string;
  /** Children (alternative to title/subtitle) */
  children?: ReactNode;
}

export interface CardBodyProps {
  /** Custom className */
  className?: string;
  /** Children elements */
  children: React.ReactNode;
}

export interface CardFooterProps {
  /** Custom className */
  className?: string;
  /** Children elements */
  children: ReactNode;
}

// ============ Style Maps ============
const variantStyles: Record<CardVariant, string> = {
  default: 'bg-bg-primary border border-border shadow-sm',
  elevated: 'bg-bg-primary shadow-lg',
  outline: 'bg-transparent border-2 border-border',
  glass: 'bg-bg-primary/70 backdrop-blur-xl border border-border/40 shadow-lg',
};

const paddingStyles: Record<'none' | 'sm' | 'md' | 'lg', string> = {
  none: '',
  sm: 'p-3',
  md: 'p-4',
  lg: 'p-6',
};

// ============ Card Component ============
export const Card = memo(
  ({
    variant = 'default',
    padding = 'none',
    hoverable = false,
    clickable = false,
    className,
    children,
    onClick,
  }: CardProps) => {
    const cardClasses = useMemo(
      () =>
        cn(
          // Base
          'rounded-xl overflow-hidden transition-all duration-300',
          // Variant
          variantStyles[variant],
          // Padding
          paddingStyles[padding],
          // Hover effect
          hoverable && 'hover:-translate-y-1 hover:shadow-xl',
          // Clickable
          clickable && 'cursor-pointer active:scale-[0.98]',
          // Custom
          className
        ),
      [variant, padding, hoverable, clickable, className]
    );

    return (
      <div className={cardClasses} onClick={onClick}>
        {children}
      </div>
    );
  }
);

// ============ Card Header ============
export const CardHeader = memo(
  ({ title, subtitle, action, className, children }: CardHeaderProps) => {
    if (children) {
      return <div className={cn('px-4 py-3 border-b border-border', className)}>{children}</div>;
    }

    return (
      <div className={cn('px-4 py-3 border-b border-border flex items-center justify-between', className)}>
        <div>
          {title && <h3 className="font-semibold text-text-primary">{title}</h3>}
          {subtitle && <p className="text-sm text-text-secondary mt-0.5">{subtitle}</p>}
        </div>
        {action && <div>{action}</div>}
      </div>
    );
  }
);

// ============ Card Body ============
export const CardBody = memo(({ className, children }: CardBodyProps) => (
  <div className={cn('p-4', className)}>{children}</div>
));

// ============ Card Footer ============
export const CardFooter = memo(({ className, children }: CardFooterProps) => (
  <div className={cn('px-4 py-3 border-t border-border bg-bg-secondary/50', className)}>
    {children}
  </div>
));

Card.displayName = 'Card';
CardHeader.displayName = 'CardHeader';
CardBody.displayName = 'CardBody';
CardFooter.displayName = 'CardFooter';

export default Card;
