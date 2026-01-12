import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Tag, Clock, Gift, Copy, Check } from 'lucide-react';
import { Button } from '@/components/ui';
import { useQuery } from '@tanstack/react-query';
import { promotionService, type Promotion } from '@/services/promotionService';
import { formatCurrency, formatDate } from '@/utils/format';
import { useToast } from '@/store';
import { useState } from 'react';

export const PromotionsPage = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const [copiedCode, setCopiedCode] = useState<string | null>(null);

    // Fetch Promotions
    const { data: promotionsResponse, isLoading } = useQuery({
        queryKey: ['promotions', 'active'],
        queryFn: () => promotionService.getAll({}), // Fetch all, filtered below
    });

    // Handle data structure (ApiResponse wrapping)
    const allPromotions: Promotion[] = promotionsResponse?.data?.items || [];

    // Filter Active Only
    const activePromotions = allPromotions.filter(p => 
        p.isActive && 
        new Date(p.endDate) > new Date() &&
        new Date(p.startDate) <= new Date() &&
        // Exclude Flash Sales (productCodes > 0 usually indicates flash sale, but not always strictly. 
        // Based on Admin Form, Flash Sale HAS products, Voucher usually doesn't or applies to all.
        // Let's rely on standard logic: if it has many products, it's a "Flash Sale" or specific product discount.
        // User said "Promotions Page List" -> "List Active Vouchers".
        // Let's Include ALL that are NOT strictly Flash Sale Type if distinguishable, or just ALL active.
        // Helper: Flash Sale usually has productCodes. Voucher might allow null productCodes (applies to order).
        // Let's show all active promotions.
        true
    );

    const handleCopy = (code: string) => {
        navigator.clipboard.writeText(code);
        setCopiedCode(code);
        toast.success(t('common.copied') || 'Đã sao chép mã!');
        setTimeout(() => setCopiedCode(null), 2000);
    };

    const getGradient = (index: number) => {
        const gradients = [
            'from-indigo-500 to-purple-600',
            'from-emerald-500 to-teal-600',
            'from-rose-500 to-pink-600',
            'from-blue-500 to-cyan-600',
            'from-amber-500 to-orange-600'
        ];
        return gradients[index % gradients.length];
    };

    return (
        <div className="min-h-screen bg-secondary py-12">
            <div className="container mx-auto px-4">
                {/* Header */}
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="text-center mb-12"
                >
                    <h1 className="text-4xl font-bold mb-4">🎉 {t('promotions.title') || 'Khuyến mãi đặc biệt'}</h1>
                    <p className="text-secondary text-lg max-w-2xl mx-auto">
                        {t('promotions.subtitle') || 'Khám phá các ưu đãi hấp dẫn dành riêng cho bạn tại VNVT Store'}
                    </p>
                </motion.div>

                {/* Promotions Grid */}
                {isLoading ? (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        {[1, 2, 3].map(i => (
                            <div key={i} className="h-64 bg-slate-200 dark:bg-slate-700 rounded-2xl animate-pulse" />
                        ))}
                    </div>
                ) : activePromotions.length > 0 ? (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        {activePromotions.map((promo, index) => (
                            <motion.div
                                key={promo.code}
                                initial={{ opacity: 0, y: 20 }}
                                animate={{ opacity: 1, y: 0 }}
                                transition={{ delay: index * 0.1 }}
                                className="bg-primary rounded-2xl overflow-hidden shadow-lg border border-slate-100 dark:border-slate-800"
                            >
                                <div className={`bg-gradient-to-r ${getGradient(index)} p-6 text-white`}>
                                    <div className="flex items-center justify-between mb-4">
                                        <Tag size={24} />
                                        <span className="bg-white/20 px-3 py-1 rounded-full text-sm font-medium backdrop-blur-sm">
                                            {promo.discountType === 'PERCENTAGE' 
                                                ? `-${promo.discountValue}%` 
                                                : `-${formatCurrency(promo.discountValue)}`}
                                        </span>
                                    </div>
                                    <h3 className="text-xl font-bold mb-2 break-all">{promo.name}</h3>
                                    <p className="text-white/80 text-sm line-clamp-2">{promo.description || t('common.noDescription')}</p>
                                </div>
                                <div className="p-6">
                                    <div className="flex items-center gap-2 mb-4 text-sm text-secondary">
                                        <Clock size={16} className="text-tertiary" />
                                        <span>{t('promotions.validUntil') || 'Hết hạn'}: {formatDate(promo.endDate)}</span>
                                    </div>
                                    
                                    {/* Usage Info if available */}
                                    {promo.usageLimit && (
                                         <div className="text-xs text-orange-500 mb-2 font-medium">
                                            ⚠️ Giới hạn: {promo.usageLimit} lượt dùng
                                         </div>
                                    )}

                                    <div className="bg-secondary rounded-lg p-3 mb-4 text-center border border-dashed border-slate-300 dark:border-slate-600 relative group cursor-pointer" onClick={() => handleCopy(promo.code)}>
                                        <span className="text-xs text-tertiary uppercase tracking-wider">{t('promotions.code') || 'Mã giảm giá'}</span>
                                        <p className="text-lg font-bold font-mono text-primary mt-1 select-all">{promo.code}</p>
                                        <div className="absolute inset-0 bg-black/5 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center rounded-lg">
                                            <span className="text-xs font-bold text-slate-600">Click to Copy</span>
                                        </div>
                                    </div>
                                    <Button 
                                        fullWidth 
                                        variant={copiedCode === promo.code ? "success" : "outline"}
                                        onClick={() => handleCopy(promo.code)}
                                        leftIcon={copiedCode === promo.code ? <Check size={16} /> : <Copy size={16} />}
                                    >
                                        {copiedCode === promo.code ? (t('common.copied') || 'Đã sao chép') : (t('common.copyCode') || 'Sao chép mã')}
                                    </Button>
                                </div>
                            </motion.div>
                        ))}
                    </div>
                ) : (
                    <div className="text-center py-12">
                        <Gift size={64} className="mx-auto text-slate-300 mb-4" />
                        <h3 className="text-xl font-bold text-secondary mb-2">Chưa có khuyến mãi nào</h3>
                        <p className="text-tertiary">Hãy quay lại sau để nhận các ưu đãi hấp dẫn nhé!</p>
                    </div>
                )}

                {/* Newsletter */}
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.4 }}
                    className="mt-16 bg-gradient-to-r from-indigo-600 to-purple-600 rounded-2xl p-8 text-center text-white"
                >
                    <Gift size={48} className="mx-auto mb-4" />
                    <h2 className="text-2xl font-bold mb-2">Đăng ký nhận ưu đãi</h2>
                    <p className="mb-6 text-white/80">Nhận ngay voucher 50k cho đơn hàng tiếp theo!</p>
                    <div className="flex gap-4 max-w-md mx-auto">
                        <input
                            type="email"
                            placeholder="Email của bạn"
                            className="flex-1 px-4 py-2 rounded-lg bg-white text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-white/50"
                        />
                        <Button className="bg-white text-indigo-600 hover:bg-white/90">Đăng ký</Button>
                    </div>
                </motion.div>
            </div>
        </div>
    );
};

export default PromotionsPage;
