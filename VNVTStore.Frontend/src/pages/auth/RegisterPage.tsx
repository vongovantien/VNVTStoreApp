import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Mail, Lock, Eye, EyeOff, User, Phone, ArrowRight, Check } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { useToast } from '@/store';
import { authService } from '@/services';
import { AuthLayout } from '@/layouts/AuthLayout';
import { validationRules } from '@/utils/validation';

export const RegisterPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const toast = useToast();
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({
    username: '',
    name: '',
    email: '',
    phone: '',
    password: '',
    confirmPassword: '',
    agreeTerms: false,
  });

  /* Validation Rules from utility */
  const [errors, setErrors] = useState<Record<string, string>>({
      email: '',
      phone: '',
      password: '',
      confirmPassword: ''
  });

  const validate = () => {
    let isValid = true;
    const newErrors = { email: '', phone: '', password: '', confirmPassword: '' };

    if (!validationRules.email(formData.email)) {
        newErrors.email = t('validation.invalidEmail') || 'Email không hợp lệ';
        isValid = false;
    }

    if (!validationRules.phone(formData.phone)) {
        newErrors.phone = t('validation.invalidPhone') || 'Số điện thoại không hợp lệ (VD: 0901234567)';
        isValid = false;
    }

    if (!validationRules.password(formData.password)) {
        newErrors.password = t('validation.weakPassword') || 'Mật khẩu phải có ít nhất 6 ký tự, bao gồm chữ hoa, chữ thường và số';
        isValid = false;
    }

    if (formData.password !== formData.confirmPassword) {
        newErrors.confirmPassword = t('register.passwordMismatch') || 'Mật khẩu xác nhận không khớp';
        isValid = false;
    }

    setErrors(newErrors);
    return isValid;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validate()) {
        return;
    }

    setIsLoading(true);
    try {
      // Call backend API
      const response = await authService.register({
        username: formData.username || formData.email.split('@')[0], // Use email prefix as username if not provided
        email: formData.email,
        password: formData.password,
        fullName: formData.name,
      });

      if (response.success && response.data) {
        toast.success(t('messages.registerSuccess') || 'Đăng ký thành công! Vui lòng đăng nhập.');
        // Redirect to login page instead of auto-login
        // (auto-login without token causes 401 on cart fetch)
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
      <form onSubmit={handleSubmit} className="space-y-4" noValidate>
        <Input
          label={t('register.name')}
          placeholder="Nguyễn Văn A"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          leftIcon={<User size={18} />}
          required
        />

        <Input
          label="Email"
          type="email"
          placeholder="email@example.com"
          value={formData.email}
          onChange={(e) => {
             setFormData({ ...formData, email: e.target.value });
             if (errors.email) setErrors({...errors, email: ''});
          }}
          leftIcon={<Mail size={18} />}
          required
          error={errors.email}
        />

        <Input
          label={t('register.phone')}
          placeholder="0901234567"
          value={formData.phone}
          onChange={(e) => {
              setFormData({ ...formData, phone: e.target.value });
              if (errors.phone) setErrors({...errors, phone: ''});
          }}
          leftIcon={<Phone size={18} />}
          required
          error={errors.phone}
        />

        <Input
          label={t('register.password')}
          type={showPassword ? 'text' : 'password'}
          placeholder="••••••••"
          value={formData.password}
          onChange={(e) => {
              setFormData({ ...formData, password: e.target.value });
              if (errors.password) setErrors({...errors, password: ''});
          }}
          leftIcon={<Lock size={18} />}
          rightIcon={
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="text-tertiary hover:text-primary focus:outline-none"
            >
              {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
            </button>
          }
          required
          error={errors.password}
        />

        <Input
          label={t('register.confirmPassword')}
          type={showConfirmPassword ? 'text' : 'password'}
          placeholder="••••••••"
          value={formData.confirmPassword}
          onChange={(e) => {
              setFormData({ ...formData, confirmPassword: e.target.value });
               if (errors.confirmPassword) setErrors({...errors, confirmPassword: ''});
          }}
          leftIcon={<Lock size={18} />}
          rightIcon={
            <button
              type="button"
              onClick={() => setShowConfirmPassword(!showConfirmPassword)}
              className="text-tertiary hover:text-primary focus:outline-none"
            >
              {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
            </button>
          }
          required
          error={errors.confirmPassword}
        />

        <label className="flex items-start gap-3 cursor-pointer">
          <input
            type="checkbox"
            checked={formData.agreeTerms}
            onChange={(e) => setFormData({ ...formData, agreeTerms: e.target.checked })}
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
