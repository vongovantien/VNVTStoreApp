import React, { memo, useMemo } from 'react';
import type { ReactNode } from 'react';
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
    solid: 'bg-slate-500 text-white',
    soft: 'bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300',
    outline: 'border border-slate-300 dark:border-slate-600 text-slate-700 dark:text-slate-300',
  },
  success: {
    solid: 'bg-emerald-500 text-white',
    soft: 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400',
    outline: 'border border-emerald-500 text-emerald-600 dark:text-emerald-400',
  },
  warning: {
    solid: 'bg-amber-500 text-white',
    soft: 'bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-400',
    outline: 'border border-amber-500 text-amber-600 dark:text-amber-400',
  },
  error: {
    solid: 'bg-rose-500 text-white',
    soft: 'bg-rose-100 dark:bg-rose-900/30 text-rose-700 dark:text-rose-400',
    outline: 'border border-rose-500 text-rose-600 dark:text-rose-400',
  },
  info: {
    solid: 'bg-blue-500 text-white',
    soft: 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400',
    outline: 'border border-blue-500 text-blue-600 dark:text-blue-400',
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
