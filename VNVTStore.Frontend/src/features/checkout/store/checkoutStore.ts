import { create } from 'zustand';
import { CheckoutSession } from '../types';

interface CheckoutState {
    session: CheckoutSession | null;
    isOpen: boolean;
    setSession: (session: CheckoutSession) => void;
    clearSession: () => void;
    openCheckout: () => void;
    closeCheckout: () => void;
}

export const useCheckoutStore = create<CheckoutState>((set) => ({
    session: null,
    isOpen: false,
    setSession: (session) => set({ session, isOpen: true }),
    clearSession: () => set({ session: null, isOpen: false }),
    openCheckout: () => set({ isOpen: true }),
    closeCheckout: () => set({ isOpen: false }),
}));
