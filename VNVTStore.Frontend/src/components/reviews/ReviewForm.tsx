import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { Star, X } from 'lucide-react';
import { Button, Textarea } from '@/components/ui';
import { reviewService as reviewApi, type CreateReviewRequest } from '@/services/reviewService';
import { useToast } from '@/store/toastStore';

interface ReviewFormProps {
  orderItemCode?: string;
  productName: string;
  productImage?: string;
  userCode: string;
  parentCode?: string;
  onSuccess: () => void;
  onCancel: () => void;
}

const ReviewForm = ({ orderItemCode, productName, productImage, userCode, parentCode, onSuccess, onCancel }: ReviewFormProps) => {
  const { t } = useTranslation();
  const [rating, setRating] = useState(5);
  const [hoverRating, setHoverRating] = useState(0);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const toast = useToast();

  const { register, handleSubmit, formState: { errors } } = useForm<CreateReviewRequest>({
    defaultValues: {
      rating: 5,
      comment: '',
    }
  });

  const onSubmit = async (data: CreateReviewRequest) => {
    setIsSubmitting(true);
    try {
      let res;
      if (parentCode) {
        res = await reviewApi.reply(parentCode, data.comment);
      } else {
        const payload: CreateReviewRequest = {
          orderItemCode: orderItemCode,
          rating: rating,
          comment: data.comment,
          userCode: userCode,
        };
        res = await reviewApi.create(payload);
      }
      if (res.success) {
        toast.success(t('review.submit_success') || 'Review submitted successfully!');
        onSuccess();
      } else {
        toast.error(res.message || t('review.submit_error') || 'Failed to submit review');
      }
    } catch (error) {
      console.error(error);
      toast.error(t('review.submit_error') || 'An error occurred');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="bg-primary p-6 rounded-xl w-full max-w-lg mx-auto relative">
      <button 
        onClick={onCancel}
        className="absolute top-4 right-4 text-tertiary hover:text-primary transition-colors"
      >
        <X size={24} />
      </button>

      <h3 className="text-xl font-bold mb-4">{t('review.write_review') || 'Write a Review'}</h3>
      
      <div className="flex items-center gap-4 mb-6 p-4 bg-tertiary rounded-lg">
        {productImage && (
          <img 
            src={productImage} 
            alt={productName} 
            className="w-16 h-16 object-cover rounded-md" 
          />
        )}
        <div>
          <p className="font-semibold text-sm text-tertiary uppercase tracking-wide">{t('product.name')}</p>
          <p className="font-medium">{productName}</p>
        </div>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        {/* Rating Stars */}
        {!parentCode && (
          <div className="space-y-2">
            <label className="block text-sm font-medium">{t('review.rating') || 'Rating'}</label>
            <div className="flex gap-1" onMouseLeave={() => setHoverRating(0)}>
              {[1, 2, 3, 4, 5].map((star) => (
                <button
                  key={star}
                  type="button"
                  className="focus:outline-none transition-transform hover:scale-110"
                  onClick={() => setRating(star)}
                  onMouseEnter={() => setHoverRating(star)}
                >
                  <Star
                    size={32}
                    className={`${
                      star <= (hoverRating || rating)
                        ? 'fill-yellow-400 text-yellow-400'
                        : 'text-gray-300'
                    } transition-colors`}
                  />
                </button>
              ))}
            </div>
            <p className="text-sm text-tertiary">
              {rating === 5 ? 'Excellent!' : 
               rating === 4 ? 'Good' : 
               rating === 3 ? 'Average' : 
               rating === 2 ? 'Poor' : 'Terrible'}
            </p>
          </div>
        )}

        {/* Comment */}
        <div className="space-y-2">
          <label className="block text-sm font-medium">{t('review.comment') || 'Comment'}</label>
          <Textarea
            {...register('comment', { 
              required: t('review.comment_required') || 'Please share your experience',
              minLength: { value: 10, message: t('review.comment_min_length') || 'Minimum 10 characters' }
            })}
            placeholder={t('review.comment_placeholder') || "Tell us what you liked or didn't like about this product..."}
            className="min-h-[120px]"
            error={errors.comment?.message}
          />
        </div>

        <div className="flex justify-end gap-3 pt-4 border-t">
          <Button variant="outline" type="button" onClick={onCancel}>
            {t('common.cancel')}
          </Button>
          <Button type="submit" isLoading={isSubmitting} disabled={isSubmitting}>
            {t('common.submit')}
          </Button>
        </div>
      </form>
    </div>
  );
};

export default ReviewForm;
