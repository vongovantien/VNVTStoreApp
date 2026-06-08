import { UseFormReturn } from 'react-hook-form';
import { ProductFormData } from './ProductForm';
import { Button, Input, Badge } from '@/components/ui';
import { X } from 'lucide-react';

interface ProductSpecsSectionProps {
    form: UseFormReturn<ProductFormData>;
    t: (key: string) => string;
}

interface ProductDetail {
  detailType: 'SPEC' | 'LOGISTICS' | 'RELATION' | 'IMAGE';
  specName: string;
  specValue: string;
}

export const ProductSpecsSection = ({ form, t }: ProductSpecsSectionProps) => {
    const details = form.watch('details') || [];
    const specSuggestions = ["Công suất", "Điện áp", "Lưu lượng", "Phi (Ø)", "Đường kính", "Độ dày", "Chất liệu", "Màu sắc"];

    const addDetail = (type: 'SPEC' | 'LOGISTICS' | 'RELATION' | 'IMAGE') => 
        form.setValue('details', [...details, { detailType: type, specName: '', specValue: '' }]);

    const updateDetail = (idx: number, f: keyof ProductDetail, val: string) => {
        const newDetails = [...details];
        newDetails[idx] = { ...newDetails[idx], [f]: val } as ProductDetail;
        form.setValue('details', newDetails);
    };

    return (
        <div className="space-y-4">
            <div className="flex gap-2 mb-2">
                <Button type="button" size="sm" variant="outline" onClick={() => addDetail('SPEC')}>+ {t('common.fields.specs')}</Button>
                <Button type="button" size="sm" variant="outline" onClick={() => addDetail('LOGISTICS')}>+ Logistics</Button>
            </div>
            <div className="space-y-3 bg-slate-50 dark:bg-slate-900/50 p-4 rounded-xl border border-border-color">
                {details.length === 0 ? (
                    <p className="text-center text-sm text-tertiary py-4">{t('common.noData')}</p>
                ) : (
                    details.map((detail, idx: number) => {
                        const dType = detail.detailType || 'SPEC';
                        const badgeColor = dType === 'SPEC' ? 'primary' : dType === 'LOGISTICS' ? 'success' : 'warning';
                        const badgeLabel = dType === 'SPEC' ? t('common.types.spec', 'SPEC') : dType === 'LOGISTICS' ? t('common.types.logistics', 'LOGIS') : t('common.types.tag', 'TAG');
                        return (
                            <div key={idx} className="flex items-center gap-3">
                                <Badge color={badgeColor} className="w-20 justify-center">{badgeLabel}</Badge>
                                <Input 
                                    className="flex-1 h-9 text-sm" 
                                    placeholder={t('common.fields.name')} 
                                    value={detail.specName} 
                                    onChange={(e) => updateDetail(idx, 'specName', e.target.value)} 
                                    list={`spec-suggestions-${idx}`} 
                                />
                                <datalist id={`spec-suggestions-${idx}`}>
                                    {specSuggestions.map(s => <option key={s} value={s} />)}
                                </datalist>
                                <Input 
                                    className="flex-1 h-9 text-sm" 
                                    placeholder={t('common.fields.value')} 
                                    value={detail.specValue} 
                                    onChange={(e) => updateDetail(idx, 'specValue', e.target.value)} 
                                />
                                <button 
                                    type="button" 
                                    onClick={() => form.setValue('details', details.filter((_, i) => i !== idx))} 
                                    className="text-slate-400 hover:text-red-500"
                                >
                                    <X size={18} />
                                </button>
                            </div>
                        );
                    })
                )}
            </div>
        </div>
    );
};
