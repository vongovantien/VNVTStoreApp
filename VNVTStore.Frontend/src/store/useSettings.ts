import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { createTabStorage } from './helpers';

interface StoreSettings {
    general: {
        storeName: string;
        email: string;
        phone: string;
        website: string;
        address: string;
        description: string;
    };
    payment: {
        cod: boolean;
        zaloPay: boolean;
        momo: boolean;
        vnpay: boolean;
        bankTransfer: boolean;
    };
    shipping: {
        defaultFee: number;
        freeShippingThreshold: number;
        estimatedDelivery: string;
    };
    notifications: {
        emailNewOrder: boolean;
        emailQuoteRequest: boolean;
        emailOrderStatus: boolean;
        lowStockAlert: boolean;
    };
}

interface SettingsState {
    settings: StoreSettings;
    updateSettings: (section: keyof StoreSettings, data: Partial<StoreSettings[keyof StoreSettings]>) => void;
    resetSettings: () => void;
}

const defaultSettings: StoreSettings = {
    general: {
        storeName: 'VNVT Store',
        email: 'contact@vnvt.store',
        phone: '1900 123 456',
        website: 'https://vnvt.store',
        address: '123 Nguyễn Huệ, Q.1, TP.HCM',
        description: 'Cửa hàng đồ gia dụng cao cấp - Chất lượng tạo nên sự khác biệt',
    },
    payment: {
        cod: true,
        zaloPay: true,
        momo: false,
        vnpay: true,
        bankTransfer: true,
    },
    shipping: {
        defaultFee: 30000,
        freeShippingThreshold: 500000,
        estimatedDelivery: '2-5 ngày làm việc',
    },
    notifications: {
        emailNewOrder: true,
        emailQuoteRequest: true,
        emailOrderStatus: true,
        lowStockAlert: false,
    },
};

export const useSettings = create<SettingsState>()(
    persist(
        (set) => ({
            settings: defaultSettings,
            updateSettings: (section, data) =>
                set((state) => ({
                    settings: {
                        ...state.settings,
                        [section]: {
                            ...state.settings[section],
                            ...data,
                        },
                    },
                })),
            resetSettings: () => set({ settings: defaultSettings }),
        }),
        {
            name: 'vnvt-admin-settings',
            storage: createJSONStorage(() => createTabStorage()),
        }
    )
);
