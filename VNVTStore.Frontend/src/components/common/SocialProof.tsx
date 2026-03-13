import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Eye, TrendingUp, ShoppingBag } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';

interface SocialProofProps {
    productId: string;
    initialViewers?: number | undefined;
    initialSold?: number | undefined;
    className?: string;
}

export const SocialProof: React.FC<SocialProofProps> = ({ 
    productId, 
    initialViewers = 0, 
    initialSold = 0, 
    className 
}) => {
    const { t } = useTranslation();
    const [viewers, setViewers] = useState(initialViewers || 0);
    const [sold24h, setSold24h] = useState(initialSold || 0);
    const [activeMsg, setActiveMsg] = useState(0);

    useEffect(() => {
        // If initial values are 0, use a fallback based on hash to avoid empty look, 
        // but real data takes precedence.
        const hash = productId.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
        const baseViewers = initialViewers > 0 ? initialViewers : 10 + (hash % 40);
        const baseSold = initialSold > 0 ? initialSold : 2 + (hash % 15);

        queueMicrotask(() => {
            setViewers(baseViewers);
            setSold24h(baseSold);
        });

        // Fluctuate viewers
        const interval = setInterval(() => {
            setViewers(prev => {
                const change = Math.floor(Math.random() * 5) - 2; // -2 to +2
                return Math.max(5, prev + change);
            });
        }, 5000);

        // Rotate messages
        const msgInterval = setInterval(() => {
            setActiveMsg(prev => (prev + 1) % 3);
        }, 8000);

        return () => {
            clearInterval(interval);
            clearInterval(msgInterval);
        };
    }, [productId, initialViewers, initialSold]);

    const messages = [
        {
            icon: Eye,
            text: t('product.social.viewing', '{{count}} người đang xem sản phẩm này', { count: viewers }),
            color: 'text-indigo-600 dark:text-indigo-400',
            bg: 'bg-indigo-50 dark:bg-indigo-900/20'
        },
        {
            icon: ShoppingBag,
            text: t('product.social.sold24h', 'Đã bán {{count}} cái trong 24h qua', { count: sold24h }),
            color: 'text-emerald-600 dark:text-emerald-400',
            bg: 'bg-emerald-50 dark:bg-emerald-900/20'
        },
        {
            icon: TrendingUp,
            text: t('product.social.trending', 'Sản phẩm đang bán chạy!'),
            color: 'text-rose-600 dark:text-rose-400',
            bg: 'bg-rose-50 dark:bg-rose-900/20'
        }
    ];

    const current = messages[activeMsg];

    return (
        <div className={`h-8 overflow-hidden ${className}`}>
            <AnimatePresence mode="wait">
                <motion.div
                    key={activeMsg}
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    exit={{ opacity: 0, y: -20 }}
                    className={`inline-flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-bold ${current.bg} ${current.color}`}
                >
                    <current.icon size={14} className="animate-pulse" />
                    <span>{current.text}</span>
                </motion.div>
            </AnimatePresence>
        </div>
    );
};
