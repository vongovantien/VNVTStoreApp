// ============ Query Keys ============
export const productKeys = {
    all: ['products'] as const,
    lists: () => [...productKeys.all, 'list'] as const,
    list: (params: Record<string, unknown>) => [...productKeys.lists(), params] as const,
    details: () => [...productKeys.all, 'detail'] as const,
    detail: (code: string) => [...productKeys.details(), code] as const,
    stats: () => [...productKeys.all, 'stats'] as const,
    categories: ['categories'] as const,
};
