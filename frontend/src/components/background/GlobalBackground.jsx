
import React, { useMemo } from 'react';
import { Compass } from 'lucide-react';
import { useLocation } from 'react-router-dom';
import './GlobalBackground.css';

export const GlobalBackground = () => {
  const location = useLocation();
  const isPublicPage = ['/', '/login', '/register', '/forgot-password', '/reset-password'].includes(location.pathname);

  // Memoize random particle positions to prevent jumping on React re-renders
  const particles = useMemo(() => {
    return Array.from({ length: 12 }).map((_, i) => ({
      left: `${Math.random() * 100}%`,
      animationDelay: `${Math.random() * 15}s`,
      opacity: Math.random() * 0.4 + 0.2, // slightly lighter/more subtle opacity
      scale: Math.random() * 0.5 + 0.8, // subtle size variation
    }));
  }, []);

  return (
    <div className="global-bg-container">
      {/* Layer 1: Base tint is handled by background-color in CSS */}

      {/* Layer 5: Faint diagonal rays (put behind so glow overlays it softly) */}
      <div className="global-bg-rays" />

      {/* Layer 2: Fine Grid */}
      <div className="global-bg-grid" />

      {/* Layer 3: Large Atmospheric Shapes */}
      <div className="global-bg-glow-1" />
      <div className="global-bg-glow-2" />
      <div className="global-bg-glow-3" />

      {/* Layer 4: Subtle Floating Particles */}
      {isPublicPage && (
        <div className="global-bg-particles">
        {particles.map((style, i) => (
          <div
            key={i}
            className="particle"
            style={{
              left: style.left,
              animationDelay: style.animationDelay,
              opacity: style.opacity,
              transform: `scale(${style.scale})`
            }}
          >
            {/* Light watermark, intentionally visible but never a focal element. */}
            <Compass className="w-full h-full text-blue-300 opacity-55" strokeWidth={1} />
          </div>
        ))}
        </div>
      )}
    </div>
  );
};

export default GlobalBackground;
