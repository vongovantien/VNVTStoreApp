import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';

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
            setStep: (step) => set({ step }),
            setFormData: (data) => set((state) => ({ formData: { ...state.formData, ...data } })),
            setPaymentMethod: (paymentMethod) => set({ paymentMethod }),
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
            storage: createJSONStorage(() => localStorage),
        }
    )
);
