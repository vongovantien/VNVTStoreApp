import { useState } from 'react';
import { Calendar, X } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';

export interface DateRange {
    startDate: string; // ISO format YYYY-MM-DD
    endDate: string;
}

interface DateRangePickerProps {
    value: DateRange;
    onChange: (range: DateRange) => void;
    presets?: boolean;
}

const PRESETS = [
    { label: 'Hôm nay', days: 0 },
    { label: '7 ngày qua', days: 7 },
    { label: '30 ngày qua', days: 30 },
    { label: '90 ngày qua', days: 90 },
];

export const DateRangePicker = ({ value, onChange, presets = true }: DateRangePickerProps) => {
    const [isOpen, setIsOpen] = useState(false);

    const handlePreset = (days: number) => {
        const endDate = new Date();
        const startDate = new Date();
        startDate.setDate(endDate.getDate() - days);

        onChange({
            startDate: startDate.toISOString().split('T')[0],
            endDate: endDate.toISOString().split('T')[0],
        });
        setIsOpen(false);
    };

    const formatDisplayDate = (dateStr: string) => {
        const date = new Date(dateStr);
        return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
    };

    return (
        <div className="relative">
            <button
                onClick={() => setIsOpen(!isOpen)}
                className="flex items-center gap-2 px-4 py-2 bg-secondary hover:bg-secondary/80 rounded-lg transition-colors text-sm font-medium border border-border"
            >
                <Calendar size={16} />
                <span>
                    {formatDisplayDate(value.startDate)} - {formatDisplayDate(value.endDate)}
                </span>
            </button>

            <AnimatePresence>
                {isOpen && (
                    <>
                        <motion.div
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            exit={{ opacity: 0 }}
                            className="fixed inset-0 z-40"
                            onClick={() => setIsOpen(false)}
                        />
                        <motion.div
                            initial={{ opacity: 0, scale: 0.95, y: -10 }}
                            animate={{ opacity: 1, scale: 1, y: 0 }}
                            exit={{ opacity: 0, scale: 0.95, y: -10 }}
                            className="absolute right-0 top-full mt-2 bg-primary border border-border rounded-xl shadow-xl z-50 p-4 w-80"
                        >
                            <div className="flex items-center justify-between mb-3">
                                <h3 className="font-semibold text-sm">Chọn khoảng thời gian</h3>
                                <button
                                    onClick={() => setIsOpen(false)}
                                    className="p-1 hover:bg-secondary rounded-lg transition-colors"
                                >
                                    <X size={16} />
                                </button>
                            </div>

                            {/* Date Inputs */}
                            <div className="space-y-3 mb-4">
                                <div>
                                    <label className="text-xs text-tertiary mb-1 block">Từ ngày</label>
                                    <input
                                        type="date"
                                        value={value.startDate}
                                        onChange={(e) => onChange({ ...value, startDate: e.target.value })}
                                        className="w-full px-3 py-2 bg-secondary border border-border rounded-lg text-sm outline-none focus:ring-2 focus:ring-accent/50"
                                    />
                                </div>
                                <div>
                                    <label className="text-xs text-tertiary mb-1 block">Đến ngày</label>
                                    <input
                                        type="date"
                                        value={value.endDate}
                                        onChange={(e) => onChange({ ...value, endDate: e.target.value })}
                                        className="w-full px-3 py-2 bg-secondary border border-border rounded-lg text-sm outline-none focus:ring-2 focus:ring-accent/50"
                                    />
                                </div>
                            </div>

                            {/* Presets */}
                            {presets && (
                                <div className="border-t border-border pt-3">
                                    <p className="text-xs text-tertiary mb-2">Khoảng thời gian nhanh</p>
                                    <div className="grid grid-cols-2 gap-2">
                                        {PRESETS.map((preset) => (
                                            <button
                                                key={preset.label}
                                                onClick={() => handlePreset(preset.days)}
                                                className="px-3 py-2 text-xs font-medium bg-secondary hover:bg-accent hover:text-white rounded-lg transition-colors"
                                            >
                                                {preset.label}
                                            </button>
                                        ))}
                                    </div>
                                </div>
                            )}
                        </motion.div>
                    </>
                )}
            </AnimatePresence>
        </div>
    );
};
