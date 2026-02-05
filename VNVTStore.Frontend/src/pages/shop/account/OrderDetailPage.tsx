import { memo, useMemo, useCallback } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { 
    ChevronLeft, Package, MapPin, Calendar, CreditCard, 
    Truck, AlertCircle, CheckCircle, Clock 
} from 'lucide-react';
import { Button, Badge } from '@/components/ui';
import SharedImage from '@/components/common/Image';
import { useOrder } from '@/hooks';
import { formatDate, formatCurrency, getStatusColor, getStatusText } from '@/utils/format';
import { OrderItemDto } from '@/services/orderService';

// ============ ORDER ITEM COMPONENT (MEMOIZED) ============
// Extracted to prevent re-renders of list items when parent state changes unrelated to items
const OrderItem = memo(({ item, onBuyAgain }: { item: OrderItemDto; onBuyAgain: (item: OrderItemDto) => void }) => {
    const { t } = useTranslation();
    
    // Callback wrapper to ensure stability if passed down further
    const handleBuyAgain = useCallback(() => {
        onBuyAgain(item);
    }, [item, onBuyAgain]);

    return (
        <div className="flex flex-col sm:flex-row items-start sm:items-center gap-4 py-4 border-b last:border-0 border-secondary/10">
            <Link to={`/products/${item.productCode}`} className="flex-shrink-0">
                <SharedImage 
                    src={item.productImage} 
                    alt={item.productName} 
                    className="w-20 h-20 object-cover rounded-lg border border-secondary/20"
                />
            </Link>
            
            <div className="flex-1 min-w-0 pointer-events-none sm:pointer-events-auto">
                <Link to={`/products/${item.productCode}`} className="font-medium text-primary hover:text-indigo-600 truncate block">
                    {item.productName}
                </Link>
                <div className="text-sm text-tertiary mt-1 space-y-1">
                    <p>{t('cart.quantity')}: <span className="text-primary font-medium">{item.quantity}</span></p>
                    {item.size && <p>{t('product.size')}: {item.size}</p>}
                    {item.color && <p>{t('product.color')}: {item.color}</p>}
                </div>
            </div>

            <div className="flex flex-col items-end gap-2 flex-shrink-0">
                <span className="font-bold text-primary">{formatCurrency(item.priceAtOrder)}</span>
                <Button 
                    variant="outline" 
                    size="sm" 
                    onClick={handleBuyAgain}
                    className="text-xs"
                >
                    {t('order.buyAgain') || 'Buy Again'}
                </Button>
            </div>
        </div>
    );
});

OrderItem.displayName = 'OrderItem';

