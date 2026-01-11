import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { formatDate, formatCurrency } from '@/utils/format';
import { quoteService, type QuoteDto } from '@/services/quoteService';
// import { QuoteRequest } from '@/types'; // Remove or unused
import { Link } from 'react-router-dom';

const QuotesContent = () => {
  const { t } = useTranslation();
  const [quotes, setQuotes] = useState<QuoteDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    quoteService.getQuotes().then(res => {
      if (res.success && res.data) {
        setQuotes(res.data);
      }
      setLoading(false);
    });
  }, []);

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'pending': return 'warning';
      case 'quoted': return 'info';
      case 'closed': return 'success';
      case 'cancelled': return 'error';
      default: return 'secondary';
    }
  };

  const getStatusText = (status: string) => {
    // Simple mapping, ideally use translation
    switch (status) {
      case 'pending': return 'Đang chờ';
      case 'quoted': return 'Đã báo giá';
      case 'closed': return 'Hoàn thành';
      case 'cancelled': return 'Đã hủy';
      default: return status;
    }
  };

  if (loading) return <div>{t('common.loading')}</div>;

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-bold">{t('account.quotes')}</h2>

      {quotes.length === 0 && (
        <div className="text-center py-8">
          <p className="text-secondary mb-4">Bạn chưa có yêu cầu báo giá nào.</p>
          <Link to="/products" className="text-primary hover:underline">Xem sản phẩm để yêu cầu báo giá</Link>
        </div>
      )}

      {quotes.map((quote) => (
        <div key={quote.code} className="bg-primary rounded-xl p-4 border border-secondary/20 hover:shadow-md transition-shadow">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2 mb-4">
            <div>
              <p className="font-semibold text-color-primary">#{quote.code.substring(0, 8)}</p>
              <p className="text-sm text-tertiary">{formatDate(quote.createdAt)}</p>
            </div>
            <span
              className={`px-3 py-1 rounded-full text-xs font-semibold bg-${getStatusColor(quote.status)}/20 text-${getStatusColor(quote.status)}`}
            >
              {getStatusText(quote.status)}
            </span>
          </div>

          <div className="flex items-center gap-3 mb-4">
            <img
              src={quote.productImage || 'https://via.placeholder.com/50'}
              alt={quote.productName}
              className="w-16 h-16 object-cover rounded bg-secondary/10"
            />
            <div className="flex-1 min-w-0">
              <p className="text-base font-medium truncate text-color-primary">{quote.productName}</p>
              <p className="text-sm text-tertiary">Số lượng: {quote.quantity}</p>
              {quote.note && <p className="text-sm text-secondary italic">" {quote.note} "</p>}
            </div>
          </div>

          {quote.quotedPrice && (
            <div className="flex justify-between items-center pt-4 border-t border-secondary/10">
              <span className="text-sm text-secondary">Giá được báo</span>
              <span className="font-bold text-lg text-primary">{formatCurrency(quote.quotedPrice)}</span>
            </div>
          )}
        </div>
      ))}
    </div>
  );
};

export default QuotesContent;
