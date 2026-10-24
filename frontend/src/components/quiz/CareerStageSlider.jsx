import React, { useState, useEffect, useRef, useCallback } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { ChevronUp, ChevronDown, ArrowRight } from 'lucide-react';
import { Button } from '@/components/ui/button';

export const CareerStageSlider = ({ stages, onSelect }) => {
  const [activeIndex, setActiveIndex] = useState(0);
  const containerRef = useRef(null);
  const isAnimating = useRef(false);

  const totalStages = stages.length;

  const goToPrev = useCallback(() => {
    if (activeIndex > 0 && !isAnimating.current) {
      setActiveIndex(prev => prev - 1);
      triggerAnimationLock();
    }
  }, [activeIndex]);

  const goToNext = useCallback(() => {
    if (activeIndex < totalStages - 1 && !isAnimating.current) {
      setActiveIndex(prev => prev + 1);
      triggerAnimationLock();
    }
  }, [activeIndex, totalStages]);

  const triggerAnimationLock = () => {
    isAnimating.current = true;
    setTimeout(() => {
      isAnimating.current = false;
    }, 400); // 400ms debounce matches roughly the animation duration
  };

  // Handle Keyboard
  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.key === 'ArrowUp') {
        e.preventDefault();
        goToPrev();
      } else if (e.key === 'ArrowDown') {
        e.preventDefault();
        goToNext();
      }
    };
    
    // Only listen to keyboard if user is interacting with or hovering over the component
    const container = containerRef.current;
    if (!container) return;
    
    const onMouseEnter = () => window.addEventListener('keydown', handleKeyDown, { passive: false });
    const onMouseLeave = () => window.removeEventListener('keydown', handleKeyDown);
    
    container.addEventListener('mouseenter', onMouseEnter);
    container.addEventListener('mouseleave', onMouseLeave);
    
    return () => {
      container.removeEventListener('mouseenter', onMouseEnter);
      container.removeEventListener('mouseleave', onMouseLeave);
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [goToPrev, goToNext]);

  // Handle Wheel Events with non-passive listener to prevent default page scrolling
  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    const handleWheel = (e) => {
      // If we can scroll further in the requested direction, prevent default page scroll
      if (e.deltaY > 0) {
        if (activeIndex < totalStages - 1) {
          e.preventDefault();
          goToNext();
        }
      } else if (e.deltaY < 0) {
        if (activeIndex > 0) {
          e.preventDefault();
          goToPrev();
        }
      }
    };

    container.addEventListener('wheel', handleWheel, { passive: false });
    return () => {
      container.removeEventListener('wheel', handleWheel);
    };
  }, [activeIndex, totalStages, goToNext, goToPrev]);

  // Calculate card positioning based on relative index
  const getCardStyles = (index) => {
    const diff = index - activeIndex;
    
    // Active card
    if (diff === 0) {
      return {
        y: 0,
        scale: 1,
        opacity: 1,
        zIndex: 10,
        pointerEvents: 'auto',
      };
    }
    
    // Previous card
    if (diff === -1) {
      return {
        y: -95, // Reduced for tighter fit
        scale: 0.85,
        opacity: 0.25,
        zIndex: 5,
        pointerEvents: 'none',
      };
    }
    
    // Next card
    if (diff === 1) {
      return {
        y: 95, // Reduced for tighter fit
        scale: 0.85,
        opacity: 0.25,
        zIndex: 5,
        pointerEvents: 'none',
      };
    }
    
    // Hidden cards
    return {
      y: diff > 0 ? 160 : -160,
      scale: 0.8,
      opacity: 0,
      zIndex: 1,
      pointerEvents: 'none',
    };
  };

  return (
    <div className="relative flex w-full max-w-3xl mx-auto items-center" ref={containerRef}>
      
      {/* Vertical Progress Indicator */}
      <div className="flex flex-col items-center justify-center mr-6 md:mr-10 shrink-0 py-8 relative">
        <div className="absolute top-10 bottom-10 w-[2px] bg-slate-200 z-0" />
        
        {stages.map((stage, idx) => (
          <div key={stage.id} className="relative z-10 flex flex-col items-center group py-3" onClick={() => setActiveIndex(idx)}>
            <div 
              className={`w-3 h-3 rounded-full cursor-pointer transition-all duration-300 ${
                idx === activeIndex 
                  ? 'bg-blue-600 scale-125 ring-4 ring-blue-600/20' 
                  : 'bg-slate-300 hover:bg-slate-400'
              }`}
            />
          </div>
        ))}
      </div>

      {/* Slider Area */}
      <div className="relative w-full h-[280px] md:h-[320px] flex items-center justify-center">
        
        {/* Navigation Buttons (Top/Bottom of slider) */}
        <div className="absolute right-0 top-0 bottom-0 flex flex-col justify-between py-6 z-20 hidden md:flex pointer-events-none">
          <Button 
            variant="outline" 
            size="icon" 
            onClick={goToPrev}
            disabled={activeIndex === 0}
            className="rounded-full w-10 h-10 bg-white/90 backdrop-blur pointer-events-auto border-slate-200 shadow-sm hover:border-blue-600 hover:text-blue-600 transition-all disabled:opacity-0 disabled:hover:border-slate-200"
            aria-label="Previous career stage"
          >
            <ChevronUp className="w-5 h-5" />
          </Button>
          
          <Button 
            variant="outline" 
            size="icon" 
            onClick={goToNext}
            disabled={activeIndex === totalStages - 1}
            className="rounded-full w-10 h-10 bg-white/90 backdrop-blur pointer-events-auto border-slate-200 shadow-sm hover:border-blue-600 hover:text-blue-600 transition-all disabled:opacity-0 disabled:hover:border-slate-200"
            aria-label="Next career stage"
          >
            <ChevronDown className="w-5 h-5" />
          </Button>
        </div>

        {/* Cards container */}
        <div className="relative w-full h-full flex items-center justify-center">
          <AnimatePresence initial={false}>
            {stages.map((stage, idx) => {
              const { id, meta } = stage;
              const isCurrent = idx === activeIndex;
              const styles = getCardStyles(idx);

              return (
                <motion.div
                  key={id}
                  initial={styles}
                  animate={styles}
                  transition={{
                    type: "spring",
                    stiffness: 300,
                    damping: 30,
                    mass: 1
                  }}
                  className="absolute w-full max-w-[560px]"
                >
                  <div 
                    onClick={() => isCurrent && onSelect(id)}
                    className={`
                      relative w-full p-6 md:p-8 rounded-2xl bg-white transition-all duration-300
                      ${isCurrent 
                        ? 'cursor-pointer shadow-[0_8px_30px_rgb(0,0,0,0.08)] border border-blue-600/20 ring-1 ring-blue-600/10 hover:shadow-[0_8px_30px_rgb(37,99,235,0.15)] hover:border-blue-600/40' 
                        : 'shadow-sm border border-slate-100 blur-[1px] cursor-pointer'
                      }
                    `}
                  >
                    {/* Visual glowing border effect for active state */}
                    {isCurrent && (
                      <div className="absolute inset-0 rounded-2xl ring-1 ring-inset ring-blue-600/10 pointer-events-none" />
                    )}

                    <div className="flex items-start gap-5">
                      <div className={`p-4 rounded-xl flex items-center justify-center transition-colors ${
                        isCurrent 
                          ? 'bg-blue-50 text-blue-600' 
                          : 'bg-slate-50 text-slate-400'
                      }`}>
                        {meta.icon}
                      </div>
                      
                      <div className="flex-1 min-w-0 pr-4">
                        <div className="flex items-center gap-2 mb-1.5">
                          <span className="text-[11px] font-bold text-blue-600/70 tracking-wider uppercase">Stage {String(idx + 1).padStart(2, '0')}</span>
                        </div>
                        <h3 className={`font-bold text-xl md:text-2xl tracking-tight transition-colors ${
                          isCurrent ? 'text-slate-900' : 'text-slate-600'
                        }`}>
                          {meta.title}
                        </h3>
                        <p className={`mt-2 text-sm md:text-base leading-relaxed transition-colors line-clamp-2 ${
                          isCurrent ? 'text-slate-500' : 'text-slate-400'
                        }`}>
                          {meta.desc}
                        </p>
                      </div>

                      {/* Right side arrow */}
                      <div className={`flex items-center self-center transition-all duration-300 ${
                        isCurrent ? 'opacity-100 translate-x-0 text-blue-600' : 'opacity-0 -translate-x-4'
                      }`}>
                        <div className="w-10 h-10 rounded-full bg-blue-50 flex items-center justify-center transition-colors">
                           <ArrowRight className="w-5 h-5" />
                        </div>
                      </div>
                    </div>

                  </div>
                </motion.div>
              );
            })}
          </AnimatePresence>
        </div>
      </div>
    </div>
  );
};
