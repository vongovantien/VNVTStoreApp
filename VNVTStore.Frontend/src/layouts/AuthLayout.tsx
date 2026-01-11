import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';

interface AuthLayoutProps {
    children: React.ReactNode;
    title: string;
    subtitle: string;
}

export const AuthLayout = ({ children, title, subtitle }: AuthLayoutProps) => {
    const { t } = useTranslation();

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
                        <h1 className="text-2xl font-bold mt-4">{title}</h1>
                        <p className="text-secondary mt-2">{subtitle}</p>
                    </div>

                    {children}
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
