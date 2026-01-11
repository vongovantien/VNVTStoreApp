import { memo, forwardRef, useMemo, type ButtonHTMLAttributes, type ReactNode } from 'react';
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
  primary: 'bg-indigo-600 text-white hover:bg-indigo-700 hover:shadow-lg hover:shadow-indigo-500/30 active:bg-indigo-800',
  secondary: 'bg-rose-500 text-white hover:bg-rose-600 active:bg-rose-700',
  outline: 'border-2 border-indigo-500 text-indigo-600 hover:bg-indigo-600 hover:text-white',
  ghost: 'text-slate-600 hover:bg-slate-100 hover:text-indigo-600',
  danger: 'bg-red-500 text-white hover:bg-red-600 active:bg-red-700',
  success: 'bg-emerald-500 text-white hover:bg-emerald-600 active:bg-emerald-700',
  link: 'text-indigo-600 hover:underline p-0 h-auto',
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
