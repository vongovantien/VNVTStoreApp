import { useTranslation } from 'react-i18next';
import { Printer, Check, Truck, Package, X } from 'lucide-react';
import { Modal, Button, Badge } from '@/components/ui';
import { OrderDto } from '@/services/orderService';
import { OrderStatus } from '@/constants';
import { formatCurrency, formatDate, getStatusColor, getStatusText } from '@/utils/format';
import defaultImage from '@/assets/default-image.png';

interface OrderDetailModalProps {
    order: OrderDto | null;
    onClose: () => void;
    onUpdateStatus: (code: string, status: string) => void;
    onPrint: () => void;
}

export const OrderDetailModal = ({ order, onClose, onUpdateStatus, onPrint }: OrderDetailModalProps) => {
    const { t } = useTranslation();

    if (!order) return null;

    return (
        <Modal
            isOpen={!!order}
            onClose={onClose}
            title={`${t('admin.actions.view')} ${t('common.fields.orderCode')}: ${order.code}`}
            size="lg"
            footer={
                <div className="flex justify-end gap-3">
                    <Button variant="ghost" onClick={onClose}>{t('common.close')}</Button>
                    
                    {order.status === OrderStatus.PENDING && (
                        <div className="flex gap-2">
                             <Button variant="danger" leftIcon={<X size={16} />} onClick={() => onUpdateStatus(order.code, OrderStatus.CANCELLED)}>
                                {t('admin.actions.cancelOrder')}
                            </Button>
                            <Button variant="primary" leftIcon={<Check size={16} />} onClick={() => onUpdateStatus(order.code, OrderStatus.CONFIRMED)}>
                                {t('admin.actions.confirmOrder')}
                            </Button>
                        </div>
                    )}
                    
                    {order.status === OrderStatus.CONFIRMED && (
                        <Button variant="primary" leftIcon={<Truck size={16} />} onClick={() => onUpdateStatus(order.code, OrderStatus.SHIPPING)}>
                            {t('admin.actions.shipOrder')}
                        </Button>
                    )}

                    {order.status === OrderStatus.SHIPPING && (
                        <Button variant="success" leftIcon={<Package size={16} />} onClick={() => onUpdateStatus(order.code, OrderStatus.DELIVERED)}>
                            {t('admin.actions.markDelivered')}
                        </Button>
                    )}

                    {order.status === OrderStatus.DELIVERED && (
                        <Button variant="outline" leftIcon={<Printer size={16} />} onClick={onPrint}>
                            {t('admin.actions.printInvoice')}
                        </Button>
                    )}
                </div>
            }
        >
            <div className="space-y-6">
                {/* Status Bar */}
                <div className="flex items-center gap-4 border-b pb-4">
                    <Badge color={getStatusColor(order.status) as 'default' | 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info'}>
                        {t(getStatusText(order.status))}
                    </Badge>
                    <Badge color={order.paymentStatus === 'Completed' ? 'success' : 'warning'}>
                        {order.paymentStatus === 'Completed' ? t('admin.payment.paid') : t('admin.payment.pending')}
                    </Badge>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {/* Customer Info */}
                    <div className="bg-slate-50 dark:bg-slate-800 rounded-lg p-4 border border-border-color">
                        <h3 className="font-semibold mb-2 flex items-center gap-2">
                            {t('admin.order.customerInfo')}
                        </h3>
                        <p className="font-medium">{order.shippingName || order.userCode}</p>
                        <p className="text-secondary text-sm">{order.shippingPhone || '-'}</p>
                        <p className="text-secondary text-sm mt-1">{order.shippingAddress || '-'}</p>
                    </div>

                    {/* Order Info */}
                    <div className="bg-slate-50 dark:bg-slate-800 rounded-lg p-4 border border-border-color">
                        <h3 className="font-semibold mb-2">{t('admin.order.orderInfo')}</h3>
                        <p className="text-secondary text-sm grid grid-cols-2 gap-y-1">
                            <span>{t('admin.order.orderDate')}:</span>
                            <span className="font-medium text-foreground">{formatDate(order.orderDate)}</span>
                            
                            <span>{t('common.fields.paymentMethod')}:</span>
                            <span className="font-medium text-foreground">
                                {order.paymentMethod === 'cod' ? t('admin.order.paymentMethodCod') : t('admin.order.paymentMethodTransfer')}
                            </span>
                        </p>
                        {order.note && (
                            <div className="mt-3 p-2 bg-yellow-50 dark:bg-yellow-900/10 text-yellow-700 dark:text-yellow-400 text-sm rounded border border-yellow-200 dark:border-yellow-900/30">
                                <span className="font-semibold">{t('common.fields.note')}:</span> {order.note}
                            </div>
                        )}
                    </div>
                </div>

                {/* Items */}
                <div>
                    <h3 className="font-semibold mb-3">{t('common.products')}</h3>
                    <div className="space-y-3 border rounded-lg p-3 bg-white dark:bg-slate-900">
                        {order.orderItems?.map((item, index) => (
                            <div key={index} className="flex items-center gap-3 py-2 border-b last:border-0 border-dashed border-slate-200 dark:border-slate-700">
                                <img 
                                    src={item.productImage || defaultImage} 
                                    alt={item.productName} 
                                    className="w-12 h-12 object-cover rounded border border-border-color bg-slate-100"
                                    onError={(e) => { (e.target as HTMLImageElement).src = defaultImage; }}
                                />
                                <div className="flex-1">
                                    <p className="font-medium text-sm line-clamp-2">{item.productName}</p>
                                    <div className="flex items-center gap-3 text-xs text-tertiary mt-1">
                                         <span>x{item.quantity}</span>
                                         {item.color && <span>{item.color}</span>}
                                         {item.size && <span>{item.size}</span>}
                                    </div>
                                </div>
                                <div className="text-right">
                                    <p className="font-medium">{formatCurrency(item.priceAtOrder * item.quantity)}</p>
                                    <p className="text-xs text-tertiary">{formatCurrency(item.priceAtOrder)} / {t('common.unit')}</p>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>

                {/* Summary */}
                <div className="border-t border-border-color pt-4 space-y-2">
                    <div className="flex justify-between text-sm">
                        <span className="text-secondary">{t('admin.order.subtotal')}</span>
                        <span>{formatCurrency(order.totalAmount)}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                        <span className="text-secondary">{t('admin.order.shippingFee')}</span>
                        <span>{formatCurrency(order.shippingFee)}</span>
                    </div>
                    {order.discountAmount > 0 && (
                        <div className="flex justify-between text-sm text-success">
                            <span>{t('admin.order.discount')}</span>
                            <span>-{formatCurrency(order.discountAmount)}</span>
                        </div>
                    )}
                    <div className="flex justify-between font-bold text-lg pt-2 border-t border-border-color mt-2">
                        <span>{t('admin.order.grandTotal')}</span>
                        <span className="text-error">{formatCurrency(order.finalAmount)}</span>
                    </div>
                </div>
            </div>
        </Modal>
    );
};
