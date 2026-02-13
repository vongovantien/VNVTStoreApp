import React from 'react';
import { useTranslation } from 'react-i18next';
import { X, ShoppingCart, Check, Minus } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { Product } from '@/types';
import { Button } from '@/components/ui';
import { formatCurrency } from '@/utils/format';
import { cn } from '@/utils/cn';
import CustomImage from '@/components/common/Image';

interface CompareTableProps {
  items: Product[];
  onRemove: (id: string) => void;
  onAddToCart: (product: Product) => void;
  onClose: () => void;
}

export const CompareTable: React.FC<CompareTableProps> = ({ items, onRemove, onAddToCart, onClose }) => {
  const { t } = useTranslation();

  if (items.length === 0) return null;

  const specs = [
    { label: 'Thương hiệu', key: 'brand' },
    { label: 'Danh mục', key: 'category' },
    { label: 'Công suất', key: 'power' },
    { label: 'Điện áp', key: 'voltage' },
    { label: 'Màu sắc', key: 'color' },
    { label: 'Chất liệu', key: 'material' },
    { label: 'Kích thước', key: 'size' },
    { label: 'Kho hàng', key: 'stock', render: (val: number) => val > 0 ? <span className="text-green-600">Còn hàng ({val})</span> : <span className="text-red-600">Hết hàng</span> },
  ];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <motion.div
        initial={{ opacity: 0, scale: 0.95, y: 20 }}
        animate={{ opacity: 1, scale: 1, y: 0 }}
        exit={{ opacity: 0, scale: 0.95, y: 20 }}
        className="bg-white dark:bg-slate-900 w-full max-w-6xl max-h-[90vh] overflow-hidden rounded-2xl shadow-2xl flex flex-col"
      >
        {/* Header */}
        <div className="p-6 border-b flex justify-between items-center bg-indigo-600 text-white">
          <div>
            <h2 className="text-2xl font-bold">So sánh sản phẩm</h2>
            <p className="text-indigo-100 text-sm">So sánh tối đa 3 sản phẩm cùng lúc</p>
          </div>
          <button onClick={onClose} className="p-2 hover:bg-white/10 rounded-full transition-colors">
            <X size={24} />
          </button>
        </div>

        {/* Table Container */}
        <div className="flex-1 overflow-auto custom-scrollbar">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-slate-50 dark:bg-slate-800/50">
                <th className="p-6 border-b w-1/4 font-semibold text-secondary">Đặc tính</th>
                {items.map((product) => (
                  <th key={product.code} className="p-6 border-b min-w-[250px] relative group">
                    <button 
                      onClick={() => onRemove(product.code)}
                      className="absolute top-2 right-2 p-1 text-slate-400 hover:text-red-500 opacity-0 group-hover:opacity-100 transition-opacity"
                    >
                      <X size={16} />
                    </button>
                    <div className="space-y-4">
                      <div className="aspect-square rounded-xl overflow-hidden bg-white border">
                        <CustomImage src={product.image} alt={product.name} className="w-full h-full object-contain p-4" />
                      </div>
                      <div className="space-y-1">
                        <h3 className="font-bold text-sm line-clamp-2 min-h-[40px]">{product.name}</h3>
                        <p className="text-indigo-600 text-lg font-bold">{formatCurrency(product.price)}</p>
                      </div>
                      <Button 
                        fullWidth 
                        size="sm" 
                        variant="primary"
                        leftIcon={<ShoppingCart size={16} />}
                        onClick={() => onAddToCart(product)}
                      >
                        Thêm vào giỏ
                      </Button>
                    </div>
                  </th>
                ))}
                {/* Empty slots */}
                {Array.from({ length: Math.max(0, 3 - items.length) }).map((_, i) => (
                  <th key={i} className="p-6 border-b min-w-[250px] bg-slate-50/50 dark:bg-slate-800/20">
                    <div className="h-full flex flex-col items-center justify-center text-slate-400 py-20 border-2 border-dashed border-slate-200 dark:border-slate-700 rounded-xl">
                      <Minus size={32} strokeWidth={1} />
                      <span className="text-xs mt-2 uppercase tracking-wider font-semibold">Chống</span>
                    </div>
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {specs.map((spec) => (
                <tr key={spec.key} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/30 transition-colors">
                  <td className="p-4 px-6 border-b font-medium text-slate-500 text-sm whitespace-nowrap bg-slate-50/30 dark:bg-slate-800/10">
                    {spec.label}
                  </td>
                  {items.map((product) => (
                    <td key={`${product.code}-${spec.key}`} className="p-4 px-6 border-b text-sm">
                      {spec.render 
                        ? (spec.render as (val: unknown) => React.ReactNode)(product[spec.key as keyof Product]) 
                        : (product[spec.key as keyof Product] as React.ReactNode || <span className="text-slate-300">---</span>)}
                    </td>
                  ))}
                  {Array.from({ length: Math.max(0, 3 - items.length) }).map((_, i) => (
                    <td key={i} className="p-4 px-6 border-b bg-slate-50/20 dark:bg-slate-800/10"></td>
                  ))}
                </tr>
              ))}
              <tr>
                 <td className="p-4 px-6 border-b font-medium text-slate-500 text-sm whitespace-nowrap bg-slate-50/30 dark:bg-slate-800/10">
                    Mô tả
                 </td>
                 {items.map((product) => (
                    <td key={`${product.code}-desc`} className="p-4 px-6 border-b text-sm align-top">
                       <p className="line-clamp-6 text-slate-600 dark:text-slate-400 leading-relaxed italic border-l-2 border-indigo-100 pl-3">
                        {product.description || 'Chưa có mô tả chi tiết'}
                       </p>
                    </td>
                 ))}
                 {Array.from({ length: Math.max(0, 3 - items.length) }).map((_, i) => (
                    <td key={i} className="p-4 px-6 border-b bg-slate-50/20 dark:bg-slate-800/10"></td>
                  ))}
              </tr>
            </tbody>
          </table>
        </div>

        {/* Footer */}
        <div className="p-4 px-8 border-t bg-slate-50 dark:bg-slate-800/50 flex justify-between items-center">
            <div className="flex items-center gap-2 text-sm text-slate-500">
                <Check size={16} className="text-green-500" />
                Dữ liệu được cập nhật thời gian thực
            </div>
            <Button variant="ghost" onClick={onClose}>Đóng cửa sổ</Button>
        </div>
      </motion.div>
    </div>
  );
};
