export enum OrderStatus {
    PENDING = 'pending',
    CONFIRMED = 'confirmed',
    PROCESSING = 'processing',
    SHIPPING = 'shipping',
    DELIVERED = 'delivered',
    CANCELLED = 'cancelled',
    RETURNED = 'returned',
    REFUNDED = 'refunded'
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
    VNPAY = 'VnPay',
    MOMO = 'MoMo',
    ZALOPAY = 'ZaloPay',
    BANK_TRANSFER = 'BankTransfer',
    CASH = 'Cash',
    CREDIT_CARD = 'CreditCard',
    DEBIT_CARD = 'DebitCard',
    E_WALLET = 'EWallet',
    PAYPAL = 'PayPal'
}

export const PaymentMethodLabel: Record<string, string> = {
    [PaymentMethod.COD]: 'Thanh toán khi nhận hàng (COD)',
    [PaymentMethod.VNPAY]: 'VNPAY QR',
    [PaymentMethod.MOMO]: 'Ví MoMo',
    [PaymentMethod.ZALOPAY]: 'ZaloPay',
    [PaymentMethod.BANK_TRANSFER]: 'Chuyển khoản ngân hàng',
    [PaymentMethod.CASH]: 'Thanh toán tiền mặt',
    [PaymentMethod.CREDIT_CARD]: 'Thẻ tín dụng',
    [PaymentMethod.DEBIT_CARD]: 'Thẻ ghi nợ',
    [PaymentMethod.E_WALLET]: 'Ví điện tử',
    [PaymentMethod.PAYPAL]: 'PayPal'
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

export enum Role {
    ADMIN = 'Admin',
    USER = 'User',
    CUSTOMER = 'Customer'
}

export enum QuoteStatus {
    PENDING = 'pending',
    QUOTED = 'quoted',
    CLOSED = 'closed',
    CANCELLED = 'cancelled'
}

export const PageSize = {
    DEFAULT: 10,
    SMALL: 5,
    LARGE: 20,
    XLARGE: 50
};

export const PaginationDefaults = {
    PAGE_INDEX: 1,
    PAGE_SIZE: PageSize.DEFAULT,
    TOTAL_ITEMS: 0,
    TOTAL_PAGES: 1
};

export enum SortDirection {
    ASC = 'asc',
    DESC = 'desc'
}

export { API_ENDPOINTS } from './endpoints';

