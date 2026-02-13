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
     <section className="bg-primary rounded-2xl p-8 border border-secondary/10 shadow-md animate-fade-in delay-75">
      <div className="flex items-center justify-between mb-8 pb-4 border-b border-secondary/5">
        <div className="flex items-center gap-3">
          <div className="p-2 bg-accent/10 rounded-lg text-accent">
            <Shield size={22} />
          </div>
          <div>
            <h2 className="text-2xl font-bold text-primary tracking-tight">{t('common.account.security')}</h2>
            <p className="text-sm text-tertiary mt-1">{t('common.account.securitySubtitle') || 'Bảo mật tài khoản của bạn'}</p>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Password Card */}
        <div className="group relative p-6 rounded-2xl bg-bg-secondary border border-secondary/5 hover:border-accent/20 transition-all duration-300 hover:shadow-lg hover:shadow-accent/5 overflow-hidden">
          <div className="absolute top-0 right-0 p-3 opacity-5 group-hover:opacity-10 transition-opacity">
            <Key size={80} />
          </div>
          
          <div className="flex flex-col h-full space-y-4">
            <div className="flex items-center gap-3">
              <div className="p-2.5 bg-primary rounded-xl shadow-sm border border-secondary/5 text-accent">
                <Key size={20} />
              </div>
              <h3 className="font-bold text-primary text-lg">{t('common.account.changePassword')}</h3>
            </div>
            
            <p className="text-sm text-secondary flex-1 leading-relaxed">
              {t('common.hints.leaveBlankKeepCurrent') || 'Cập nhật mật khẩu định kỳ để nâng cao tính bảo mật cho tài khoản của bạn.'}
            </p>
            
            <Button 
              variant="outline" 
              onClick={onPasswordChange}
              fullWidth
              className="mt-4 border-secondary/20 hover:border-accent hover:text-accent group-hover:bg-accent/5 transition-all text-sm font-semibold"
            >
              {t('common.account.changePassword')}
            </Button>
          </div>
        </div>

        {/* 2FA Card */}
        <div className="group relative p-6 rounded-2xl bg-bg-secondary border border-secondary/5 hover:border-accent/20 transition-all duration-300 hover:shadow-lg hover:shadow-accent/5 overflow-hidden">
           <div className="absolute top-0 right-0 p-3 opacity-5 group-hover:opacity-10 transition-opacity">
            <Smartphone size={80} />
          </div>

          <div className="flex flex-col h-full space-y-4">
            <div className="flex items-center gap-3">
              <div className="p-2.5 bg-primary rounded-xl shadow-sm border border-secondary/5 text-accent">
                <Smartphone size={20} />
              </div>
              <div className="flex items-center gap-2">
                <h3 className="font-bold text-primary text-lg">{t('common.account.twoFactor')}</h3>
                <span className="px-2 py-0.5 text-[9px] font-extrabold bg-tertiary/20 text-tertiary border border-tertiary/10 rounded-full uppercase tracking-tighter">
                   {t('common.account.notActivated')}
                </span>
              </div>
            </div>
            
            <p className="text-sm text-secondary flex-1 leading-relaxed">
              {t('common.account.twoFactorDesc') || 'Thêm một lớp bảo mật bằng cách yêu cầu mã xác thực từ điện thoại của bạn.'}
            </p>
            
            <Button 
              variant="outline" 
              onClick={onTwoFactorSetup}
              fullWidth
              className="mt-4 border-secondary/20 hover:border-accent hover:text-accent group-hover:bg-accent/5 transition-all text-sm font-semibold"
            >
              {t('common.account.setupTwoFactor')}
            </Button>
          </div>
        </div>
      </div>
    </section>
  );
};

export default AccountSecurity;
