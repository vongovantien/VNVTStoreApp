import { useState, useEffect, memo, useMemo } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { ChevronRight, X, PlayCircle } from 'lucide-react';
import SharedImage from '@/components/common/Image';

// ============ Helper ============
const getMediaType = (url: string) => {
    if (!url) return 'image';
    if (url.includes('youtube.com') || url.includes('youtu.be')) return 'youtube';
    if (url.includes('vimeo.com')) return 'vimeo';
    if (url.match(/\.(mp4|webm|ogg)$/i)) return 'video';
    return 'image';
};

const getVideoId = (url: string) => {
    if (url.includes('youtube.com')) return url.split('v=')[1]?.split('&')[0];
    if (url.includes('youtu.be')) return url.split('/').pop();
    if (url.includes('vimeo.com')) return url.split('/').pop();
    return null;
};

// ============ Image Lightbox Component ============
import { createPortal } from 'react-dom';

interface ImageLightboxProps {
  images: string[];
  video?: string | undefined;
  initialIndex: number;
  isOpen: boolean;
  onClose: () => void;
}

const ImageLightbox = ({
  images,
  video,
  initialIndex,
  isOpen,
  onClose,
}: ImageLightboxProps) => {
  const [index, setIndex] = useState(initialIndex);
  const mediaItems = useMemo(() => video ? [...images, video] : images, [images, video]);

  useEffect(() => {
    if (isOpen) {
      queueMicrotask(() => setIndex(initialIndex));
    }
  }, [isOpen, initialIndex]);

  const renderContent = (url: string) => {
    const type = getMediaType(url);
    const videoId = getVideoId(url);

    if (type === 'youtube' && videoId) {
      return (
        <iframe
          src={`https://www.youtube.com/embed/${videoId}?autoplay=1`}
          className="w-full aspect-video rounded-lg shadow-2xl"
          allow="autoplay; encrypted-media"
          allowFullScreen
        />
      );
    }

    if (type === 'vimeo' && videoId) {
      return (
        <iframe
          src={`https://player.vimeo.com/video/${videoId}?autoplay=1`}
          className="w-full aspect-video rounded-lg shadow-2xl"
          allow="autoplay; fullscreen"
          allowFullScreen
        />
      );
    }

    if (type === 'video') {
      return (
        <video
          src={url}
          controls
          autoPlay
          className="max-w-full max-h-[85vh] rounded-lg shadow-2xl"
        />
      );
    }

    return (
      <SharedImage
        src={url}
        alt="Lightbox View"
        className="max-w-full max-h-[85vh] object-contain rounded-lg shadow-2xl"
      />
    );
  };

  if (typeof window === 'undefined') return null;

  return createPortal(
    <AnimatePresence>
      {isOpen && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/95 backdrop-blur-sm"
          onClick={onClose}
        >
          {/* Close Button */}
          <button
            onClick={onClose}
            className="absolute top-4 right-4 p-2 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors z-[10000]"
          >
            <X size={32} />
          </button>

          {/* Main Container */}
          <div className="relative w-full h-full flex items-center justify-center p-4 pointer-events-none">
            <motion.div
              key={index}
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ type: "spring", stiffness: 300, damping: 30 }}
              className="relative w-full max-w-5xl max-h-full flex items-center justify-center pointer-events-auto"
              onClick={(e) => e.stopPropagation()} 
            >
              {renderContent(mediaItems[index])}
            </motion.div>

            {/* Navigation Buttons */}
            {index > 0 && (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  setIndex(index - 1);
                }}
                className="absolute left-4 top-1/2 -translate-y-1/2 p-3 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors pointer-events-auto z-50"
              >
                <ChevronRight className="rotate-180" size={40} />
              </button>
            )}

            {index < mediaItems.length - 1 && (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  setIndex(index + 1);
                }}
                className="absolute right-4 top-1/2 -translate-y-1/2 p-3 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors pointer-events-auto z-50"
              >
                <ChevronRight size={40} />
              </button>
            )}
            
            {/* Counter */}
            <div className="absolute top-4 left-4 px-3 py-1 bg-black/50 text-white rounded-full text-sm font-medium pointer-events-auto z-50">
                {index + 1} / {mediaItems.length}
            </div>

            {/* Thumbnails Strip */}
            <div className="absolute bottom-4 left-0 right-0 flex justify-center gap-2 px-4 pointer-events-none z-50">
              <div className="flex gap-2 p-2 bg-black/50 rounded-xl overflow-x-auto max-w-full pointer-events-auto">
                {mediaItems.map((item, i) => (
                  <button
                    key={i}
                    onClick={(e) => {
                      e.stopPropagation();
                      setIndex(i);
                    }}
                    className={`relative w-12 h-12 flex-shrink-0 rounded-lg overflow-hidden border-2 transition-all ${
                      i === index ? 'border-primary scale-110' : 'border-transparent opacity-50 hover:opacity-100'
                    }`}
                  >
                    {getMediaType(item) !== 'image' ? (
                        <div className="w-full h-full bg-slate-800 flex items-center justify-center">
                            <PlayCircle size={20} className="text-white" />
                        </div>
                    ) : (
                        <SharedImage 
                          src={item} 
                          alt={`Thumbnail ${i + 1}`} 
                          className="w-full h-full object-cover" 
                        />
                    )}
                  </button>
                ))}
              </div>
            </div>
          </div>
        </motion.div>
      )}
    </AnimatePresence>,
    document.body
  );
};

