import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Scale, X, ShoppingCart, Check, Minus } from 'lucide-react';
import { Button } from '@/components/ui';
import { useCompareStore, useCartStore } from '@/store';
import { formatCurrency } from '@/utils/format';

export const ComparePage = () => {
  const { t } = useTranslation();
  const { items, removeItem, clearCompare } = useCompareStore();
  const addToCart = useCartStore((state) => state.addItem);

  if (items.length === 0) {
    return (
      <div className="min-h-screen bg-secondary">
        <div className="container mx-auto px-4 py-16 text-center">
          <div className="w-24 h-24 mx-auto bg-tertiary rounded-full flex items-center justify-center mb-6">
            <Scale size={48} className="text-tertiary" />
          </div>
          <h1 className="text-2xl font-bold mb-4">{t('compare.empty')}</h1>
          <p className="text-secondary mb-8">{t('compare.emptyMessage')}</p>
          <Link to="/products">
            <Button size="lg">{t('common.browseProducts')}</Button>
          </Link>
        </div>
      </div>
    );
  }

  const specs = ['Thương hiệu', 'Danh mục', 'Đánh giá', 'Tình trạng'];

  return (
    <div className="min-h-screen bg-secondary">
      <div className="container mx-auto px-4 py-8">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-2xl md:text-3xl font-bold">{t('compare.title')}</h1>
          <Button variant="ghost" onClick={clearCompare}>
            {t('compare.clearAll')}
          </Button>
        </div>

        <div className="bg-primary rounded-xl overflow-x-auto">
          <table className="w-full min-w-[600px]">
            <thead>
              <tr>
                <th className="p-4 text-left w-40"></th>
                {items.map((product) => (
                  <th key={product.id} className="p-4 text-center border-l">
                    <div className="relative">
                      <button
                        onClick={() => removeItem(product.id)}
                        className="absolute -top-2 -right-2 p-1 bg-error text-white rounded-full hover:bg-error/80 transition-colors"
                      >
                        <X size={14} />
                      </button>
                      <Link to={`/product/${product.id}`}>
                        <img
                          src={product.image}
                          alt={product.name}
                          className="w-32 h-32 object-cover rounded-lg mx-auto mb-3"
                        />
                        <h3 className="font-semibold text-primary hover:text-primary-dark transition-colors line-clamp-2">
                          {product.name}
                        </h3>
                      </Link>
                    </div>
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {/* Price */}
              <tr className="bg-secondary/50">
                <td className="p-4 font-semibold">{t('compare.price')}</td>
                {items.map((product) => (
                  <td key={product.id} className="p-4 text-center border-l">
                    {product.price > 0 ? (
                      <span className="text-xl font-bold text-error">{formatCurrency(product.price)}</span>
                    ) : (
                      <span className="text-primary font-medium">{t('product.contactForPrice')}</span>
                    )}
                  </td>
                ))}
              </tr>

              {/* Brand */}
              <tr>
                <td className="p-4 font-semibold">Thương hiệu</td>
                {items.map((product) => (
                  <td key={product.id} className="p-4 text-center border-l">
                    {product.brand || <Minus size={16} className="mx-auto text-tertiary" />}
                  </td>
                ))}
              </tr>

              {/* Category */}
              <tr className="bg-secondary/50">
                <td className="p-4 font-semibold">Danh mục</td>
                {items.map((product) => (
                  <td key={product.id} className="p-4 text-center border-l">
                    {product.category}
                  </td>
                ))}
              </tr>

              {/* Rating */}
              <tr>
                <td className="p-4 font-semibold">Đánh giá</td>
                {items.map((product) => (
                  <td key={product.id} className="p-4 text-center border-l">
                    <span className="inline-flex items-center gap-1">
                      ⭐ {product.rating} ({product.reviewCount})
                    </span>
                  </td>
                ))}
              </tr>

              {/* Stock */}
              <tr className="bg-secondary/50">
                <td className="p-4 font-semibold">Tình trạng</td>
                {items.map((product) => (
                  <td key={product.id} className="p-4 text-center border-l">
                    {product.stock > 0 ? (
                      <span className="inline-flex items-center gap-1 text-success">
                        <Check size={16} /> Còn hàng
                      </span>
                    ) : (
                      <span className="text-error">Hết hàng</span>
                    )}
                  </td>
                ))}
              </tr>

              {/* Actions */}
              <tr>
                <td className="p-4"></td>
                {items.map((product) => (
                  <td key={product.id} className="p-4 text-center border-l">
                    {product.price > 0 ? (
                      <Button
                        size="sm"
                        onClick={() => addToCart(product)}
                        disabled={product.stock === 0}
                        leftIcon={<ShoppingCart size={16} />}
                      >
                        {t('product.addToCart')}
                      </Button>
                    ) : (
                      <Link to={`/quote-request/${product.id}`}>
                        <Button size="sm" variant="outline">
                          {t('product.requestQuote')}
                        </Button>
                      </Link>
                    )}
                  </td>
                ))}
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default ComparePage;
