import { useState, useCallback } from 'react';
import { exportToExcel, ExportColumn } from '@/utils/export';

interface UseExportOptions<T> {
    defaultFilename?: string;
    defaultColumns?: ExportColumn<T>[];
}

export function useExport<T extends Record<string, any>>(initialOptions: UseExportOptions<T> = {}) {
    const [isExporting, setIsExporting] = useState(false);

    const exportData = useCallback(async (
        data: T[] | Promise<T[]>,
        filename: string = initialOptions.defaultFilename || 'export',
        columns?: ExportColumn<T>[]
    ) => {
        setIsExporting(true);
        try {
            const resolvedData = await Promise.resolve(data);
            const cols = columns || initialOptions.defaultColumns;
            await exportToExcel(resolvedData, filename, cols);
        } catch (error) {
            console.error('Export failed:', error);
            throw error;
        } finally {
            setIsExporting(false);
        }
    }, [initialOptions.defaultFilename, initialOptions.defaultColumns]);

    return { isExporting, exportData };
}
