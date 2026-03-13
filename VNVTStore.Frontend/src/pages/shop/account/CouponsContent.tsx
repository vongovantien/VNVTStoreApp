import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Ticket, Clock, Info } from 'lucide-react';
import { promotionService, type Promotion } from '@/services/promotionService';
import { Badge, Loading } from '@/components/ui';
import { formatCurrency } from '@/utils/format';
import { motion } from 'framer-motion';

const CouponsContent = () => {
    const { t } = useTranslation();
    const [coupons, setCoupons] = useState<Promotion[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const fetchCoupons = async () => {
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
        fetchCoupons();
    }, []);

    if (isLoading) return <div className="flex justify-center py-20"><Loading /></div>;

    return (
        <div className="space-y-8 animate-fade-in">
            <div className="bg-primary rounded-[2rem] p-6 md:p-10 border border-secondary/5 shadow-2xl shadow-indigo-500/5 overflow-hidden relative">
                <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-emerald-500 via-teal-500 to-cyan-500 opacity-70"></div>
                
                <div className="flex flex-col md:flex-row items-center justify-between mb-10 pb-6 border-b border-secondary/5 gap-4">
                    <div className="text-center md:text-left">
                        <h2 className="text-3xl font-extrabold text-primary tracking-tight lg:text-4xl">
                            {t('common.account.myCoupons', 'Ưu đãi của tôi')}
                        </h2>
                        <p className="text-sm text-tertiary mt-2 font-medium">
                            {t('common.account.couponsSubtitle', 'Khám phá và sử dụng các mã giảm giá dành riêng cho bạn')}
                        </p>
                    </div>
                </div>

                {coupons.length > 0 ? (
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        {coupons.map((coupon) => (
                            <motion.div 
                                key={coupon.code}
                                whileHover={{ y: -5 }}
                                className="relative group border-2 border-slate-100 dark:border-slate-800 rounded-3xl p-6 transition-all bg-white dark:bg-slate-900 shadow-sm hover:shadow-xl hover:border-indigo-500/50"
                            >
                                {/* Decorative elements */}
                                <div className="absolute -left-3 top-1/2 -translate-y-1/2 w-6 h-6 bg-gray-50 dark:bg-slate-950 rounded-full border-r-2 border-slate-100 dark:border-slate-800" />
                                <div className="absolute -right-3 top-1/2 -translate-y-1/2 w-6 h-6 bg-gray-50 dark:bg-slate-950 rounded-full border-l-2 border-slate-100 dark:border-slate-800" />
                                
                                <div className="flex gap-6 items-start">
                                    <div className="w-16 h-16 rounded-2xl bg-indigo-50 dark:bg-indigo-900/30 flex items-center justify-center text-indigo-600 shrink-0 border border-indigo-100 dark:border-indigo-800">
                                        <Ticket size={32} />
                                    </div>
                                    <div className="flex-1 space-y-2">
                                        <div className="flex items-center gap-2">
                                            <Badge className="font-black tracking-widest px-3 py-1 bg-indigo-600 text-white rounded-lg border-none">
                                                {coupon.code}
                                            </Badge>
                                            {coupon.discountType === 'PERCENTAGE' ? (
                                                <Badge color="error" className="rounded-lg font-bold">-{coupon.discountValue}%</Badge>
                                            ) : (
                                                <Badge color="error" className="rounded-lg font-bold">-{formatCurrency(coupon.discountValue)}</Badge>
                                            )}
                                        </div>
                                        <h4 className="font-black text-primary leading-tight text-lg">
                                            {coupon.name}
                                        </h4>
                                        <p className="text-sm text-tertiary line-clamp-2 font-medium">
                                            {coupon.description || t('checkout.couponNoDescription', 'Tiết kiệm hơn với ưu đãi này')}
                                        </p>
                                        
                                        <div className="pt-4 flex items-center justify-between border-t border-secondary/5 mt-4">
                                            <div className="flex items-center gap-1.5 text-xs text-secondary font-bold">
                                                <Clock size={14} className="text-tertiary" />
                                                <span>{t('common.expiry', 'Hết hạn')}: {new Date(coupon.endDate).toLocaleDateString()}</span>
                                            </div>
                                            {coupon.minOrderAmount && (
                                                <div className="flex items-center gap-1.5 text-[10px] font-black text-rose-500 uppercase tracking-tighter">
                                                    <Info size={12} />
                                                    <span>{t('checkout.minOrder', 'Đơn từ {{amount}}', { amount: formatCurrency(coupon.minOrderAmount) })}</span>
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                </div>
                            </motion.div>
                        ))}
                    </div>
                ) : (
                    <div className="text-center py-20 px-6">
                        <div className="w-24 h-24 bg-secondary/5 rounded-full flex items-center justify-center mx-auto mb-6 text-tertiary/20">
                            <Ticket size={48} />
                        </div>
                        <h3 className="text-primary font-black text-xl mb-2">{t('checkout.noCoupons', 'Không tìm thấy ưu đãi')}</h3>
                        <p className="text-tertiary max-w-xs mx-auto font-medium">{t('checkout.noCouponsHint', 'Hiện không có mã giảm giá nào khả dụng cho bạn.')}</p>
                    </div>
                )}
            </div>
        </div>
    );
};

export default CouponsContent;
