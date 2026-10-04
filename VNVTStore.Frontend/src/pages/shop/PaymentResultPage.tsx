import { useEffect, useState } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { CheckCircle2, XCircle, ArrowRight, RefreshCw, ShoppingBag, ShieldCheck } from 'lucide-react';
import { Button } from '@/components/ui';
import { formatCurrency } from '@/utils/format';
import { paymentService, PaymentConfirmationResult } from '@/services/paymentService';
import { useSEO } from '@/hooks/useSEO';

export const PaymentResultPage = () => {
    const { t } = useTranslation();
    useSEO({
        title: 'Kết quả thanh toán',
        noindex: true
    });

    const [searchParams] = useSearchParams();
    const navigate = useNavigate();

    const [isLoading, setIsLoading] = useState(true);
    const [result, setResult] = useState<PaymentConfirmationResult | null>(null);
    const [isRetrying, setIsRetrying] = useState(false);

    // Extract common query parameters
    const vnpResponseCode = searchParams.get('vnp_ResponseCode');
    const momoResultCode = searchParams.get('resultCode');
    const orderCodeFromParam = searchParams.get('orderCode') || searchParams.get('orderId');

    const isVnPay = searchParams.has('vnp_SecureHash') || searchParams.has('vnp_ResponseCode');
    const isMoMo = searchParams.has('partnerCode') || searchParams.has('resultCode');

    useEffect(() => {
        let isMounted = true;

        const verify = async () => {
            const params: Record<string, string> = {};
            searchParams.forEach((val, key) => {
                params[key] = val;
            });

            try {
                if (isVnPay) {
                    const res = await paymentService.verifyVnPayReturn(params);
                    if (isMounted) {
                        setResult(res.data || {
                            outcome: 'Failed',
                            isSuccess: false,
                            amount: 0,
                            message: res.message || 'Xác thực thất bại'
                        });
                    }
                } else if (isMoMo) {
                    const res = await paymentService.verifyMoMoReturn(params);
                    if (isMounted) {
                        setResult(res.data || {
                            outcome: 'Failed',
                            isSuccess: false,
                            amount: 0,
                            message: res.message || 'Xác thực thất bại'
                        });
                    }
                } else {
                    // No gateway params detected
                    if (isMounted) {
                        setResult({
                            outcome: 'NotFound',
                            isSuccess: false,
                            amount: 0,
                            message: 'Không tìm thấy thông tin giao dịch'
                        });
                    }
                }
            } catch (err: unknown) {
                if (isMounted) {
                    const msg = err instanceof Error ? err.message : 'Lỗi kết nối khi xác thực thanh toán';
                    setResult({
                        outcome: 'Failed',
                        isSuccess: false,
                        amount: 0,
                        message: msg
                    });
                }
            } finally {
                if (isMounted) setIsLoading(false);
            }
        };

        verify();

        return () => {
            isMounted = false;
        };
    }, [searchParams, isVnPay, isMoMo]);

    const handleRetryPayment = async () => {
        const orderCode = result?.orderCode || orderCodeFromParam;
        if (!orderCode) {
            navigate('/products');
            return;
        }

        setIsRetrying(true);
        try {
            const method = isMoMo ? 'MoMo' : 'VnPay';
            const res = await paymentService.createCheckoutUrl(orderCode, method);
            if (res.success && res.data?.paymentUrl) {
                window.location.href = res.data.paymentUrl;
            } else {
                alert(res.message || 'Không thể tạo cổng thanh toán mới. Vui lòng thử lại sau.');
            }
        } catch {
            alert('Lỗi khi kết nối tới cổng thanh toán.');
        } finally {
            setIsRetrying(false);
        }
    };

    if (isLoading) {
        return (
            <div className="min-h-[70vh] flex flex-col items-center justify-center bg-secondary px-4">
                <div className="bg-primary rounded-3xl p-8 max-w-md w-full text-center shadow-lg border border-primary/10">
                    <div className="inline-flex p-4 rounded-full bg-indigo-50 dark:bg-indigo-900/30 text-indigo-600 mb-4 animate-spin">
                        <RefreshCw size={36} />
                    </div>
                    <h2 className="text-xl font-bold mb-2">Đang xác thực giao dịch...</h2>
                    <p className="text-sm text-tertiary">
                        Hệ thống đang kiểm tra kết quả chữ ký điện tử với cổng thanh toán. Vui lòng không đóng trang.
                    </p>
                </div>
            </div>
        );
    }

    const isSuccess = result?.isSuccess ?? false;
    const finalOrderCode = result?.orderCode || orderCodeFromParam;

    return (
        <div className="min-h-[80vh] bg-secondary py-12 px-4 flex items-center justify-center">
            <div className="bg-primary rounded-3xl p-8 sm:p-10 max-w-lg w-full shadow-xl border border-primary/10 text-center animate-in fade-in zoom-in-95 duration-300">
                {isSuccess ? (
                    <>
                        <div className="inline-flex p-4 rounded-full bg-emerald-100 dark:bg-emerald-900/30 text-emerald-600 mb-6">
                            <CheckCircle2 size={56} strokeWidth={2.2} />
                        </div>
                        <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-emerald-50 dark:bg-emerald-950/50 text-emerald-600 text-xs font-semibold mb-3">
                            <ShieldCheck size={14} /> Giao dịch được bảo mật
                        </span>
                        <h1 className="text-2xl sm:text-3xl font-black text-primary mb-2">
                            Thanh toán thành công!
                        </h1>
                        <p className="text-sm text-tertiary mb-6">
                            Đơn hàng của bạn đã được ghi nhận và chuyển sang trạng thái đã thanh toán.
                        </p>

                        <div className="bg-secondary/60 rounded-2xl p-5 mb-8 text-left space-y-3 border border-secondary/20">
                            {finalOrderCode && (
                                <div className="flex justify-between items-center text-sm">
                                    <span className="text-tertiary">Mã đơn hàng</span>
                                    <span className="font-bold text-primary">#{finalOrderCode}</span>
                                </div>
                            )}
                            {result?.amount ? (
                                <div className="flex justify-between items-center text-sm">
                                    <span className="text-tertiary">Số tiền thanh toán</span>
                                    <span className="font-bold text-emerald-600 text-base">
                                        {formatCurrency(result.amount)}
                                    </span>
                                </div>
                            ) : null}
                            <div className="flex justify-between items-center text-sm">
                                <span className="text-tertiary">Cổng thanh toán</span>
                                <span className="font-semibold text-primary">
                                    {isMoMo ? 'Ví MoMo' : isVnPay ? 'VNPAY QR' : 'Online Gateway'}
                                </span>
                            </div>
                        </div>

                        <div className="flex flex-col sm:flex-row gap-3">
                            {finalOrderCode && (
                                <Link to={`/order-success?code=${finalOrderCode}`} className="flex-1">
                                    <Button className="w-full gap-2">
                                        Xem chi tiết đơn <ArrowRight size={16} />
                                    </Button>
                                </Link>
                            )}
                            <Link to="/products" className="flex-1">
                                <Button variant="outline" className="w-full gap-2">
                                    <ShoppingBag size={16} /> Tiếp tục mua sắm
                                </Button>
                            </Link>
                        </div>
                    </>
                ) : (
                    <>
                        <div className="inline-flex p-4 rounded-full bg-rose-100 dark:bg-rose-900/30 text-rose-600 mb-6">
                            <XCircle size={56} strokeWidth={2.2} />
                        </div>
                        <h1 className="text-2xl sm:text-3xl font-black text-primary mb-2">
                            Thanh toán không thành công
                        </h1>
                        <p className="text-sm text-tertiary mb-6">
                            {result?.message || 'Giao dịch bị huỷ hoặc có lỗi xảy ra từ ngân hàng / ví điện tử.'}
                        </p>

                        {finalOrderCode && (
                            <div className="bg-secondary/60 rounded-2xl p-5 mb-8 text-left space-y-2 border border-secondary/20">
                                <div className="flex justify-between items-center text-sm">
                                    <span className="text-tertiary">Mã đơn hàng</span>
                                    <span className="font-bold text-primary">#{finalOrderCode}</span>
                                </div>
                                <p className="text-xs text-tertiary">
                                    Đơn hàng của bạn vẫn được lưu ở trạng thái <strong>Chờ thanh toán</strong>. Bạn có thể thử thanh toán lại hoặc chọn phương thức khác.
                                </p>
                            </div>
                        )}

                        <div className="flex flex-col sm:flex-row gap-3">
                            <Button 
                                onClick={handleRetryPayment} 
                                isLoading={isRetrying} 
                                className="flex-1 gap-2 bg-indigo-600 hover:bg-indigo-700"
                            >
                                <RefreshCw size={16} /> Thanh toán lại
                            </Button>
                            {finalOrderCode ? (
                                <Link to={`/order-success?code=${finalOrderCode}`} className="flex-1">
                                    <Button variant="outline" className="w-full">
                                        Xem đơn hàng
                                    </Button>
                                </Link>
                            ) : (
                                <Link to="/cart" className="flex-1">
                                    <Button variant="outline" className="w-full">
                                        Về giỏ hàng
                                    </Button>
                                </Link>
                            )}
                        </div>
                    </>
                )}
            </div>
        </div>
    );
};

export default PaymentResultPage;
