import React, { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Sparkles, ArrowRight, Plus, ShoppingCart } from 'lucide-react';
import { Button } from '@/components/ui';
import { ProductCard } from '@/components/common/ProductCard';
import { useProducts } from '@/hooks';
import type { Product } from '@/types';
import { motion } from 'framer-motion';

interface UpsellSectionProps {
    currentProduct: Product;
}

export const UpsellSection: React.FC<UpsellSectionProps> = ({ currentProduct }) => {
    const { t } = useTranslation();
    
    // Fetch complementary products (different category or brand-mates)
    const { data: upsellData, isLoading } = useProducts({
        pageSize: 4,
        sortField: 'reviewCount', // Proxy for bestseller if explicit field is missing
        sortDir: 'desc'
    });

    const products = useMemo(() => {
        return (upsellData?.products || []).filter(p => p.code !== currentProduct.code);
    }, [upsellData?.products, currentProduct.code]);

    if (!isLoading && products.length === 0) return null;

    return (
        <section className="py-12 bg-slate-50 dark:bg-slate-900/50 rounded-3xl my-12 overflow-hidden border border-slate-100 dark:border-slate-800">
            <div className="px-8 flex flex-col md:flex-row justify-between items-center gap-6 mb-10">
                <div className="text-center md:text-left">
                    <div className="inline-flex items-center gap-2 px-3 py-1 bg-indigo-50 dark:bg-indigo-900/20 text-indigo-600 dark:text-indigo-400 rounded-full text-[10px] font-black uppercase tracking-widest mb-3">
                        <Sparkles size={12} className="fill-current" />
                        {t('product.expertPick', 'Gợi ý từ chuyên gia')}
                    </div>
                    <h2 className="text-3xl font-black text-slate-900 dark:text-white leading-tight">
                        {t('product.boughtTogether', 'Sản phẩm thường mua cùng')}
                    </h2>
                    <p className="text-slate-500 text-sm mt-2 max-w-md">
                        {t('product.upsellHint', 'Những sản phẩm này sẽ là sự kết hợp hoàn hảo cho thiết bị của bạn.')}
                    </p>
                </div>
                <div className="flex gap-4">
                    <div className="hidden lg:flex flex-col items-center justify-center p-4 bg-white dark:bg-slate-800 rounded-2xl shadow-sm border border-slate-100 dark:border-slate-700 min-w-[140px]">
                        <span className="text-2xl font-black text-rose-600">Bundle</span>
                        <span className="text-[10px] font-bold text-slate-400 uppercase tracking-tighter">Tiết kiệm 5%</span>
                    </div>
                </div>
            </div>

            <div className="px-8 grid grid-cols-2 md:grid-cols-4 gap-6">
                {isLoading ? (
                    Array.from({ length: 4 }).map((_, i) => (
                        <div key={i} className="aspect-[3/4] bg-slate-200 dark:bg-slate-800 animate-pulse rounded-2xl" />
                    ))
                ) : (
                    products.slice(0, 4).map((product, index) => (
                        <motion.div
                            key={product.code}
                            initial={{ opacity: 0, y: 30 }}
                            whileInView={{ opacity: 1, y: 0 }}
                            viewport={{ once: true }}
                            transition={{ delay: index * 0.1 }}
                            className="relative group"
                        >
                            <div className="absolute -top-3 -right-3 z-10 hidden group-hover:block transition-all transform scale-110">
                                <Button size="sm" rounded className="w-10 h-10 p-0 shadow-xl border-2 border-white">
                                    <Plus size={20} />
                                </Button>
                            </div>
                            <ProductCard product={product} />
                        </motion.div>
                    ))
                )}
            </div>
            
            <div className="mt-12 text-center">
                <Button variant="ghost" className="text-indigo-600 hover:text-indigo-700 font-bold group">
                    {t('common.viewMoreRecommendations', 'Xem thêm gợi ý')}
                    <ArrowRight size={16} className="ml-2 group-hover:translate-x-1 transition-transform" />
                </Button>
            </div>
        </section>
    );
};
