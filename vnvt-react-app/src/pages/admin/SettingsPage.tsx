import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Save, Store, CreditCard, Truck, Bell, Shield, Globe } from 'lucide-react';
import { Button, Input, Select } from '@/components/ui';

export const SettingsPage = () => {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState('general');

  const tabs = [
    { id: 'general', label: 'Thông tin cửa hàng', icon: Store },
    { id: 'payment', label: 'Thanh toán', icon: CreditCard },
    { id: 'shipping', label: 'Vận chuyển', icon: Truck },
    { id: 'notifications', label: 'Thông báo', icon: Bell },
    { id: 'security', label: 'Bảo mật', icon: Shield },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">{t('admin.settings')}</h1>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        {/* Sidebar */}
        <div className="lg:col-span-1">
          <div className="bg-primary rounded-xl p-2">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`flex items-center gap-3 w-full px-4 py-3 rounded-lg text-left transition-colors ${
                  activeTab === tab.id ? 'bg-primary/10 text-primary' : 'hover:bg-secondary text-secondary'
                }`}
              >
                <tab.icon size={18} />
                <span className="font-medium">{tab.label}</span>
              </button>
            ))}
          </div>
        </div>

        {/* Content */}
        <div className="lg:col-span-3">
          <div className="bg-primary rounded-xl p-6">
            {/* General Settings */}
            {activeTab === 'general' && (
              <div className="space-y-6">
                <h2 className="text-lg font-bold flex items-center gap-2">
                  <Store size={20} />
                  Thông tin cửa hàng
                </h2>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Input label="Tên cửa hàng" defaultValue="VNVT Store" />
                  <Input label="Email liên hệ" type="email" defaultValue="contact@vnvt.store" />
                  <Input label="Số điện thoại" defaultValue="1900 123 456" />
                  <Input label="Website" defaultValue="https://vnvt.store" />
                  <div className="md:col-span-2">
                    <Input label="Địa chỉ" defaultValue="123 Nguyễn Huệ, Q.1, TP.HCM" />
                  </div>
                  <div className="md:col-span-2">
                    <label className="block text-sm font-medium mb-2">Mô tả cửa hàng</label>
                    <textarea
                      rows={3}
                      defaultValue="Cửa hàng đồ gia dụng cao cấp - Chất lượng tạo nên sự khác biệt"
                      className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:border-primary resize-none"
                    />
                  </div>
                </div>

                <Button leftIcon={<Save size={18} />}>Lưu thay đổi</Button>
              </div>
            )}

            {/* Payment Settings */}
            {activeTab === 'payment' && (
              <div className="space-y-6">
                <h2 className="text-lg font-bold flex items-center gap-2">
                  <CreditCard size={20} />
                  Cài đặt thanh toán
                </h2>

                <div className="space-y-4">
                  {[
                    { name: 'COD', desc: 'Thanh toán khi nhận hàng', enabled: true },
                    { name: 'ZaloPay', desc: 'Thanh toán qua ví ZaloPay', enabled: true },
                    { name: 'MoMo', desc: 'Thanh toán qua ví MoMo', enabled: false },
                    { name: 'VNPAY', desc: 'Thanh toán qua VNPAY QR', enabled: true },
                    { name: 'Chuyển khoản', desc: 'Chuyển khoản ngân hàng', enabled: true },
                  ].map((method) => (
                    <div key={method.name} className="flex items-center justify-between p-4 border rounded-lg">
                      <div>
                        <p className="font-medium">{method.name}</p>
                        <p className="text-sm text-tertiary">{method.desc}</p>
                      </div>
                      <label className="relative inline-flex items-center cursor-pointer">
                        <input type="checkbox" defaultChecked={method.enabled} className="sr-only peer" />
                        <div className="w-11 h-6 bg-gray-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:bg-primary after:content-[''] after:absolute after:top-0.5 after:left-0.5 after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all"></div>
                      </label>
                    </div>
                  ))}
                </div>

                <Button leftIcon={<Save size={18} />}>Lưu thay đổi</Button>
              </div>
            )}

            {/* Shipping Settings */}
            {activeTab === 'shipping' && (
              <div className="space-y-6">
                <h2 className="text-lg font-bold flex items-center gap-2">
                  <Truck size={20} />
                  Cài đặt vận chuyển
                </h2>

                <div className="space-y-4">
                  <Input
                    label="Phí ship mặc định"
                    type="number"
                    defaultValue="30000"
                    helperText="Đơn vị: VNĐ"
                  />
                  <Input
                    label="Miễn phí ship cho đơn từ"
                    type="number"
                    defaultValue="500000"
                    helperText="Đơn hàng từ giá trị này trở lên được miễn phí ship"
                  />
                  <Input
                    label="Thời gian giao hàng dự kiến"
                    defaultValue="2-5 ngày làm việc"
                  />
                </div>

                <Button leftIcon={<Save size={18} />}>Lưu thay đổi</Button>
              </div>
            )}

            {/* Notification Settings */}
            {activeTab === 'notifications' && (
              <div className="space-y-6">
                <h2 className="text-lg font-bold flex items-center gap-2">
                  <Bell size={20} />
                  Cài đặt thông báo
                </h2>

                <div className="space-y-4">
                  {[
                    { label: 'Email khi có đơn hàng mới', enabled: true },
                    { label: 'Email khi có yêu cầu báo giá', enabled: true },
                    { label: 'Email khách hàng khi đơn thay đổi trạng thái', enabled: true },
                    { label: 'Thông báo hàng sắp hết', enabled: false },
                  ].map((setting, index) => (
                    <div key={index} className="flex items-center justify-between p-4 border rounded-lg">
                      <p className="font-medium">{setting.label}</p>
                      <label className="relative inline-flex items-center cursor-pointer">
                        <input type="checkbox" defaultChecked={setting.enabled} className="sr-only peer" />
                        <div className="w-11 h-6 bg-gray-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:bg-primary after:content-[''] after:absolute after:top-0.5 after:left-0.5 after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all"></div>
                      </label>
                    </div>
                  ))}
                </div>

                <Button leftIcon={<Save size={18} />}>Lưu thay đổi</Button>
              </div>
            )}

            {/* Security Settings */}
            {activeTab === 'security' && (
              <div className="space-y-6">
                <h2 className="text-lg font-bold flex items-center gap-2">
                  <Shield size={20} />
                  Bảo mật
                </h2>

                <div className="p-4 border rounded-lg">
                  <h3 className="font-medium mb-4">Đổi mật khẩu</h3>
                  <div className="space-y-4">
                    <Input label="Mật khẩu hiện tại" type="password" />
                    <Input label="Mật khẩu mới" type="password" />
                    <Input label="Xác nhận mật khẩu mới" type="password" />
                  </div>
                </div>

                <div className="p-4 border rounded-lg">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">Xác thực 2 bước (2FA)</p>
                      <p className="text-sm text-tertiary">Tăng cường bảo mật cho tài khoản</p>
                    </div>
                    <Button variant="outline" size="sm">Bật</Button>
                  </div>
                </div>

                <Button leftIcon={<Save size={18} />}>Lưu thay đổi</Button>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default SettingsPage;
