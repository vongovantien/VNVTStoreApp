import { SearchCondition, type SearchDTO } from '@/services/api';
import { format, isSameDay, isSameMonth, parseISO } from 'date-fns';

/**
 * Client-side implementation of Backend QueryHelper.ApplyFilters
 * Allows filtering local data arrays using the same SearchCondition logic.
 */

export const applyClientFilters = <T extends Record<string, any>>(data: T[], filters?: SearchDTO[]): T[] => {
    if (!filters || filters.length === 0) return data;

    return data.filter(item => {
        return filters.every(filter => {
            const { searchField, searchCondition, searchValue } = filter;

            // Handle nested fields (e.g. "category.name")
            const itemValue = getNestedValue(item, searchField);

            switch (searchCondition) {
                case SearchCondition.Equal:
                    return compareEqual(itemValue, searchValue);

                case SearchCondition.NotEqual:
                    return !compareEqual(itemValue, searchValue);

                case SearchCondition.Contains:
                    if (typeof itemValue === 'string' && typeof searchValue === 'string') {
                        return itemValue.toLowerCase().includes(searchValue.toLowerCase());
                    }
                    return compareEqual(itemValue, searchValue);

                case SearchCondition.GreaterThan:
                    return itemValue > searchValue!;

                case SearchCondition.GreaterThanEqual:
                    return itemValue >= searchValue!;

                case SearchCondition.LessThan:
                    return itemValue < searchValue!;

                case SearchCondition.LessThanEqual:
                    return itemValue <= searchValue!;

                case SearchCondition.In: // 13
                    if (Array.isArray(searchValue)) {
                        return searchValue.some(v => compareEqual(itemValue, v));
                    }
                    return false;

                case SearchCondition.NotIn: // 14
                    if (Array.isArray(searchValue)) {
                        return !searchValue.some(v => compareEqual(itemValue, v));
                    }
                    return true;

                case SearchCondition.IsNull: // 11
                    return itemValue === null || itemValue === undefined;

                case SearchCondition.IsNotNull: // 12
                    return itemValue !== null && itemValue !== undefined;

                case SearchCondition.EqualExact: // 15
                    return itemValue === searchValue;

                case SearchCondition.DateTimeRange: // 7
                    if (Array.isArray(searchValue) && searchValue.length >= 2) {
                        const dateVal = new Date(itemValue);
                        const start = new Date(searchValue[0] as string);
                        const end = new Date(searchValue[1] as string);
                        return dateVal >= start && dateVal <= end;
                    }
                    return false;

                // Date/Month/Day Parts currently assuming simplified handling or string matching for now
                // Backend implementation iterates date parts. We can add if standard Date usage is confirmed.

                default:
                    // Default to Equal
                    return compareEqual(itemValue, searchValue);
            }
        });
    });
};

// Helper to access nested properties 'user.role.name'
const getNestedValue = (obj: any, path: string): any => {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
};

// Helper for loose equality (handling string/number types and date strings)
const compareEqual = (a: any, b: any): boolean => {
    // strict equality first
    if (a === b) return true;
    // loose equality for numbers/strings mismatch
    if (a == b) return true; // eslint-disable-line eqeqeq

    // Case insensitive string comparison
    if (typeof a === 'string' && typeof b === 'string') {
        return a.toLowerCase() === b.toLowerCase();
    }

    return false;
};
