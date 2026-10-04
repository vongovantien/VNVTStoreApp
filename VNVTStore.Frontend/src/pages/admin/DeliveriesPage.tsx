import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { motion, AnimatePresence } from 'framer-motion';
import {
  Truck,
  Package,
  MapPin,
  Clock,
  CheckCircle,
  XCircle,
  Loader2,
  Phone,
  Search,
  RefreshCw,
  UserCheck,
  Calendar,
  AlertTriangle,
  History,
  Send,
  ExternalLink,
  ChevronRight
} from 'lucide-react';
import { AdminPageHeader } from '@/components/admin';
import { Badge, Button, Modal, Input, Select } from '@/components/ui';
import { formatCurrency, formatDate } from '@/utils/format';
import { deliveryService } from '@/services/deliveryService';
import { useToastStore } from '@/store';
import { useAuthStore } from '@/store/authStore';
import type { Delivery, DeliveryHistory } from '@/types';
import { DeliveryStatus } from '@/types';

const STATUS_CONFIG: Record<string, { label: string; badgeColor: 'default' | 'info' | 'primary' | 'warning' | 'success' | 'error'; icon: React.ElementType; step: number }> = {
  Assigned: { label: 'Đã phân công', badgeColor: 'info', icon: Clock, step: 1 },
  PickedUp: { label: 'Đã lấy hàng', badgeColor: 'primary', icon: Package, step: 2 },
  InTransit: { label: 'Đang giao hàng', badgeColor: 'warning', icon: Truck, step: 3 },
  AtHub: { label: 'Đang tại kho trung chuyển', badgeColor: 'info', icon: MapPin, step: 3 },
  OutForDelivery: { label: 'Đang phát hàng', badgeColor: 'warning', icon: MapPin, step: 4 },
  Delivered: { label: 'Giao thành công', badgeColor: 'success', icon: CheckCircle, step: 5 },
  Failed: { label: 'Giao thất bại', badgeColor: 'error', icon: XCircle, step: 0 },
  Returned: { label: 'Đã hoàn trả', badgeColor: 'error', icon: RotateCcwFallback, step: 0 },
};

function RotateCcwFallback(props: React.SVGProps<SVGSVGElement>) {
  return <XCircle {...props} />;
}

