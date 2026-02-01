import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Input, Switch } from '@/components/ui';
import { userService } from '@/services/userService';
import { useToast } from '@/store';
import { useForm } from 'react-hook-form';
import { Eye, EyeOff, Shield, Lock, Save, Smartphone, Bell } from 'lucide-react';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { createSchemas } from '@/utils/schemas';

const ProfileContent = () => {
  const { t } = useTranslation();
  const { userBaseSchema, changePasswordSchema } = createSchemas(t);
  
  type ProfileFormData = z.infer<typeof userBaseSchema>;
  type PasswordFormData = z.infer<typeof changePasswordSchema>;
  
  const toast = useToast();
  const [loading, setLoading] = useState(false);
  const [passwordLoading, setPasswordLoading] = useState(false);
  const [receiveEmailNotifications, setReceiveEmailNotifications] = useState(false);
  const [showCurrentPassword, setShowCurrentPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  
  const profileForm = useForm<ProfileFormData>({
    resolver: zodResolver(userBaseSchema),
    defaultValues: {
      fullName: '',
      email: '',
      phone: '',
    }
  });

  const passwordForm = useForm<PasswordFormData>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    }
  });
  
  useEffect(() => {
    const fetchProfile = async () => {
        try {
            const res = await userService.getProfile();
            if(res.success && res.data) {
                profileForm.reset({
                  fullName: res.data.fullName || '',
                  email: res.data.email || '',
                  phone: res.data.phone || '',
                });
            }
        } catch (error) {
            console.error('Error fetching profile:', error);
        }
    };
    fetchProfile();
  }, [profileForm]);

  const handleSave = async (data: ProfileFormData) => {
    setLoading(true);
    try {
      const res = await userService.updateProfile({
          fullName: data.fullName,
          phone: data.phone || '',
          email: data.email || ''
      });
      if (res && res.success) {
        toast.success(t('common.messages.saveSuccess'));
      } else {
        toast.error((res && res.message) || t('common.messages.saveError'));
      }
    } catch (e) {
      console.error(e);
      toast.error(t('common.messages.saveFailed'));
    } finally {
      setLoading(false);
    }
  };
  
  const onChangePassword = async (data: PasswordFormData) => {
    console.log('[DEBUG] Form submitted with:', { ...data, currentPassword: '***', newPassword: '***' });
    setPasswordLoading(true);
    try {
      console.log('[DEBUG] Calling userService.changePassword...');
      const res = await userService.changePassword({
        currentPassword: data.currentPassword,
        newPassword: data.newPassword,
        confirmNewPassword: data.confirmPassword
      });
      console.log('[DEBUG] API Response:', res);
      
      if (res && res.success) {
        toast.success(t('common.messages.updateSuccess'));
        passwordForm.reset();
      } else {
        toast.error((res && res.message) || t('common.messages.updateError'));
      }
    } catch (e) {
      console.error('Change password error:', e);
      toast.error(t('common.errorOccurred'));
    } finally {
      setPasswordLoading(false);
    }
  };
   
  return (
    <div className="space-y-6">
      {/* Profile Information */}
      <div className="bg-primary rounded-xl p-6 border shadow-sm">
        <h2 className="text-xl font-bold mb-6">{t('common.account.profile')}</h2>
        
        <form onSubmit={profileForm.handleSubmit(handleSave)} className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Input
            label={t('common.fields.name')}
            placeholder={t('common.placeholders.fullName')}
            {...profileForm.register('fullName')}
            error={profileForm.formState.errors.fullName?.message}
          />
          <Input
            label={t('common.fields.email')}
            type="email"
            {...profileForm.register('email')}
            readOnly
            disabled
            placeholder={t('common.placeholders.email')}
          />
          <Input
            label={t('common.fields.phone')}
            type="tel"
            placeholder={t('common.placeholders.phone')}
            {...profileForm.register('phone')}
            error={profileForm.formState.errors.phone?.message}
          />
          <div className="md:col-span-2 mt-4">
            <Button type="submit" isLoading={loading} leftIcon={<Save size={18} />}>
              {t('common.save')}
            </Button>
          </div>
        </form>
      </div>
      
      {/* Security Section (Change Password & 2FA) */}
      <div className="bg-primary rounded-xl overflow-hidden border shadow-sm">
        <div className="p-6 border-b flex items-center gap-3">
          <Shield className="text-indigo-600" size={24} />
          <h2 className="text-xl font-bold">{t('common.account.security')}</h2>
        </div>
        
        <form onSubmit={passwordForm.handleSubmit(onChangePassword)} className="p-6 space-y-8">
          {/* Change Password */}
          <div>
             <h3 className="text-lg font-semibold mb-6 flex items-center gap-2">
                <Lock size={18} className="text-tertiary" />
                {t('common.account.changePassword')}
             </h3>
             <div className="space-y-4 max-w-2xl">
                <Input
                  label={t('common.fields.currentPassword')}
                  type={showCurrentPassword ? 'text' : 'password'}
                  placeholder={t('common.placeholders.currentPassword')}
                  {...passwordForm.register('currentPassword')}
                  error={passwordForm.formState.errors.currentPassword?.message}
                  rightIcon={
                    <button 
                        type="button" 
                        onClick={() => setShowCurrentPassword(!showCurrentPassword)}
                        className="flex items-center justify-center p-1 text-slate-400 opacity-50 hover:opacity-100 hover:text-primary focus:outline-none transition-all"
                        tabIndex={-1}
                    >
                        {showCurrentPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                    </button>
                  }
                  isRequired
                />
                <Input
                  label={t('common.fields.newPassword')}
                  type={showNewPassword ? 'text' : 'password'}
                  placeholder={t('common.placeholders.newPassword')}
                  {...passwordForm.register('newPassword')}
                  error={passwordForm.formState.errors.newPassword?.message}
                  rightIcon={
                    <button 
                        type="button" 
                        onClick={() => setShowNewPassword(!showNewPassword)}
                        className="flex items-center justify-center p-1 text-slate-400 opacity-50 hover:opacity-100 hover:text-primary focus:outline-none transition-all"
                        tabIndex={-1}
                    >
                        {showNewPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                    </button>
                  }
                  isRequired
                />
                <Input
                  label={t('common.fields.confirmPassword')}
                  type={showConfirmPassword ? 'text' : 'password'}
                  placeholder={t('common.placeholders.confirmPassword')}
                  {...passwordForm.register('confirmPassword')}
                  error={passwordForm.formState.errors.confirmPassword?.message}
                  rightIcon={
                    <button 
                        type="button" 
                        onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                        className="flex items-center justify-center p-1 text-slate-400 opacity-50 hover:opacity-100 hover:text-primary focus:outline-none transition-all"
                        tabIndex={-1}
                    >
                        {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                    </button>
                  }
                  isRequired
                />
             </div>
          </div>
 
          <hr className="border-slate-100 dark:border-slate-800" />
 
          {/* 2FA Placeholder */}
          <div>
             <div className="flex items-center justify-between mb-2">
                <h3 className="text-lg font-semibold flex items-center gap-2">
                   {t('common.account.twoFactor')}
                </h3>
                <span className="text-xs bg-slate-100 dark:bg-slate-800 text-slate-500 px-2 py-1 rounded">{t('common.account.notActivated')}</span>
             </div>
             <p className="text-sm text-secondary mb-4">{t('common.account.twoFactorDesc')}</p>
             <button type="button" className="text-primary text-sm font-medium hover:underline flex items-center gap-2">
                <Smartphone size={16} /> {t('common.account.setupTwoFactor')}
             </button>
          </div>
 
          <div className="pt-4">
             <Button 
                type="submit"
                isLoading={passwordLoading}
                leftIcon={<Save size={18} />}
             >
                {t('common.save')}
             </Button>
          </div>
        </form>
      </div>
      
      {/* Email Notifications */}
      <div className="bg-primary rounded-xl p-6 border shadow-sm max-w-2xl">
        <h2 className="text-xl font-bold mb-6 flex items-center gap-2">
            <Bell className="text-indigo-600" size={24} />
            {t('common.account.notificationSettings')}
        </h2>
        
        <Switch
            label={t('common.account.emailNotifications')}
            description={t('common.account.emailNotificationsDesc')}
            checked={receiveEmailNotifications}
            onChange={setReceiveEmailNotifications}
            className="p-4 bg-slate-50 dark:bg-slate-800/40 rounded-xl"
        />
      </div>
    </div>
  );
};

export default ProfileContent;
