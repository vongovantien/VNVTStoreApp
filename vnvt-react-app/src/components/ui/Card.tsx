import { memo, type ReactNode, useMemo } from 'react';
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
  children: ReactNode;
}

export interface CardFooterProps {
  /** Custom className */
  className?: string;
  /** Children elements */
  children: ReactNode;
}

// ============ Style Maps ============
const variantStyles: Record<CardVariant, string> = {
  default: 'bg-primary border border-gray-100 shadow-sm',
  elevated: 'bg-primary shadow-lg',
  outline: 'bg-transparent border-2 border-gray-200',
  glass: 'bg-white/70 backdrop-blur-xl border border-white/30 shadow-lg',
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
      return <div className={cn('px-4 py-3 border-b border-gray-100', className)}>{children}</div>;
    }

    return (
      <div className={cn('px-4 py-3 border-b border-gray-100 flex items-center justify-between', className)}>
        <div>
          {title && <h3 className="font-semibold text-primary">{title}</h3>}
          {subtitle && <p className="text-sm text-secondary mt-0.5">{subtitle}</p>}
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
  <div className={cn('px-4 py-3 border-t border-gray-100 bg-secondary/30', className)}>
    {children}
  </div>
));

Card.displayName = 'Card';
CardHeader.displayName = 'CardHeader';
CardBody.displayName = 'CardBody';
CardFooter.displayName = 'CardFooter';

export default Card;
