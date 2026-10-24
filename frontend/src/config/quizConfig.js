import { after10Questions } from './questions/after10.js';
import { after12Questions } from './questions/after12.js';
import { afterGraduationQuestions } from './questions/after_graduation.js';
import { afterPgQuestions } from './questions/after_pg.js';
import { careerSwitchQuestions } from './questions/career_switch.js';
import { upskillingQuestions } from './questions/upskilling.js';
import { entrepreneurshipQuestions } from './questions/entrepreneurship.js';
import { governmentJobsQuestions } from './questions/government_jobs.js';

import { after12PcmQuestions } from './questions/after12_pcm.js';
import { after12PcbQuestions } from './questions/after12_pcb.js';
import { after12CommerceQuestions } from './questions/after12_commerce.js';
import { after12ArtsQuestions } from './questions/after12_arts.js';
import { after12VocationalQuestions } from './questions/after12_vocational.js';
import { after12OtherQuestions } from './questions/after12_other.js';

export const careerQuizConfig = {
  after10: after10Questions,
  after12: {
    pcm: after12PcmQuestions,
    pcb: after12PcbQuestions,
    commerce: after12CommerceQuestions,
    arts: after12ArtsQuestions,
    vocational: after12VocationalQuestions,
    other: after12OtherQuestions
  },
  afterGraduation: afterGraduationQuestions,
  afterPostGraduation: afterPgQuestions,
  careerSwitch: careerSwitchQuestions,
  upskilling: upskillingQuestions,
  entrepreneurship: entrepreneurshipQuestions,
  governmentJobs: governmentJobsQuestions
};
