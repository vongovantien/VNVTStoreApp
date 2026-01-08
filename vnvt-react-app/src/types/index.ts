// ============ Product Types ============
export interface Product {
    id: string;
    name: string;
    slug: string;
    description: string;
    price: number;
    originalPrice?: number;
    discount?: number;
    image: string;
    images?: string[];
    category: string;
    categoryId: string;
    brand?: string;
    stock: number;
    rating: number;
    reviewCount: number;
    isFeatured?: boolean;
    isNew?: boolean;
    specifications?: Record<string, string>;
    createdAt: string;
    updatedAt?: string;
}

// ============ Category Types ============
export interface Category {
    id: string;
    name: string;
    slug: string;
    description?: string;
    image?: string;
    parentId?: string;
    productCount: number;
}

// ============ Cart Types ============
export interface CartItem {
    id: string;
    product: Product;
    quantity: number;
}

// ============ User Types ============
export interface User {
    id: string;
    email: string;
    name: string;
    phone?: string;
    avatar?: string;
    role: 'customer' | 'admin' | 'staff';
    addresses?: Address[];
    createdAt: string;
}

export interface Address {
    id: string;
    fullName: string;
    phone: string;
    address: string;
    city: string;
    district: string;
    ward: string;
    isDefault?: boolean;
}

// ============ Order Types ============
export type OrderStatus = 'pending' | 'confirmed' | 'processing' | 'shipping' | 'delivered' | 'cancelled';

export interface Order {
    id: string;
    orderNumber: string;
    userId: string;
    customer: {
        name: string;
        email: string;
        phone: string;
        address: string;
    };
    items: OrderItem[];
    subtotal: number;
    shippingFee: number;
    discount: number;
    total: number;
    status: OrderStatus;
    paymentMethod: string;
    paymentStatus: 'pending' | 'paid' | 'failed' | 'refunded';
    note?: string;
    createdAt: string;
    updatedAt?: string;
}

export interface OrderItem {
    productId: string;
    productName: string;
    productImage: string;
    price: number;
    quantity: number;
}

// ============ Review Types ============
export interface Review {
    id: string;
    productId: string;
    userId: string;
    userName: string;
    userAvatar?: string;
    rating: number;
    title?: string;
    content: string;
    images?: string[];
    createdAt: string;
    helpful?: number;
}

// ============ Quote Request Types ============
export type QuoteStatus = 'pending' | 'quoted' | 'closed' | 'cancelled';

export interface QuoteRequest {
    id: string;
    productId: string;
    productName: string;
    productImage: string;
    customer: {
        name: string;
        email: string;
        phone: string;
    };
    quantity: number;
    note?: string;
    status: QuoteStatus;
    quotedPrice?: number;
    adminNote?: string;
    createdAt: string;
    updatedAt?: string;
}

// ============ Dashboard Types ============
export interface DashboardStats {
    totalRevenue: number;
    revenueChange: number;
    totalOrders: number;
    ordersChange: number;
    totalProducts: number;
    totalCustomers: number;
    customersChange: number;
    pendingQuotes: number;
}

export interface ChartData {
    labels: string[];
    data: number[];
}

// ============ AI/Chat Types ============
export interface ChatMessage {
    id: string;
    role: 'user' | 'assistant';
    content: string;
    timestamp: string;
}

export interface AIRecommendation {
    productId: string;
    score: number;
    reason: string;
}

// ============ Filter Types ============
export interface ProductFilter {
    search?: string;
    categories?: string[];
    brands?: string[];
    priceRange?: [number, number];
    priceType?: 'all' | 'fixed' | 'contact';
    rating?: number;
    sortBy?: 'newest' | 'price-asc' | 'price-desc' | 'rating' | 'bestseller';
    page?: number;
    limit?: number;
}

// ============ API Response Types ============
export interface PaginatedResponse<T> {
    data: T[];
    total: number;
    page: number;
    limit: number;
    totalPages: number;
}

export interface ApiResponse<T> {
    success: boolean;
    data?: T;
    message?: string;
    errors?: Record<string, string[]>;
}
