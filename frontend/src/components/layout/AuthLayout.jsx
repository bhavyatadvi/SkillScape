import React from 'react';
import { Outlet, Link } from 'react-router-dom';

const SkillScapeLogo = () => (
  <Link to="/" className="flex items-center hover:opacity-90 transition-opacity">
    <img src="/logo_light.png" alt="SkillScape Logo" className="h-[72px] object-contain" />
  </Link>
);

export const AuthLayout = () => {
  return (
    <div className="flex min-h-screen w-full bg-transparent">
      {/* Left Panel — Branding */}
      <div className="hidden lg:flex lg:w-1/2 bg-[#0F172A] items-center justify-center p-12 relative overflow-hidden">
        {/* Subtle grid background */}
        <div className="absolute inset-0 opacity-[0.03]"
          style={{
            backgroundImage: `linear-gradient(rgba(255,255,255,0.1) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.1) 1px, transparent 1px)`,
            backgroundSize: '40px 40px'
          }}
        />
        
        <div className="max-w-md text-center relative z-10">
          <div className="flex justify-center mb-8">
            <SkillScapeLogo />
          </div>
          <p className="text-lg text-white/70 mb-10 leading-relaxed">
            Discover your perfect career path, master in-demand skills, and build the future you want.
          </p>
          <div className="grid grid-cols-3 gap-8 mt-8">
            <div className="text-center">
              <div className="text-3xl font-bold text-[#2563EB]">50+</div>
              <div className="text-sm text-white/50 mt-1">Career Paths</div>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-[#4F46E5]">500+</div>
              <div className="text-sm text-white/50 mt-1">Skills</div>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-[#7C3AED]">100+</div>
              <div className="text-sm text-white/50 mt-1">Career Tools</div>
            </div>
          </div>
        </div>
      </div>

      {/* Right Panel — Auth Forms */}
      <div className="flex w-full lg:w-1/2 items-center justify-center p-8 bg-transparent relative z-10">
        <div className="w-full max-w-md">
          {/* Mobile Logo */}
          <div className="flex lg:hidden justify-center mb-8">
            <div className="flex items-center gap-2.5">
              <div className="relative w-8 h-8">
                <svg viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg" className="w-8 h-8">
                  <path d="M8 24L16 14L24 8" stroke="#2563EB" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" />
                  <circle cx="8" cy="24" r="3.5" fill="#2563EB" />
                  <circle cx="16" cy="14" r="3.5" fill="#4F46E5" />
                  <circle cx="24" cy="8" r="3.5" fill="#0F172A" />
                </svg>
              </div>
              <span className="text-xl font-bold text-[#0F172A] tracking-tight">SkillScape</span>
            </div>
          </div>
          
          <Outlet />
        </div>
      </div>
    </div>
  );
};

export default AuthLayout;
