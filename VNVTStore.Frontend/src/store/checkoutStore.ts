import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { createTabStorage } from './helpers';
import { useDiagnosticStore } from './diagnosticStore';

export interface CheckoutFormData {
    fullName: string;
    phone: string;
    email: string;
    address: string;
    city: string;
    district: string;
    ward: string;
    note: string;
}

interface CheckoutState {
    step: number;
    formData: CheckoutFormData;
    paymentMethod: string;
    voucherCode: string;
    setStep: (step: number) => void;
    setFormData: (data: Partial<CheckoutFormData>) => void;
    setPaymentMethod: (method: string) => void;
    setVoucherCode: (code: string) => void;
    resetCheckout: () => void;
}

const initialFormData: CheckoutFormData = {
    fullName: '',
    phone: '',
    email: '',
    address: '',
    city: '',
    district: '',
    ward: '',
    note: ''
};

export const useCheckoutStore = create<CheckoutState>()(
    persist(
        (set) => ({
            step: 1,
            formData: initialFormData,
            paymentMethod: 'COD',
            voucherCode: '',
            setStep: (step) => {
                useDiagnosticStore.getState().track({
                    module: 'SHOP',
                    eventType: 'CHECKOUT_STEP_CHANGE',
                    description: `Moved to checkout step ${step}`,
                    payload: { step },
                    severity: 'INFO'
                });
                set({ step });
            },
            setFormData: (data) => set((state) => ({ formData: { ...state.formData, ...data } })),
            setPaymentMethod: (paymentMethod) => {
                useDiagnosticStore.getState().track({
                    module: 'SHOP',
                    eventType: 'PAYMENT_METHOD_SELECT',
                    description: `User selected ${paymentMethod} payment method`,
                    payload: { paymentMethod },
                    severity: 'INFO'
                });
                set({ paymentMethod });
            },
            setVoucherCode: (voucherCode) => set({ voucherCode }),
            resetCheckout: () => set({
                step: 1,
                formData: initialFormData,
                paymentMethod: 'COD',
                voucherCode: ''
            })
        }),
        {
            name: 'vnvt-checkout',
            storage: createJSONStorage(() => createTabStorage()),
        }
    )
);
