import React, { forwardRef } from 'react';
import { cn } from '@/utils/cn';

export interface CheckboxProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'type'> {
    label?: string;
    error?: string;
}

export const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(
    ({ className, label, error, ...props }, ref) => {
        return (
            <div className="flex flex-col gap-1.5">
                <label className="flex items-center gap-2 cursor-pointer group">
                    <div className="relative flex items-center justify-center">
                        <input
                            type="checkbox"
                            ref={ref}
                            className={cn(
                                "peer shrink-0 appearance-none w-5 h-5 rounded-md border-2 border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900",
                                "checked:border-accent-primary checked:bg-accent-primary",
                                "focus:outline-none focus:ring-2 focus:ring-accent-primary/20",
                                "disabled:opacity-50 disabled:cursor-not-allowed",
                                "transition-all duration-200 ease-in-out cursor-pointer",
                                className
                            )}
                            {...props}
                        />
                        <svg
                            className="absolute w-3.5 h-3.5 text-white opacity-0 peer-checked:opacity-100 transition-opacity pointer-events-none"
                            xmlns="http://www.w3.org/2000/svg"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="4"
                            strokeLinecap="round"
                            strokeLinejoin="round"
                        >
                            <polyline points="20 6 9 17 4 12" />
                        </svg>
                    </div>
                    {label && (
                        <span className="text-sm font-medium text-primary select-none group-hover:text-accent-primary transition-colors">
                            {label}
                        </span>
                    )}
                </label>
                {error && <span className="text-xs text-red-500 font-medium ml-1">{error}</span>}
            </div>
        );
    }
);

Checkbox.displayName = 'Checkbox';
