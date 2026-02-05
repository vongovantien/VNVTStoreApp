import React, { useState, useRef, useEffect } from 'react';
import type { ReactNode } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { cn } from '@/utils/cn';

interface DropdownProps {
  trigger: ReactNode;
  children: ReactNode;
  align?: 'left' | 'right';
  className?: string;
  width?: string;
}

export const Dropdown: React.FC<DropdownProps> = ({
  trigger,
  children,
  align = 'right',
  className,
  width = 'w-48'
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  return (
    <div className={cn("relative inline-block text-left", className)} ref={dropdownRef}>
      <div onClick={() => setIsOpen(!isOpen)} className="cursor-pointer">
        {trigger}
      </div>

      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, scale: 0.95, y: -10 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.95, y: -10 }}
            transition={{ duration: 0.1 }}
            className={cn(
              "absolute z-[100] mt-2 rounded-md shadow-lg bg-white dark:bg-slate-800 ring-1 ring-black ring-opacity-5 focus:outline-none origin-top",
              align === 'right' ? 'right-0' : 'left-0',
              width
            )}
          >
            <div className="py-1" role="menu" aria-orientation="vertical" aria-labelledby="options-menu">
              {React.Children.map(children, (child) => {
                 if (React.isValidElement(child)) {
                    return React.cloneElement(child, {
                        // @ts-expect-error - injecting onClick into child
                        onClick: (e: React.MouseEvent) => {
                            (child as React.ReactElement<{ onClick?: (e: React.MouseEvent) => void }>).props.onClick?.(e);
                            setIsOpen(false);
                        }
                    });
                 }
                 return child;
              })}
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

interface DropdownItemProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  icon?: React.ReactNode;
  variant?: 'default' | 'danger';
}

export const DropdownItem: React.FC<DropdownItemProps> = ({ 
  children, 
  icon, 
  variant = 'default', 
  className,
  ...props 
}) => {
  return (
    <button
      className={cn(
        "group flex w-full items-center px-4 py-2 text-sm transition-colors",
        variant === 'danger' 
          ? "text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20" 
          : "text-gray-700 dark:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-700",
        className
      )}
      role="menuitem"
      {...props}
    >
      {icon && (
        <span className={cn(
          "mr-3 h-4 w-4",
          variant === 'danger' ? "text-red-500" : "text-gray-400 group-hover:text-gray-500"
        )}>
          {icon}
        </span>
      )}
      {children}
    </button>
  );
};
