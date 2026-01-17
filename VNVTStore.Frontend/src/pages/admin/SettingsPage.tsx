import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { Save, Store, CreditCard, Truck, Bell, Shield, Globe } from 'lucide-react';
import { Button, Input, Select } from '@/components/ui';
import { AdminPageHeader } from '@/components/admin';
import { useToast, useSettings } from '@/store';

export const SettingsPage = () => {
  const { t } = useTranslation();
  const toast = useToast();
  const [activeTab, setActiveTab] = useState('general');
  const { settings, updateSettings } = useSettings();

  const handleSave = (section: any, data: any) => {
    updateSettings(section, data);
    toast.success(t('messages.saveSuccess'));
  };

  const tabs = [
    { id: 'general', label: 'Thông tin cửa hàng', icon: Store },
    { id: 'payment', label: 'Thanh toán', icon: CreditCard },
    { id: 'shipping', label: 'Vận chuyển', icon: Truck },
    { id: 'notifications', label: 'Thông báo', icon: Bell },
    { id: 'security', label: 'Bảo mật', icon: Shield },
  ];

  const GeneralSettings = () => {
    const { register, handleSubmit } = useForm({
      defaultValues: settings.general
    });

    return (
      <form onSubmit={handleSubmit((data) => handleSave('general', data))} className="space-y-6">
        <h2 className="text-lg font-bold flex items-center gap-2">
          <Store size={20} />
          {t('admin.subtitles.settings')}
        </h2>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input label="Tên cửa hàng" {...register('storeName')} />
          <Input label="Email liên hệ" type="email" {...register('email')} />
          <Input label="Số điện thoại" {...register('phone')} />
          <Input label="Website" {...register('website')} />
          <div className="md:col-span-2">
            <Input label="Địa chỉ" {...register('address')} />
          </div>
          <div className="md:col-span-2">
            <label className="block text-sm font-medium mb-2">Mô tả cửa hàng</label>
            <textarea
              rows={3}
              {...register('description')}
              className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:border-primary resize-none"
            />
          </div>
        </div>

        <Button type="submit" leftIcon={<Save size={18} />}>{t('common.save')}</Button>
      </form>
    );
  };

  const PaymentSettings = () => {
    const { register, handleSubmit, watch } = useForm({
        defaultValues: settings.payment
    });

    return (
      <form onSubmit={handleSubmit((data) => handleSave('payment', data))} className="space-y-6">
        <h2 className="text-lg font-bold flex items-center gap-2">
          <CreditCard size={20} />
          Cài đặt thanh toán
        </h2>

        <div className="space-y-4">
          {[
            { id: 'cod', name: 'COD', desc: 'Thanh toán khi nhận hàng' },
            { id: 'zaloPay', name: 'ZaloPay', desc: 'Thanh toán qua ví ZaloPay' },
            { id: 'momo', name: 'MoMo', desc: 'Thanh toán qua ví MoMo' },
            { id: 'vnpay', name: 'VNPAY', desc: 'Thanh toán qua VNPAY QR' },
            { id: 'bankTransfer', name: 'Chuyển khoản', desc: 'Chuyển khoản ngân hàng' },
          ].map((method) => (
            <div key={method.id} className="flex items-center justify-between p-4 border rounded-lg">
              <div>
                <p className="font-medium">{method.name}</p>
                <p className="text-sm text-tertiary">{method.desc}</p>
              </div>
              <label className="relative inline-flex items-center cursor-pointer">
                <input 
                    type="checkbox" 
                    {...register(method.id as any)}
                    className="sr-only peer" 
                />
                <div className="w-11 h-6 bg-gray-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:bg-primary after:content-[''] after:absolute after:top-0.5 after:left-0.5 after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all"></div>
              </label>
            </div>
          ))}
        </div>

        <Button type="submit" leftIcon={<Save size={18} />}>{t('common.save')}</Button>
      </form>
    );
  };

  const ShippingSettings = () => {
      const { register, handleSubmit } = useForm({
          defaultValues: settings.shipping
      });
      
      return (
        <form onSubmit={handleSubmit((data) => handleSave('shipping', data))} className="space-y-6">
          <h2 className="text-lg font-bold flex items-center gap-2">
            <Truck size={20} />
            Cài đặt vận chuyển
          </h2>

          <div className="space-y-4">
            <Input
              label="Phí ship mặc định"
              type="number"
              helperText="Đơn vị: VNĐ"
              {...register('defaultFee', { valueAsNumber: true })}
            />
            <Input
              label="Miễn phí ship cho đơn từ"
              type="number"
              helperText="Đơn hàng từ giá trị này trở lên được miễn phí ship"
              {...register('freeShippingThreshold', { valueAsNumber: true })}
            />
            <Input
              label="Thời gian giao hàng dự kiến"
              {...register('estimatedDelivery')}
            />
          </div>

          <Button type="submit" leftIcon={<Save size={18} />}>{t('common.save')}</Button>
        </form>
      );
  };

  const NotificationSettings = () => {
    const { register, handleSubmit } = useForm({
        defaultValues: settings.notifications
    });

    return (
        <form onSubmit={handleSubmit((data) => handleSave('notifications', data))} className="space-y-6">
            <h2 className="text-lg font-bold flex items-center gap-2">
            <Bell size={20} />
            Cài đặt thông báo
            </h2>

            <div className="space-y-4">
            {[
                { id: 'emailNewOrder', label: 'Email khi có đơn hàng mới' },
                { id: 'emailQuoteRequest', label: 'Email khi có yêu cầu báo giá' },
                { id: 'emailOrderStatus', label: 'Email khách hàng khi đơn thay đổi trạng thái' },
                { id: 'lowStockAlert', label: 'Thông báo hàng sắp hết' },
            ].map((setting) => (
                <div key={setting.id} className="flex items-center justify-between p-4 border rounded-lg">
                <p className="font-medium">{setting.label}</p>
                <label className="relative inline-flex items-center cursor-pointer">
                    <input type="checkbox" {...register(setting.id as any)} className="sr-only peer" />
                    <div className="w-11 h-6 bg-gray-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:bg-primary after:content-[''] after:absolute after:top-0.5 after:left-0.5 after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all"></div>
                </label>
                </div>
            ))}
            </div>

            <Button type="submit" leftIcon={<Save size={18} />}>{t('common.save')}</Button>
        </form>
    );
  };

  // Security Mock - no persistence needed for password/modal logic yet
  const SecuritySettings = () => (
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

        <Button leftIcon={<Save size={18} />} onClick={() => toast.success(t('messages.saveSuccess'))}>Lưu thay đổi</Button>
    </div>
  );

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="admin.sidebar.settings"
        subtitle="admin.subtitles.settings"
      />

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        {/* Sidebar */}
        <div className="lg:col-span-1">
          <div className="bg-primary rounded-xl p-2 sticky top-20">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`flex items-center gap-3 w-full px-4 py-3 rounded-lg text-left transition-colors ${activeTab === tab.id ? 'bg-primary/10 text-indigo-600 dark:text-indigo-400 font-medium' : 'hover:bg-secondary text-secondary'
                  }`}
              >
                <tab.icon size={18} />
                <span className="">{tab.label}</span>
              </button>
            ))}
          </div>
        </div>

        {/* Content */}
        <div className="lg:col-span-3">
          <div className="bg-primary rounded-xl p-6 shadow-sm border border-slate-100 dark:border-slate-800">
            {activeTab === 'general' && <GeneralSettings />}
            {activeTab === 'payment' && <PaymentSettings />}
            {activeTab === 'shipping' && <ShippingSettings />}
            {activeTab === 'notifications' && <NotificationSettings />}
            {activeTab === 'security' && <SecuritySettings />}
          </div>
        </div>
      </div>
    </div>
  );
};

export default SettingsPage;
