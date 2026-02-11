import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { bannerService, CreateBannerRequest, UpdateBannerRequest } from '@/services/bannerService';
import { SearchParams } from '@/services/baseService';
import { BANNER_LIST_FIELDS } from '@/constants/fieldConstants';

export const useBanners = (params?: SearchParams) => {
    const { fields = BANNER_LIST_FIELDS } = params || {};
    return useQuery({
        queryKey: ['banners', params],
        queryFn: () => bannerService.search({ ...params, fields: params?.fields || fields }),
        select: (response) => {
            if (response.success && response.data) {
                // Deduplicate
                const uniqueItems = Array.from(new Map((response.data.items || []).map(item => [item.code, item])).values());

                return {
                    ...response,
                    data: {
                        ...response.data,
                        items: uniqueItems
                    }
                };
            }
            return response;
        }
    });
};

export const useBanner = (code: string) => {
    return useQuery({
        queryKey: ['banner', code],
        queryFn: () => bannerService.getByCode(code),
        enabled: !!code,
    });
};

export const useCreateBanner = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (data: CreateBannerRequest) => bannerService.create(data),
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['banners'] }),
    });
};

export const useUpdateBanner = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ code, data }: { code: string; data: UpdateBannerRequest }) =>
            bannerService.update(code, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['banners'] });
            queryClient.invalidateQueries({ queryKey: ['banner'] });
        },
    });
};

export const useDeleteBanner = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (code: string) => bannerService.delete(code),
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['banners'] }),
    });
};
