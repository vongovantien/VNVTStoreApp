/**
 * Format currency with locale and currency options
 */
export const formatCurrency = (amount: number, options?: { locale?: string; currency?: string }): string => {
    return new Intl.NumberFormat(options?.locale || 'vi-VN', {
        style: 'currency',
        currency: options?.currency || 'VND',
        minimumFractionDigits: options?.currency === 'USD' ? 2 : 0,
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
 * Format relative time (e.g., "2 hours ago") — locale-aware
 */
export const formatRelativeTime = (dateString: string | Date): string => {
    const date = typeof dateString === 'string' ? new Date(dateString) : dateString;
    const now = new Date();
    const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);

    // Determine the current locale from i18n or fallback to browser default
    let locale: string;
    try {
        // Dynamic import avoidance: read from HTML lang attribute set by i18n
        locale = document.documentElement.lang || navigator.language || 'vi';
    } catch {
        locale = 'vi';
    }

    const rtf = new Intl.RelativeTimeFormat(locale, { numeric: 'auto' });

    const intervals: Array<{ unit: Intl.RelativeTimeFormatUnit; seconds: number }> = [
        { unit: 'year', seconds: 31536000 },
        { unit: 'month', seconds: 2592000 },
        { unit: 'week', seconds: 604800 },
        { unit: 'day', seconds: 86400 },
        { unit: 'hour', seconds: 3600 },
        { unit: 'minute', seconds: 60 },
    ];

    for (const { unit, seconds } of intervals) {
        const interval = Math.floor(diffInSeconds / seconds);
        if (interval >= 1) {
            return rtf.format(-interval, unit);
        }
    }

    return rtf.format(0, 'second'); // "just now" / "vừa xong"
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
    const statusMap: Record<string, string> = {
        pending: 'common.status.pending',
        confirmed: 'common.status.confirmed',
        processing: 'common.status.processing',
        shipping: 'common.status.shipping',
        delivered: 'common.status.delivered',
        cancelled: 'common.status.cancelled',
    };
    return statusMap[status] || status;
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
    const apiBase = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
    // Remove /api/v1 suffix to get root
    const root = apiBase.replace(/\/api\/v1\/?$/, '');

    // Ensure path starts with /
    const cleanPath = path.startsWith('/') ? path : `/${path}`;

    return `${root}${cleanPath}`;
};

/**
 * Parse customer name and phone from order details, extracting from shipping address if needed (useful for guest/partial checkouts)
 */
export interface ParsableOrderCustomer {
    customerName?: string;
    customerPhone?: string;
    shippingName?: string;
    shippingPhone?: string;
    userCode: string;
    shippingAddress?: string;
}

export const parseOrderCustomer = (order: ParsableOrderCustomer) => {
    let name = order.customerName || order.shippingName || order.userCode;
    let phone = order.customerPhone || order.shippingPhone || '-';

    if ((name === order.userCode || name === 'USR_GUEST') && order.shippingAddress?.includes('Receiver:')) {
        const matchName = order.shippingAddress.match(/Receiver:\s*([^,|]+)/);
        const matchPhone = order.shippingAddress.match(/Phone:\s*([^,|]+)/);
        if (matchName?.[1]) name = matchName[1].trim();
        if (matchPhone?.[1]) phone = matchPhone[1].trim();
    }

    return { name, phone };
};
