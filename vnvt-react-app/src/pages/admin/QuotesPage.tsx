import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, FileText, Check, X, MessageSquare, DollarSign, Eye } from 'lucide-react';
import { Button, Badge, Modal, Input } from '@/components/ui';
import { formatCurrency, formatDate } from '@/utils/format';

// Mock quote requests
const mockQuotes = [
  {
    id: 'quote-1',
    productName: 'Điều hòa Daikin Inverter 1.5HP',
    productImage: 'https://images.unsplash.com/photo-1631545806609-279fe8e56c10?w=100',
    customer: { name: 'Công ty ABC', email: 'contact@abc.com', phone: '0901234567' },
    quantity: 10,
    note: 'Cần báo giá cho dự án văn phòng mới',
    status: 'pending' as const,
    createdAt: '2024-01-26T10:00:00Z',
  },
  {
    id: 'quote-2',
    productName: 'Robot hút bụi lau nhà Ecovacs T20',
    productImage: 'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=100',
    customer: { name: 'Nguyễn Văn B', email: 'nguyenvanb@email.com', phone: '0912345678' },
    quantity: 2,
    note: '',
    status: 'quoted' as const,
    quotedPrice: 35000000,
    createdAt: '2024-01-25T14:00:00Z',
  },
];

export const QuotesPage = () => {
  const { t } = useTranslation();
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [selectedQuote, setSelectedQuote] = useState<typeof mockQuotes[0] | null>(null);
  const [showQuoteModal, setShowQuoteModal] = useState(false);
  const [quotePrice, setQuotePrice] = useState('');

  const quotes = mockQuotes.filter((quote) => {
    const matchesSearch =
      quote.productName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      quote.customer.name.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesStatus = statusFilter === 'all' || quote.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleQuote = (quote: typeof mockQuotes[0]) => {
    setSelectedQuote(quote);
    setShowQuoteModal(true);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-bold">{t('admin.quotes')}</h1>
        <div className="flex items-center gap-2">
          <Badge color="warning">{mockQuotes.filter((q) => q.status === 'pending').length} chờ xử lý</Badge>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-primary rounded-xl p-4">
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="flex-1 relative">
            <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary" />
            <input
              type="text"
              placeholder="Tìm theo sản phẩm, khách hàng..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border rounded-lg focus:outline-none focus:border-primary"
            />
          </div>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="px-4 py-2 border rounded-lg focus:outline-none focus:border-primary"
          >
            <option value="all">Tất cả</option>
            <option value="pending">Chờ báo giá</option>
            <option value="quoted">Đã báo giá</option>
            <option value="closed">Đã chốt</option>
            <option value="cancelled">Đã hủy</option>
          </select>
        </div>
      </div>

      {/* Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {quotes.map((quote) => (
          <div key={quote.id} className="bg-primary rounded-xl p-5">
            {/* Status */}
            <div className="flex items-center justify-between mb-4">
              <Badge
                color={
                  quote.status === 'pending'
                    ? 'warning'
                    : quote.status === 'quoted'
                    ? 'info'
                    : quote.status === 'closed'
                    ? 'success'
                    : 'error'
                }
              >
                {quote.status === 'pending'
                  ? 'Chờ báo giá'
                  : quote.status === 'quoted'
                  ? 'Đã báo giá'
                  : quote.status === 'closed'
                  ? 'Đã chốt'
                  : 'Đã hủy'}
              </Badge>
              <span className="text-xs text-tertiary">{formatDate(quote.createdAt)}</span>
            </div>

            {/* Product */}
            <div className="flex gap-3 mb-4">
              <img
                src={quote.productImage}
                alt={quote.productName}
                className="w-16 h-16 object-cover rounded-lg"
              />
              <div>
                <p className="font-semibold line-clamp-2">{quote.productName}</p>
                <p className="text-sm text-tertiary">Số lượng: {quote.quantity}</p>
              </div>
            </div>

            {/* Customer */}
            <div className="mb-4 p-3 bg-secondary rounded-lg">
              <p className="font-medium">{quote.customer.name}</p>
              <p className="text-sm text-tertiary">{quote.customer.phone}</p>
              <p className="text-sm text-tertiary">{quote.customer.email}</p>
            </div>

            {/* Note */}
            {quote.note && (
              <div className="mb-4">
                <p className="text-sm text-secondary">
                  <MessageSquare size={14} className="inline mr-1" />
                  {quote.note}
                </p>
              </div>
            )}

            {/* Quoted Price */}
            {quote.quotedPrice && (
              <div className="mb-4 p-3 bg-success/10 rounded-lg text-center">
                <p className="text-sm text-secondary">Giá đã báo</p>
                <p className="text-xl font-bold text-success">{formatCurrency(quote.quotedPrice)}</p>
              </div>
            )}

            {/* Actions */}
            <div className="flex gap-2">
              {quote.status === 'pending' && (
                <>
                  <Button size="sm" fullWidth onClick={() => handleQuote(quote)} leftIcon={<DollarSign size={16} />}>
                    Báo giá
                  </Button>
                  <Button size="sm" variant="ghost" className="text-error">
                    <X size={16} />
                  </Button>
                </>
              )}
              {quote.status === 'quoted' && (
                <>
                  <Button size="sm" fullWidth variant="outline" onClick={() => handleQuote(quote)}>
                    Cập nhật giá
                  </Button>
                  <Button size="sm" leftIcon={<Check size={16} />}>
                    Chốt
                  </Button>
                </>
              )}
              {(quote.status === 'closed' || quote.status === 'cancelled') && (
                <Button size="sm" fullWidth variant="outline" leftIcon={<Eye size={16} />}>
                  Xem chi tiết
                </Button>
              )}
            </div>
          </div>
        ))}
      </div>

      {/* Quote Modal */}
      <Modal
        isOpen={showQuoteModal}
        onClose={() => {
          setShowQuoteModal(false);
          setQuotePrice('');
        }}
        title="Báo giá"
        size="sm"
        footer={
          <div className="flex gap-3 justify-end">
            <Button variant="outline" onClick={() => setShowQuoteModal(false)}>
              Hủy
            </Button>
            <Button
              onClick={() => {
                // Handle submit
                setShowQuoteModal(false);
                setQuotePrice('');
              }}
            >
              Gửi báo giá
            </Button>
          </div>
        }
      >
        {selectedQuote && (
          <div className="space-y-4">
            <div className="flex gap-3">
              <img
                src={selectedQuote.productImage}
                alt={selectedQuote.productName}
                className="w-16 h-16 object-cover rounded-lg"
              />
              <div>
                <p className="font-semibold">{selectedQuote.productName}</p>
                <p className="text-sm text-tertiary">Số lượng: {selectedQuote.quantity}</p>
              </div>
            </div>

            <Input
              label="Giá báo (VNĐ)"
              type="number"
              placeholder="Nhập giá báo..."
              value={quotePrice}
              onChange={(e) => setQuotePrice(e.target.value)}
              leftIcon={<DollarSign size={18} />}
            />

            <div>
              <label className="block text-sm font-medium mb-2">Ghi chú</label>
              <textarea
                placeholder="Ghi chú cho khách hàng..."
                rows={3}
                className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:border-primary resize-none"
              />
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
};

export default QuotesPage;
