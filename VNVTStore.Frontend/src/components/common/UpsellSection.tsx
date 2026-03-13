import React, { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Sparkles, Plus } from 'lucide-react';
import { Button, Checkbox } from '@/components/ui';
import { useProducts } from '@/hooks';
import type { Product } from '@/types';
import { formatCurrency } from '@/utils/format';
import { useCartStore, useToast } from '@/store'; // Import store and toast
import { Link } from 'react-router-dom';

interface UpsellSectionProps {
    currentProduct: Product;
}

export const UpsellSection: React.FC<UpsellSectionProps> = ({ currentProduct }) => {
    const { t } = useTranslation();
    const { addItem } = useCartStore();
    const { success } = useToast();
    const [selectedProducts, setSelectedProducts] = useState<string[]>([]);
    
    // Fetch complementary products (same category, different brand or accessories)
    const { data: upsellData, isLoading } = useProducts({
        pageSize: 2, // Get 2 recommendations
        category: currentProduct.categoryCode || undefined, // Same category for now
        sortField: 'reviewCount', // Popular items
        sortDir: 'desc'
    });

    const recommendations = useMemo(() => {
        return (upsellData?.products || []).filter(p => p.code !== currentProduct.code).slice(0, 2);
    }, [upsellData?.products, currentProduct.code]);

    // Initialize selected products when data loads
    React.useEffect(() => {
        if (recommendations.length > 0) {
            setSelectedProducts(recommendations.map(p => p.code));
        }
    }, [recommendations]);

    const toggleProduct = (code: string) => {
        setSelectedProducts(prev => 
            prev.includes(code) ? prev.filter(c => c !== code) : [...prev, code]
        );
    };

    const bundleTotal = useMemo(() => {
        let total = currentProduct.price;
        recommendations.forEach(p => {
            if (selectedProducts.includes(p.code)) {
                total += p.price;
            }
        });
        return total;
    }, [currentProduct, recommendations, selectedProducts]);

    const handleAddBundle = async () => {
        // Add current product
        await addItem(currentProduct);
        
        // Add selected recommendations
        const productsToAdd = recommendations.filter(p => selectedProducts.includes(p.code));
        for (const p of productsToAdd) {
            await addItem(p);
        }
        
        success(t('cart.bundleAdded', 'Đã thêm bộ sản phẩm vào giỏ hàng'));
    };

    if (!isLoading && recommendations.length === 0) return null;

    return (
        <section className="py-8 bg-slate-50 dark:bg-slate-900/50 rounded-3xl my-12 border border-slate-100 dark:border-slate-800">
            <div className="px-6 md:px-8">
                <div className="flex items-center gap-2 mb-6">
                    <Sparkles size={20} className="text-indigo-600 dark:text-indigo-400 fill-indigo-100 dark:fill-indigo-900" />
                    <h2 className="text-xl md:text-2xl font-black text-slate-900 dark:text-white">
                        {t('product.frequentlyBoughtTogether', 'Thường được mua cùng')}
                    </h2>
                </div>

                <div className="flex flex-col xl:flex-row gap-8 items-start">
                    {/* Products List */}
                    <div className="flex-1 w-full overflow-x-auto pb-4 custom-scrollbar">
                        <div className="flex items-center gap-4 min-w-max">
                            {/* Current Product */}
                            <div className="w-48 group">
                                <div className="aspect-[3/4] rounded-xl overflow-hidden border border-indigo-200 dark:border-indigo-800 relative mb-3">
                                    <img 
                                        src={currentProduct.image || '/placeholder-product.jpg'} 
                                        alt={currentProduct.name} 
                                        className="w-full h-full object-cover" 
                                        onError={(e) => {
                                            (e.target as HTMLImageElement).src = '/placeholder-product.jpg';
                                        }}
                                    />
                                    <div className="absolute top-2 right-2 bg-indigo-600 text-white text-[10px] font-bold px-2 py-0.5 rounded shadow-sm">
                                        This Item
                                    </div>
                                </div>
                                <h3 className="font-bold text-sm text-slate-900 dark:text-white line-clamp-2 mb-1" title={currentProduct.name}>{currentProduct.name}</h3>
                                <div className="text-rose-600 font-bold">{formatCurrency(currentProduct.price)}</div>
                            </div>

                            {/* Plus Icon */}
                            <Plus size={24} className="text-slate-300 flex-shrink-0" />

                            {/* Recommendations */}
                            {isLoading ? (
                                Array.from({ length: 2 }).map((_, i) => (
                                    <React.Fragment key={i}>
                                        <div className="w-48 aspect-[3/4] bg-slate-200 dark:bg-slate-700 animate-pulse rounded-xl" />
                                        {i < 1 && <Plus size={24} className="text-slate-300 flex-shrink-0" />}
                                    </React.Fragment>
                                ))
                            ) : (
                                recommendations.map((product, index) => (
                                    <React.Fragment key={product.code}>
                                        <div className="w-48 relative">
                                            <div className="aspect-[3/4] rounded-xl overflow-hidden border border-slate-200 dark:border-slate-700 relative mb-3 bg-white dark:bg-slate-800">
                                                <Link to={`/product/${product.code}`}>
                                                    <img 
                                                        src={product.image || '/placeholder-product.jpg'} 
                                                        alt={product.name} 
                                                        className="w-full h-full object-cover hover:scale-105 transition-transform duration-500" 
                                                        onError={(e) => {
                                                            (e.target as HTMLImageElement).src = '/placeholder-product.jpg';
                                                        }}
                                                    />
                                                </Link>
                                                <div className="absolute top-2 left-2">
                                                    <Checkbox 
                                                        checked={selectedProducts.includes(product.code)}
                                                        onChange={() => toggleProduct(product.code)}
                                                    />
                                                </div>
                                            </div>
                                            <Link to={`/product/${product.code}`} className="font-medium text-sm text-slate-700 dark:text-slate-300 hover:text-indigo-600 line-clamp-2 mb-1">
                                                {product.name}
                                            </Link>
                                            <div className="text-rose-600 font-bold">{formatCurrency(product.price)}</div>
                                        </div>
                                        {index < recommendations.length - 1 && <Plus size={24} className="text-slate-300 flex-shrink-0" />}
                                    </React.Fragment>
                                ))
                            )}
                        </div>
                    </div>

                    {/* Bundle Action */}
                    <div className="w-full xl:w-72 flex-shrink-0 bg-white dark:bg-slate-800 p-6 rounded-2xl border border-slate-100 dark:border-slate-700 shadow-sm">
                        <div className="text-sm font-medium text-slate-500 mb-4">
                            Tổng tiền cho {selectedProducts.length + 1} sản phẩm:
                        </div>
                        <div className="text-3xl font-black text-slate-900 dark:text-white mb-6">
                            {formatCurrency(bundleTotal)}
                        </div>
                        <Button 
                            fullWidth 
                            size="lg" 
                            onClick={handleAddBundle}
                            className="bg-slate-900 dark:bg-white text-white dark:text-slate-900 hover:bg-slate-800 dark:hover:bg-slate-100"
                        >
                            {t('cart.addAllToCart', 'Thêm tất cả vào giỏ')}
                        </Button>
                        <p className="text-xs text-center text-slate-500 mt-4">
                           Tiết kiệm thời gian mua sắm
                        </p>
                    </div>
                </div>
            </div>
        </section>
    );
};
