import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X, Ruler, CheckCircle2, Info } from 'lucide-react';
import { Button } from '@/components/ui';

interface SizeGuideProps {
  category?: string | undefined;
  onClose: () => void;
}

export const SizeGuideDrawer: React.FC<SizeGuideProps> = ({ category, onClose }) => {
  const [selectedSize, setSelectedSize] = useState<string | null>(null);

  type ApparelRow = { size: string; chest: string; waist: string; hip: string };
  type SpecRow = { spec: string; value: string };
  type GuideData = { unit: string; rows: ApparelRow[] | SpecRow[] };

  const isApparelGuide = (g: GuideData): g is { unit: string; rows: ApparelRow[] } => {
    return 'size' in (g.rows[0] || {});
  };

  const getGuideData = () => {
    if (category?.toLowerCase().includes('áo') || category?.toLowerCase().includes('quần')) {
      return {
        unit: 'cm',
        rows: [
          { size: 'S', chest: '86-90', waist: '70-74', hip: '90-94' },
          { size: 'M', chest: '91-95', waist: '75-79', hip: '95-99' },
          { size: 'L', chest: '96-100', waist: '80-84', hip: '100-104' },
          { size: 'XL', chest: '101-105', waist: '85-89', hip: '105-109' },
        ]
      };
    }
    return {
      unit: 'inch',
      rows: [
        { spec: 'Chiều cao', value: '45.5"' },
        { spec: 'Chiều rộng', value: '24.2"' },
        { spec: 'Độ dày', value: '2.5"' },
      ]
    };
  };

  const guide = getGuideData();

  return (
    <AnimatePresence>
      {category && (
        <div className="fixed inset-0 z-[200] flex justify-end pointer-events-none">
          <motion.div 
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={onClose}
            className="absolute inset-0 bg-black/40 backdrop-blur-sm pointer-events-auto"
          />
          
          <motion.div
            initial={{ x: '100%' }}
            animate={{ x: 0 }}
            exit={{ x: '100%' }}
            transition={{ type: 'spring', damping: 25, stiffness: 200 }}
            className="relative w-full max-w-md bg-white dark:bg-slate-900 shadow-2xl h-full flex flex-col pointer-events-auto"
          >
            <div className="p-6 border-b border-slate-100 dark:border-slate-800 flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Ruler className="text-primary" />
                <h2 className="text-xl font-bold">Hướng dẫn kích thước</h2>
              </div>
              <button onClick={onClose} className="p-2 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-full transition-colors">
                <X size={20} />
              </button>
            </div>

            <div className="flex-1 overflow-y-auto p-6 space-y-8">
              <div className="bg-blue-50 dark:bg-blue-900/20 p-4 rounded-xl flex gap-3 text-sm text-blue-700 dark:text-blue-400">
                <Info className="flex-shrink-0" size={18} />
                <p>Chọn kích thước thường dùng của bạn để xem khuyến nghị từ hệ thống phân tích AI của chúng tôi.</p>
              </div>

              <div className="space-y-4">
                <h3 className="font-semibold text-sm uppercase tracking-wider text-slate-500">Bảng quy đổi ({guide.unit})</h3>
                <div className="border border-slate-100 dark:border-slate-800 rounded-xl overflow-hidden">
                  <table className="w-full text-sm text-left">
                    <thead className="bg-slate-50 dark:bg-slate-800/50">
                      <tr>
                        {isApparelGuide(guide) ? (
                          <>
                            <th className="px-4 py-3 font-medium">Size</th>
                            <th className="px-4 py-3 font-medium">Ngực</th>
                            <th className="px-4 py-3 font-medium">Eo</th>
                            <th className="px-4 py-3 font-medium">Hông</th>
                          </>
                        ) : (
                          <>
                            <th className="px-4 py-3 font-medium">Thông số</th>
                            <th className="px-4 py-3 font-medium">Giá trị</th>
                          </>
                        )}
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
                      {guide.rows.map((row, idx: number) => (
                        <tr 
                          key={idx} 
                          onClick={() => {
                            if (isApparelGuide(guide)) {
                              const r = row as ApparelRow;
                              setSelectedSize(r.size);
                            }
                          }}
                          className={cn(
                            "transition-colors cursor-pointer",
                            isApparelGuide(guide) && selectedSize === (row as ApparelRow).size ? "bg-primary/5" : "hover:bg-slate-50 dark:hover:bg-slate-800/30"
                          )}
                        >
                          {isApparelGuide(guide) ? (
                            <>
                              <td className="px-4 py-3 font-bold text-primary">{(row as ApparelRow).size}</td>
                              <td className="px-4 py-3">{(row as ApparelRow).chest}</td>
                              <td className="px-4 py-3">{(row as ApparelRow).waist}</td>
                              <td className="px-4 py-3">{(row as ApparelRow).hip}</td>
                            </>
                          ) : (
                            <>
                              <td className="px-4 py-3 font-medium">{(row as SpecRow).spec}</td>
                              <td className="px-4 py-3 text-slate-500">{(row as SpecRow).value}</td>
                            </>
                          )}
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>

              {selectedSize && (
                <motion.div 
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="p-4 rounded-xl border-2 border-emerald-500 bg-emerald-50 dark:bg-emerald-900/20"
                >
                  <div className="flex items-center gap-2 text-emerald-600 dark:text-emerald-400 font-bold mb-1">
                    <CheckCircle2 size={18} />
                    Fit Assistant Recommendation
                  </div>
                  <p className="text-sm">Bạn nên chọn <strong>Size {selectedSize}</strong>. Dựa trên dữ liệu từ 12,000+ khách hàng có chỉ số tương tự, kích thước này sẽ mang lại cảm giác thoải mái nhất.</p>
                </motion.div>
              )}
            </div>

            <div className="p-6 border-t border-slate-100 dark:border-slate-800">
              <Button className="w-full" size="lg" onClick={onClose}>Xác nhận & Đóng</Button>
            </div>
          </motion.div>
        </div>
      )}
    </AnimatePresence>
  );
};

// Simple CN utility for this standalone component
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function cn(...classes: any[]) {
  return classes.filter(Boolean).join(' ');
}
