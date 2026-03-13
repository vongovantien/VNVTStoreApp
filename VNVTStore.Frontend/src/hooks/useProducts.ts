/**
 * Product Hooks Barrel File
 * Re-exports the split hook modules to maintain backward compatibility.
 */

// Keys
export { productKeys } from './products/keys';

// Mappers
export { mapProductDtoToProduct, getProductImages } from './products/productMapper';

// Queries
export { useProducts, useProduct, useCategories, useCategoriesList, useInfiniteProducts } from './products/useProductQueries';

// Mutations
export { useCreateProduct, useUpdateProduct, useDeleteProduct } from './products/useProductMutations';
