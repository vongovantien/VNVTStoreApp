import { memo, type ReactNode, useMemo } from 'react';
import { cn } from '@/utils/cn';

// ============ Badge Types ============
export type BadgeVariant = 'solid' | 'soft' | 'outline';
export type BadgeColor = 'default' | 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info';
export type BadgeSize = 'sm' | 'md' | 'lg';

// ============ Badge Props Interface ============
export interface BadgeProps {
  /** Badge variant */
  variant?: BadgeVariant;
  /** Badge color */
  color?: BadgeColor;
  /** Badge size */
  size?: BadgeSize;
  /** Rounded pill style */
  rounded?: boolean;
  /** Dot indicator (no text) */
  dot?: boolean;
  /** Left icon */
  leftIcon?: ReactNode;
  /** Right icon */
  rightIcon?: ReactNode;
  /** Closable */
  closable?: boolean;
  /** On close handler */
  onClose?: () => void;
  /** Custom className */
  className?: string;
  /** Children */
  children?: ReactNode;
}

// ============ Color Styles ============
const colorStyles: Record<BadgeColor, Record<BadgeVariant, string>> = {
  default: {
    solid: 'bg-gray-500 text-white',
    soft: 'bg-gray-100 dark:bg-slate-800 text-gray-700 dark:text-slate-200',
    outline: 'border border-gray-300 dark:border-slate-500 text-gray-700 dark:text-slate-200',
  },
  primary: {
    solid: 'bg-primary text-white',
    soft: 'bg-primary/10 text-primary',
    outline: 'border border-primary text-primary',
  },
  secondary: {
    solid: 'bg-secondary text-white',
    soft: 'bg-secondary/10 text-secondary',
    outline: 'border border-secondary text-secondary',
  },
  success: {
    solid: 'bg-success text-white',
    soft: 'bg-success/10 text-success',
    outline: 'border border-success text-success',
  },
  warning: {
    solid: 'bg-warning text-white',
    soft: 'bg-warning/10 text-warning',
    outline: 'border border-warning text-warning',
  },
  error: {
    solid: 'bg-error text-white',
    soft: 'bg-error/10 text-error',
    outline: 'border border-error text-error',
  },
  info: {
    solid: 'bg-info text-white',
    soft: 'bg-info/10 text-info',
    outline: 'border border-info text-info',
  },
};

const sizeStyles: Record<BadgeSize, string> = {
  sm: 'px-1.5 py-0.5 text-xs',
  md: 'px-2 py-0.5 text-xs',
  lg: 'px-2.5 py-1 text-sm',
};

// ============ Badge Component ============
export const Badge = memo(
  ({
    variant = 'solid',
    color = 'default',
    size = 'md',
    rounded = true,
    dot = false,
    leftIcon,
    rightIcon,
    closable = false,
    onClose,
    className,
    children,
  }: BadgeProps) => {
    const badgeClasses = useMemo(
      () =>
        cn(
          // Base
          'inline-flex items-center gap-1 font-semibold transition-all',
          // Color & variant
          colorStyles[color][variant],
          // Size
          !dot && sizeStyles[size],
          // Rounded
          rounded ? 'rounded-full' : 'rounded',
          // Dot
          dot && 'w-2 h-2 p-0',
          // Custom
          className
        ),
      [variant, color, size, rounded, dot, className]
    );

    if (dot) {
      return <span className={badgeClasses} />;
    }

    return (
      <span className={badgeClasses}>
        {leftIcon}
        {children}
        {rightIcon}
        {closable && (
          <button
            onClick={onClose}
            className="ml-0.5 hover:opacity-70 transition-opacity"
            aria-label="Close"
          >
            ✕
          </button>
        )}
      </span>
    );
  }
);

Badge.displayName = 'Badge';

export default Badge;
