import { memo, forwardRef, useMemo, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@/utils/cn';

// ============ Input Variants & Sizes ============
export type InputVariant = 'default' | 'filled' | 'flushed';
export type InputSize = 'sm' | 'md' | 'lg';

// ============ Input Props Interface ============
export interface InputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'size'> {
  /** Input variant style */
  variant?: InputVariant;
  /** Input size */
  size?: InputSize;
  /** Label text */
  label?: string;
  /** Helper text below input */
  helperText?: string;
  /** Error message */
  error?: string;
  /** Left addon element */
  leftAddon?: ReactNode;
  /** Right addon element */
  rightAddon?: ReactNode;
  /** Left icon */
  leftIcon?: ReactNode;
  /** Right icon */
  rightIcon?: ReactNode;
  /** Full width */
  fullWidth?: boolean;
  /** Required indicator */
  isRequired?: boolean;
  /** Wrapper className */
  wrapperClassName?: string;
}

// ============ Style Maps ============
const variantStyles: Record<InputVariant, string> = {
  default: 'border border-border rounded-lg focus:border-accent focus:ring-1 focus:ring-accent bg-bg-primary text-text-primary',
  filled: 'border border-transparent bg-bg-tertiary rounded-lg focus:bg-bg-secondary focus:border-accent focus:ring-1 focus:ring-accent text-text-primary',
  flushed: 'border-b border-border rounded-none focus:border-accent focus:ring-0 bg-transparent text-text-primary',
};

const sizeStyles: Record<InputSize, { input: string; label: string }> = {
  sm: { input: 'px-3 py-1.5 text-sm', label: 'text-xs' },
  md: { input: 'px-4 py-2 text-sm', label: 'text-sm' },
  lg: { input: 'px-4 py-3 h-12 text-base', label: 'text-base' },
};

// ============ Input Component ============
export const Input = memo(
  forwardRef<HTMLInputElement, InputProps>(
    (
      {
        variant = 'default',
        size = 'md',
        label,
        helperText,
        error,
        leftAddon,
        rightAddon,
        leftIcon,
        rightIcon,
        fullWidth = true,
        isRequired = false,
        wrapperClassName,
        className,
        id,
        ...props
      },
      ref
    ) => {
      const inputId = id || `input-${Math.random().toString(36).substr(2, 9)}`;

      const inputClasses = useMemo(
        () =>
          cn(
            // Base styles
            'w-full bg-bg-primary text-text-primary placeholder:text-text-tertiary',
            'transition-all duration-200 outline-none',
            'disabled:opacity-50 disabled:cursor-not-allowed',
            // Variant
            variantStyles[variant],
            // Size
            sizeStyles[size].input,
            // Icons padding
            leftIcon && 'pl-10',
            rightIcon && 'pr-10',
            // Error state
            error && 'border-error focus:border-error',
            // Custom
            className
          ),
        [variant, size, leftIcon, rightIcon, error, className]
      );

      return (
        <div className={cn('flex flex-col gap-1', fullWidth && 'w-full', wrapperClassName)}>
          {/* Label */}
          {label && (
            <label
              htmlFor={inputId}
              className={cn(
                'font-bold text-primary',
                sizeStyles[size].label
              )}
            >
              {label}
              {isRequired && <span className="text-red-500 ml-1">*</span>}
            </label>
          )}

          {/* Input wrapper */}
          <div className="relative flex">
            {/* Left Addon */}
            {leftAddon && (
              <div className="flex items-center px-3 bg-bg-tertiary border-2 border-r-0 border-border rounded-l-lg text-text-secondary">
                {leftAddon}
              </div>
            )}

            {/* Left Icon */}
            {leftIcon && (
              <div className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary">
                {leftIcon}
              </div>
            )}

            {/* Input */}
            <input
              ref={ref}
              id={inputId}
              className={cn(
                inputClasses,
                leftAddon && 'rounded-l-none',
                rightAddon && 'rounded-r-none'
              )}
              {...props}
            />

            {/* Right Icon */}
            {rightIcon && (
              <div className="absolute right-3 top-1/2 -translate-y-1/2 flex items-center justify-center">
                {rightIcon}
              </div>
            )}

            {/* Right Addon */}
            {rightAddon && (
              <div className="flex items-center px-3 bg-bg-tertiary border-2 border-l-0 border-border rounded-r-lg text-text-secondary">
                {rightAddon}
              </div>
            )}
          </div>

          {/* Helper/Error text */}
          {(helperText || error) && (
            <span className={cn('text-xs', error ? 'text-error' : 'text-tertiary')}>
              {error || helperText}
            </span>
          )}
        </div>
      );
    }
  )
);

Input.displayName = 'Input';

export default Input;
