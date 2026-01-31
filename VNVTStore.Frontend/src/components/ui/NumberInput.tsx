import { memo, forwardRef, useState, useEffect, type InputHTMLAttributes, type ChangeEvent } from 'react';
import { cn } from '@/utils/cn';
import { Input, type InputProps } from './Input';

export interface NumberInputProps extends Omit<InputProps, 'onChange' | 'value'> {
  value?: number;
  onChange?: (value: number | undefined) => void;
  min?: number;
  max?: number;
}

const formatNumber = (num: number | undefined): string => {
  if (num === undefined || num === null || isNaN(num)) return '';
  return new Intl.NumberFormat('vi-VN').format(num);
};

export const NumberInput = memo(
  forwardRef<HTMLInputElement, NumberInputProps>(
    ({ value, onChange, min = 0, max, ...props }, ref) => {
      // Local state for the display value (formatted string)
      const [displayValue, setDisplayValue] = useState<string>('');

      // Sync display value when prop value changes (e.g. from React Hook Form or initial load)
      useEffect(() => {
        setDisplayValue(formatNumber(value));
      }, [value]);

      const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
        // Allow digits and commas
        const start = e.target.selectionStart;
        const oldLength = e.target.value.length;
        
        let inputValue = e.target.value.replace(/[^0-9]/g, '');

        if (inputValue === '') {
          setDisplayValue('');
          onChange?.(undefined);
          return;
        }

        let numValue = parseInt(inputValue, 10);

        if (max !== undefined && numValue > max) {
            numValue = max;
        }
        
        // Don't enforce min on change efficiently to allow typing, but maybe enforce non-negative if desired
        // For now standard positive integet input

        const formatted = formatNumber(numValue);
        setDisplayValue(formatted);
        onChange?.(numValue);
      };

      return (
        <Input
          {...props}
          ref={ref}
          value={displayValue}
          onChange={handleChange}
          inputMode="numeric"
          autoComplete="off"
        />
      );
    }
  )
);

NumberInput.displayName = 'NumberInput';
