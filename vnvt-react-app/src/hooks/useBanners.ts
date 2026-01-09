import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { bannerService, CreateBannerRequest, UpdateBannerRequest } from '@/services/bannerService';
import { SearchParams } from '@/services/baseService';

export const useBanners = (params?: SearchParams) => {
    return useQuery({
        queryKey: ['banners', params],
        queryFn: () => bannerService.getAll(params),
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
