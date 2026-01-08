export enum OrderStatus {
    PENDING = 'Pending',
    CONFIRMED = 'Confirmed',
    PROCESSING = 'Processing',
    SHIPPING = 'Shipping',
    DELIVERED = 'Delivered',
    CANCELLED = 'Cancelled',
    RETURNED = 'Returned',
    REFUNDED = 'Refunded'
}

export const OrderStatusLabel: Record<string, string> = {
    [OrderStatus.PENDING]: 'payment.status.pending', // or 'order.status.pending'
    [OrderStatus.CONFIRMED]: 'order.status.confirmed',
    [OrderStatus.PROCESSING]: 'order.status.processing',
    [OrderStatus.SHIPPING]: 'order.status.shipping',
    [OrderStatus.DELIVERED]: 'order.status.delivered',
    [OrderStatus.CANCELLED]: 'order.status.cancelled',
    [OrderStatus.RETURNED]: 'order.status.returned',
    [OrderStatus.REFUNDED]: 'order.status.refunded',
};

export enum PaymentMethod {
    COD = 'COD',
    VNPAY = 'VNPAY',
    MOMO = 'MOMO',
    ZALOPAY = 'ZALOPAY',
    BANK_TRANSFER = 'BANK_TRANSFER'
}

export const PaymentMethodLabel: Record<string, string> = {
    [PaymentMethod.COD]: 'Thanh toán khi nhận hàng (COD)',
    [PaymentMethod.VNPAY]: 'VNPAY QR',
    [PaymentMethod.MOMO]: 'Ví MoMo',
    [PaymentMethod.ZALOPAY]: 'ZaloPay',
    [PaymentMethod.BANK_TRANSFER]: 'Chuyển khoản ngân hàng'
};

export enum PaymentStatus {
    PENDING = 'Pending',
    PAID = 'Paid',
    FAILED = 'Failed',
    REFUNDED = 'Refunded'
}

export enum ReviewStatus {
    PENDING = 'Pending',
    APPROVED = 'Approved',
    REJECTED = 'Rejected'
}

export const PageSize = {
    DEFAULT: 10,
    SMALL: 5,
    LARGE: 20,
    XLARGE: 50
};
