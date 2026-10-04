import React from 'react';
import type { Product } from '@/types';
import { createSafeHTML } from '@/utils/sanitize';

interface ProductDescriptionProps {
  product: Product;
}

export const ProductDescription: React.FC<ProductDescriptionProps> = ({ product }) => {
  return (
    <div className="prose max-w-none">
      {/* 🛡️ XSS PROTECTION: Sanitize product description HTML */}
      <div 
        className="text-secondary leading-relaxed"
        dangerouslySetInnerHTML={createSafeHTML(product.description || '')}
      />
    </div>
  );
};
