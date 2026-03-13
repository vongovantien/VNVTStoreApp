import { useState, useMemo, useCallback, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useCartStore, useWishlistStore, useCompareStore, useToast, useRecentStore } from '@/store';
import { useProduct, useProducts } from '@/hooks';
import { ProductDetailType } from '@/types';

export const useProductDetail = (id: string | undefined) => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const [quantity, setQuantity] = useState(1);
    const [isAddingToCart, setIsAddingToCart] = useState(false);
    const [activeTab, setActiveTab] = useState<'description' | 'specs' | 'units' | 'variants' | 'images' | 'reviews' | 'qa'>('description');
    const [selectedIndex, setSelectedIndex] = useState(0);
    const [lightboxOpen, setLightboxOpen] = useState(false);

    // Store actions
    const addToCart = useCartStore((state) => state.addItem);
    const { addItem: addToWishlist, removeItem: removeFromWishlist, isInWishlist } = useWishlistStore();
    const { addItem: addToCompare, isInCompare } = useCompareStore();
    const { success, error: toastError } = useToast();
    const addToRecent = useRecentStore((state) => state.addToRecent);

    // Fetch product from API
    const { data: product, isLoading, isError, error } = useProduct(id || '');

    // Add to recent viewed
    useEffect(() => {
        if (product) {
            addToRecent(product);
        }
    }, [product, addToRecent]);

    // Fetch related products (same category)
    const { data: relatedData } = useProducts({
        pageIndex: 1,
        pageSize: 8,
        enabled: !!product?.categoryCode,
    });

    const relatedProducts = useMemo(
        () => (relatedData?.products || []).filter((p) => p.code !== id).slice(0, 4),
        [relatedData?.products, id]
    );

    // Group: RELATION for accessories/cross-selling
    const relations = useMemo(() =>
        product?.details?.filter(d => d.detailType === ProductDetailType.RELATION) || [],
        [product?.details]
    );

    // Derived states
    const isWishlisted = product ? isInWishlist(product.code) : false;
    const isCompared = product ? isInCompare(product.code) : false;
    const hasFixedPrice = product ? (product.price > 0) : false;
    const images = useMemo(() =>
        (product ? (product.images?.length ? product.images : (product.image ? [product.image] : [])) : []).filter(img => img && img.length > 0),
        [product]
    );

    const stockStatus = useMemo(() => {
        if (!product) return null;
        const stock = product.stock || 0;
        const minStock = product.minStockLevel || 5;

        if (stock <= 0) return { label: t('product.outOfStock'), color: 'error' as const, text: 'text-red-600' };
        if (stock <= minStock) return { label: t('product.lowStock') || 'Sắp hết hàng', color: 'warning' as const, text: 'text-orange-600' };
        return { label: `${t('product.inStock')} (${stock})`, color: 'success' as const, text: 'text-green-600' };
    }, [product, t]);

    // Handlers
    const handleAddToCart = useCallback(async () => {
        if (product && hasFixedPrice) {
            setIsAddingToCart(true);
            try {
                void addToCart(product, quantity);
                success(t('product.addToCartSuccess') || 'Đã thêm vào giỏ hàng');
            } catch (err) {
                console.error(err);
                toastError(t('product.addToCartError') || 'Có lỗi xảy ra');
            } finally {
                setIsAddingToCart(false);
            }
        }
    }, [product, hasFixedPrice, quantity, addToCart, success, toastError, t]);

    const handleBuyNow = useCallback(async () => {
        if (product && hasFixedPrice) {
            setIsAddingToCart(true);
            try {
                await addToCart(product, quantity);
                success(t('product.addToCartSuccess') || 'Đã thêm vào giỏ hàng');
                navigate('/checkout');
            } catch (err) {
                console.error(err);
                toastError(t('product.addToCartError') || 'Có lỗi xảy ra');
            } finally {
                setIsAddingToCart(false);
            }
        }
    }, [product, hasFixedPrice, quantity, addToCart, success, toastError, t, navigate]);

    const handleWishlistToggle = useCallback(() => {
        if (product) {
            if (isWishlisted) {
                removeFromWishlist(product.code);
            } else {
                addToWishlist(product);
            }
        }
    }, [product, isWishlisted, addToWishlist, removeFromWishlist]);

    const handleCompareToggle = useCallback(() => {
        if (product) {
            addToCompare(product);
        }
    }, [product, addToCompare]);

    return {
        product,
        isLoading,
        isError,
        error,
        quantity,
        setQuantity,
        isAddingToCart,
        activeTab,
        setActiveTab,
        selectedIndex,
        setSelectedIndex,
        lightboxOpen,
        setLightboxOpen,
        relatedProducts,
        relations,
        isWishlisted,
        isCompared,
        hasFixedPrice,
        images,
        stockStatus,
        handleAddToCart,
        handleBuyNow,
        handleWishlistToggle,
        handleCompareToggle
    };
};
