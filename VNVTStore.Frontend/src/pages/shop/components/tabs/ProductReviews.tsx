import React from 'react';
import { useTranslation } from 'react-i18next';
import { ProductReviewButton } from '@/components/reviews/ProductReviewButton';
import ReviewsList from '@/components/reviews/ReviewsList';
import { useReviewStore } from '@/store/reviewStore';
import type { Product } from '@/types';

interface ProductReviewsProps {
  product: Product;
}

export const ProductReviews: React.FC<ProductReviewsProps> = ({ product }) => {
  const { t } = useTranslation();

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between bg-gradient-to-r from-indigo-50 to-purple-50 dark:from-indigo-900/20 dark:to-purple-900/20 p-4 rounded-xl">
        <div>
          <h3 className="font-semibold text-lg text-slate-800 dark:text-slate-200">
            {t('review.shareExperience', 'Chia sẻ trải nghiệm của bạn')}
          </h3>
          <p className="text-sm text-slate-500 dark:text-slate-400">
            {t('review.helpOthers', 'Đánh giá của bạn sẽ giúp người khác tham khảo')}
          </p>
        </div>
        <ProductReviewButton
          productCode={product.code}
          productName={product.name}
          productImage={product.images?.[0]}
          onReviewSubmitted={() => {
            useReviewStore.getState().triggerRefresh();
          }}
        />
      </div>
      <ReviewsList productCode={product.code} />
    </div>
  );
};
