import { useCallback } from 'react';
import { useCheckoutStore } from '../store/checkoutStore';
import { useService } from '@/core/di/useService';
import { ServiceKeys } from '@/core/di/ServiceKeys';
import { ICheckoutService } from '../services/ICheckoutService';
import { useToast } from '@/store';

export const useBuyNow = () => {
    const setSession = useCheckoutStore((state) => state.setSession);
    const checkoutService = useService<ICheckoutService>(ServiceKeys.Checkout);
    const { error } = useToast();

    const handleBuyNow = useCallback(async (product: { code: string; name: string; price: number; images?: string[] }, quantity: number) => {
        try {
            const isAvailable = await checkoutService.validateStock(product.code, quantity);

            if (!isAvailable) {
                error('Sản phẩm tạm thời hết hàng hoặc không đủ số lượng.');
                return;
            }

            setSession({
                product: {
                    id: product.code,
                    name: product.name,
                    price: product.price,
                    image: product.images?.[0],
                },
                quantity,
            });
        } catch {
            error('Không thể kiểm tra tồn kho. Vui lòng thử lại.');
        }
    }, [setSession, checkoutService, error]);

    return { handleBuyNow };
};
