import { memo, forwardRef, useMemo, useId, type TextareaHTMLAttributes } from 'react';
import { cn } from '@/utils/cn';

// ============ Textarea Variants & Sizes ============
export type TextareaVariant = 'default' | 'filled' | 'flushed';
export type TextareaSize = 'sm' | 'md' | 'lg';

// ============ Textarea Props Interface ============
export interface TextareaProps extends Omit<TextareaHTMLAttributes<HTMLTextAreaElement>, 'size'> {
  /** Textarea variant style */
  variant?: TextareaVariant;
  /** Textarea size */
  size?: TextareaSize;
  /** Label text */
  label?: string;
  /** Helper text below textarea */
  helperText?: string;
  /** Error message */
  error?: string;
  /** Full width */
  fullWidth?: boolean;
  /** Required indicator */
  isRequired?: boolean;
  /** Wrapper className */
  wrapperClassName?: string;
}

// ============ Style Maps ============
const variantStyles: Record<TextareaVariant, string> = {
  default: 'border border-gray-200 rounded-lg focus:border-primary focus:ring-1 focus:ring-primary',
  filled: 'border border-transparent bg-tertiary rounded-lg focus:bg-secondary focus:border-primary focus:ring-1 focus:ring-primary',
  flushed: 'border-b border-gray-200 rounded-none focus:border-primary focus:ring-0',
};

const sizeStyles: Record<TextareaSize, { input: string; label: string }> = {
  sm: { input: 'px-3 py-1.5 text-sm', label: 'text-xs' },
  md: { input: 'px-4 py-2 text-sm', label: 'text-sm' },
  lg: { input: 'px-4 py-3 text-base', label: 'text-base' },
};

// ============ Textarea Component ============
export const Textarea = memo(
  forwardRef<HTMLTextAreaElement, TextareaProps>(
    (
      {
        variant = 'default',
        size = 'md',
        label,
        helperText,
        error,
        fullWidth = true,
        isRequired = false,
        wrapperClassName,
        className,
        id,
        rows = 4,
        ...props
      },
      ref
    ) => {
      const generatedId = useId();
      const textareaId = id || generatedId;

      const textareaClasses = useMemo(
        () =>
          cn(
            // Base styles
            'w-full bg-primary text-primary placeholder:text-tertiary',
            'transition-all duration-200 outline-none resize-y',
            'disabled:opacity-50 disabled:cursor-not-allowed',
            // Variant
            variantStyles[variant],
            // Size
            sizeStyles[size].input,
            // Error state
            error && 'border-error focus:border-error',
            // Custom
            className
          ),
        [variant, size, error, className]
      );

      return (
        <div className={cn('flex flex-col gap-1', fullWidth && 'w-full', wrapperClassName)}>
          {/* Label */}
          {label && (
            <label
              htmlFor={textareaId}
              className={cn(
                'font-bold text-primary',
                sizeStyles[size].label
              )}
            >
              {label}
              {isRequired && <span className="text-red-500 ml-1">*</span>}
            </label>
          )}

          {/* Textarea */}
          <textarea
            ref={ref}
            id={textareaId}
            rows={rows}
            className={textareaClasses}
            {...props}
          />

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

Textarea.displayName = 'Textarea';

export default Textarea;
