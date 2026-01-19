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
    { id: 'general', label: t('admin.settingsPage.tabs.general'), icon: Store },
    { id: 'payment', label: t('admin.settingsPage.tabs.payment'), icon: CreditCard },
    { id: 'shipping', label: t('admin.settingsPage.tabs.shipping'), icon: Truck },
    { id: 'notifications', label: t('admin.settingsPage.tabs.notifications'), icon: Bell },
    { id: 'security', label: t('admin.settingsPage.tabs.security'), icon: Shield },
  ];

  const GeneralSettings = () => {
    const { register, handleSubmit } = useForm({
      defaultValues: settings.general
    });

    return (
      <form onSubmit={handleSubmit((data) => handleSave('general', data))} className="space-y-6">
        <h2 className="text-lg font-bold flex items-center gap-2">
          <Store size={20} />
          {t('admin.settingsPage.tabs.general')}
        </h2>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input label={t('admin.settingsPage.general.storeName')} {...register('storeName')} />
          <Input label={t('admin.settingsPage.general.email')} type="email" {...register('email')} />
          <Input label={t('admin.settingsPage.general.phone')} {...register('phone')} />
          <Input label={t('admin.settingsPage.general.website')} {...register('website')} />
          <div className="md:col-span-2">
            <Input label={t('admin.settingsPage.general.address')} {...register('address')} />
          </div>
          <div className="md:col-span-2">
            <label className="block text-sm font-medium mb-2">{t('admin.settingsPage.general.description')}</label>
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
          {t('admin.settingsPage.payment.title')}
        </h2>

        <div className="space-y-4">
          {[
            { id: 'cod', name: t('admin.settingsPage.payment.cod'), desc: t('admin.settingsPage.payment.codDesc') },
            { id: 'zaloPay', name: t('admin.settingsPage.payment.zaloPay'), desc: t('admin.settingsPage.payment.zaloPayDesc') },
            { id: 'momo', name: t('admin.settingsPage.payment.momo'), desc: t('admin.settingsPage.payment.momoDesc') },
            { id: 'vnpay', name: t('admin.settingsPage.payment.vnpay'), desc: t('admin.settingsPage.payment.vnpayDesc') },
            { id: 'bankTransfer', name: t('admin.settingsPage.payment.bankTransfer'), desc: t('admin.settingsPage.payment.bankTransferDesc') },
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
            {t('admin.settingsPage.shipping.title')}
          </h2>

          <div className="space-y-4">
            <Input
              label={t('admin.settingsPage.shipping.defaultFee')}
              type="number"
              helperText={t('admin.settingsPage.shipping.currencyUnit')}
              {...register('defaultFee', { valueAsNumber: true })}
            />
            <Input
              label={t('admin.settingsPage.shipping.freeShippingThreshold')}
              type="number"
              helperText={t('admin.settingsPage.shipping.freeShippingDesc')}
              {...register('freeShippingThreshold', { valueAsNumber: true })}
            />
            <Input
              label={t('admin.settingsPage.shipping.estimatedDelivery')}
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
            {t('admin.settingsPage.notifications.title')}
            </h2>

            <div className="space-y-4">
            {[
                { id: 'emailNewOrder', label: t('admin.settingsPage.notifications.emailNewOrder') },
                { id: 'emailQuoteRequest', label: t('admin.settingsPage.notifications.emailQuoteRequest') },
                { id: 'emailOrderStatus', label: t('admin.settingsPage.notifications.emailOrderStatus') },
                { id: 'lowStockAlert', label: t('admin.settingsPage.notifications.lowStockAlert') },
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
        {t('admin.settingsPage.security.title')}
        </h2>

        <div className="p-4 border rounded-lg">
        <h3 className="font-medium mb-4">{t('admin.settingsPage.security.changePassword')}</h3>
        <div className="space-y-4">
            <Input label={t('admin.settingsPage.security.currentPassword')} type="password" />
            <Input label={t('admin.settingsPage.security.newPassword')} type="password" />
            <Input label={t('admin.settingsPage.security.confirmNewPassword')} type="password" />
        </div>
        </div>

        <div className="p-4 border rounded-lg">
        <div className="flex items-center justify-between">
            <div>
            <p className="font-medium">{t('admin.settingsPage.security.twoFactor')}</p>
            <p className="text-sm text-tertiary">{t('admin.settingsPage.security.twoFactorDesc')}</p>
            </div>
            <Button variant="outline" size="sm">{t('admin.settingsPage.security.enable')}</Button>
        </div>
        </div>

        <Button leftIcon={<Save size={18} />} onClick={() => toast.success(t('messages.saveSuccess'))}>{t('common.save')}</Button>
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
