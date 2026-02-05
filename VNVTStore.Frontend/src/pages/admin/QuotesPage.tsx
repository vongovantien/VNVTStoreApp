/* eslint-disable @typescript-eslint/no-explicit-any */
import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, MessageSquare, DollarSign, Check, Eye, ClipboardList, FileText, X } from 'lucide-react';
import { Button, Badge, Modal, Input } from '@/components/ui';
import { formatCurrency, formatDate } from '@/utils/format';
import { quoteService } from '@/services/quoteService';
import type { QuoteRequest } from '@/types';
import { AdminPageHeader } from '@/components/admin';

import { useToast } from '@/store/toastStore';
import { StatsCards } from '@/components/admin/StatsCards';
import { useQuery } from '@tanstack/react-query';

export const QuotesPage = () => {
  const { t } = useTranslation();
  const { success, error } = useToast();

  const [quotes, setQuotes] = useState<QuoteRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [selectedQuote, setSelectedQuote] = useState<QuoteRequest | null>(null);
  const [showQuoteModal, setShowQuoteModal] = useState(false);
  const [quotePrice, setQuotePrice] = useState('');

  // Fetch Stats
  const { data: statsData, isLoading: isStatsLoading } = useQuery({
      queryKey: ['quote-stats'],
      queryFn: () => quoteService.getStats(),
      staleTime: 60000,
  });

  // Fetch quotes
  useEffect(() => {
    fetchQuotes();
  }, []);

  const fetchQuotes = async () => {
    try {
      const res = await quoteService.getAll();
      if (res.success && res.data) {
        const mappedQuotes: QuoteRequest[] = (res.data.items || []).map((dto) => ({
          code: dto.code,
          productCode: dto.productCode,
          productName: dto.productName || 'Unknown Product',
          productImage: dto.productImage || 'https://placehold.co/100',
          quantity: dto.quantity,
          note: dto.note,
          status: dto.status as any,
          quotedPrice: dto.quotedPrice,
          createdAt: dto.createdAt,
          customer: {
            name: dto.customerName || dto.userName || 'Unknown Customer',
            email: dto.customerEmail || '',
            phone: dto.customerPhone || ''
          }
        }));
        setQuotes(mappedQuotes);
      }
    } catch (error) {
      console.error('Failed to fetch quotes', error);
    } finally {
      setLoading(false);
    }
  };

  const filteredQuotes = quotes.filter((quote) => {
    const matchesSearch =
      quote.productName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      quote.customer.name.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesStatus = statusFilter === 'all' || quote.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleQuote = (quote: QuoteRequest) => {
    setSelectedQuote(quote);
    setShowQuoteModal(true);
    setQuotePrice(quote.quotedPrice ? quote.quotedPrice.toString() : '');
  };

  const handleSubmitQuote = async () => {
    if (!selectedQuote || !quotePrice) return;

    try {
      const res = await quoteService.update(selectedQuote.code, {
        status: 'quoted',
        quotedPrice: parseFloat(quotePrice)
      });

      if (res.success) {
        success('Báo giá thành công');
        setShowQuoteModal(false);
        setQuotePrice('');
        fetchQuotes(); // Refresh
      }
    } catch {
      error(t('common.messages.errorOccurred'));
    }

  };

  if (loading) return <div>Loading...</div>;

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="admin.quotes"
        subtitle="admin.subtitles.quotes"
        rightSection={
          <div className="flex items-center gap-2">
            <Badge color="warning">{quotes.filter((q) => q.status === 'pending').length} {t('admin.status.pending')}</Badge>
          </div>
        }
      />

      <StatsCards stats={[
        {
            label: t('admin.stats.totalOrders'),
            value: statsData?.total || 0,
            icon: <ClipboardList size={24} />,
            color: 'blue',
            loading: isStatsLoading
        },
        {
            label: t('admin.stats.pending'),
            value: statsData?.pending || 0,
            icon: <FileText size={24} />,
            color: 'amber',
            loading: isStatsLoading
        },
        {
            label: t('admin.stats.processedQuotes'),
             value: statsData?.processed || 0,
            icon: <Check size={24} />,
            color: 'emerald',
            loading: isStatsLoading
        }
    ]} />

      {/* Filters */}
      <div className="bg-primary rounded-xl p-4">
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="flex-1 relative">
            <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary" />
            <input
              type="text"
              placeholder={t('common.placeholders.search')}
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
            <option value="all">{t('admin.filters.allStatus')}</option>
            <option value="pending">{t('admin.status.pending')}</option>
            <option value="quoted">{t('admin.status.quoted')}</option>
            <option value="closed">{t('admin.status.closed')}</option>
            <option value="cancelled">{t('admin.status.cancelled')}</option>
          </select>
        </div>
      </div>

      {/* Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredQuotes.map((quote) => (
          <div key={quote.code} className="bg-primary rounded-xl p-5">
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
                  ? t('admin.status.pending')
                  : quote.status === 'quoted'
                    ? t('admin.status.quoted')
                    : quote.status === 'closed'
                      ? t('admin.status.closed')
                      : t('admin.status.cancelled')}
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
                <p className="text-sm text-tertiary">{t('common.fields.quantity')}: {quote.quantity}</p>
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
                <p className="text-sm text-secondary">{t('common.fields.quotedPrice')}</p>
                <p className="text-xl font-bold text-success">{formatCurrency(quote.quotedPrice)}</p>
              </div>
            )}

            {/* Actions */}
            <div className="flex gap-2">
              {quote.status === 'pending' && (
                <>
                  <Button size="sm" fullWidth onClick={() => handleQuote(quote)} leftIcon={<DollarSign size={16} />}>
                    {t('admin.actions.quote')}
                  </Button>
                  <Button size="sm" variant="ghost" className="text-error">
                    <X size={16} />
                  </Button>
                </>
              )}
              {quote.status === 'quoted' && (
                <>
                  <Button size="sm" fullWidth variant="outline" onClick={() => handleQuote(quote)}>
                    {t('admin.actions.updatePrice')}
                  </Button>
                  <Button size="sm" leftIcon={<Check size={16} />}>
                    {t('admin.actions.closeDeal')}
                  </Button>
                </>
              )}
              {(quote.status === 'closed' || quote.status === 'cancelled') && (
                <Button size="sm" fullWidth variant="outline" leftIcon={<Eye size={16} />}>
                  {t('admin.actions.view')}
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
        title={t('admin.actions.quote')}
        size="sm"
        footer={
          <div className="flex gap-3 justify-end">
            <Button variant="outline" onClick={() => setShowQuoteModal(false)}>
              {t('admin.actions.cancel')}
            </Button>
            <Button
              onClick={handleSubmitQuote}
            >
              {t('admin.actions.sendQuote')}
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
                <p className="text-sm text-tertiary">{t('common.fields.quantity')}: {selectedQuote.quantity}</p>
              </div>
            </div>

            <Input
              label={t('common.fields.price') + ' (VNĐ)'}
              type="number"
              placeholder={t('common.placeholders.enterPrice')}
              value={quotePrice}
              onChange={(e) => setQuotePrice(e.target.value)}
              leftIcon={<DollarSign size={18} />}
            />

            <div>
              <label className="block text-sm font-medium mb-2">{t('common.fields.note')}</label>
              <textarea
                placeholder={t('common.placeholders.enterNote')}
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
