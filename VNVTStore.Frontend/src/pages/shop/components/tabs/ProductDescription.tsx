import React from 'react';
import type { Product } from '@/types';

interface ProductDescriptionProps {
  product: Product;
}

export const ProductDescription: React.FC<ProductDescriptionProps> = ({ product }) => {
  return (
    <div className="prose max-w-none">
      <p className="text-secondary leading-relaxed">{product.description}</p>
    </div>
  );
};
