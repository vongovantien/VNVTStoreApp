import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Star, User } from 'lucide-react';
import { useAuthStore } from '@/store';
import { UserRole } from '@/types';
import { reviewService, type ReviewDto } from '@/services/reviewService';
import { formatDate } from '@/utils/format';
import { Pagination } from '@/components/ui';
import { useReviewStore } from '@/store/reviewStore';
import ReviewForm from './ReviewForm';

interface ReviewsListProps {
  productCode: string;
}

const ReviewItem = ({ 
  review, 
  onReply, 
  isReply = false 
}: { 
  review: ReviewDto; 
  onReply?: (review: ReviewDto) => void;
  isReply?: boolean;
}) => {
  const { t } = useTranslation();
  const { user } = useAuthStore();
  const isAdmin = user?.role === UserRole.Admin;
  
  return (
    <div className={`${isReply ? 'ml-8 mt-4' : 'bg-primary p-4 rounded-xl border border-secondary/20'} transition-all`}>
      <div className="flex justify-between items-start mb-2">
        <div className="flex items-center gap-2">
          {review.userAvatar ? (
             <img 
               src={review.userAvatar} 
               alt={review.userName || 'User'} 
               className="w-8 h-8 rounded-full object-cover border border-secondary/10" 
             />
          ) : (
            <div className="bg-secondary/20 p-2 rounded-full">
              <User size={16} className="text-secondary" />
            </div>
          )}
          <div>
            <p className="font-semibold text-sm">{review.userName || 'Anonymous'}</p>
            <p className="text-xs text-tertiary">{formatDate(review.createdAt)}</p>
          </div>
        </div>
        {!isReply && (
          <div className="flex items-center gap-0.5">
            <span className="font-bold text-lg mr-1">{review.rating}</span>
            <Star size={16} className="fill-yellow-400 text-yellow-400" />
          </div>
        )}
      </div>
      
      <div className={`${isReply ? 'pl-2 border-l-2 border-secondary/20' : 'pl-10'}`}>
         <p className="text-secondary mb-2 whitespace-pre-wrap">{review.comment}</p>
         
         {isAdmin && (
           <button 
             onClick={() => onReply?.(review)}
             className="text-xs font-semibold text-indigo-500 hover:text-indigo-600 transition-colors"
           >
             {t('common.reply') || 'Reply'}
           </button>
         )}

         {review.replies && review.replies.length > 0 && (
           <div className="space-y-4 pt-2">
             {review.replies.map(reply => (
               <ReviewItem 
                 key={reply.code} 
                 review={reply} 
                 isReply={true} 
                 onReply={onReply}
               />
             ))}
           </div>
         )}
      </div>
    </div>
  );
};

const ReviewsList = ({ productCode }: ReviewsListProps) => {
  const { t } = useTranslation();
  const [reviews, setReviews] = useState<ReviewDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [pageIndex, setPageIndex] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [replyingTo, setReplyingTo] = useState<ReviewDto | null>(null);
  
  const pageSize = 5;
  const refreshTrigger = useReviewStore(state => state.refreshTrigger);
  const triggerRefresh = useReviewStore(state => state.triggerRefresh);

  const fetchReviews = useCallback(async () => {
    setLoading(true);
    try {
      const res = await reviewService.getByProduct(productCode, { pageIndex, pageSize });
      if (res.success && res.data) {
        setReviews(res.data.items || []);
        setTotalCount(res.data.totalItems || 0);
      }
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  }, [productCode, pageIndex, pageSize]);

  useEffect(() => {
    if (productCode) {
      fetchReviews();
    }
  }, [productCode, fetchReviews, refreshTrigger]);

  const handleReplySuccess = () => {
    setReplyingTo(null);
    triggerRefresh();
  };

  return (
    <div className="space-y-8">
      <h3 className="text-2xl font-bold mb-6">{t('product.reviews') || 'Customer Reviews'}</h3>
      
      {loading ? (
        <div className="text-center py-8 text-secondary">Loading reviews...</div>
      ) : reviews.length === 0 ? (
        <div className="text-center py-8 text-secondary italic">
          {t('product.noReviews') || 'No reviews yet for this product.'}
        </div>
      ) : (
        <div className="space-y-6">
          {reviews.map((review) => (
            <ReviewItem 
              key={review.code} 
              review={review} 
              onReply={setReplyingTo}
            />
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

      {/* Reply Modal/Form could be injected here or as a floating element */}
      {replyingTo && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50 animate-in fade-in duration-200">
           <div className="w-full max-w-lg">
              <ReviewForm 
                orderItemCode={undefined} // Not required for replies
                productName={t('product.replying_to', { name: replyingTo.userName }) || `Replying to ${replyingTo.userName}`}
                userCode="" // Handled inside ReviewForm usually or by service
                parentCode={replyingTo.code}
                onSuccess={handleReplySuccess}
                onCancel={() => setReplyingTo(null)}
              />
           </div>
        </div>
      )}
    </div>
  );
};

export default ReviewsList;
