import { memo } from 'react';
import type { ReactNode } from 'react';
import { motion } from 'framer-motion';
import { cn } from '@/utils/cn';

interface FadeInSectionProps {
  children: ReactNode;
  className?: string;
  /** Delay in seconds before animation starts */
  delay?: number;
  /** Direction to animate in from */
  direction?: 'up' | 'down' | 'left' | 'right' | 'none';
  /** Amount to translate (pixels) */
  distance?: number;
}

/**
 * Wrapper that fades + slides in when scrolled into view.
 * Uses viewport={{ once: true }} so the animation only fires once — not on every scroll.
 */
export const FadeInSection = memo(({
  children,
  className,
  delay = 0,
  direction = 'up',
  distance = 20,
}: FadeInSectionProps) => {
  const directionMap: Record<string, { x?: number; y?: number }> = {
    up:    { y: distance },
    down:  { y: -distance },
    left:  { x: distance },
    right: { x: -distance },
    none:  {},
  };

  const initial = { opacity: 0, ...directionMap[direction] };
  const animate = { opacity: 1, x: 0, y: 0 };

  return (
    <motion.div
      initial={initial}
      whileInView={animate}
      viewport={{ once: true, margin: '-60px' }}
      transition={{ duration: 0.45, delay, ease: [0.22, 1, 0.36, 1] }}
      className={cn(className)}
    >
      {children}
    </motion.div>
  );
});

FadeInSection.displayName = 'FadeInSection';

export default FadeInSection;
