import { useTranslation } from 'react-i18next';
import { useState } from 'react';
import { Button, Badge, ConfirmDialog, Input } from '@/components/ui';
import { Plus, X } from 'lucide-react';
import LazySelect from '@/components/ui/LazySelect';
import { formatCurrency } from '@/utils/format';

export interface ProductUnitDto {
    code?: string;
    unitName: string;
    conversionRate: number;
    price: number;
    isActive?: boolean;
    isBaseUnit: boolean;
    productCode?: string;
}

interface ProductUnitsManagerProps {
    baseUnitName?: string;
    baseUnitPrice?: number;
    units: ProductUnitDto[];
    onChange: (units: ProductUnitDto[]) => void;
    fetchUnitOptions: (params: { pageIndex: number; pageSize: number; search?: string }) => Promise<{ items: { value: string; label: string }[]; totalItems: number; }>;
}

export const ProductUnitsManager = ({ 
    baseUnitName, 
    baseUnitPrice,
    units = [],
    onChange,
    fetchUnitOptions
}: ProductUnitsManagerProps) => {
    const { t } = useTranslation();
    const [unitToDeleteIdx, setUnitToDeleteIdx] = useState<number | null>(null);

    const handleAddUnit = () => {
        const newUnit: ProductUnitDto = {
            code: `temp-${Date.now()}`,
            unitName: '',
            conversionRate: 1,
            price: baseUnitPrice || 0,
            isActive: true,
            isBaseUnit: false
        };
        onChange([...units, newUnit]);
    };

    const handleUpdateUnit = (index: number, field: keyof ProductUnitDto, value: string | number | boolean) => {
        const newUnits = [...units];
        newUnits[index] = { ...newUnits[index], [field]: value };
        onChange(newUnits);
    };

    const handleDeleteUnit = () => {
        if (unitToDeleteIdx !== null) {
            const newUnits = units.filter((_, i) => i !== unitToDeleteIdx);
            onChange(newUnits);
            setUnitToDeleteIdx(null);
        }
    };

    return (
        <div className="space-y-4">
            <div className="flex gap-2 mb-2">
                <Button type="button" size="sm" variant="outline" onClick={handleAddUnit}>
                    <Plus size={16} className="mr-1" /> {t('admin.actions.addUnit')}
                </Button>
            </div>
            
            <div className="space-y-3 bg-slate-50 dark:bg-slate-900/50 p-4 rounded-xl border border-border-color">
                {/* Header Row */}
                <div className="grid grid-cols-12 gap-3 px-2 mb-2 text-xs font-semibold text-secondary uppercase">
                    <div className="col-span-4">{t('common.fields.unitName')}</div>
                    <div className="col-span-3">{t('common.fields.conversionRate')}</div>
                    <div className="col-span-4">{t('common.fields.price')}</div>
                    <div className="col-span-1"></div>
                </div>

                {/* Base Unit Row (Read Only) */}
                <div className="grid grid-cols-12 gap-3 items-center bg-blue-50/50 dark:bg-blue-900/20 p-2 rounded-lg border border-blue-100 dark:border-blue-800/30">
                    <div className="col-span-4 flex items-center gap-2">
                         <span className="text-sm font-medium text-slate-700 dark:text-slate-200">{baseUnitName || 'Cái'}</span>
                         <Badge size="sm" color="primary" className="text-[10px] py-0">{t('product.defaultUnit')}</Badge>
                    </div>
                    <div className="col-span-3 text-sm text-slate-500 italic">
                        1 = 1
                    </div>
                    <div className="col-span-4 text-sm font-medium text-rose-600">
                        {formatCurrency(baseUnitPrice || 0)}
                    </div>
                    <div className="col-span-1"></div>
                </div>

                {/* Editable Units */}
                {units.map((unit, idx) => (
                    <div key={unit.code || idx} className="grid grid-cols-12 gap-3 items-center bg-white dark:bg-slate-800 p-3 rounded-lg border border-slate-200 dark:border-slate-700 shadow-sm hover:shadow-md transition-shadow" data-testid={`unit-row-${idx}`}>
                        <div className="col-span-4">
                            <LazySelect
                                value={unit.unitName}
                                onChange={(val) => handleUpdateUnit(idx, 'unitName', val)}
                                fetchFn={fetchUnitOptions}
                                placeholder={t('common.fields.unitName')}
                                initialLabel={unit.unitName}
                                queryKeyPrefix={`unit-select-${idx}`}
                                className="h-10 text-sm font-medium"
                            />
                        </div>
                        <div className="col-span-3 relative">
                            <Input 
                                type="number"
                                className="h-10 text-sm pr-16 text-center font-medium" 
                                placeholder="1" 
                                value={unit.conversionRate} 
                                onChange={(e) => handleUpdateUnit(idx, 'conversionRate', Number(e.target.value))} 
                            />
                             <span className="absolute right-2 top-1/2 -translate-y-1/2 text-xs text-slate-400 pointer-events-none bg-slate-100 dark:bg-slate-700 px-1.5 py-0.5 rounded">
                                × {baseUnitName || 'PCS'}
                            </span>
                        </div>
                        <div className="col-span-4 relative">
                            <Input 
                                className="h-10 text-sm text-right pr-12 font-semibold text-indigo-600 dark:text-indigo-400 bg-indigo-50 dark:bg-indigo-900/20 border-indigo-200 dark:border-indigo-800" 
                                placeholder={t('common.fields.price')} 
                                value={unit.price.toLocaleString('vi-VN')} 
                                onChange={(e) => {
                                    const cleaned = e.target.value.replace(/[^\d]/g, '');
                                    handleUpdateUnit(idx, 'price', cleaned ? parseInt(cleaned, 10) : 0);
                                }} 
                            />
                            <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs font-medium text-indigo-500 pointer-events-none">₫</span>
                        </div>
                        <div className="col-span-1 flex justify-end">
                            <button 
                                type="button" 
                                onClick={() => setUnitToDeleteIdx(idx)} 
                                className="text-slate-400 hover:text-red-500 p-2 rounded-lg hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
                                data-testid={`delete-unit-button-${idx}`}
                            >
                                <X size={18} />
                            </button>
                        </div>
                    </div>
                ))}
            </div>

             <ConfirmDialog
                isOpen={unitToDeleteIdx !== null}
                onClose={() => setUnitToDeleteIdx(null)}
                onConfirm={handleDeleteUnit}
                title={t('admin.actions.delete')}
                message={t('messages.confirmDelete', { name: unitToDeleteIdx !== null ? units[unitToDeleteIdx].unitName : '' })}
                confirmText={t('admin.actions.delete')}
                variant="danger"
            />
        </div>
    );
};
