import { UseFormReturn } from 'react-hook-form';
import { ProductFormData } from './ProductForm';
import { Input } from '@/components/ui';

interface ProductPricingSectionProps {
    form: UseFormReturn<ProductFormData>;
    t: (key: string) => string;
}

export const ProductPricingSection = ({ form, t }: ProductPricingSectionProps) => {
    const price = form.watch('price') || 0;
    const wholesalePrice = form.watch('wholesalePrice') || 0;
    const costPrice = form.watch('costPrice') || 0;
    const vatRate = form.watch('vatRate') || 0;
    
    // Calculate profit
    const profit = price - costPrice;
    const profitMargin = price > 0 ? ((profit / price) * 100).toFixed(1) : '0';
    
    // Format number with thousand separators
    const formatNumber = (num: number) => num.toLocaleString('vi-VN');
    const parseNumber = (str: string) => {
        const cleaned = str.replace(/[^\d]/g, '');
        return cleaned ? parseInt(cleaned, 10) : 0;
    };
    
    // Validation warnings
    const warnings: string[] = [];
    if (wholesalePrice > 0 && wholesalePrice >= price) {
        warnings.push('⚠️ Giá bán sỉ phải nhỏ hơn Giá lẻ');
    }
    if (costPrice > 0 && costPrice >= price) {
        warnings.push('⚠️ Giá vốn phải nhỏ hơn Giá bán');
    }
    
    // Auto-calculate price from cost + VAT when cost or VAT changes
    const handleCostChange = (newCost: number) => {
        form.setValue('costPrice', newCost, { shouldValidate: true });
        if (newCost > 0) {
            const calculatedPrice = Math.round(newCost * (1 + vatRate / 100));
            form.setValue('price', calculatedPrice, { shouldValidate: true });
        }
    };
    
    const handleVatChange = (newVat: number) => {
        form.setValue('vatRate', newVat, { shouldValidate: true });
        if (costPrice > 0) {
            const calculatedPrice = Math.round(costPrice * (1 + newVat / 100));
            form.setValue('price', calculatedPrice, { shouldValidate: true });
        }
    };
    
    return (
        <div className="space-y-4">
            <div className="grid grid-cols-12 gap-4">
                <div className="col-span-3">
                    <Input
                        label={t('common.fields.costPrice', 'Cost Price')}
                        isRequired
                        value={formatNumber(costPrice)}
                        onChange={(e) => handleCostChange(parseNumber(e.target.value))}
                        placeholder={t('common.placeholders.enterPrice', 'Enter price...')}
                        className="text-right"
                    />
                </div>
                
                <div className="col-span-3">
                    <Input
                        label={t('common.fields.wholesalePrice', 'Wholesale Price')}
                        value={formatNumber(wholesalePrice)}
                        onChange={(e) => form.setValue('wholesalePrice', parseNumber(e.target.value), { shouldValidate: true })}
                        placeholder={t('common.placeholders.enterPrice', 'Enter price...')}
                        className={`text-right ${wholesalePrice >= price && wholesalePrice > 0 ? 'border-yellow-500' : ''}`}
                    />
                </div>
                
                <div className="col-span-2">
                    <Input
                        label={t('common.fields.vatPercent', 'VAT (%)')}
                        type="number"
                        value={vatRate}
                        onChange={(e) => handleVatChange(parseFloat(e.target.value) || 0)}
                        placeholder="0"
                        className="text-right"
                    />
                </div>
                
                <div className="col-span-4">
                    <Input
                        id="product-price-input"
                        data-testid="product-price-input"
                        label={`${t('common.fields.price', 'Price')} bán`}
                        value={formatNumber(price)}
                        onChange={(e) => form.setValue('price', parseNumber(e.target.value), { shouldValidate: true })}
                        placeholder={t('common.placeholders.enterPrice', 'Enter price...')}
                        className="text-right font-semibold bg-green-50 dark:bg-green-900/20 border-green-300"
                    />
                </div>
            </div>
            
            <div className="flex items-center gap-4 p-3 rounded-lg bg-slate-50 dark:bg-slate-900/50 border border-border-color">
                <div className="flex-1">
                    <span className="text-sm text-secondary">Lợi nhuận gộp:</span>
                    <span className={`ml-2 font-semibold ${profit >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {formatNumber(profit)} đ
                    </span>
                    <span className="ml-2 text-sm text-tertiary">({profitMargin}%)</span>
                </div>
            </div>
            
            {warnings.length > 0 && (
                <div className="text-sm text-yellow-600 dark:text-yellow-400 space-y-1">
                    {warnings.map((w, i) => <div key={i}>{w}</div>)}
                </div>
            )}
        </div>
    );
};
