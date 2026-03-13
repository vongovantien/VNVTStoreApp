import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { userService } from '@/services/userService';
import { useToast, useAuthStore } from '@/store';
import { Loading } from '@/components/ui';
import { UserRole } from '@/types';
import { ProfileForm, AccountSecurity, NotificationSettings, LoyaltySummary } from './components';
import { type UpdateProfileData } from './components/ProfileForm';

const ProfileContent = () => {
    const { t } = useTranslation();
    const { error, success, info } = useToast();
    const { user, updateUser } = useAuthStore();
    const [loading, setLoading] = useState(false);
    const [pageLoading, setPageLoading] = useState(true);
    const [initialData, setInitialData] = useState({
        fullName: '',
        email: '',
        phone: '',
        avatar: ''
    });

    useEffect(() => {
        const fetchProfile = async () => {
            try {
                const res = await userService.getProfile();
                if(res.success && res.data) {
                    const avatarVal = res.data.avatar || (res.data as { Avatar?: string }).Avatar;
                    const data = {
                        fullName: res.data.fullName || '',
                        email: res.data.email || '',
                        phone: res.data.phone || '',
                        avatar: avatarVal || ''
                    };
                    setInitialData(data);
                    // Update auth store with normalized avatar
                    updateUser({
                        ...res.data,
                        role: res.data.role as UserRole,
                        ...(avatarVal ? { avatar: avatarVal } : {})
                    });
                }
            } catch (err: unknown) {
                console.error('Error fetching profile:', err);
                error((err as Error).message || t('common.messages.fetchError'));
            } finally {
                setPageLoading(false);
            }
        };

        fetchProfile();
    }, [t, error, updateUser]); 

    const handleSaveProfile = async (data: UpdateProfileData) => {
        setLoading(true);
        try {
            const res = await userService.updateProfile({
                fullName: data.fullName,
                phone: data.phone,
                email: data.email,
                ...(data.avatarUrl ? { avatarUrl: data.avatarUrl } : {})
            });

            if (res.success && res.data) {
                success(t('common.messages.updateSuccess'));
                
                // Normalize avatar field before updating store
                const avatarVal = res.data.avatar || (res.data as { Avatar?: string }).Avatar;
                updateUser({
                    ...res.data,
                    role: res.data.role as UserRole,
                    ...(avatarVal ? { avatar: avatarVal } : {})
                });

                setInitialData({
                    fullName: res.data.fullName || '',
                    email: res.data.email || '',
                    phone: res.data.phone || '',
                    avatar: avatarVal || ''
                });
            } else {
                error(res.message || t('common.messages.updateError'));
            }
        } catch (err: unknown) {
            console.error('Error saving profile:', err);
            error((err as Error).message || t('common.messages.updateError'));
        } finally {
            setLoading(false);
        }
    };

    const handlePasswordChange = () => {
        info(t('common.messages.featureComingSoon'));
    };

    const handleTwoFactorSetup = () => {
        info(t('common.messages.featureComingSoon'));
    };

    const handleDeleteAccount = async () => {
        if (!window.confirm(t('profile.deleteAccount.confirm', 'Bạn có chắc chắn muốn xóa tài khoản? Hành động này không thể hoàn tác.'))) return;
        
        try {
            const res = await userService.deleteAccount();
            if (res.success) {
                success(t('profile.deleteAccount.success', 'Tài khoản đã được vô hiệu hóa.'));
                // Logout user
                useAuthStore.getState().logout();
                window.location.href = '/';
            } else {
                error(res.message || t('profile.deleteAccount.error', 'Có lỗi xảy ra khi xóa tài khoản.'));
            }
        } catch (err) {
            console.error('Error deleting account:', err);
            error(t('profile.deleteAccount.error', 'Có lỗi xảy ra khi xóa tài khoản.'));
        }
    };

    if (pageLoading) return <div className="flex justify-center py-20"><Loading /></div>;

    return (
        <div className="space-y-8 animate-fade-in pb-10">
            {/* Loyalty & Credit Summary */}
            {user && user.role === 'Customer' && (
                <LoyaltySummary 
                    points={user.loyaltyPoints || 0}
                    debtLimit={user.debtLimit || 0}
                    currentDebt={user.currentDebt || 0}
                />
            )}

            {/* Main Profile Info */}
            <ProfileForm 
                initialData={initialData} 
                onSave={handleSaveProfile} 
                loading={loading} 
            />

            {/* Account Security */}
            <AccountSecurity 
                onPasswordChange={handlePasswordChange}
                onTwoFactorSetup={handleTwoFactorSetup}
            />

            {/* Notification Settings */}
            <NotificationSettings />

            {/* Danger Zone */}
            <div className="pt-8 border-t border-rose-100">
                <div className="bg-rose-50 rounded-2xl p-6 border border-rose-100">
                    <h3 className="text-lg font-bold text-rose-600 mb-2">
                        {t('profile.dangerZone.title', 'Vùng nguy hiểm')}
                    </h3>
                    <p className="text-sm text-rose-500 mb-4">
                        {t('profile.dangerZone.description', 'Xóa tài khoản của bạn sẽ vô hiệu hóa quyền truy cập. Hành động này có thể được khôi phục bởi quản trị viên.')}
                    </p>
                    <button
                        onClick={handleDeleteAccount}
                        className="px-6 py-2 bg-rose-600 text-white rounded-xl font-semibold hover:bg-rose-700 transition-colors shadow-lg shadow-rose-200"
                    >
                        {t('profile.dangerZone.deleteButton', 'Xóa tài khoản')}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default ProfileContent;
