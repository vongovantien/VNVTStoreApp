import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { Truck, MapPin, Calendar, Clock, CheckCircle, Package } from 'lucide-react';
import { Button, Input, Select, Badge } from '@/components/ui';
import { deliveryService } from '@/services/deliveryService';
import { DeliveryStatus } from '@/types';
import { formatDate } from '@/utils/format';
import { useToast } from '@/store';

interface OrderDeliveryPanelProps {
  orderCode: string;
  orderStatus: string;
}

export const OrderDeliveryPanel: React.FC<OrderDeliveryPanelProps> = ({ orderCode, orderStatus }) => {
  const { t } = useTranslation();
  const toast = useToast();
  const queryClient = useQueryClient();
  const [isAssigning, setIsAssigning] = useState(false);

  // Form state
  const [shipperName, setShipperName] = useState('');
  const [shipperPhone, setShipperPhone] = useState('');
  const [carrierName, setCarrierName] = useState('');
  const [trackingNumber, setTrackingNumber] = useState('');
  const [note, setNote] = useState('');

  const { data: deliveryRes, isLoading } = useQuery({
    queryKey: ['delivery', orderCode],
    queryFn: () => deliveryService.getByOrderCode(orderCode),
    retry: false, // It will fail 404 if not assigned yet
  });

  const delivery = deliveryRes?.data;

  const assignMutation = useMutation({
    mutationFn: (payload: any) => deliveryService.assignShipper(orderCode, payload),
    onSuccess: () => {
      toast.success(t('messages.assignSuccess', 'Assigned shipper successfully'));
      queryClient.invalidateQueries({ queryKey: ['delivery', orderCode] });
      setIsAssigning(false);
    },
    onError: (err: any) => {
      toast.error(err.message || t('messages.error'));
    }
  });

  const updateStatusMutation = useMutation({
    mutationFn: (payload: { code: string; status: DeliveryStatus }) => 
      deliveryService.updateStatus(payload.code, { status: payload.status, note: 'Updated by Admin' }),
    onSuccess: () => {
      toast.success(t('messages.updateSuccess', 'Updated delivery status'));
      queryClient.invalidateQueries({ queryKey: ['delivery', orderCode] });
    }
  });

  if (isLoading) return <div className="p-4 text-center">Loading delivery info...</div>;

  if (!delivery) {
    if (orderStatus !== 'Shipping' && orderStatus !== 'Confirmed') {
      return (
        <div className="bg-secondary rounded-lg p-4 text-center text-tertiary">
          Delivery is not available for this order status.
        </div>
      );
    }

    if (!isAssigning) {
      return (
        <div className="bg-secondary rounded-lg p-4 flex flex-col items-center justify-center gap-3">
          <Truck className="text-tertiary" size={32} />
          <p className="text-sm font-medium">No shipper assigned yet</p>
          <Button size="sm" onClick={() => setIsAssigning(true)}>Assign Shipper</Button>
        </div>
      );
    }

    return (
      <div className="bg-secondary rounded-lg p-4 space-y-4 border border-border">
        <h3 className="font-semibold text-sm border-b pb-2">Assign Shipper</h3>
        <div className="grid grid-cols-2 gap-3">
          <Input label="Carrier Name" value={carrierName} onChange={(e) => setCarrierName(e.target.value)} placeholder="e.g. GHTK, VNPost" />
          <Input label="Tracking Number" value={trackingNumber} onChange={(e) => setTrackingNumber(e.target.value)} />
          <Input label="Shipper Name" value={shipperName} onChange={(e) => setShipperName(e.target.value)} />
          <Input label="Shipper Phone" value={shipperPhone} onChange={(e) => setShipperPhone(e.target.value)} />
        </div>
        <Input label="Note" value={note} onChange={(e) => setNote(e.target.value)} />
        <div className="flex justify-end gap-2 pt-2">
          <Button variant="ghost" size="sm" onClick={() => setIsAssigning(false)}>Cancel</Button>
          <Button size="sm" isLoading={assignMutation.isPending} onClick={() => assignMutation.mutate({ shipperName, shipperPhone, carrierName, trackingNumber, note })}>
            Confirm Assignment
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-secondary rounded-lg p-4 space-y-4">
      <div className="flex justify-between items-center border-b pb-3">
        <h3 className="font-semibold flex items-center gap-2">
          <Truck size={18} className="text-primary" />
          Delivery Information
        </h3>
        <Badge color={delivery.status === DeliveryStatus.Delivered ? 'success' : 'warning'}>
          {delivery.statusText}
        </Badge>
      </div>

      <div className="grid grid-cols-2 gap-4 text-sm">
        <div>
          <p className="text-tertiary text-xs mb-1">Carrier / Tracking</p>
          <p className="font-medium">{delivery.carrierName || 'N/A'}</p>
          <p className="text-xs text-primary mt-0.5">{delivery.trackingNumber}</p>
        </div>
        <div>
          <p className="text-tertiary text-xs mb-1">Shipper</p>
          <p className="font-medium">{delivery.shipperName || 'N/A'}</p>
          <p className="text-xs">{delivery.shipperPhone}</p>
        </div>
      </div>

      {delivery.status !== DeliveryStatus.Delivered && delivery.status !== DeliveryStatus.Returned && delivery.status !== DeliveryStatus.Failed && (
        <div className="pt-3 border-t flex flex-wrap gap-2">
          <span className="text-xs text-tertiary w-full mb-1">Quick Actions:</span>
          {delivery.status === DeliveryStatus.Assigned && (
             <Button size="xs" variant="outline" onClick={() => updateStatusMutation.mutate({ code: delivery.code, status: DeliveryStatus.PickedUp })}>Mark Picked Up</Button>
          )}
          {delivery.status === DeliveryStatus.PickedUp && (
             <Button size="xs" variant="outline" onClick={() => updateStatusMutation.mutate({ code: delivery.code, status: DeliveryStatus.InTransit })}>Mark In Transit</Button>
          )}
          {delivery.status === DeliveryStatus.InTransit && (
             <Button size="xs" variant="success" onClick={() => updateStatusMutation.mutate({ code: delivery.code, status: DeliveryStatus.Delivered })}>Mark Delivered</Button>
          )}
          <Button size="xs" variant="danger" onClick={() => updateStatusMutation.mutate({ code: delivery.code, status: DeliveryStatus.Failed })}>Report Failure</Button>
        </div>
      )}
    </div>
  );
};
