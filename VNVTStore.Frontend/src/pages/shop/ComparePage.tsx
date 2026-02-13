import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Scale, X, ShoppingCart, Check, Minus } from 'lucide-react';
import { Button, Badge } from '@/components/ui';
import { useCompareStore, useCartStore } from '@/store';
import SharedImage from '@/components/common/Image';
import { formatCurrency } from '@/utils/format';
import { Product } from '@/types';

import { useSEO } from '@/hooks/useSEO';

export const ComparePage = () => {
  const { t } = useTranslation();
  
  useSEO({
    title: 'So sánh sản phẩm',
    noindex: true,
  });
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



  return (
    <div className="min-h-screen bg-secondary">
      <div className="container mx-auto px-4 py-8">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-2xl md:text-3xl font-bold">{t('compare.title')}</h1>
          <Button variant="ghost" onClick={clearCompare}>
            {t('compare.clearAll')}
          </Button>
        </div>

        <div className="bg-white dark:bg-slate-900 rounded-3xl shadow-xl overflow-hidden border border-slate-100 dark:border-slate-800">
          <div className="overflow-x-auto">
            <table className="w-full min-w-[800px] border-collapse">
              <thead>
                <tr className="bg-slate-50 dark:bg-slate-800/50">
                  <th className="p-6 text-left w-64 sticky left-0 z-20 bg-slate-50 dark:bg-slate-800 border-b border-slate-100 dark:border-slate-700 shadow-[2px_0_5px_-2px_rgba(0,0,0,0.05)]">
                    <div className="flex items-center gap-2 text-indigo-600 font-bold text-xs uppercase tracking-widest">
                      <Scale size={16} />
                      {t('compare.features', 'Tính năng')}
                    </div>
                  </th>
                  {items.map((product) => (
                    <th key={product.code} className="p-6 text-center border-b border-l border-slate-100 dark:border-slate-700 min-w-[250px]">
                      <div className="relative group">
                        <button
                          onClick={() => removeItem(product.code)}
                          className="absolute -top-2 -right-2 p-1.5 bg-rose-500 text-white rounded-full hover:bg-rose-600 transition-all opacity-0 group-hover:opacity-100 shadow-lg z-10"
                        >
                          <X size={14} />
                        </button>
                        <Link to={`/product/${product.code}`} className="block">
                          <div className="w-32 h-32 mx-auto mb-4 rounded-2xl overflow-hidden border border-slate-100 dark:border-slate-800 bg-slate-50 dark:bg-slate-950">
                            <SharedImage
                              src={product.image}
                              alt={product.name}
                              className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                            />
                          </div>
                          <h3 className="font-bold text-slate-800 dark:text-white group-hover:text-indigo-600 transition-colors line-clamp-2 text-sm">
                            {product.name}
                          </h3>
                        </Link>
                      </div>
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
                {/* Price Row */}
                <tr className="hover:bg-slate-50/50 dark:hover:bg-slate-800/20 transition-colors">
                  <td className="p-6 font-bold text-slate-900 dark:text-white sticky left-0 z-10 bg-white dark:bg-slate-900 border-r border-slate-100 dark:border-slate-800 shadow-[2px_0_5px_-2px_rgba(0,0,0,0.05)]">
                    {t('compare.price')}
                  </td>
                  {items.map((product) => (
                    <td key={product.code} className="p-6 text-center">
                      {product.price > 0 ? (
                        <div className="flex flex-col items-center">
                          <span className="text-xl font-black text-rose-600 dark:text-rose-500">{formatCurrency(product.price)}</span>
                          {product.originalPrice && product.originalPrice > product.price && (
                            <span className="text-xs text-slate-400 line-through font-medium">{formatCurrency(product.originalPrice)}</span>
                          )}
                        </div>
                      ) : (
                        <span className="text-indigo-600 font-bold text-sm tracking-tight">{t('product.contactForPrice')}</span>
                      )}
                    </td>
                  ))}
                </tr>

                {/* Technical Specs Header */}
                <tr className="bg-slate-50/50 dark:bg-slate-800/30">
                  <td colSpan={items.length + 1} className="p-3 text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 text-center">
                    {t('compare.technicalSpecs', 'Thông số kỹ thuật')}
                  </td>
                </tr>

                {/* Dynamic Attributes Mapping */}
                {([
                  { key: 'brand', label: t('product.brand', 'Thương hiệu'), icon: null },
                  { key: 'color', label: t('product.color', 'Màu sắc'), icon: null },
                  { key: 'material', label: t('product.material', 'Chất liệu'), icon: null },
                  { key: 'power', label: t('product.power', 'Công suất'), icon: null },
                  { key: 'voltage', label: t('product.voltage', 'Điện áp'), icon: null },
                  { key: 'size', label: t('product.size', 'Kích thước / Dung tích'), icon: null },
                  { key: 'countryOfOrigin', label: t('product.origin', 'Xuất xứ'), icon: null },
                ] as const).map((attr) => (
                  <tr key={attr.key} className="hover:bg-hover transition-colors">
                    <td className="p-6 font-semibold text-slate-700 dark:text-slate-300 sticky left-0 z-10 bg-white dark:bg-slate-900 border-r border-slate-100 dark:border-slate-800 shadow-[2px_0_5px_-2px_rgba(0,0,0,0.05)]">
                      {attr.label}
                    </td>
                    {items.map((product) => {
                      const value = product[attr.key as keyof Product];
                      const displayValue = (typeof value === 'string' || typeof value === 'number') ? value : null;
                      return (
                        <td key={product.code} className="p-6 text-center text-slate-600 dark:text-slate-400 font-medium">
                          {displayValue || <Minus size={16} className="mx-auto text-slate-200" />}
                        </td>
                      );
                    })}
                  </tr>
                ))}

                {/* Rating Row */}
                <tr className="hover:bg-slate-50/50 dark:hover:bg-slate-800/20 transition-colors">
                  <td className="p-6 font-bold text-slate-900 dark:text-white sticky left-0 z-10 bg-white dark:bg-slate-900 border-r border-slate-100 dark:border-slate-800 shadow-[2px_0_5px_-2px_rgba(0,0,0,0.05)]">
                    {t('compare.rating', 'Đánh giá')}
                  </td>
                  {items.map((product) => (
                    <td key={product.code} className="p-6 text-center">
                      <div className="flex flex-col items-center gap-1">
                        <div className="flex items-center gap-0.5 text-amber-400">
                          <Check size={14} className="fill-current" />
                          <span className="font-black text-slate-900 dark:text-white">{(product.averageRating || product.rating || 0).toFixed(1)}</span>
                        </div>
                        <span className="text-[10px] text-slate-400 font-bold uppercase">{product.reviewCount} {t('product.reviews')}</span>
                      </div>
                    </td>
                  ))}
                </tr>

                {/* Stock Row */}
                <tr className="hover:bg-slate-50/50 dark:hover:bg-slate-800/20 transition-colors">
                  <td className="p-6 font-bold text-slate-900 dark:text-white sticky left-0 z-10 bg-white dark:bg-slate-900 border-r border-slate-100 dark:border-slate-800 shadow-[2px_0_5px_-2px_rgba(0,0,0,0.05)]">
                    {t('compare.availability', 'Khả dụng')}
                  </td>
                  {items.map((product) => (
                    <td key={product.code} className="p-6 text-center">
                      {product.stock > 0 ? (
                        <Badge color="success" className="font-bold tracking-tight">
                          {t('product.inStock')}
                        </Badge>
                      ) : (
                        <Badge color="error" className="font-bold tracking-tight">
                          {t('product.outOfStock')}
                        </Badge>
                      )}
                    </td>
                  ))}
                </tr>

                {/* Actions Row */}
                <tr className="bg-slate-50/30 dark:bg-slate-800/10">
                  <td className="p-6 sticky left-0 z-10 bg-slate-50 dark:bg-slate-800/50 border-r border-slate-100 dark:border-slate-800 shadow-[2px_0_5px_-2px_rgba(0,0,0,0.05)]"></td>
                  {items.map((product) => (
                    <td key={product.code} className="p-6 text-center">
                      {product.price > 0 ? (
                        <Button
                          fullWidth
                          size="sm"
                          onClick={() => addToCart(product)}
                          disabled={product.stock === 0}
                          className="bg-slate-900 hover:bg-black text-white rounded-xl py-2.5"
                          leftIcon={<ShoppingCart size={16} />}
                        >
                          {t('product.addToCart')}
                        </Button>
                      ) : (
                        <Link to={`/quote-request/${product.code}`} className="block">
                          <Button size="sm" variant="outline" fullWidth className="rounded-xl py-2.5">
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
    </div>
  );
};

export default ComparePage;
