/**
 * CAREER EDUCATION MAP — India-Specific, Web-Researched
 * 
 * Covers all career nodeIds from the SkillScape quiz taxonomy.
 * Each entry lists all recommended degrees with:
 *   - name: Full degree name
 *   - shortName: Short label (e.g., B.Tech CSE)
 *   - duration: In years
 *   - type: 'Primary' | 'Alternative' | 'Fast Track'
 *   - streams: Eligible 12th streams
 *   - topColleges: Sample India colleges
 *   - afterDegree: Next step qualification (optional)
 */

const CAREER_EDUCATION_MAP = {
  // ─── COMPUTER SCIENCE / SOFTWARE ────────────────────────────────────────────

  'sci.eng.cs': {
    primaryDegrees: [
      { name: 'Bachelor of Technology – Computer Science Engineering', shortName: 'B.Tech CSE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'Bachelor of Engineering – Computer Science', shortName: 'B.E. CS', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'Bachelor of Computer Applications', shortName: 'BCA', duration: 3, type: 'Alternative', streams: ['PCM', 'Commerce', 'Arts'] },
      { name: 'B.Sc. Computer Science', shortName: 'B.Sc. CS', duration: 3, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech / MCA / MBA-Tech',
    notes: 'JEE Main / JEE Advanced for top NITs & IITs. KCET / MHT-CET for state colleges.',
  },

  'sci.eng.cs.software': {
    primaryDegrees: [
      { name: 'Bachelor of Technology – Computer Science Engineering', shortName: 'B.Tech CSE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'Bachelor of Engineering – Computer Science', shortName: 'B.E. CS', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'Bachelor of Computer Applications', shortName: 'BCA', duration: 3, type: 'Alternative', streams: ['PCM', 'Commerce'] },
    ],
    afterDegree: 'M.Tech Software Engineering / MCA',
    notes: 'JEE Main for NITs; state CETs for private colleges.',
  },

  'sci.eng.cs.data': {
    primaryDegrees: [
      { name: 'B.Tech Artificial Intelligence & Data Science', shortName: 'B.Tech AI & DS', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Computer Science Engineering', shortName: 'B.Tech CSE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Data Science', shortName: 'B.Sc. Data Science', duration: 3, type: 'Alternative', streams: ['PCM'] },
      { name: 'B.Sc. Statistics', shortName: 'B.Sc. Stats', duration: 3, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Data Science / M.Sc. Data Science / MBA Analytics',
    notes: 'IIT Madras B.Sc. Data Science is a popular online option alongside engineering.',
  },

  'sci.eng.cs.ai_ml': {
    primaryDegrees: [
      { name: 'B.Tech – Artificial Intelligence & Machine Learning', shortName: 'B.Tech AI/ML', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Computer Science (AI Specialization)', shortName: 'B.Tech CSE (AI)', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Artificial Intelligence', shortName: 'B.Sc. AI', duration: 3, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech AI / M.Sc. ML / PhD AI',
    notes: 'IIIT Hyderabad, VIT, SRM, Manipal offer dedicated B.Tech AI/ML programs.',
  },

  'sci.eng.cs.cybersec': {
    primaryDegrees: [
      { name: 'B.Tech – Cybersecurity', shortName: 'B.Tech Cybersecurity', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Computer Science Engineering', shortName: 'B.Tech CSE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Cybersecurity', shortName: 'B.Sc. Cybersecurity', duration: 3, type: 'Alternative', streams: ['PCM'] },
      { name: 'Bachelor of Computer Applications', shortName: 'BCA', duration: 3, type: 'Alternative', streams: ['PCM', 'Commerce'] },
    ],
    afterDegree: 'M.Tech Cybersecurity / CEH / CISSP Certification',
    notes: 'CERT-In approved cybersecurity programs available at several NITs and private institutes.',
  },

  'sci.eng.cs.pentest': {
    primaryDegrees: [
      { name: 'B.Tech Cybersecurity / Ethical Hacking', shortName: 'B.Tech Cybersecurity', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Computer Science Engineering', shortName: 'B.Tech CSE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Cybersecurity', shortName: 'B.Sc. Cybersecurity', duration: 3, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'CEH / OSCP / CISSP post-degree certifications',
    notes: 'Penetration testing roles often prefer CEH + B.Tech combo.',
  },

  'sci.eng.cs.cloudsec': {
    primaryDegrees: [
      { name: 'B.Tech Computer Science Engineering', shortName: 'B.Tech CSE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech – Cloud Computing & Security', shortName: 'B.Tech Cloud', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Computer Science', shortName: 'B.Sc. CS', duration: 3, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'AWS/Azure Cloud Certifications + M.Tech',
    notes: 'Cloud security roles increasingly require vendor certifications alongside degrees.',
  },

  'sci.eng.cs.iam': {
    primaryDegrees: [
      { name: 'B.Tech Computer Science Engineering', shortName: 'B.Tech CSE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Information Technology', shortName: 'B.Sc. IT', duration: 3, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'CISSP / CompTIA Security+ + M.Tech Cybersecurity',
    notes: 'Identity & Access Management is a specialization within cybersecurity.',
  },

  'sci.eng.cs.backend': {
    primaryDegrees: [
      { name: 'B.Tech Computer Science Engineering', shortName: 'B.Tech CSE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.E. Computer Science', shortName: 'B.E. CS', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'Bachelor of Computer Applications', shortName: 'BCA', duration: 3, type: 'Alternative', streams: ['PCM', 'Commerce'] },
    ],
    afterDegree: 'M.Tech Software Engineering',
    notes: 'Strong DSA and system design skills needed alongside degree.',
  },

  'sci.eng.cs.devops': {
    primaryDegrees: [
      { name: 'B.Tech Computer Science Engineering', shortName: 'B.Tech CSE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Information Technology', shortName: 'B.Tech IT', duration: 4, type: 'Alternative', streams: ['PCM'] },
      { name: 'Bachelor of Computer Applications', shortName: 'BCA', duration: 3, type: 'Alternative', streams: ['PCM', 'Commerce'] },
    ],
    afterDegree: 'AWS DevOps / Docker & Kubernetes certifications',
    notes: 'DevOps roles combine cloud, Linux, and CI/CD skills with CSE foundation.',
  },

  'sci.eng.cs.nlp': {
    primaryDegrees: [
      { name: 'B.Tech AI & Machine Learning', shortName: 'B.Tech AI/ML', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech CSE', shortName: 'B.Tech CSE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Linguistics + Computer Science', shortName: 'B.Sc. Linguistics + CS', duration: 3, type: 'Alternative', streams: ['PCM', 'Arts'] },
    ],
    afterDegree: 'M.Sc. / M.Tech NLP or Computational Linguistics',
    notes: 'NLP engineers blend linguistics and deep learning expertise.',
  },

  'sci.eng.cs.cv': {
    primaryDegrees: [
      { name: 'B.Tech AI & Data Science', shortName: 'B.Tech AI & DS', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Computer Science Engineering', shortName: 'B.Tech CSE', duration: 4, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Computer Vision / Research PhD',
    notes: 'IIT Bombay, IIT Madras have strong CV research groups.',
  },

  'sci.eng.cs.soc': {
    primaryDegrees: [
      { name: 'B.Tech Computer Science Engineering', shortName: 'B.Tech CSE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Social Computing', shortName: 'B.Sc. Social Computing', duration: 3, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Sc. Human-Computer Interaction',
    notes: 'Emerging specialization combining social science and computer science.',
  },

  'sci.eng.cs.ir': {
    primaryDegrees: [
      { name: 'B.Tech Computer Science Engineering', shortName: 'B.Tech CSE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Information Science', shortName: 'B.Sc. IS', duration: 3, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Information Retrieval / Library & Information Science',
    notes: 'Information retrieval is core to search engines and recommendation systems.',
  },

  'sci.eng.cs.data_analysis': {
    primaryDegrees: [
      { name: 'B.Sc. Statistics / Mathematics', shortName: 'B.Sc. Stats', duration: 3, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech AI & Data Science', shortName: 'B.Tech AI & DS', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Com (Hons)', shortName: 'B.Com Hons', duration: 3, type: 'Alternative', streams: ['Commerce'] },
    ],
    afterDegree: 'MBA Analytics / M.Sc. Data Science',
    notes: 'Excel, Power BI, SQL are essential practical tools alongside the degree.',
  },

  'sci.eng.cs.dataeng': {
    primaryDegrees: [
      { name: 'B.Tech Computer Science Engineering', shortName: 'B.Tech CSE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech – Data Engineering', shortName: 'B.Tech Data Eng', duration: 4, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Data Engineering / Cloud Certifications (AWS, GCP)',
    notes: 'Apache Spark, Kafka, cloud pipelines experience is critical.',
  },

  // ─── ELECTRONICS / ELECTRICAL ───────────────────────────────────────────────

  'sci.eng.ece': {
    primaryDegrees: [
      { name: 'B.Tech Electronics & Communication Engineering', shortName: 'B.Tech ECE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.E. Electronics & Communication', shortName: 'B.E. EC', duration: 4, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech VLSI / Embedded Systems / Signal Processing',
    notes: 'GATE exam for M.Tech at IITs, NITs. Strong demand in semiconductor & telecom sector.',
  },

  'sci.eng.elec': {
    primaryDegrees: [
      { name: 'B.Tech Electrical Engineering', shortName: 'B.Tech EE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.E. Electrical Engineering', shortName: 'B.E. EE', duration: 4, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Power Systems / M.Tech High Voltage Engineering',
    notes: 'GATE EE score used for PSU jobs (BHEL, NTPC, Power Grid).',
  },

  'sci.eng.eee': {
    primaryDegrees: [
      { name: 'B.Tech Electrical & Electronics Engineering', shortName: 'B.Tech EEE', duration: 4, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Power Electronics',
    notes: 'Offered at NITs, VIT, SRM, Manipal. Strong demand in EV & energy sector.',
  },

  'sci.eng.telecom': {
    primaryDegrees: [
      { name: 'B.Tech Electronics & Communication Engineering', shortName: 'B.Tech ECE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Telecommunications Engineering', shortName: 'B.Tech Telecom', duration: 4, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Telecommunications / MBA Telecom Management',
    notes: '5G and satellite communication specialists are in high demand.',
  },

  'sci.eng.vlsi': {
    primaryDegrees: [
      { name: 'B.Tech Electronics & Communication Engineering', shortName: 'B.Tech ECE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Electrical & Electronics Engineering', shortName: 'B.Tech EEE', duration: 4, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech VLSI Design / PG Diploma VLSI (C-DAC)',
    notes: 'C-DAC ACTS VLSI PG Diploma is highly sought after. IIT Bombay, IIT Madras top for VLSI research.',
  },

  'sci.eng.bim': {
    primaryDegrees: [
      { name: 'B.Tech Civil Engineering', shortName: 'B.Tech Civil', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Arch', shortName: 'B.Arch', duration: 5, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Construction Technology / BIM Autodesk Revit Certification',
    notes: 'BIM (Building Information Modeling) specialist demand growing rapidly in India.',
  },

  // ─── MECHANICAL / AUTO ──────────────────────────────────────────────────────

  'sci.eng.mech': {
    primaryDegrees: [
      { name: 'B.Tech Mechanical Engineering', shortName: 'B.Tech Mech', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.E. Mechanical Engineering', shortName: 'B.E. Mech', duration: 4, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Thermal / Manufacturing / Design / PhD',
    notes: 'GATE ME score for PSU jobs (BHEL, HAL, DRDO). Auto sector hiring high.',
  },

  'sci.eng.mech.design': {
    primaryDegrees: [
      { name: 'B.Tech Mechanical Engineering', shortName: 'B.Tech Mech', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Des Industrial Design', shortName: 'B.Des Industrial', duration: 4, type: 'Alternative', streams: ['PCM', 'Arts'] },
    ],
    afterDegree: 'M.Tech Machine Design / NID PG Design',
    notes: 'CAD software (SolidWorks, CATIA, AutoCAD) proficiency is essential.',
  },

  'sci.eng.mech.mfg': {
    primaryDegrees: [
      { name: 'B.Tech Mechanical Engineering', shortName: 'B.Tech Mech', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Manufacturing Engineering', shortName: 'B.Tech Mfg', duration: 4, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Manufacturing / Lean Six Sigma certification',
    notes: 'Manufacturing engineers work in auto, aerospace, FMCG sectors.',
  },

  'sci.eng.mech.thermal': {
    primaryDegrees: [
      { name: 'B.Tech Mechanical Engineering', shortName: 'B.Tech Mech', duration: 4, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Thermal Engineering / Heat Transfer',
    notes: 'GATE ME for M.Tech at IITs. Thermal engineers work in power plants and HVAC.',
  },

  'sci.eng.mech.aero': {
    primaryDegrees: [
      { name: 'B.Tech Aerospace Engineering', shortName: 'B.Tech Aerospace', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Mechanical Engineering', shortName: 'B.Tech Mech', duration: 4, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Aerospace / DRDO / ISRO scientist pathway',
    notes: 'IIT Bombay, IIT Madras, IIT Kanpur have excellent Aerospace programs.',
  },

  'sci.eng.mech.robot': {
    primaryDegrees: [
      { name: 'B.Tech Mechatronics Engineering', shortName: 'B.Tech Mechatronics', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Robotics & Automation', shortName: 'B.Tech Robotics', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Mechanical Engineering', shortName: 'B.Tech Mech', duration: 4, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Robotics / ROS (Robot Operating System) certifications',
    notes: 'Growing demand in EV, defense, manufacturing automation sectors.',
  },

  'sci.eng.mechatronics': {
    primaryDegrees: [
      { name: 'B.Tech Mechatronics Engineering', shortName: 'B.Tech Mechatronics', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Mechanical + Electronics (Dual)', shortName: 'B.Tech Dual', duration: 4, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Automation / Embedded Systems',
    notes: 'Mechatronics combines mechanical, electronics, and software skills.',
  },

  'sci.eng.aero': {
    primaryDegrees: [
      { name: 'B.Tech Aeronautical / Aerospace Engineering', shortName: 'B.Tech Aerospace', duration: 4, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Aerospace / ISRO / HAL careers',
    notes: 'IIT Bombay, MIT Manipal, SRM offer top Aerospace programs.',
  },

  'sci.eng.auto': {
    primaryDegrees: [
      { name: 'B.Tech Automobile Engineering', shortName: 'B.Tech Auto', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Mechanical Engineering', shortName: 'B.Tech Mech', duration: 4, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Automotive Engineering / MBA Operations',
    notes: 'EV specialization increasingly valuable. Tata, Maruti, Mahindra hire here.',
  },

  // ─── CIVIL / ARCHITECTURE ───────────────────────────────────────────────────

  'sci.eng.civil': {
    primaryDegrees: [
      { name: 'B.Tech Civil Engineering', shortName: 'B.Tech Civil', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.E. Civil Engineering', shortName: 'B.E. Civil', duration: 4, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Structural / Geotechnical / Transportation Engg',
    notes: 'GATE CE score for government jobs (CPWD, PWD, NHAI).',
  },

  'law.civil': {
    primaryDegrees: [
      { name: 'B.A. LLB (Integrated 5-Year)', shortName: 'B.A. LLB', duration: 5, type: 'Primary', streams: ['Arts', 'Commerce', 'PCM'] },
      { name: 'LLB (After Graduation)', shortName: 'LLB', duration: 3, type: 'Alternative', streams: ['Any'] },
    ],
    afterDegree: 'Enroll with the State Bar Council and clear the AIBE to practice; consider an LLM in Civil Law.',
    notes: 'For courtroom practice in India, complete a recognized LLB and meet Bar Council enrolment requirements.',
  },
  'law.criminal': {
    primaryDegrees: [
      { name: 'B.A. LLB (Integrated 5-Year)', shortName: 'B.A. LLB', duration: 5, type: 'Primary', streams: ['Arts', 'Commerce', 'PCM'] },
      { name: 'LLB (After Graduation)', shortName: 'LLB', duration: 3, type: 'Alternative', streams: ['Any'] },
    ],
    afterDegree: 'Enroll with the State Bar Council and clear the AIBE; consider an LLM in Criminal Law or public prosecution.',
    notes: 'CLAT and other university entrance routes may apply to integrated law programs.',
  },
  'law.civil_law': {
    primaryDegrees: [
      { name: 'B.A. LLB (Integrated 5-Year)', shortName: 'B.A. LLB', duration: 5, type: 'Primary', streams: ['Arts', 'Commerce', 'PCM'] },
      { name: 'LLB (After Graduation)', shortName: 'LLB', duration: 3, type: 'Alternative', streams: ['Any'] },
    ],
    afterDegree: 'Enroll with the State Bar Council and clear the AIBE; consider an LLM in Civil Law.',
    notes: 'For courtroom practice in India, complete a recognized LLB and meet Bar Council enrolment requirements.',
  },
  'law.criminal_law': {
    primaryDegrees: [
      { name: 'B.A. LLB (Integrated 5-Year)', shortName: 'B.A. LLB', duration: 5, type: 'Primary', streams: ['Arts', 'Commerce', 'PCM'] },
      { name: 'LLB (After Graduation)', shortName: 'LLB', duration: 3, type: 'Alternative', streams: ['Any'] },
    ],
    afterDegree: 'Enroll with the State Bar Council and clear the AIBE; consider an LLM in Criminal Law or public prosecution.',
    notes: 'CLAT and other university entrance routes may apply to integrated law programs.',
  },
  'law.corporate_law': {
    primaryDegrees: [
      { name: 'BBA LLB or B.A. LLB (Integrated 5-Year)', shortName: 'BBA / B.A. LLB', duration: 5, type: 'Primary', streams: ['Arts', 'Commerce'] },
      { name: 'LLB (After Graduation)', shortName: 'LLB', duration: 3, type: 'Alternative', streams: ['Any'] },
    ],
    afterDegree: 'Enroll with the State Bar Council; specialize through an LLM in Corporate Law or a compliance qualification.',
    notes: 'A commerce background can help with contracts, company law, and finance-focused legal work.',
  },
  'arts.law.corp': {
    primaryDegrees: [
      { name: 'B.A. LLB or BBA LLB (Integrated 5-Year)', shortName: 'B.A. / BBA LLB', duration: 5, type: 'Primary', streams: ['Arts', 'Commerce'] },
      { name: 'LLB (After Graduation)', shortName: 'LLB', duration: 3, type: 'Alternative', streams: ['Any'] },
    ],
    afterDegree: 'Enroll with the State Bar Council; specialize through an LLM in Corporate Law or a compliance qualification.',
    notes: 'A commerce background can help with contracts, company law, and finance-focused legal work.',
  },
  'sci.eng.civil.struct': { ...{ primaryDegrees: [{ name: 'B.Tech Civil Engineering', shortName: 'B.Tech Civil', duration: 4, type: 'Primary', streams: ['PCM'] }], afterDegree: 'M.Tech Structural Engineering', notes: 'STAAD Pro, ETABS software skills essential.' } },
  'sci.eng.civil.structural': { ...{ primaryDegrees: [{ name: 'B.Tech Civil Engineering', shortName: 'B.Tech Civil', duration: 4, type: 'Primary', streams: ['PCM'] }], afterDegree: 'M.Tech Structural Engineering', notes: 'STAAD Pro, ETABS software skills essential.' } },
  'sci.eng.civil.geotech': { primaryDegrees: [{ name: 'B.Tech Civil Engineering', shortName: 'B.Tech Civil', duration: 4, type: 'Primary', streams: ['PCM'] }], afterDegree: 'M.Tech Geotechnical Engineering', notes: 'Foundation and soil analysis expertise.' },
  'sci.eng.civil.transport': { primaryDegrees: [{ name: 'B.Tech Civil Engineering', shortName: 'B.Tech Civil', duration: 4, type: 'Primary', streams: ['PCM'] }], afterDegree: 'M.Tech Transportation Engineering', notes: 'Road, rail, urban mobility planning.' },
  'sci.eng.civil.surveying': { primaryDegrees: [{ name: 'B.Tech Civil Engineering', shortName: 'B.Tech Civil', duration: 4, type: 'Primary', streams: ['PCM'] }], afterDegree: 'M.Tech Geomatics / GIS Certification', notes: 'GPS, GIS, drone surveying skills essential.' },
  'sci.eng.civil.water': { primaryDegrees: [{ name: 'B.Tech Civil Engineering', shortName: 'B.Tech Civil', duration: 4, type: 'Primary', streams: ['PCM'] }], afterDegree: 'M.Tech Water Resources Engineering', notes: 'Hydrology and irrigation expertise.' },
  'sci.eng.civil.environ_civil': { primaryDegrees: [{ name: 'B.Tech Civil Engineering', shortName: 'B.Tech Civil', duration: 4, type: 'Primary', streams: ['PCM'] }], afterDegree: 'M.Tech Environmental Engineering', notes: 'Water treatment, waste management roles.' },
  'sci.eng.civil.construction': { primaryDegrees: [{ name: 'B.Tech Civil Engineering', shortName: 'B.Tech Civil', duration: 4, type: 'Primary', streams: ['PCM'] }], afterDegree: 'MBA Construction Management / PMP Certification', notes: 'Project management is core to this role.' },
  'sci.eng.civil.env': { primaryDegrees: [{ name: 'B.Tech Civil Engineering', shortName: 'B.Tech Civil', duration: 4, type: 'Primary', streams: ['PCM'] }], afterDegree: 'M.Tech Environmental Engineering', notes: 'Sustainable development focus.' },

  'sci.eng.arch': {
    primaryDegrees: [
      { name: 'Bachelor of Architecture', shortName: 'B.Arch', duration: 5, type: 'Primary', streams: ['PCM', 'Arts'] },
    ],
    afterDegree: 'M.Arch / Council of Architecture license (CoA)',
    notes: 'NATA exam required for B.Arch admission in India.',
  },

  'sci.eng.environ': {
    primaryDegrees: [
      { name: 'B.Tech Environmental Engineering', shortName: 'B.Tech Environ', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Environmental Science', shortName: 'B.Sc. Env Science', duration: 3, type: 'Alternative', streams: ['PCB', 'PCM'] },
    ],
    afterDegree: 'M.Tech Environmental Engineering / M.Sc. Environmental Science',
    notes: 'Growing demand in green energy, ESG consulting, EIA sector.',
  },

  'sci.eng.material': {
    primaryDegrees: [
      { name: 'B.Tech Materials Science & Engineering', shortName: 'B.Tech Materials', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Tech Metallurgical Engineering', shortName: 'B.Tech Metallurgy', duration: 4, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech Materials / Nanotechnology Research',
    notes: 'Defense, semiconductor, battery technology sectors hiring.',
  },

  'sci.eng.chem': {
    primaryDegrees: [
      { name: 'B.Tech Chemical Engineering', shortName: 'B.Tech Chem', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Chemistry (Hons)', shortName: 'B.Sc. Chemistry', duration: 3, type: 'Alternative', streams: ['PCB', 'PCM'] },
    ],
    afterDegree: 'M.Tech Chemical Engineering / M.Sc. Chemistry',
    notes: 'Pharma, petrochemical, FMCG sectors hire chemical engineers.',
  },

  'sci.eng.it': {
    primaryDegrees: [
      { name: 'B.Tech Information Technology', shortName: 'B.Tech IT', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'Bachelor of Computer Applications', shortName: 'BCA', duration: 3, type: 'Alternative', streams: ['PCM', 'Commerce'] },
    ],
    afterDegree: 'MCA / M.Tech IT',
    notes: 'IT sector has high demand; NASSCOM reports 5M+ IT professionals needed by 2030.',
  },

  // ─── MEDICAL / HEALTH ───────────────────────────────────────────────────────

  'sci.med.mbbs': {
    primaryDegrees: [
      { name: 'Bachelor of Medicine, Bachelor of Surgery', shortName: 'MBBS', duration: 5.5, type: 'Primary', streams: ['PCB'] },
    ],
    afterDegree: 'MD / MS Residency (3 years)',
    notes: 'NEET-UG required. Top colleges: AIIMS, JIPMER, AFMC. 1-year internship included.',
  },

  'deg.mbbs': {
    primaryDegrees: [
      { name: 'Bachelor of Medicine, Bachelor of Surgery', shortName: 'MBBS', duration: 5.5, type: 'Primary', streams: ['PCB'] },
    ],
    afterDegree: 'MD / MS / MCh Residency',
    notes: 'NEET-UG required. 900+ medical colleges in India.',
  },

  'sci.med.mbbs.psychiatry': {
    primaryDegrees: [
      { name: 'MBBS + MD Psychiatry', shortName: 'MBBS + MD Psychiatry', duration: 8.5, type: 'Primary', streams: ['PCB'] },
    ],
    afterDegree: 'DM Psychiatry / Clinical Fellowship',
    notes: 'NEET PG for MD. Major shortage of psychiatrists in India (~10,000 vs need of 150,000).',
  },

  'sci.med.nursing': {
    primaryDegrees: [
      { name: 'B.Sc. Nursing', shortName: 'B.Sc. Nursing', duration: 4, type: 'Primary', streams: ['PCB'] },
      { name: 'General Nursing & Midwifery', shortName: 'GNM Diploma', duration: 3, type: 'Alternative', streams: ['PCB', 'Arts'] },
    ],
    afterDegree: 'M.Sc. Nursing / NP Certification',
    notes: 'NEET is not required for B.Sc. Nursing. State nursing councils regulate.',
  },

  'deg.bsc.nursing': {
    primaryDegrees: [
      { name: 'B.Sc. Nursing', shortName: 'B.Sc. Nursing', duration: 4, type: 'Primary', streams: ['PCB'] },
      { name: 'GNM (General Nursing & Midwifery)', shortName: 'GNM Diploma', duration: 3, type: 'Alternative', streams: ['PCB'] },
      { name: 'ANM (Auxiliary Nurse Midwifery)', shortName: 'ANM Diploma', duration: 2, type: 'Fast Track', streams: ['Arts', 'PCB'] },
    ],
    afterDegree: 'M.Sc. Nursing',
    notes: 'Huge demand in India + overseas (Gulf, UK, USA) for Indian nurses.',
  },

  'sci.med.allied': {
    primaryDegrees: [
      { name: 'B.Sc. Allied Health Sciences', shortName: 'B.Sc. Allied Health', duration: 3, type: 'Primary', streams: ['PCB'] },
      { name: 'B.Sc. Medical Lab Technology', shortName: 'B.Sc. MLT', duration: 3, type: 'Primary', streams: ['PCB'] },
      { name: 'B.Sc. Radiology & Imaging Technology', shortName: 'B.Sc. Radiology', duration: 3, type: 'Alternative', streams: ['PCB'] },
    ],
    afterDegree: 'M.Sc. Allied Health / Clinical Specialization',
    notes: 'AIIMS, CMC Vellore, Manipal offer top Allied Health programs.',
  },

  'sci.med.physio': {
    primaryDegrees: [
      { name: 'Bachelor of Physiotherapy', shortName: 'BPT', duration: 4.5, type: 'Primary', streams: ['PCB'] },
    ],
    afterDegree: 'MPT (Master of Physiotherapy)',
    notes: 'Growing demand in sports rehab, neurology, and geriatric physiotherapy.',
  },

  'deg.bpt': {
    primaryDegrees: [
      { name: 'Bachelor of Physiotherapy', shortName: 'BPT', duration: 4.5, type: 'Primary', streams: ['PCB'] },
    ],
    afterDegree: 'MPT – Orthopedics / Neurology / Sports',
    notes: 'Internship of 6 months mandatory. Registration with IAP required.',
  },

  'sci.med.pharma': {
    primaryDegrees: [
      { name: 'Bachelor of Pharmacy', shortName: 'B.Pharm', duration: 4, type: 'Primary', streams: ['PCB', 'PCM'] },
      { name: 'Diploma in Pharmacy', shortName: 'D.Pharm', duration: 2, type: 'Fast Track', streams: ['PCB'] },
    ],
    afterDegree: 'M.Pharm / MBA Pharma Management',
    notes: 'GPAT for M.Pharm admission. Pharma sector valued at $50Bn+ in India.',
  },

  'deg.bpharm': {
    primaryDegrees: [
      { name: 'Bachelor of Pharmacy', shortName: 'B.Pharm', duration: 4, type: 'Primary', streams: ['PCB', 'PCM'] },
      { name: 'Diploma in Pharmacy', shortName: 'D.Pharm', duration: 2, type: 'Fast Track', streams: ['PCB'] },
    ],
    afterDegree: 'M.Pharm / Pharm.D / MBA Healthcare',
    notes: 'PCI (Pharmacy Council of India) regulates pharmacist education.',
  },

  'deg.bams': {
    primaryDegrees: [
      { name: 'Bachelor of Ayurvedic Medicine & Surgery', shortName: 'BAMS', duration: 5.5, type: 'Primary', streams: ['PCB'] },
    ],
    afterDegree: 'MD Ayurveda',
    notes: 'NEET required. Ayurveda council (CCIM) regulates. 270+ BAMS colleges in India.',
  },

  'deg.bhms': {
    primaryDegrees: [
      { name: 'Bachelor of Homeopathic Medicine & Surgery', shortName: 'BHMS', duration: 5.5, type: 'Primary', streams: ['PCB'] },
    ],
    afterDegree: 'MD Homeopathy',
    notes: 'NEET required. Central Council of Homeopathy regulates.',
  },

  'deg.bds': {
    primaryDegrees: [
      { name: 'Bachelor of Dental Surgery', shortName: 'BDS', duration: 5, type: 'Primary', streams: ['PCB'] },
    ],
    afterDegree: 'MDS (Master of Dental Surgery)',
    notes: 'NEET-UG required. Dental Council of India regulates.',
  },

  'deg.bvsc': {
    primaryDegrees: [
      { name: 'Bachelor of Veterinary Science & Animal Husbandry', shortName: 'B.V.Sc.', duration: 5.5, type: 'Primary', streams: ['PCB'] },
    ],
    afterDegree: 'M.V.Sc. / PhD Veterinary Sciences',
    notes: 'NEET-UG required for some states. State-specific veterinary entrance tests.',
  },

  'deg.bsc.microbio': {
    primaryDegrees: [
      { name: 'B.Sc. Microbiology', shortName: 'B.Sc. Microbiology', duration: 3, type: 'Primary', streams: ['PCB'] },
      { name: 'B.Sc. Biotechnology', shortName: 'B.Sc. Biotech', duration: 3, type: 'Alternative', streams: ['PCB'] },
    ],
    afterDegree: 'M.Sc. Microbiology / Biotechnology',
    notes: 'Pharma, diagnostics, food technology careers after graduation.',
  },

  'deg.bsc.biotech': {
    primaryDegrees: [
      { name: 'B.Sc. Biotechnology', shortName: 'B.Sc. Biotech', duration: 3, type: 'Primary', streams: ['PCB'] },
      { name: 'B.Tech Biotechnology', shortName: 'B.Tech Biotech', duration: 4, type: 'Alternative', streams: ['PCB', 'PCM'] },
    ],
    afterDegree: 'M.Sc. Biotechnology / M.Tech Biotech',
    notes: 'JNU, DU, Pune University top for B.Sc. Biotech.',
  },

  'deg.btech.biotech': {
    primaryDegrees: [
      { name: 'B.Tech Biotechnology Engineering', shortName: 'B.Tech Biotech', duration: 4, type: 'Primary', streams: ['PCB', 'PCM'] },
    ],
    afterDegree: 'M.Tech Biotech / Research PhD',
    notes: 'Manipal, VIT, Amity have strong B.Tech Biotech programs.',
  },

  'deg.bsc.genetics': {
    primaryDegrees: [
      { name: 'B.Sc. Genetics', shortName: 'B.Sc. Genetics', duration: 3, type: 'Primary', streams: ['PCB'] },
      { name: 'B.Sc. Molecular Biology', shortName: 'B.Sc. Mol Bio', duration: 3, type: 'Alternative', streams: ['PCB'] },
    ],
    afterDegree: 'M.Sc. Genetics / PhD Life Sciences',
    notes: 'Genomics, genetic counseling, bioinformatics are growing fields.',
  },

  'deg.bsc.nutri': {
    primaryDegrees: [
      { name: 'B.Sc. Nutrition & Dietetics', shortName: 'B.Sc. Nutrition', duration: 3, type: 'Primary', streams: ['PCB', 'Home Science'] },
      { name: 'B.Sc. Food Technology', shortName: 'B.Sc. Food Tech', duration: 3, type: 'Alternative', streams: ['PCB', 'PCM'] },
    ],
    afterDegree: 'M.Sc. Nutrition & Dietetics / RDN Certification',
    notes: 'Dietitians needed in hospitals, sports, corporate wellness programs.',
  },

  // ─── SCIENCE / PURE ─────────────────────────────────────────────────────────

  'sci.pure.maths': {
    primaryDegrees: [
      { name: 'B.Sc. Mathematics (Hons)', shortName: 'B.Sc. Maths', duration: 3, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Stat (Indian Statistical Institute)', shortName: 'B.Stat (ISI)', duration: 3, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Math (BSMS) – IISER', shortName: 'BS-MS', duration: 5, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Sc. Mathematics / MBA / Actuarial Science',
    notes: 'ISI Kolkata, CMI Chennai, IIT Math departments are premier destinations.',
  },

  'sci.pure.physics': {
    primaryDegrees: [
      { name: 'B.Sc. Physics (Hons)', shortName: 'B.Sc. Physics', duration: 3, type: 'Primary', streams: ['PCM'] },
      { name: 'BS-MS Physics – IISER / IIT', shortName: 'BS-MS Physics', duration: 5, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Sc. Physics / PhD / Research at TIFR, BARC',
    notes: 'IISER, IIT, DU have strong physics programs. JAM exam for IIT M.Sc.',
  },

  'sci.pure.env_sci': {
    primaryDegrees: [
      { name: 'B.Sc. Environmental Science', shortName: 'B.Sc. Env Science', duration: 3, type: 'Primary', streams: ['PCB', 'PCM'] },
      { name: 'B.Tech Environmental Engineering', shortName: 'B.Tech Environ', duration: 4, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Sc. Environmental Science / MBA Sustainability',
    notes: 'ESG and sustainability consulting growing rapidly post-COP agreements.',
  },

  'deg.bsc.eco': {
    primaryDegrees: [
      { name: 'B.Sc. Economics (Hons)', shortName: 'B.Sc. Economics', duration: 3, type: 'Primary', streams: ['Commerce', 'Arts'] },
      { name: 'B.A. Economics (Hons)', shortName: 'B.A. Economics', duration: 3, type: 'Primary', streams: ['Commerce', 'Arts'] },
    ],
    afterDegree: 'M.Sc. Economics / MBA Finance / IES (Indian Economic Service)',
    notes: 'Delhi School of Economics, Presidency, SRCC top economics colleges.',
  },

  'deg.bstat': {
    primaryDegrees: [
      { name: 'B.Stat – Indian Statistical Institute', shortName: 'B.Stat (ISI)', duration: 3, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Statistics (Hons)', shortName: 'B.Sc. Statistics', duration: 3, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Stat / M.Sc. Statistics / PhD',
    notes: 'ISI Delhi/Kolkata/Bangalore are premier stats institutions in India.',
  },

  'deg.bsc.agri': {
    primaryDegrees: [
      { name: 'B.Sc. Agriculture', shortName: 'B.Sc. Agriculture', duration: 4, type: 'Primary', streams: ['PCB', 'PCM'] },
      { name: 'B.Tech Agricultural Engineering', shortName: 'B.Tech Agri', duration: 4, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Sc. Agriculture / MBA Agribusiness',
    notes: 'ICAR-AIEEA entrance exam for top agri colleges. IARI, BHU, PAU are top.',
  },

  'deg.bsc.data': {
    primaryDegrees: [
      { name: 'B.Sc. Data Science', shortName: 'B.Sc. Data Science', duration: 3, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Statistics', shortName: 'B.Sc. Stats', duration: 3, type: 'Alternative', streams: ['PCM'] },
      { name: 'B.Tech AI & Data Science', shortName: 'B.Tech AI & DS', duration: 4, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Sc. Data Science / MBA Analytics',
    notes: 'IIT Madras offers a popular online B.Sc. Data Science accessible to all streams.',
  },

  // ─── COMMERCE / FINANCE / MANAGEMENT ────────────────────────────────────────

  'com.fin': {
    primaryDegrees: [
      { name: 'Bachelor of Commerce (Hons)', shortName: 'B.Com Hons', duration: 3, type: 'Primary', streams: ['Commerce'] },
      { name: 'Bachelor of Business Administration – Finance', shortName: 'BBA Finance', duration: 3, type: 'Alternative', streams: ['Commerce', 'PCM'] },
      { name: 'Bachelor of Economics', shortName: 'B.Sc./B.A. Economics', duration: 3, type: 'Alternative', streams: ['Commerce', 'Arts'] },
    ],
    afterDegree: 'CA / CFA / MBA Finance',
    notes: 'CFA from CFA Institute is globally recognized. CA via ICAI.',
  },

  'com.finance': {
    primaryDegrees: [
      { name: 'Bachelor of Commerce (Hons)', shortName: 'B.Com Hons', duration: 3, type: 'Primary', streams: ['Commerce'] },
      { name: 'BBA Finance', shortName: 'BBA Finance', duration: 3, type: 'Alternative', streams: ['Commerce'] },
    ],
    afterDegree: 'CA / CFA / MBA Finance',
    notes: 'NSE, BSE certifications valuable alongside degree.',
  },

  'com.fin.risk': {
    primaryDegrees: [
      { name: 'B.Com (Hons)', shortName: 'B.Com Hons', duration: 3, type: 'Primary', streams: ['Commerce'] },
      { name: 'BBA Finance', shortName: 'BBA Finance', duration: 3, type: 'Alternative', streams: ['Commerce'] },
      { name: 'B.Sc. Statistics', shortName: 'B.Sc. Stats', duration: 3, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'FRM (Financial Risk Manager) / CFA / MBA Risk Management',
    notes: 'FRM from GARP is the top global credential for risk professionals.',
  },

  'com.fin.ib': {
    primaryDegrees: [
      { name: 'B.Com (Hons)', shortName: 'B.Com Hons', duration: 3, type: 'Primary', streams: ['Commerce'] },
      { name: 'BBA Finance', shortName: 'BBA Finance', duration: 3, type: 'Alternative', streams: ['Commerce'] },
    ],
    afterDegree: 'MBA Finance (IIM/ISB) / CFA',
    notes: 'IIMs, XLRI, ISB are gateway to investment banking. Internships critical.',
  },

  'com.fin.analysis': {
    primaryDegrees: [
      { name: 'B.Com (Hons)', shortName: 'B.Com Hons', duration: 3, type: 'Primary', streams: ['Commerce'] },
      { name: 'B.Sc. Statistics', shortName: 'B.Sc. Stats', duration: 3, type: 'Alternative', streams: ['PCM'] },
      { name: 'BBA Finance', shortName: 'BBA Finance', duration: 3, type: 'Alternative', streams: ['Commerce'] },
    ],
    afterDegree: 'CFA / MBA Finance / NISM Certifications',
    notes: 'Excel, Bloomberg terminal skills are essential for equity analysts.',
  },

  'com.fin.eco': {
    primaryDegrees: [
      { name: 'B.A. Economics (Hons)', shortName: 'B.A. Economics', duration: 3, type: 'Primary', streams: ['Commerce', 'Arts'] },
      { name: 'B.Sc. Economics', shortName: 'B.Sc. Economics', duration: 3, type: 'Alternative', streams: ['Commerce', 'PCM'] },
    ],
    afterDegree: 'M.Sc. Economics / MBA Finance / IES',
    notes: 'Delhi School of Economics, Presidency University top picks.',
  },

  'com.banking': {
    primaryDegrees: [
      { name: 'B.Com (Hons)', shortName: 'B.Com Hons', duration: 3, type: 'Primary', streams: ['Commerce'] },
      { name: 'BBA', shortName: 'BBA', duration: 3, type: 'Alternative', streams: ['Commerce', 'PCM', 'Arts'] },
      { name: 'B.A. Economics', shortName: 'B.A. Economics', duration: 3, type: 'Alternative', streams: ['Commerce', 'Arts'] },
    ],
    afterDegree: 'IBPS PO/Clerk Exam / MBA Finance / CAIIB',
    notes: 'IBPS, SBI PO exams for government banking careers.',
  },

  'com.mktg': {
    primaryDegrees: [
      { name: 'BBA Marketing', shortName: 'BBA Marketing', duration: 3, type: 'Primary', streams: ['Commerce', 'PCM', 'Arts'] },
      { name: 'B.Com', shortName: 'B.Com', duration: 3, type: 'Alternative', streams: ['Commerce'] },
    ],
    afterDegree: 'MBA Marketing / Google/Meta Digital Marketing Certifications',
    notes: 'Digital marketing certifications from Google, HubSpot widely valued.',
  },

  'com.marketing': {
    primaryDegrees: [
      { name: 'BBA Marketing', shortName: 'BBA Marketing', duration: 3, type: 'Primary', streams: ['Commerce', 'PCM', 'Arts'] },
      { name: 'B.Com', shortName: 'B.Com', duration: 3, type: 'Alternative', streams: ['Commerce'] },
    ],
    afterDegree: 'MBA Marketing',
    notes: 'CAT/XAT for top MBA programs in marketing.',
  },

  'com.mgmt': {
    primaryDegrees: [
      { name: 'BBA', shortName: 'BBA', duration: 3, type: 'Primary', streams: ['Commerce', 'PCM', 'Arts'] },
      { name: 'B.Com (Hons)', shortName: 'B.Com Hons', duration: 3, type: 'Alternative', streams: ['Commerce'] },
    ],
    afterDegree: 'MBA (IIM / XLRI / ISB)',
    notes: 'CAT score opens IIMs. MBA from top school essential for senior management.',
  },

  'com.mgmt.analytics': {
    primaryDegrees: [
      { name: 'BBA', shortName: 'BBA', duration: 3, type: 'Primary', streams: ['Commerce', 'PCM'] },
      { name: 'B.Sc. Statistics', shortName: 'B.Sc. Stats', duration: 3, type: 'Alternative', streams: ['PCM'] },
      { name: 'B.Tech CSE / Data Science', shortName: 'B.Tech CSE', duration: 4, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'MBA Business Analytics / M.Sc. Data Science',
    notes: 'Business analytics bridges data science and management consulting.',
  },

  'com.mgmt.consult': {
    primaryDegrees: [
      { name: 'BBA', shortName: 'BBA', duration: 3, type: 'Primary', streams: ['Commerce', 'PCM', 'Arts'] },
      { name: 'B.Tech (Any Engineering)', shortName: 'B.Tech', duration: 4, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'MBA (IIM / ISB) – CAT/GMAT',
    notes: 'McKinsey, BCG, Deloitte recruit from top MBA programs. CAT score crucial.',
  },

  'com.hr': {
    primaryDegrees: [
      { name: 'BBA Human Resource Management', shortName: 'BBA HRM', duration: 3, type: 'Primary', streams: ['Commerce', 'Arts'] },
      { name: 'B.A. Psychology', shortName: 'B.A. Psychology', duration: 3, type: 'Alternative', streams: ['Arts'] },
    ],
    afterDegree: 'MBA HR / PGDM HR (XLRI)',
    notes: 'XLRI Jamshedpur is the gold standard for HR education in India.',
  },

  'com.startup': {
    primaryDegrees: [
      { name: 'BBA Entrepreneurship', shortName: 'BBA Entrepreneurship', duration: 3, type: 'Primary', streams: ['Commerce', 'PCM', 'Arts'] },
      { name: 'B.Tech (Any)', shortName: 'B.Tech', duration: 4, type: 'Alternative', streams: ['PCM'] },
      { name: 'B.Com / BBA', shortName: 'B.Com / BBA', duration: 3, type: 'Alternative', streams: ['Commerce'] },
    ],
    afterDegree: 'MBA Entrepreneurship / Startup India Incubator Programs',
    notes: 'IIM Ahmedabad, IIT Madras, BITS Pilani have strong startup ecosystems.',
  },

  'mgmt.mba': {
    primaryDegrees: [
      { name: 'Bachelor\'s Degree (Any Stream)', shortName: 'Any Bachelor\'s', duration: 3, type: 'Primary', streams: ['Any'] },
    ],
    afterDegree: 'MBA from IIM / ISB / XLRI (via CAT/GMAT)',
    notes: 'MBA is a postgraduate degree; a bachelor\'s is the prerequisite. CAT for IIMs.',
  },

  'mgmt.hr': {
    primaryDegrees: [
      { name: 'BBA Human Resource Management', shortName: 'BBA HRM', duration: 3, type: 'Primary', streams: ['Commerce', 'Arts'] },
      { name: 'B.A. Psychology', shortName: 'B.A. Psychology', duration: 3, type: 'Alternative', streams: ['Arts'] },
    ],
    afterDegree: 'MBA HR / PGDM HR',
    notes: 'XLRI BM/HRM programs are highly regarded for HR leadership roles.',
  },

  'mgmt.project': {
    primaryDegrees: [
      { name: 'B.Tech (Any Engineering)', shortName: 'B.Tech', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'BBA', shortName: 'BBA', duration: 3, type: 'Alternative', streams: ['Commerce', 'PCM'] },
    ],
    afterDegree: 'PMP Certification / MBA Operations / M.Tech',
    notes: 'PMP (Project Management Professional) from PMI is the gold standard globally.',
  },

  'mgmt.operations': {
    primaryDegrees: [
      { name: 'B.Tech Industrial Engineering', shortName: 'B.Tech IE', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'BBA Operations', shortName: 'BBA Operations', duration: 3, type: 'Alternative', streams: ['Commerce', 'PCM'] },
    ],
    afterDegree: 'MBA Operations / Six Sigma Black Belt',
    notes: 'Supply chain, logistics, lean manufacturing are core domains.',
  },

  // ─── PROFESSIONAL QUALIFICATIONS ────────────────────────────────────────────

  'prof.ca': {
    primaryDegrees: [
      { name: 'Chartered Accountancy (ICAI)', shortName: 'CA', duration: 5, type: 'Primary', streams: ['Commerce', 'PCM', 'Arts'] },
      { name: 'B.Com (Hons) + CA Articleship', shortName: 'B.Com + CA', duration: 6, type: 'Alternative', streams: ['Commerce'] },
    ],
    afterDegree: 'CPA / ACCA / CFA (International)',
    notes: 'CA via ICAI: Foundation → Intermediate → Final + 3-year Articleship.',
  },

  'prof.cs': {
    primaryDegrees: [
      { name: 'Company Secretary (ICSI)', shortName: 'CS', duration: 5, type: 'Primary', streams: ['Commerce', 'Arts', 'PCM'] },
      { name: 'B.Com + CS', shortName: 'B.Com + CS', duration: 6, type: 'Alternative', streams: ['Commerce'] },
    ],
    afterDegree: 'LLB (Corporate Law) / MBA',
    notes: 'CS via ICSI: Foundation → Executive → Professional. Mandatory training.',
  },

  'prof.cma': {
    primaryDegrees: [
      { name: 'Cost & Management Accountancy (ICMAI)', shortName: 'CMA', duration: 4, type: 'Primary', streams: ['Commerce', 'PCM'] },
    ],
    afterDegree: 'CPA (US) / CIMA (UK)',
    notes: 'CMA India via ICMAI. Foundation → Intermediate → Final.',
  },

  'deg.bcom.acc': {
    primaryDegrees: [
      { name: 'B.Com (Accounting & Finance) / B.Com (Hons)', shortName: 'B.Com A&F', duration: 3, type: 'Primary', streams: ['Commerce'] },
    ],
    afterDegree: 'CA / CMA / M.Com',
    notes: 'SRCC, Loyola, Christ University top for B.Com Accounting.',
  },

  'deg.bcom.fin': {
    primaryDegrees: [
      { name: 'B.Com (Finance)', shortName: 'B.Com Finance', duration: 3, type: 'Primary', streams: ['Commerce'] },
      { name: 'BBA Finance', shortName: 'BBA Finance', duration: 3, type: 'Alternative', streams: ['Commerce'] },
    ],
    afterDegree: 'CFA / MBA Finance',
    notes: 'CFA Level 1 can be started in final year of B.Com.',
  },

  'deg.bba.gen': {
    primaryDegrees: [
      { name: 'Bachelor of Business Administration', shortName: 'BBA', duration: 3, type: 'Primary', streams: ['Commerce', 'PCM', 'Arts'] },
    ],
    afterDegree: 'MBA (CAT/GMAT/XAT)',
    notes: 'Symbiosis, Christ, Amity, NMIMS top for BBA. Pathway to IIM MBA.',
  },

  'deg.bba.mktg': {
    primaryDegrees: [
      { name: 'BBA Marketing', shortName: 'BBA Marketing', duration: 3, type: 'Primary', streams: ['Commerce', 'PCM', 'Arts'] },
    ],
    afterDegree: 'MBA Marketing / Digital Marketing Certifications',
    notes: 'Digital marketing skills (SEO, SEM, Meta Ads) essential alongside degree.',
  },

  'deg.bba.hr': {
    primaryDegrees: [
      { name: 'BBA Human Resource Management', shortName: 'BBA HRM', duration: 3, type: 'Primary', streams: ['Commerce', 'Arts'] },
    ],
    afterDegree: 'MBA HR (XLRI / TISS)',
    notes: 'TISS Mumbai runs a dedicated HRM & LR PG program.',
  },

  'deg.bba.fin': {
    primaryDegrees: [
      { name: 'BBA Finance', shortName: 'BBA Finance', duration: 3, type: 'Primary', streams: ['Commerce'] },
    ],
    afterDegree: 'CA / CFA / MBA Finance',
    notes: 'BBA Finance + CFA is a strong combo for investment and corporate finance roles.',
  },

  'deg.bba.ent': {
    primaryDegrees: [
      { name: 'BBA Entrepreneurship', shortName: 'BBA Entrepreneurship', duration: 3, type: 'Primary', streams: ['Commerce', 'PCM', 'Arts'] },
    ],
    afterDegree: 'MBA Entrepreneurship / Startup Incubator',
    notes: 'IIT, IIM startup cells provide early-stage entrepreneurship exposure.',
  },

  'deg.bba.llb': {
    primaryDegrees: [
      { name: 'BBA LLB (Integrated)', shortName: 'BBA LLB', duration: 5, type: 'Primary', streams: ['Commerce', 'Arts'] },
    ],
    afterDegree: 'LLM Corporate Law / Bar Council Enrolment',
    notes: 'CLAT exam required. NLSIU, NALSAR, NLU Delhi are top law schools.',
  },

  // ─── ARTS / LAW ─────────────────────────────────────────────────────────────

  'arts.law': {
    primaryDegrees: [
      { name: 'B.A. LLB (Integrated)', shortName: 'B.A. LLB', duration: 5, type: 'Primary', streams: ['Arts', 'Commerce', 'PCM'] },
      { name: 'LLB (After Graduation)', shortName: 'LLB', duration: 3, type: 'Alternative', streams: ['Any'] },
    ],
    afterDegree: 'LLM / Bar Council Enrolment',
    notes: 'CLAT for NLUs. AILET for NLU Delhi. 3-year LLB after any bachelor\'s.',
  },

  'deg.ba.llb': {
    primaryDegrees: [
      { name: 'B.A. LLB (Integrated 5-Year)', shortName: 'B.A. LLB', duration: 5, type: 'Primary', streams: ['Arts', 'Commerce', 'PCM'] },
    ],
    afterDegree: 'LLM / Practice at Bar Council',
    notes: 'CLAT required. NLSIU Bangalore is the top NLU in India.',
  },

  'arts.law.corporate': {
    primaryDegrees: [
      { name: 'B.A. LLB / BBA LLB (Integrated)', shortName: 'B.A./BBA LLB', duration: 5, type: 'Primary', streams: ['Arts', 'Commerce'] },
      { name: 'LLB (After Graduation)', shortName: 'LLB', duration: 3, type: 'Alternative', streams: ['Any'] },
    ],
    afterDegree: 'LLM Corporate Law / CS',
    notes: 'Corporate lawyers work in M&A, IPOs, contracts. Top law firms: AZB, Cyril Amarchand.',
  },

  'arts.law.cyber': {
    primaryDegrees: [
      { name: 'B.A. LLB (Integrated)', shortName: 'B.A. LLB', duration: 5, type: 'Primary', streams: ['Arts', 'Commerce', 'PCM'] },
      { name: 'B.Tech + LLB (Dual Degree)', shortName: 'B.Tech + LLB', duration: 6, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'LLM Cyber Law / PG Diploma Cyber Law (NALSAR)',
    notes: 'NALSAR Hyderabad & NLU Delhi offer specialized cyber law courses.',
  },

  'arts.law.criminal': {
    primaryDegrees: [
      { name: 'B.A. LLB (Integrated)', shortName: 'B.A. LLB', duration: 5, type: 'Primary', streams: ['Arts', 'Commerce', 'PCM'] },
    ],
    afterDegree: 'LLM Criminal Law / Public Prosecution',
    notes: 'Criminal lawyers practice in District Courts, High Courts, Supreme Court.',
  },

  'arts.law.ip': {
    primaryDegrees: [
      { name: 'B.A. LLB / B.Tech + LLB', shortName: 'B.A. LLB / B.Tech LLB', duration: 5, type: 'Primary', streams: ['PCM', 'Arts'] },
    ],
    afterDegree: 'LLM Intellectual Property Law',
    notes: 'IP law is booming in India with startup ecosystem and AI/software patents.',
  },

  'arts.law.const': {
    primaryDegrees: [
      { name: 'B.A. LLB (Integrated)', shortName: 'B.A. LLB', duration: 5, type: 'Primary', streams: ['Arts', 'Commerce'] },
    ],
    afterDegree: 'LLM Constitutional Law / Judicial Services Exam',
    notes: 'Constitutional lawyers argue fundamental rights cases in Supreme Court.',
  },

  // ─── ARTS / HUMANITIES / EDUCATION ─────────────────────────────────────────

  'arts.edu': {
    primaryDegrees: [
      { name: 'Bachelor of Education', shortName: 'B.Ed', duration: 2, type: 'Primary', streams: ['Any – After Bachelor\'s'] },
      { name: 'B.A. Education', shortName: 'B.A. Education', duration: 3, type: 'Alternative', streams: ['Arts'] },
      { name: 'Integrated B.Ed (4-Year)', shortName: 'Integrated B.Ed', duration: 4, type: 'Alternative', streams: ['Any'] },
    ],
    afterDegree: 'M.Ed / PhD Education / CTET Qualification',
    notes: 'CTET / State TET exams for government school teacher posts.',
  },

  'arts.education': {
    primaryDegrees: [
      { name: 'B.Ed (after any graduation)', shortName: 'B.Ed', duration: 2, type: 'Primary', streams: ['Any'] },
      { name: 'Integrated B.Ed (4-Year)', shortName: 'Integrated B.Ed', duration: 4, type: 'Alternative', streams: ['Any'] },
    ],
    afterDegree: 'M.Ed / NET-JRF for college teaching',
    notes: 'UGC NET for assistant professor positions. CTET for school teaching.',
  },

  'deg.ba.edu': {
    primaryDegrees: [
      { name: 'B.A. Education', shortName: 'B.A. Education', duration: 3, type: 'Primary', streams: ['Arts'] },
      { name: 'B.Ed (2-Year, after Graduation)', shortName: 'B.Ed', duration: 2, type: 'Alternative', streams: ['Any'] },
    ],
    afterDegree: 'M.Ed / Diploma in Special Education',
    notes: 'Special education, curriculum design, educational technology sub-fields.',
  },

  'deg.ba': {
    primaryDegrees: [
      { name: 'B.A. (Hons) – Various Humanities/Social Sciences', shortName: 'B.A. Hons', duration: 3, type: 'Primary', streams: ['Arts', 'Commerce'] },
    ],
    afterDegree: 'M.A. / MBA / Civil Services (UPSC)',
    notes: 'Humanities graduates pursue UPSC civil services, management, or specialization.',
  },

  'deg.ba.english': {
    primaryDegrees: [
      { name: 'B.A. English (Hons)', shortName: 'B.A. English', duration: 3, type: 'Primary', streams: ['Arts'] },
    ],
    afterDegree: 'M.A. English / Journalism / Content Marketing',
    notes: 'Strong writing base for content creation, publishing, academia.',
  },

  'deg.ba.history': {
    primaryDegrees: [
      { name: 'B.A. History (Hons)', shortName: 'B.A. History', duration: 3, type: 'Primary', streams: ['Arts'] },
    ],
    afterDegree: 'M.A. History / UPSC Civil Services / Museum Studies',
    notes: 'History graduates often pursue UPSC IAS, Heritage Management, Archaeology.',
  },

  'deg.ba.polsci': {
    primaryDegrees: [
      { name: 'B.A. Political Science (Hons)', shortName: 'B.A. PolSci', duration: 3, type: 'Primary', streams: ['Arts'] },
    ],
    afterDegree: 'M.A. Pol Science / UPSC / Law / Journalism',
    notes: 'Prepares for civil services, policy research, international relations.',
  },

  'deg.ba.socio': {
    primaryDegrees: [
      { name: 'B.A. Sociology (Hons)', shortName: 'B.A. Sociology', duration: 3, type: 'Primary', streams: ['Arts'] },
    ],
    afterDegree: 'M.A. Sociology / MSW / TISS PG',
    notes: 'Social workers, NGO professionals, policy analysts often come from sociology.',
  },

  'deg.ba.journalism': {
    primaryDegrees: [
      { name: 'B.A. Journalism & Mass Communication', shortName: 'BJMC', duration: 3, type: 'Primary', streams: ['Arts', 'Commerce'] },
      { name: 'B.Sc. Media Studies', shortName: 'B.Sc. Media', duration: 3, type: 'Alternative', streams: ['Arts'] },
    ],
    afterDegree: 'M.A. Journalism / PG Diploma Journalism (IIMC)',
    notes: 'IIMC Delhi, ACJ Chennai, Symbiosis top journalism schools in India.',
  },

  'deg.ba.psych': {
    primaryDegrees: [
      { name: 'B.A. Psychology (Hons)', shortName: 'B.A. Psychology', duration: 3, type: 'Primary', streams: ['Arts', 'PCB'] },
      { name: 'B.Sc. Psychology', shortName: 'B.Sc. Psychology', duration: 3, type: 'Alternative', streams: ['Arts', 'PCB'] },
    ],
    afterDegree: 'M.Sc. Clinical Psychology / M.Phil Psychology (RCI)',
    notes: 'RCI (Rehabilitation Council of India) regulates clinical psychology practice.',
  },

  'arts.psych': {
    primaryDegrees: [
      { name: 'B.A./B.Sc. Psychology (Hons)', shortName: 'BA/B.Sc. Psychology', duration: 3, type: 'Primary', streams: ['Arts', 'PCB'] },
    ],
    afterDegree: 'M.Sc. Psychology / M.Phil Clinical Psychology',
    notes: 'TISS, NIMHANS, Christ University top for psychology programs.',
  },

  'arts.psych.clinical': {
    primaryDegrees: [
      { name: 'B.Sc. / B.A. Psychology', shortName: 'B.Sc./B.A. Psychology', duration: 3, type: 'Primary', streams: ['Arts', 'PCB'] },
    ],
    afterDegree: 'M.Phil Clinical Psychology (2 yrs, RCI-approved)',
    notes: 'NIMHANS Bangalore, AIIMS are premier institutions for clinical psychology.',
  },

  'arts.psych.cognitive': {
    primaryDegrees: [
      { name: 'B.Sc. / B.A. Psychology', shortName: 'B.Sc./B.A. Psychology', duration: 3, type: 'Primary', streams: ['Arts', 'PCB'] },
    ],
    afterDegree: 'M.Sc. Cognitive Science / PhD Cognitive Neuroscience',
    notes: 'IITB, IITM, CDS Jadavpur offer cognitive science programs.',
  },

  'arts.psych.research': {
    primaryDegrees: [
      { name: 'B.Sc. Psychology', shortName: 'B.Sc. Psychology', duration: 3, type: 'Primary', streams: ['Arts', 'PCB'] },
      { name: 'B.Sc. Statistics', shortName: 'B.Sc. Stats', duration: 3, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Sc. Psychology / PhD Research Methods',
    notes: 'Research psychologists often pursue NET-JRF for academic positions.',
  },

  'arts.psych.applied': {
    primaryDegrees: [
      { name: 'B.A./B.Sc. Psychology', shortName: 'B.A. Psychology', duration: 3, type: 'Primary', streams: ['Arts', 'PCB'] },
    ],
    afterDegree: 'M.Sc. Applied Psychology / MBA Organizational Behavior',
    notes: 'Applied psychologists work in HR, UX Research, behavior change programs.',
  },

  'arts.sociol': {
    primaryDegrees: [
      { name: 'B.A. Sociology (Hons)', shortName: 'B.A. Sociology', duration: 3, type: 'Primary', streams: ['Arts'] },
    ],
    afterDegree: 'M.A. Sociology / MSW / TISS Social Sciences',
    notes: 'Delhi University, Presidency, TISS are top sociology programs.',
  },

  'arts.media': {
    primaryDegrees: [
      { name: 'B.A. Mass Communication / Journalism', shortName: 'BJMC', duration: 3, type: 'Primary', streams: ['Arts', 'Commerce'] },
      { name: 'B.Sc. Media Studies', shortName: 'B.Sc. Media', duration: 3, type: 'Alternative', streams: ['Arts'] },
    ],
    afterDegree: 'M.A. Mass Communication / PG Diploma (IIMC)',
    notes: 'IIMC Delhi, ACJ Chennai, Symbiosis Pune top mass comm institutions.',
  },

  'arts.media.film': {
    primaryDegrees: [
      { name: 'B.A. Film Studies', shortName: 'B.A. Film Studies', duration: 3, type: 'Primary', streams: ['Arts'] },
      { name: 'BFA in Direction / Cinematography', shortName: 'BFA Film', duration: 4, type: 'Primary', streams: ['Arts'] },
    ],
    afterDegree: 'MFA Film Direction / FTII Diploma',
    notes: 'FTII Pune and SRFTI Kolkata are the premier film institutes in India.',
  },

  'arts.media.jour': {
    primaryDegrees: [
      { name: 'B.A. Journalism & Mass Communication', shortName: 'BJMC', duration: 3, type: 'Primary', streams: ['Arts', 'Commerce'] },
    ],
    afterDegree: 'PG Diploma Journalism (IIMC) / M.A. Journalism',
    notes: 'Print, broadcast, and digital journalism tracks available.',
  },

  'arts.media.content': {
    primaryDegrees: [
      { name: 'B.A. English / Mass Communication', shortName: 'B.A. English / BJMC', duration: 3, type: 'Primary', streams: ['Arts'] },
      { name: 'BCA / B.Tech CSE', shortName: 'BCA / B.Tech CSE', duration: 3, type: 'Alternative', streams: ['PCM', 'Commerce'] },
    ],
    afterDegree: 'Digital Marketing Certifications / MBA Marketing',
    notes: 'Content creation covers copywriting, SEO, video, and social media.',
  },

  // ─── DESIGN / ARTS / CREATIVE ───────────────────────────────────────────────

  'arts.fine': {
    primaryDegrees: [
      { name: 'Bachelor of Fine Arts', shortName: 'BFA', duration: 4, type: 'Primary', streams: ['Arts', 'PCM'] },
      { name: 'B.A. Fine Arts', shortName: 'B.A. Fine Arts', duration: 3, type: 'Alternative', streams: ['Arts'] },
    ],
    afterDegree: 'MFA / NID PG Design',
    notes: 'BHU, MSU Baroda, NID are top fine arts institutions in India.',
  },

  'deg.bfa': {
    primaryDegrees: [
      { name: 'Bachelor of Fine Arts (BFA)', shortName: 'BFA', duration: 4, type: 'Primary', streams: ['Arts', 'PCM'] },
    ],
    afterDegree: 'MFA / NID / Srishti Post-Graduate Programs',
    notes: 'Specializations: Painting, Sculpture, Applied Arts, Printmaking.',
  },

  'deg.bdes': {
    primaryDegrees: [
      { name: 'Bachelor of Design (B.Des)', shortName: 'B.Des', duration: 4, type: 'Primary', streams: ['Arts', 'PCM'] },
    ],
    afterDegree: 'M.Des / MFA',
    notes: 'UCEED exam for IIT B.Des programs. NID entrance for NID.',
  },

  'deg.bdes.anim': {
    primaryDegrees: [
      { name: 'B.Des Animation', shortName: 'B.Des Animation', duration: 4, type: 'Primary', streams: ['Arts', 'PCM'] },
      { name: 'B.Sc. Animation & VFX', shortName: 'B.Sc. Animation', duration: 3, type: 'Alternative', streams: ['Arts', 'PCM'] },
    ],
    afterDegree: 'M.Des Animation / Industry Certifications (Maya, Blender)',
    notes: 'Arena Animation, MAAC institutes offer practical animation training.',
  },

  'deg.bdes.ui': {
    primaryDegrees: [
      { name: 'B.Des Interaction Design / UI-UX', shortName: 'B.Des UI-UX', duration: 4, type: 'Primary', streams: ['Arts', 'PCM'] },
      { name: 'B.Tech CSE + UX Bootcamp', shortName: 'B.Tech + UX', duration: 4, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Des HCI / Google UX Design Certificate',
    notes: 'UCEED for IIT Design; NID entrance for NID programs. Figma portfolio essential.',
  },

  'design': {
    primaryDegrees: [
      { name: 'Bachelor of Design (B.Des)', shortName: 'B.Des', duration: 4, type: 'Primary', streams: ['Arts', 'PCM'] },
    ],
    afterDegree: 'M.Des / NID Post-Graduate Diploma',
    notes: 'UCEED / NID DAT / NIFT entrance exams depending on college.',
  },

  'design.graphic': {
    primaryDegrees: [
      { name: 'B.Des Graphic Design / Communication Design', shortName: 'B.Des Graphic', duration: 4, type: 'Primary', streams: ['Arts', 'PCM'] },
      { name: 'BFA Applied Arts', shortName: 'BFA Applied Arts', duration: 4, type: 'Alternative', streams: ['Arts'] },
      { name: 'Diploma in Graphic Design', shortName: 'Diploma Graphic', duration: 2, type: 'Fast Track', streams: ['Arts', 'Any'] },
    ],
    afterDegree: 'M.Des Communication Design / NID PG',
    notes: 'Adobe Creative Suite mastery (Photoshop, Illustrator, InDesign) is non-negotiable.',
  },

  'design.ui_ux': {
    primaryDegrees: [
      { name: 'B.Des Interaction Design', shortName: 'B.Des Interaction', duration: 4, type: 'Primary', streams: ['Arts', 'PCM'] },
      { name: 'B.Tech CSE + UX Specialization', shortName: 'B.Tech + UX', duration: 4, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Des Human-Computer Interaction (HCI) / Google UX Certificate',
    notes: 'Figma, Adobe XD, user research, prototyping are core skills.',
  },

  'design.animation': {
    primaryDegrees: [
      { name: 'B.Des Animation', shortName: 'B.Des Animation', duration: 4, type: 'Primary', streams: ['Arts', 'PCM'] },
      { name: 'B.Sc. Animation & VFX', shortName: 'B.Sc. Animation', duration: 3, type: 'Alternative', streams: ['Arts'] },
    ],
    afterDegree: 'M.Des / Industry Certifications (Unreal, Unity, Blender)',
    notes: 'Game design, VFX, OTT platforms are major hirers of animation graduates.',
  },

  'design.film': {
    primaryDegrees: [
      { name: 'BFA Film / Cinematography', shortName: 'BFA Film', duration: 4, type: 'Primary', streams: ['Arts'] },
      { name: 'B.A. Film Studies', shortName: 'B.A. Film', duration: 3, type: 'Alternative', streams: ['Arts'] },
    ],
    afterDegree: 'FTII Diploma / MFA Film',
    notes: 'FTII Pune is nationally premier. JNU SIS offers film studies MA.',
  },

  'design.arch.urban': {
    primaryDegrees: [
      { name: 'B.Arch', shortName: 'B.Arch', duration: 5, type: 'Primary', streams: ['PCM', 'Arts'] },
      { name: 'B.Planning', shortName: 'B.Planning', duration: 4, type: 'Alternative', streams: ['PCM', 'Arts'] },
    ],
    afterDegree: 'M.Arch Urban Design / M.Planning',
    notes: 'NATA exam for B.Arch. SPA Delhi, CEPT Ahmedabad top for urban design.',
  },

  'design.interior': {
    primaryDegrees: [
      { name: 'B.Des Interior Design', shortName: 'B.Des Interior', duration: 4, type: 'Primary', streams: ['Arts', 'PCM'] },
      { name: 'B.Arch (with Interior focus)', shortName: 'B.Arch', duration: 5, type: 'Alternative', streams: ['PCM'] },
      { name: 'Diploma in Interior Design', shortName: 'Diploma Interior', duration: 3, type: 'Fast Track', streams: ['Arts', 'Any'] },
    ],
    afterDegree: 'M.Des Interior Architecture / NID PG',
    notes: 'AutoCAD, SketchUp, 3ds Max software proficiency is essential.',
  },

  'arts.des': {
    primaryDegrees: [
      { name: 'B.Des (Bachelor of Design)', shortName: 'B.Des', duration: 4, type: 'Primary', streams: ['Arts', 'PCM'] },
      { name: 'BFA', shortName: 'BFA', duration: 4, type: 'Alternative', streams: ['Arts'] },
    ],
    afterDegree: 'M.Des / NID PG Diploma',
    notes: 'UCEED for IIT Design; NID-DAT for NID. Strong portfolio essential.',
  },

  'arts.des.anim': {
    primaryDegrees: [
      { name: 'B.Des Animation', shortName: 'B.Des Animation', duration: 4, type: 'Primary', streams: ['Arts', 'PCM'] },
      { name: 'B.Sc. Animation & VFX', shortName: 'B.Sc. Animation', duration: 3, type: 'Alternative', streams: ['Arts'] },
    ],
    afterDegree: 'M.Des / 3D Animation Certifications',
    notes: 'Maya, Blender, After Effects are key tools for animation careers.',
  },

  'arts.des.fashion': {
    primaryDegrees: [
      { name: 'B.Des Fashion Design', shortName: 'B.Des Fashion', duration: 4, type: 'Primary', streams: ['Arts', 'PCM'] },
      { name: 'B.Sc. Fashion Technology', shortName: 'B.Sc. Fashion Tech', duration: 3, type: 'Alternative', streams: ['Arts', 'PCM'] },
    ],
    afterDegree: 'M.Des Fashion / MBA Fashion Management',
    notes: 'NIFT entrance exam. NIFT is the top fashion design institution in India.',
  },

  'arts.des.ui': {
    primaryDegrees: [
      { name: 'B.Des Interaction Design / UI Design', shortName: 'B.Des UI', duration: 4, type: 'Primary', streams: ['Arts', 'PCM'] },
    ],
    afterDegree: 'M.Des HCI / Google UX Certificate',
    notes: 'UCEED for IIT; NID entrance for NID UI programs.',
  },

  // ─── GOVERNMENT / CIVIL SERVICES ────────────────────────────────────────────

  'gov_admin': {
    primaryDegrees: [
      { name: 'Bachelor\'s Degree (Any Stream)', shortName: 'Any Bachelor\'s', duration: 3, type: 'Primary', streams: ['Any'] },
      { name: 'B.A. Political Science / History / Sociology', shortName: 'B.A. Hons', duration: 3, type: 'Recommended', streams: ['Arts'] },
    ],
    afterDegree: 'UPSC CSE (IAS/IPS/IFS) / State PCS',
    notes: 'UPSC Civil Services Exam. Any bachelor\'s eligible to appear.',
  },

  'gov_admin_upsc': {
    primaryDegrees: [
      { name: 'Any Bachelor\'s Degree', shortName: 'Any Bachelor\'s', duration: 3, type: 'Primary', streams: ['Any'] },
      { name: 'B.A. Political Science / History', shortName: 'B.A. Hons', duration: 3, type: 'Recommended', streams: ['Arts'] },
    ],
    afterDegree: 'UPSC Prelims → Mains → Interview',
    notes: 'Most IAS/IPS officers have humanities or engineering backgrounds.',
  },

  'gov_admin_state': {
    primaryDegrees: [
      { name: 'Any Bachelor\'s Degree', shortName: 'Any Bachelor\'s', duration: 3, type: 'Primary', streams: ['Any'] },
    ],
    afterDegree: 'State PCS / MPSC / UPPSC / TNPSC Exams',
    notes: 'State-specific exams recruit for Group A/B/C administrative posts.',
  },

  'gov_admin_other': {
    primaryDegrees: [
      { name: 'Any Bachelor\'s Degree', shortName: 'Any Bachelor\'s', duration: 3, type: 'Primary', streams: ['Any'] },
    ],
    afterDegree: 'SSC CGL / Railways / IBPS Bank Exams',
    notes: 'Central government roles through SSC, railway recruitment boards.',
  },

  'gov.admin': {
    primaryDegrees: [
      { name: 'Any Bachelor\'s Degree', shortName: 'Any Bachelor\'s', duration: 3, type: 'Primary', streams: ['Any'] },
    ],
    afterDegree: 'UPSC / State PCS / SSC CGL',
    notes: 'Good General Studies foundation is essential for government competitive exams.',
  },

  'govt.banking': {
    primaryDegrees: [
      { name: 'B.Com / BBA / B.A. (Any)', shortName: 'Any Bachelor\'s', duration: 3, type: 'Primary', streams: ['Any'] },
    ],
    afterDegree: 'IBPS PO / SBI PO / RBI Grade B Exam',
    notes: 'IBPS conducts banking recruitment exams for nationalized banks.',
  },

  'govt.railways': {
    primaryDegrees: [
      { name: 'Any Bachelor\'s / Diploma / ITI', shortName: 'Bachelor\'s / Diploma', duration: 3, type: 'Primary', streams: ['Any'] },
    ],
    afterDegree: 'RRB NTPC / JE / ALP Exams',
    notes: 'Railway Recruitment Boards conduct tiered exams for various posts.',
  },

  'govt.upsc': {
    primaryDegrees: [
      { name: 'Any Bachelor\'s Degree', shortName: 'Any Bachelor\'s', duration: 3, type: 'Primary', streams: ['Any'] },
    ],
    afterDegree: 'UPSC Civil Services Exam (IAS/IPS/IFS)',
    notes: 'Most IAS officers spend 1-2 years dedicated preparation post-graduation.',
  },

  'govt.teaching_govt': {
    primaryDegrees: [
      { name: 'B.Ed (2-Year, after Graduation)', shortName: 'B.Ed', duration: 2, type: 'Primary', streams: ['Any – After Bachelor\'s'] },
      { name: 'Integrated B.Ed (4-Year)', shortName: 'Integrated B.Ed', duration: 4, type: 'Alternative', streams: ['Any'] },
    ],
    afterDegree: 'CTET / TET / NET-JRF for Higher Education',
    notes: 'KVS, NVS, state teaching recruitment require CTET/STET qualification.',
  },

  'govt.ssc': {
    primaryDegrees: [
      { name: 'Any Bachelor\'s Degree', shortName: 'Any Bachelor\'s', duration: 3, type: 'Primary', streams: ['Any'] },
    ],
    afterDegree: 'SSC CGL / SSC CHSL / SSC MTS Exams',
    notes: 'Staff Selection Commission exams recruit for central government department roles.',
  },

  // ─── HOSPITALITY / AVIATION ─────────────────────────────────────────────────

  'hosp.aviation': {
    primaryDegrees: [
      { name: 'B.Sc. Aviation / Airline Management', shortName: 'B.Sc. Aviation', duration: 3, type: 'Primary', streams: ['PCM'] },
      { name: 'Diploma in Aviation Hospitality', shortName: 'Diploma Aviation', duration: 1, type: 'Fast Track', streams: ['Any'] },
    ],
    afterDegree: 'Commercial Pilot License (CPL) / IATA Certifications',
    notes: 'DGCA regulates pilot licenses. IndiGo, Air India ab-initio cadet programs.',
  },

  'hosp.hotel_mgmt': {
    primaryDegrees: [
      { name: 'Bachelor of Hotel Management (BHM)', shortName: 'BHM', duration: 3, type: 'Primary', streams: ['Any'] },
      { name: 'B.Sc. Hospitality & Hotel Administration', shortName: 'B.Sc. Hospitality', duration: 3, type: 'Alternative', streams: ['Any'] },
    ],
    afterDegree: 'MBA Hotel Management / Cornell School of Hotel Administration (online)',
    notes: 'NCHMCT JEE for IHMs. Taj Group, Marriott, Hyatt are top employers.',
  },

  // ─── VOCATIONAL / ITI / DIPLOMA ─────────────────────────────────────────────

  'stream.iti': {
    primaryDegrees: [
      { name: 'ITI (Industrial Training Institute) Certificate', shortName: 'ITI', duration: 2, type: 'Primary', streams: ['Any (after Class 10)'] },
    ],
    afterDegree: 'Diploma in Engineering / AISECT Apprenticeship',
    notes: 'Electrician, Welder, Fitter, Mechanic are popular trades. Direct industry employment.',
  },

  'stream.diploma': {
    primaryDegrees: [
      { name: 'Diploma in Engineering (Polytechnic)', shortName: 'Diploma Engg', duration: 3, type: 'Primary', streams: ['PCM (after Class 10 or 12)'] },
    ],
    afterDegree: 'Lateral Entry to B.Tech (2nd year)',
    notes: 'Diploma holders can enter B.Tech directly in 2nd year via lateral entry.',
  },

  'voc.diploma': {
    primaryDegrees: [
      { name: 'Diploma in Vocational / Technical Field', shortName: 'Vocational Diploma', duration: 2, type: 'Primary', streams: ['Any'] },
    ],
    afterDegree: 'Advanced Diploma / B.Tech Lateral Entry',
    notes: 'NIOS and state boards offer vocational diplomas in various trades.',
  },

  'voc.iti': {
    primaryDegrees: [
      { name: 'ITI Certificate (1-2 years)', shortName: 'ITI', duration: 2, type: 'Primary', streams: ['Any (after Class 10)'] },
    ],
    afterDegree: 'Apprenticeship / Diploma Engineering',
    notes: 'NCVT affiliation ensures national recognition. COPA, Auto Electrician popular trades.',
  },

  // ─── STREAMS / BROAD CATEGORIES ─────────────────────────────────────────────

  'stream.pcm': {
    primaryDegrees: [
      { name: 'B.Tech / B.E. (Any Engineering Branch)', shortName: 'B.Tech', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Physics / Maths / Computer Science', shortName: 'B.Sc.', duration: 3, type: 'Alternative', streams: ['PCM'] },
      { name: 'B.Sc. Data Science', shortName: 'B.Sc. Data Science', duration: 3, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'M.Tech / M.Sc. / MBA',
    notes: 'PCM students have widest engineering + science options.',
  },

  'stream.pcb': {
    primaryDegrees: [
      { name: 'MBBS', shortName: 'MBBS', duration: 5.5, type: 'Primary', streams: ['PCB'] },
      { name: 'B.Sc. Nursing', shortName: 'B.Sc. Nursing', duration: 4, type: 'Alternative', streams: ['PCB'] },
      { name: 'B.Pharm', shortName: 'B.Pharm', duration: 4, type: 'Alternative', streams: ['PCB'] },
      { name: 'B.Sc. Allied Health Sciences', shortName: 'B.Sc. AHS', duration: 3, type: 'Alternative', streams: ['PCB'] },
    ],
    afterDegree: 'MD / M.Sc. / M.Pharm',
    notes: 'NEET-UG required for medical programs. Largest scope in healthcare sector.',
  },

  'stream.pcmb': {
    primaryDegrees: [
      { name: 'MBBS / B.Tech (Biomedical / Biotech)', shortName: 'MBBS / B.Tech Biotech', duration: 5.5, type: 'Primary', streams: ['PCMB'] },
      { name: 'B.Sc. Bioinformatics', shortName: 'B.Sc. Bioinformatics', duration: 3, type: 'Alternative', streams: ['PCMB'] },
    ],
    afterDegree: 'M.Sc. Bioinformatics / PhD Life Sciences',
    notes: 'PCMB students have the widest flexibility to choose medical or engineering.',
  },

  'stream.arts': {
    primaryDegrees: [
      { name: 'B.A. (Hons) Humanities / Social Sciences', shortName: 'B.A. Hons', duration: 3, type: 'Primary', streams: ['Arts'] },
      { name: 'B.A. LLB (Law)', shortName: 'B.A. LLB', duration: 5, type: 'Alternative', streams: ['Arts'] },
    ],
    afterDegree: 'M.A. / UPSC / LLM / MBA',
    notes: 'Arts students can pursue UPSC civil services, law, journalism, education.',
  },

  'stream.commerce': {
    primaryDegrees: [
      { name: 'B.Com (Hons)', shortName: 'B.Com Hons', duration: 3, type: 'Primary', streams: ['Commerce'] },
      { name: 'BBA', shortName: 'BBA', duration: 3, type: 'Alternative', streams: ['Commerce'] },
      { name: 'CA Foundation', shortName: 'CA', duration: 5, type: 'Alternative', streams: ['Commerce'] },
    ],
    afterDegree: 'CA / CFA / MBA Finance / CS',
    notes: 'Commerce stream opens finance, accounting, management, entrepreneurship paths.',
  },

  // ─── OTHER SPECIFIC CAREERS ──────────────────────────────────────────────────

  'deg.bca': {
    primaryDegrees: [
      { name: 'Bachelor of Computer Applications', shortName: 'BCA', duration: 3, type: 'Primary', streams: ['PCM', 'Commerce', 'Arts (with Maths)'] },
    ],
    afterDegree: 'MCA / M.Tech / MBA-Tech',
    notes: 'BCA is a strong alternative to B.Tech for software development careers.',
  },

  'deg.bsc.data': {
    primaryDegrees: [
      { name: 'B.Sc. Data Science', shortName: 'B.Sc. Data Science', duration: 3, type: 'Primary', streams: ['PCM'] },
    ],
    afterDegree: 'M.Sc. Data Science / MBA Analytics',
    notes: 'IIT Madras online B.Sc. Data Science is highly recognized.',
  },

  'afterpg.adv_dip.tech': {
    primaryDegrees: [
      { name: 'B.Tech Computer Science / IT', shortName: 'B.Tech CSE/IT', duration: 4, type: 'Primary', streams: ['PCM'] },
      { name: 'B.Sc. Computer Science', shortName: 'B.Sc. CS', duration: 3, type: 'Alternative', streams: ['PCM'] },
    ],
    afterDegree: 'Advanced Diploma / PG Diploma (C-DAC, NIIT) + Industry Certifications',
    notes: 'C-DAC ACTS Advanced Computing programs are nationally recognized.',
  },

  // Fallback for any unmapped career
  '_default': {
    primaryDegrees: [
      { name: 'Bachelor\'s Degree (Stream-Relevant)', shortName: 'Bachelor\'s Degree', duration: 3, type: 'Primary', streams: ['Any'] },
    ],
    afterDegree: 'Postgraduate Degree / Professional Certification',
    notes: 'Education pathway depends on specific specialization and career goals.',
  },
};

/**
 * Get education data for a career nodeId
 * Falls back to stream detection → generic default
 */
function buildPostgraduatePathway(context) {
  const value = String(context || '').toLowerCase();
  let degree = 'PhD in the relevant field or a closely related research specialization';
  let next = 'Pursue research, teaching, or senior specialist roles related to your quiz match';
  let note = 'A doctorate is most useful for research and academic roles; many industry careers offer specialist roles without one.';

  if (/pharm|pharmacy|pharmaceutical|drug/.test(value)) {
    degree = 'PhD in Pharmaceutical Sciences, Pharmaceutics, Pharmacology, or Drug Research';
    next = 'Apply for pharmaceutical R&D, clinical research, regulatory affairs, or academic research roles';
    note = 'Choose a PhD specialization aligned with your M.Pharm focus; it is optional for many industry roles.';
  } else if (/medical research|clinical research|biomedical research/.test(value)) {
    degree = 'PhD in Clinical Research, Biomedical Sciences, or Medical Sciences';
    next = 'Pursue research scientist, clinical trials, pharmacovigilance, or academic research roles';
    note = 'Select a doctoral program aligned with your prior qualification and intended research area.';
  } else if (/medical|medicine|mbbs|clinical|nurs|dent|health|biomed|allied/.test(value)) {
    degree = 'PhD / DM / M.Ch. in a specialization aligned with your clinical qualification';
    next = 'Complete the required specialty training and registration, then pursue clinical practice, research, or teaching';
    note = 'The appropriate route depends on your qualifying degree and the clinical specialty; verify regulator and institution requirements.';
  } else if (/computer|software|artificial intelligence|\bai\b|machine learning|data|cyber|engineering|\btech\b/.test(value)) {
    degree = 'PhD in the matched engineering or computing specialization';
    next = 'Target specialist industry roles, R&D positions, or doctoral research in your quiz-matched area';
  } else if (/architecture|urban|planning|design|fashion/.test(value)) {
    degree = 'PhD in the matched design, architecture, or planning specialization';
    next = 'Build a specialist portfolio and pursue advanced practice, research, or teaching roles';
  } else if (/law|legal|justice/.test(value)) {
    degree = 'PhD in Law or a related legal research specialization';
    next = 'Pursue specialist legal practice, judicial or policy work, or legal research and teaching';
  } else if (/psych|counsel|social work/.test(value)) {
    degree = 'PhD in Psychology or the matched behavioral science specialization';
    next = 'Pursue supervised specialist practice, research, or academic roles, following applicable registration rules';
  } else if (/business|management|finance|account|commerce|econom/.test(value)) {
    degree = 'PhD / DBA in the matched business, finance, or economics specialization';
    next = 'Pursue senior specialist, consulting, leadership, or research roles aligned with your quiz match';
  } else if (/science|research|biology|chemistry|physics|math|agri|environment/.test(value)) {
    degree = 'PhD in the matched scientific or research specialization';
    next = 'Pursue research, laboratory, academic, or specialist industry roles in your field';
  } else if (/education|teacher|academic/.test(value)) {
    degree = 'PhD in Education or your teaching subject specialization';
    next = 'Pursue senior teaching, academic leadership, curriculum, or education research roles';
  } else if (/media|journal|communication|content|arts/.test(value)) {
    degree = 'PhD in the matched media, communication, or arts specialization (optional)';
    next = 'Build a professional portfolio and pursue senior practice, research, or teaching roles';
  }

  return {
    primaryDegrees: [{ name: degree, shortName: degree.split(' in ')[0], duration: 4, type: 'Primary', streams: [] }],
    afterDegree: next,
    notes: note,
  };
}

function normalizeEducationLabel(value) {
  return String(value || '').toLowerCase().replace(/[^a-z0-9]/g, '');
}

function parseSelectedEducationFields(value) {
  if (Array.isArray(value)) return value.filter(item => typeof item === 'string' && item.trim());
  if (typeof value !== 'string' || !value.trim()) return [];
  try {
    const parsed = JSON.parse(value);
    if (Array.isArray(parsed)) return parsed.filter(item => typeof item === 'string' && item.trim());
  } catch { /* Older clients may send comma-separated values. */ }
  return value.split(',').map(item => item.trim()).filter(Boolean);
}

function buildSelectedFieldEducation(field) {
  const label = String(field || '').trim();
  const normalized = label.toLowerCase();
  let qualification;
  let nextStep;
  let note;

  if (/automobile|automotive|vehicle|electric vehicle|\bev\b/.test(normalized)) {
    qualification = 'M.Tech in Automotive Engineering, Electric Vehicles, or Mobility Systems';
    nextStep = 'Build vehicle design, powertrain, EV, or mobility project experience';
  } else if (/aerospace|aeronautical|aviation|avionics/.test(normalized)) {
    qualification = 'M.Tech in Aerospace / Aeronautical Engineering or a related specialization';
    nextStep = 'Choose a focus such as aerodynamics, propulsion, avionics, or structures';
  } else if (/mechatronic|robot|automation/.test(normalized)) {
    qualification = 'M.Tech in Mechatronics, Robotics, or Automation';
    nextStep = 'Develop practical experience with controls, embedded systems, and robotics projects';
  } else if (/thermal|hvac|energy|power/.test(normalized)) {
    qualification = 'M.Tech in Thermal, Energy, or Power Engineering';
    nextStep = 'Build applied experience in thermal systems, energy, or power applications';
  } else if (/mechanical|manufactur|production|design engineering/.test(normalized)) {
    qualification = 'M.Tech in Mechanical, Manufacturing, or Design Engineering';
    nextStep = 'Select a specialization that matches the engineering work you want to pursue';
  } else if (/civil|structural|geotechnical|transport|construction|urban planning/.test(normalized)) {
    qualification = 'M.Tech / M.Plan in the relevant civil engineering or planning specialization';
    nextStep = 'Build project experience in the relevant infrastructure or planning area';
  } else if (/computer|software|artificial intelligence|machine learning|data|cloud|devops|cyber|network|it management/.test(normalized)) {
    qualification = `M.Tech / M.Sc. in ${label} or a closely related computing specialization`;
    nextStep = 'Build a portfolio, complete an applied project, and gain relevant industry experience';
  } else if (/finance|bank|risk|investment|account|chartered account|company secretary|cost & management/.test(normalized)) {
    qualification = 'MBA / M.Com in the relevant finance specialization, or the corresponding professional qualification';
    nextStep = 'Choose a recognized program or professional route that matches your intended finance role';
  } else if (/management|marketing|human resources|operations|consult|business/.test(normalized)) {
    qualification = `MBA / PGDM in ${label}`;
    nextStep = 'Compare program specializations, placement outcomes, and eligibility requirements';
  } else if (/psycholog|counsell|behavioral research/.test(normalized)) {
    qualification = `M.A. / M.Sc. in ${label} or a related Psychology specialization`;
    nextStep = 'Check subject prerequisites and supervised practice requirements for your intended role';
  } else if (/law|legal/.test(normalized)) {
    qualification = `LL.M. in ${label.replace(/\blaw\b/i, '').trim() || 'your chosen legal specialization'}`;
    nextStep = 'Check law-program eligibility and the practice requirements for your intended role';
  } else if (/design|fashion|graphic|visual|interior|arts administration|creative/.test(normalized)) {
    qualification = `M.Des / M.F.A. in ${label} or a closely related design specialization`;
    nextStep = 'Prepare a portfolio and compare studio-based postgraduate programs';
  } else if (/medicine|surgery|clinical|pharmacy|health/.test(normalized)) {
    qualification = `A recognized postgraduate qualification in ${label}`;
    nextStep = 'Confirm the required entrance examination, registration, and supervised clinical training';
  } else if (/research|science|biology|physics|chemistry|biotechnology|environment/.test(normalized)) {
    qualification = `M.Sc. / M.Tech in ${label} or a closely related research specialization`;
    nextStep = 'Look for a program with relevant laboratory, field, or research-project experience';
  } else if (/teaching|academia|education|curriculum/.test(normalized)) {
    qualification = `M.A. / M.Sc. in ${label} or a relevant Education specialization`;
    nextStep = 'Check the subject-specific teaching qualification and eligibility requirements';
  } else if (/social work|community|public policy|civil services|journal|media|content|language|translation|history|geograph|econom|political|sociolog|humanit|research|publishing|communication|ngo|heritage|museum|ethic/.test(normalized)) {
    qualification = `M.A. / M.S.W. / postgraduate qualification in ${label}`;
    nextStep = 'Choose a postgraduate program with practical projects or field experience in this area';
  } else {
    qualification = `Postgraduate qualification in ${label}`;
    nextStep = 'Compare recognized programs in this field and confirm their entry requirements and specialization options';
  }

  note = `This is a general pathway for ${label}. Check each institution’s current admission criteria, accreditation, and professional requirements.`;
  return {
    primaryDegrees: [{ name: qualification, shortName: 'Postgraduate specialization', duration: 2, type: 'Primary', streams: [] }],
    afterDegree: nextStep,
    notes: note,
  };
}

export function getCareerEducation(nodeId, careerName = '', phaseKey = '', profile = {}) {
  const normalizedId = String(nodeId || '').toLowerCase();
  const normalizedLookupId = normalizedId.replace(/\|.*$/, '');
  const normalizedName = String(careerName || '').toLowerCase().trim();
  const normalizedPhase = String(phaseKey || '').toLowerCase().replace(/[_\s-]/g, '');
  const profileContext = [
    profile?.postGraduationDegree,
    profile?.postGraduationMajor,
    profile?.postGraduationSpecialization,
    profile?.previousDegree,
    profile?.selectedFields,
  ].filter(Boolean).join(' ');
  const normalizedProfileStage = String(profile?.educationLevel || '').toLowerCase().replace(/[_\s-]/g, '');
  const effectivePhase = normalizedPhase || normalizedProfileStage;
  const isAfterGraduation = effectivePhase.includes('aftergraduation') || effectivePhase.includes('afterpostgraduation');
  const isAfterPostGraduation = effectivePhase.includes('afterpostgraduation') || effectivePhase.includes('postgraduation');
  const careerContext = [profileContext, normalizedId, normalizedName].join(' ');

  // After-graduation medical results describe specialty choices for an MBBS
  // graduate, not a new undergraduate medical degree. Keep these paths explicit
  // because several scorer result IDs are specialty traits outside the catalog.
  if (isAfterGraduation && !isAfterPostGraduation && /medical|medicine|mbbs|physician|doctor|anesthes|anaesthes|radiolog|patholog|cardio|pediatr|paediatr|emergency|surgeon|psychiatr/.test(careerContext)) {
    const specialty = /emergency/.test(careerContext) ? 'MD Emergency Medicine' :
      /anesthes|anaesthes/.test(careerContext) ? 'MD Anaesthesiology' :
      /radiolog/.test(careerContext) ? 'MD Radiodiagnosis / Radiology' :
      /patholog/.test(careerContext) ? 'MD Pathology' :
      /cardio/.test(careerContext) ? 'MD General Medicine, followed by DM Cardiology' :
      /pediatr|paediatr/.test(careerContext) ? 'MD Pediatrics' :
      /psychiatr/.test(careerContext) ? 'MD Psychiatry' :
      /surgeon|surgery/.test(careerContext) ? 'MS General Surgery' :
      'MD / MS in a clinical specialty aligned with your interests';
    const isSuperSpecialty = /cardio/.test(careerContext);
    return {
      primaryDegrees: [{ name: specialty, shortName: specialty.split(',')[0], duration: 3, type: 'Primary', streams: [] }],
      afterDegree: isSuperSpecialty
        ? 'After MD General Medicine, pursue DM Cardiology through the applicable super-specialty entrance and training route'
        : 'After MBBS and internship, check current NEET-PG / NMC requirements for MD or MS admission; pursue DM / M.Ch. later if your specialty requires it',
      notes: 'This pathway assumes you have completed or are completing MBBS. Specialty seats, accepted entrance routes, and registration rules can change; confirm current requirements with NMC, NBEMS, and the admitting institution.',
    };
  }

  // Selected-field results can use scorer-generated IDs (for example,
  // "selected.automobileengineering") that do not exist in the taxonomy map.
  // Build the education route from the learner's actual selection so every
  // selected field still gets a useful, field-specific recommendation.
  if (isAfterGraduation && String(profile?.preferenceMode || '').toLowerCase() === 'selected') {
    const selectedFields = parseSelectedEducationFields(profile?.selectedFields);
    const normalizedCareer = normalizeEducationLabel(careerName || normalizedId.split('.').pop());
    const selectedField = selectedFields.find(field => {
      const normalizedField = normalizeEducationLabel(field);
      return normalizedField && (normalizedCareer.includes(normalizedField) || normalizedField.includes(normalizedCareer));
    }) || selectedFields[0];
    if (selectedField) return buildSelectedFieldEducation(selectedField);
  }

  // The adaptive scorer can recommend the broad accounting-professional
  // cluster (com.acc.prof), which is not a leaf in the degree pathway map.
  // Give it a meaningful set of professional routes instead of the empty card.
  if (normalizedLookupId === 'com.acc.prof' || normalizedLookupId === 'com.acc' || normalizedLookupId === 'com.acco') {
    if (isAfterPostGraduation) return buildPostgraduatePathway(`accounting professional ${careerContext}`);
    return {
      primaryDegrees: [{
        name: isAfterGraduation
          ? 'CA (ICAI), CMA (ICMAI), or Company Secretary (ICSI) qualification, based on your preferred specialization'
          : 'B.Com in Accounting and Finance, with the option to pursue CA (ICAI), CMA (ICMAI), or CS (ICSI)',
        shortName: isAfterGraduation ? 'Accounting qualification' : 'B.Com + professional qualification',
        duration: isAfterGraduation ? null : 3,
        type: 'Primary',
        streams: [],
      }],
      afterDegree: isAfterGraduation
        ? 'Choose the relevant professional body and verify its current graduate-entry, exam, and practical-training requirements before enrolling'
        : 'Build accounting experience through internships or articleship and check the current eligibility rules of the professional body you choose',
      notes: 'The best route depends on whether you prefer audit and tax, cost accounting, or corporate governance; eligibility and completion time vary by qualification.',
    };
  }

  // Role IDs emitted by the BA and social-science quiz banks are not catalog
  // taxonomy IDs. Give them explicit, BA-relevant routes instead of showing
  // the empty "verified pathway unavailable" state on the result screen.
  const rolePathways = {
    role_social_worker: {
      undergraduate: { degree: 'Bachelor of Social Work (BSW)', shortName: 'BSW', duration: 3, next: 'Complete supervised fieldwork with an NGO, hospital, community programme, or social-service organization', note: 'For a BA graduate, an MSW is a common route into professional social-work roles; check each institution’s eligibility and fieldwork requirements.' },
      postgraduate: { degree: 'Master of Social Work (MSW) / M.A. Social Work', shortName: 'MSW', duration: 2, next: 'Build supervised practice experience and specialize in community, medical, school, or family social work', note: 'Confirm admission eligibility and the required supervised field placement with the institution and employer.' },
    },
    role_community_development_officer: {
      undergraduate: { degree: 'B.A. in Social Work, Sociology, or Development Studies', shortName: 'BA Social Sciences', duration: 3, next: 'Gain field experience through community projects, internships, or local development organizations', note: 'A BA in a related social-science subject can lead to entry-level programme work; requirements vary by employer.' },
      postgraduate: { degree: 'M.A. Development Studies / Master of Social Work / M.A. Public Administration', shortName: 'MA Development Studies', duration: 2, next: 'Build experience in programme delivery, community mobilization, and monitoring and evaluation', note: 'Choose the specialization that matches the communities and programmes you want to work with.' },
    },
    role_career_counsellor: {
      undergraduate: { degree: 'B.A. Psychology or Education', shortName: 'BA Psychology / Education', duration: 3, next: 'Add supervised counselling exposure and training in assessment and career guidance', note: 'Counselling scope and credential expectations vary; verify training and supervision requirements before practice.' },
      postgraduate: { degree: 'M.A. Psychology / Education with a recognized career-guidance or counselling qualification', shortName: 'MA Psychology / Education', duration: 2, next: 'Complete supervised counselling practice and build experience with career assessments and guidance', note: 'Check eligibility, supervision, and professional requirements for your intended counselling role.' },
    },
    role_journalist: {
      undergraduate: { degree: 'B.A. Journalism and Mass Communication', shortName: 'BA Journalism', duration: 3, next: 'Build a published reporting portfolio through internships, campus media, or local news work', note: 'A strong reporting and writing portfolio is important alongside the degree.' },
      postgraduate: { degree: 'M.A. Journalism / Mass Communication or a Postgraduate Diploma in Journalism', shortName: 'MA Journalism', duration: 2, next: 'Develop a reporting specialization and publish work with a newsroom or independent outlet', note: 'Admissions and course duration vary by institution.' },
    },
    role_editor: {
      undergraduate: { degree: 'B.A. English, Journalism, or Mass Communication', shortName: 'BA English / Media', duration: 3, next: 'Build editing samples and experience with publishing, media, or communications teams', note: 'Editing roles value a strong language, fact-checking, and portfolio record.' },
      postgraduate: { degree: 'M.A. English / Journalism / Mass Communication or a publishing qualification', shortName: 'MA English / Journalism', duration: 2, next: 'Create an editing portfolio and specialize in a publication or content domain', note: 'Choose a program aligned with the kind of publishing work you want.' },
    },
    role_social_researcher: {
      undergraduate: { degree: 'B.A. Sociology, Psychology, Political Science, or Economics', shortName: 'BA Social Sciences', duration: 3, next: 'Gain research exposure through surveys, internships, and supervised undergraduate projects', note: 'Research roles benefit from training in research methods and ethical fieldwork.' },
      postgraduate: { degree: 'M.A. Sociology / Social Work / Development Studies with Research Methods', shortName: 'MA Social Research', duration: 2, next: 'Build a research portfolio using interviews, surveys, data analysis, and ethical field methods', note: 'Select methods training that matches the research roles you are targeting.' },
    },
    role_public_policy_analyst: {
      undergraduate: { degree: 'B.A. Political Science, Economics, Sociology, or Public Administration', shortName: 'BA Policy Fields', duration: 3, next: 'Build policy-writing samples and seek research or public-programme internships', note: 'Policy analyst roles often expect strong writing, research, and data interpretation.' },
      postgraduate: { degree: 'M.A. Public Policy / Development Studies / Political Science / Economics', shortName: 'MA Public Policy', duration: 2, next: 'Complete a policy research project or internship and build evidence-based writing samples', note: 'Compare curriculum, methods training, internships, and eligibility across institutions.' },
    },
    role_development_officer: {
      undergraduate: { degree: 'B.A. Social Work, Sociology, Political Science, or Development Studies', shortName: 'BA Development Studies', duration: 3, next: 'Gain field experience with a community programme, NGO, or development project', note: 'Entry requirements vary by organization and programme.' },
      postgraduate: { degree: 'M.A. Development Studies / Master of Social Work / M.A. Public Administration', shortName: 'MA Development Studies', duration: 2, next: 'Build programme-management and monitoring-and-evaluation experience', note: 'Match the postgraduate specialization to the development sector you want to enter.' },
    },
    role_programme_officer: {
      undergraduate: { degree: 'B.A. Social Work, Sociology, Public Administration, or Development Studies', shortName: 'BA Programme Fields', duration: 3, next: 'Build project coordination experience through internships or community programmes', note: 'Programme roles may also require sector-specific experience.' },
      postgraduate: { degree: 'M.A. Development Studies / Master of Social Work / M.A. Public Administration', shortName: 'MA Development Studies', duration: 2, next: 'Add project planning, budgeting, monitoring, and evaluation experience', note: 'Choose a program with practical project or fieldwork opportunities.' },
    },
    role_archivist: {
      undergraduate: { degree: 'B.A. History, Archaeology, or Library and Information Science', shortName: 'BA History / Archives', duration: 3, next: 'Gain collection-handling and digitization experience through a museum, archive, or library internship', note: 'Archives roles value careful documentation and preservation skills.' },
      postgraduate: { degree: 'M.A. History / Museology or a postgraduate qualification in Archives and Records Management', shortName: 'MA Archives / History', duration: 2, next: 'Build experience in cataloguing, preservation, and digital collections', note: 'Check that the program includes practical archival or collections training.' },
    },
    role_museum_educator: {
      undergraduate: { degree: 'B.A. History, Fine Arts, Archaeology, or Education', shortName: 'BA History / Arts', duration: 3, next: 'Create interpretation or teaching samples and seek museum or heritage volunteering', note: 'Museum education roles combine subject knowledge with public communication.' },
      postgraduate: { degree: 'M.A. Museology / Heritage Studies / History or Education', shortName: 'MA Museology', duration: 2, next: 'Build guided-tour, exhibit interpretation, or public-learning experience', note: 'Program options and entry criteria vary by institution.' },
    },
    role_translator: {
      undergraduate: { degree: 'B.A. in the relevant language, English, or Linguistics', shortName: 'BA Language / English', duration: 3, next: 'Build a translation portfolio and verify fluency in both working languages', note: 'Professional translation depends on language proficiency and subject accuracy as well as the degree.' },
      postgraduate: { degree: 'M.A. Translation Studies / Linguistics / the relevant language', shortName: 'MA Translation', duration: 2, next: 'Specialize in a subject area and build reviewed translation samples', note: 'Choose training that matches the language pair and sector you plan to serve.' },
    },
    role_economic_analyst: {
      undergraduate: { degree: 'B.A. Economics, Statistics, or Mathematics', shortName: 'BA Economics', duration: 3, next: 'Build quantitative skills and applied data-analysis projects', note: 'Check mathematics prerequisites for quantitative economics programs and roles.' },
      postgraduate: { degree: 'M.A. Economics / Applied Economics / Statistics', shortName: 'MA Economics', duration: 2, next: 'Develop econometrics, data analysis, and policy or industry research experience', note: 'Compare quantitative prerequisites and methods training across programs.' },
    },
    role_geospatial_analyst: {
      undergraduate: { degree: 'B.A. Geography with GIS, cartography, or spatial-analysis coursework', shortName: 'BA Geography / GIS', duration: 3, next: 'Create GIS map projects and gain experience with spatial data tools', note: 'Select coursework that includes practical GIS and remote-sensing work.' },
      postgraduate: { degree: 'M.A. Geography / Geoinformatics / GIS and Remote Sensing', shortName: 'MA GIS', duration: 2, next: 'Build a portfolio of spatial analyses and applied mapping projects', note: 'Program titles and eligibility vary; confirm hands-on GIS coverage.' },
    },
    role_urban_planner: {
      undergraduate: { degree: 'B.A. Geography, Sociology, Economics, or Political Science', shortName: 'BA Urban Studies Fields', duration: 3, next: 'Explore planning through GIS, urban research, and community-project experience', note: 'Professional planning roles may require a planning qualification with specific entry criteria.' },
      postgraduate: { degree: 'Master of Planning (M.Plan) / M.A. Urban Planning or Urban Studies', shortName: 'M.Plan / MA Urban Studies', duration: 2, next: 'Build planning studio, GIS, and urban-policy project experience', note: 'Verify each program’s accepted undergraduate subjects and professional eligibility.' },
    },
    role_psychology_researcher: {
      undergraduate: { degree: 'B.A. Psychology', shortName: 'BA Psychology', duration: 3, next: 'Gain supervised research experience and training in research ethics and methods', note: 'Psychology practice and research roles have different education and supervision requirements.' },
      postgraduate: { degree: 'M.A. Psychology with research-methods training', shortName: 'MA Psychology', duration: 2, next: 'Complete a supervised research project and build quantitative or qualitative methods skills', note: 'Check program eligibility against your BA subjects and intended role.' },
    },
    role_research_analyst: {
      undergraduate: { degree: 'B.A. Economics, Sociology, Political Science, Psychology, or a related social science', shortName: 'BA Social Sciences', duration: 3, next: 'Practice research design, evidence review, and clear report writing', note: 'Analyst roles vary by sector and may require additional quantitative training.' },
      postgraduate: { degree: 'M.A. in the relevant social-science subject with research-methods training', shortName: 'MA Research Methods', duration: 2, next: 'Build a portfolio of research briefs, surveys, or data-analysis projects', note: 'Choose methods training that matches the intended analyst role.' },
    },
    role_sociologist: {
      undergraduate: { degree: 'B.A. Sociology', shortName: 'BA Sociology', duration: 3, next: 'Gain experience with community research, surveys, or social-service projects', note: 'Research and policy roles benefit from strong methods and writing skills.' },
      postgraduate: { degree: 'M.A. Sociology / Social Research', shortName: 'MA Sociology', duration: 2, next: 'Build research-methods expertise and apply it through a dissertation or field project', note: 'Select a program aligned with your intended research or policy area.' },
    },
    role_public_administration_officer: {
      undergraduate: { degree: 'B.A. Political Science, Public Administration, Economics, or Sociology', shortName: 'BA Public Administration Fields', duration: 3, next: 'Build public-policy knowledge and prepare for the relevant recruitment process', note: 'Government hiring and eligibility depend on the specific post and jurisdiction.' },
      postgraduate: { degree: 'M.A. Public Administration / Public Policy / Political Science', shortName: 'MA Public Administration', duration: 2, next: 'Develop policy, programme-management, and public-sector research experience', note: 'A postgraduate degree does not replace post-specific recruitment or exam requirements.' },
    },
    role_content_strategist: {
      undergraduate: { degree: 'B.A. English, Journalism, Mass Communication, or Media Studies', shortName: 'BA English / Media', duration: 3, next: 'Build writing samples and practice audience research and content planning', note: 'A portfolio and communication experience are important for entry-level roles.' },
      postgraduate: { degree: 'M.A. Mass Communication / Media Studies / English', shortName: 'MA Media / Communication', duration: 2, next: 'Develop a strategy portfolio with editorial, audience, and campaign examples', note: 'Select programs with practical media and digital communication work.' },
    },
    role_instructional_designer: {
      undergraduate: { degree: 'B.A. Education, English, Psychology, or a subject you want to teach', shortName: 'BA Education Fields', duration: 3, next: 'Create sample learning materials and gain teaching or training experience', note: 'Teaching credentials depend on the role; instructional design employers may prioritize a portfolio.' },
      postgraduate: { degree: 'M.A. Education / M.Ed. / Learning Design qualification', shortName: 'MA Education / M.Ed.', duration: 2, next: 'Build a portfolio of course plans, assessments, and learner-centered materials', note: 'Check program prerequisites and the requirements of your target education role.' },
    },
    role_ethics_researcher: {
      undergraduate: { degree: 'B.A. Philosophy, Political Science, Sociology, or a related humanities subject', shortName: 'BA Philosophy / Humanities', duration: 3, next: 'Build argument-analysis and research-writing experience through projects or internships', note: 'The relevant specialization depends on whether you target research, policy, or applied ethics.' },
      postgraduate: { degree: 'M.A. Philosophy / Applied Ethics / Public Policy', shortName: 'MA Applied Ethics', duration: 2, next: 'Develop a focused research portfolio in an applied ethics area', note: 'Compare the program’s research focus and prerequisites with your intended field.' },
    },
    role_social_research_analyst: {
      undergraduate: { degree: 'B.A. Sociology, Psychology, Economics, or Political Science', shortName: 'BA Social Sciences', duration: 3, next: 'Practice ethical research, survey design, and clear evidence-based writing', note: 'Add quantitative or qualitative methods training based on the analyst roles you want.' },
      postgraduate: { degree: 'M.A. Social Research / Sociology / Development Studies with methods training', shortName: 'MA Social Research', duration: 2, next: 'Complete a supervised study and build a portfolio of research briefs or analysis', note: 'Check that the program covers the research methods required by target employers.' },
    },
    role_legal_career: {
      undergraduate: { degree: 'Three-year LL.B. after graduation', shortName: 'LL.B.', duration: 3, next: 'Complete the required legal education and enrolment steps before practicing law', note: 'Check current Bar Council of India recognition, entrance requirements, and enrolment rules for each institution and jurisdiction.' },
      postgraduate: { degree: 'LL.M. in the relevant legal specialization', shortName: 'LL.M.', duration: 2, next: 'Build specialist legal research or practice experience', note: 'An LL.M. is a specialization; confirm that your prior law qualification meets admission requirements.' },
    },
    role_marketing_career: {
      undergraduate: { degree: 'B.A. in a relevant subject plus a practical marketing or communications qualification', shortName: 'BA + Marketing', duration: 3, next: 'Build campaign, audience-research, and writing samples through internships or projects', note: 'A BA can be a foundation for marketing roles; employers often value a portfolio and practical experience.' },
      postgraduate: { degree: 'MBA / M.A. in Marketing, Communications, or a related specialization', shortName: 'MBA / MA Marketing', duration: 2, next: 'Develop campaign and audience-insight experience with measurable project outcomes', note: 'Compare admissions eligibility and practical experience across programs.' },
    },
    role_ui_ux_career: {
      undergraduate: { degree: 'B.Des in Interaction / Communication Design or a BA followed by structured UX training', shortName: 'B.Des / UX Training', duration: 3, next: 'Create a portfolio with user research, wireframes, and tested interface improvements', note: 'For BA graduates, entry depends on demonstrable design skills and a portfolio; degree prerequisites vary by program.' },
      postgraduate: { degree: 'M.Des in Interaction Design / Human-Computer Interaction or a related design qualification', shortName: 'M.Des / HCI', duration: 2, next: 'Build a portfolio of user research and tested design projects', note: 'Check accepted undergraduate backgrounds and portfolio requirements.' },
    },
    role_data_analyst: {
      undergraduate: { degree: 'B.A. Economics, Geography, Sociology, or another quantitative social-science subject plus analytics training', shortName: 'BA + Analytics', duration: 3, next: 'Build spreadsheet, statistics, and data-visualization projects using real datasets', note: 'A BA can support analyst roles when paired with quantitative skills and demonstrable projects.' },
      postgraduate: { degree: 'M.A. Applied Economics / Statistics / Data Analytics or a related quantitative qualification', shortName: 'MA Analytics', duration: 2, next: 'Complete applied analysis projects and show how evidence informs decisions', note: 'Confirm quantitative prerequisites before applying.' },
    },
  };
  const roleNameId = `role_${normalizedName.replace(/[^a-z0-9]+/g, '_').replace(/^_|_$/g, '')}`;
  const taxonomyRoleAliases = {
    'arts.journalism': 'role_journalist',
    'arts.psych': 'role_psychology_researcher',
    'arts.sociol': 'role_sociologist',
    'arts.poli_sci': 'role_public_policy_analyst',
    'arts.education': 'role_curriculum_developer',
    'arts.language': 'role_translator',
    'arts.law': 'role_legal_career',
    'com.marketing': 'role_marketing_career',
    'design.ui_ux': 'role_ui_ux_career',
    'tech.data.analytics': 'role_data_analyst',
  };
  const rolePathway = rolePathways[normalizedLookupId] || rolePathways[taxonomyRoleAliases[normalizedLookupId]] || rolePathways[roleNameId];
  if (rolePathway) {
    if (isAfterPostGraduation) return buildPostgraduatePathway(careerContext.replace(/[_-]/g, ' '));
    const pathway = isAfterGraduation ? rolePathway.postgraduate : rolePathway.undergraduate;
    return {
      primaryDegrees: [{ name: pathway.degree, shortName: pathway.shortName, duration: pathway.duration, type: 'Primary', streams: [] }],
      afterDegree: pathway.next,
      notes: pathway.note,
    };
  }

  // Medical research is returned by the scorer as a trait ID ("med.research")
  // rather than a catalog career node. Give it a real pathway for every quiz stage.
  if (normalizedId.startsWith('med.research') || normalizedName === 'research' || normalizedName.includes('clinical research')) {
    if (isAfterPostGraduation) return buildPostgraduatePathway(`medical research ${careerContext}`);
    if (isAfterGraduation) {
      return {
        primaryDegrees: [{ name: /pharm|pharmacy|pharmaceutical/i.test(profileContext) ? 'PhD or advanced specialization in Pharmaceutical / Clinical Research' : 'M.Sc. in Clinical Research, Biomedical Sciences, or a related field', shortName: 'Clinical Research', duration: 2, type: 'Primary', streams: [] }],
        afterDegree: 'Apply for clinical research associate, trial coordinator, pharmacovigilance, or research analyst roles',
        notes: 'Select a program recognized by relevant institutions and aligned with your prior degree.',
      };
    }
    return {
      primaryDegrees: [{ name: 'B.Sc. in Biomedical Sciences, Life Sciences, or a related research field', shortName: 'B.Sc. Research', duration: 3, type: 'Primary', streams: ['PCB', 'PCM'] }],
      afterDegree: 'Continue to an M.Sc. in Clinical Research / Biomedical Sciences or enter an eligible research support role',
      notes: 'Clinical research roles may require additional training and institution-specific eligibility.',
    };
  }

  // Quiz result labels can be short display names (for example, "AI") rather
  // than taxonomy IDs. Resolve those labels before falling back to an empty
  // pathway so the result card can still show relevant education.
  const careerAliases = [
    { match: /^(ai|artificial intelligence|machine learning|ml engineer|ai engineer)$/, id: 'sci.eng.cs.ai_ml' },
    { match: /^(data science|data scientist|data analyst)$/, id: 'sci.eng.cs.data' },
    { match: /^(software|software engineer|software developer|developer)$/, id: 'sci.eng.cs.software' },
    { match: /^(cyber ?security|cybersecurity|ethical hacker|penetration tester)$/, id: 'sci.eng.cs.cybersec' },
  ];
  const alias = careerAliases.find(({ match }) => match.test(normalizedName));
  const resolvedLookupId = CAREER_EDUCATION_MAP[normalizedLookupId]
    ? normalizedLookupId
    : alias?.id || normalizedLookupId;

  // The scorer also emits the legacy "des.fashion" ID. Keep both taxonomy
  // IDs and name-based results connected to the real fashion degree pathway.
  if (normalizedId.includes('fashion') || normalizedName.includes('fashion')) {
    if (isAfterPostGraduation) {
      return {
        primaryDegrees: [{ name: 'PhD in Fashion Design, Textile Studies, or Fashion Management', shortName: 'PhD Fashion Studies', duration: 4, type: 'Primary', streams: [] }],
        afterDegree: 'Research, teaching, or advanced fashion and textile innovation roles',
        notes: 'A doctorate is most relevant for research and academic careers; it is not required for most fashion industry roles.',
      };
    }
    if (isAfterGraduation) {
      return {
        primaryDegrees: [
          { name: 'M.Des Fashion Design / Textile Design', shortName: 'M.Des Fashion', duration: 2, type: 'Primary', streams: [] },
          { name: 'MBA Fashion Management', shortName: 'MBA Fashion Management', duration: 2, type: 'Alternative', streams: [] },
        ],
        afterDegree: 'Specialist design, merchandising, sourcing, or fashion business roles',
        notes: 'Choose the design or management route based on the work you want to do.',
      };
    }
    return CAREER_EDUCATION_MAP['arts.des.fashion'];
  }

  // The adaptive scorer can return a pharmacy specialization trait (for example,
  // pharm.research) rather than a taxonomy career node. Resolve that specialty
  // directly so the result card still gets a relevant next education step.
  if (normalizedId.startsWith('pharm.') || normalizedName.includes('pharmacy') || normalizedName.includes('pharmaceutical')) {
    if (isAfterPostGraduation) {
      return {
        primaryDegrees: [{ name: 'PhD in Pharmaceutical Sciences / Drug Research', shortName: 'PhD Pharmaceutical Sciences', duration: 4, type: 'Primary', streams: [] }],
        afterDegree: 'Postdoctoral research or industry R&D leadership',
        notes: 'Research pathways vary by specialization and institution.',
      };
    }
    if (isAfterGraduation) {
      return {
        primaryDegrees: [{ name: 'Master of Pharmacy (M.Pharm) – Pharmaceutics / Pharmacology / Pharmaceutical Chemistry', shortName: 'M.Pharm', duration: 2, type: 'Primary', streams: [] }],
        afterDegree: 'PhD in Pharmaceutical Sciences for research and academic roles',
        notes: 'Choose the M.Pharm specialization that matches the intended research area.',
      };
    }
    return CAREER_EDUCATION_MAP['sci.med.pharma'];
  }

  // Use the learner's declared postgraduate qualification when a quiz result
  // returns a broad label (for example, "Clinical Medicine" for an M.Pharm).
  if (isAfterPostGraduation && /pharm|pharmacy|pharmaceutical/.test(profileContext)) {
    return buildPostgraduatePathway(profileContext);
  }

  // Architecture quiz traits use IDs like arch.urban rather than taxonomy IDs.
  if (normalizedId.startsWith('arch.') || normalizedName.includes('architecture') || normalizedName.includes('urban design')) {
    if (isAfterPostGraduation) {
      return {
        primaryDegrees: [{ name: 'PhD in Architecture, Urban Planning, or Built Environment', shortName: 'PhD Built Environment', duration: 4, type: 'Primary', streams: [] }],
        afterDegree: 'Academic research, public-sector planning, or specialist practice',
        notes: 'Choose a research area aligned with your architecture specialization.',
      };
    }
    if (isAfterGraduation) {
      const urban = normalizedId.includes('urban') || normalizedName.includes('urban');
      return {
        primaryDegrees: [{ name: urban ? 'M.Arch in Urban Design / Master of Planning' : 'M.Arch in a related specialization', shortName: urban ? 'M.Arch Urban Design / M.Plan' : 'M.Arch', duration: 2, type: 'Primary', streams: [] }],
        afterDegree: 'Specialist urban design, planning, sustainable design, or architectural practice',
        notes: 'A postgraduate specialization can deepen your design focus; requirements vary by institution.',
      };
    }
    return CAREER_EDUCATION_MAP['design.arch.urban'];
  }
  
  // Direct lookup
  if (CAREER_EDUCATION_MAP[resolvedLookupId]) {
    const pathway = CAREER_EDUCATION_MAP[resolvedLookupId];
    if (isAfterPostGraduation) {
      return buildPostgraduatePathway(careerContext);
    }
    if (isAfterGraduation) {
      const career = normalizedName || resolvedLookupId.split('.').pop().replace(/[_-]/g, ' ');
      const engineeringMasters = normalizedLookupId.startsWith('sci.eng.ece')
        ? 'M.Tech in Electronics & Communication Engineering, VLSI, or Embedded Systems'
        : normalizedLookupId.startsWith('sci.eng.elec')
          ? 'M.Tech in Electrical Engineering, Power Systems, or Renewable Energy'
          : normalizedLookupId.startsWith('sci.eng.chem')
            ? 'M.Tech in Chemical Engineering or Process Engineering'
            : normalizedLookupId.startsWith('sci.eng.aero') || normalizedLookupId.startsWith('sci.eng.mech.aero')
              ? 'M.Tech in Aerospace / Aeronautical Engineering'
              : normalizedLookupId.startsWith('sci.eng.auto') || normalizedLookupId.startsWith('sci.eng.mech.auto')
                ? 'M.Tech in Automotive Engineering, Electric Vehicles, or Mobility Systems'
                : normalizedLookupId.startsWith('sci.eng.mech')
                  ? 'M.Tech in Mechanical Engineering or the relevant specialization'
                  : normalizedLookupId.startsWith('sci.eng.civil')
                    ? 'M.Tech in Civil Engineering or the relevant infrastructure specialization'
                    : normalizedLookupId.startsWith('sci.eng')
                      ? 'M.Tech / M.E. in the relevant engineering specialization'
                    : '';
      const masters = engineeringMasters || (/^(ai|artificial intelligence|machine learning|ml engineer|ai engineer)$/.test(career)
        ? 'M.Tech / M.Sc. in Artificial Intelligence, Machine Learning, or Data Science'
        : /data/.test(career)
          ? 'M.Tech / M.Sc. in Data Science, Statistics, or Analytics'
          : /cyber|security|hacker|penetration/.test(career)
            ? 'M.Tech / M.Sc. in Cybersecurity or Information Security'
            : 'M.Tech / M.Sc. in Computer Science or the relevant specialization');
      return {
        primaryDegrees: [{ name: masters, shortName: 'Master’s specialization', duration: 2, type: 'Primary', streams: [] }],
        afterDegree: 'Build a focused portfolio, complete an internship or research project, and apply for specialist roles',
        notes: 'Choose a program that builds on your bachelor’s degree and matches your target role.',
      };
    }
    return pathway;
  }

  if (isAfterPostGraduation) {
    return buildPostgraduatePathway(careerContext);
  }
  
  // Prefix-based lookup for unmapped sub-careers
  const prefixes = [
    'sci.eng.cs.cybersec', 'sci.eng.cs.data', 'sci.eng.cs', 
    'sci.eng.mech', 'sci.eng.civil', 'sci.eng.ece', 'sci.eng',
    'sci.med.mbbs', 'sci.med', 'sci.pure',
    'arts.law', 'arts.psych', 'arts.media', 'arts.des',
    'com.fin', 'com.mgmt', 'com',
    'gov', 'govt', 'mgmt', 'design',
    'deg.btech', 'deg.bsc', 'deg.bba', 'deg.ba',
    'prof', 'voc', 'stream', 'hosp',
  ];
  for (const prefix of prefixes) {
    if (resolvedLookupId.startsWith(prefix) && CAREER_EDUCATION_MAP[prefix]) {
      return CAREER_EDUCATION_MAP[prefix];
    }
  }
  
  const careerLabel = careerName.trim() || normalizedLookupId.split('.').filter(Boolean).pop()?.replace(/[_-]/g, ' ') || 'chosen career';
  return {
    primaryDegrees: [],
    afterDegree: '',
    notes: `A verified degree pathway is not yet available for ${careerLabel}.`,
  };
}

export default CAREER_EDUCATION_MAP;
