import React from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { RefreshCw } from 'lucide-react';
import { Button } from '@/components/ui';
import SharedImage from '@/components/common/Image';
import { ProductReviewButton } from '@/components/reviews/ProductReviewButton';
import ReviewsList from '@/components/reviews/ReviewsList';
import { ProductDetail, ProductDetailType } from '@/types';
import { formatCurrency } from '@/utils/format';
import type { Product } from '@/types';
import { useReviewStore } from '@/store/reviewStore';

interface ProductTabsProps {
  product: Product;
  activeTab: 'description' | 'specs' | 'units' | 'variants' | 'images' | 'reviews';
  setActiveTab: (tab: 'description' | 'specs' | 'units' | 'variants' | 'images' | 'reviews') => void;
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
        <div className="flex border-b mb-6 overflow-x-auto">
          {(['description', 'specs', 'units', 'variants', 'images', 'reviews'] as const).map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`px-6 py-3 font-medium whitespace-nowrap transition-colors ${
                activeTab === tab
                  ? 'text-primary border-b-2 border-primary'
                  : 'text-secondary hover:text-primary'
              }`}
            >
              {t(`product.tab.${tab}`)}
            </button>
          ))}
        </div>

        {/* Tab Content */}
        {activeTab === 'description' && (
          <div className="prose max-w-none">
            <p className="text-secondary leading-relaxed">{product.description}</p>
          </div>
        )}

        {activeTab === 'specs' && (
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
        )}

        {activeTab === 'units' && (
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
        )}

        {activeTab === 'variants' && (
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
                    <tr key={idx} className="border-b border-slate-50 hover:bg-secondary/20">
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
        )}

        {activeTab === 'images' && (
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
                  <div className="absolute inset-0 bg-black/0 group-hover:bg-black/5 transition-colors" />
                </div>
              ))
            ) : (
              <div className="col-span-full text-center py-12 text-secondary bg-secondary/30 rounded-xl">
                {t('product.noImages', 'Không có hình ảnh nào.')}
              </div>
            )}
          </div>
        )}

        {activeTab === 'reviews' && (
          <div className="space-y-6">
            <div className="flex items-center justify-between bg-gradient-to-r from-indigo-50 to-purple-50 dark:from-indigo-900/20 dark:to-purple-900/20 p-4 rounded-xl">
              <div>
                <h3 className="font-semibold text-lg text-slate-800 dark:text-slate-200">
                  {t('review.shareExperience', 'Chia sẻ trải nghiệm của bạn')}
                </h3>
                <p className="text-sm text-slate-500 dark:text-slate-400">
                  {t('review.helpOthers', 'Đánh giá của bạn sẽ giúp người khác tham khảo')}
                </p>
              </div>
              <ProductReviewButton
                productCode={product.code}
                productName={product.name}
                productImage={product.images?.[0]}
                onReviewSubmitted={() => {
                  useReviewStore.getState().triggerRefresh();
                }}
              />
            </div>
            <ReviewsList productCode={product.code} />
          </div>
        )}
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
