import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui';
import { userService, type UserProfileDto } from '@/services/userService';

const ProfileContent = () => {
  const { t } = useTranslation();
  const [loading, setLoading] = useState(false);
  const [profile, setProfile] = useState<Partial<UserProfileDto>>({});
  
  useEffect(() => {
    userService.getProfile().then(res => {
      if(res.success && res.data) setProfile(res.data);
    });
  }, []);

  const handleSave = async () => {
     if (!profile.fullName || !profile.phoneNumber || !profile.email) return;
     
     setLoading(true);
     await userService.updateProfile({
         fullName: profile.fullName,
         phoneNumber: profile.phoneNumber,
         email: profile.email
     });
     setLoading(false);
  };
  
  return (
    <div className="bg-primary rounded-xl p-6">
      <h2 className="text-xl font-bold mb-6">{t('account.profile')}</h2>
      
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <label className="block text-sm font-medium mb-2">Họ và tên</label>
          <input
            type="text"
            value={profile.fullName || ''}
            onChange={(e) => setProfile({...profile, fullName: e.target.value})}
            className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:border-primary"
          />
        </div>
        <div>
          <label className="block text-sm font-medium mb-2">Email</label>
          <input
            type="email"
            value={profile.email || ''}
            readOnly
            className="w-full px-4 py-2 border rounded-lg bg-secondary/50"
          />
        </div>
        <div>
          <label className="block text-sm font-medium mb-2">Số điện thoại</label>
          <input
            type="tel"
            value={profile.phoneNumber || ''}
             onChange={(e) => setProfile({...profile, phoneNumber: e.target.value})}
            className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:border-primary"
          />
        </div>
      </div>

      <div className="mt-6">
        <Button onClick={handleSave} isLoading={loading}>{t('common.save')}</Button>
      </div>
    </div>
  );
};

export default ProfileContent;
