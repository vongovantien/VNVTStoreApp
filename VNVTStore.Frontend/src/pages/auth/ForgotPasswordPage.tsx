import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation, Trans } from 'react-i18next';
import { Mail, ArrowLeft, Loader2, CheckCircle } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { authService } from '@/services/authService';
import { useToast } from '@/store';

export const ForgotPasswordPage = () => {
    const { t } = useTranslation();
    const { success, error: toastError } = useToast();
    const [email, setEmail] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [isSubmitted, setIsSubmitted] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!email) return;

        setIsLoading(true);
        try {
            const response = await authService.forgotPassword(email);
            if (response.success) {
                setIsSubmitted(true);
                success(t('messages.forgotPasswordSuccess') || 'If an account exists with this email, a reset link has been sent.');
            } else {
                toastError(response.message || t('messages.errorOccurred'));
            }
        } catch (err: any) {
            toastError(err.response?.data?.message || t('messages.errorOccurred'));
        } finally {
            setIsLoading(false);
        }
    };

    if (isSubmitted) {
        return (
            <div className="min-h-[60vh] flex items-center justify-center px-4">
                <div className="max-w-md w-full bg-primary p-8 rounded-2xl shadow-xl border border-slate-100 dark:border-slate-800 text-center space-y-6">
                    <CheckCircle className="w-16 h-16 text-emerald-500 mx-auto" />
                    <h2 className="text-2xl font-bold">{t('auth.checkEmailTitle')}</h2>
                    <p className="text-secondary">
                        <Trans i18nKey="auth.checkEmailMessage" values={{ email }} components={{ 1: <span className="font-semibold text-primary" /> }} />
                    </p>
                    <Link to="/login" className="block w-full py-2.5 border-2 border-indigo-500 text-indigo-600 font-semibold rounded-lg hover:bg-indigo-600 hover:text-white transition-colors text-center mt-4">
                        {t('auth.backToLogin')}
                    </Link>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-[60vh] flex items-center justify-center px-4">
            <div className="max-w-md w-full bg-primary p-8 rounded-2xl shadow-xl border border-slate-100 dark:border-slate-800">
                <div className="text-center mb-8">
                    <h1 className="text-3xl font-bold mb-2">{t('auth.forgotPasswordTitle')}</h1>
                    <p className="text-secondary">{t('auth.forgotPasswordSubtitle')}</p>
                </div>

                <form onSubmit={handleSubmit} className="space-y-6">
                    <Input
                        label={t('fields.email')}
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        placeholder="yourname@example.com"
                        leftIcon={<Mail size={18} />}
                        required
                        autoFocus
                    />

                    <Button type="submit" fullWidth isLoading={isLoading} disabled={!email}>
                        {t('auth.sendResetLink')}
                    </Button>
                </form>

                <div className="mt-8 pt-6 border-t text-center">
                    <Link 
                        to="/login" 
                        className="inline-flex items-center gap-2 text-sm text-secondary hover:text-primary transition-colors font-medium"
                    >
                        <ArrowLeft size={16} />
                        {t('auth.backToLogin')}
                    </Link>
                </div>
            </div>
        </div>
    );
};

export default ForgotPasswordPage;
