import React from 'react';
import { useTranslation } from 'react-i18next';
import SharedImage from '@/components/common/Image';
import type { Product } from '@/types';

interface ProductMediaGridProps {
  product: Product;
  images: string[];
  setSelectedIndex: (index: number) => void;
  setLightboxOpen: (open: boolean) => void;
}

export const ProductMediaGrid: React.FC<ProductMediaGridProps> = ({ 
  product, 
  images, 
  setSelectedIndex, 
  setLightboxOpen 
}) => {
  const { t } = useTranslation();

  return (
    <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
      {images.length > 0 ? (
        images.map((img, idx) => (
          <div 
            key={idx} 
            className="aspect-square rounded-xl overflow-hidden border border-slate-100 bg-white group cursor-zoom-in relative"
            onClick={() => {
              setSelectedIndex(idx);
              setLightboxOpen(true);
            }}
          >
            <SharedImage 
              src={img} 
              alt={`${product.name} gallery ${idx + 1}`} 
              className="w-full h-full object-contain transition-transform group-hover:scale-110" 
            />
            <div className="absolute inset-0 bg-black/0 group-hover:bg-hover transition-colors" />
          </div>
        ))
      ) : (
        <div className="col-span-full text-center py-12 text-secondary bg-secondary/30 rounded-xl">
          {t('product.noImages', 'Không có hình ảnh nào.')}
        </div>
      )}
    </div>
  );
};
