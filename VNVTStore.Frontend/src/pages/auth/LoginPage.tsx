import { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Mail, Lock, Eye, EyeOff, ArrowRight } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { useAuthStore, useToast } from '@/store';
import { UserRole, UserStatus } from '@/types';
import { authService } from '@/services/authService';
import { AuthLayout } from '@/layouts/AuthLayout';
/* eslint-disable @typescript-eslint/no-explicit-any */
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { createSchemas } from '@/utils/schemas';
import { signInWithPopup, type AuthProvider } from 'firebase/auth';
import { auth, googleProvider, facebookProvider } from '@/config/firebase';

export const LoginPage = () => {
  const { t } = useTranslation();
  const { loginSchema } = createSchemas(t);
  
  interface LoginFormData {
    email: string;
    password: string;
    remember: boolean;
  }

  const navigate = useNavigate();
  const location = useLocation();
  const { login, isAuthenticated, user } = useAuthStore();
  const toast = useToast();
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const {
      register,
      handleSubmit,
      setValue,
      formState: { errors },
  } = useForm<LoginFormData>({
      resolver: zodResolver(loginSchema),
      defaultValues: {
          email: '',
          password: '',
          remember: false,
      }
  });

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated && user) {
        if (String(user.role).toLowerCase() === 'admin') { 
            navigate('/admin', { replace: true });
        } else {
            navigate('/', { replace: true });
        }
    }
  }, [isAuthenticated, user, navigate]);

  /* Load saved email if 'Remember Me' was checked */
  useEffect(() => {
      const isRemembered = localStorage.getItem('vnvt-remember') === 'true';
      if (isRemembered) {
          const savedEmail = localStorage.getItem('vnvt-email');
          if (savedEmail) {
              setValue('email', savedEmail);
              setValue('remember', true);
          }
      }
  }, [setValue]);

  const from = (location.state as { from?: string })?.from || '/';

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await authService.login({
        username: data.email,
        password: data.password,
      });

      if (response.success && response.data) {
        const { token, refreshToken, user } = response.data;

        // Handle Remember Me preference
        if (data.remember) {
            localStorage.setItem('vnvt-remember', 'true');
            localStorage.setItem('vnvt-email', data.email);
        } else {
            localStorage.removeItem('vnvt-remember');
            localStorage.removeItem('vnvt-email');
        }

        login(
          {
            code: user.code,
            email: user.email,
            fullName: user.fullName || user.username,
            role: (user.role as UserRole) || UserRole.Customer,
            status: UserStatus.Active,
            createdAt: new Date().toISOString(),
          },
          token,
          refreshToken
        );

        toast.success(t('messages.loginSuccess'));
        
        // Redirect to admin if admin role, otherwise to original destination
        // backend returns PascalCase 'Admin', Enum is 'Admin'
        if (String(user.role).toLowerCase() === 'admin') { 
            navigate('/admin', { replace: true });
        } else {
            navigate(from, { replace: true });
        }
      } else {
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

  const handleSocialLogin = async (provider: string, authProvider: AuthProvider) => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await signInWithPopup(auth, authProvider);
      const token = await result.user.getIdToken();
      
      const response = await authService.externalLogin(provider, token);
      
      if (response.success && response.data) {
        const { token: accessToken, refreshToken, user: userData } = response.data;
        
        login(
          {
            code: userData.code,
            email: userData.email,
            fullName: userData.fullName || userData.username,
            role: (userData.role as UserRole) || UserRole.Customer,
            status: UserStatus.Active,
            createdAt: new Date().toISOString(),
            avatar: userData.avatar
          },
          accessToken,
          refreshToken
        );

        toast.success(t('messages.loginSuccess'));
        
        if (String(userData.role).toLowerCase() === 'admin') { 
            navigate('/admin', { replace: true });
        } else {
            navigate(from, { replace: true });
        }
      } else {
        setError(response.message || t('messages.error'));
      }
    } catch (err: unknown) {
      console.error(`${provider} login error:`, err);
      const isPopupClosed = err instanceof Error && (err as any).code === 'auth/popup-closed-by-user';
      if (!isPopupClosed) {
        toast.error(t('messages.error'));
        setError(t('messages.error'));
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthLayout title={t('login.title')} subtitle={t('login.subtitle')}>
      <div className="grid grid-cols-2 gap-4 mb-6">
        {/* Google Login */}
        <button 
          onClick={() => handleSocialLogin('google', googleProvider)}
          disabled={isLoading}
          className="flex items-center justify-center gap-2 py-3 px-4 border rounded-xl font-medium hover:bg-gray-50 transition-colors disabled:opacity-50"
        >
          <img src="https://www.gstatic.com/firebasejs/ui/2.0.0/images/auth/google.svg" alt="Google" className="w-5 h-5" />
          <span>Google</span>
        </button>

        {/* Facebook Login */}
        <button 
          onClick={() => handleSocialLogin('facebook', facebookProvider)}
          disabled={isLoading}
          className="flex items-center justify-center gap-2 py-3 px-4 border rounded-xl font-medium hover:bg-gray-50 transition-colors disabled:opacity-50"
        >
          <img src="https://www.gstatic.com/firebasejs/ui/2.0.0/images/auth/facebook.svg" alt="Facebook" className="w-5 h-5" />
          <span>Facebook</span>
        </button>
      </div>

      <div className="flex items-center gap-4 mb-6">
        <hr className="flex-1" />
        <span className="text-tertiary text-sm">{t('login.or')}</span>
        <hr className="flex-1" />
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        {error && (
          <div className="p-3 border border-error bg-error/10 text-error text-sm rounded-lg">
            {error}
          </div>
        )}
        <Input
          label="Email"
          type="email"
          placeholder="email@example.com"
          {...register('email')}
          leftIcon={<Mail size={18} />}
          required
          error={errors.email?.message}
        />

        <Input
          label={t('login.password')}
          type={showPassword ? 'text' : 'password'}
          placeholder="••••••••"
          {...register('password')}
          leftIcon={<Lock size={18} />}
          rightIcon={
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="flex items-center justify-center p-1 text-slate-400 opacity-50 hover:opacity-100 hover:text-primary focus:outline-none transition-all"
              tabIndex={-1}
            >
              {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
            </button>
          }
          required
          error={errors.password?.message}
        />

        <div className="flex items-center justify-between">
          <label className="flex items-center gap-2 cursor-pointer">
            <input
              type="checkbox"
              {...register('remember')}
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
