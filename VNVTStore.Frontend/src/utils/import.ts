export const parseCSV = (text: string): Record<string, any>[] => {
    const lines = text.split('\n').filter((line) => line.trim() !== '');
    if (lines.length < 2) return [];

    const headers = lines[0].split(',').map((h) => h.trim().replace(/^"|"$/g, ''));

    return lines.slice(1).map((line) => {
        // Basic CSV splitting (doesn't handle commas inside quotes perfectly, but sufficient for simple data)
        // For robust parsing, a library like PapaParse is recommended, but this works for simple standard CSVs
        const values = line.split(',').map((v) => v.trim().replace(/^"|"$/g, ''));

        const obj: Record<string, any> = {};
        headers.forEach((header, index) => {
            // Try to convert to number if possible
            const value = values[index];
            const numInfo = Number(value);
            obj[header] = !isNaN(numInfo) && value !== '' ? numInfo : value;
        });

        return obj;
    });
};
