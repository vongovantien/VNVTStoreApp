import { useTranslation } from 'react-i18next';
import { Shield, Key, Smartphone } from 'lucide-react';
import { Button } from '@/components/ui';

interface AccountSecurityProps {
  onPasswordChange: () => void;
  onTwoFactorSetup: () => void;
}

const AccountSecurity = ({ onPasswordChange, onTwoFactorSetup }: AccountSecurityProps) => {
  const { t } = useTranslation();

  return (
     <section className="bg-primary rounded-xl p-6 border shadow-sm border-secondary/10">
      <h2 className="text-xl font-bold mb-6 flex items-center gap-2 text-primary">
        <Shield className="text-accent" size={24} />
        {t('common.account.security')}
      </h2>

      <div className="space-y-6">
        {/* Password */}
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 p-4 rounded-lg bg-secondary/30 border border-secondary/10">
          <div className="flex gap-4">
            <div className="bg-primary p-3 rounded-full shadow-sm text-accent">
              <Key size={20} />
            </div>
            <div>
              <h3 className="font-semibold text-primary">{t('common.account.changePassword')}</h3>
              <p className="text-sm text-secondary">{t('common.hints.leaveBlankKeepCurrent') || 'Cập nhật mật khẩu để bảo vệ tài khoản tốt hơn'}</p>
            </div>
          </div>
          <Button variant="outline" onClick={onPasswordChange}>
            {t('common.account.changePassword')}
          </Button>
        </div>

        {/* 2FA */}
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 p-4 rounded-lg bg-secondary/30 border border-secondary/10">
          <div className="flex gap-4">
            <div className="bg-primary p-3 rounded-full shadow-sm text-accent">
              <Smartphone size={20} />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h3 className="font-semibold text-primary">{t('common.account.twoFactor')}</h3>
                <span className="px-2 py-0.5 text-[10px] font-bold bg-secondary/50 text-secondary rounded-full uppercase">
                   {t('common.account.notActivated')}
                </span>
              </div>
              <p className="text-sm text-secondary">{t('common.account.twoFactorDesc')}</p>
            </div>
          </div>
          <Button variant="ghost" onClick={onTwoFactorSetup}>
            {t('common.account.setupTwoFactor')}
          </Button>
        </div>
      </div>
    </section>
  );
};

export default AccountSecurity;
