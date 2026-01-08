import { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Mail, Lock, Eye, EyeOff, ArrowRight, AlertCircle } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { useAuthStore } from '@/store';
import { authService } from '@/services/authService';

export const LoginPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const { login } = useAuthStore();
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
        const { token, user } = response.data;

        login(
          {
            id: user.code,
            email: user.email,
            name: user.fullName || user.username,
            role: (user.role as 'admin' | 'customer' | 'staff') || 'customer',
            createdAt: new Date().toISOString(),
          },
          token
        );

        // Redirect to admin if admin role, otherwise to original destination
        navigate(user.role === 'admin' ? '/admin' : from, { replace: true });
      } else {
        setError(response.message || 'Tên đăng nhập hoặc mật khẩu không đúng');
      }
    } catch (err) {
      setError('Có lỗi xảy ra. Vui lòng thử lại.');
      console.error('Login error:', err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-primary/5 via-secondary to-purple-500/5 flex items-center justify-center p-4">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="w-full max-w-md"
      >
        <div className="bg-primary rounded-2xl shadow-2xl p-8">
          {/* Logo */}
          <div className="text-center mb-8">
            <Link to="/" className="inline-flex items-center gap-2">
              <span className="text-4xl">🏠</span>
              <span className="text-2xl font-extrabold bg-gradient-to-r from-primary to-purple-500 bg-clip-text text-transparent">
                VNVT Store
              </span>
            </Link>
            <h1 className="text-2xl font-bold mt-4">{t('login.title')}</h1>
            <p className="text-secondary mt-2">{t('login.subtitle')}</p>
          </div>

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
              loading={isLoading}
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
        </div>

        {/* Back to Home */}
        <div className="text-center mt-6">
          <Link to="/" className="text-secondary hover:text-primary transition-colors">
            ← {t('common.backToHome')}
          </Link>
        </div>
      </motion.div>
    </div>
  );
};

export default LoginPage;
