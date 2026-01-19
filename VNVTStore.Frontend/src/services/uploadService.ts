import apiClient from '@/services/api';

export const uploadService = {
    upload: async (file: File): Promise<string> => {
        const formData = new FormData();
        formData.append('file', file);

        // Using apiClient.instance to ensure auth headers are included
        const response = await apiClient.instance.post('/upload', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });

        // Backend returns { url: "/uploads/..." }
        // We need to prepend the API root URL if it's a relative path
        const relativeUrl = response.data.url;

        // Get API Base URL from env or default, remove /api/v1 part to get root
        const apiBaseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5176/api/v1';
        // Simple heuristic: assume API is at /api/v1 and we want root
        // If apiBaseUrl is http://localhost:5176/api/v1, root is http://localhost:5176
        const rootUrl = apiBaseUrl.replace(/\/api\/v1\/?$/, '');

        return `${rootUrl}${relativeUrl}`;
    }
};
