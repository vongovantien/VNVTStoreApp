import { memo, forwardRef, useMemo, type SelectHTMLAttributes, type ReactNode } from 'react';
import { ChevronDown } from 'lucide-react';
import { cn } from '@/utils/cn';

// ============ Select Types ============
export type SelectSize = 'sm' | 'md' | 'lg';

export interface SelectOption {
  value: string | number;
  label: string;
  disabled?: boolean;
}

// ============ Select Props Interface ============
export interface SelectProps extends Omit<SelectHTMLAttributes<HTMLSelectElement>, 'size'> {
  /** Select size */
  size?: SelectSize;
  /** Label text */
  label?: string;
  /** Helper text */
  helperText?: string;
  /** Error message */
  error?: string;
  /** Placeholder */
  placeholder?: string;
  /** Options array */
  options: SelectOption[];
  /** Left icon */
  leftIcon?: ReactNode;
  /** Full width */
  fullWidth?: boolean;
  /** Required */
  isRequired?: boolean;
  /** Wrapper className */
  wrapperClassName?: string;
}

// ============ Style Maps ============
const sizeStyles: Record<SelectSize, { select: string; label: string }> = {
  sm: { select: 'px-3 py-1.5 text-sm pr-8', label: 'text-xs' },
  md: { select: 'px-4 py-2 text-sm pr-10', label: 'text-sm' },
  lg: { select: 'px-4 py-3 text-base pr-12', label: 'text-base' },
};

// ============ Select Component ============
export const Select = memo(
  forwardRef<HTMLSelectElement, SelectProps>(
    (
      {
        size = 'md',
        label,
        helperText,
        error,
        placeholder,
        options,
        leftIcon,
        fullWidth = true,
        isRequired = false,
        wrapperClassName,
        className,
        id,
        ...props
      },
      ref
    ) => {
      const selectId = id || `select-${Math.random().toString(36).substr(2, 9)}`;

      const selectClasses = useMemo(
        () =>
          cn(
            // Base styles
            'w-full bg-primary text-primary appearance-none cursor-pointer',
            'border-2 border-gray-200 rounded-lg',
            'transition-all duration-200 outline-none focus:border-primary',
            'disabled:opacity-50 disabled:cursor-not-allowed',
            // Size
            sizeStyles[size].select,
            // Left icon
            leftIcon && 'pl-10',
            // Error
            error && 'border-error focus:border-error',
            // Custom
            className
          ),
        [size, leftIcon, error, className]
      );

      return (
        <div className={cn('flex flex-col gap-1', fullWidth && 'w-full', wrapperClassName)}>
          {/* Label */}
          {label && (
            <label
              htmlFor={selectId}
              className={cn('font-medium text-primary', sizeStyles[size].label)}
            >
              {label}
              {isRequired && <span className="text-error ml-1">*</span>}
            </label>
          )}

          {/* Select wrapper */}
          <div className="relative">
            {/* Left Icon */}
            {leftIcon && (
              <div className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary pointer-events-none">
                {leftIcon}
              </div>
            )}

            {/* Select */}
            <select ref={ref} id={selectId} className={selectClasses} {...props}>
              {placeholder && (
                <option value="" disabled>
                  {placeholder}
                </option>
              )}
              {options.map((option) => (
                <option key={option.value} value={option.value} disabled={option.disabled}>
                  {option.label}
                </option>
              ))}
            </select>

            {/* Chevron Icon */}
            <ChevronDown
              className="absolute right-3 top-1/2 -translate-y-1/2 text-tertiary pointer-events-none"
              size={size === 'sm' ? 14 : size === 'lg' ? 20 : 16}
            />
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

Select.displayName = 'Select';

export default Select;
