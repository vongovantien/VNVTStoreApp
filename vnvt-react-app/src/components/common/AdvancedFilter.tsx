import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, X } from 'lucide-react';
import { Button, Input, Modal } from '@/components/ui';

export interface FilterDef {
    id: string;
    label: string;
    type: 'text' | 'select' | 'date' | 'number';
    options?: { label: string; value: string }[];
    placeholder?: string;
    className?: string; // For custom width or spanning
}

interface AdvancedFilterProps {
    isOpen: boolean;
    onClose: () => void;
    filterDefs: FilterDef[];
    onApply: (filters: Record<string, string>) => void;
    onClear: () => void;
    currentFilters: Record<string, string>;
}

export const AdvancedFilter = ({
    isOpen,
    onClose,
    filterDefs,
    onApply,
    onClear,
    currentFilters,
}: AdvancedFilterProps) => {
    const { t } = useTranslation();
    const [localFilters, setLocalFilters] = useState<Record<string, string>>(currentFilters);

    useEffect(() => {
        setLocalFilters(currentFilters);
    }, [currentFilters, isOpen]);

    const handleChange = (id: string, value: string) => {
        setLocalFilters((prev) => ({ ...prev, [id]: value }));
    };

    const handleApply = () => {
        onApply(localFilters);
        onClose();
    };

    const handleClear = () => {
        setLocalFilters({});
        onClear();
    };

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            title={null} // Custom header or just clean
            size="xl" // Wider modal for 2 cols
            footer={null} // Custom footer
            className="max-w-4xl"
        >
            <div className="p-1">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-4">
                    {filterDefs.map((field) => (
                        <div key={field.id} className="grid grid-cols-12 items-center gap-2">
                            <label
                                htmlFor={field.id}
                                className="col-span-4 text-right text-sm font-semibold text-blue-600 dark:text-blue-400 pr-2"
                            >
                                {field.label}
                            </label>
                            <div className="col-span-8">
                                {field.type === 'select' ? (
                                    <select
                                        id={field.id}
                                        value={localFilters[field.id] || ''}
                                        onChange={(e) => handleChange(field.id, e.target.value)}
                                        className="w-full px-3 py-2 border rounded-md bg-white dark:bg-slate-800 focus:outline-none focus:ring-1 focus:ring-primary"
                                    >
                                        <option value="">{t('common.choose') || 'Chọn'}</option>
                                        {field.options?.map((opt) => (
                                            <option key={opt.value} value={opt.value}>
                                                {opt.label}
                                            </option>
                                        ))}
                                    </select>
                                ) : field.type === 'date' ? (
                                    <Input
                                        id={field.id}
                                        type="datetime-local"
                                        value={localFilters[field.id] || ''}
                                        onChange={(e) => handleChange(field.id, e.target.value)}
                                        className="w-full"
                                    />
                                ) : (
                                    <Input
                                        id={field.id}
                                        type={field.type === 'number' ? 'number' : 'text'}
                                        value={localFilters[field.id] || ''}
                                        onChange={(e) => handleChange(field.id, e.target.value)}
                                        placeholder={field.placeholder}
                                        className="w-full"
                                    />
                                )}
                            </div>
                        </div>
                    ))}
                </div>

                {/* Footer Actions */}
                <div className="flex justify-center gap-4 mt-8 pb-2">
                    <Button
                        onClick={handleApply}
                        className="bg-green-600 hover:bg-green-700 text-white min-w-[120px]"
                        leftIcon={<Search size={18} />}
                    >
                        {t('common.search') || 'Tìm kiếm'}
                    </Button>
                    <Button
                        variant="outline"
                        onClick={onClose}
                        className="bg-gray-500 hover:bg-gray-600 text-white border-transparent hover:text-white min-w-[100px]"
                        leftIcon={<X size={18} />}
                    >
                        {t('common.close') || 'Đóng'}
                    </Button>
                </div>
            </div>
        </Modal>
    );
};
