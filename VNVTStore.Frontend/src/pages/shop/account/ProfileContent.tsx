import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui';
import { userService, type UserProfileDto } from '@/services/userService';
import { useToast } from '@/store';

const ProfileContent = () => {
  const { t } = useTranslation();
  const toast = useToast();
  const [loading, setLoading] = useState(false);
  const [passwordLoading, setPasswordLoading] = useState(false);
  const [profile, setProfile] = useState<Partial<UserProfileDto>>({});
  
  // Password state
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });
  
  // Email notification toggle
  const [receiveEmailNotifications, setReceiveEmailNotifications] = useState(false);
  
  useEffect(() => {
    userService.getProfile().then(res => {
      if(res.success && res.data) setProfile(res.data);
    });
  }, []);

  const handleSave = async () => {
     if (!profile.fullName) {
       toast.error(t('validation.required') || 'Vui lòng nhập họ tên');
       return;
     }
     
     setLoading(true);
     try {
       const res = await userService.updateProfile({
           fullName: profile.fullName,
           phone: profile.phone || '',
           email: profile.email || ''
       });
       if (res.success) {
         toast.success(t('messages.saveSuccess') || 'Lưu thành công!');
       } else {
         toast.error(res.message || t('messages.saveError') || 'Không thể lưu');
       }
     } catch (e) {
       console.error(e);
       toast.error(t('messages.saveError') || 'Không thể lưu. Vui lòng thử lại.');
     } finally {
       setLoading(false);
     }
  };
  
  const handleChangePassword = async () => {
    // Validation
    if (!passwordForm.currentPassword || !passwordForm.newPassword || !passwordForm.confirmPassword) {
      toast.error('Vui lòng điền đầy đủ thông tin mật khẩu');
      return;
    }
    
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      toast.error(t('register.passwordMismatch') || 'Mật khẩu xác nhận không khớp');
      return;
    }
    
    if (passwordForm.newPassword.length < 6) {
      toast.error('Mật khẩu mới phải có ít nhất 6 ký tự');
      return;
    }
    
    setPasswordLoading(true);
    try {
      const res = await userService.changePassword({
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword,
        confirmNewPassword: passwordForm.confirmPassword
      });
      if (res.success) {
        toast.success('Đổi mật khẩu thành công!');
        setPasswordForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
      } else {
        toast.error(res.message || 'Đổi mật khẩu thất bại');
      }
    } catch (e) {
      console.error(e);
      toast.error('Có lỗi xảy ra khi đổi mật khẩu');
    } finally {
      setPasswordLoading(false);
    }
  };
   
  return (
    <div className="space-y-6">
      {/* Profile Information */}
      <div className="bg-primary rounded-xl p-6">
        <h2 className="text-xl font-bold mb-6">{t('account.profile')}</h2>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm font-medium mb-2">Họ và tên</label>
            <input
              type="text"
              value={profile.fullName || ''}
              onChange={(e) => setProfile({...profile, fullName: e.target.value})}
              className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:border-indigo-500"
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
              value={profile.phone || ''}
              onChange={(e) => setProfile({...profile, phone: e.target.value})}
              className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:border-indigo-500"
            />
          </div>
        </div>

        <div className="mt-6">
          <Button onClick={handleSave} isLoading={loading}>{t('common.save')}</Button>
        </div>
      </div>
      
      {/* Change Password */}
      <div className="bg-primary rounded-xl p-6">
        <h2 className="text-xl font-bold mb-6">Đổi mật khẩu</h2>
        
        <div className="space-y-4 max-w-md">
          <div>
            <label className="block text-sm font-medium mb-2">
              Mật khẩu cũ <span className="text-red-500">*</span>
            </label>
            <input
              type="password"
              value={passwordForm.currentPassword}
              onChange={(e) => setPasswordForm({...passwordForm, currentPassword: e.target.value})}
              className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:border-indigo-500"
              placeholder="Nhập mật khẩu hiện tại"
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-2">
              Mật khẩu mới <span className="text-red-500">*</span>
            </label>
            <input
              type="password"
              value={passwordForm.newPassword}
              onChange={(e) => setPasswordForm({...passwordForm, newPassword: e.target.value})}
              className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:border-indigo-500"
              placeholder="Nhập mật khẩu mới"
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-2">
              Nhập lại mật khẩu <span className="text-red-500">*</span>
            </label>
            <input
              type="password"
              value={passwordForm.confirmPassword}
              onChange={(e) => setPasswordForm({...passwordForm, confirmPassword: e.target.value})}
              className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:border-indigo-500"
              placeholder="Xác nhận mật khẩu mới"
            />
          </div>
        </div>

        <div className="mt-6">
          <Button onClick={handleChangePassword} isLoading={passwordLoading}>
            Đổi mật khẩu
          </Button>
        </div>
      </div>
      
      {/* Email Notifications */}
      <div className="bg-primary rounded-xl p-6">
        <h2 className="text-xl font-bold mb-6">Cài đặt thông báo</h2>
        
        <label className="flex items-center gap-3 cursor-pointer">
          <div 
            className={`relative w-12 h-6 rounded-full transition-colors ${
              receiveEmailNotifications ? 'bg-indigo-600' : 'bg-gray-300'
            }`}
            onClick={() => setReceiveEmailNotifications(!receiveEmailNotifications)}
          >
            <div 
              className={`absolute w-5 h-5 rounded-full bg-white shadow top-0.5 transition-transform ${
                receiveEmailNotifications ? 'translate-x-6' : 'translate-x-0.5'
              }`}
            />
          </div>
          <span className="text-indigo-600 font-medium">Nhận thông báo qua email</span>
        </label>
      </div>
    </div>
  );
};

export default ProfileContent;

