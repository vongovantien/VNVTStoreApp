import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { Truck, CheckCircle, Package, AlertCircle, Clock } from 'lucide-react';
import { deliveryService } from '@/services/deliveryService';
import { DeliveryStatus } from '@/types';
import { formatDate } from '@/utils/format';
import { useTranslation } from 'react-i18next';

interface OrderDeliveryTimelineProps {
  orderCode: string;
}

export const OrderDeliveryTimeline: React.FC<OrderDeliveryTimelineProps> = ({ orderCode }) => {
  const { t } = useTranslation();

  const { data: deliveryRes, isLoading } = useQuery({
    queryKey: ['delivery', orderCode],
    queryFn: () => deliveryService.getByOrderCode(orderCode),
    retry: false,
  });

  if (isLoading) return <div className="p-4 text-center text-sm text-tertiary">Loading delivery info...</div>;

  const delivery = deliveryRes?.data;
  if (!delivery) return null;

  const getIcon = (status: DeliveryStatus) => {
    switch (status) {
      case DeliveryStatus.Delivered: return <CheckCircle size={16} className="text-success" />;
      case DeliveryStatus.InTransit: return <Truck size={16} className="text-primary" />;
      case DeliveryStatus.PickedUp: return <Package size={16} className="text-info" />;
      case DeliveryStatus.Failed:
      case DeliveryStatus.Returned: return <AlertCircle size={16} className="text-error" />;
      default: return <Clock size={16} className="text-warning" />;
    }
  };

  return (
    <div className="bg-primary rounded-xl shadow-sm border border-secondary/20 p-6 mt-6">
      <h3 className="font-semibold mb-4 flex items-center gap-2">
        <Truck size={18} />
        {t('order.deliveryTimeline', 'Delivery Timeline')}
      </h3>
      
      <div className="text-sm text-secondary mb-6 grid grid-cols-2 gap-4">
        <div>
          <p className="text-xs text-tertiary mb-1">Carrier</p>
          <p className="font-medium text-primary">{delivery.carrierName || 'N/A'}</p>
        </div>
        <div>
          <p className="text-xs text-tertiary mb-1">Tracking Number</p>
          <p className="font-medium text-primary">{delivery.trackingNumber || 'N/A'}</p>
        </div>
        <div>
          <p className="text-xs text-tertiary mb-1">Shipper</p>
          <p className="font-medium text-primary">{delivery.shipperName || 'N/A'} - {delivery.shipperPhone}</p>
        </div>
        <div>
          <p className="text-xs text-tertiary mb-1">Status</p>
          <p className="font-medium text-primary">{delivery.statusText}</p>
        </div>
      </div>

      <div className="relative border-l border-secondary/20 ml-3 space-y-6">
        {delivery.histories?.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()).map((history, idx) => (
          <div key={idx} className="relative pl-6">
            <span className="absolute -left-[9px] top-1 bg-primary rounded-full">
              {getIcon(history.status)}
            </span>
            <div className="flex flex-col">
              <span className="font-medium text-primary text-sm">{history.statusText}</span>
              <span className="text-xs text-tertiary">{formatDate(history.timestamp)}</span>
              {history.note && <p className="text-sm text-secondary mt-1 bg-secondary/10 p-2 rounded">{history.note}</p>}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
