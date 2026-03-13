export interface CustomerInfo {
    fullName: string;
    phoneNumber: string;
    address: string;
    email?: string;
    note?: string;
}

export interface OrderItem {
    productId: string;
    quantity: number;
    price: number;
    variantId?: string;
}

export interface Order {
    id: string;
    customer: CustomerInfo;
    items: OrderItem[];
    totalAmount: number;
    status: 'pending' | 'confirmed' | 'shipped';
    createdAt: string;
}

export interface CheckoutSession {
    product: {
        id: string;
        name: string;
        price: number;
        image?: string;
    };
    quantity: number;
    variantId?: string;
}
