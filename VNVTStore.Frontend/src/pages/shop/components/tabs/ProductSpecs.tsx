import React from 'react';
import { useTranslation } from 'react-i18next';
import { ProductDetailType } from '@/types';
import type { Product } from '@/types';

interface ProductSpecsProps {
  product: Product;
}

export const ProductSpecs: React.FC<ProductSpecsProps> = ({ product }) => {
  const { t } = useTranslation();

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-x-12 gap-y-2">
      {product.details?.filter(d => d.detailType === ProductDetailType.SPEC).length ? (
        product.details.filter(d => d.detailType === ProductDetailType.SPEC).map((detail, idx) => (
          <div key={idx} className="flex border-b border-slate-100 py-3 text-sm">
            <span className="w-1/2 font-medium text-secondary">{detail.specName}</span>
            <span className="flex-1 text-primary font-semibold">{detail.specValue}</span>
          </div>
        ))
      ) : (
        <div className="col-span-2 text-center py-8 text-tertiary">
          {t('product.noSpecs', 'Không có thông số kỹ thuật chi tiết.')}
        </div>
      )}
    </div>
  );
};
