import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { createTabStorage } from './helpers';

// ============ Notification Store ============
interface NotificationState {
    notifications: string[];
    unreadCount: number;
    addNotification: (message: string) => void;
    markAllRead: () => void;
    clearNotifications: () => void;
}

export const useNotificationStore = create<NotificationState>()(
    persist(
        (set) => ({
            notifications: [],
            unreadCount: 0,
            addNotification: (message) => {
                set((state) => ({
                    notifications: [message, ...state.notifications],
                    unreadCount: state.unreadCount + 1,
                }));
            },
            markAllRead: () => set({ unreadCount: 0 }),
            clearNotifications: () => set({ notifications: [], unreadCount: 0 }),
        }),
        {
            name: 'vnvt-notifications',
            storage: createJSONStorage(() => createTabStorage()),
        }
    )
);
