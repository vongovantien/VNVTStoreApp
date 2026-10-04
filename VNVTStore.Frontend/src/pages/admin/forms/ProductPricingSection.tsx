import { UseFormReturn } from 'react-hook-form';
import { ProductFormData } from './ProductForm';
import { Input } from '@/components/ui';
import { cn } from '@/utils/cn';
import { Phone } from 'lucide-react';

interface ProductPricingSectionProps {
    form: UseFormReturn<ProductFormData>;
    t: (key: string) => string;
}

export const ProductPricingSection = ({ form, t }: ProductPricingSectionProps) => {
    const price         = form.watch('price') || 0;
    const wholesalePrice= form.watch('wholesalePrice') || 0;
    const costPrice     = form.watch('costPrice') || 0;
    const vatRate       = form.watch('vatRate') || 0;
    const contactPrice  = form.watch('contactPrice') ?? false;

    // ── Helpers ──────────────────────────────────────────────────────────────
    const formatNumber = (num: number) => num.toLocaleString('vi-VN');
    const parseNumber  = (str: string) => {
        const cleaned = str.replace(/[^\d]/g, '');
        return cleaned ? parseInt(cleaned, 10) : 0;
    };

    // ── Toggle handler ───────────────────────────────────────────────────────
    const handleContactPriceToggle = (checked: boolean) => {
        form.setValue('contactPrice', checked, { shouldValidate: true });
        if (checked) {
            // Khi bật "Giá liên hệ" — reset price về 0 để không gây nhầm
            form.setValue('price', 0, { shouldValidate: true });
        }
    };

    // ── Auto-calculate price từ cost + VAT ───────────────────────────────────
    const handleCostChange = (newCost: number) => {
        form.setValue('costPrice', newCost, { shouldValidate: true });
        if (newCost > 0 && !contactPrice) {
            form.setValue('price', Math.round(newCost * (1 + vatRate / 100)), { shouldValidate: true });
        }
    };

    const handleVatChange = (newVat: number) => {
        form.setValue('vatRate', newVat, { shouldValidate: true });
        if (costPrice > 0 && !contactPrice) {
            form.setValue('price', Math.round(costPrice * (1 + newVat / 100)), { shouldValidate: true });
        }
    };

    // ── Validation warnings ───────────────────────────────────────────────────
    const warnings: string[] = [];
    if (!contactPrice) {
        if (wholesalePrice > 0 && wholesalePrice >= price) warnings.push('⚠️ Giá bán sỉ phải nhỏ hơn Giá lẻ');
        if (costPrice > 0 && costPrice >= price)            warnings.push('⚠️ Giá vốn phải nhỏ hơn Giá bán');
    }

    const profit       = contactPrice ? 0 : price - costPrice;
    const profitMargin = price > 0 && !contactPrice ? ((profit / price) * 100).toFixed(1) : '0';

    return (
        <div className="space-y-4">

            {/* ── Contact price toggle ── */}
            <div className={cn(
                'flex items-center justify-between p-3 rounded-lg border transition-colors',
                contactPrice
                    ? 'bg-amber-50 dark:bg-amber-950/30 border-amber-300 dark:border-amber-700'
                    : 'bg-bg-secondary border-border',
            )}>
                <div className="flex items-center gap-2">
                    <Phone size={16} className={contactPrice ? 'text-amber-600' : 'text-text-tertiary'} />
                    <div>
                        <p className={cn('text-sm font-semibold', contactPrice ? 'text-amber-700 dark:text-amber-400' : 'text-text-primary')}>
                            Giá liên hệ
                        </p>
                        <p className="text-xs text-text-tertiary">
                            {contactPrice
                                ? 'Hiển thị "Liên hệ" thay vì giá cụ thể'
                                : 'Nhập giá bán cụ thể bên dưới'}
                        </p>
                    </div>
                </div>

                {/* Toggle switch */}
                <button
                    type="button"
                    role="switch"
                    aria-checked={contactPrice}
                    onClick={() => handleContactPriceToggle(!contactPrice)}
                    className={cn(
                        'relative w-11 h-6 rounded-full transition-colors duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-accent',
                        contactPrice ? 'bg-amber-500' : 'bg-bg-tertiary border border-border',
                    )}
                >
                    <span className={cn(
                        'absolute top-0.5 left-0.5 w-5 h-5 rounded-full bg-white shadow-sm transition-transform duration-200',
                        contactPrice ? 'translate-x-5' : 'translate-x-0',
                    )} />
                </button>
            </div>

            {/* ── Price fields ── */}
            <div className="grid grid-cols-12 gap-4">
                <div className="col-span-3">
                    <Input
                        label={t('common.fields.costPrice') || 'Giá vốn'}
                        value={formatNumber(costPrice)}
                        onChange={(e) => handleCostChange(parseNumber(e.target.value))}
                        placeholder="0"
                        className="text-right"
                    />
                </div>

                <div className="col-span-3">
                    <Input
                        label={t('common.fields.wholesalePrice') || 'Giá sỉ'}
                        value={formatNumber(wholesalePrice)}
                        onChange={(e) => form.setValue('wholesalePrice', parseNumber(e.target.value), { shouldValidate: true })}
                        placeholder="0"
                        className={cn('text-right', wholesalePrice > 0 && wholesalePrice >= price && !contactPrice && 'border-warning')}
                        disabled={contactPrice}
                    />
                </div>

                <div className="col-span-2">
                    <Input
                        label={t('common.fields.vatPercent') || 'VAT (%)'}
                        type="number"
                        value={vatRate}
                        onChange={(e) => handleVatChange(parseFloat(e.target.value) || 0)}
                        placeholder="0"
                        className="text-right"
                    />
                </div>

                <div className="col-span-4">
                    {contactPrice ? (
                        /* Giá liên hệ — disabled placeholder */
                        <div className="flex flex-col gap-1">
                            <label className="text-sm font-bold text-text-primary">
                                Giá bán <span className="text-text-tertiary font-normal">(vô hiệu)</span>
                            </label>
                            <div className={cn(
                                'h-10 px-4 flex items-center rounded-lg border border-border',
                                'bg-bg-tertiary text-text-tertiary select-none cursor-not-allowed',
                            )}>
                                <Phone size={14} className="mr-2 text-amber-500 shrink-0" />
                                <span className="text-sm italic font-medium text-amber-600">Giá liên hệ</span>
                            </div>
                            <span className="text-[11px] text-amber-600">
                                Khách hàng sẽ thấy "Liên hệ" thay vì số tiền
                            </span>
                        </div>
                    ) : (
                        <Input
                            id="product-price-input"
                            data-testid="product-price-input"
                            label={`${t('common.fields.price') || 'Giá'} bán`}
                            isRequired
                            value={formatNumber(price)}
                            onChange={(e) => form.setValue('price', parseNumber(e.target.value), { shouldValidate: true })}
                            placeholder="0"
                            className="text-right font-semibold bg-green-50 dark:bg-green-900/20 border-green-300"
                        />
                    )}
                </div>
            </div>

            {/* ── Profit summary (chỉ hiện khi có giá cụ thể) ── */}
            {!contactPrice && (
                <div className="flex items-center gap-4 p-3 rounded-lg bg-bg-secondary border border-border">
                    <div className="flex-1">
                        <span className="text-sm text-text-secondary">Lợi nhuận gộp:</span>
                        <span className={cn('ml-2 font-semibold', profit >= 0 ? 'text-success' : 'text-error')}>
                            {formatNumber(profit)} đ
                        </span>
                        <span className="ml-2 text-sm text-text-tertiary">({profitMargin}%)</span>
                    </div>
                </div>
            )}

            {/* ── Warnings ── */}
            {warnings.length > 0 && (
                <div className="text-sm text-warning space-y-1">
                    {warnings.map((w, i) => <div key={i}>{w}</div>)}
                </div>
            )}
        </div>
    );
};
