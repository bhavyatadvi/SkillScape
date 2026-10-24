/**
 * quizScoring.js
 * ─────────────────────────────────────────────────────────────────────────────
 * Two jobs:
 *  1. calculateLocalWeights(stage, accumulatedDims)
 *     Converts raw per-question weight-key sums into career-path scores that
 *     the backend can match against its CareerPath.Tags column.
 *
 *  2. selectNextAdaptiveQuestion(pool, answeredIds, currentDimensions)
 *     Picks the next question that best diversifies the dimension coverage.
 *
 * KEY RULE: every key used in getD() here MUST match the keys used in the
 * `weights` objects inside the question files (after10.js, after12.js, …).
 * ─────────────────────────────────────────────────────────────────────────────
 */

const getMappedWeights = (stage, accumulatedDims) => {
  const getD = (key) => accumulatedDims[key] || 0;

  switch (stage) {

    // ─── AFTER 10TH ──────────────────────────────────────────────────────────
    // after10.js weight keys: mathematics, physics, biology, healthcare,
    //   accounting, financial, economics, business, humanities, reading,
    //   social, language, historical, scientific, logical, analytical,
    //   programming, computational, mechanical, iti, technical
    case 'after10':
      return {
        // Matches DB tags on 10th-sci-pcm / 10th-sci-pcb / 10th-sci-pcmb
        "Science (PCM)":
          (getD('mathematics') * 4) +
          (getD('physics') * 4) +
          (getD('logical') * 2) +
          (getD('analytical') * 2) +
          (getD('programming') * 1) +
          (getD('computational') * 1),

        "Science (PCB)":
          (getD('biology') * 4) +
          (getD('healthcare') * 4) +
          (getD('scientific') * 2),

        "Science (PCMB)":
          (getD('mathematics') * 2) +
          (getD('biology') * 2) +
          (getD('physics') * 2) +
          (getD('scientific') * 2),

        // Matches DB tags on 10th-com-math / 10th-com-nomath
        "Commerce with Mathematics":
          (getD('accounting') * 4) +
          (getD('financial') * 3) +
          (getD('economics') * 3) +
          (getD('business') * 3) +
          (getD('mathematics') * 2),

        "Commerce without Mathematics":
          (getD('accounting') * 4) +
          (getD('financial') * 3) +
          (getD('economics') * 3) +
          (getD('business') * 4),

        // Matches DB tags on 10th-art-* paths
        "Arts & Humanities":
          (getD('humanities') * 4) +
          (getD('reading') * 3) +
          (getD('social') * 3) +
          (getD('language') * 3) +
          (getD('historical') * 3),

        // Matches DB tags on 10th-dip-* paths
        "Diploma Studies":
          (getD('mechanical') * 3) +
          (getD('technical') * 3) +
          (getD('programming') * 2) +
          (getD('iti') * 2),

        // Matches DB tags on 10th-iti-* paths
        "ITI Trades":
          (getD('iti') * 5) +
          (getD('technical') * 3) +
          (getD('mechanical') * 2),
      };

    // ─── AFTER 12TH (STREAM SELECTOR) ───────────────────────────────────────
    // after12.js weight keys: pcm, pcb, commerce, arts, iti
    case 'after12':
    case 'after12.pcm':
    case 'after12.pcb':
    case 'after12.pcmb':
    case 'after12.commerce':
    case 'after12.arts':
    case 'after12.vocational':
    case 'after12.other':
      return {
        // DB paths at level 2 for After12th, with their tags
        "Engineering (B.Tech/B.E)":
          (getD('pcm') * 5) + (getD('pcmb') * 5) +
          (getD('programming') * 2) +
          (getD('logical') * 1),

        "Medical Sciences (MBBS/BDS)":
          (getD('pcb') * 5) + (getD('pcmb') * 5) +
          (getD('healthcare') * 2) +
          (getD('biology') * 1),

        "Core Research Sciences":
          (getD('pcm') * 4) + (getD('pcmb') * 4) +
          (getD('scientific') * 2),

        "Biological Research":
          (getD('pcb') * 4) + (getD('pcmb') * 4) +
          (getD('scientific') * 2),

        "Commerce & Finance (B.Com / BBA / CA)":
          (getD('commerce') * 5) +
          (getD('accounting') * 2) +
          (getD('financial') * 1),

        "Arts & Humanities (BA / LLB / Journalism)":
          (getD('arts') * 5) +
          (getD('humanities') * 2) +
          (getD('reading') * 1),

        "Polytechnic Diploma":
          (getD('iti') * 3) +
          (getD('pcm') * 2) +
          (getD('mechanical') * 2),

        "ITI / Vocational":
          (getD('iti') * 5) +
          (getD('mechanical') * 2),
      };

    // ─── AFTER GRADUATION ────────────────────────────────────────────────────
    // after_graduation.js weight keys:
    //   mba, mtech, research, law, upsc, finance, healthcare, creative,
    //   entrepreneurship, social
    case 'afterGraduation':
      return {
        "MBA / Management":
          (getD('mba') * 5) +
          (getD('entrepreneurship') * 2),

        "M.Tech / MS (Technical)":
          (getD('mtech') * 5) +
          (getD('research') * 2),

        "PhD / Research / Academia":
          (getD('research') * 5) +
          (getD('mtech') * 2),

        "Law (LLB / LLM)":
          (getD('law') * 5) +
          (getD('upsc') * 1),

        "UPSC / Civil Services":
          (getD('upsc') * 5) +
          (getD('law') * 1) +
          (getD('social') * 1),

        "Finance (CFA / CA / FRM)":
          (getD('finance') * 5) +
          (getD('mba') * 2),

        "Entrepreneurship / Startup":
          (getD('entrepreneurship') * 5) +
          (getD('mba') * 2),

        "Creative / Media / Design":
          (getD('creative') * 5) +
          (getD('social') * 2),

        "Social Impact / NGO / Development":
          (getD('social') * 5) +
          (getD('upsc') * 1),
      };

    // ─── AFTER POST-GRADUATION ────────────────────────────────────────────────
    // after_pg.js weight keys:
    //   phd, senior_role, consulting, teaching, entrepreneurship, policy, freelance
    case 'afterPostGraduation':
      return {
        "PhD / Academic Research":
          (getD('phd') * 5) +
          (getD('teaching') * 2),

        "Senior Corporate Role":
          (getD('senior_role') * 5) +
          (getD('consulting') * 2),

        "Management Consulting":
          (getD('consulting') * 5) +
          (getD('senior_role') * 2) +
          (getD('freelance') * 1),

        "Teaching / Training / EdTech":
          (getD('teaching') * 5) +
          (getD('phd') * 2),

        "Entrepreneurship / Own Venture":
          (getD('entrepreneurship') * 5),

        "Public Policy / Think Tank":
          (getD('policy') * 5) +
          (getD('teaching') * 1),

        "Independent Consulting / Freelance":
          (getD('freelance') * 5) +
          (getD('consulting') * 2),
      };

    // ─── CAREER SWITCH ────────────────────────────────────────────────────────
    // career_switch.js weight keys:
    //   tech, finance, consulting, marketing, hr, operations,
    //   creative, entrepreneurship, healthcare, social
    case 'careerSwitch':
      return {
        "IT / Data Science / Product":
          (getD('tech') * 5) +
          (getD('consulting') * 1),

        "Banking / Finance / Investment":
          (getD('finance') * 5) +
          (getD('consulting') * 1),

        "Strategy / Management Consulting":
          (getD('consulting') * 5) +
          (getD('finance') * 2) +
          (getD('tech') * 1),

        "Marketing / Brand / Digital":
          (getD('marketing') * 5) +
          (getD('creative') * 2),

        "HR / L&D / People Culture":
          (getD('hr') * 5) +
          (getD('social') * 2),

        "Operations / Supply Chain":
          (getD('operations') * 5),

        "Design / Content / Creative":
          (getD('creative') * 5) +
          (getD('marketing') * 2),

        "Startup / Own Business":
          (getD('entrepreneurship') * 5) +
          (getD('tech') * 1),

        "Healthcare / MedTech / Wellness":
          (getD('healthcare') * 5),

        "NGO / CSR / Social Impact":
          (getD('social') * 5) +
          (getD('hr') * 1),
      };

    // ─── UPSKILLING ───────────────────────────────────────────────────────────
    // upskilling.js weight keys:
    //   data_tech, software, business, finance_cert, marketing,
    //   design, communication, hr_people
    case 'upskilling':
      return {
        "Data Science / AI / ML":
          (getD('data_tech') * 5) +
          (getD('software') * 2),

        "Software Development / DevOps":
          (getD('software') * 5) +
          (getD('data_tech') * 1),

        "Product / Project Management":
          (getD('business') * 5) +
          (getD('communication') * 1),

        "Finance Certifications (CFA / FRM)":
          (getD('finance_cert') * 5),

        "Digital Marketing / Growth":
          (getD('marketing') * 5) +
          (getD('design') * 1),

        "UI/UX Design / Creative":
          (getD('design') * 5) +
          (getD('marketing') * 1),

        "Communication / Leadership":
          (getD('communication') * 5) +
          (getD('hr_people') * 2),

        "HR Tech / L&D / Coaching":
          (getD('hr_people') * 5) +
          (getD('communication') * 2),
      };

    // ─── ENTREPRENEURSHIP ─────────────────────────────────────────────────────
    // entrepreneurship.js weight keys:
    //   tech_startup, d2c, service_biz, social_enterprise,
    //   franchise, content_creator, fintech, edtech
    case 'entrepreneurship':
      return {
        "SaaS / Tech Startup":
          (getD('tech_startup') * 5) +
          (getD('fintech') * 2),

        "D2C / E-Commerce Brand":
          (getD('d2c') * 5) +
          (getD('content_creator') * 1),

        "Agency / Service Business":
          (getD('service_biz') * 5) +
          (getD('content_creator') * 1),

        "Social Enterprise / NGO":
          (getD('social_enterprise') * 5) +
          (getD('edtech') * 1),

        "Franchise / Traditional Business":
          (getD('franchise') * 5) +
          (getD('d2c') * 1),

        "Content Creator / YouTuber":
          (getD('content_creator') * 5) +
          (getD('d2c') * 1),

        "Fintech Startup":
          (getD('fintech') * 5) +
          (getD('tech_startup') * 2),

        "EdTech / Coaching Platform":
          (getD('edtech') * 5) +
          (getD('service_biz') * 1),
      };

    // ─── GOVERNMENT JOBS ──────────────────────────────────────────────────────
    // government_jobs.js weight keys:
    //   ias_ips, banking, ssc, defence, railways, psu, teaching, police
    case 'governmentJobs':
      return {
        "UPSC Civil Services (IAS/IPS/IFS)":
          (getD('ias_ips') * 5),

        "Banking (SBI PO / RBI / NABARD)":
          (getD('banking') * 5) +
          (getD('ssc') * 1),

        "SSC (CGL / CHSL / MTS)":
          (getD('ssc') * 5) +
          (getD('railways') * 1),

        "Defence (NDA / CDS / Air Force)":
          (getD('defence') * 5) +
          (getD('police') * 1),

        "Indian Railways (RRB)":
          (getD('railways') * 5) +
          (getD('ssc') * 1),

        "PSU via GATE (ONGC / NTPC / BHEL)":
          (getD('psu') * 5),

        "Teaching (TET / UGC-NET / KVS)":
          (getD('teaching') * 5),

        "Police / Paramilitary (CRPF / BSF)":
          (getD('police') * 5) +
          (getD('defence') * 2),
      };

    default:
      // Fallback: pass raw dims directly so backend can handle them
      return accumulatedDims;
  }
};

