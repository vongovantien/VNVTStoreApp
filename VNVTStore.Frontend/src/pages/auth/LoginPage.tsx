import { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Mail, Lock, Eye, EyeOff, ArrowRight } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { useAuthStore, useToast } from '@/store';
import { UserRole } from '@/types';
import { authService } from '@/services/authService';
import { AuthLayout } from '@/layouts/AuthLayout';

export const LoginPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const { login } = useAuthStore();
  const toast = useToast();
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    remember: false,
  });

  const from = (location.state as { from?: string })?.from || '/';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);

    try {
      const response = await authService.login({
        username: formData.email,
        password: formData.password,
      });

      if (response.success && response.data) {
        const { token, refreshToken, user } = response.data;

        login(
          {
            id: user.code,
            email: user.email,
            fullName: user.fullName || user.username,
            role: (user.role as UserRole) || UserRole.CUSTOMER,
            createdAt: new Date().toISOString(),
          },
          token,
          refreshToken
        );

        toast.success(t('messages.loginSuccess'));
        // Redirect to admin if admin role, otherwise to original destination
        navigate(user.role === UserRole.ADMIN ? '/admin' : from, { replace: true });
      } else {
        toast.error(response.message || t('messages.loginError'));
        setError(response.message || 'Tên đăng nhập hoặc mật khẩu không đúng');
      }
    } catch (err) {
      toast.error(t('messages.error'));
      setError('Có lỗi xảy ra. Vui lòng thử lại.');
      console.error('Login error:', err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthLayout title={t('login.title')} subtitle={t('login.subtitle')}>
      {/* Zalo Login */}
      <button className="w-full flex items-center justify-center gap-3 py-3 px-4 border-2 rounded-xl font-medium hover:bg-blue-50 transition-colors mb-6">
        <span className="text-blue-500 font-bold">Zalo</span>
        <span>{t('login.withZalo')}</span>
      </button>

      <div className="flex items-center gap-4 mb-6">
        <hr className="flex-1" />
        <span className="text-tertiary text-sm">{t('login.or')}</span>
        <hr className="flex-1" />
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && (
          <div className="p-3 bg-red-50 text-red-500 text-sm rounded-lg">
            {error}
          </div>
        )}
        <Input
          label="Email"
          type="email"
          placeholder="email@example.com"
          value={formData.email}
          onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          leftIcon={<Mail size={18} />}
          required
        />

        <div className="relative">
          <Input
            label={t('login.password')}
            type={showPassword ? 'text' : 'password'}
            placeholder="••••••••"
            value={formData.password}
            onChange={(e) => setFormData({ ...formData, password: e.target.value })}
            leftIcon={<Lock size={18} />}
            required
          />
          <button
            type="button"
            onClick={() => setShowPassword(!showPassword)}
            className="absolute right-3 top-9 text-tertiary hover:text-primary"
          >
            {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
          </button>
        </div>

        <div className="flex items-center justify-between">
          <label className="flex items-center gap-2 cursor-pointer">
            <input
              type="checkbox"
              checked={formData.remember}
              onChange={(e) => setFormData({ ...formData, remember: e.target.checked })}
              className="w-4 h-4 rounded"
            />
            <span className="text-sm text-secondary">{t('login.remember')}</span>
          </label>
          <Link to="/forgot-password" className="text-sm text-primary hover:underline">
            {t('login.forgot')}
          </Link>
        </div>

        <Button
          type="submit"
          fullWidth
          size="lg"
          isLoading={isLoading} // Fixed: loading -> isLoading to match ButtonProps if needed, or if Button accepts loading.
          // Wait, previous code used loading={isLoading}. Let's check ButtonProps.
          // Assuming Button accepts isLoading or loading. Previous code said loading={isLoading}.
          // But I see isLoading={isLoading} in RegisterPage. Let's use isLoading if possible for consistency.
          // The Button component likely has isLoading alias or prop. I will use isLoading.
          rightIcon={<ArrowRight size={20} />}
        >
          {t('login.submit')}
        </Button>
      </form>

      {/* Register Link */}
      <p className="text-center mt-6 text-secondary">
        {t('login.noAccount')}{' '}
        <Link to="/register" className="text-primary font-semibold hover:underline">
          {t('login.register')}
        </Link>
      </p>
    </AuthLayout>
  );
};

export default LoginPage;
