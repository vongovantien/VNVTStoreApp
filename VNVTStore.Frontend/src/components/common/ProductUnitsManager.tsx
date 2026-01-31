import { useTranslation } from 'react-i18next';
import { useState, useMemo, useEffect } from 'react';
import { Checkbox, Button, Badge, ConfirmDialog, Input } from '@/components/ui';
import { Edit, Trash2, Plus, X } from 'lucide-react';
import LazySelect from '@/components/ui/LazySelect';

export interface ProductUnitDto {
    code?: string;
    unitName: string;
    conversionRate: number;
    price: number;
    isActive?: boolean;
    isBaseUnit?: boolean;
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

    const handleUpdateUnit = (index: number, field: keyof ProductUnitDto, value: any) => {
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
                        {new Intl.NumberFormat('vi-VN').format(baseUnitPrice || 0)} đ
                    </div>
                    <div className="col-span-1"></div>
                </div>

                {/* Editable Units */}
                {units.map((unit, idx) => (
                    <div key={unit.code || idx} className="grid grid-cols-12 gap-3 items-center" data-testid={`unit-row-${idx}`}>
                        <div className="col-span-4">
                            <LazySelect
                                value={unit.unitName}
                                onChange={(val) => handleUpdateUnit(idx, 'unitName', val)}
                                fetchFn={fetchUnitOptions}
                                placeholder={t('common.fields.unitName')}
                                initialLabel={unit.unitName}
                                queryKeyPrefix={`unit-select-${idx}`}
                                className="h-9 text-sm"
                            />
                        </div>
                        <div className="col-span-3 relative">
                            <Input 
                                type="number"
                                className="h-9 text-sm pr-12" 
                                placeholder="1" 
                                value={unit.conversionRate} 
                                onChange={(e) => handleUpdateUnit(idx, 'conversionRate', Number(e.target.value))} 
                            />
                             <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-slate-400 pointer-events-none">
                                x {baseUnitName}
                            </span>
                        </div>
                        <div className="col-span-4">
                            <Input 
                                type="number"
                                className="h-9 text-sm" 
                                placeholder={t('common.fields.price')} 
                                value={unit.price} 
                                onChange={(e) => handleUpdateUnit(idx, 'price', Number(e.target.value))} 
                            />
                        </div>
                        <div className="col-span-1 flex justify-end">
                            <button 
                                type="button" 
                                onClick={() => setUnitToDeleteIdx(idx)} 
                                className="text-slate-400 hover:text-red-500 p-1 rounded-md hover:bg-red-50 transition-colors"
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
