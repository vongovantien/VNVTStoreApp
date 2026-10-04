import { apiClient, type ApiResponse } from './api';

export interface NotificationDto {
    code: string;
    userCode: string;
    title: string;
    message: string;
    type: string; // 'order' | 'quote' | 'delivery' | 'system' | 'INFO' | etc.
    link?: string;
    isRead: boolean;
    createdAt: string;
}

export const notificationService = {
    getMyNotifications: async () => {
        const response = await apiClient.get<ApiResponse<NotificationDto[]>>('/notifications');
        return response.data?.data || [];
    },

    getUnreadCount: async () => {
        const response = await apiClient.get<ApiResponse<number>>('/notifications/unread-count');
        return response.data?.data || 0;
    },

    markAsRead: async (code: string) => {
        const response = await apiClient.put<ApiResponse<boolean>>(`/notifications/${code}/read`);
        return response.data?.data || false;
    },

    markAllAsRead: async () => {
        const response = await apiClient.put<ApiResponse<boolean>>('/notifications/read-all');
        return response.data?.data || false;
    },

    deleteNotification: async (code: string) => {
        const response = await apiClient.delete<ApiResponse<boolean>>(`/notifications/${code}`);
        return response.data?.data || false;
    },
};

export default notificationService;
