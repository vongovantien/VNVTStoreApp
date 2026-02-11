import React from 'react';
import { useTranslation } from 'react-i18next';
import { Truck, Scale, Package, Globe, Shield, RefreshCw } from 'lucide-react';
import { ProductDetailType } from '@/types';
import type { Product } from '@/types';

interface ProductLogisticsProps {
  product: Product;
}

export const ProductLogistics: React.FC<ProductLogisticsProps> = ({ product }) => {
  const { t } = useTranslation();

  const logisticsDetails = product.details?.filter(d => d.detailType === ProductDetailType.LOGISTICS) || [];
  const hasLogistics = logisticsDetails.length > 0 || !!product.countryOfOrigin;

  return (
    <>
      {/* Dynamic Logistics Cards */}
      {hasLogistics && (
          <div className="flex flex-wrap gap-3 py-2">
              {logisticsDetails.map((detail, idx) => {
                  const lowerName = detail.specName.toLowerCase();
                  let Icon = Truck;
                  if (lowerName.includes('weight') || lowerName.includes('cân') || lowerName.includes('nặng') || lowerName.includes('kg')) Icon = Scale;
                  if (lowerName.includes('box') || lowerName.includes('hộp') || lowerName.includes('thùng') || lowerName.includes('pack') || lowerName.includes('quy cách')) Icon = Package;

                  return (
                      <div key={idx} className="flex items-center gap-2 p-3 bg-white border border-slate-100 rounded-xl shadow-sm">
                          <Icon size={16} className="text-indigo-500" />
                          <div className="text-xs">
                              <span className="text-tertiary">{detail.specName}: </span>
                              <span className="font-bold text-primary">{detail.specValue}</span>
                          </div>
                      </div>
                  );
              })}
              {product.countryOfOrigin && (
                  <div className="flex items-center gap-2 p-3 bg-white border border-slate-100 rounded-xl shadow-sm">
                      <Globe size={16} className="text-blue-500" />
                      <div className="text-xs">
                          <span className="text-tertiary">{t('product.origin', 'Xuất xứ')}: </span>
                          <span className="font-bold text-primary">{product.countryOfOrigin}</span>
                      </div>
                  </div>
              )}
          </div>
      )}

      {/* Features */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 pt-4">
        {[
          { icon: Truck, title: t('product.freeShipping'), desc: t('product.freeShippingDesc') },
          { icon: Shield, title: t('product.warranty'), desc: t('product.warrantyDesc') },
          { icon: RefreshCw, title: t('product.returns'), desc: t('product.returnsDesc') },
        ].map((feature, i) => (
          <div key={i} className="flex items-start gap-3 p-3 bg-secondary rounded-lg">
            <feature.icon size={20} className="text-primary flex-shrink-0 mt-0.5" />
            <div>
              <p className="font-medium text-sm">{feature.title}</p>
              <p className="text-xs text-tertiary">{feature.desc}</p>
            </div>
          </div>
        ))}
      </div>
    </>
  );
};
