import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Tag, Percent, Clock, Gift } from 'lucide-react';
import { Button } from '@/components/ui';

const promotions = [
    {
        id: 1,
        title: 'Giảm 20% cho đơn hàng đầu tiên',
        description: 'Sử dụng mã WELCOME20 để nhận ưu đãi ngay',
        code: 'WELCOME20',
        discount: '20%',
        validUntil: '31/01/2026',
        bgGradient: 'from-indigo-500 to-purple-600',
    },
    {
        id: 2,
        title: 'Miễn phí vận chuyển',
        description: 'Cho đơn hàng từ 500,000đ trở lên',
        code: 'FREESHIP',
        discount: 'Free Ship',
        validUntil: '28/02/2026',
        bgGradient: 'from-emerald-500 to-teal-600',
    },
    {
        id: 3,
        title: 'Flash Sale cuối tuần',
        description: 'Giảm đến 50% các sản phẩm điện gia dụng',
        code: 'WEEKEND50',
        discount: '50%',
        validUntil: '15/01/2026',
        bgGradient: 'from-rose-500 to-pink-600',
    },
];

export const PromotionsPage = () => {
    const { t } = useTranslation();

    return (
        <div className="min-h-screen bg-secondary py-12">
            <div className="container mx-auto px-4">
                {/* Header */}
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="text-center mb-12"
                >
                    <h1 className="text-4xl font-bold mb-4">🎉 Khuyến mãi đặc biệt</h1>
                    <p className="text-secondary text-lg max-w-2xl mx-auto">
                        Khám phá các ưu đãi hấp dẫn dành riêng cho bạn tại VNVT Store
                    </p>
                </motion.div>

                {/* Promotions Grid */}
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {promotions.map((promo, index) => (
                        <motion.div
                            key={promo.id}
                            initial={{ opacity: 0, y: 20 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ delay: index * 0.1 }}
                            className="bg-primary rounded-2xl overflow-hidden shadow-lg"
                        >
                            <div className={`bg-gradient-to-r ${promo.bgGradient} p-6 text-white`}>
                                <div className="flex items-center justify-between mb-4">
                                    <Tag size={24} />
                                    <span className="bg-white/20 px-3 py-1 rounded-full text-sm font-medium">
                                        {promo.discount}
                                    </span>
                                </div>
                                <h3 className="text-xl font-bold mb-2">{promo.title}</h3>
                                <p className="text-white/80 text-sm">{promo.description}</p>
                            </div>
                            <div className="p-6">
                                <div className="flex items-center gap-2 mb-4 text-sm text-secondary">
                                    <Clock size={16} />
                                    <span>Hết hạn: {promo.validUntil}</span>
                                </div>
                                <div className="bg-secondary rounded-lg p-3 mb-4 text-center">
                                    <span className="text-xs text-tertiary">Mã giảm giá</span>
                                    <p className="text-lg font-bold font-mono">{promo.code}</p>
                                </div>
                                <Button fullWidth variant="outline">
                                    Sao chép mã
                                </Button>
                            </div>
                        </motion.div>
                    ))}
                </div>

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
                            className="flex-1 px-4 py-3 rounded-lg bg-white/20 border border-white/30 placeholder-white/60 text-white focus:outline-none focus:border-white"
                        />
                        <Button className="bg-white text-indigo-600 hover:bg-white/90">Đăng ký</Button>
                    </div>
                </motion.div>
            </div>
        </div>
    );
};

export default PromotionsPage;
