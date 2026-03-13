import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
// Store access imported indirectly via interceptors
// For pure TS files, we might need a singleton or event bus for Toasts if not inside a hook.
// However, typically we can import the store directly if using Zustand outside components.
// For now, I'll simulate the console warnings/redirections.

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';

export const apiClient = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
    timeout: 10000,
});

// Request Interceptor: Attach Token
apiClient.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const token = localStorage.getItem('auth_token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response Interceptor: Global Error Handling
apiClient.interceptors.response.use(
    (response) => response.data,
    (error: AxiosError) => {
        // 1. Handle 401 Unauthorized
        if (error.response?.status === 401) {
            // Clear session
            localStorage.removeItem('auth_token');
            // Redirect to login (window.location is the simplest way outside React Router context)
            window.location.href = '/login?expired=true';
            return Promise.reject(new Error('Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.'));
        }

        // 2. Handle 500 Server Error
        if (error.response?.status && error.response.status >= 500) {
            // We could dispatch a global event here
            console.error('System Error:', error.message);
            return Promise.reject(new Error('Hệ thống đang bảo trì. Vui lòng thử lại sau.'));
        }

        // 3. Handle Generic Errors
        return Promise.reject(error.response?.data || error);
    }
);
