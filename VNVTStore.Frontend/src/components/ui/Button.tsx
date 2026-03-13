import { memo, forwardRef, ButtonHTMLAttributes, ReactNode } from 'react';
import { Loader2 } from 'lucide-react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/utils/cn';

// ============ Button Variants (CVA) ============
const buttonVariants = cva(
  "inline-flex items-center justify-center font-semibold transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-primary/50 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:transform-none",
  {
    variants: {
      variant: {
        primary: "bg-accent text-accent-foreground hover:bg-accent-hover hover:text-white hover:shadow-md hover:shadow-accent/20 active:scale-[0.98]",
        secondary: "bg-secondary text-white hover:opacity-90 active:scale-[0.98]",
        outline: "border-2 border-accent text-accent hover:bg-accent/10 hover:text-accent hover:shadow-md hover:shadow-accent/10 active:scale-[0.98]",
        ghost: "text-text-secondary hover:bg-bg-tertiary hover:text-accent active:scale-[0.98]",
        danger: "bg-error text-white hover:shadow-md hover:shadow-error/20 active:scale-[0.98]",
        success: "bg-emerald-600 text-white hover:bg-emerald-700 active:scale-[0.98]",
        link: "text-accent hover:underline p-0 h-auto",
      },
      size: {
        xs: "px-2 py-1 text-xs gap-1",
        sm: "px-3 py-1.5 text-sm gap-1.5",
        md: "px-4 py-2 text-sm gap-2",
        lg: "px-6 py-2.5 text-base gap-2",
        xl: "px-8 py-3 text-lg gap-3",
      },
      rounded: {
        true: "rounded-full",
        false: "rounded-lg",
      },
      fullWidth: {
        true: "w-full",
      },
    },
    defaultVariants: {
      variant: "primary",
      size: "md",
      rounded: false,
      fullWidth: false,
    },
  }
);

// ============ Button Props Interface ============
export interface ButtonProps
  extends ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  isLoading?: boolean;
  loadingText?: string;
  leftIcon?: ReactNode;
  rightIcon?: ReactNode;
}

// ============ Button Component ============
export const Button = memo(
  forwardRef<HTMLButtonElement, ButtonProps>(
    (
      {
        className,
        variant,
        size,
        rounded,
        fullWidth,
        isLoading = false,
        loadingText,
        leftIcon,
        rightIcon,
        disabled,
        children,
        ...props
      },
      ref
    ) => {
      return (
        <button
          ref={ref}
          className={cn(buttonVariants({ variant, size, rounded, fullWidth, className }))}
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

