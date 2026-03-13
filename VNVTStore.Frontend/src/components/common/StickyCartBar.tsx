import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { ShoppingCart, Phone, MessageCircle } from 'lucide-react';
import { Button } from '@/components/ui';
import { formatCurrency } from '@/utils/format';
import type { Product } from '@/types';
import { motion, AnimatePresence } from 'framer-motion';

interface StickyCartBarProps {
    product: Product;
    handleAddToCart: () => void;
    handleBuyNow: () => void;
    isAddingToCart: boolean;
    hasFixedPrice: boolean;
}

export const StickyCartBar: React.FC<StickyCartBarProps> = ({
    product,
    handleAddToCart,
    handleBuyNow,
    isAddingToCart,
    hasFixedPrice
}) => {
    const { t } = useTranslation();
    const [isVisible, setIsVisible] = useState(false);

    useEffect(() => {
        const handleScroll = () => {
            // Show bar when scrolled past 600px
            if (window.scrollY > 600) {
                setIsVisible(true);
            } else {
                setIsVisible(false);
            }
        };

        window.addEventListener('scroll', handleScroll);
        return () => window.removeEventListener('scroll', handleScroll);
    }, []);

    const handleChat = () => {
        window.open('https://zalo.me/0912345678', '_blank'); // Replace with actual Zalo/Messenger link
    };

    return (
        <AnimatePresence>
            {isVisible && (
                <motion.div
                    initial={{ y: 100, opacity: 0 }}
                    animate={{ y: 0, opacity: 1 }}
                    exit={{ y: 100, opacity: 0 }}
                    transition={{ type: 'spring', damping: 20, stiffness: 100 }}
                    className="fixed bottom-0 md:top-0 md:bottom-auto left-0 right-0 z-[100] bg-white/95 dark:bg-slate-900/95 backdrop-blur-md border-t md:border-t-0 md:border-b border-slate-200 dark:border-slate-800 shadow-[0_-4px_6px_-1px_rgba(0,0,0,0.1)] md:shadow-md py-3 pb-safe-area-bottom"
                >
                    <div className="container mx-auto px-4 flex items-center justify-between gap-4">
                        {/* Mobile: Simple info or hidden */}
                        <div className="hidden md:flex items-center gap-4 min-w-0">
                            <div className="w-12 h-12 rounded-lg bg-slate-100 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 overflow-hidden shrink-0">
                                <img src={product.image} alt={product.name} className="w-full h-full object-cover" />
                            </div>
                            <div className="min-w-0">
                                <h4 className="text-sm font-bold text-slate-800 dark:text-white truncate max-w-[200px] lg:max-w-md">
                                    {product.name}
                                </h4>
                                <div className="flex items-center gap-2">
                                    {hasFixedPrice ? (
                                        <span className="text-rose-600 dark:text-rose-500 font-black text-sm">
                                            {formatCurrency(product.price)}
                                        </span>
                                    ) : (
                                        <span className="text-indigo-600 font-bold text-xs uppercase">
                                            {t('product.contactForPrice')}
                                        </span>
                                    )}
                                </div>
                            </div>
                        </div>

                        {/* Mobile Price (Left side) */}
                         <div className="md:hidden flex flex-col">
                            {hasFixedPrice ? (
                                <span className="text-rose-600 dark:text-rose-500 font-black text-lg">
                                    {formatCurrency(product.price)}
                                </span>
                            ) : (
                                <span className="text-indigo-600 font-bold text-xs uppercase">
                                    {t('product.contact')}
                                </span>
                            )}
                             <span className="text-[10px] text-slate-500 line-through">
                                 {product.wholesalePrice ? formatCurrency(product.wholesalePrice) : ''}
                             </span>
                        </div>

                        {/* Actions */}
                        <div className="flex items-center gap-2 flex-1 md:flex-initial justify-end">
                            {/* Chat Button (Mobile only or always?) - Let's keep it for support */}
                            <Button 
                                size="sm" 
                                variant="outline" 
                                onClick={handleChat}
                                className="w-10 h-10 p-0 rounded-full border-indigo-200 text-indigo-600"
                            >
                                <MessageCircle size={20} />
                            </Button>

                            {hasFixedPrice ? (
                                <>
                                    <Button
                                        size="sm"
                                        variant="outline"
                                        onClick={handleAddToCart}
                                        disabled={isAddingToCart}
                                        isLoading={isAddingToCart}
                                        className="hidden sm:flex rounded-xl border-slate-300"
                                        leftIcon={<ShoppingCart size={16} />}
                                    >
                                        {t('product.addToCart')}
                                    </Button>
                                    <Button
                                        size="sm"
                                        onClick={handleBuyNow}
                                        className="bg-slate-900 hover:bg-black text-white rounded-xl px-4 md:px-6 flex-1 md:flex-initial shadow-lg shadow-slate-900/20"
                                    >
                                        {t('product.buyNow', 'Mua ngay')}
                                    </Button>
                                </>
                            ) : (
                                <Button 
                                    size="sm"
                                    onClick={() => window.location.href = `/quote-request/${product.code}`}
                                    className="rounded-xl px-6 flex-1 md:flex-initial"
                                    leftIcon={<Phone size={16} />}
                                >
                                    {t('product.requestQuote')}
                                </Button>
                            )}
                        </div>
                    </div>
                </motion.div>
            )}
        </AnimatePresence>
    );
};
