import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { userService } from '@/services/userService';
import { useToast, useAuthStore } from '@/store';
import { Loading } from '@/components/ui';
import { ProfileForm, AccountSecurity, NotificationSettings } from './components';
import { type UpdateProfileData } from './components/ProfileForm';

const ProfileContent = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const { user, setUser } = useAuthStore();
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
                    const data = {
                        fullName: res.data.fullName || '',
                        email: res.data.email || '',
                        phone: res.data.phone || '',
                        avatar: res.data.avatar || ''
                    };
                    setInitialData(data);
                    // Update auth store user if needed
                    if (user) {
                        setUser({ ...user, ...res.data });
                    }
                }
            } catch (error) {
                console.error('Error fetching profile:', error);
                toast.error(t('common.messages.loadError'));
            } finally {
                setPageLoading(false);
            }
        };

        fetchProfile();
    }, [t, toast, setUser, user]); // Added user to deps

    const handleSaveProfile = async (data: UpdateProfileData) => {
        setLoading(true);
        try {
            const res = await userService.updateProfile({
                fullName: data.fullName,
                phone: data.phone,
                email: data.email,
                avatarUrl: data.avatarUrl
            });

            if (res.success && res.data) {
                toast.success(t('common.messages.updateSuccess'));
                if (user) {
                    setUser({ ...user, ...res.data });
                }
                setInitialData({
                    fullName: res.data.fullName || '',
                    email: res.data.email || '',
                    phone: res.data.phone || '',
                    avatar: res.data.avatar || ''
                });
            } else {
                toast.error(res.message || t('common.messages.updateError'));
            }
        } catch (error) {
            console.error('Error saving profile:', error);
            toast.error(t('common.messages.updateError'));
        } finally {
            setLoading(false);
        }
    };

    const handlePasswordChange = () => {
        toast.info(t('common.messages.featureComingSoon'));
    };

    const handleTwoFactorSetup = () => {
        toast.info(t('common.messages.featureComingSoon'));
    };

    if (pageLoading) return <div className="flex justify-center py-20"><Loading /></div>;

    return (
        <div className="space-y-8 animate-fade-in max-w-4xl mx-auto pb-10">
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
        </div>
    );
};

export default ProfileContent;
