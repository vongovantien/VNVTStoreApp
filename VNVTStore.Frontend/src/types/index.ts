// ============ Product Types ============
export interface Product {
    code: string;
    name: string;
    slug: string;
    description: string;
    price: number;
    wholesalePrice?: number; // New
    costPrice?: number; // New
    originalPrice?: number;
    discount?: number;
    image: string;
    images?: string[];
    productImages?: ProductImage[];
    category: string;
    categoryCode?: string;
    brand?: string;
    brandCode?: string; // New
    supplierCode?: string; // New
    supplierName?: string; // New
    stock: number;
    stockQuantity?: number; // Sync with backend name
    minStockLevel?: number; // New
    binLocation?: string; // New
    vatRate?: number; // New
    countryOfOrigin?: string; // New
    baseUnit?: string; // New
    weight?: number;
    color?: string;
    power?: string;
    voltage?: string;
    material?: string;
    size?: string;
    rating?: number;
    averageRating?: number;
    reviewCount: number;
    isFeatured?: boolean;
    isNew?: boolean;
    isActive?: boolean;
    createdAt: string;
    updatedAt?: string;

    // Relations
    details?: ProductDetail[];
    productUnits?: ProductUnit[];
    tags?: ProductTag[];
    variants?: ProductVariant[];
    promotionEndDate?: string;
    soldCount?: number; // New
    promotionOriginalQuantity?: number; // New
    isTrending?: boolean; // New
    isBestseller?: boolean; // New
    videoURL?: string; // New
    wholesaleTiers?: { minQuantity: number; price: number }[]; // New
    warrantyInfo?: string; // New
    currencyCode?: string; // New
    estimatedDeliveryDays?: number; // New
    promoLabel?: string; // New: Feature 31
    abandonedCartRecoveryEnabled?: boolean; // New: Feature 32
    frequentlyBoughtTogether?: string[]; // New: Feature 33
    viewCount?: number;
    soldCount24h?: number;
    videoReviews?: { user: string; videoUrl: string }[]; // New: Feature 40
}

export enum ProductDetailType {
    SPEC = 'SPEC',
    LOGISTICS = 'LOGISTICS',
    RELATION = 'RELATION',
    IMAGE = 'IMAGE'
}


export interface ProductDetail {
    code: string;
    productCode: string;
    detailType: ProductDetailType;
    specName: string;
    specValue: string;
}

export interface ProductUnit {
    code: string;
    productCode: string;
    unitName: string;
    conversionRate: number;
    price: number;
    isActive?: boolean;
    isBaseUnit?: boolean;
}

export interface ProductTag {
    productCode: string;
    tagCode: string;
}

export interface ProductImage {
    code: string;
    imageURL?: string;
    isPrimary?: boolean;
}

export interface ProductVariant {
    code: string;
    productCode: string;
    sku: string;
    attributes: string; // JSON string
    price: number;
    stockQuantity: number;
    isActive: boolean;
}

// ============ Tag Types ============
export interface Tag {
    id: number;
    name: string;
    isActive: boolean;
}

// ============ Category Types ============
export interface Category {
    code: string; // Changed from id
    name: string;
    slug: string; // Is slug used?
    description?: string;
    image?: string;
    parentCode?: string; // Changed from parentId
    productCount: number;
    isActive?: boolean;
    imageURL?: string;
}

// ============ Cart Types ============
export interface CartItem {
    code: string;
    product: Product;
    quantity: number;
    color?: string;
    size?: string;
}

// ============ User Types ============
export enum UserRole {
    Admin = 'Admin',
    Customer = 'Customer',
    Staff = 'Staff'
}

export enum UserStatus {
    Active = 'Active',
    Inactive = 'Inactive',
    Locked = 'Locked',
    Pending = 'Pending'
}

export interface User {
    code: string; // Users also use Code in DTO? Check UserDto. Yes.
    email: string;
    fullName: string;
    username?: string;
    phone?: string;
    avatar?: string;
    role: UserRole;
    roleCode?: string;
    roleName?: string;
    permissions?: string[];
    menus?: string[];
    addresses?: Address[];
    loyaltyPoints?: number;
    userTier?: string; // NEW, LOYAL, VIP
    debtLimit?: number;
    currentDebt?: number;
    status: UserStatus;
    createdAt: string;
}

export interface Address {
    code: string;
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
    code: string;
    orderNumber: string;
    userCode: string; // userId -> userCode
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
    productCode: string; // productId -> productCode
    productName: string;
    productImage: string;
    price: number;
    quantity: number;
}

// ============ Review Types ============
export interface Review {
    code: string;
    productCode: string;
    userCode: string; // userId -> userCode
    userName: string;
    userAvatar?: string;
    rating: number;
    title?: string;
    content: string;
    images?: string[];
    createdAt: string;
    helpful?: number;
}

export interface ProductQA {
    code: string;
    productCode: string;
    userCode: string;
    userName: string;
    userAvatar?: string;
    comment: string;
    parentCode?: string;
    replies?: ProductQA[];
    likes?: number;
    createdAt: string;
    isApproved: boolean;
}

// ============ Quote Request Types ============
export type QuoteStatus = 'pending' | 'quoted' | 'closed' | 'cancelled';

export interface QuoteRequest {
    code: string;
    productCode: string;
    productName: string;
    productImage: string;
    customer: {
        name: string;
        email: string;
        phone: string;
        company?: string;
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

// ============ Menu Types ============
export interface Menu {
    code: string;
    name: string;
    path: string;
    groupCode: string;
    groupName: string;
    icon?: string;
    sortOrder: number;
    isActive: boolean;
}

// ============ RBAC Types ============
export interface Permission {
    code: string;
    name: string;
    module: string;
    description?: string;
}

export interface Role {
    code: string;
    name: string;
    description?: string;
    isActive: boolean;
    permissions: Permission[];
    menus: Menu[];
}

export interface RolePermission {
    roleCode: string;
    permissionCode: string;
}
