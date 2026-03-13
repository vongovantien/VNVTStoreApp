import React from 'react';
import { useTranslation } from 'react-i18next';
import { formatCurrency } from '@/utils/format';
import type { Product } from '@/types';

interface ProductVariantsProps {
  product: Product;
}

export const ProductVariants: React.FC<ProductVariantsProps> = ({ product }) => {
  const { t } = useTranslation();

  return (
    <div className="overflow-x-auto">
      {product.variants && product.variants.length ? (
        <table className="w-full text-sm text-left">
          <thead className="bg-secondary/50 text-tertiary font-medium">
            <tr>
              <th className="px-4 py-3 border-b">{t('common.fields.code')}</th>
              <th className="px-4 py-3 border-b">{t('common.fields.specs')}</th>
              <th className="px-4 py-3 border-b text-right">{t('common.fields.price')}</th>
              <th className="px-4 py-3 border-b text-right">{t('common.fields.stock')}</th>
            </tr>
          </thead>
          <tbody>
            {product.variants.map((v, idx) => (
              <tr key={idx} className="hover:bg-hover transition-colors">
                <td className="px-4 py-3 font-medium text-primary">{v.sku}</td>
                <td className="px-4 py-3 text-secondary">{v.attributes}</td>
                <td className="px-4 py-3 text-right font-bold text-error">{formatCurrency(v.price)}</td>
                <td className="px-4 py-3 text-right text-secondary">{v.stockQuantity}</td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <div className="text-center py-8 text-tertiary">
          {t('product.noVariants', 'Không có phiên bản nào cho sản phẩm này.')}
        </div>
      )}
    </div>
  );
};
