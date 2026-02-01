import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { Star, X, Edit3 } from 'lucide-react';
import { Button, Modal } from '@/components/ui';
import { cn } from '@/utils/cn';
import { reviewService, type CreateReviewRequest } from '@/services/reviewService';
import { useToast } from '@/store/toastStore';
import { useAuthStore } from '@/store';

interface ProductReviewButtonProps {
  productCode: string;
  productName: string;
  productImage?: string;
  onReviewSubmitted?: () => void;
}

/**
 * A self-contained "Write Review" button with built-in modal form.
 * Designed for use in Shop ProductDetailPage.
 */
export const ProductReviewButton = ({ 
  productCode, 
  productName, 
  productImage,
  onReviewSubmitted 
}: ProductReviewButtonProps) => {
  const { t } = useTranslation();
  const [showModal, setShowModal] = useState(false);
  const [rating, setRating] = useState(5);
  const [hoverRating, setHoverRating] = useState(0);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const toast = useToast();
  const { user, isAuthenticated } = useAuthStore();

  const { register, handleSubmit, reset, watch, formState: { errors } } = useForm<{ comment: string }>({
    defaultValues: { comment: '' }
  });

  const commentValue = watch('comment', '');

  const handleOpenModal = () => {
    if (!isAuthenticated) {
      toast.warning(t('review.loginRequired', 'Vui lòng đăng nhập để viết đánh giá'));
      return;
    }
    setShowModal(true);
  };

  const handleClose = () => {
    setShowModal(false);
    reset();
    setRating(5);
  };

  const onSubmit = async (data: { comment: string }) => {
    if (!user?.code) {
      toast.error(t('review.userNotFound', 'Không tìm thấy thông tin người dùng'));
      return;
    }

    setIsSubmitting(true);
    try {
      const payload: CreateReviewRequest = {
        productCode: productCode, 
        rating: rating,
        comment: data.comment,
        userCode: user.code
      };

      const res = await reviewService.create(payload);
      if (res.success) {
        toast.success(t('review.submitSuccess', 'Đánh giá của bạn đã được gửi thành công!'));
        handleClose();
        onReviewSubmitted?.();
      } else {
        toast.error(res.message || t('review.submitError', 'Không thể gửi đánh giá'));
      }
    } catch (error) {
      console.error('Review submit error:', error);
      toast.error(t('review.submitError', 'Có lỗi xảy ra khi gửi đánh giá'));
    } finally {
      setIsSubmitting(false);
    }
  };

  const getRatingLabel = (r: number) => {
    const labels = {
      5: t('review.excellent', 'Tuyệt vời!'),
      4: t('review.good', 'Tốt'),
      3: t('review.average', 'Trung bình'),
      2: t('review.poor', 'Kém'),
      1: t('review.terrible', 'Rất tệ')
    };
    return labels[r as keyof typeof labels] || '';
  };

  return (
    <>
      {/* Trigger Button */}
      <Button
        onClick={handleOpenModal}
        variant="outline"
        className="gap-2 border-indigo-500 text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-900/20"
      >
        <Edit3 size={18} />
        {t('review.writeReview', 'Viết đánh giá')}
      </Button>

      {/* Review Modal */}
      <Modal
        isOpen={showModal}
        onClose={handleClose}
        title={t('review.writeReview', 'Viết đánh giá')}
        size="md"
      >
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          {/* Product Info */}
          <div className="flex items-center gap-4 p-4 bg-slate-50 dark:bg-slate-800 rounded-lg">
            {productImage && (
              <img 
                src={productImage} 
                alt={productName} 
                className="w-16 h-16 object-cover rounded-md shadow" 
              />
            )}
            <div className="flex-1 min-w-0">
              <p className="text-xs text-slate-500 uppercase tracking-wide font-medium mb-1">
                {t('product.name', 'Sản phẩm')}
              </p>
              <p className="font-semibold text-slate-800 dark:text-slate-200 truncate">
                {productName}
              </p>
            </div>
          </div>

          {/* Rating Stars */}
          <div className="space-y-3">
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
              {t('review.yourRating', 'Đánh giá của bạn')}
            </label>
            <div 
              className="flex gap-1 justify-center p-4 bg-gradient-to-r from-amber-50 to-yellow-50 dark:from-amber-900/20 dark:to-yellow-900/20 rounded-lg" 
              onMouseLeave={() => setHoverRating(0)}
            >
              {[1, 2, 3, 4, 5].map((star) => (
                <button
                  key={star}
                  type="button"
                  className="focus:outline-none transition-transform hover:scale-125 active:scale-95"
                  onClick={() => setRating(star)}
                  onMouseEnter={() => setHoverRating(star)}
                >
                  <Star
                    size={36}
                    className={`${
                      star <= (hoverRating || rating)
                        ? 'fill-yellow-400 text-yellow-400 drop-shadow-sm'
                        : 'text-gray-300 dark:text-gray-600'
                    } transition-colors duration-150`}
                  />
                </button>
              ))}
            </div>
            <p className="text-center text-sm font-medium text-amber-600 dark:text-amber-400">
              {getRatingLabel(hoverRating || rating)}
            </p>
          </div>

          {/* Comment */}
          <div className="space-y-2">
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
              {t('review.comment', 'Nhận xét')}
              <span className="text-red-500 ml-1">*</span>
            </label>
            <textarea
              {...register('comment', { 
                required: t('review.commentRequired', 'Vui lòng chia sẻ nhận xét của bạn'),
                minLength: { 
                  value: 10, 
                  message: t('review.commentMinLength', 'Tối thiểu 10 ký tự') 
                }
              })}
              placeholder={t('review.commentPlaceholder', 'Chia sẻ trải nghiệm của bạn về sản phẩm này...')}
              className="w-full min-h-[120px] px-4 py-3 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-800 text-slate-800 dark:text-slate-200 placeholder:text-slate-400 focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all resize-none"
            />
            <div className="flex justify-between items-center text-xs">
              {errors.comment ? (
                <p className="text-red-500">{errors.comment.message}</p>
              ) : (
                <p className="text-slate-500">
                  {commentValue.length < 10 
                    ? t('review.minCharsLeft', { count: 10 - commentValue.length }) || `Cần thêm ${10 - commentValue.length} ký tự nữa`
                    : t('review.minLengthReached', 'Đã đủ độ dài tối thiểu')}
                </p>
              )}
              <span className={cn(
                "font-medium",
                commentValue.length < 10 ? "text-slate-400" : "text-emerald-500"
              )}>
                {commentValue.length} ký tự
              </span>
            </div>
          </div>

          {/* Actions */}
          <div className="flex justify-end gap-3 pt-4 border-t border-slate-200 dark:border-slate-700">
            <Button variant="outline" type="button" onClick={handleClose}>
              {t('common.cancel', 'Hủy')}
            </Button>
            <Button 
              type="submit" 
              isLoading={isSubmitting} 
              disabled={isSubmitting}
              className="bg-indigo-600 hover:bg-indigo-700"
            >
              {t('common.submit', 'Gửi đánh giá')}
            </Button>
          </div>
        </form>
      </Modal>
    </>
  );
};

export default ProductReviewButton;
