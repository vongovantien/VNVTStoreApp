import { cn } from '@/utils/cn';

export interface SwitchProps {
  checked?: boolean;
  onChange?: (checked: boolean) => void;
  label?: string;
  description?: string;
  disabled?: boolean;
  className?: string;
  size?: 'sm' | 'md' | 'lg';
}

export const Switch = ({
  checked = false,
  onChange,
  label,
  description,
  disabled = false,
  className,
  size = 'md',
}: SwitchProps) => {
  const sizeClasses = {
    sm: { track: 'h-5 w-9', thumb: 'h-3 w-3', translate: 'translate-x-4' },
    md: { track: 'h-6 w-11', thumb: 'h-4 w-4', translate: 'translate-x-5' },
    lg: { track: 'h-7 w-14', thumb: 'h-5 w-5', translate: 'translate-x-7' },
  };

  const currentSize = sizeClasses[size];

  return (
    <div className={cn('flex items-center justify-between', className)}>
      {(label || description) && (
        <div className="mr-4">
          {label && (
            <label className="text-sm font-semibold text-text-primary">
              {label}
            </label>
          )}
          {description && (
            <p className="text-xs text-text-secondary mt-0.5">
              {description}
            </p>
          )}
        </div>
      )}
      <button
        type="button"
        role="switch"
        aria-checked={checked}
        disabled={disabled}
        onClick={() => onChange?.(!checked)}
        className={cn(
          'relative inline-flex items-center rounded-full transition-all duration-300 focus:outline-none focus:ring-2 focus:ring-accent/40',
          currentSize.track,
          checked 
            ? 'bg-accent shadow-[inset_0_2px_4px_rgba(0,0,0,0.1)]' 
            : 'bg-bg-tertiary shadow-inner',
          disabled && 'opacity-50 cursor-not-allowed'
        )}
      >
        <span
          className={cn(
            'inline-block transform rounded-full bg-white shadow-[0_2px_8px_rgba(0,0,0,0.15)] transition-all duration-300 ease-[cubic-bezier(0.34,1.56,0.64,1)]',
            currentSize.thumb,
            checked ? currentSize.translate : 'translate-x-1'
          )}
        />
      </button>
    </div>
  );
};

// Wrapper with card-like background for forms
export interface FormSwitchProps extends SwitchProps {
  containerClassName?: string;
}

export const FormSwitch = ({
  containerClassName,
  ...props
}: FormSwitchProps) => {
  return (
    <div className={cn(
      'py-3 border-b border-border last:border-0',
      containerClassName
    )}>
      <Switch {...props} />
    </div>
  );
};

export default Switch;
