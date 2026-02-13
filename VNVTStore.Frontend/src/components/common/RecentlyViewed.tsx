import React from 'react';
import { useTranslation } from 'react-i18next';
import { useRecentStore } from '@/store';
import { ProductCard } from '@/components/common/ProductCard';
import { motion } from 'framer-motion';
import { History } from 'lucide-react';

export const RecentlyViewed: React.FC = () => {
    const { t } = useTranslation();
    const { viewedProducts, clearRecent } = useRecentStore();

    if (viewedProducts.length === 0) return null;

    return (
        <section className="py-12 bg-white dark:bg-slate-900 overflow-hidden">
            <div className="container mx-auto px-4">
                <div className="flex justify-between items-end mb-8">
                    <div>
                        <div className="flex items-center gap-2 text-indigo-600 font-bold text-sm uppercase tracking-widest mb-2">
                            <History size={16} />
                            {t('recent.browsingHistory', 'Lịch sử xem')}
                        </div>
                        <h2 className="text-3xl font-bold text-slate-900 dark:text-white">
                            {t('recent.title', 'Sản phẩm vừa xem')}
                        </h2>
                    </div>
                    <button 
                        onClick={clearRecent}
                        className="text-sm text-slate-400 hover:text-rose-500 transition-colors font-medium"
                    >
                        {t('recent.clearAll', 'Xóa tất cả')}
                    </button>
                </div>

                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-6">
                    {viewedProducts.map((product, index) => (
                        <motion.div
                            key={product.code}
                            initial={{ opacity: 0, y: 20 }}
                            whileInView={{ opacity: 1, y: 0 }}
                            viewport={{ once: true }}
                            transition={{ delay: index * 0.1 }}
                        >
                            <ProductCard product={product} />
                        </motion.div>
                    ))}
                </div>
            </div>
        </section>
    );
};
