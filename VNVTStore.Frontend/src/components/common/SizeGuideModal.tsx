/**
 * Feature #11: Size Guide Calculator
 * Input Height/Weight to get recommended size — self-contained, no third party.
 */
import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { X, Ruler, Info } from 'lucide-react';

interface ClothingSizeRow {
  size: string;
  minHeight: number;
  maxHeight: number;
  minWeight: number;
  maxWeight: number;
  chest: string;
  waist: string;
}

interface ShoeSizeRow {
  size: string;
  minHeight: number;
  maxHeight: number;
  minWeight: number;
  maxWeight: number;
  foot: string;
}

interface SizeGuideProps {
  isOpen: boolean;
  onClose: () => void;
  productType?: 'clothing' | 'shoes' | 'general';
}

const SIZE_CHART = {
  clothing: [
    { size: 'XS', minHeight: 150, maxHeight: 158, minWeight: 40, maxWeight: 48, chest: '76-80', waist: '60-64' },
    { size: 'S',  minHeight: 155, maxHeight: 163, minWeight: 45, maxWeight: 55, chest: '80-84', waist: '64-68' },
    { size: 'M',  minHeight: 160, maxHeight: 170, minWeight: 55, maxWeight: 65, chest: '84-92', waist: '68-76' },
    { size: 'L',  minHeight: 168, maxHeight: 178, minWeight: 65, maxWeight: 78, chest: '92-100', waist: '76-84' },
    { size: 'XL', minHeight: 175, maxHeight: 185, minWeight: 75, maxWeight: 90, chest: '100-108', waist: '84-92' },
    { size: 'XXL', minHeight: 180, maxHeight: 195, minWeight: 85, maxWeight: 105, chest: '108-116', waist: '92-100' },
  ],
  shoes: [
    { size: '38', minHeight: 155, maxHeight: 165, minWeight: 0, maxWeight: 200, foot: '24 cm' },
    { size: '39', minHeight: 160, maxHeight: 170, minWeight: 0, maxWeight: 200, foot: '24.5 cm' },
    { size: '40', minHeight: 165, maxHeight: 175, minWeight: 0, maxWeight: 200, foot: '25 cm' },
    { size: '41', minHeight: 168, maxHeight: 178, minWeight: 0, maxWeight: 200, foot: '25.5 cm' },
    { size: '42', minHeight: 172, maxHeight: 185, minWeight: 0, maxWeight: 200, foot: '26 cm' },
    { size: '43', minHeight: 178, maxHeight: 190, minWeight: 0, maxWeight: 200, foot: '27 cm' },
    { size: '44', minHeight: 183, maxHeight: 195, minWeight: 0, maxWeight: 200, foot: '27.5 cm' },
  ],
};

