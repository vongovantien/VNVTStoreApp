import { useState, useEffect } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Lock, Eye, EyeOff, Loader2, CheckCircle, AlertCircle } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { authService } from '@/services/authService';
import { useToast } from '@/store';

export const ResetPasswordPage = () => {
    const { t } = useTranslation();
    const { success, error: toastError } = useToast();
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();

    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [isSuccess, setIsSuccess] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const email = searchParams.get('email');
    const token = searchParams.get('token');

    useEffect(() => {
        if (!email || !token) {
            setError('Invalid or expired reset link.');
        }
    }, [email, token]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        
        if (password !== confirmPassword) {
            toastError("Passwords do not match.");
            return;
        }

        if (!email || !token) return;

        setIsLoading(true);
        setError(null);
        
        try {
            const response = await authService.resetPassword({
                email,
                token,
                newPassword: password
            });

            if (response.success) {
                setIsSuccess(true);
                success("Password has been reset successfully!");
                setTimeout(() => navigate('/login'), 3000);
            } else {
                setError(response.message || 'Failed to reset password.');
                toastError(response.message || 'Failed to reset password.');
            }
        } catch (err: any) {
            const msg = err.response?.data?.message || 'Something went wrong.';
            setError(msg);
            toastError(msg);
        } finally {
            setIsLoading(false);
        }
    };

    if (isSuccess) {
        return (
            <div className="min-h-[60vh] flex items-center justify-center px-4">
                <div className="max-w-md w-full bg-primary p-8 rounded-2xl shadow-xl border border-slate-100 dark:border-slate-800 text-center space-y-6">
                    <CheckCircle className="w-16 h-16 text-emerald-500 mx-auto" />
                    <h2 className="text-2xl font-bold">Success!</h2>
                    <p className="text-secondary">
                        Your password has been successfully reset. Redirecting you to login...
                    </p>
                    <Link to="/login" className="block w-full py-2.5 bg-indigo-600 text-white font-semibold rounded-lg hover:bg-indigo-700 transition-colors text-center">
                        Go to Login Now
                    </Link>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-[60vh] flex items-center justify-center px-4">
            <div className="max-w-md w-full bg-primary p-8 rounded-2xl shadow-xl border border-slate-100 dark:border-slate-800">
                <div className="text-center mb-8">
                    <h1 className="text-3xl font-bold mb-2">Reset Password</h1>
                    <p className="text-secondary">Set a new strong password for your account.</p>
                </div>

                {error && (
                    <div className="mb-6 p-4 bg-rose-50 dark:bg-rose-900/20 border border-rose-100 dark:border-rose-900/30 rounded-lg flex items-start gap-3 text-rose-600 dark:text-rose-400 text-sm">
                        <AlertCircle size={18} className="shrink-0 mt-0.5" />
                        <p>{error}</p>
                    </div>
                )}

                <form onSubmit={handleSubmit} className="space-y-5">
                    <div className="relative">
                        <Input
                            label="New Password"
                            type={showPassword ? "text" : "password"}
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            placeholder="••••••••"
                            leftIcon={<Lock size={18} />}
                            isRequired
                            disabled={!!error || isLoading}
                        />
                        <button
                            type="button"
                            onClick={() => setShowPassword(!showPassword)}
                            className="absolute right-3 top-[38px] text-slate-400 hover:text-indigo-500 transition-colors"
                        >
                            {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                        </button>
                    </div>

                    <Input
                        label="Confirm New Password"
                        type={showPassword ? "text" : "password"}
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        placeholder="••••••••"
                        leftIcon={<Lock size={18} />}
                        isRequired
                        disabled={!!error || isLoading}
                    />

                    <Button 
                        type="submit" 
                        fullWidth 
                        isLoading={isLoading} 
                        disabled={!!error || !password || password !== confirmPassword}
                    >
                        Update Password
                    </Button>
                </form>

                <div className="mt-8 pt-6 border-t text-center">
                    <Link 
                        to="/login" 
                        className="text-sm text-secondary hover:text-primary transition-colors font-medium"
                    >
                        Back to Login
                    </Link>
                </div>
            </div>
        </div>
    );
};

export default ResetPasswordPage;
