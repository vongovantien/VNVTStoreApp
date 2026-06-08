import { useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { Search, Command, ShoppingCart, Package, Users } from 'lucide-react';

interface AdminSearchModalProps {
    isOpen: boolean;
    onClose: () => void;
    searchQuery: string;
    onSearchChange: (query: string) => void;
}

export const AdminSearchModal = ({
    isOpen,
    onClose,
    searchQuery,
    onSearchChange
}: AdminSearchModalProps) => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const searchInputRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        if (isOpen && searchInputRef.current) {
            setTimeout(() => searchInputRef.current?.focus(), 100);
        }
    }, [isOpen]);

    const handleQuickAccess = (path: string) => {
        onClose();
        navigate(path);
    };

    return (
        <AnimatePresence>
            {isOpen && (
                <>
                    <motion.div
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        className="fixed inset-0 bg-black/50 z-[999]"
                        onClick={onClose}
                    />
                    <motion.div
                        initial={{ opacity: 0, scale: 0.95, y: -20 }}
                        animate={{ opacity: 1, scale: 1, y: 0 }}
                        exit={{ opacity: 0, scale: 0.95, y: -20 }}
                        className="fixed top-[15%] left-1/2 -translate-x-1/2 w-full max-w-lg bg-primary rounded-2xl shadow-2xl border border-border z-[1000] overflow-hidden"
                    >
                        <div className="bg-gradient-to-r from-indigo-500 to-purple-500 p-4">
                            <div className="flex items-center gap-3 bg-white/10 backdrop-blur rounded-xl px-4 py-3">
                                <Search size={20} className="text-white/80" />
                                <input
                                    ref={searchInputRef}
                                    type="text"
                                    value={searchQuery}
                                    onChange={(e) => onSearchChange(e.target.value)}
                                    placeholder={t('admin.searchPlaceholder')}
                                    className="flex-1 bg-transparent outline-none text-white placeholder:text-white/60 text-sm font-medium"
                                    onKeyDown={(e) => {
                                        if (e.key === 'Enter' && searchQuery.trim()) {
                                            onClose();
                                            navigate(`/admin/products?search=${encodeURIComponent(searchQuery)}`);
                                        }
                                    }}
                                />
                                <kbd className="px-2 py-1 text-[10px] font-bold text-white/80 bg-white/20 rounded-lg">ESC</kbd>
                            </div>
                        </div>
                        
                        <div className="p-4">
                            <p className="mb-3 text-xs font-semibold text-tertiary uppercase tracking-widest">
                                ⚡ {t('common.quickAccess')}
                            </p>
                            <div className="grid grid-cols-3 gap-2">
                                <button onClick={() => handleQuickAccess('/admin/orders')} className="flex flex-col items-center gap-2 p-4 rounded-xl hover:bg-secondary transition-colors group">
                                    <div className="w-10 h-10 rounded-full bg-orange-500 flex items-center justify-center text-white"><ShoppingCart size={18} /></div>
                                    <span className="text-xs font-medium">{t('admin.sidebar.orders')}</span>
                                </button>
                                <button onClick={() => handleQuickAccess('/admin/products')} className="flex flex-col items-center gap-2 p-4 rounded-xl hover:bg-secondary transition-colors group">
                                    <div className="w-10 h-10 rounded-full bg-blue-500 flex items-center justify-center text-white"><Package size={18} /></div>
                                    <span className="text-xs font-medium">{t('admin.sidebar.products')}</span>
                                </button>
                                <button onClick={() => handleQuickAccess('/admin/customers')} className="flex flex-col items-center gap-2 p-4 rounded-xl hover:bg-secondary transition-colors group">
                                    <div className="w-10 h-10 rounded-full bg-emerald-500 flex items-center justify-center text-white"><Users size={18} /></div>
                                    <span className="text-xs font-medium">{t('admin.sidebar.customers')}</span>
                                </button>
                            </div>
                        </div>
                    </motion.div>
                </>
            )}
        </AnimatePresence>
    );
};
