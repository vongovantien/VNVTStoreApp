import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import userService from '../repositories/UserRepository';
import { UserPayload } from '../types/User';
import { SearchParams } from '@/services/baseService';

export const USER_KEYS = {
    all: ['users'] as const,
    list: (params: SearchParams) => [...USER_KEYS.all, params] as const,
    details: () => [...USER_KEYS.all, 'detail'] as const,
    detail: (id: string) => [...USER_KEYS.details(), id] as const,
};

export const useUsers = (params: SearchParams) => {
    return useQuery({
        queryKey: USER_KEYS.list(params),
        queryFn: async () => {
            const response = await userService.search(params);
            if (!response.success) {
                throw new Error(response.message || 'Failed to fetch users');
            }
            // Unwrap PagedResult from ApiResponse
            return response.data!;
        },
        placeholderData: (previousData) => previousData,
        staleTime: 1000 * 60 * 5, // 5 minutes
    });
};

export const useUserMutation = () => {
    const queryClient = useQueryClient();

    const create = useMutation({
        mutationFn: (payload: UserPayload) => userService.create(payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: USER_KEYS.all });
        }
    });

    const update = useMutation({
        mutationFn: ({ id, payload }: { id: string; payload: Partial<UserPayload> }) =>
            userService.update(id, payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: USER_KEYS.all });
        }
    });

    const deleteUser = useMutation({
        mutationFn: (id: string) => userService.delete(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: USER_KEYS.all });
        }
    });

    return { create, update, delete: deleteUser };
};
