import { useState, useEffect } from 'react';
import { Switch, Button } from '@/components/ui';
import { Bell, Moon, Sun, Globe, Smartphone, Mail } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useToast } from '@/store';

export const NotificationsContent = () => {
    const { t } = useTranslation();
    return (
        <div className="bg-primary rounded-xl p-6 border shadow-sm h-full min-h-[400px]">
            <h2 className="text-xl font-bold mb-6 flex items-center gap-2">
                <Bell className="text-primary" size={24} />
                {t('account.notifications') || 'Thông báo của tôi'}
            </h2>
            
            <div className="flex flex-col items-center justify-center py-16 text-center">
                <div className="w-20 h-20 bg-secondary rounded-full flex items-center justify-center mb-4">
                    <Bell className="text-tertiary" size={40} />
                </div>
                <h3 className="text-lg font-semibold text-primary">{t('common.noNotifications') || 'Chưa có thông báo nào'}</h3>
                <p className="text-secondary mt-1">{t('common.messages.checkBackLater') || 'Chúng tôi sẽ gửi thông báo đến bạn khi có cập nhật mới.'}</p>
            </div>
        </div>
    );
};

export const SettingsContent = () => {
    const { t, i18n } = useTranslation();
    const toast = useToast();
    const [loading, setLoading] = useState(false);
    
    // Preferences State
    const [settings, setSettings] = useState({
        emailNotif: true,
        orderNotif: true,
        promoNotif: true,
        darkMode: false,
        language: 'vi'
    });

    // Load from LocalStorage on mount
    useEffect(() => {
        const saved = localStorage.getItem('user_settings');
        if (saved) {
            try {
                setSettings({ ...settings, ...JSON.parse(saved) });
            } catch (e) { console.error('Failed to parse settings', e); }
        }
    }, []);

    const handleSave = () => {
        setLoading(true);
        // Simulate API call
        setTimeout(() => {
            localStorage.setItem('user_settings', JSON.stringify(settings));
            // Apply side effects
            if (settings.language !== i18n.language) {
                i18n.changeLanguage(settings.language);
            }
            // Dark mode toggle would go here if implemented globally
            
            toast.success('Cập nhật cài đặt thành công');
            setLoading(false);
        }, 800);
    };

    return (
        <div className="space-y-6">
            <div className="bg-primary rounded-xl p-6 border shadow-sm">
                <h2 className="text-xl font-bold mb-6">{t('account.settings') || 'Cài đặt tài khoản'}</h2>
                
                <div className="space-y-8">
                    {/* Notifications Group */}
                    <div>
                        <h3 className="font-semibold mb-4 text-primary flex items-center gap-2">
                            <Bell size={18} /> {t('settings.notifications') || 'Thông báo'}
                        </h3>
                        <div className="space-y-4 bg-secondary/50 p-4 rounded-xl border border-secondary/20">
                            <Switch
                                label="Thông báo đơn hàng"
                                description="Nhận cập nhật về trạng thái đơn hàng của bạn"
                                checked={settings.orderNotif}
                                onChange={(v) => setSettings(s => ({ ...s, orderNotif: v }))}
                            />
                            <div className="h-px bg-secondary/10" />
                            <Switch
                                label="Email khuyến mãi"
                                description="Nhận tin tức về ưu đãi và sản phẩm mới"
                                checked={settings.emailNotif}
                                onChange={(v) => setSettings(s => ({ ...s, emailNotif: v }))}
                            />
                        </div>
                    </div>

                    {/* App Preferences */}
                    <div>
                        <h3 className="font-semibold mb-4 text-primary flex items-center gap-2">
                            <Globe size={18} /> {t('settings.preferences') || 'Tùy chọn ứng dụng'}
                        </h3>
                        <div className="space-y-4 bg-secondary/50 p-4 rounded-xl border border-secondary/20">
                            <div className="flex items-center justify-between">
                                <div>
                                    <p className="font-medium">Ngôn ngữ</p>
                                    <p className="text-xs text-secondary">Ngôn ngữ hiển thị của ứng dụng</p>
                                </div>
                                <div className="flex gap-2">
                                    <button 
                                        onClick={() => setSettings(s => ({ ...s, language: 'vi' }))}
                                        className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${settings.language === 'vi' ? 'bg-primary text-white shadow-sm' : 'bg-secondary text-secondary hover:bg-secondary/80'}`}
                                    >
                                        Tiếng Việt
                                    </button>
                                    <button 
                                        onClick={() => setSettings(s => ({ ...s, language: 'en' }))}
                                        className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${settings.language === 'en' ? 'bg-primary text-white shadow-sm' : 'bg-secondary text-secondary hover:bg-secondary/80'}`}
                                    >
                                        English
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="flex justify-end pt-4">
                        <Button onClick={handleSave} isLoading={loading}>
                            {t('common.saveChanges') || 'Lưu thay đổi'}
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    );
};
