
export const NotificationsContent = () => (
  <div className="bg-primary rounded-xl p-6">
    <h2 className="text-xl font-bold mb-4">Thông báo</h2>
    <p className="text-secondary">Không có thông báo mới.</p>
  </div>
);

export const SettingsContent = () => (
  <div className="bg-primary rounded-xl p-6">
    <h2 className="text-xl font-bold mb-6">Cài đặt</h2>
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <span>Nhận email khuyến mãi</span>
        <input type="checkbox" defaultChecked className="w-5 h-5" />
      </div>
      <div className="flex items-center justify-between">
        <span>Nhận thông báo đơn hàng</span>
        <input type="checkbox" defaultChecked className="w-5 h-5" />
      </div>
    </div>
  </div>
);
