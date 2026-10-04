/**
 * Frontend enums — mirrors VNVTStore.Domain.Enums (C#).
 * Single source of truth for all magic strings. Import from here instead of
 * using raw string literals scattered across the codebase.
 *
 * Keep in sync with:
 *   VNVTStore.Backend/src/VNVTStore.Domain/Enums/Enums.cs
 */

// ─── Order ───────────────────────────────────────────────────────────────────

export enum OrderStatus {
  Pending    = 'pending',
  Confirmed  = 'confirmed',
  Paid       = 'paid',
  Processing = 'processing',
  Shipping   = 'shipping',
  Delivered  = 'delivered',
  Completed  = 'completed',
  Cancelled  = 'cancelled',
  Refunded   = 'refunded',
}

// ─── Delivery ─────────────────────────────────────────────────────────────────

export enum DeliveryStatus {
  Assigned       = 'assigned',
  PickedUp       = 'pickedup',
  InTransit      = 'intransit',
  AtHub          = 'athub',
  OutForDelivery = 'outfordelivery',
  Delivered      = 'delivered',
  Failed         = 'failed',
  Returned       = 'returned',
}

// ─── Payment ──────────────────────────────────────────────────────────────────

export enum PaymentStatus {
  Pending   = 'pending',
  Completed = 'completed',
  Failed    = 'failed',
  Refunded  = 'refunded',
  Cancelled = 'cancelled',
}

export enum PaymentMethod {
  Cash         = 'cash',
  CreditCard   = 'creditcard',
  DebitCard    = 'debitcard',
  BankTransfer = 'banktransfer',
  COD          = 'cod',
  EWallet      = 'ewallet',
  PayPal       = 'paypal',
  VnPay        = 'vnpay',
  MoMo         = 'momo',
  ZaloPay      = 'zalopay',
}

// ─── User ─────────────────────────────────────────────────────────────────────

export enum UserRole {
  Customer = 'Customer',
  Admin    = 'Admin',
  Staff    = 'Staff',
}

export enum UserStatus {
  Active   = 'Active',
  Inactive = 'Inactive',
  Banned   = 'Banned',
  Pending  = 'Pending',
  Locked   = 'Banned',
}

export enum UserTier {
  New   = 'NEW',
  Loyal = 'LOYAL',
  Vip   = 'VIP',
}

// ─── Product ──────────────────────────────────────────────────────────────────

/** Maps to ProductDetailType enum on the backend. */
export enum ProductDetailType {
  Spec      = 'SPEC',
  Logistics = 'LOGISTICS',
  Relation  = 'RELATION',
  Image     = 'IMAGE',
}

// ─── Promotion / Coupon ───────────────────────────────────────────────────────

export enum DiscountType {
  Percentage = 'PERCENTAGE',
  Fixed      = 'FIXED',
}

// ─── Quote ────────────────────────────────────────────────────────────────────

export enum QuoteStatus {
  Pending            = 'pending',
  Approved           = 'approved',
  Rejected           = 'rejected',
  ConvertedToOrder   = 'convertedtoorder',
}

// ─── Record modification ──────────────────────────────────────────────────────

/** Mirrors ModificationType enum — used in ModifiedType column. */
export enum ModificationType {
  Add    = 'ADD',
  Update = 'UPDATE',
  Delete = 'DELETE',
}

// ─── Search / Filter ──────────────────────────────────────────────────────────

/**
 * Maps to SearchCondition enum in the backend RequestDTO.
 * Used when building search payloads for the generic paged query API.
 */
export enum SearchCondition {
  Equal            = 0,
  NotEqual         = 1,
  Contains         = 2,
  GreaterThan      = 3,
  GreaterThanEqual = 4,
  LessThan         = 5,
  LessThanEqual    = 6,
  DateTimeRange    = 7,
  DayPart          = 8,
  MonthPart        = 9,
  DatePart         = 10,
  IsNull           = 11,
  IsNotNull        = 12,
  In               = 13,
  NotIn            = 14,
  EqualExact       = 15,
}

// ─── Helper maps (label lookups) ──────────────────────────────────────────────

export const ORDER_STATUS_LABEL: Record<OrderStatus, string> = {
  [OrderStatus.Pending]:    'Chờ xác nhận',
  [OrderStatus.Confirmed]:  'Đã xác nhận',
  [OrderStatus.Paid]:       'Đã thanh toán',
  [OrderStatus.Processing]: 'Đang xử lý',
  [OrderStatus.Shipping]:   'Đang giao',
  [OrderStatus.Delivered]:  'Đã giao',
  [OrderStatus.Completed]:  'Hoàn thành',
  [OrderStatus.Cancelled]:  'Đã huỷ',
  [OrderStatus.Refunded]:   'Đã hoàn tiền',
};

export const ORDER_STATUS_COLOR: Record<OrderStatus, 'warning' | 'info' | 'success' | 'error' | 'secondary'> = {
  [OrderStatus.Pending]:    'warning',
  [OrderStatus.Confirmed]:  'info',
  [OrderStatus.Paid]:       'info',
  [OrderStatus.Processing]: 'info',
  [OrderStatus.Shipping]:   'info',
  [OrderStatus.Delivered]:  'success',
  [OrderStatus.Completed]:  'success',
  [OrderStatus.Cancelled]:  'error',
  [OrderStatus.Refunded]:   'secondary',
};

export const PAYMENT_METHOD_LABEL: Record<PaymentMethod, string> = {
  [PaymentMethod.Cash]:         'Tiền mặt',
  [PaymentMethod.CreditCard]:   'Thẻ tín dụng',
  [PaymentMethod.DebitCard]:    'Thẻ ghi nợ',
  [PaymentMethod.BankTransfer]: 'Chuyển khoản',
  [PaymentMethod.COD]:          'Thanh toán khi nhận hàng (COD)',
  [PaymentMethod.EWallet]:      'Ví điện tử',
  [PaymentMethod.PayPal]:       'PayPal',
  [PaymentMethod.VnPay]:        'VNPay',
  [PaymentMethod.MoMo]:         'MoMo',
  [PaymentMethod.ZaloPay]:      'ZaloPay',
};

export const PRODUCT_DETAIL_TYPE_LABEL: Record<ProductDetailType, string> = {
  [ProductDetailType.Spec]:      'Thông số kỹ thuật',
  [ProductDetailType.Logistics]: 'Vận chuyển',
  [ProductDetailType.Relation]:  'Sản phẩm liên quan',
  [ProductDetailType.Image]:     'Hình ảnh',
};
