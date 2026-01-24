import { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { CheckCircle, XCircle, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui';
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
            setStatus('error');
            setMessage('Invalid verification link.');
            return;
        }

        const verify = async () => {
            try {
                const response = await authService.verifyEmail(email, token);
                if (response.success) {
                    setStatus('success');
                    setMessage('Your email has been successfully verified! You can now log in.');
                } else {
                    setStatus('error');
                    setMessage(response.message || 'Verification failed.');
                }
            } catch (err: any) {
                setStatus('error');
                setMessage(err.response?.data?.message || 'Something went wrong.');
            }
        };

        verify();
    }, [searchParams]);

    return (
        <div className="min-h-[60vh] flex items-center justify-center px-4">
            <div className="max-w-md w-full bg-primary p-8 rounded-2xl shadow-xl border border-slate-100 dark:border-slate-800 text-center">
                {status === 'loading' && (
                    <div className="space-y-4">
                        <Loader2 className="w-16 h-16 text-blue-500 animate-spin mx-auto" />
                        <h2 className="text-2xl font-bold">Verifying Email...</h2>
                        <p className="text-secondary">Please wait while we activate your account.</p>
                    </div>
                )}

                {status === 'success' && (
                    <div className="space-y-6">
                        <CheckCircle className="w-16 h-16 text-emerald-500 mx-auto" />
                        <h2 className="text-2xl font-bold">Account Activated!</h2>
                        <p className="text-secondary">{message}</p>
                        <Link to="/login" className="block w-full py-2.5 bg-indigo-600 text-white font-semibold rounded-lg hover:bg-indigo-700 transition-colors text-center">
                            Go to Login
                        </Link>
                    </div>
                )}

                {status === 'error' && (
                    <div className="space-y-6">
                        <XCircle className="w-16 h-16 text-rose-500 mx-auto" />
                        <h2 className="text-2xl font-bold">Verification Failed</h2>
                        <p className="text-red-500">{message}</p>
                        <div className="flex flex-col gap-3">
                            <Link to="/register" className="block w-full py-2.5 border-2 border-indigo-500 text-indigo-600 font-semibold rounded-lg hover:bg-indigo-600 hover:text-white transition-colors text-center">
                                Try Registering Again
                            </Link>
                            <Link to="/contact" className="text-sm text-secondary hover:text-primary transition-colors">
                                Need help? Contact Support
                            </Link>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default VerifyEmailPage;
