import api from './api';
import { ApiResponse, ProductQA } from '@/types';

export const productQAService = {
    getByProduct: async (productCode: string) => {
        const response = await api.get<ApiResponse<ProductQA[]>>(`/productqa/${productCode}`);
        return response.data;
    },

    create: async (productCode: string, question: string) => {
        const response = await api.post<ApiResponse<boolean>>('/productqa', {
            productCode,
            question
        });
        return response.data;
    }
};
