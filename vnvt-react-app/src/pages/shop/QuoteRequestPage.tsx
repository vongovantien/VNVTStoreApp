import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';

import { useTranslation } from 'react-i18next';
import { ChevronRight, Phone, Mail, User, Send, Check } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { productService, quoteService } from '@/services';
import type { Product } from '@/types';

export const QuoteRequestPage = () => {
  const { t } = useTranslation();
  const { productId } = useParams<{ productId: string }>();


  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [product, setProduct] = useState<Product | null>(null);
  const [loading, setLoading] = useState(true);

  // Fetch product
  useEffect(() => {
    const fetchProduct = async () => {
        if (!productId) return;
        try {
            const res = await productService.getProductByCode(productId);
            if (res.success && res.data) {
                setProduct(res.data);
            }
        } catch (error) {
            console.error('Failed to fetch product', error);
        } finally {
            setLoading(false);
        }
    };
    fetchProduct();
  }, [productId]);


  const [formData, setFormData] = useState({
    name: '',
    email: '',
    phone: '',
    company: '',
    quantity: '1',
    note: '',
  });

  const handleInputChange = (field: string, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!product || !productId) return;

    setIsSubmitting(true);
    try {
        const res = await quoteService.createQuote({
            productId: productId,
            productName: product.name,
            productImage: product.image,
            customerName: formData.name,
            customerEmail: formData.email,
            customerPhone: formData.phone,
            company: formData.company,
            quantity: parseInt(formData.quantity) || 1,
            note: formData.note
        });

        if (res.success) {
            setIsSubmitted(true);
        }
    } catch (error) {
        console.error('Submit quote failed', error);
    } finally {
        setIsSubmitting(false);
    }
  };

  if (loading) {
      return <div className="min-h-screen flex items-center justify-center">Loading...</div>;
  }

  if (!product) {
    return (
      <div className="min-h-screen bg-secondary flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">{t('common.notFound')}</h1>
          <Link to="/products">
            <Button>{t('common.backToProducts')}</Button>
          </Link>
        </div>
      </div>
    );
  }

  if (isSubmitted) {
    return (
      <div className="min-h-screen bg-secondary flex items-center justify-center">
        <div className="text-center max-w-md mx-auto p-8">
          <div className="w-20 h-20 mx-auto bg-success/20 rounded-full flex items-center justify-center mb-6">
            <Check size={40} className="text-success" />
          </div>
          <h1 className="text-2xl font-bold mb-4">{t('quote.success')}</h1>
          <p className="text-secondary mb-8">{t('quote.successMessage')}</p>
          <div className="flex flex-col sm:flex-row gap-3 justify-center">
            <Link to="/products">
              <Button variant="outline">{t('common.browseProducts')}</Button>
            </Link>
            <Link to="/">
              <Button>{t('common.backToHome')}</Button>
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-secondary">
      {/* Breadcrumb */}
      <div className="bg-primary border-b py-3">
        <div className="container mx-auto px-4 flex items-center gap-2 text-sm flex-wrap">
          <Link to="/" className="text-secondary hover:text-primary">{t('common.home')}</Link>
          <ChevronRight size={14} />
          <Link to={`/product/${product.id}`} className="text-secondary hover:text-primary truncate max-w-[150px]">
            {product.name}
          </Link>
          <ChevronRight size={14} />
          <span className="text-primary font-medium">{t('quote.title')}</span>
        </div>
      </div>

      <div className="container mx-auto px-4 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Form */}
          <div className="lg:col-span-2">
            <div className="bg-primary rounded-xl p-6">
              <h1 className="text-2xl font-bold mb-2">{t('quote.title')}</h1>
              <p className="text-secondary mb-6">{t('quote.description')}</p>

              <form onSubmit={handleSubmit} className="space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Input
                    label={t('quote.name')}
                    placeholder="Nguyễn Văn A"
                    value={formData.name}
                    onChange={(e) => handleInputChange('name', e.target.value)}
                    leftIcon={<User size={18} />}
                    required
                  />
                  <Input
                    label={t('quote.phone')}
                    placeholder="0901234567"
                    value={formData.phone}
                    onChange={(e) => handleInputChange('phone', e.target.value)}
                    leftIcon={<Phone size={18} />}
                    required
                  />
                  <Input
                    label={t('quote.email')}
                    type="email"
                    placeholder="email@example.com"
                    value={formData.email}
                    onChange={(e) => handleInputChange('email', e.target.value)}
                    leftIcon={<Mail size={18} />}
                    required
                  />
                  <Input
                    label={t('quote.company')}
                    placeholder="Tên công ty (không bắt buộc)"
                    value={formData.company}
                    onChange={(e) => handleInputChange('company', e.target.value)}
                  />
                  <Input
                    label={t('quote.quantity')}
                    type="number"
                    min="1"
                    value={formData.quantity}
                    onChange={(e) => handleInputChange('quantity', e.target.value)}
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium mb-2">{t('quote.note')}</label>
                  <textarea
                    placeholder="Mô tả yêu cầu của bạn..."
                    value={formData.note}
                    onChange={(e) => handleInputChange('note', e.target.value)}
                    rows={4}
                    className="w-full px-4 py-3 border rounded-lg focus:outline-none focus:border-primary resize-none"
                  />
                </div>

                <Button
                  type="submit"
                  size="lg"
                  fullWidth
                  isLoading={isSubmitting}
                  leftIcon={<Send size={20} />}
                >
                  {t('quote.submit')}
                </Button>
              </form>
            </div>
          </div>

          {/* Product Summary */}
          <div className="lg:col-span-1">
            <div className="bg-primary rounded-xl p-6 sticky top-24">
              <h2 className="font-bold mb-4">{t('quote.productInfo')}</h2>

              <div className="flex gap-4 mb-4">
                <img
                  src={product.image}
                  alt={product.name}
                  className="w-24 h-24 object-cover rounded-lg"
                />
                <div>
                  <h3 className="font-semibold line-clamp-2">{product.name}</h3>
                  <p className="text-sm text-tertiary">{product.category}</p>
                  <p className="text-sm text-tertiary">{product.brand}</p>
                </div>
              </div>

              <div className="bg-secondary rounded-lg p-4 text-center">
                <p className="text-sm text-tertiary mb-1">{t('product.priceType')}</p>
                <p className="text-lg font-semibold text-primary">{t('product.contactForPrice')}</p>
              </div>

              <div className="mt-6 space-y-2 text-sm">
                <div className="flex items-center gap-2 text-secondary">
                  <Check size={16} className="text-success" />
                  <span>Báo giá trong 24h</span>
                </div>
                <div className="flex items-center gap-2 text-secondary">
                  <Check size={16} className="text-success" />
                  <span>Tư vấn miễn phí</span>
                </div>
                <div className="flex items-center gap-2 text-secondary">
                  <Check size={16} className="text-success" />
                  <span>Giá cạnh tranh nhất</span>
                </div>
              </div>

              <div className="mt-6 pt-6 border-t">
                <p className="text-sm text-secondary mb-2">Cần hỗ trợ ngay?</p>
                <a href="tel:1900123456" className="flex items-center gap-2 text-primary font-semibold">
                  <Phone size={18} />
                  1900 123 456
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default QuoteRequestPage;
