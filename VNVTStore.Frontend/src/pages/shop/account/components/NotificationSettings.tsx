import { useTranslation } from 'react-i18next';
import { Bell } from 'lucide-react';
import { Switch } from '@/components/ui';

const NotificationSettings = () => {
  const { t } = useTranslation();

  return (
    <section className="bg-primary rounded-xl p-6 border shadow-sm border-secondary/10">
      <h2 className="text-xl font-bold mb-6 flex items-center gap-2 text-primary">
        <Bell className="text-accent" size={24} />
        {t('common.account.notificationSettings')}
      </h2>

      <div className="flex items-center justify-between p-4 rounded-lg bg-secondary/30 border border-secondary/10">
        <div>
          <h3 className="font-semibold text-primary">{t('common.account.emailNotifications')}</h3>
          <p className="text-sm text-secondary">{t('common.account.emailNotificationsDesc')}</p>
        </div>
        <div className="flex items-center space-x-2">
           <Switch checked={true} />
        </div>
      </div>
    </section>
  );
};

export default NotificationSettings;
