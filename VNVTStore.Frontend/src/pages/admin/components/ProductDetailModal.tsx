import React from 'react';
import { useTranslation } from 'react-i18next';
import { Modal, Button, Badge } from '@/components/ui';
import { formatCurrency, getImageUrl } from '@/utils/format';
import type { Product } from '@/types';

interface ProductDetailModalProps {
  product: Product | null;
  onClose: () => void;
}

export const ProductDetailModal: React.FC<ProductDetailModalProps> = ({ product, onClose }) => {
  const { t } = useTranslation();

  // Helper to safely get image URL
  const getSafeImageUrl = (img: string | { imageURL?: string; imageUrl?: string; ImageURL?: string } | null | undefined) => {
      if (!img) return '';
      if (typeof img === 'string') return getImageUrl(img);
      const imageObj = img as { imageURL?: string; imageUrl?: string; ImageURL?: string };
      return getImageUrl(imageObj.imageURL || imageObj.imageUrl || imageObj.ImageURL || '');
  };
  
  if (!product) return null;

  return (
    <Modal
        isOpen={!!product}
        onClose={onClose}
        title={t('common.actions.view') + ' ' + t('common.modules.products')}
        size="lg"
      >
        <div className="space-y-6">
            <div className="flex items-start gap-6">
              <img
                src={getSafeImageUrl(product.images?.[0] || product.productImages?.[0]) || 'https://placehold.co/200'}
                alt={product.name}
                className="w-32 h-32 rounded-lg object-cover border bg-white"
              />
              <div className="flex-1">
                <h2 className="text-xl font-bold text-slate-800 dark:text-white">{product.name}</h2>
                <div className="flex items-center gap-2 mt-2">
                  <Badge color="info">{product.brand || t('common.fields.noBrand')}</Badge>
                  <Badge color="secondary">{product.category || '-'}</Badge>
                  <Badge color={product.isActive !== false ? 'success' : 'error'}>
                    {product.isActive !== false ? t('common.status.active') : t('common.status.inactive')}
                  </Badge>
                </div>
                <div className="mt-4 flex flex-col gap-1">
                  <div className="flex items-baseline gap-2">
                    <span className="text-sm text-slate-500 w-20">{t('common.fields.price')}:</span>
                    <span className="text-2xl font-bold text-rose-600">{formatCurrency(product.price)}</span>
                  </div>
                  {(product.wholesalePrice || 0) > 0 && (
                    <div className="flex items-baseline gap-2">
                      <span className="text-sm text-slate-500 w-20">{t('common.fields.wholesalePrice')}:</span>
                      <span className="text-lg font-semibold text-blue-600">{formatCurrency(product.wholesalePrice || 0)}</span>
                    </div>
                  )}
                  {(product.costPrice || 0) > 0 && (
                    <div className="flex items-baseline gap-2">
                      <span className="text-sm text-slate-500 w-20">{t('common.fields.costPrice')}:</span>
                      <span className="text-md font-medium text-slate-600">{formatCurrency(product.costPrice || 0)}</span>
                    </div>
                  )}
                </div>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-4">
                <h3 className="font-semibold text-slate-800 dark:text-white border-b pb-2">{t('common.fields.info')}</h3>
                <div className="space-y-2 text-sm">
                  <div className="flex justify-between py-1 border-b border-dashed">
                    <span className="text-slate-500">{t('common.fields.stock')}</span>
                    <span className="font-medium">{product.stock}</span>
                  </div>
                  {product.material && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('common.fields.material')}</span>
                      <span className="font-medium">{product.material}</span>
                    </div>
                  )}
                  {product.size && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('common.fields.size')}</span>
                      <span className="font-medium">{product.size}</span>
                    </div>
                  )}
                  {product.color && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('common.fields.color')}</span>
                      <span className="font-medium">{product.color}</span>
                    </div>
                  )}
                </div>
              </div>

              <div className="space-y-4">
                <h3 className="font-semibold text-slate-800 dark:text-white border-b pb-2">{t('common.fields.specs')}</h3>
                <div className="space-y-2 text-sm">
                  {product.details?.map((detail, idx) => (
                    <div key={idx} className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{detail.specName}</span>
                      <span className="font-medium">{detail.specValue}</span>
                    </div>
                  ))}
                  
                  {product.voltage && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('common.fields.voltage')}</span>
                      <span className="font-medium">{product.voltage}</span>
                    </div>
                  )}
                  {product.power && (
                    <div className="flex justify-between py-1 border-b border-dashed">
                      <span className="text-slate-500">{t('common.fields.power')}</span>
                      <span className="font-medium">{product.power}</span>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {product.description && (
              <div>
                <h3 className="font-semibold text-slate-800 dark:text-white border-b pb-2 mb-2">{t('common.fields.description')}</h3>
                <p className="text-sm text-slate-600 dark:text-slate-300 leading-relaxed max-h-40 overflow-y-auto">
                  {product.description}
                </p>
              </div>
            )}

            <div className="flex justify-end pt-4">
              <Button onClick={onClose} variant="ghost">
                {t('common.actions.close')}
              </Button>
            </div>
          </div>
      </Modal>
  );
};
