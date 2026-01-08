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