// ============ Main Gallery Component ============
interface ImageGalleryProps {
  images: string[];
  video?: string;
  altPrefix?: string;
  selectedIndex: number;
  onSelectIndex: (index: number) => void;
  lightboxOpen: boolean;
  setLightboxOpen: (open: boolean) => void;
}

export const ImageGallery = memo(({ 
  images, 
  video,
  altPrefix = 'Gallery image', 
  selectedIndex, 
  onSelectIndex, 
  lightboxOpen, 
  setLightboxOpen 
}: ImageGalleryProps) => {
  const mediaItems = useMemo(() => video ? [...images, video] : images, [images, video]);
  const currentItem = mediaItems[selectedIndex] || mediaItems[0];
  const isCurrentVideo = getMediaType(currentItem) !== 'image';

  return (
    <div className="space-y-4">
      {/* Main Image/Video Preview */}
      <div 
        className="aspect-[4/3] max-h-[500px] rounded-2xl overflow-hidden bg-secondary border border-tertiary cursor-pointer relative group"
        onClick={() => setLightboxOpen(true)}
      >
        <motion.div
            key={selectedIndex}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ duration: 0.3 }}
            className="w-full h-full flex items-center justify-center bg-black/5"
        >
            {isCurrentVideo ? (
                 <div className="relative w-full h-full flex items-center justify-center bg-slate-900">
                    <PlayCircle size={64} className="text-white opacity-80 group-hover:opacity-100 transition-opacity" />
                    {getMediaType(currentItem) === 'video' && (
                        <video src={currentItem} className="w-full h-full object-contain opacity-50" />
                    )}
                    {/* For YouTube/Vimeo, we show a placeholder or just the play icon on black bg */}
                 </div>
            ) : (
                <SharedImage
                    src={currentItem}
                    alt={`${altPrefix} - Main`}
                    className="w-full h-full object-contain bg-white transition-transform duration-300 group-hover:scale-105"
                />
            )}
        </motion.div>
        
        {/* Hover Hint */}
        <div className="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none">
            {!isCurrentVideo && (
                <span className="bg-black/60 text-white text-xs px-3 py-1.5 rounded-full backdrop-blur-sm">
                    Click to expand
                </span>
            )}
        </div>
      </div>

      {/* Thumbnails */}
      {mediaItems.length > 1 && (
        <div className="flex gap-2 overflow-x-auto pb-2">
          {mediaItems.map((item, index) => {
            const isVideo = getMediaType(item) !== 'image';
            return (
                <button
                key={index}
                onClick={() => onSelectIndex(index)}
                className={`w-20 h-20 flex-shrink-0 rounded-lg overflow-hidden border-2 transition-all relative ${index === selectedIndex ? 'border-primary' : 'border-transparent opacity-70 hover:opacity-100'
                    }`}
                >
                {isVideo ? (
                    <div className="w-full h-full bg-slate-800 flex items-center justify-center">
                        <PlayCircle size={24} className="text-white" />
                    </div>
                ) : (
                    <SharedImage src={item} alt={`${altPrefix} - Thumbnail ${index + 1}`} className="w-full h-full object-cover" />
                )}
                </button>
            );
          })}
        </div>
      )}

      {/* Lightbox */}
      <ImageLightbox 
        images={images}
        video={video}
        initialIndex={selectedIndex}
        isOpen={lightboxOpen}
        onClose={() => setLightboxOpen(false)}
      />
    </div>
  );
});

ImageGallery.displayName = 'ImageGallery';
