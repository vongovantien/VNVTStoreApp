import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Mail, Lock, Eye, EyeOff, User, Phone, ArrowRight, Check } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { useToast } from '@/store';
import { authService } from '@/services';
import { AuthLayout } from '@/layouts/AuthLayout';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { createSchemas } from '@/utils/schemas';

export const RegisterPage = () => {
  const { t } = useTranslation();
  const { registerSchema } = createSchemas(t);
  
  type RegisterFormData = z.infer<typeof registerSchema>;
  const navigate = useNavigate();
  const toast = useToast();
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const {
      register,
      handleSubmit,
      formState: { errors },
  } = useForm<RegisterFormData>({
      resolver: zodResolver(registerSchema),
      defaultValues: {
          username: '',
          fullName: '',
          email: '',
          phone: '',
          password: '',
          confirmPassword: '',
          agreeTerms: false,
      } as RegisterFormData
  });

  const onSubmit = async (data: RegisterFormData) => {
    setIsLoading(true);
    try {
      // Call backend API
      const response = await authService.register({
        username: data.username || data.email.split('@')[0], // Use email prefix as username if not provided
        email: data.email,
        password: data.password,
        fullName: data.fullName,
      });

      if (response.success && response.data) {
        toast.success(t('messages.registerSuccess') || 'Đăng ký thành công! Vui lòng đăng nhập.');
        navigate('/login');
      } else {
        toast.error(response.message || 'Đăng ký thất bại');
      }
    } catch (error: unknown) {
      const msg = error instanceof Error ? error.message : 'Đăng ký thất bại. Vui lòng thử lại.';
      toast.error(msg);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthLayout title={t('register.title')} subtitle={t('register.subtitle')}>
      {/* Zalo Register */}
      <button className="w-full flex items-center justify-center gap-3 py-3 px-4 border-2 rounded-xl font-medium hover:bg-blue-50 transition-colors mb-6">
        <span className="text-blue-500 font-bold">Zalo</span>
        <span>{t('register.withZalo')}</span>
      </button>

      <div className="flex items-center gap-4 mb-6">
        <hr className="flex-1" />
        <span className="text-tertiary text-sm">{t('login.or')}</span>
        <hr className="flex-1" />
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <Input
          label={t('register.name')}
          placeholder="Nguyễn Văn A"
          {...register('fullName')}
          leftIcon={<User size={18} />}
          required
          error={errors.fullName?.message}
        />

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
          label={t('register.phone')}
          placeholder="0901234567"
          {...register('phone')}
          leftIcon={<Phone size={18} />}
          required
          error={errors.phone?.message}
        />

        <Input
          label={t('register.password')}
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

        <Input
          label={t('register.confirmPassword')}
          type={showConfirmPassword ? 'text' : 'password'}
          placeholder="••••••••"
          {...register('confirmPassword')}
          leftIcon={<Lock size={18} />}
          rightIcon={
            <button
              type="button"
              onClick={() => setShowConfirmPassword(!showConfirmPassword)}
              className="flex items-center justify-center p-1 text-slate-400 opacity-50 hover:opacity-100 hover:text-primary focus:outline-none transition-all"
              tabIndex={-1}
            >
              {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
            </button>
          }
          required
          error={errors.confirmPassword?.message}
        />

        <div className="space-y-1">
            <label className="flex items-start gap-3 cursor-pointer">
              <input
                type="checkbox"
                {...register('agreeTerms')}
                className="w-5 h-5 rounded mt-0.5"
                required
              />
              <span className="text-sm text-secondary">
                {t('register.agreeTerms')}{' '}
                <Link to="/terms" className="text-primary hover:underline">
                  {t('register.terms')}
                </Link>
              </span>
            </label>
            {errors.agreeTerms?.message && (
              <p className="text-error text-xs">{String(errors.agreeTerms.message)}</p>
            )}
        </div>

        <Button
          type="submit"
          fullWidth
          size="lg"
          isLoading={isLoading}
          rightIcon={<ArrowRight size={20} />}
        >
          {t('register.submit')}
        </Button>
      </form>

      {/* Benefits */}
      <div className="mt-6 p-4 bg-secondary rounded-lg">
        <p className="text-sm font-medium mb-2">{t('register.benefits')}</p>
        <ul className="space-y-1">
          {['Theo dõi đơn hàng dễ dàng', 'Ưu đãi dành riêng cho thành viên', 'Tích điểm đổi quà'].map((item) => (
            <li key={item} className="flex items-center gap-2 text-sm text-secondary">
              <Check size={14} className="text-success" />
              {item}
            </li>
          ))}
        </ul>
      </div>

      {/* Login Link */}
      <p className="text-center mt-6 text-secondary">
        {t('register.hasAccount')}{' '}
        <Link to="/login" className="text-primary font-semibold hover:underline">
          {t('register.login')}
        </Link>
      </p>
    </AuthLayout>
  );
};

export default RegisterPage;