// ============ MAIN PAGE COMPONENT ============
const OrderDetailPage = () => {
    const { t } = useTranslation();
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    
    // Fetch Order Data
    const { data: order, isLoading, isError, error } = useOrder(id || '');

    // Handle Buy Again Action (Memoized Callback)
    const handleBuyAgain = useCallback((item: OrderItemDto) => {
        // Navigate to product page for now, could act as "Add to Cart" directly
        navigate(`/products/${item.productCode}`);
        // In a real scenario: addToCart(item.productCode, 1, ...)
    }, [navigate]);

    // Compute Status Icon (Memoized Value)
    const StatusIcon = useMemo(() => {
        if (!order) return Clock;
        switch (order.status) {
            case 'Delivered': return CheckCircle;
            case 'Cancelled': return AlertCircle;
            case 'Processing': return Package;
            case 'Shipping': return Truck;
            default: return Clock;
        }
    }, [order]);

    if (isLoading) {
        return (
            <div className="flex items-center justify-center min-h-[60vh]">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
            </div>
        );
    }

    if (isError || !order) {
        return (
            <div className="container mx-auto px-4 py-8 text-center">
                <AlertCircle className="mx-auto h-12 w-12 text-error mb-4" />
                <h2 className="text-xl font-bold mb-2">{t('common.error')}</h2>
                <p className="text-secondary mb-6">{error instanceof Error ? error.message : 'Order not found'}</p>
                <Button onClick={() => navigate('/account/orders')} leftIcon={<ChevronLeft size={16}/>}>
                    {t('order.backToOrders') || 'Back to Orders'}
                </Button>
            </div>
        );
    }

    return (
        <div className="container mx-auto px-4 py-8 max-w-5xl">
            {/* Header / Back Link */}
            <div className="mb-6">
                <Link to="/account?tab=orders" className="inline-flex items-center text-secondary hover:text-primary transition-colors mb-4">
                    <ChevronLeft size={16} className="mr-1" />
                    {t('order.backToOrders') || 'Back to Orders'}
                </Link>
                
                <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                    <div>
                        <h1 className="text-2xl font-bold flex items-center gap-3">
                            {t('order.title')} #{order.code}
                            {/* eslint-disable-next-line @typescript-eslint/no-explicit-any */}
                            <Badge color={getStatusColor(order.status.toLowerCase()) as any} className="text-base px-3 py-1">
                                {t(getStatusText(order.status.toLowerCase()))}
                            </Badge>
                        </h1>
                        <p className="text-secondary mt-1 flex items-center gap-2">
                            <Calendar size={14} />
                            {formatDate(order.orderDate)}
                        </p>
                    </div>
                    {order.status === 'Delivered' && (
                        <div className="flex gap-2">
                             <Button variant="outline" className="border-error text-error hover:bg-error/5" onClick={() => navigate('/support')}>
                                {t('order.returnRefund') || 'Yêu cầu Trả hàng/Hoàn tiền'}
                            </Button>
                        </div>
                    )}
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                {/* Left Column: Order Items */}
                <div className="lg:col-span-2 space-y-6">
                    <div className="bg-primary rounded-xl shadow-sm border border-secondary/20 overflow-hidden">
                        <div className="p-4 border-b border-secondary/10 bg-secondary/5">
                            <h3 className="font-semibold flex items-center gap-2">
                                <Package size={18} />
                                {t('order.items')} ({order.orderItems.length})
                            </h3>
                        </div>
                        <div className="p-4">
                            {order.orderItems.map((item) => (
                                <OrderItem 
                                    key={item.id || item.code} // Fallback to code if id missing
                                    item={item} 
                                    onBuyAgain={handleBuyAgain} 
                                />
                            ))}
                        </div>
                    </div>

                    {/* Order Timeline / Status Steps (Simplified) */}
                    <div className="bg-primary rounded-xl shadow-sm border border-secondary/20 p-6">
                        <h3 className="font-semibold mb-4 flex items-center gap-2">
                            <Clock size={18} />
                            {t('order.timeline') || 'Order Status'}
                        </h3>
                        <div className="flex items-center gap-4 text-sm text-secondary">
                             <StatusIcon className={`text-${getStatusColor(order.status.toLowerCase())}`} size={24} />
                             <span className="font-medium text-primary">{t(getStatusText(order.status.toLowerCase()))}</span>
                             <span className="text-tertiary">- {formatDate(order.orderDate)}</span>
                        </div>
                    </div>
                </div>

                {/* Right Column: Summary & Info */}
                <div className="space-y-6">
                    {/* Shipping Address */}
                    <div className="bg-primary rounded-xl shadow-sm border border-secondary/20 p-6">
                        <h3 className="font-semibold mb-4 flex items-center gap-2">
                            <MapPin size={18} />
                            {t('checkout.shippingAddress')}
                        </h3>
                        <div className="text-sm space-y-2 text-secondary">
                            <p className="font-medium text-primary">{order.customerName}</p>
                            <p>{order.shippingAddress || 'N/A'}</p>
                            <p>{order.customerPhone}</p>
                        </div>
                    </div>

                    {/* Payment Info */}
                    <div className="bg-primary rounded-xl shadow-sm border border-secondary/20 p-6">
                        <h3 className="font-semibold mb-4 flex items-center gap-2">
                            <CreditCard size={18} />
                            {t('checkout.paymentMethod')}
                        </h3>
                        <p className="text-sm text-secondary">
                            {order.paymentMethod ? t(`payment.${order.paymentMethod}`) : 'N/A'}
                        </p>
                    </div>

                    {/* Order Summary */}
                    <div className="bg-primary rounded-xl shadow-sm border border-secondary/20 p-6">
                        <h3 className="font-semibold mb-4">{t('order.summary')}</h3>
                        <div className="space-y-3 text-sm pb-4 border-b border-secondary/10">
                            <div className="flex justify-between">
                                <span className="text-secondary">{t('cart.subtotal')}</span>
                                <span className="font-medium">{formatCurrency(order.totalAmount)}</span> 
                                {/* Note: Backend might need to send subtotal separately if tax/shipping logic complex */}
                            </div>
                            <div className="flex justify-between">
                                <span className="text-secondary">{t('cart.shipping')}</span>
                                <span className="font-medium">{formatCurrency(0)}</span>
                            </div>
                            {/* Tax, Discount placeholders */}
                        </div>
                        <div className="flex justify-between pt-4 font-bold text-lg">
                            <span>{t('cart.total')}</span>
                            <span className="text-error">{formatCurrency(order.totalAmount)}</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default memo(OrderDetailPage);
