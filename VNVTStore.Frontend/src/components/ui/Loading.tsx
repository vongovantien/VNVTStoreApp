import React from 'react';
import { cn } from '@/utils/cn';

interface LoadingProps {
    className?: string;
    size?: 'sm' | 'md' | 'lg' | 'xl';
    text?: string;
    fullScreen?: boolean;
}

export const Loading: React.FC<LoadingProps> = ({ 
    className, 
    size = 'md', 
    text,
    fullScreen = false
}) => {
    const sizeClasses = {
        sm: 'w-4 h-4 border-2',
        md: 'w-8 h-8 border-[3px]',
        lg: 'w-12 h-12 border-4',
        xl: 'w-16 h-16 border-4'
    };

    const spinner = (
        <div className="flex flex-col items-center justify-center gap-3">
            <div 
                className={cn(
                    "rounded-full border-indigo-600 border-t-transparent animate-spin",
                    sizeClasses[size],
                    className
                )} 
            />
            {text && <span className="text-sm font-medium text-slate-600 dark:text-slate-300">{text}</span>}
        </div>
    );

    if (fullScreen) {
        return (
            <div className="fixed inset-0 bg-white/80 dark:bg-slate-900/80 z-[9999] flex items-center justify-center backdrop-blur-sm">
                {spinner}
            </div>
        );
    }

    return spinner;
};
