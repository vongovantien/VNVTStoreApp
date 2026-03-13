import React from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { RefreshCw } from 'lucide-react';
import { Button, Tabs } from '@/components/ui';
import { ProductDetail } from '@/types';
import type { Product } from '@/types';
import { ProductDescription } from './tabs/ProductDescription';
import { ProductSpecs } from './tabs/ProductSpecs';
import { ProductUnits } from './tabs/ProductUnits';
import { ProductVariants } from './tabs/ProductVariants';
import { ProductMediaGrid } from './tabs/ProductMediaGrid';
import { ProductReviews } from './tabs/ProductReviews';
import { ProductQA } from './ProductQA';

interface ProductTabsProps {
  product: Product;
  activeTab: string;
  setActiveTab: (tab: string) => void;
  images: string[];
  relations: ProductDetail[];
  setSelectedIndex: (index: number) => void;
  setLightboxOpen: (open: boolean) => void;
}

export const ProductTabs: React.FC<ProductTabsProps> = ({
  product,
  activeTab,
  setActiveTab,
  images,
  relations,
  setSelectedIndex,
  setLightboxOpen
}) => {
  const { t } = useTranslation();

  return (
    <>
      <div className="bg-primary rounded-xl p-6 mb-12">
        <Tabs value={activeTab} onValueChange={setActiveTab}>
            <Tabs.List>
                {(['description', 'specs', 'units', 'variants', 'images', 'reviews', 'qa'] as const).map((tab) => (
                    <Tabs.Trigger key={tab} value={tab}>
                        {t(`product.tab.${tab}`)}
                    </Tabs.Trigger>
                ))}
            </Tabs.List>

            <Tabs.Content value="description">
                <ProductDescription product={product} />
            </Tabs.Content>
            
            <Tabs.Content value="specs">
                <ProductSpecs product={product} />
            </Tabs.Content>

            <Tabs.Content value="units">
                <ProductUnits product={product} />
            </Tabs.Content>

            <Tabs.Content value="variants">
                <ProductVariants product={product} />
            </Tabs.Content>
            
            <Tabs.Content value="images">
                 <ProductMediaGrid 
                    product={product} 
                    images={images} 
                    setSelectedIndex={setSelectedIndex} 
                    setLightboxOpen={setLightboxOpen} 
                />
            </Tabs.Content>

            <Tabs.Content value="reviews">
                <ProductReviews product={product} />
            </Tabs.Content>
            
            <Tabs.Content value="qa">
                <ProductQA productCode={product.code} />
            </Tabs.Content>
        </Tabs>
      </div>

      {/* Product Relations (Cross-selling) */}
      {relations.length > 0 && (
        <div className="mb-12">
          <h2 className="text-2xl font-bold mb-6 flex items-center gap-2">
            <RefreshCw className="text-indigo-500" size={24} />
            {t('product.frequentlyBoughtTogether', 'Thường được mua cùng (Phụ kiện)')}
          </h2>
          <div className="flex gap-4 overflow-x-auto pb-4 scrollbar-hide">
            {relations.map((rel, idx) => (
              <Link 
                key={idx}
                to={`/products?search=${encodeURIComponent(rel.specValue)}`}
                className="flex-shrink-0"
              >
                <Button variant="outline" className="h-auto py-3 px-6 rounded-2xl border-indigo-100 hover:border-indigo-500 hover:bg-indigo-50 transition-all flex flex-col items-center gap-1 group">
                  <span className="text-xs text-tertiary">{t('product.suggestion', 'Gợi ý')}:</span>
                  <span className="font-bold text-indigo-600 group-hover:scale-105 transition-transform">{rel.specValue}</span>
                </Button>
              </Link>
            ))}
          </div>
        </div>
      )}
    </>
  );
};
