import { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { CheckCircle, XCircle, Loader2 } from 'lucide-react';
import { authService } from '@/services/authService';

export const VerifyEmailPage = () => {
    const { t } = useTranslation();
    const [searchParams] = useSearchParams();
    const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
    const [message, setMessage] = useState('');

    useEffect(() => {
        const email = searchParams.get('email');
        const token = searchParams.get('token');

        if (!email || !token) {
            // eslint-disable-next-line
            setStatus('error');
            setMessage(t('auth.verifyEmail.invalidLink'));
            return;
        }

        const verify = async () => {
            try {
                const response = await authService.verifyEmail(email, token);
                if (response.success) {
                    setStatus('success');
                    setMessage(t('auth.verifyEmail.successDesc'));
                } else {
                    setStatus('error');
                    setMessage(response.message || t('auth.verifyEmail.failed'));
                }
            } catch (err: unknown) {
                setStatus('error');
                const msg = err instanceof Error ? err.message : t('messages.error');
                setMessage(msg);
            }
        };

        verify();
    }, [searchParams, t]);

    return (
        <div className="min-h-[60vh] flex items-center justify-center px-4">
            <div className="max-w-md w-full bg-primary p-8 rounded-2xl shadow-xl border border-slate-100 dark:border-slate-800 text-center">
                {status === 'loading' && (
                    <div className="space-y-4">
                        <Loader2 className="w-16 h-16 text-blue-500 animate-spin mx-auto" />
                        <h2 className="text-2xl font-bold">{t('auth.verifyEmail.loading')}</h2>
                        <p className="text-secondary">{t('auth.verifyEmail.waitData')}</p>
                    </div>
                )}

                {status === 'success' && (
                    <div className="space-y-6">
                        <CheckCircle className="w-16 h-16 text-emerald-500 mx-auto" />
                        <h2 className="text-2xl font-bold">{t('auth.verifyEmail.success')}</h2>
                        <p className="text-secondary">{message}</p>
                        <Link to="/login" className="block w-full py-2.5 bg-indigo-600 text-white font-semibold rounded-lg hover:bg-indigo-700 transition-colors text-center">
                            {t('auth.verifyEmail.goToLogin')}
                        </Link>
                    </div>
                )}

                {status === 'error' && (
                    <div className="space-y-6">
                        <XCircle className="w-16 h-16 text-rose-500 mx-auto" />
                        <h2 className="text-2xl font-bold">{t('auth.verifyEmail.failed')}</h2>
                        <p className="text-error">{message}</p>
                        <div className="flex flex-col gap-3">
                            <Link to="/register" className="block w-full py-2.5 border-2 border-indigo-500 text-indigo-600 font-semibold rounded-lg hover:bg-indigo-600 hover:text-white transition-colors text-center">
                                {t('auth.verifyEmail.tryRegister')}
                            </Link>
                            <Link to="/contact" className="text-sm text-secondary hover:text-primary transition-colors">
                                {t('auth.verifyEmail.contactSupport')}
                            </Link>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default VerifyEmailPage;
