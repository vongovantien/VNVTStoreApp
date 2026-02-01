import React, { memo, forwardRef, useMemo, type ButtonHTMLAttributes, type ReactNode } from 'react';
import { Loader2 } from 'lucide-react';
import { cn } from '@/utils/cn';

// ============ Button Variants & Sizes ============
export type ButtonVariant = 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger' | 'success' | 'link';
export type ButtonSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl';

// ============ Button Props Interface ============
export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  /** Button visual variant */
  variant?: ButtonVariant;
  /** Button size */
  size?: ButtonSize;
  /** Full width button */
  fullWidth?: boolean;
  /** Loading state */
  isLoading?: boolean;
  /** Loading text */
  loadingText?: string;
  /** Left icon */
  leftIcon?: ReactNode;
  /** Right icon */
  rightIcon?: ReactNode;
  /** Rounded style */
  rounded?: boolean;
  /** Custom className */
  className?: string;
  /** Children elements */
  children?: ReactNode;
}

// ============ Style Maps ============
const variantStyles: Record<ButtonVariant, string> = {
  primary: 'bg-accent text-accent-foreground hover:bg-accent-hover hover:shadow-lg hover:shadow-accent/30 active:opacity-90',
  secondary: 'bg-secondary text-white hover:bg-slate-600 active:bg-slate-700', // mapped to new secondary var
  outline: 'border-2 border-accent text-accent hover:bg-accent hover:text-accent-foreground',
  ghost: 'text-text-secondary hover:bg-bg-tertiary hover:text-accent',
  danger: 'bg-error text-white hover:opacity-90 active:opacity-100',
  success: 'bg-emerald-600 text-white hover:bg-emerald-700 active:bg-emerald-800', // Keep specific success color or map to semantic? Keeping for now.
  link: 'text-accent hover:underline p-0 h-auto',
};

const sizeStyles: Record<ButtonSize, string> = {
  xs: 'px-2 py-1 text-xs gap-1',
  sm: 'px-3 py-1.5 text-sm gap-1.5',
  md: 'px-4 py-2 text-sm gap-2',
  lg: 'px-6 py-2.5 text-base gap-2',
  xl: 'px-8 py-3 text-lg gap-3',
};

// ============ Button Component ============
export const Button = memo(
  forwardRef<HTMLButtonElement, ButtonProps>(
    (
      {
        variant = 'primary',
        size = 'md',
        fullWidth = false,
        isLoading = false,
        loadingText,
        leftIcon,
        rightIcon,
        rounded = false,
        className,
        disabled,
        children,
        ...props
      },
      ref
    ) => {
      const buttonClasses = useMemo(
        () =>
          cn(
            // Base styles
            'inline-flex items-center justify-center font-semibold transition-all duration-200',
            'focus:outline-none focus:ring-2 focus:ring-primary/50 focus:ring-offset-2',
            'disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:transform-none',
            // Variant
            variantStyles[variant],
            // Size
            sizeStyles[size],
            // Border radius
            rounded ? 'rounded-full' : 'rounded-lg',
            // Full width
            fullWidth && 'w-full',
            // Custom
            className
          ),
        [variant, size, rounded, fullWidth, className]
      );

      return (
        <button
          ref={ref}
          className={buttonClasses}
          disabled={disabled || isLoading}
          {...props}
        >
          {isLoading ? (
            <>
              <Loader2 className="animate-spin" size={size === 'xs' ? 12 : size === 'sm' ? 14 : 16} />
              {loadingText || children}
            </>
          ) : (
            <>
              {leftIcon}
              {children}
              {rightIcon}
            </>
          )}
        </button>
      );
    }
  )
);

Button.displayName = 'Button';

export default Button;
