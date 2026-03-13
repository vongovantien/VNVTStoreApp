import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Ticket, ChevronRight, Clock, Info } from 'lucide-react';
import { promotionService, type Promotion } from '@/services/promotionService';
import { Badge, Drawer } from '@/components/ui';
import { formatCurrency } from '@/utils/format';

interface CouponSelectorProps {
    isOpen: boolean;
    onClose: () => void;
    onSelect: (couponCode: string) => void;
    currentSubtotal: number;
}

export const CouponSelector: React.FC<CouponSelectorProps> = ({ 
    isOpen, 
    onClose, 
    onSelect,
    currentSubtotal 
}) => {
    const { t } = useTranslation();
    const [coupons, setCoupons] = useState<Promotion[]>([]);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        if (isOpen) {
            fetchCoupons();
        }
    }, [isOpen]);

    const fetchCoupons = async () => {
        setIsLoading(true);
        try {
            const res = await promotionService.getAll({ activeOnly: true });
            if (res.success && res.data) {
                setCoupons(res.data.items || []);
            }
        } catch (error) {
            console.error('Failed to fetch coupons', error);
        } finally {
            setIsLoading(false);
        }
    };

    const isAvailable = (coupon: Promotion) => {
        if (coupon.minOrderAmount && currentSubtotal < coupon.minOrderAmount) return false;
        return true;
    };

    return (
        <Drawer
            isOpen={isOpen}
            onClose={onClose}
            title={t('checkout.availableCoupons', 'Mã giảm giá có sẵn')}
            position="right"
            size="md"
        >
            <div className="space-y-4">
                {isLoading ? (
                    <div className="flex flex-col items-center justify-center py-12 space-y-4">
                        <div className="w-10 h-10 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin" />
                        <p className="text-slate-400 text-sm font-medium">{t('common.loading')}</p>
                    </div>
                ) : coupons.length > 0 ? (
                    coupons.map((coupon) => {
                        const available = isAvailable(coupon);
                        return (
                            <div 
                                key={coupon.code}
                                onClick={() => available && onSelect(coupon.code)}
                                className={`relative group border-2 rounded-2xl p-4 transition-all overflow-hidden ${
                                    available 
                                    ? 'bg-white border-slate-100 hover:border-indigo-600 cursor-pointer shadow-sm hover:shadow-md' 
                                    : 'bg-slate-50 border-slate-100 opacity-70 grayscale cursor-not-allowed'
                                }`}
                            >
                                {/* Coupon Design Element */}
                                <div className="absolute top-0 left-0 w-1.5 h-full bg-indigo-600 group-hover:bg-indigo-700 transition-colors" />
                                
                                <div className="flex justify-between items-start">
                                    <div className="flex-1">
                                        <div className="flex items-center gap-2 mb-1">
                                            <Badge color={available ? 'primary' : 'secondary'} className="font-black tracking-widest px-3 py-1 bg-indigo-50 text-indigo-700 border-indigo-100">
                                                {coupon.code}
                                            </Badge>
                                            {coupon.discountType === 'PERCENTAGE' ? (
                                                <Badge color="error">-{coupon.discountValue}%</Badge>
                                            ) : (
                                                <Badge color="error">-{formatCurrency(coupon.discountValue)}</Badge>
                                            )}
                                        </div>
                                        <h4 className="font-bold text-slate-800 dark:text-white leading-tight">
                                            {coupon.name}
                                        </h4>
                                        <p className="text-xs text-slate-500 mt-1 line-clamp-2">
                                            {coupon.description || t('checkout.couponNoDescription', 'Tiết kiệm hơn với ưu đãi này')}
                                        </p>
                                    </div>
                                    <div className="text-right">
                                        {!available && coupon.minOrderAmount && (
                                            <div className="flex items-center gap-1.5 text-[10px] font-bold text-rose-500 mb-1">
                                                <Info size={12} />
                                                <span>{t('checkout.minSpend', 'Chi tiêu thêm {{amount}}', { 
                                                    amount: formatCurrency(coupon.minOrderAmount - currentSubtotal) 
                                                })}</span>
                                            </div>
                                        )}
                                        <div className="flex items-center gap-1.5 text-[10px] text-slate-400 font-medium">
                                            <Clock size={12} />
                                            <span>Hết hạn: {new Date(coupon.endDate).toLocaleDateString()}</span>
                                        </div>
                                    </div>
                                </div>

                                {available && (
                                    <div className="mt-4 flex items-center justify-end">
                                        <span className="text-xs font-bold text-indigo-600 flex items-center gap-1 group-hover:translate-x-1 transition-transform">
                                            {t('common.useNow', 'Dùng ngay')}
                                            <ChevronRight size={14} />
                                        </span>
                                    </div>
                                )}
                            </div>
                        );
                    })
                ) : (
                    <div className="text-center py-12 px-6">
                        <div className="w-16 h-16 bg-slate-50 rounded-full flex items-center justify-center mx-auto mb-4">
                            <Ticket size={32} className="text-slate-300" />
                        </div>
                        <h3 className="text-slate-900 font-bold mb-1">{t('checkout.noCoupons', 'Không tìm thấy ưu đãi')}</h3>
                        <p className="text-sm text-slate-500">{t('checkout.noCouponsHint', 'Hiện không có mã giảm giá nào khả dụng cho bạn.')}</p>
                    </div>
                )}
            </div>
            
            {!isLoading && coupons.length > 0 && (
                <div className="mt-6 pt-4 border-t border-slate-100 text-[10px] text-slate-400 text-center font-medium italic">
                    {t('checkout.couponPolicy', '* Mỗi đơn hàng chỉ có thể áp dụng 01 mã giảm giá.')}
                </div>
            )}
        </Drawer>
    );
};