export const DeliveriesPage: React.FC = () => {
  const queryClient = useQueryClient();
  const { user } = useAuthStore();
  const [activeTab, setActiveTab] = useState<'all' | 'my-tasks'>('all');
  const [selectedStatus, setSelectedStatus] = useState<string>('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [pageIndex, setPageIndex] = useState(1);

  // Modal states
  const [selectedDelivery, setSelectedDelivery] = useState<Delivery | null>(null);
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [isAssignModalOpen, setIsAssignModalOpen] = useState(false);
  const [isHistoryModalOpen, setIsHistoryModalOpen] = useState(false);

  // Status update form inside modal
  const [updateStatusVal, setUpdateStatusVal] = useState<string>('');
  const [updateNote, setUpdateNote] = useState('');
  const [updateLocation, setUpdateLocation] = useState('');

  // Assign form state
  const [assignOrderCode, setAssignOrderCode] = useState('');
  const [assignShipperName, setAssignShipperName] = useState('');
  const [assignShipperPhone, setAssignShipperPhone] = useState('');
  const [assignCarrierName, setAssignCarrierName] = useState('Giao hàng nội bộ');
  const [assignTrackingNumber, setAssignTrackingNumber] = useState('');
  const [assignNote, setAssignNote] = useState('');

  // Fetch Deliveries
  const {
    data: deliveriesData,
    isLoading,
    refetch,
    isFetching
  } = useQuery({
    queryKey: ['admin-deliveries', activeTab, selectedStatus, searchQuery, pageIndex],
    queryFn: async () => {
      if (activeTab === 'my-tasks') {
        const res = await deliveryService.getMyTasks({
          status: selectedStatus !== 'all' ? selectedStatus : undefined,
          pageIndex,
          pageSize: 20
        });
        return res.data;
      } else {
        const res = await deliveryService.getDeliveries({
          status: selectedStatus !== 'all' ? selectedStatus : undefined,
          search: searchQuery || undefined,
          pageIndex,
          pageSize: 20
        });
        return res.data;
      }
    }
  });

  const deliveries: Delivery[] = deliveriesData?.items || [];
  const totalCount = deliveriesData?.totalItems || deliveries.length;
  const totalPages = deliveriesData?.totalPages || 1;

  // History query for selected delivery
  const { data: historyData, isLoading: historyLoading } = useQuery({
    queryKey: ['delivery-history', selectedDelivery?.code],
    queryFn: () => (selectedDelivery ? deliveryService.getHistory(selectedDelivery.code) : null),
    enabled: !!selectedDelivery && isHistoryModalOpen
  });

  // Update status mutation
  const updateStatusMutation = useMutation({
    mutationFn: (payload: { code: string; status: string; note?: string; location?: string }) =>
      deliveryService.updateStatus(payload.code, {
        status: payload.status,
        note: payload.note,
        location: payload.location
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-deliveries'] });
      queryClient.invalidateQueries({ queryKey: ['delivery-history'] });
      useToastStore.getState().success('Cập nhật trạng thái giao hàng thành công');
      setIsDetailModalOpen(false);
      setUpdateNote('');
      setUpdateLocation('');
    },
    onError: (err: any) => {
      useToastStore.getState().error(err.message || 'Không thể cập nhật trạng thái');
    }
  });

  // Assign shipper mutation
  const assignMutation = useMutation({
    mutationFn: (payload: any) => deliveryService.assignShipper(payload.orderCode, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-deliveries'] });
      useToastStore.getState().success('Phân công shipper thành công');
      setIsAssignModalOpen(false);
      resetAssignForm();
    },
    onError: (err: any) => {
      useToastStore.getState().error(err.message || 'Phân công shipper thất bại');
    }
  });

  const resetAssignForm = () => {
    setAssignOrderCode('');
    setAssignShipperName('');
    setAssignShipperPhone('');
    setAssignCarrierName('Giao hàng nội bộ');
    setAssignTrackingNumber('');
    setAssignNote('');
  };

  const openAssignModal = (orderCode?: string) => {
    resetAssignForm();
    if (orderCode) setAssignOrderCode(orderCode);
    setIsAssignModalOpen(true);
  };

  const handleQuickStatusChange = (code: string, newStatus: string) => {
    updateStatusMutation.mutate({ code, status: newStatus, note: `Cập nhật nhanh sang ${newStatus}` });
  };

  const handleDetailedStatusSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedDelivery || !updateStatusVal) return;
    updateStatusMutation.mutate({
      code: selectedDelivery.code,
      status: updateStatusVal,
      note: updateNote,
      location: updateLocation
    });
  };

  // Helper stats
  const countAssigned = deliveries.filter(d => String(d.status).toLowerCase() === 'assigned').length;
  const countInTransit = deliveries.filter(d => ['intransit', 'pickedup', 'outfordelivery'].includes(String(d.status).toLowerCase())).length;
  const countDelivered = deliveries.filter(d => String(d.status).toLowerCase() === 'delivered').length;
  const countFailed = deliveries.filter(d => ['failed', 'returned'].includes(String(d.status).toLowerCase())).length;

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="Quản lý giao hàng & Shipper"
        subtitle="Theo dõi lộ trình, phân công vận chuyển và cập nhật trạng thái đơn hàng"
        rightSection={
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => refetch()}
              className="flex items-center gap-1.5"
            >
              <RefreshCw size={14} className={isFetching ? 'animate-spin' : ''} />
              Làm mới
            </Button>
            <Button
              variant="primary"
              size="sm"
              onClick={() => openAssignModal()}
              className="flex items-center gap-1.5"
            >
              <UserCheck size={16} />
              Tạo phiếu giao hàng
            </Button>
          </div>
        }
      />

      {/* Tabs */}
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-border pb-3">
        <div className="flex items-center gap-2">
          <button
            onClick={() => { setActiveTab('all'); setPageIndex(1); }}
            className={`px-4 py-2 rounded-xl text-sm font-semibold transition-all flex items-center gap-2 ${
              activeTab === 'all'
                ? 'bg-primary text-white shadow-sm shadow-primary/25'
                : 'text-secondary hover:text-primary hover:bg-secondary/40'
            }`}
          >
            <Truck size={16} />
            Tất cả vận đơn
            <span className="px-2 py-0.5 rounded-full text-xs bg-black/15 font-mono">{totalCount}</span>
          </button>
          <button
            onClick={() => { setActiveTab('my-tasks'); setPageIndex(1); }}
            className={`px-4 py-2 rounded-xl text-sm font-semibold transition-all flex items-center gap-2 ${
              activeTab === 'my-tasks'
                ? 'bg-primary text-white shadow-sm shadow-primary/25'
                : 'text-secondary hover:text-primary hover:bg-secondary/40'
            }`}
          >
            <Package size={16} />
            Nhiệm vụ của tôi (Shipper)
          </button>
        </div>

        {/* Search Input */}
        <div className="relative min-w-[260px] max-w-sm flex-1 sm:flex-initial">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary" />
          <input
            type="text"
            placeholder="Tìm mã đơn, vận đơn, khách hàng..."
            value={searchQuery}
            onChange={(e) => { setSearchQuery(e.target.value); setPageIndex(1); }}
            className="w-full pl-9 pr-4 py-2 text-sm bg-bg-primary rounded-xl border border-border focus:outline-none focus:border-primary transition-all"
          />
        </div>
      </div>

      {/* Metric Cards */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div className="bg-bg-primary rounded-xl p-4 border border-border shadow-sm flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-blue-500/10 text-blue-500 flex items-center justify-center">
            <Clock size={20} />
          </div>
          <div>
            <p className="text-xl font-bold">{countAssigned}</p>
            <p className="text-xs text-secondary">Chờ lấy hàng</p>
          </div>
        </div>

        <div className="bg-bg-primary rounded-xl p-4 border border-border shadow-sm flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-amber-500/10 text-amber-500 flex items-center justify-center">
            <Truck size={20} />
          </div>
          <div>
            <p className="text-xl font-bold">{countInTransit}</p>
            <p className="text-xs text-secondary">Đang vận chuyển</p>
          </div>
        </div>

        <div className="bg-bg-primary rounded-xl p-4 border border-border shadow-sm flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-emerald-500/10 text-emerald-500 flex items-center justify-center">
            <CheckCircle size={20} />
          </div>
          <div>
            <p className="text-xl font-bold">{countDelivered}</p>
            <p className="text-xs text-secondary">Giao thành công</p>
          </div>
        </div>

        <div className="bg-bg-primary rounded-xl p-4 border border-border shadow-sm flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-rose-500/10 text-rose-500 flex items-center justify-center">
            <XCircle size={20} />
          </div>
          <div>
            <p className="text-xl font-bold">{countFailed}</p>
            <p className="text-xs text-secondary">Thất bại / Hoàn trả</p>
          </div>
        </div>
      </div>

      {/* Status Filter Pills */}
      <div className="flex flex-wrap items-center gap-2">
        <button
          onClick={() => { setSelectedStatus('all'); setPageIndex(1); }}
          className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
            selectedStatus === 'all'
              ? 'bg-primary text-white'
              : 'bg-bg-secondary text-secondary hover:bg-secondary'
          }`}
        >
          Tất cả
        </button>
        {Object.entries(STATUS_CONFIG).map(([key, cfg]) => (
          <button
            key={key}
            onClick={() => { setSelectedStatus(key); setPageIndex(1); }}
            className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors flex items-center gap-1.5 ${
              selectedStatus === key
                ? 'bg-primary text-white'
                : 'bg-bg-secondary text-secondary hover:bg-secondary'
            }`}
          >
            <cfg.icon size={12} />
            {cfg.label}
          </button>
        ))}
      </div>

      {/* Deliveries Table / Cards */}
      <div className="bg-bg-primary rounded-xl border border-border shadow-sm overflow-hidden">
        {isLoading ? (
          <div className="py-20 text-center">
            <Loader2 className="animate-spin mx-auto text-primary mb-2" size={32} />
            <p className="text-sm text-secondary">Đang tải danh sách đơn giao hàng...</p>
          </div>
        ) : deliveries.length === 0 ? (
          <div className="py-16 text-center space-y-3">
            <Truck size={44} className="mx-auto text-tertiary opacity-40" />
            <p className="font-semibold text-secondary">Không có đơn giao hàng nào</p>
            <p className="text-xs text-tertiary">Các đơn hàng khi được phân công giao hàng sẽ xuất hiện tại đây.</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="bg-bg-secondary border-b border-border text-xs text-tertiary uppercase">
                <tr>
                  <th className="py-3.5 px-4 font-semibold">Đơn hàng / Vận đơn</th>
                  <th className="py-3.5 px-4 font-semibold">Khách hàng & Địa chỉ</th>
                  <th className="py-3.5 px-4 font-semibold">Shipper / Đơn vị</th>
                  <th className="py-3.5 px-4 font-semibold text-right">Thu hộ (COD)</th>
                  <th className="py-3.5 px-4 font-semibold text-center">Trạng thái</th>
                  <th className="py-3.5 px-4 font-semibold text-right">Thao tác nhanh</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {deliveries.map((delivery) => {
                  const statusStr = String(delivery.status);
                  const cfg = STATUS_CONFIG[statusStr] || { label: statusStr, badgeColor: 'default', icon: Clock };
                  const isDelivered = statusStr === 'Delivered';
                  const isFailed = statusStr === 'Failed';

                  return (
                    <tr key={delivery.code} className="hover:bg-bg-secondary/40 transition-colors">
                      {/* Order & Tracking */}
                      <td className="py-3.5 px-4">
                        <div className="space-y-0.5">
                          <p className="font-semibold text-primary">{delivery.orderCode}</p>
                          <p className="text-xs text-tertiary font-mono">
                            {delivery.trackingNumber ? `Mã VĐ: ${delivery.trackingNumber}` : 'Chưa có mã vận đơn'}
                          </p>
                          <p className="text-xs text-tertiary">
                            {delivery.createdAt ? formatDate(delivery.createdAt) : ''}
                          </p>
                        </div>
                      </td>

                      {/* Customer & Address */}
                      <td className="py-3.5 px-4 max-w-xs">
                        <div>
                          <p className="font-medium text-text-primary">{delivery.customerName || 'Khách hàng'}</p>
                          {delivery.customerPhone && (
                            <a
                              href={`tel:${delivery.customerPhone}`}
                              className="text-xs text-primary font-mono inline-flex items-center gap-1 hover:underline"
                            >
                              <Phone size={11} />
                              {delivery.customerPhone}
                            </a>
                          )}
                          <p className="text-xs text-secondary truncate mt-0.5" title={delivery.deliveryAddress}>
                            {delivery.deliveryAddress || 'Chưa có địa chỉ'}
                          </p>
                        </div>
                      </td>

                      {/* Shipper */}
                      <td className="py-3.5 px-4">
                        <div>
                          <p className="font-medium text-text-primary">
                            {delivery.shipperName || <span className="text-tertiary italic">Chưa chỉ định</span>}
                          </p>
                          {delivery.shipperPhone && (
                            <p className="text-xs text-tertiary font-mono">{delivery.shipperPhone}</p>
                          )}
                          <p className="text-xs text-tertiary">{delivery.carrierName || 'Nội bộ'}</p>
                        </div>
                      </td>

                      {/* Amount */}
                      <td className="py-3.5 px-4 text-right font-medium text-error">
                        {delivery.orderFinalAmount ? formatCurrency(delivery.orderFinalAmount) : '—'}
                      </td>

                      {/* Status */}
                      <td className="py-3.5 px-4 text-center">
                        <Badge color={cfg.badgeColor} size="sm">
                          <cfg.icon size={12} className="mr-1 inline" />
                          {cfg.label}
                        </Badge>
                      </td>

                      {/* Actions */}
                      <td className="py-3.5 px-4 text-right">
                        <div className="flex items-center justify-end gap-1.5">
                          {/* Shipper fast progress buttons */}
                          {!isDelivered && !isFailed && (
                            <>
                              {statusStr === 'Assigned' && (
                                <Button
                                  size="xs"
                                  variant="outline"
                                  onClick={() => handleQuickStatusChange(delivery.code, 'PickedUp')}
                                  disabled={updateStatusMutation.isPending}
                                  title="Đã nhận hàng từ kho"
                                >
                                  Lấy hàng
                                </Button>
                              )}
                              {statusStr === 'PickedUp' && (
                                <Button
                                  size="xs"
                                  variant="outline"
                                  onClick={() => handleQuickStatusChange(delivery.code, 'InTransit')}
                                  disabled={updateStatusMutation.isPending}
                                  title="Bắt đầu vận chuyển đến khách"
                                >
                                  Giao ngay
                                </Button>
                              )}
                              {(statusStr === 'InTransit' || statusStr === 'OutForDelivery') && (
                                <Button
                                  size="xs"
                                  variant="primary"
                                  onClick={() => handleQuickStatusChange(delivery.code, 'Delivered')}
                                  disabled={updateStatusMutation.isPending}
                                  title="Xác nhận giao hàng thành công"
                                >
                                  Đã giao
                                </Button>
                              )}
                            </>
                          )}

                          {/* Detail & History */}
                          <Button
                            size="xs"
                            variant="ghost"
                            onClick={() => {
                              setSelectedDelivery(delivery);
                              setUpdateStatusVal(delivery.status);
                              setIsDetailModalOpen(true);
                            }}
                          >
                            Cập nhật
                          </Button>
                          <Button
                            size="xs"
                            variant="ghost"
                            onClick={() => {
                              setSelectedDelivery(delivery);
                              setIsHistoryModalOpen(true);
                            }}
                            title="Lịch sử giao hàng"
                          >
                            <History size={14} />
                          </Button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="p-4 border-t border-border flex items-center justify-between text-xs text-secondary">
            <span>Trang {pageIndex} / {totalPages}</span>
            <div className="flex items-center gap-2">
              <Button
                size="xs"
                variant="outline"
                disabled={pageIndex <= 1}
                onClick={() => setPageIndex(p => Math.max(1, p - 1))}
              >
                Trước
              </Button>
              <Button
                size="xs"
                variant="outline"
                disabled={pageIndex >= totalPages}
                onClick={() => setPageIndex(p => Math.min(totalPages, p + 1))}
              >
                Sau
              </Button>
            </div>
          </div>
        )}
      </div>

      {/* ================= MODAL: UPDATE STATUS ================= */}
      <Modal
        isOpen={isDetailModalOpen}
        onClose={() => setIsDetailModalOpen(false)}
        title={`Cập nhật vận đơn: ${selectedDelivery?.orderCode}`}
        size="md"
      >
        {selectedDelivery && (
          <form onSubmit={handleDetailedStatusSubmit} className="space-y-4">
            <div className="p-3 bg-bg-secondary rounded-xl text-xs space-y-1">
              <p><span className="text-secondary">Khách hàng:</span> <strong className="text-text-primary">{selectedDelivery.customerName}</strong> ({selectedDelivery.customerPhone})</p>
              <p><span className="text-secondary">Địa chỉ:</span> {selectedDelivery.deliveryAddress}</p>
              <p><span className="text-secondary">Đơn vị:</span> {selectedDelivery.carrierName || 'Nội bộ'} | Mã: {selectedDelivery.trackingNumber || '—'}</p>
            </div>

            <div>
              <label className="block text-xs font-semibold text-secondary mb-1">Trạng thái mới</label>
              <select
                value={updateStatusVal}
                onChange={(e) => setUpdateStatusVal(e.target.value)}
                className="w-full px-3 py-2 text-sm bg-bg-primary rounded-xl border border-border focus:outline-none focus:border-primary"
              >
                {Object.entries(STATUS_CONFIG).map(([k, cfg]) => (
                  <option key={k} value={k}>{cfg.label}</option>
                ))}
              </select>
            </div>

            <Input
              label="Vị trí / Trạm trung chuyển"
              placeholder="VD: Bưu cục Quận 1, Trên đường giao..."
              value={updateLocation}
              onChange={(e) => setUpdateLocation(e.target.value)}
            />

            <Input
              label="Ghi chú cập nhật"
              placeholder="VD: Khách hẹn giao sau 5h, Đã liên hệ khách..."
              value={updateNote}
              onChange={(e) => setUpdateNote(e.target.value)}
            />

            <div className="flex justify-end gap-2 pt-2 border-t border-border">
              <Button variant="ghost" size="sm" type="button" onClick={() => setIsDetailModalOpen(false)}>
                Hủy
              </Button>
              <Button variant="primary" size="sm" type="submit" isLoading={updateStatusMutation.isPending}>
                Lưu trạng thái
              </Button>
            </div>
          </form>
        )}
      </Modal>

      {/* ================= MODAL: ASSIGN SHIPPER ================= */}
      <Modal
        isOpen={isAssignModalOpen}
        onClose={() => setIsAssignModalOpen(false)}
        title="Tạo phiếu giao hàng / Phân công Shipper"
        size="md"
      >
        <form
          onSubmit={(e) => {
            e.preventDefault();
            if (!assignOrderCode) {
              useToastStore.getState().error('Vui lòng nhập mã đơn hàng');
              return;
            }
            assignMutation.mutate({
              orderCode: assignOrderCode.trim(),
              shipperName: assignShipperName,
              shipperPhone: assignShipperPhone,
              carrierName: assignCarrierName,
              trackingNumber: assignTrackingNumber,
              note: assignNote
            });
          }}
          className="space-y-4"
        >
          <Input
            label="Mã đơn hàng (*)"
            placeholder="VD: ORD-202610-001"
            value={assignOrderCode}
            onChange={(e) => setAssignOrderCode(e.target.value)}
            required
          />

          <div className="grid grid-cols-2 gap-3">
            <Input
              label="Đơn vị vận chuyển"
              placeholder="GHTK, ViettelPost, Nội bộ..."
              value={assignCarrierName}
              onChange={(e) => setAssignCarrierName(e.target.value)}
            />
            <Input
              label="Mã vận đơn"
              placeholder="VD: VNVT-123456"
              value={assignTrackingNumber}
              onChange={(e) => setAssignTrackingNumber(e.target.value)}
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <Input
              label="Tên Shipper"
              placeholder="VD: Nguyễn Văn A"
              value={assignShipperName}
              onChange={(e) => setAssignShipperName(e.target.value)}
            />
            <Input
              label="SĐT Shipper"
              placeholder="VD: 0901234567"
              value={assignShipperPhone}
              onChange={(e) => setAssignShipperPhone(e.target.value)}
            />
          </div>

          <Input
            label="Ghi chú giao hàng"
            placeholder="VD: Giao giờ hành chính, hàng dễ vỡ..."
            value={assignNote}
            onChange={(e) => setAssignNote(e.target.value)}
          />

          <div className="flex justify-end gap-2 pt-2 border-t border-border">
            <Button variant="ghost" size="sm" type="button" onClick={() => setIsAssignModalOpen(false)}>
              Hủy
            </Button>
            <Button variant="primary" size="sm" type="submit" isLoading={assignMutation.isPending}>
              Xác nhận phân công
            </Button>
          </div>
        </form>
      </Modal>

      {/* ================= MODAL: DELIVERY HISTORY ================= */}
      <Modal
        isOpen={isHistoryModalOpen}
        onClose={() => setIsHistoryModalOpen(false)}
        title={`Lịch trình vận đơn: ${selectedDelivery?.trackingNumber || selectedDelivery?.orderCode}`}
        size="md"
      >
        {historyLoading ? (
          <div className="py-8 text-center">
            <Loader2 className="animate-spin mx-auto text-primary" size={24} />
          </div>
        ) : !historyData?.data || historyData.data.length === 0 ? (
          <p className="text-center text-sm text-secondary py-6">Chưa có lịch sử di chuyển</p>
        ) : (
          <div className="space-y-4 py-2">
            {historyData.data.map((item, idx) => {
              const cfg = STATUS_CONFIG[item.statusText] || { label: item.statusText, badgeColor: 'default', icon: Clock };
              return (
                <div key={idx} className="flex gap-3 text-xs">
                  <div className="flex flex-col items-center">
                    <div className="w-6 h-6 rounded-full bg-primary/10 text-primary flex items-center justify-center font-bold">
                      {idx + 1}
                    </div>
                    {idx < historyData.data.length - 1 && <div className="w-0.5 flex-1 bg-border my-1" />}
                  </div>
                  <div className="flex-1 pb-3">
                    <div className="flex items-center justify-between">
                      <Badge color={cfg.badgeColor} size="sm">{cfg.label}</Badge>
                      <span className="text-tertiary font-mono">{formatDate(item.timestamp)}</span>
                    </div>
                    {item.location && <p className="font-medium text-text-primary mt-1">📍 {item.location}</p>}
                    {item.note && <p className="text-secondary mt-0.5 italic">"{item.note}"</p>}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </Modal>
    </div>
  );
};

export default DeliveriesPage;
