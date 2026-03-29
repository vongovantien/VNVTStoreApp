import { useTranslation } from 'react-i18next';
import { ShoppingBag } from 'lucide-react';

import { Modal } from '@/components/ui';
import { ErrorBoundary } from '@/core/errors/ErrorBoundary';
import { WidgetError } from '@/components/shared/Fallbacks/WidgetError';
import { useCheckoutStore } from '../store/checkoutStore';
import { CheckoutForm } from './CheckoutForm';

export const BuyNowModal = () => {
  const { t } = useTranslation();
  const { session, isOpen, closeCheckout } = useCheckoutStore();

  if (!session) return null;

  return (
    <Modal
      isOpen={isOpen}
      onClose={closeCheckout}
      title={
        <div className="flex items-center gap-2 text-rose-600">
          <ShoppingBag size={24} />
          <span>{t('checkout.buyNow', 'Mua ngay')}</span>
        </div>
      }
      size="lg"
    >
      <ErrorBoundary fallback={WidgetError} context={{ title: 'Lỗi thanh toán' }}>
        <div className="space-y-4">
          <p className="text-sm text-slate-500">
            {t('checkout.description', 'Vui lòng điền thông tin để chúng tôi giao hàng cho bạn nhanh nhất.')}
          </p>
          <CheckoutForm />
        </div>
      </ErrorBoundary>
    </Modal>
  );
};
