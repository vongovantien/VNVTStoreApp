import React from 'react';
import { useTranslation } from 'react-i18next';
import { formatCurrency } from '@/utils/format';
import type { Product } from '@/types';

interface ProductUnitsProps {
  product: Product;
}

export const ProductUnits: React.FC<ProductUnitsProps> = ({ product }) => {
  const { t } = useTranslation();

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-x-12 gap-y-2">
      {product.productUnits && product.productUnits.length ? (
        product.productUnits.map((unit, idx) => (
          <div key={idx} className="flex border-b border-slate-100 py-3 text-sm">
            <span className="w-1/2 font-medium text-secondary">{unit.unitName}</span>
            <span className="flex-1 text-primary font-semibold">
              {formatCurrency(unit.price)} (x{unit.conversionRate})
            </span>
          </div>
        ))
      ) : (
        <div className="col-span-2 text-center py-8 text-tertiary">
          {t('product.noUnits', 'Không có thông tin đơn vị tính.')}
        </div>
      )}
    </div>
  );
};
