import { useTranslation } from 'react-i18next';
import { Bell } from 'lucide-react';
import { Switch } from '@/components/ui';

const NotificationSettings = () => {
  const { t } = useTranslation();

  return (
    <section className="bg-primary rounded-2xl p-8 border border-secondary/10 shadow-md animate-fade-in delay-150">
      <div className="flex items-center justify-between mb-8 pb-4 border-b border-secondary/5">
        <div className="flex items-center gap-3">
          <div className="p-2 bg-accent/10 rounded-lg text-accent">
            <Bell size={22} />
          </div>
          <div>
            <h2 className="text-2xl font-bold text-primary tracking-tight">{t('common.account.notificationSettings')}</h2>
            <p className="text-sm text-tertiary mt-1">{t('common.account.notificationSubtitle') || 'Quản lý cách chúng tôi liên lạc với bạn'}</p>
          </div>
        </div>
      </div>

      <div className="flex items-center justify-between p-6 rounded-2xl bg-bg-secondary border border-secondary/5 group hover:border-accent/10 transition-all duration-300">
        <div className="flex gap-4 items-center">
            <div className="bg-primary p-3 rounded-xl shadow-sm border border-secondary/5 text-secondary group-hover:text-accent transition-colors">
                 <Bell size={20} />
            </div>
            <div>
              <h3 className="font-bold text-primary">{t('common.account.emailNotifications')}</h3>
              <p className="text-sm text-secondary mt-0.5">{t('common.account.emailNotificationsDesc')}</p>
            </div>
        </div>
        <div className="flex items-center space-x-2">
           <Switch checked={true} className="data-[state=checked]:bg-accent shadow-sm" />
        </div>
      </div>
    </section>
  );
};

export default NotificationSettings;
