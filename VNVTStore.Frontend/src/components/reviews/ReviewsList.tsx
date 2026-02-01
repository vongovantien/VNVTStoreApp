import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Star, ThumbsUp, MessageSquare, User } from 'lucide-react';
import { reviewService, type ReviewDto } from '@/services/reviewService';
import SharedImage from '@/components/common/Image';
import { formatDate } from '@/utils/format';
import { Pagination } from '@/components/ui';

interface ReviewsListProps {
  productCode: string;
}

const ReviewsList = ({ productCode }: ReviewsListProps) => {
  const { t } = useTranslation();
  const [reviews, setReviews] = useState<ReviewDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [pageIndex, setPageIndex] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 5;

  const fetchReviews = async () => {
    setLoading(true);
    try {
      const res = await reviewService.getByProduct(productCode, pageIndex, pageSize);
      if (res.success && res.data) {
        setReviews(res.data.items || []);
        setTotalCount(res.data.totalItems || 0);
      }
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (productCode) {
      fetchReviews();
    }
  }, [productCode, pageIndex]);

  return (
    <div className="space-y-8">
      <h3 className="text-2xl font-bold mb-6">{t('product.reviews') || 'Customer Reviews'}</h3>
      
      {loading ? (
        <div className="text-center py-8 text-secondary">Loading reviews...</div>
      ) : reviews.length === 0 ? (
        <div className="text-center py-8 text-secondary italic">
          {t('product.no_reviews') || 'No reviews yet for this product.'}
        </div>
      ) : (
        <div className="space-y-6">
          {reviews.map((review) => (
            <div key={review.code} className="bg-primary p-4 rounded-xl border border-secondary/20">
              <div className="flex justify-between items-start mb-2">
                <div className="flex items-center gap-2">
                  <div className="bg-secondary/20 p-2 rounded-full">
                    <User size={16} className="text-secondary" />
                  </div>
                  <div>
                    <p className="font-semibold text-sm">{review.userName || 'Anonymous'}</p>
                    <p className="text-xs text-tertiary">{formatDate(review.createdAt)}</p>
                  </div>
                </div>
                <div className="flex items-center gap-0.5">
                  <span className="font-bold text-lg mr-1">{review.rating}</span>
                  <Star size={16} className="fill-yellow-400 text-yellow-400" />
                </div>
              </div>
              
              <div className="pl-10">
                 <p className="text-secondary mb-3">{review.comment}</p>
                 
                 {review.adminReply && (
                   <div className="bg-tertiary/10 p-3 rounded-lg border-l-2 border-indigo-500 mt-3">
                     <p className="text-xs font-bold text-indigo-600 mb-1">Admin Reply</p>
                     <p className="text-sm text-secondary">{review.adminReply}</p>
                   </div>
                 )}
              </div>
            </div>
          ))}
          
          {totalCount > pageSize && (
             <Pagination
                currentPage={pageIndex}
                totalPages={Math.ceil(totalCount / pageSize)}
                totalItems={totalCount}
                pageSize={pageSize}
                onPageChange={setPageIndex}
             />
          )}
        </div>
      )}
    </div>
  );
};

export default ReviewsList;