export const calculateLocalWeights = (stage, accumulatedDims) => {
  const mapped = getMappedWeights(stage, accumulatedDims);
  return { ...accumulatedDims, ...mapped };
};

/**
 * selectNextAdaptiveQuestion
 * Picks the question from pool that covers the most under-represented dimensions.
 * Supports both `weights` (new format) and `scores` (legacy format).
 */
export const selectNextAdaptiveQuestion = (pool, answeredIds, currentDimensions) => {
  const available = pool.filter(q => !answeredIds.includes(q.id));
  if (available.length === 0) return null;

  const scoredAvailable = available.map(q => {
    let score = 0;

    // Collect all dimension keys from this question's options
    const dimSet = new Set();
    if (q.options) {
      q.options.forEach(opt => {
        const src = opt.weights || opt.scores || {};
        Object.keys(src).forEach(d => dimSet.add(d));
      });
    }
    // Also check q.dimensions if present (legacy)
    if (q.dimensions) q.dimensions.forEach(d => dimSet.add(d));

    dimSet.forEach(dim => {
      const currentVal = currentDimensions[dim] || 0;
      if (currentVal === 0) score += 10;          // heavily prefer unseen dims
      else score += (1 / currentVal);             // prefer lower-coverage dims
    });

    // Small random noise to break identical ties
    score += Math.random() * 2;
    return { q, score };
  });

  scoredAvailable.sort((a, b) => b.score - a.score);
  return scoredAvailable[0].q;
};
