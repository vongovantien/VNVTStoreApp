/**
 * Services barrel export
 */

// API Core exports - including new AsyncState discriminated union
export {
    apiClient,
    type ApiResponse,
    type PagedResult,
    type RequestDTO,
    type AsyncState,
    AsyncState as AsyncStateFactory,
    isIdle,
    isLoading,
    isSuccess,
    isError,
} from './api';

// Entity Services
export { productService, categoryService, type CategoryDto, type CreateCategoryRequest, type UpdateCategoryRequest } from './productService';
export { brandService, type BrandDto, type CreateBrandRequest, type UpdateBrandRequest } from './brandService';
export { authService } from './authService';
export { cartService, type CartDto } from './cartService';
export { userService, type UserProfileDto, type AddressDto } from './userService';
export { orderService, type OrderDto, type CreateOrderRequest } from './orderService';
export { paymentService } from './paymentService';
export { reviewService, type ReviewDto, type CreateReviewRequest } from './reviewService';
export { supplierService, type SupplierDto, type CreateSupplierRequest, type UpdateSupplierRequest } from './supplierService';
export { customerService, type CustomerDto, type CreateCustomerRequest, type UpdateCustomerRequest } from './customerService';
export { dashboardService, type DashboardStatsDto } from './dashboardService';
export { quoteService } from './quoteService';
export { unitService, type CatalogUnitDto, type CreateUnitRequest, type UpdateUnitRequest } from './unitService';
export { bannerService } from './bannerService';
export { newsService } from './newsService';
export { promotionService } from './promotionService';
export { couponService } from './couponService';
export { roleService } from './roleService';
export { permissionService } from './permissionService';

