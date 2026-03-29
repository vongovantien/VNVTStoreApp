import { useTranslation } from 'react-i18next';
import { Package, Star } from 'lucide-react';
import { Badge } from '@/components/ui';
import { formatCurrency, getImageUrl } from '@/utils/format';
import type { Product } from '@/types';
import { type DataTableColumn, CommonColumns } from '@/components/common/DataTable';

export const useProductColumns = () => {
  const { t, i18n } = useTranslation();

  const getSafeImageUrl = (img: string | { imageURL?: string; imageUrl?: string; ImageURL?: string } | null | undefined) => {
      if (!img) return '';
      if (typeof img === 'string') return getImageUrl(img);
      const imageObj = img as { imageURL?: string; imageUrl?: string; ImageURL?: string };
      return getImageUrl(imageObj.imageURL || imageObj.imageUrl || imageObj.ImageURL || '');
  };

  const columns: DataTableColumn<Product>[] = [
    {
      id: 'image',
      header: t('common.fields.image'),
      width: '120px',
      className: 'text-center',
      accessor: (product) => (
        <div className="flex flex-col items-center">
            <div className="w-10 h-10 rounded-lg overflow-hidden bg-gray-100 dark:bg-slate-700 border border-gray-200 dark:border-slate-600">
               {(product.images?.[0] || product.productImages?.[0]) ? (
                 <img 
                   src={getSafeImageUrl(product.images?.[0] || product.productImages?.[0])} 
                   alt={product.name} 
                   className="w-full h-full object-cover"
                   onError={(e) => console.error("Img Error:", e.currentTarget.src)}
                 />
               ) : (
                 <div className="w-full h-full flex items-center justify-center text-gray-400">
                   <Package size={18} />
                 </div>
               )}
            </div>
        </div>
      )
    },
    {
      id: 'name',
      header: t('common.fields.name'),
      accessor: (product) => (
        <div>
          <p className="font-medium text-sm text-slate-700 dark:text-slate-200">{product.name}</p>
          <p className="text-xs text-slate-500">{product.brand}</p>
        </div>
      ),
      sortable: true
    },
    {
      id: 'category',
      header: t('common.fields.category'),
      accessor: 'category',
    },
    {
      id: 'price',
      header: t('common.fields.price'),
      accessor: (product) => (
        product.price > 0 ? (
          <span className="font-semibold text-rose-600">{formatCurrency(product.price)}</span>
        ) : (
          <Badge color="primary" size="sm">{t('common.fields.contact')}</Badge>
        )
      ),
      className: 'text-right',
      headerClassName: 'text-right',
      sortable: true
    },
    {
      id: 'stock',
      header: t('common.fields.stock'),
      accessor: (product) => (
        <span className={product.stock > 10 ? 'text-emerald-600 font-medium' : product.stock > 0 ? 'text-amber-600 font-medium' : 'text-rose-600 font-medium'}>
          {product.stock}
        </span>
      ),
      className: 'text-center',
      headerClassName: 'text-center',
      sortable: true
    },
    {
      id: 'rating',
      header: t('common.fields.rating'),
      accessor: (product) => (
        <div className="flex items-center justify-center gap-1 text-sm">
          <Star size={16} className="text-amber-400 fill-amber-400" />
          <span>{product.rating} ({product.reviewCount})</span>
        </div>
      ),
      className: 'text-center',
      headerClassName: 'text-center'
    },
    CommonColumns.createStatusColumn(t),
    {
      id: 'createdAt',
      header: t('common.fields.createdAt') || 'Created At',
      accessor: (product) => (
        <span className="text-sm text-slate-500">
           {product.createdAt ? new Date(product.createdAt).toLocaleDateString(i18n.language, { year: 'numeric', month: 'short', day: 'numeric' }) : '-'}
        </span>
      ),
      className: 'text-center',
      headerClassName: 'text-center',
      sortable: true
    }
  ];

  return columns;
};
