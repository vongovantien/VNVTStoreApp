import React from 'react';
import { useTranslation } from 'react-i18next';
import { Sparkles, CreditCard, ShieldCheck, TrendingUp } from 'lucide-react';
import { formatCurrency } from '@/utils/format';

interface LoyaltySummaryProps {
    points: number;
    debtLimit: number;
    currentDebt: number;
}

export const LoyaltySummary: React.FC<LoyaltySummaryProps> = ({ points, debtLimit, currentDebt }) => {
    const { t } = useTranslation();

    const membership = points >= 5000 ? { label: t('membership.gold', 'Vàng'), color: 'from-amber-400 to-yellow-500', icon: '🏆' } :
                       points >= 1000 ? { label: t('membership.silver', 'Bạc'), color: 'from-slate-300 to-slate-400', icon: '🥈' } :
                       { label: t('membership.bronze', 'Đồng'), color: 'from-orange-300 to-orange-400', icon: '🥉' };

    const debtPercentage = debtLimit > 0 ? Math.min(100, (currentDebt / debtLimit) * 100) : 0;

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 animate-fade-in">
            {/* Loyalty Card */}
            <div className={`relative overflow-hidden rounded-[2rem] p-8 border border-white/20 shadow-xl bg-gradient-to-br ${membership.color} text-white`}>
                <div className="absolute top-0 right-0 -mr-8 -mt-8 w-32 h-32 bg-white/20 rounded-full blur-3xl" />
                <div className="relative z-10">
                    <div className="flex justify-between items-start mb-6">
                        <div className="p-3 bg-white/20 backdrop-blur-md rounded-2xl">
                            <Sparkles size={24} className="text-white" />
                        </div>
                        <span className="text-4xl">{membership.icon}</span>
                    </div>
                    <p className="text-sm font-bold uppercase tracking-widest opacity-80 mb-1">{t('common.account.loyaltyPoints', 'Điểm tích lũy')}</p>
                    <h3 className="text-4xl font-black tracking-tighter mb-4">{points.toLocaleString()} <span className="text-lg opacity-80 uppercase tracking-normal">{t('common.points', 'điểm')}</span></h3>
                    <div className="inline-flex items-center gap-2 px-4 py-1.5 bg-white/20 backdrop-blur-md rounded-full border border-white/30">
                        <ShieldCheck size={14} />
                        <span className="text-xs font-black uppercase tracking-widest">{t('membership.level', 'Hạng {{level}}', { level: membership.label })}</span>
                    </div>
                </div>
            </div>

            {/* Debt/Credit Card */}
            <div className="bg-primary rounded-[2rem] p-8 border border-secondary/5 shadow-2xl shadow-indigo-500/5 flex flex-col justify-between">
                <div>
                    <div className="flex justify-between items-center mb-6">
                        <div className="flex items-center gap-3">
                            <div className="p-3 bg-indigo-50 dark:bg-indigo-900/30 rounded-2xl text-indigo-600">
                                <CreditCard size={24} />
                            </div>
                            <h4 className="font-extrabold text-primary">{t('common.account.creditStatus', 'Tình trạng công nợ')}</h4>
                        </div>
                        {debtLimit > 0 && <TrendingUp size={20} className="text-emerald-500" />}
                    </div>
                    
                    <div className="space-y-4">
                        <div className="flex justify-between items-end">
                            <div>
                                <p className="text-[10px] font-black text-tertiary uppercase tracking-widest mb-1">{t('common.account.currentDebt', 'Dư nợ hiện tại')}</p>
                                <p className={`text-2xl font-black tracking-tight ${currentDebt > 0 ? 'text-error' : 'text-primary'}`}>
                                    {formatCurrency(currentDebt)}
                                </p>
                            </div>
                            <div className="text-right">
                                <p className="text-[10px] font-black text-tertiary uppercase tracking-widest mb-1">{t('common.account.debtLimit', 'Hạn mức')}</p>
                                <p className="text-sm font-bold text-primary opacity-80">{formatCurrency(debtLimit)}</p>
                            </div>
                        </div>

                        {/* Progress Bar */}
                        <div className="relative h-2.5 bg-secondary/10 rounded-full overflow-hidden">
                            <div 
                                className={`absolute left-0 top-0 h-full transition-all duration-1000 ${
                                    debtPercentage > 90 ? 'bg-error' : debtPercentage > 70 ? 'bg-warning' : 'bg-indigo-500'
                                }`}
                                style={{ width: `${debtPercentage}%` }}
                            />
                        </div>
                        <div className="flex justify-between text-[10px] font-bold text-tertiary uppercase tracking-tighter">
                            <span>0%</span>
                            <span>{debtPercentage.toFixed(1)}% {t('common.account.used', 'đã dùng')}</span>
                            <span>100%</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};
