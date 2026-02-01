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
                {t('account.myNotifications')}
            </h2>
            
            <div className="flex flex-col items-center justify-center py-16 text-center">
                <div className="w-20 h-20 bg-secondary rounded-full flex items-center justify-center mb-4">
                    <Bell className="text-tertiary" size={40} />
                </div>
                <h3 className="text-lg font-semibold text-primary">{t('account.noNotifications')}</h3>
                <p className="text-secondary mt-1">{t('account.checkBackLater')}</p>
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
            
            toast.success(t('settings.updateSuccess'));
            setLoading(false);
        }, 800);
    };

    return (
        <div className="space-y-6">
            <div className="bg-primary rounded-xl p-6 border shadow-sm">
                <h2 className="text-xl font-bold mb-6">{t('account.settings')}</h2>
                
                <div className="space-y-8">
                    {/* Notifications Group */}
                    <div>
                        <h3 className="font-semibold mb-4 text-primary flex items-center gap-2">
                            <Bell size={18} /> {t('settings.notifications')}
                        </h3>
                        <div className="space-y-4 bg-secondary/50 p-4 rounded-xl border border-secondary/20">
                            <Switch
                                label={t('settings.orderNotif')}
                                description={t('settings.orderNotifDesc')}
                                checked={settings.orderNotif}
                                onChange={(v) => setSettings(s => ({ ...s, orderNotif: v }))}
                            />
                            <div className="h-px bg-secondary/10" />
                            <Switch
                                label={t('settings.promoNotif')}
                                description={t('settings.promoNotifDesc')}
                                checked={settings.emailNotif}
                                onChange={(v) => setSettings(s => ({ ...s, emailNotif: v }))}
                            />
                        </div>
                    </div>

                    {/* App Preferences */}
                    <div>
                        <h3 className="font-semibold mb-4 text-primary flex items-center gap-2">
                            <Globe size={18} /> {t('settings.preferences')}
                        </h3>
                        <div className="space-y-4 bg-secondary/50 p-4 rounded-xl border border-secondary/20">
                            <div className="flex items-center justify-between">
                                <div>
                                    <p className="font-medium">{t('settings.language')}</p>
                                    <p className="text-xs text-secondary">{t('settings.languageDesc')}</p>
                                </div>
                                <div className="flex gap-2">
                                    <button 
                                        onClick={() => setSettings(s => ({ ...s, language: 'vi' }))}
                                        className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${settings.language === 'vi' ? 'bg-primary text-white shadow-sm' : 'bg-secondary text-secondary hover:bg-secondary/80'}`}
                                    >
                                        {t('settings.vietnamese')}
                                    </button>
                                    <button 
                                        onClick={() => setSettings(s => ({ ...s, language: 'en' }))}
                                        className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${settings.language === 'en' ? 'bg-primary text-white shadow-sm' : 'bg-secondary text-secondary hover:bg-secondary/80'}`}
                                    >
                                        {t('settings.english')}
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="flex justify-end pt-4">
                        <Button onClick={handleSave} isLoading={loading}>
                            {t('settings.saveChanges')}
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    );
};

