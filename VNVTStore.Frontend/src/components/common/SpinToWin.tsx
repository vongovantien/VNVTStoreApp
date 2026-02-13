/**
 * Feature #56: Gamified Spin-to-Win Coupon Wheel
 * Self-contained, uses localStorage for state management.
 */
import { useState, useRef, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { X, Gift, Copy, Check } from 'lucide-react';

const SPIN_KEY = 'vnvt_spin_result';

interface WheelSegment {
  label: string;
  code: string;
  color: string;
  probability: number;
}

const SEGMENTS: WheelSegment[] = [
  { label: 'Giảm 5%', code: 'SPIN5', color: '#6366f1', probability: 25 },
  { label: 'Giảm 10%', code: 'SPIN10', color: '#8b5cf6', probability: 20 },
  { label: 'Giảm 15%', code: 'SPIN15', color: '#a855f7', probability: 15 },
  { label: 'Giảm 20%', code: 'SPIN20', color: '#d946ef', probability: 8 },
  { label: 'FreeShip', code: 'FREESHIP', color: '#ec4899', probability: 12 },
  { label: 'Thử lại', code: '', color: '#f43f5e', probability: 20 },
];

function getWeightedRandom(): number {
  const total = SEGMENTS.reduce((sum, s) => sum + s.probability, 0);
  let random = Math.random() * total;
  for (let i = 0; i < SEGMENTS.length; i++) {
    random -= SEGMENTS[i].probability;
    if (random <= 0) return i;
  }
  return SEGMENTS.length - 1;
}

export const SpinToWin = ({ 
  isOpen, 
  onClose 
}: { 
  isOpen: boolean; 
  onClose: () => void;
}) => {
  const { t } = useTranslation();
  const [spinning, setSpinning] = useState(false);
  const [result, setResult] = useState<WheelSegment | null>(null);
  const [rotation, setRotation] = useState(0);
  const [copied, setCopied] = useState(false);
  const hasSpun = !!localStorage.getItem(SPIN_KEY);

  const handleSpin = useCallback(() => {
    if (spinning || hasSpun) return;
    
    const winnerIdx = getWeightedRandom();
    const segmentAngle = 360 / SEGMENTS.length;
    // Calculate rotation to land on the winning segment
    const targetAngle = 360 - (winnerIdx * segmentAngle + segmentAngle / 2);
    const totalRotation = rotation + 1440 + targetAngle; // 4 full spins + offset
    
    setSpinning(true);
    setRotation(totalRotation);
    
    setTimeout(() => {
      setSpinning(false);
      setResult(SEGMENTS[winnerIdx]);
      if (SEGMENTS[winnerIdx].code) {
        localStorage.setItem(SPIN_KEY, JSON.stringify(SEGMENTS[winnerIdx]));
      }
    }, 4000);
  }, [spinning, hasSpun, rotation]);

  const handleCopy = useCallback(() => {
    if (result?.code) {
      navigator.clipboard.writeText(result.code);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  }, [result]);

  const segmentAngle = 360 / SEGMENTS.length;

  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          className="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4"
          onClick={onClose}
        >
          <motion.div
            initial={{ scale: 0.8 }}
            animate={{ scale: 1 }}
            exit={{ scale: 0.8 }}
            onClick={(e) => e.stopPropagation()}
            className="relative w-full max-w-md bg-gradient-to-b from-indigo-900 to-purple-900 rounded-3xl p-6 text-white shadow-2xl"
          >
            <button onClick={onClose} className="absolute top-4 right-4 p-1 hover:bg-white/20 rounded-full z-10">
              <X size={20} />
            </button>

            <div className="text-center mb-6">
              <Gift size={24} className="mx-auto mb-2" />
              <h2 className="text-xl font-bold">{t('spin.title', 'Vòng quay may mắn!')}</h2>
              <p className="text-sm text-white/60">{t('spin.desc', 'Quay để nhận ưu đãi hấp dẫn')}</p>
            </div>

            {/* Wheel */}
            <div className="relative w-64 h-64 mx-auto mb-6">
              {/* Pointer */}
              <div className="absolute -top-2 left-1/2 -translate-x-1/2 z-10 w-0 h-0 border-l-[12px] border-r-[12px] border-t-[20px] border-transparent border-t-yellow-400 drop-shadow-lg" />
              
              {/* Spinning wheel using CSS */}
              <div 
                className="w-full h-full rounded-full overflow-hidden border-4 border-yellow-400 shadow-2xl"
                style={{
                  transform: `rotate(${rotation}deg)`,
                  transition: spinning ? 'transform 4s cubic-bezier(0.17, 0.67, 0.12, 0.99)' : undefined,
                }}
              >
                <svg viewBox="0 0 200 200" className="w-full h-full">
                  {SEGMENTS.map((seg, i) => {
                    const startAngle = (i * segmentAngle * Math.PI) / 180;
                    const endAngle = ((i + 1) * segmentAngle * Math.PI) / 180;
                    const x1 = 100 + 100 * Math.cos(startAngle);
                    const y1 = 100 + 100 * Math.sin(startAngle);
                    const x2 = 100 + 100 * Math.cos(endAngle);
                    const y2 = 100 + 100 * Math.sin(endAngle);
                    const largeArc = segmentAngle > 180 ? 1 : 0;
                    const midAngle = ((i + 0.5) * segmentAngle * Math.PI) / 180;
                    const tx = 100 + 60 * Math.cos(midAngle);
                    const ty = 100 + 60 * Math.sin(midAngle);

                    return (
                      <g key={i}>
                        <path
                          d={`M100,100 L${x1},${y1} A100,100 0 ${largeArc},1 ${x2},${y2} Z`}
                          fill={seg.color}
                          stroke="white"
                          strokeWidth="1"
                        />
                        <text
                          x={tx}
                          y={ty}
                          textAnchor="middle"
                          dominantBaseline="middle"
                          fill="white"
                          fontSize="10"
                          fontWeight="bold"
                          transform={`rotate(${(i + 0.5) * segmentAngle}, ${tx}, ${ty})`}
                        >
                          {seg.label}
                        </text>
                      </g>
                    );
                  })}
                </svg>
              </div>
            </div>

            {/* Spin Button or Result */}
            {!result ? (
              <button
                onClick={handleSpin}
                disabled={spinning || hasSpun}
                className="w-full py-3 bg-yellow-400 text-indigo-900 font-bold rounded-xl hover:bg-yellow-300 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {hasSpun ? t('spin.alreadySpun', 'Bạn đã quay rồi!') : spinning ? '🎰 Đang quay...' : t('spin.button', '🎯 Quay ngay!')}
              </button>
            ) : (
              <div className="text-center">
                {result.code ? (
                  <>
                    <p className="text-lg font-bold text-yellow-400 mb-2">🎉 {result.label}!</p>
                    <div className="flex items-center justify-center gap-2 bg-white/10 rounded-xl px-4 py-3 mb-3">
                      <span className="font-mono font-bold text-lg tracking-wider">{result.code}</span>
                      <button onClick={handleCopy} className="p-1.5 bg-white/20 rounded-lg hover:bg-white/30">
                        {copied ? <Check size={16} /> : <Copy size={16} />}
                      </button>
                    </div>
                    <button onClick={onClose} className="text-sm text-white/60 hover:text-white">
                      {t('popup.continueShopping', 'Tiếp tục mua sắm')}
                    </button>
                  </>
                ) : (
                  <>
                    <p className="text-white/80 mb-3">{t('spin.tryAgain', 'Chúc bạn may mắn lần sau!')}</p>
                    <button onClick={onClose} className="text-sm text-white/60 hover:text-white">
                      {t('common.close', 'Đóng')}
                    </button>
                  </>
                )}
              </div>
            )}
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};
