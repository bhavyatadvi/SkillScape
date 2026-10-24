import React, { useState } from 'react';
import { BookOpen, FlaskConical, Dna, Brain, Cpu, Layers, Trophy } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';

const PHASE_ICONS = [BookOpen, FlaskConical, Dna, Brain, Cpu, Layers];
const getPhaseIcon = (i) => PHASE_ICONS[i % PHASE_ICONS.length];

/* ───────── layout constants ───────── */
const H = 440;            // component height (px)
const GOAL_X = 90.5;      // goal position (% of width) – matches the big peak
const GOAL_Y = 215;       // goal dot position (px from top)
const CONNECTOR = 46;     // gap between card bottom and dot centre (px)

/* ───────── little pine tree used in the scenery ───────── */
const Tree = ({ x, y, s = 1, o = 0.55 }) => (
  <g transform={`translate(${x},${y}) scale(${s})`} opacity={o} fill="#A9C6F3">
    <polygon points="0,-34 -9,-14 9,-14" />
    <polygon points="0,-24 -12,0 12,0" />
    <rect x="-1.2" y="0" width="2.4" height="6" />
  </g>
);

/* ───────── scenery (mountains, sun, hills, trees) ───────── */
function Scenery() {
  return (
    <svg
      className="absolute inset-0 w-full h-full pointer-events-none"
      viewBox={`0 0 1000 ${H}`}
      preserveAspectRatio="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <defs>
        <linearGradient id="rv-far" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor="#D6E4FA" stopOpacity="0.85" />
          <stop offset="1" stopColor="#EEF4FE" stopOpacity="0.2" />
        </linearGradient>
        <linearGradient id="rv-mid" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor="#BCD3F6" stopOpacity="0.9" />
          <stop offset="1" stopColor="#E3EDFC" stopOpacity="0.35" />
        </linearGradient>
        <linearGradient id="rv-hill1" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor="#B4CDF5" />
          <stop offset="1" stopColor="#DCE8FB" />
        </linearGradient>
        <linearGradient id="rv-hill2" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor="#9DBDF2" />
          <stop offset="1" stopColor="#CFE0FA" />
        </linearGradient>
        <linearGradient id="rv-peak" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor="#8DB2F3" />
          <stop offset="1" stopColor="#D2E1FA" />
        </linearGradient>
        <linearGradient id="rv-peak-shade" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor="#6F9BEE" />
          <stop offset="1" stopColor="#B9D1F7" />
        </linearGradient>
        <radialGradient id="rv-sun">
          <stop offset="0" stopColor="#FFFFFF" stopOpacity="0.95" />
          <stop offset="1" stopColor="#FFFFFF" stopOpacity="0.35" />
        </radialGradient>
      </defs>

      {/* soft sun */}
      <circle cx="755" cy="145" r="30" fill="url(#rv-sun)" />

      {/* far mountains */}
      <path
        d="M-20 330 L110 240 L230 320 L370 228 L520 325 L640 255 L770 330 L1020 290 L1020 460 L-20 460Z"
        fill="url(#rv-far)" stroke="url(#rv-far)" strokeWidth="8" strokeLinejoin="round"
      />

      {/* mid mountains */}
      <path
        d="M-20 385 L140 292 L290 372 L455 296 L620 376 L780 318 L1020 385 L1020 460 L-20 460Z"
        fill="url(#rv-mid)" stroke="url(#rv-mid)" strokeWidth="8" strokeLinejoin="round"
      />

      {/* GOAL PEAK (apex sits right under the goal dot) */}
      <g strokeLinejoin="round">
        <path
          d="M690 460 L775 355 L845 305 L905 205 L965 300 L1030 335 L1030 460Z"
          fill="url(#rv-peak)" stroke="url(#rv-peak)" strokeWidth="4"
        />
        {/* shaded face */}
        <path
          d="M905 205 L965 300 L1030 335 L1030 460 L915 460 L925 380 L905 320Z"
          fill="url(#rv-peak-shade)" opacity="0.75"
        />
        {/* ridge highlights */}
        <path d="M905 205 L880 300 L860 360 L845 460" fill="none" stroke="#fff" strokeOpacity="0.35" strokeWidth="2" />
        <path d="M905 205 L925 290 L940 380" fill="none" stroke="#5B8DEF" strokeOpacity="0.25" strokeWidth="2" />
        {/* snow cap */}
        <path d="M905 205 L926 246 L914 240 L905 256 L894 240 L884 247Z" fill="#fff" opacity="0.92" />
      </g>

      {/* foreground hills */}
      <path
        d="M-20 405 Q120 350 270 392 T560 388 T800 400 L800 460 L-20 460Z"
        fill="url(#rv-hill1)" opacity="0.95"
      />
      <path
        d="M-20 430 Q160 392 330 425 T640 420 T1020 412 L1020 460 L-20 460Z"
        fill="url(#rv-hill2)" opacity="0.9"
      />

      {/* pine trees */}
      <Tree x={22}  y={412} s={1.15} />
      <Tree x={44}  y={420} s={0.85} />
      <Tree x={100} y={432} s={0.6}  />
      <Tree x={205} y={434} s={0.7}  />
      <Tree x={218} y={436} s={0.55} />
      <Tree x={478} y={436} s={0.85} />
      <Tree x={489} y={430} s={0.6}  />
      <Tree x={585} y={392} s={0.6}  o={0.45} />
      <Tree x={612} y={400} s={0.8}  o={0.5} />
      <Tree x={775} y={392} s={0.8}  />
      <Tree x={790} y={398} s={0.55} />
      <Tree x={925} y={412} s={0.75} />
      <Tree x={955} y={400} s={0.95} />
      <Tree x={976} y={414} s={1.15} />
    </svg>
  );
}

