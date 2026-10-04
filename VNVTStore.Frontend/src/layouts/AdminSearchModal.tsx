import { useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { Search, ShoppingCart, Package, Users, Loader2, TrendingUp } from 'lucide-react';
import { useGlobalSearch } from '@/hooks/useGlobalSearch';

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

    const { data, isLoading } = useGlobalSearch(searchQuery, isOpen);

    useEffect(() => {
        if (isOpen && searchInputRef.current) {
            setTimeout(() => searchInputRef.current?.focus(), 100);
        }
    }, [isOpen]);

    const handleNavigate = (path: string) => {
        onClose();
        navigate(path);
    };

    const hasResults = data && (
        data.products.length > 0 || 
        data.orders.length > 0 || 
        data.customers.length > 0
    );

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
                        className="fixed top-[10%] left-1/2 -translate-x-1/2 w-full max-w-2xl bg-primary rounded-2xl shadow-2xl border border-border z-[1000] overflow-hidden max-h-[80vh] flex flex-col"
                    >
                        {/* Search Input */}
                        <div className="bg-gradient-to-r from-indigo-500 to-purple-500 p-4">
                            <div className="flex items-center gap-3 bg-white/10 backdrop-blur rounded-xl px-4 py-3">
                                {isLoading ? (
                                    <Loader2 size={20} className="text-white/80 animate-spin" />
                                ) : (
                                    <Search size={20} className="text-white/80" />
                                )}
                                <input
                                    ref={searchInputRef}
                                    type="text"
                                    value={searchQuery}
                                    onChange={(e) => onSearchChange(e.target.value)}
                                    placeholder={t('admin.searchPlaceholder')}
                                    className="flex-1 bg-transparent outline-none text-white placeholder:text-white/60 text-sm font-medium"
                                />
                                <kbd className="px-2 py-1 text-[10px] font-bold text-white/80 bg-white/20 rounded-lg">ESC</kbd>
                            </div>
                        </div>

                        {/* Results */}
                        <div className="flex-1 overflow-y-auto p-4">
                            {searchQuery.length < 2 && (
                                <div className="space-y-4">
                                    <p className="text-xs font-semibold text-tertiary uppercase tracking-widest">
                                        ⚡ {t('common.quickAccess')}
                                    </p>
                                    <div className="grid grid-cols-3 gap-2">
                                        <button onClick={() => handleNavigate('/admin/orders')} className="flex flex-col items-center gap-2 p-4 rounded-xl hover:bg-secondary transition-colors">
                                            <div className="w-10 h-10 rounded-full bg-orange-500 flex items-center justify-center text-white">
                                                <ShoppingCart size={18} />
                                            </div>
                                            <span className="text-xs font-medium">{t('admin.sidebar.orders')}</span>
                                        </button>
                                        <button onClick={() => handleNavigate('/admin/products')} className="flex flex-col items-center gap-2 p-4 rounded-xl hover:bg-secondary transition-colors">
                                            <div className="w-10 h-10 rounded-full bg-blue-500 flex items-center justify-center text-white">
                                                <Package size={18} />
                                            </div>
                                            <span className="text-xs font-medium">{t('admin.sidebar.products')}</span>
                                        </button>
                                        <button onClick={() => handleNavigate('/admin/customers')} className="flex flex-col items-center gap-2 p-4 rounded-xl hover:bg-secondary transition-colors">
                                            <div className="w-10 h-10 rounded-full bg-emerald-500 flex items-center justify-center text-white">
                                                <Users size={18} />
                                            </div>
                                            <span className="text-xs font-medium">{t('admin.sidebar.customers')}</span>
                                        </button>
                                    </div>
                                </div>
                            )}

                            {searchQuery.length >= 2 && !hasResults && !isLoading && (
                                <div className="text-center py-12">
                                    <Search size={48} className="mx-auto text-tertiary/30 mb-3" />
                                    <p className="text-sm text-tertiary">Không tìm thấy kết quả cho "{searchQuery}"</p>
                                </div>
                            )}

                            {hasResults && (
                                <div className="space-y-6">
                                    {/* Products */}
                                    {data.products.length > 0 && (
                                        <div>
                                            <h3 className="text-xs font-semibold text-tertiary uppercase tracking-widest mb-3 flex items-center gap-2">
                                                <Package size={14} />
                                                Sản phẩm ({data.products.length})
                                            </h3>
                                            <div className="space-y-2">
                                                {data.products.map((product) => (
                                                    <button
                                                        key={product.code}
                                                        onClick={() => handleNavigate(`/admin/products?search=${encodeURIComponent(product.name || product.code)}`)}
                                                        className="w-full flex items-center gap-3 p-3 rounded-xl hover:bg-secondary transition-colors text-left"
                                                    >
                                                        {product.imageUrl ? (
                                                            <img src={product.imageUrl} alt="" className="w-10 h-10 rounded-lg object-cover" />
                                                        ) : (
                                                            <div className="w-10 h-10 rounded-lg bg-secondary flex items-center justify-center">
                                                                <Package size={16} className="text-tertiary" />
                                                            </div>
                                                        )}
                                                        <div className="flex-1 min-w-0">
                                                            <p className="font-medium text-sm truncate">{product.name}</p>
                                                            <p className="text-xs text-tertiary">{product.price.toLocaleString('vi-VN')}₫ • Stock: {product.stock}</p>
                                                        </div>
                                                    </button>
                                                ))}
                                            </div>
                                        </div>
                                    )}

                                    {/* Orders */}
                                    {data.orders.length > 0 && (
                                        <div>
                                            <h3 className="text-xs font-semibold text-tertiary uppercase tracking-widest mb-3 flex items-center gap-2">
                                                <ShoppingCart size={14} />
                                                Đơn hàng ({data.orders.length})
                                            </h3>
                                            <div className="space-y-2">
                                                {data.orders.map((order) => (
                                                    <button
                                                        key={order.code}
                                                        onClick={() => handleNavigate(`/admin/orders?search=${encodeURIComponent(order.code)}`)}
                                                        className="w-full flex items-center gap-3 p-3 rounded-xl hover:bg-secondary transition-colors text-left"
                                                    >
                                                        <div className="w-10 h-10 rounded-full bg-orange-500/10 flex items-center justify-center">
                                                            <ShoppingCart size={16} className="text-orange-500" />
                                                        </div>
                                                        <div className="flex-1 min-w-0">
                                                            <p className="font-medium text-sm">{order.orderNumber}</p>
                                                            <p className="text-xs text-tertiary">{order.customerName} • {order.totalAmount.toLocaleString('vi-VN')}₫</p>
                                                        </div>
                                                        <span className={`px-2 py-1 rounded-full text-[10px] font-medium ${
                                                            order.status === 'completed' ? 'bg-green-500/10 text-green-600' :
                                                            order.status === 'pending' ? 'bg-yellow-500/10 text-yellow-600' :
                                                            'bg-blue-500/10 text-blue-600'
                                                        }`}>
                                                            {order.status}
                                                        </span>
                                                    </button>
                                                ))}
                                            </div>
                                        </div>
                                    )}

                                    {/* Customers */}
                                    {data.customers.length > 0 && (
                                        <div>
                                            <h3 className="text-xs font-semibold text-tertiary uppercase tracking-widest mb-3 flex items-center gap-2">
                                                <Users size={14} />
                                                Khách hàng ({data.customers.length})
                                            </h3>
                                            <div className="space-y-2">
                                                {data.customers.map((customer) => (
                                                    <button
                                                        key={customer.code}
                                                        onClick={() => handleNavigate(`/admin/customers?search=${encodeURIComponent(customer.email || customer.fullName || customer.code)}`)}
                                                        className="w-full flex items-center gap-3 p-3 rounded-xl hover:bg-secondary transition-colors text-left"
                                                    >
                                                        <div className="w-10 h-10 rounded-full bg-emerald-500/10 flex items-center justify-center">
                                                            <Users size={16} className="text-emerald-500" />
                                                        </div>
                                                        <div className="flex-1 min-w-0">
                                                            <p className="font-medium text-sm">{customer.fullName}</p>
                                                            <p className="text-xs text-tertiary truncate">{customer.email}</p>
                                                        </div>
                                                        {customer.totalSpent !== undefined && (
                                                            <div className="flex items-center gap-1 text-xs text-emerald-600">
                                                                <TrendingUp size={12} />
                                                                <span>{customer.totalSpent.toLocaleString('vi-VN')}₫</span>
                                                            </div>
                                                        )}
                                                    </button>
                                                ))}
                                            </div>
                                        </div>
                                    )}
                                </div>
                            )}
                        </div>

                        {/* Footer hint */}
                        {searchQuery.length >= 2 && hasResults && (
                            <div className="border-t border-border px-4 py-2 bg-secondary/30">
                                <p className="text-[10px] text-tertiary text-center">
                                    ↑↓ di chuyển • Enter chọn • ESC đóng
                                </p>
                            </div>
                        )}
                    </motion.div>
                </>
            )}
        </AnimatePresence>
    );
};
