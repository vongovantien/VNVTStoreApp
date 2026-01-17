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

        // Backend returns { url: "..." }
        return response.data.url;
    }
};