export default function RoadmapVisualHeader({ title, description, phases, onViewOverview }) {
  const [hoveredIndex, setHoveredIndex] = useState(null);

  const handleScrollToPhase = (index) => {
    const el = document.getElementById(`phase-section-${index}`);
    if (el) {
      const y = el.getBoundingClientRect().top + window.pageYOffset - 120;
      window.scrollTo({ top: y, behavior: 'smooth' });
    }
  };

  if (!phases || phases.length === 0) return null;

  const N = phases.length;
  const CARD_W = N > 6 ? 190 : 230;

  /* ── positions ── */
  const dotX = (i) => (N === 1 ? 10 : 7 + (i * 64) / (N - 1));            // % from left
  const dotY = (i) =>
    N === 1 ? 285 : 285 - (i / (N - 1)) * 30 + Math.sin(i * 2) * 10;        // px from top

  /* ── smooth dashed path (Catmull-Rom → Bezier) ──
     x is in % (viewBox width 100), y is in px (viewBox height H) */
  const pts = [
    ...phases.map((_, i) => ({ x: dotX(i), y: dotY(i) })),
    { x: GOAL_X, y: GOAL_Y },
  ];

  function smoothD(p) {
    if (p.length === 1) return `M ${p[0].x},${p[0].y}`;
    let d = `M ${p[0].x},${p[0].y}`;
    for (let i = 0; i < p.length - 1; i++) {
      const p0 = p[Math.max(0, i - 1)];
      const p1 = p[i];
      const p2 = p[i + 1];
      const p3 = p[Math.min(p.length - 1, i + 2)];
      const cp1x = p1.x + (p2.x - p0.x) / 6;
      const cp1y = p1.y + (p2.y - p0.y) / 6;
      const cp2x = p2.x - (p3.x - p1.x) / 6;
      const cp2y = p2.y - (p3.y - p1.y) / 6;
      d += ` C ${cp1x},${cp1y} ${cp2x},${cp2y} ${p2.x},${p2.y}`;
    }
    return d;
  }
  const pathD = smoothD(pts);

  return (
    <div
      className="relative rounded-3xl border border-blue-100 shadow-sm mb-8"
      style={{
        height: H,
        background: 'linear-gradient(180deg, #FFFFFF 0%, #F3F8FF 45%, #E6F0FE 100%)',
        // no overflow:hidden here – hover tooltips must not be clipped
      }}
    >
      {/* clipped scenery layer (keeps rounded corners) */}
      <div className="absolute inset-0 rounded-3xl overflow-hidden pointer-events-none">
        <Scenery />
        {/* dashed path */}
        <svg
          className="absolute inset-0 w-full h-full"
          viewBox={`0 0 100 ${H}`}
          preserveAspectRatio="none"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path d={pathD} fill="none" stroke="#FFFFFF" strokeOpacity="0.7" strokeWidth="7"
                strokeLinecap="round" vectorEffect="non-scaling-stroke" />
          <path d={pathD} fill="none" stroke="#3B82F6" strokeWidth="2.2"
                strokeDasharray="7 7" strokeLinecap="round" vectorEffect="non-scaling-stroke" />
        </svg>
      </div>

      {/* ══ Header – top left ══ */}
      <div className="absolute top-6 left-7 z-20 flex items-center gap-4">
        <div className="w-14 h-14 rounded-2xl bg-blue-50 border border-blue-100 flex items-center justify-center shadow-sm shrink-0">
          <BookOpen className="text-blue-600 w-6 h-6" />
        </div>
        <div>
          <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight leading-tight">
            {title || 'Curriculum Roadmap'}
          </h1>
          <p className="text-slate-500 text-sm mt-0.5 max-w-xl leading-relaxed">
            {description ||
              'A structured learning path to build your conceptual clarity, practical knowledge and career readiness.'}
          </p>
        </div>
      </div>

      {/* ══ View Overview – top right ══ */}
      <button
        type="button"
        onClick={onViewOverview || (() => handleScrollToPhase(0))}
        className="absolute top-6 right-7 z-[100] flex items-center gap-2 bg-white/90 hover:bg-white border border-blue-100 rounded-xl px-4 py-2.5 shadow-sm text-sm font-semibold text-blue-600 transition-colors"
      >
        <BookOpen className="w-4 h-4" />
        View Overview
      </button>

      {/* ══ Goal card + flag ══ */}
      <div
        style={{
          position: 'absolute',
          left: `${GOAL_X}%`,
          top: 104,
          transform: 'translateX(-50%)',
          zIndex: 30,
        }}
      >
        <motion.div
          initial={{ opacity: 0, y: -6 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
          className="flex items-center gap-3 bg-white border border-blue-100 rounded-2xl pl-3 pr-5 py-2.5 shadow-[0_8px_24px_rgba(59,130,246,0.15)]"
        >
          <div className="w-10 h-10 rounded-xl bg-blue-50 flex items-center justify-center">
            <Trophy className="w-5 h-5 text-blue-600" />
          </div>
          <div>
            <div className="text-[11px] font-extrabold text-blue-500 tracking-wider leading-none mb-1">GOAL</div>
            <div className="text-[13px] font-bold text-slate-900 whitespace-nowrap leading-none">Career Readiness</div>
          </div>
        </motion.div>
        {/* pointer arrow */}
        <div
          style={{
            width: 0, height: 0, margin: '0 auto',
            borderLeft: '5px solid transparent',
            borderRight: '5px solid transparent',
            borderTop: '6px solid #93C5FD',
          }}
        />
      </div>

      {/* flag on the summit */}
      <svg
        width="44" height="52" viewBox="0 0 44 52"
        style={{
          position: 'absolute',
          left: `${GOAL_X}%`,
          top: GOAL_Y - 52,
          transform: 'translateX(-2px)',
          zIndex: 25,
          pointerEvents: 'none',
        }}
      >
        <line x1="2" y1="4" x2="2" y2="52" stroke="#1E3A8A" strokeWidth="2.5" strokeLinecap="round" />
        <path d="M2 4 L38 12 L2 22Z" fill="#3B82F6" />
      </svg>

      {/* goal dot */}
      <div
        style={{
          position: 'absolute',
          left: `${GOAL_X}%`,
          top: GOAL_Y,
          transform: 'translate(-50%, -50%)',
          zIndex: 26,
        }}
      >
        <div className="w-5 h-5 rounded-full bg-white border-[2.5px] border-blue-500 shadow-md flex items-center justify-center">
          <div className="w-2 h-2 rounded-full bg-blue-500" />
        </div>
      </div>

      {/* ══ Phase cards + connectors + dots ══ */}
      {phases.map((phase, index) => {
        const Icon = getPhaseIcon(index);
        const isHovered = hoveredIndex === index;
        const x = dotX(index);
        const y = dotY(index);
        const isFirst = index === 0;
        const isBelow = index % 2 === 1;        // zig-zag: even = above the path, odd = below
        const dotR = isFirst ? 14 : 9;          // dot radius (px)

        return (
          <React.Fragment key={index}>
            {/* Card – bottom edge sits CONNECTOR px above the dot; clamped so it never leaves the left edge */}
            <div
              style={{
                position: 'absolute',
                left: `max(16px, calc(${x}% - ${CARD_W / 2}px))`,
                ...(isBelow
                  ? { top: y + CONNECTOR }
                  : { bottom: H - y + CONNECTOR }),
                width: CARD_W,
                zIndex: isHovered ? 60 : 20,
              }}
              onMouseEnter={() => setHoveredIndex(index)}
              onMouseLeave={() => setHoveredIndex(null)}
              onClick={() => handleScrollToPhase(index)}
            >
              <motion.div
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.08 + index * 0.08 }}
                className={`cursor-pointer bg-white border rounded-2xl px-3.5 py-3 transition-all
                  ${isHovered
                    ? 'border-blue-300 shadow-[0_12px_28px_rgba(59,130,246,0.22)]'
                    : 'border-blue-100 shadow-[0_8px_22px_rgba(59,130,246,0.12)]'}`}
              >
                <div className="flex items-center gap-3">
                  <div
                    className={`w-10 h-10 rounded-xl flex items-center justify-center shrink-0 transition-colors
                    ${isHovered ? 'bg-blue-100 text-blue-700' : 'bg-blue-50 text-blue-600'}`}
                  >
                    <Icon className="w-[18px] h-[18px]" />
                  </div>
                  <div className="min-w-0">
                    <div className="text-[10px] font-extrabold text-blue-300 uppercase tracking-widest leading-none mb-1">
                      Phase {index + 1}
                    </div>
                    <div className="text-[13px] font-bold text-slate-900 leading-snug line-clamp-2">
                      {phase.title}
                    </div>
                  </div>
                </div>

                {/* Hover tooltip */}
                <AnimatePresence>
                  {isHovered && phase.modules && phase.modules.length > 0 && (
                    <motion.div
                      initial={{ opacity: 0, y: 4 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, y: 2 }}
                      transition={{ duration: 0.14 }}
                      className="absolute top-full left-0 mt-2 bg-white border border-blue-200 rounded-2xl shadow-2xl p-4"
                      style={{ width: 240, zIndex: 9999 }}
                      onMouseEnter={() => setHoveredIndex(index)}
                      onMouseLeave={() => setHoveredIndex(null)}
                    >
                      <p className="text-xs font-black text-slate-900 mb-2 leading-snug">{phase.title}</p>
                      <ul className="space-y-1.5">
                        {phase.modules.slice(0, 5).map((mod, mIdx) => (
                          <li key={mIdx} className="flex items-start gap-1.5 text-[11px] text-slate-600">
                            <span className="mt-0.5 w-4 h-4 rounded-full bg-blue-50 text-blue-600 font-black flex items-center justify-center shrink-0 text-[9px]">
                              {mIdx + 1}
                            </span>
                            <span className="leading-tight">{mod.title}</span>
                          </li>
                        ))}
                        {phase.modules.length > 5 && (
                          <li className="text-[10px] text-slate-400 pl-5">
                            +{phase.modules.length - 5} more modules
                          </li>
                        )}
                      </ul>
                    </motion.div>
                  )}
                </AnimatePresence>
              </motion.div>
            </div>

            {/* Dashed connector between dot and card (arrow points at the card) */}
            <div
              style={{
                position: 'absolute',
                left: `${x}%`,
                top: isBelow ? y + dotR : y - CONNECTOR,
                transform: 'translateX(-50%)',
                zIndex: 15,
                display: 'flex',
                flexDirection: isBelow ? 'column-reverse' : 'column',
                alignItems: 'center',
                pointerEvents: 'none',
              }}
            >
              <div
                style={{
                  width: 0, height: 0,
                  borderLeft: '4.5px solid transparent',
                  borderRight: '4.5px solid transparent',
                  ...(isBelow
                    ? { borderBottom: '6px solid #93C5FD' }
                    : { borderTop: '6px solid #93C5FD' }),
                }}
              />
              <div
                style={{
                  height: CONNECTOR - 6 - dotR,
                  borderLeft: '1.5px dashed #93C5FD',
                }}
              />
            </div>

            {/* Dot on path */}
            <motion.div
              initial={{ opacity: 0, scale: 0 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ delay: 0.2 + index * 0.08 }}
              style={{
                position: 'absolute',
                left: `${x}%`,
                top: y,
                transform: 'translate(-50%, -50%)',
                zIndex: 20,
                cursor: 'pointer',
              }}
              onClick={() => handleScrollToPhase(index)}
            >
              {isFirst ? (
                <div className="w-7 h-7 rounded-full bg-white flex items-center justify-center shadow-[0_0_0_5px_rgba(59,130,246,0.18)]">
                  <div className="w-[18px] h-[18px] rounded-full bg-blue-500" />
                </div>
              ) : (
                <div className="w-[18px] h-[18px] rounded-full bg-white flex items-center justify-center shadow-md">
                  <div className="w-2.5 h-2.5 rounded-full bg-blue-500" />
                </div>
              )}
            </motion.div>
          </React.Fragment>
        );
      })}
    </div>
  );
}