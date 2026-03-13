/**
 * Default Fields for List Views
 * Reduces data transfer by only fetching necessary columns
 * 
 * NOTE: All LIST_FIELDS extend COMMON_FIELDS for consistency
 */

// Common fields shared by all entities
export const COMMON_FIELDS = [
    'Code',
    'IsActive',
    'CreatedAt',
];

// Common audit fields (for detail views)
export const AUDIT_FIELDS = [
    'CreatedAt',
    'UpdatedAt',
    'CreatedBy',
    'ModifiedBy',
];

// ============ List View Fields (extend COMMON_FIELDS) ============

// Product fields for list view
export const PRODUCT_LIST_FIELDS = [
    ...COMMON_FIELDS,
    'Name',
    'Description',
    'Price',
    'CategoryName',
    'CategoryCode',
    'Brand',
    'StockQuantity',
    'ProductImages'
];

// Product fields for detail/edit view
export const PRODUCT_DETAIL_FIELDS = [
    ...PRODUCT_LIST_FIELDS,
    'WholesalePrice',
    'CostPrice',
    'Weight',
    'Color',
    'Power',
    'Voltage',
    'Material',
    'Size',
    'CountryOfOrigin',
    'BaseUnit',
    'BinLocation',
    'VatRate',
    'MinStockLevel',
    'SupplierCode',
    'SupplierName',
    'BrandCode',
    'ProductImages',
    'ProductUnits',
    'Details',
    'Variants',
];

// Category fields
export const CATEGORY_LIST_FIELDS = [
    ...COMMON_FIELDS,
    'Name',
    'Description',
    'Files',
    'ParentCode',
    'ParentName',
];

// Supplier fields
export const SUPPLIER_LIST_FIELDS = [
    ...COMMON_FIELDS,
    'Name',
    'Email',
    'Phone',
    'Address',
];

// Order fields
export const ORDER_LIST_FIELDS = [
    ...COMMON_FIELDS,
    'UserCode',
    'UserName',
    'TotalAmount',
    'Status',
    'PaymentMethod',
];

// Coupon fields
export const COUPON_LIST_FIELDS = [
    ...COMMON_FIELDS,
    'DiscountType',
    'DiscountValue',
    'MinOrderAmount',
    'MaxUsage',
    'CurrentUsage',
    'ExpiryDate',
];

// Banner fields
export const BANNER_LIST_FIELDS = [
    ...COMMON_FIELDS,
    'Title',
    'ImageUrl',
    'Link',
    'Position',
];

// Review fields
export const REVIEW_LIST_FIELDS = [
    ...COMMON_FIELDS,
    'ProductCode',
    'ProductName',
    'UserCode',
    'UserName',
    'Rating',
    'Comment',
];

// Quote fields
export const QUOTE_LIST_FIELDS = [
    ...COMMON_FIELDS,
    'ProductCode',
    'ProductName',
    'UserCode',
    'Quantity',
    'Status',
];

// Promotion fields
export const PROMOTION_LIST_FIELDS = [
    ...COMMON_FIELDS,
    'Name',
    'DiscountType',
    'DiscountValue',
    'StartDate',
    'EndDate',
];

// User fields
export const USER_LIST_FIELDS = [
    ...COMMON_FIELDS,
    'Username',
    'FullName',
    'Email',
    'Phone',
    'Role',
    'IsEmailVerified',
    'LastLogin',
];
