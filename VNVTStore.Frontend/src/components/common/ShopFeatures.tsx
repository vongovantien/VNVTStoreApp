/**
 * Feature #57: Newsletter Signup in Footer
 * Feature #79: Share Sheet Integration (Web Share API fallback)
 * Feature #56: Gamified Spin-to-Win coupon wheel
 * Feature #88: Custom 404 Page with product recommendations
 * Feature #60: Post-Purchase Upsell suggestions
 * Feature #23: Gift Message Option
 * Feature #36: Re-order Button
 * Feature #22: Cart Progress Bar (free shipping threshold)
 * Feature #41: Q&A Section
 * Feature #45: Helpfulness Voting
 * Feature #43: Review Search
 * Feature #13: Before/After Image Slider
 */

// ============ #57 Newsletter Signup ============
import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { Mail, Check, Send, Share2, Copy, Gift, MessageSquare, ThumbsUp, ThumbsDown, Search, ChevronLeft, ChevronRight } from 'lucide-react';

export const NewsletterSignup = () => {
  const { t } = useTranslation();
  const [email, setEmail] = useState('');
  const [subscribed, setSubscribed] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.includes('@')) return;
    // Store locally (no third party)
    const subs = JSON.parse(localStorage.getItem('vnvt_newsletter') || '[]');
    if (!subs.includes(email)) {
      subs.push(email);
      localStorage.setItem('vnvt_newsletter', JSON.stringify(subs));
    }
    setSubscribed(true);
  };

  if (subscribed) {
    return (
      <div className="text-center p-4">
        <div className="w-12 h-12 mx-auto mb-2 bg-green-100 rounded-full flex items-center justify-center">
          <Check size={24} className="text-green-600" />
        </div>
        <p className="text-sm font-medium text-green-600">
          {t('newsletter.success', 'Đăng ký thành công!')}
        </p>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="flex gap-2">
      <div className="relative flex-1">
        <Mail size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary" />
        <input
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder={t('newsletter.placeholder', 'Email của bạn...')}
          className="w-full pl-9 pr-3 py-2.5 bg-white/10 border border-white/20 rounded-xl text-sm text-white placeholder-white/50 focus:outline-none focus:border-white/50"
          required
        />
      </div>
      <button type="submit" className="px-4 py-2.5 bg-accent-primary hover:bg-accent-primary/90 text-white rounded-xl text-sm font-medium flex items-center gap-1.5 transition-colors shrink-0">
        <Send size={14} />
        {t('newsletter.subscribe', 'Đăng ký')}
      </button>
    </form>
  );
};

// ============ #79 Share Button (Web Share API) ============
export const ShareButton = ({ 
  title, 
  text, 
  url,
  className = '' 
}: { 
  title: string; 
  text?: string; 
  url?: string;
  className?: string;
}) => {
  const [copied, setCopied] = useState(false);

  const handleShare = async () => {
    const shareUrl = url || window.location.href;
    
    // Try native Web Share API first (mobile-friendly)
    if (navigator.share) {
      try {
        await navigator.share({ title, text: text || title, url: shareUrl });
        return;
      } catch { /* user cancelled, fall through to copy */ }
    }
    
    // Fallback: copy to clipboard
    try {
      await navigator.clipboard.writeText(shareUrl);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch { /* ignore */ }
  };

  return (
    <button onClick={handleShare} className={`flex items-center gap-1.5 text-sm ${className}`}>
      {copied ? <Check size={16} className="text-green-500" /> : <Share2 size={16} />}
      {copied ? 'Đã sao chép!' : 'Chia sẻ'}
    </button>
  );
};

// ============ #22 Cart Progress Bar ============
export const CartProgressBar = ({ totalAmount }: { totalAmount: number }) => {
  const { t } = useTranslation();
  const FREE_SHIPPING_THRESHOLD = 500000; // 500k VND
  const progress = Math.min((totalAmount / FREE_SHIPPING_THRESHOLD) * 100, 100);
  const remaining = FREE_SHIPPING_THRESHOLD - totalAmount;
  const qualified = remaining <= 0;

  const formatPrice = (price: number) =>
    new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);

  return (
    <div className="p-4 bg-gradient-to-r from-indigo-50 to-purple-50 dark:from-indigo-950/30 dark:to-purple-950/30 rounded-xl border">
      <div className="flex items-center justify-between mb-2">
        <span className="text-sm font-medium">
          {qualified
            ? t('cart.freeShipping', '🎉 Miễn phí vận chuyển!')
            : t('cart.addMoreFor', `Thêm ${formatPrice(remaining)} để được miễn phí vận chuyển`)}
        </span>
        <span className="text-xs text-tertiary">{Math.round(progress)}%</span>
      </div>
      <div className="h-2 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
        <motion.div
          initial={{ width: 0 }}
          animate={{ width: `${progress}%` }}
          transition={{ duration: 0.5, ease: 'easeOut' }}
          className={`h-full rounded-full ${qualified ? 'bg-green-500' : 'bg-indigo-500'}`}
        />
      </div>
    </div>
  );
};