export const SizeGuideModal = ({ isOpen, onClose, productType = 'clothing' }: SizeGuideProps) => {
  const { t } = useTranslation();
  const [height, setHeight] = useState('');
  const [weight, setWeight] = useState('');

  const recommendation = useMemo(() => {
    const h = parseFloat(height);
    const w = parseFloat(weight);
    if (!h || h < 140 || h > 210) return null;
    
    const chart = SIZE_CHART[productType === 'shoes' ? 'shoes' : 'clothing'];
    
    // Find best matching size
    const matches = chart.filter(s => 
      h >= s.minHeight && h <= s.maxHeight && 
      (productType === 'shoes' || (w >= s.minWeight && w <= s.maxWeight))
    );
    
    if (matches.length > 0) return matches[0];
    
    // Find closest if no exact match
    const closest = chart.reduce((prev, curr) => {
      const prevDiff = Math.abs((prev.minHeight + prev.maxHeight) / 2 - h);
      const currDiff = Math.abs((curr.minHeight + curr.maxHeight) / 2 - h);
      return currDiff < prevDiff ? curr : prev;
    });
    return closest;
  }, [height, weight, productType]);

  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          className="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 backdrop-blur-sm p-4"
          onClick={onClose}
        >
          <motion.div
            initial={{ scale: 0.9, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            exit={{ scale: 0.9, opacity: 0 }}
            onClick={(e) => e.stopPropagation()}
            className="w-full max-w-lg bg-primary rounded-2xl shadow-2xl overflow-hidden"
          >
            {/* Header */}
            <div className="flex items-center justify-between p-5 border-b bg-gradient-to-r from-indigo-50 to-purple-50 dark:from-indigo-950/30 dark:to-purple-950/30">
              <div className="flex items-center gap-2">
                <Ruler size={20} className="text-indigo-600" />
                <h2 className="text-lg font-bold">{t('sizeGuide.title', 'Hướng dẫn chọn size')}</h2>
              </div>
              <button onClick={onClose} className="p-1 hover:bg-black/10 rounded-lg transition-colors">
                <X size={20} />
              </button>
            </div>

            {/* Input */}
            <div className="p-5">
              <div className="grid grid-cols-2 gap-4 mb-6">
                <div>
                  <label className="block text-sm font-medium mb-1.5">{t('sizeGuide.height', 'Chiều cao (cm)')}</label>
                  <input
                    type="number"
                    value={height}
                    onChange={(e) => setHeight(e.target.value)}
                    placeholder="170"
                    min="140" max="210"
                    className="w-full px-3 py-2.5 border rounded-xl focus:outline-none focus:border-indigo-500 bg-secondary text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1.5">{t('sizeGuide.weight', 'Cân nặng (kg)')}</label>
                  <input
                    type="number"
                    value={weight}
                    onChange={(e) => setWeight(e.target.value)}
                    placeholder="65"
                    min="30" max="150"
                    className="w-full px-3 py-2.5 border rounded-xl focus:outline-none focus:border-indigo-500 bg-secondary text-sm"
                  />
                </div>
              </div>

              {/* Recommendation */}
              {recommendation && (
                <motion.div
                  initial={{ y: 10, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                  className="p-4 bg-green-50 dark:bg-green-950/30 border border-green-200 dark:border-green-800 rounded-xl mb-6"
                >
                  <div className="flex items-center gap-2 mb-2">
                    <Info size={16} className="text-green-600" />
                    <span className="text-sm font-medium text-green-800 dark:text-green-300">
                      {t('sizeGuide.recommended', 'Size gợi ý cho bạn')}
                    </span>
                  </div>
                  <div className="text-3xl font-bold text-green-700 dark:text-green-400">
                    {recommendation.size}
                  </div>
                </motion.div>
              )}

              {/* Size Chart Table */}
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="text-left py-2 px-2 font-semibold">Size</th>
                      <th className="text-left py-2 px-2 font-semibold">{t('sizeGuide.height', 'Chiều cao')}</th>
                      <th className="text-left py-2 px-2 font-semibold">{t('sizeGuide.weight', 'Cân nặng')}</th>
                      {productType === 'clothing' && (
                        <>
                          <th className="text-left py-2 px-2 font-semibold">{t('sizeGuide.chest', 'Ngực')}</th>
                          <th className="text-left py-2 px-2 font-semibold">{t('sizeGuide.waist', 'Eo')}</th>
                        </>
                      )}
                      {productType === 'shoes' && (
                        <th className="text-left py-2 px-2 font-semibold">{t('sizeGuide.foot', 'Chân')}</th>
                      )}
                    </tr>
                  </thead>
                  <tbody>
                    {(SIZE_CHART[productType === 'shoes' ? 'shoes' : 'clothing']).map((row) => (
                      <tr
                        key={row.size}
                        className={`border-b transition-colors ${
                          recommendation?.size === row.size
                            ? 'bg-indigo-50 dark:bg-indigo-950/30 font-semibold'
                            : 'hover:bg-hover'
                        }`}
                      >
                        <td className="py-2 px-2 font-bold">{row.size}</td>
                        <td className="py-2 px-2">{row.minHeight}-{row.maxHeight}cm</td>
                        <td className="py-2 px-2">{row.minWeight}-{row.maxWeight}kg</td>
                        {productType === 'clothing' && (
                          <>
                            <td className="py-2 px-2">{(row as ClothingSizeRow).chest}cm</td>
                            <td className="py-2 px-2">{(row as ClothingSizeRow).waist}cm</td>
                          </>
                        )}
                        {productType === 'shoes' && (
                          <td className="py-2 px-2">{(row as ShoeSizeRow).foot}</td>
                        )}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};
