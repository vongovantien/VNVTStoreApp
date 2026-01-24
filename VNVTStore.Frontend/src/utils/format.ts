/**
 * Format currency in Vietnamese Dong
 */
export const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND',
    }).format(amount);
};

/**
 * Format number with thousand separators
 */
export const formatNumber = (num: number): string => {
    return new Intl.NumberFormat('vi-VN').format(num);
};

/**
 * Format date in Vietnamese locale
 */
export const formatDate = (
    dateString: string | Date | undefined | null,
    locale: string = 'vi-VN',
    options?: Intl.DateTimeFormatOptions
): string => {
    if (!dateString) return '';
    const date = typeof dateString === 'string' ? new Date(dateString) : dateString;
    // Check for valid date
    if (isNaN(date.getTime())) return '';

    return date.toLocaleDateString(locale, {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        ...options,
    });
};

/**
 * Format relative time (e.g., "2 hours ago")
 */
export const formatRelativeTime = (dateString: string | Date): string => {
    const date = typeof dateString === 'string' ? new Date(dateString) : dateString;
    const now = new Date();
    const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);

    const intervals: Record<string, number> = {
        năm: 31536000,
        tháng: 2592000,
        tuần: 604800,
        ngày: 86400,
        giờ: 3600,
        phút: 60,
    };

    for (const [unit, seconds] of Object.entries(intervals)) {
        const interval = Math.floor(diffInSeconds / seconds);
        if (interval >= 1) {
            return `${interval} ${unit} trước`;
        }
    }

    return 'Vừa xong';
};

/**
 * Truncate text with ellipsis
 */
export const truncateText = (text: string, maxLength: number): string => {
    if (text.length <= maxLength) return text;
    return `${text.slice(0, maxLength)}...`;
};

/**
 * Generate slug from text
 */
export const slugify = (text: string): string => {
    return text
        .toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/đ/g, 'd')
        .replace(/Đ/g, 'D')
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-|-$/g, '');
};

/**
 * Generate order status badge color
 */
export const getStatusColor = (status: string): string => {
    const colors: Record<string, string> = {
        pending: 'warning',
        confirmed: 'secondary',
        processing: 'info',
        shipping: 'primary',
        delivered: 'success',
        cancelled: 'error',
    };
    return colors[status] || 'default';
};

/**
 * Get order status text in Vietnamese
 */
export const getStatusText = (status: string): string => {
    const texts: Record<string, string> = {
        pending: 'admin.status.pending',
        confirmed: 'admin.status.confirmed',
        processing: 'admin.status.processing',
        shipping: 'admin.status.shipping',
        delivered: 'admin.status.delivered',
        cancelled: 'admin.status.cancelled',
    };
    return texts[status] || status;
};

/**
 * Calculate discount percentage
 */
export const calculateDiscount = (originalPrice: number, salePrice: number): number => {
    if (originalPrice <= 0) return 0;
    return Math.round(((originalPrice - salePrice) / originalPrice) * 100);
};

/**
 * Format file size
 */
export const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(1))} ${sizes[i]}`;
};

/**
 * Get full image URL from relative path
 */
export const getImageUrl = (path: string | undefined | null): string => {
    if (!path) return '';
    if (path.startsWith('http') || path.startsWith('data:')) return path;

    // Get base URL from env or default
    const apiBase = import.meta.env.VITE_API_URL || 'http://localhost:5176/api/v1';
    // Remove /api/v1 suffix to get root
    const root = apiBase.replace(/\/api\/v1\/?$/, '');

    // Ensure path starts with /
    const cleanPath = path.startsWith('/') ? path : `/${path}`;

    return `${root}${cleanPath}`;
};
