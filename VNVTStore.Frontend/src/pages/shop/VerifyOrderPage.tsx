import { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { CheckCircle, XCircle } from 'lucide-react';
import { Button } from '@/components/ui';
import { orderService } from '@/services/orderService';
import { PageLoader } from '@/components/common/PageLoader';

const VerifyOrderPage = () => {
    const { t } = useTranslation();
    const [searchParams] = useSearchParams();
    const token = searchParams.get('token');
    
    const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
    const [message, setMessage] = useState('');

    useEffect(() => {
        const verify = async () => {
            if (!token) {
                setStatus('error');
                setMessage(t('verifyOrder.missingToken') || 'Mã xác thực không hợp lệ');
                return;
            }

            try {
                const res = await orderService.verify(token);
                if (res.success) {
                    setStatus('success');
                    setMessage(res.message || t('verifyOrder.success') || 'Xác thực đơn hàng thành công!');
                } else {
                    setStatus('error');
                    setMessage(res.message || t('verifyOrder.failed') || 'Xác thực thất bại');
                }
            } catch (error: any) {
                setStatus('error');
                setMessage(error.message || t('verifyOrder.error') || 'Có lỗi xảy ra khi xác thực');
            }
        };

        verify();
    }, [token, t]);

    if (status === 'loading') {
        return <PageLoader />;
    }

    return (
        <div className="min-h-screen bg-secondary flex items-center justify-center p-4">
            <div className="bg-primary p-8 rounded-2xl shadow-lg max-w-md w-full text-center">
                {status === 'success' ? (
                    <>
                        <div className="w-16 h-16 bg-success/10 rounded-full flex items-center justify-center mx-auto mb-4">
                            <CheckCircle className="w-8 h-8 text-success" />
                        </div>
                        <h1 className="text-2xl font-bold mb-2">{t('verifyOrder.verifiedTitle') || 'Xác thực thành công'}</h1>
                        <p className="text-secondary mb-6">{message}</p>
                        <Link to="/">
                            <Button className="w-full">{t('common.continueShopping') || 'Tiếp tục mua sắm'}</Button>
                        </Link>
                    </>
                ) : (
                    <>
                         <div className="w-16 h-16 bg-error/10 rounded-full flex items-center justify-center mx-auto mb-4">
                            <XCircle className="w-8 h-8 text-error" />
                        </div>
                        <h1 className="text-2xl font-bold mb-2">{t('verifyOrder.failedTitle') || 'Xác thực thất bại'}</h1>
                        <p className="text-secondary mb-6">{message}</p>
                        <Link to="/">
                            <Button variant="outline" className="w-full">{t('common.home') || 'Về trang chủ'}</Button>
                        </Link>
                    </>
                )}
            </div>
        </div>
    );
};

export default VerifyOrderPage;