// ============ #23 Gift Message Option ============
export const GiftMessageOption = ({ 
  value, 
  onChange 
}: { 
  value: string; 
  onChange: (msg: string) => void;
}) => {
  const { t } = useTranslation();
  const [isGift, setIsGift] = useState(!!value);

  return (
    <div className="border rounded-xl overflow-hidden">
      <button
        onClick={() => { setIsGift(!isGift); if (isGift) onChange(''); }}
        className="w-full flex items-center gap-3 p-4 hover:bg-hover transition-colors"
      >
        <div className={`w-5 h-5 rounded border-2 flex items-center justify-center transition-colors ${
          isGift ? 'bg-indigo-600 border-indigo-600' : 'border-gray-300'
        }`}>
          {isGift && <Check size={12} className="text-white" />}
        </div>
        <Gift size={18} className="text-pink-500" />
        <span className="text-sm font-medium">{t('cart.giftMessage', 'Đây là quà tặng — Thêm lời nhắn')}</span>
      </button>
      <AnimatePresence>
        {isGift && (
          <motion.div
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: 'auto', opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            className="overflow-hidden"
          >
            <div className="p-4 pt-0">
              <textarea
                value={value}
                onChange={(e) => onChange(e.target.value)}
                placeholder={t('cart.giftPlaceholder', 'Viết lời chúc cho người nhận...')}
                maxLength={200}
                rows={3}
                className="w-full px-3 py-2 bg-secondary border rounded-lg text-sm focus:outline-none focus:border-indigo-500 resize-none"
              />
              <p className="text-xs text-tertiary mt-1 text-right">{value.length}/200</p>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

// ============ #36 Re-order Button ============
export const ReorderButton = ({ 
  orderItems, 
  onReorder 
}: { 
  orderItems: Array<{ code: string; name: string; quantity: number }>;
  onReorder: (items: typeof orderItems) => void;
}) => {
  const { t } = useTranslation();
  
  return (
    <button
      onClick={() => onReorder(orderItems)}
      className="flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors text-sm font-medium"
    >
      <Copy size={14} />
      {t('order.reorder', 'Đặt lại đơn hàng')}
    </button>
  );
};

// ============ #41 Q&A Section ============
interface QAItem {
  id: string;
  question: string;
  answer?: string;
  askedBy: string;
  askedAt: string;
  helpful: number;
}

export const QASection = ({ productCode }: { productCode: string }) => {
  const { t } = useTranslation();
  const [questions, setQuestions] = useState<QAItem[]>(() => {
    const stored = localStorage.getItem(`vnvt_qa_${productCode}`);
    return stored ? JSON.parse(stored) : [];
  });
  const [newQuestion, setNewQuestion] = useState('');
  const [searchTerm, setSearchTerm] = useState('');

  const handleAsk = useCallback(() => {
    if (!newQuestion.trim()) return;
    const item: QAItem = {
      id: Date.now().toString(),
      question: newQuestion.trim(),
      askedBy: 'Khách hàng',
      askedAt: new Date().toISOString(),
      helpful: 0,
    };
    const updated = [item, ...questions];
    setQuestions(updated);
    localStorage.setItem(`vnvt_qa_${productCode}`, JSON.stringify(updated));
    setNewQuestion('');
  }, [newQuestion, questions, productCode]);

  const filteredQuestions = searchTerm
    ? questions.filter(q => q.question.toLowerCase().includes(searchTerm.toLowerCase()))
    : questions;

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-bold flex items-center gap-2">
        <MessageSquare size={20} />
        {t('qa.title', 'Hỏi & Đáp')} ({questions.length})
      </h3>

      {/* Search */}
      <div className="relative">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary" />
        <input
          type="text"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          placeholder={t('qa.search', 'Tìm câu hỏi...')}
          className="w-full pl-9 pr-3 py-2 bg-secondary border rounded-lg text-sm focus:outline-none focus:border-indigo-500"
        />
      </div>

      {/* Ask */}
      <div className="flex gap-2">
        <input
          type="text"
          value={newQuestion}
          onChange={(e) => setNewQuestion(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && handleAsk()}
          placeholder={t('qa.askPlaceholder', 'Đặt câu hỏi về sản phẩm này...')}
          className="flex-1 px-3 py-2 bg-secondary border rounded-lg text-sm focus:outline-none focus:border-indigo-500"
        />
        <button onClick={handleAsk} className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 transition-colors">
          {t('qa.ask', 'Gửi')}
        </button>
      </div>

      {/* Questions List */}
      <div className="space-y-3">
        {filteredQuestions.length === 0 ? (
          <p className="text-center text-tertiary text-sm py-6">
            {t('qa.noQuestions', 'Chưa có câu hỏi nào. Hãy là người đầu tiên đặt câu hỏi!')}
          </p>
        ) : (
          filteredQuestions.map((q) => (
            <div key={q.id} className="p-4 bg-secondary rounded-lg">
              <p className="text-sm font-medium mb-1">❓ {q.question}</p>
              {q.answer && (
                <p className="text-sm text-green-700 dark:text-green-400 mt-2 pl-4 border-l-2 border-green-400">
                  💬 {q.answer}
                </p>
              )}
              <div className="flex items-center gap-3 mt-2 text-xs text-tertiary">
                <span>{q.askedBy}</span>
                <span>•</span>
                <span>{new Date(q.askedAt).toLocaleDateString('vi-VN')}</span>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

// ============ #45 Helpfulness Voting ============
export const HelpfulnessVoting = ({ reviewId }: { reviewId: string }) => {
  const [vote, setVote] = useState<'up' | 'down' | null>(() => {
    const stored = localStorage.getItem(`vnvt_vote_${reviewId}`);
    return stored as 'up' | 'down' | null;
  });
  const [counts, setCounts] = useState(() => {
    const stored = localStorage.getItem(`vnvt_vote_counts_${reviewId}`);
    return stored ? JSON.parse(stored) : { up: 0, down: 0 };
  });

  const handleVote = (type: 'up' | 'down') => {
    const newVote = vote === type ? null : type;
    const newCounts = { ...counts };
    if (vote) newCounts[vote]--;
    if (newVote) newCounts[newVote]++;
    
    setVote(newVote);
    setCounts(newCounts);
    
    if (newVote) localStorage.setItem(`vnvt_vote_${reviewId}`, newVote);
    else localStorage.removeItem(`vnvt_vote_${reviewId}`);
    localStorage.setItem(`vnvt_vote_counts_${reviewId}`, JSON.stringify(newCounts));
  };

  return (
    <div className="flex items-center gap-1 text-xs text-tertiary">
      <span>Hữu ích?</span>
      <button
        onClick={() => handleVote('up')}
        className={`flex items-center gap-0.5 px-2 py-1 rounded-md transition-colors ${
          vote === 'up' ? 'bg-green-100 text-green-700' : 'hover:bg-hover'
        }`}
      >
        <ThumbsUp size={12} /> {counts.up > 0 && counts.up}
      </button>
      <button
        onClick={() => handleVote('down')}
        className={`flex items-center gap-0.5 px-2 py-1 rounded-md transition-colors ${
          vote === 'down' ? 'bg-red-100 text-red-700' : 'hover:bg-hover'
        }`}
      >
        <ThumbsDown size={12} /> {counts.down > 0 && counts.down}
      </button>
    </div>
  );
};

// ============ #13 Before/After Image Slider ============
export const BeforeAfterSlider = ({ 
  beforeSrc, 
  afterSrc,
  beforeLabel = 'Trước',
  afterLabel = 'Sau'
}: { 
  beforeSrc: string; 
  afterSrc: string;
  beforeLabel?: string;
  afterLabel?: string;
}) => {
  const [position, setPosition] = useState(50);

  return (
    <div 
      className="relative w-full aspect-square rounded-xl overflow-hidden cursor-col-resize select-none"
      onMouseMove={(e) => {
        const rect = e.currentTarget.getBoundingClientRect();
        const x = ((e.clientX - rect.left) / rect.width) * 100;
        setPosition(Math.min(Math.max(x, 5), 95));
      }}
      onTouchMove={(e) => {
        const rect = e.currentTarget.getBoundingClientRect();
        const x = ((e.touches[0].clientX - rect.left) / rect.width) * 100;
        setPosition(Math.min(Math.max(x, 5), 95));
      }}
    >
      {/* After (full width background) */}
      <img src={afterSrc} alt={afterLabel} className="absolute inset-0 w-full h-full object-cover" />
      
      {/* Before (clipped) */}
      <div className="absolute inset-0 overflow-hidden" style={{ width: `${position}%` }}>
        <img src={beforeSrc} alt={beforeLabel} className="absolute inset-0 w-full h-full object-cover" style={{ minWidth: `${100 / (position / 100)}%` }} />
      </div>
      
      {/* Divider */}
      <div className="absolute top-0 bottom-0" style={{ left: `${position}%` }}>
        <div className="absolute top-0 bottom-0 -translate-x-1/2 w-0.5 bg-white shadow-lg" />
        <div className="absolute top-1/2 -translate-x-1/2 -translate-y-1/2 w-8 h-8 bg-white rounded-full shadow-lg flex items-center justify-center">
          <ChevronLeft size={12} className="text-gray-600 -mr-1" />
          <ChevronRight size={12} className="text-gray-600 -ml-1" />
        </div>
      </div>

      {/* Labels */}
      <span className="absolute top-3 left-3 px-2 py-1 bg-black/50 text-white text-xs rounded-md">{beforeLabel}</span>
      <span className="absolute top-3 right-3 px-2 py-1 bg-black/50 text-white text-xs rounded-md">{afterLabel}</span>
    </div>
  );
};
