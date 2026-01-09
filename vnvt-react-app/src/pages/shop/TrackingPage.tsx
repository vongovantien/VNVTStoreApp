import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Search, Package, Truck, CheckCircle, Clock, AlertCircle } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { useToast } from '@/store';

export const TrackingPage = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const [orderCode, setOrderCode] = useState('');
    const [orderInfo, setOrderInfo] = useState<null | {
        code: string;
        status: string;
        date: string;
        steps: { title: string; time: string; done: boolean }[];
    }>(null);
    const [loading, setLoading] = useState(false);

    const handleSearch = (e: React.FormEvent) => {
        e.preventDefault();
        if (!orderCode.trim()) return;

        setLoading(true);
        // Simulate API call
        setTimeout(() => {
            if (orderCode.toUpperCase().startsWith('VN')) {
                setOrderInfo({
                    code: orderCode.toUpperCase(),
                    status: 'shipping',
                    date: '08/01/2026',
                    steps: [
                        { title: t('tracking.stepConfirmed'), time: '08/01 10:30', done: true },
                        { title: t('tracking.stepProcessing'), time: '08/01 14:00', done: true },
                        { title: t('tracking.stepShipping'), time: '09/01 08:00', done: true },
                        { title: t('tracking.stepDelivered'), time: '', done: false },
                    ]
                });
            } else {
                toast.error(t('tracking.notFound'));
                setOrderInfo(null);
            }
            setLoading(false);
        }, 1000);
    };

    return (
        <div className="min-h-screen bg-secondary py-12">
            <div className="container mx-auto px-4">
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="text-center mb-12"
                >
                    <h1 className="text-4xl font-bold mb-4">📦 {t('tracking.title')}</h1>
                    <p className="text-secondary text-lg max-w-xl mx-auto">{t('tracking.subtitle')}</p>
                </motion.div>

                {/* Search Form */}
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.1 }}
                    className="max-w-xl mx-auto mb-12"
                >
                    <form onSubmit={handleSearch} className="bg-primary rounded-2xl p-6 shadow-lg">
                        <Input
                            label={t('tracking.orderCode')}
                            placeholder={t('tracking.placeholder')}
                            leftIcon={<Search size={18} />}
                            value={orderCode}
                            onChange={(e) => setOrderCode(e.target.value)}
                        />
                        <Button type="submit" fullWidth className="mt-4" isLoading={loading}>
                            {t('tracking.search')}
                        </Button>
                    </form>
                </motion.div>

                {/* Order Info */}
                {orderInfo && (
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="max-w-2xl mx-auto bg-primary rounded-2xl p-8 shadow-lg"
                    >
                        <div className="flex items-center justify-between mb-8">
                            <div>
                                <p className="text-sm text-tertiary">{t('tracking.orderCode')}</p>
                                <p className="text-xl font-bold">{orderInfo.code}</p>
                            </div>
                            <div className="text-right">
                                <p className="text-sm text-tertiary">{t('tracking.orderDate')}</p>
                                <p className="font-medium">{orderInfo.date}</p>
                            </div>
                        </div>

                        {/* Timeline */}
                        <div className="space-y-6">
                            {orderInfo.steps.map((step, index) => (
                                <div key={index} className="flex gap-4">
                                    <div className="flex flex-col items-center">
                                        <div className={`w-10 h-10 rounded-full flex items-center justify-center ${step.done
                                                ? 'bg-success text-white'
                                                : 'bg-secondary text-tertiary'
                                            }`}>
                                            {step.done ? <CheckCircle size={20} /> : <Clock size={20} />}
                                        </div>
                                        {index < orderInfo.steps.length - 1 && (
                                            <div className={`w-0.5 h-12 mt-2 ${step.done ? 'bg-success' : 'bg-secondary'}`} />
                                        )}
                                    </div>
                                    <div className="pt-2">
                                        <p className={`font-medium ${step.done ? 'text-primary' : 'text-tertiary'}`}>
                                            {step.title}
                                        </p>
                                        {step.time && <p className="text-sm text-tertiary">{step.time}</p>}
                                    </div>
                                </div>
                            ))}
                        </div>
                    </motion.div>
                )}
            </div>
        </div>
    );
};

export default TrackingPage;
