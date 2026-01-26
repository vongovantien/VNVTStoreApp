import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { Save, Store, CreditCard, Truck, Bell, Shield, Globe, Eye, EyeOff, Lock } from 'lucide-react';
import { Button, Input, Select, Switch } from '@/components/ui';
import { AdminPageHeader } from '@/components/admin';
import { useToast, useSettings } from '@/store';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { createSchemas } from '@/utils/schemas';

export const SettingsPage = () => {
    const { t } = useTranslation();
    const { 
        generalSettingsSchema, 
        paymentSettingsSchema, 
        shippingSettingsSchema, 
        notificationsSettingsSchema, 
        changePasswordSchema 
    } = createSchemas(t);
  const toast = useToast();
  const [activeTab, setActiveTab] = useState('general');
  const { settings, updateSettings } = useSettings();

  const handleSave = (section: keyof typeof settings, data: any) => {
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
    const { register, handleSubmit, formState: { errors } } = useForm({
      resolver: zodResolver(generalSettingsSchema),
      defaultValues: settings.general
    });

    return (
      <form onSubmit={handleSubmit((data) => handleSave('general', data))} className="space-y-6">
        <h2 className="text-lg font-bold flex items-center gap-2">
          <Store size={20} />
          {t('admin.settingsPage.tabs.general')}
        </h2>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input 
            label={t('admin.settingsPage.general.storeName')} 
            placeholder={t('common.placeholders.storeName')}
            {...register('storeName')} 
            error={errors.storeName?.message} 
          />
          <Input 
            label={t('admin.settingsPage.general.email')} 
            type="email" 
            placeholder={t('common.placeholders.email')}
            {...register('email')} 
            error={errors.email?.message} 
          />
          <Input 
            label={t('admin.settingsPage.general.phone')} 
            placeholder={t('common.placeholders.phone')}
            {...register('phone')} 
            error={errors.phone?.message} 
          />
          <Input 
            label={t('admin.settingsPage.general.website')} 
            placeholder={t('common.placeholders.website')}
            {...register('website')} 
            error={errors.website?.message} 
          />
          <div className="md:col-span-2">
            <Input 
                label={t('admin.settingsPage.general.address')} 
                placeholder={t('common.placeholders.address')}
                {...register('address')} 
                error={errors.address?.message} 
            />
          </div>
          <div className="md:col-span-2">
            <label className="block text-sm font-bold text-primary mb-2">{t('admin.settingsPage.general.description')}</label>
            <textarea
              rows={3}
              {...register('description')}
              placeholder={t('common.placeholders.description')}
              className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary resize-none placeholder:text-tertiary text-sm"
            />
          </div>
        </div>

        <Button type="submit" leftIcon={<Save size={18} />}>{t('common.save')}</Button>
      </form>
    );
  };

  const PaymentSettings = () => {
    const { register, handleSubmit, setValue, watch } = useForm({
        resolver: zodResolver(paymentSettingsSchema),
        defaultValues: settings.payment
    });

    const formState = watch();

    return (
      <form onSubmit={handleSubmit((data) => handleSave('payment', data))} className="space-y-6">
        <h2 className="text-lg font-bold flex items-center gap-2">
          <CreditCard size={20} />
          {t('admin.settingsPage.payment.title')}
        </h2>

        <div className="space-y-4">
          {[
            { id: 'cod', name: t('admin.settingsPage.payment.cod'), desc: t('admin.settingsPage.payment.codDesc') },
            { id: 'zaloPay', name: t('shared.zaloPay'), desc: t('admin.settingsPage.payment.zaloPayDesc') },
            { id: 'momo', name: t('shared.momo'), desc: t('admin.settingsPage.payment.momoDesc') },
            { id: 'vnpay', name: t('shared.vnpay'), desc: t('admin.settingsPage.payment.vnpayDesc') },
            { id: 'bankTransfer', name: t('admin.settingsPage.payment.bankTransfer'), desc: t('admin.settingsPage.payment.bankTransferDesc') },
          ].map((method) => (
            <div key={method.id} className="p-1">
                <Switch
                    label={method.name}
                    description={method.desc}
                    checked={formState[method.id as keyof typeof formState] as boolean}
                    onChange={(val) => setValue(method.id as any, val)}
                    className="p-4 bg-slate-50 dark:bg-slate-800/40 border border-slate-100 dark:border-slate-800 rounded-xl"
                />
            </div>
          ))}
        </div>

        <Button type="submit" leftIcon={<Save size={18} />}>{t('common.save')}</Button>
      </form>
    );
  };

  const ShippingSettings = () => {
      const { register, handleSubmit, formState: { errors } } = useForm({
          resolver: zodResolver(shippingSettingsSchema),
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
              placeholder={t('common.placeholders.shippingFee')}
              {...register('defaultFee', { valueAsNumber: true })}
              error={errors.defaultFee?.message}
            />
            <Input
              label={t('admin.settingsPage.shipping.freeShippingThreshold')}
              type="number"
              placeholder={t('common.placeholders.shippingThreshold')}
              {...register('freeShippingThreshold', { valueAsNumber: true })}
              error={errors.freeShippingThreshold?.message}
            />
            <Input
              label={t('admin.settingsPage.shipping.estimatedDelivery')}
              placeholder={t('common.placeholders.deliveryTime')}
              {...register('estimatedDelivery')}
              error={errors.estimatedDelivery?.message}
            />
          </div>

          <Button type="submit" leftIcon={<Save size={18} />}>{t('common.save')}</Button>
        </form>
      );
  };

  const NotificationSettings = () => {
    const { register, handleSubmit, setValue, watch } = useForm({
        resolver: zodResolver(notificationsSettingsSchema),
        defaultValues: settings.notifications
    });

    const formState = watch();

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
                <div key={setting.id} className="p-1">
                    <Switch
                        label={setting.label}
                        checked={formState[setting.id as keyof typeof formState] as boolean}
                        onChange={(val) => setValue(setting.id as any, val)}
                        className="p-4 bg-slate-50 dark:bg-slate-800/40 border border-slate-100 dark:border-slate-800 rounded-xl"
                    />
                </div>
            ))}
            </div>

            <Button type="submit" leftIcon={<Save size={18} />}>{t('common.save')}</Button>
        </form>
    );
  };

  // Helper for password fields with toggle
  interface PasswordInputProps extends React.ComponentProps<typeof Input> {
    label: string;
  }
  const PasswordInput = ({ label, ...props }: PasswordInputProps) => {
    const [show, setShow] = useState(false);
    return (
      <Input
        label={label}
        type={show ? 'text' : 'password'}
        rightIcon={
          <button
            type="button"
            onClick={() => setShow(!show)}
            className="flex items-center justify-center p-1 text-slate-400 opacity-50 hover:opacity-100 hover:text-primary focus:outline-none transition-all"
            tabIndex={-1}
          >
            {show ? <EyeOff size={18} /> : <Eye size={18} />}
          </button>
        }
        {...props}
      />
    );
  };

  // Security Mock - no persistence needed for password/modal logic yet
  const SecuritySettings = () => {
    const { register, handleSubmit, formState: { errors }, reset } = useForm({
        resolver: zodResolver(changePasswordSchema),
        defaultValues: {
            currentPassword: '',
            newPassword: '',
            confirmPassword: '',
        }
    });

    const onSubmit = (data: z.infer<typeof changePasswordSchema>) => {
        // Logic to call API or update store
        toast.success(t('messages.saveSuccess'));
        reset();
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            <h2 className="text-lg font-bold flex items-center gap-2">
            <Shield size={20} />
            {t('admin.settingsPage.security.title')}
            </h2>

            <div className="p-4 border border-gray-100 dark:border-slate-800 rounded-xl bg-slate-50/50 dark:bg-slate-900/20">
            <h3 className="font-bold text-md mb-4 flex items-center gap-2">
                <Lock size={18} className="text-tertiary" />
                {t('admin.settingsPage.security.changePassword')}
            </h3>
            <div className="space-y-4">
                <PasswordInput 
                    label={t('admin.settingsPage.security.currentPassword')} 
                    placeholder={t('common.placeholders.currentPassword')}
                    {...register('currentPassword')} 
                    error={errors.currentPassword?.message}
                />
                <PasswordInput 
                    label={t('admin.settingsPage.security.newPassword')} 
                    placeholder={t('common.placeholders.newPassword')}
                    {...register('newPassword')} 
                    error={errors.newPassword?.message}
                />
                <PasswordInput 
                    label={t('admin.settingsPage.security.confirmNewPassword')} 
                    placeholder={t('common.placeholders.confirmPassword')}
                    {...register('confirmPassword')} 
                    error={errors.confirmPassword?.message}
                />
            </div>
            </div>

            <div className="p-4 border rounded-lg">
            <div className="flex items-center justify-between">
                <div>
                <p className="font-medium">{t('admin.settingsPage.security.twoFactor')}</p>
                <p className="text-sm text-tertiary">{t('admin.settingsPage.security.twoFactorDesc')}</p>
                </div>
                <Button variant="outline" size="sm" type="button">{t('admin.settingsPage.security.enable')}</Button>
            </div>
            </div>

            <Button type="submit" leftIcon={<Save size={18} />}>{t('common.save')}</Button>
        </form>
    );
  };

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
