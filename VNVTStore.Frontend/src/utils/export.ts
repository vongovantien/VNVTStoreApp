import ExcelJS from 'exceljs';

export interface ExportColumn<T> {
    key: keyof T;
    label: string;
    width?: number;
}

/**
 * Export data to Excel (.xlsx) file using ExcelJS
 * @param data - Array of data objects to export
 * @param filename - Filename without extension
 * @param columns - Optional column definitions for headers and keys
 */
export const exportToExcel = async <T extends Record<string, any>>(
    data: T[],
    filename: string,
    columns?: ExportColumn<T>[]
): Promise<void> => {
    if (!data.length) {
        console.warn('No data to export');
        return;
    }

    // Create workbook and worksheet
    const workbook = new ExcelJS.Workbook();
    const worksheet = workbook.addWorksheet('Data');

    // Determine columns
    const exportColumns = columns || Object.keys(data[0]).map(key => ({
        key: key as keyof T,
        label: key,
        width: 15
    }));

    // Set up worksheet columns
    worksheet.columns = exportColumns.map(col => ({
        header: col.label,
        key: String(col.key),
        width: col.width || 15
    }));

    // Style header row
    worksheet.getRow(1).font = { bold: true };
    worksheet.getRow(1).fill = {
        type: 'pattern',
        pattern: 'solid',
        fgColor: { argb: 'FF4472C4' }
    };
    worksheet.getRow(1).font = { bold: true, color: { argb: 'FFFFFFFF' } };

    // Add data rows
    data.forEach(item => {
        const row: Record<string, any> = {};
        exportColumns.forEach(col => {
            const value = item[col.key];
            // Handle different value types
            if (value === null || value === undefined) {
                row[String(col.key)] = '';
            } else if (typeof value === 'object') {
                row[String(col.key)] = JSON.stringify(value);
            } else {
                row[String(col.key)] = value;
            }
        });
        worksheet.addRow(row);
    });

    // Auto-fit columns (approximate)
    worksheet.columns.forEach(column => {
        if (column.eachCell) {
            let maxLength = 0;
            column.eachCell({ includeEmpty: true }, cell => {
                const cellValue = cell.value ? String(cell.value) : '';
                maxLength = Math.max(maxLength, cellValue.length);
            });
            column.width = Math.min(Math.max(maxLength + 2, 10), 50);
        }
    });

    // Generate buffer and trigger download
    const buffer = await workbook.xlsx.writeBuffer();
    const blob = new Blob([buffer], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    });

    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', `${filename}.xlsx`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

/**
 * Legacy CSV export function for backwards compatibility
 */
export const exportToCSV = <T extends Record<string, any>>(
    data: T[],
    filename: string,
    columns?: { key: keyof T; label: string }[]
) => {
    if (!data.length) return;

    // Determine headers
    const headers = columns
        ? columns.map((c) => c.label)
        : Object.keys(data[0]);

    const keys = columns
        ? columns.map((c) => c.key)
        : Object.keys(data[0]);

    // Convert to CSV string
    const csvContent = [
        headers.join(','),
        ...data.map((row) =>
            keys
                .map((key) => {
                    const value = row[key];
                    // Handle strings with commas or quotes
                    if (typeof value === 'string') {
                        return `"${value.replace(/"/g, '""')}"`;
                    }
                    return value;
                })
                .join(',')
        ),
    ].join('\n');

    // Trigger download
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', `${filename}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
