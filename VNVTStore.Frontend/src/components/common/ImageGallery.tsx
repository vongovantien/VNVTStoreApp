import React, { useState, useEffect, memo } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { ChevronRight, X } from 'lucide-react';
import SharedImage from '@/components/common/Image';

// ============ Image Lightbox Component ============
interface ImageLightboxProps {
  images: string[];
  initialIndex: number;
  isOpen: boolean;
  onClose: () => void;
}

const ImageLightbox = ({
  images,
  initialIndex,
  isOpen,
  onClose,
}: ImageLightboxProps) => {
  const [index, setIndex] = useState(initialIndex);
  const [prevInitialIndex, setPrevInitialIndex] = useState(initialIndex);
  const [prevIsOpen, setPrevIsOpen] = useState(isOpen);

  if (isOpen !== prevIsOpen || initialIndex !== prevInitialIndex) {
    setPrevIsOpen(isOpen);
    setPrevInitialIndex(initialIndex);
    if (isOpen) {
      setIndex(initialIndex);
    }
  }

  // Keyboard navigation
  useEffect(() => {
    if (!isOpen) return;
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
      if (e.key === 'ArrowLeft') setIndex((prev) => (prev > 0 ? prev - 1 : prev));
      if (e.key === 'ArrowRight') setIndex((prev) => (prev < images.length - 1 ? prev + 1 : prev));
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose, images.length]);

  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/90 backdrop-blur-sm"
          onClick={onClose}
        >
          {/* Close Button */}
          <button
            onClick={onClose}
            className="absolute top-4 right-4 p-2 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors z-50"
          >
            <X size={32} />
          </button>

          {/* Main Image Container */}
          <div className="relative w-full h-full flex items-center justify-center p-4 pointer-events-none">
            <motion.div
              key={index}
              initial={{ opacity: 0, scale: 0.9 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ type: "spring", stiffness: 300, damping: 30 }}
              className="relative max-w-full max-h-full pointer-events-auto"
              onClick={(e) => e.stopPropagation()} 
            >
              <SharedImage
                src={images[index]}
                alt={`Gallery image ${index + 1}`}
                className="max-w-full max-h-[90vh] object-contain select-none shadow-2xl"
              />
            </motion.div>

            {/* Prev Button */}
            {index > 0 && (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  setIndex(index - 1);
                }}
                className="absolute left-4 top-1/2 -translate-y-1/2 p-3 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors pointer-events-auto"
              >
                <ChevronRight className="rotate-180" size={40} />
              </button>
            )}

            {/* Next Button */}
            {index < images.length - 1 && (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  setIndex(index + 1);
                }}
                className="absolute right-4 top-1/2 -translate-y-1/2 p-3 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors pointer-events-auto"
              >
                <ChevronRight size={40} />
              </button>
            )}
            
            {/* Image Counter */}
            <div className="absolute top-4 left-4 px-3 py-1 bg-black/50 text-white rounded-full text-sm font-medium pointer-events-auto">
                {index + 1} / {images.length}
            </div>

            {/* Thumbnails Strip */}
            <div className="absolute bottom-4 left-0 right-0 flex justify-center gap-2 px-4 pointer-events-none">
              <div className="flex gap-2 p-2 bg-black/50 rounded-xl overflow-x-auto max-w-full pointer-events-auto">
                {images.map((img, i) => (
                  <button
                    key={i}
                    onClick={(e) => {
                      e.stopPropagation();
                      setIndex(i);
                    }}
                    className={`w-12 h-12 flex-shrink-0 rounded-lg overflow-hidden border-2 transition-all ${
                      i === index ? 'border-primary scale-110' : 'border-transparent opacity-50 hover:opacity-100'
                    }`}
                  >
                    <SharedImage 
                      src={img} 
                      alt={`Thumbnail ${i + 1}`} 
                      className="w-full h-full object-cover" 
                    />
                  </button>
                ))}
              </div>
            </div>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};

// ============ Main Gallery Component ============
interface ImageGalleryProps {
  images: string[];
  altPrefix?: string;
  selectedIndex: number;
  onSelectIndex: (index: number) => void;
  lightboxOpen: boolean;
  setLightboxOpen: (open: boolean) => void;
}

export const ImageGallery = memo(({ 
  images, 
  altPrefix = 'Gallery image', 
  selectedIndex, 
  onSelectIndex, 
  lightboxOpen, 
  setLightboxOpen 
}: ImageGalleryProps) => {
  return (
    <div className="space-y-4">
      {/* Main Image */}
      <div 
        className="aspect-[4/3] max-h-[500px] rounded-2xl overflow-hidden bg-secondary border border-tertiary cursor-zoom-in relative group"
        onClick={() => setLightboxOpen(true)}
      >
        <motion.div
            key={selectedIndex}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ duration: 0.3 }}
            className="w-full h-full"
        >
            <SharedImage
                src={images[selectedIndex]}
                alt={`${altPrefix} - Main`}
                className="w-full h-full object-contain bg-white transition-transform duration-300 group-hover:scale-105"
            />
        </motion.div>
        
        {/* Hover Hint */}
        <div className="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity bg-black/10 pointer-events-none">
            <span className="bg-black/60 text-white text-xs px-3 py-1.5 rounded-full backdrop-blur-sm">
                Click to expand
            </span>
        </div>
      </div>

      {/* Thumbnails */}
      {images.length > 1 && (
        <div className="flex gap-2 overflow-x-auto pb-2">
          {images.map((img, index) => (
            <button
              key={index}
              onClick={() => onSelectIndex(index)}
              className={`w-20 h-20 flex-shrink-0 rounded-lg overflow-hidden border-2 transition-all ${index === selectedIndex ? 'border-primary' : 'border-transparent opacity-70 hover:opacity-100'
                }`}
            >
              <SharedImage src={img} alt={`${altPrefix} - Thumbnail ${index + 1}`} className="w-full h-full object-cover" />
            </button>
          ))}
        </div>
      )}

      {/* Lightbox */}
      <ImageLightbox 
        images={images}
        initialIndex={selectedIndex}
        isOpen={lightboxOpen}
        onClose={() => setLightboxOpen(false)}
      />
    </div>
  );
});

ImageGallery.displayName = 'ImageGallery';
