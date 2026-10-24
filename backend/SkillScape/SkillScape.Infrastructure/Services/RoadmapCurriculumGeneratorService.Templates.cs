namespace SkillScape.Infrastructure.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using SkillScape.Domain.Entities;

public partial class RoadmapCurriculumGeneratorService
{
    private static List<PhaseTemp> CreateStructuredPhases(
        string category,
        string careerTitle,
        (string PhaseTitle, string Level, (string ModTitle, string[] Topics)[] Modules)[] phaseDefs)
    {
        return phaseDefs.Select((phase, phaseIndex) => new PhaseTemp
        {
            Title = $"Phase {phaseIndex + 1}: {phase.PhaseTitle}",
            Level = phase.Level,
            Summary = $"Master core {phase.PhaseTitle.ToLowerInvariant()} competencies through rigorous conceptual study and hands-on deliverables.",
            Duration = phaseIndex == 0 ? "4-6 Weeks" : "5-7 Weeks",
            Modules = phase.Modules.Select((mod, modIndex) => new ModuleTemp
            {
                Title = mod.ModTitle,
                Description = $"Develop expertise in {mod.ModTitle.ToLowerInvariant()} tailored for a professional {careerTitle} role.",
                EstimatedHours = 18 + (phaseIndex * 2),
                Topics = mod.Topics.Select((topic, topicIndex) => new TopicTemp
                {
                    Title = topic,
                    Type = topicIndex == mod.Topics.Length - 1
                        ? RoadmapTopicType.Project
                        : (topicIndex % 2 == 0 ? RoadmapTopicType.Concept : RoadmapTopicType.HandsOnLab)
                }).ToList(),
                Questions = new List<QuestionTemp>
                {
                    new QuestionTemp
                    {
                        QuestionText = $"Which approach is most critical when executing {mod.ModTitle} within {careerTitle}?",
                        Options = new[]
                        {
                            "Apply industry standards, verify calculations and specifications, and document the workflow",
                            "Proceed without documentation or formal verification",
                            "Rely strictly on unverified third-party assumptions",
                            "Bypass quality control and safety checks to expedite completion"
                        },
                        CorrectIndex = 0,
                        Explanation = $"Professional practice in {careerTitle} requires rigorous adherence to standards, verification, and documentation.",
                        Difficulty = phaseIndex < 2 ? "Easy" : "Medium"
                    },
                    new QuestionTemp
                    {
                        QuestionText = $"What is the primary indicator of successful implementation in {mod.ModTitle}?",
                        Options = new[]
                        {
                            "Demonstrated compliance with specifications, robust outcomes, and validated performance",
                            "Minimal testing and rapid deployment without sign-off",
                            "Avoidance of peer review and inspection protocols",
                            "Subjective satisfaction without measurable criteria"
                        },
                        CorrectIndex = 0,
                        Explanation = "Measurable compliance with requirements and verified performance indicate successful professional execution.",
                        Difficulty = phaseIndex < 2 ? "Medium" : "Hard"
                    },
                    new QuestionTemp
                    {
                        QuestionText = $"When resolving technical or operational challenges in {mod.ModTitle}, which methodology delivers the most reliable outcomes?",
                        Options = new[]
                        {
                            $"Perform root-cause analysis, cross-reference domain standards in {careerTitle}, and implement validated solutions",
                            "Apply unverified workarounds without investigating the underlying failure mechanisms",
                            "Ignore anomalous observations and proceed with uncalibrated parameters",
                            "Bypass documentation and skip verification testing to save time"
                        },
                        CorrectIndex = 0,
                        Explanation = $"Systematic root-cause analysis grounded in {careerTitle} standards ensures dependable, long-term outcomes.",
                        Difficulty = "Medium"
                    },
                    new QuestionTemp
                    {
                        QuestionText = $"How should progress, deliverables, and quality metrics for {mod.ModTitle} be validated and communicated?",
                        Options = new[]
                        {
                            "Through structured technical documentation, verifiable metrics, and formal review checkpoints",
                            "Through casual verbal assurances without supporting data or test logs",
                            "By concealing discrepancies until the final delivery phase",
                            "By omitting peer review and quality control records"
                        },
                        CorrectIndex = 0,
                        Explanation = "Verifiable technical documentation, transparent metrics, and formal reviews are essential professional practices.",
                        Difficulty = "Hard"
                    }
                }
            }).ToList()
        }).ToList();
    }

    private static List<PhaseTemp> GetCivilEngineeringPhaseTemplates(string careerTitle)
    {
        var phases = new (string, string, (string, string[])[])[]
        {
            ("Engineering Mechanics, Surveying & Materials", "Beginner", new[]
            {
                ("Engineering Mechanics & Statics", new[] { "System of forces and moments", "Free-body diagrams and equilibrium", "Analysis of pin-jointed trusses", "Friction and friction angle", "Centroid and moment of inertia" }),
                ("Building Materials & Construction Technology", new[] { "Cement chemistry and grade properties", "Concrete technology and mix design", "Aggregates, mortar, and brick/stone masonry", "Structural steel and timber properties", "Indian & international standards (IS/ASTM)" }),
                ("Surveying & Geomatics", new[] { "Total station and theodolite operations", "Differential and reciprocal leveling", "Contouring and earthwork calculations", "GPS, GIS, and remote sensing applications", "Field surveying error adjustments" })
            }),
            ("Structural Analysis & Geotechnical Engineering", "Intermediate", new[]
            {
                ("Strength of Materials & Structural Analysis", new[] { "Stress, strain, and Mohr's circle", "Shear force and bending moment diagrams", "Deflection of determinate beams", "Indeterminate structures (Moment distribution & Slope deflection)", "Influence lines for moving loads" }),
                ("Soil Mechanics & Foundation Engineering", new[] { "Soil classification and index properties", "Permeability, seepage, and flownets", "Shear strength parameters and compaction", "Bearing capacity of shallow footings", "Deep pile foundations and slope stability" }),
                ("Fluid Mechanics & Hydraulics", new[] { "Fluid statics and pressure measurements", "Continuity and Bernoulli equations", "Flow through pipes and head loss", "Open channel flow and hydraulic jump", "Centrifugal pumps and hydraulic turbines" })
            }),
            ("Structural Design (RCC & Steel)", "Advanced", new[]
            {
                ("Design of Reinforced Concrete Structures", new[] { "Limit state design fundamentals", "Design of singular and doubly reinforced beams", "One-way and two-way slab design", "Short and slender columns under axial/eccentric load", "Footings, retaining walls, and seismic detailing" }),
                ("Design of Steel Structures", new[] { "Tension and compression member design", "Bolted and welded structural connections", "Design of built-up beams and plate girders", "Industrial roof trusses and purlins", "Plastic analysis and limit state steel design" }),
                ("Computer-Aided Civil Design & BIM", new[] { "AutoCAD drafting for structural drawings", "STAAD.Pro modeling and load combinations", "ETABS analysis for multi-story buildings", "Building Information Modeling (BIM) fundamentals", "Structural bar bending schedule (BBS) preparation" })
            }),
            ("Transportation, Environmental & Water Resources", "Advanced", new[]
            {
                ("Transportation & Highway Engineering", new[] { "Highway geometric design and sight distances", "Flexible and rigid pavement design (IRC)", "Traffic engineering and intersection design", "Highway drainage and subgrade stabilization", "Pavement distress evaluation and maintenance" }),
                ("Environmental Engineering & Sanitation", new[] { "Water demand forecasting and water quality metrics", "Water treatment plant (WTP) unit processes", "Sewer design and sewerage networks", "Sewage treatment plant (STP) design and effluent disposal", "Solid waste management and environmental impact assessment" }),
                ("Hydrology & Irrigation Engineering", new[] { "Precipitation analysis and hydrograph synthesis", "Reservoir capacity and flood routing", "Gravity dams, spillways, and diversion headworks", "Canal design and cross-drainage works", "Groundwater hydraulics and well testing" })
            }),
            ("Construction Management, Estimating & Capstone Project", "ProLevel", new[]
            {
                ("Quantity Surveying, Estimation & Contracts", new[] { "Detailed item rate estimation", "Bill of Quantities (BOQ) preparation", "Rate analysis and schedule of rates (DSR)", "Tendering, contract types, and FIDIC guidelines", "Property valuation and dispute resolution" }),
                ("Construction Project Planning & Site Safety", new[] { "CPM and PERT network scheduling", "Primavera P6 and MS Project for construction tracking", "Quality assurance and quality control (QA/QC)", "Site safety protocols and OSHA regulations", "Equipment selection and resource leveling" }),
                ("Civil Engineering Comprehensive Capstone", new[] { "Architectural and structural layout formulation", "Complete geotechnical and subsoil investigation", "3D structural analysis and detailed RCC design", "Comprehensive BOQ and cost estimation", "Site logistics, construction schedule, and project report" })
            })
        };
        return CreateStructuredPhases("CivilEngineering", careerTitle, phases);
    }

    private static List<PhaseTemp> GetCybersecurityPhaseTemplates(string careerTitle)
    {
        var phases = new (string, string, (string, string[])[])[]
        {
            ("Networking, Systems & Security Fundamentals", "Beginner", new[]
            {
                ("Computer Networking & Protocol Analysis", new[] { "OSI and TCP/IP protocol architecture", "Subnetting, routing, and switching", "DNS, DHCP, ARP, and ICMP mechanics", "Packet inspection using Wireshark", "Network scanning with Nmap" }),
                ("Linux & Operating System Security", new[] { "Linux terminal mastery and bash scripting", "File system permissions and ACLs", "Windows security architecture and Active Directory basics", "Process management, Daemons, and services", "System auditing and logging configurations" }),
                ("Cryptography & Security Principles", new[] { "Confidentiality, Integrity, and Availability (CIA)", "Symmetric encryption (AES, DES)", "Asymmetric encryption (RSA, ECC)", "Cryptographic hashing and digital signatures", "Public Key Infrastructure (PKI) and SSL/TLS" })
            }),
            ("Defensive Security & SOC Operations", "Intermediate", new[]
            {
                ("Security Operations & SIEM Deployment", new[] { "SOC roles, tiers, and incident workflows", "Log ingestion and normalization (Syslog, Event Viewer)", "SIEM architecture and rule creation (Splunk, ELK)", "Correlation searches and alert triaging", "Writing detection rules with Sigma and YARA" }),
                ("Endpoint Security & Hardening", new[] { "Endpoint Detection and Response (EDR) principles", "Host-based firewalls and antivirus mechanics", "OS hardening benchmarks (CIS Benchmarks)", "Privilege access management (PAM)", "Application whitelisting and patch management" }),
                ("Network Defense & Perimeter Security", new[] { "Next-Generation Firewalls (NGFW) and stateful filtering", "Intrusion Detection & Prevention Systems (Snort, Suricata)", "Virtual Private Networks (VPN) and IPSec", "Zero Trust Network Architecture (ZTNA)", "Web Application Firewalls (WAF)" })
            }),
            ("Offensive Security & Vulnerability Assessment", "Advanced", new[]
            {
                ("Vulnerability Management & Scanning", new[] { "Vulnerability lifecycle and assessment methodology", "Automated scanning with Nessus and OpenVAS", "Common Vulnerability Scoring System (CVSS v3/v4)", "Vulnerability validation and false positive reduction", "Remediation prioritization and executive reporting" }),
                ("Penetration Testing Methodologies", new[] { "Reconnaissance and OSINT techniques", "Port scanning, service enumeration, and banner grabbing", "Exploitation frameworks and Metasploit usage", "Privilege escalation techniques (Linux & Windows)", "Post-exploitation, pivoting, and reporting" }),
                ("Web Application Security & OWASP", new[] { "OWASP Top 10 vulnerabilities walkthrough", "SQL Injection and command injection flaws", "Cross-Site Scripting (XSS) and CSRF mitigation", "Server-Side Request Forgery (SSRF) and IDOR", "Burp Suite Professional for web assessments" })
            }),
            ("Threat Intelligence, Incident Response & Forensics", "Advanced", new[]
            {
                ("Incident Response & Handling", new[] { "NIST and SANS Incident Response lifecycles", "Live incident triage and containment strategies", "Eradication, recovery, and post-incident review", "Evidence acquisition and chain of custody", "Automating IR with SOAR playbooks" }),
                ("Digital Forensics & Artifact Analysis", new[] { "Volatile memory capture and analysis (Volatility)", "Disk imaging and file system forensics (Autopsy, FTK)", "Windows registry, prefetch, and shimcache forensics", "Network flow analysis and pcap reconstruction", "Malware triage in sandbox environments" }),
                ("Threat Intelligence & Threat Hunting", new[] { "MITRE ATT&CK and Cyber Kill Chain mapping", "Indicators of Compromise (IOCs) and STIX/TAXII", "Threat intelligence platforms (MISP, OpenCTI)", "Proactive threat hunting methodologies", "Adversary emulation and tabletop exercises" })
            }),
            ("Cloud Security, Governance & Capstone", "ProLevel", new[]
            {
                ("Cloud & Container Security", new[] { "Cloud shared responsibility model", "AWS/Azure Identity and Access Management (IAM)", "Cloud Security Posture Management (CSPM)", "Container security (Docker & Kubernetes hardening)", "Infrastructure as Code (IaC) security scanning" }),
                ("Security Governance, Risk & Compliance (GRC)", new[] { "ISO 27001, NIST CSF, and CIS Controls", "SOC 2, HIPAA, and GDPR compliance requirements", "Quantitative and qualitative risk assessments", "Vendor risk management and third-party audits", "Business continuity and disaster recovery planning" }),
                ("Cybersecurity Enterprise Capstone", new[] { "Comprehensive enterprise threat model", "Simulated Red Team / Blue Team adversarial engagement", "Full Incident Response and forensic investigation report", "Hardening architecture implementation", "Executive presentation and CISO defense strategy" })
            })
        };
        return CreateStructuredPhases("Cybersecurity", careerTitle, phases);
    }

    private static List<PhaseTemp> GetElectricalElectronicsPhaseTemplates(string careerTitle)
    {
        var phases = new (string, string, (string, string[])[])[]
        {
            ("Circuits, Semiconductors & Instrumentation", "Beginner", new[]
            {
                ("Electric Circuit Theory & Network Analysis", new[] { "Ohm's Law, Kirchhoff's Current & Voltage Laws", "Mesh and nodal analysis of DC circuits", "Network theorems: Thevenin, Norton, Superposition", "AC circuit analysis, phasors, and resonance", "Transient analysis in RL, RC, and RLC circuits" }),
                ("Semiconductor Physics & Diode Applications", new[] { "Intrinsic and extrinsic semiconductors, PN junctions", "Diode characteristics, forward and reverse bias", "Half-wave, full-wave, and bridge rectifiers", "Zener diodes for voltage regulation", "Clipping, clamping, and protection circuits" }),
                ("Electrical Measurements & Instrumentation", new[] { "Analog meters and digital multimeters (DMM)", "Cathode Ray Oscilloscope (CRO) and Digital Storage Oscilloscope (DSO)", "Measurement of resistance, inductance, and capacitance", "Transducers: RTD, thermocouples, strain gauges, LVDT", "Measurement error analysis and instrument calibration" })
            }),
            ("Analog Circuits & Digital Logic Design", "Intermediate", new[]
            {
                ("Analog Electronics & Amplifiers", new[] { "Bipolar Junction Transistors (BJT): configurations and biasing", "Field Effect Transistors (MOSFETs) and switching characteristics", "Small-signal amplifier analysis and frequency response", "Operational Amplifiers (Op-Amps): inverting, non-inverting, differential", "Active filters, comparators, and 555 timer oscillator circuits" }),
                ("Digital Electronics & Logic Design", new[] { "Number systems, Boolean algebra, and Karnaugh maps (K-Maps)", "Combinational circuits: adders, multiplexers, decoders", "Sequential circuits: Latches, Flip-Flops (SR, JK, D, T)", "Synchronous and asynchronous counter design", "Finite State Machines (Mealy and Moore models)" }),
                ("Microcontrollers & Embedded Hardware", new[] { "Microcontroller architecture (ARM Cortex, AVR, PIC)", "GPIO programming, interrupt service routines (ISRs)", "Timers, PWM generation, and ADC/DAC interfacing", "Communication protocols: UART, SPI, and I2C", "Embedded C programming and bare-metal debugging" })
            }),
            ("Signals, Systems & Power Engineering", "Advanced", new[]
            {
                ("Signals, Systems & Control Engineering", new[] { "Continuous-time and discrete-time signal classifications", "Fourier series, Fourier transforms, and Laplace transforms", "Transfer functions and block diagram reduction", "Time-domain response and Routh-Hurwitz stability", "Frequency response analysis: Bode plots and Nyquist criteria" }),
                ("Electrical Machines & Transformers", new[] { "Single-phase and three-phase transformer theory", "DC machines: motors, generators, and speed control", "Three-phase induction motor operating principles", "Synchronous generators and synchronous motors", "Testing, efficiency calculations, and equivalent circuits" }),
                ("Power Systems Generation, Transmission & Distribution", new[] { "Power generation methods (thermal, hydro, nuclear, solar)", "Transmission line parameters (inductance, capacitance, ABCD parameters)", "Per-unit system and load flow analysis (Gauss-Seidel, Newton-Raphson)", "Fault analysis: symmetrical and asymmetrical faults", "Power factor improvement and reactive power compensation" })
            }),
            ("Power Electronics & Industrial Automation", "Advanced", new[]
            {
                ("Power Electronic Devices & Converters", new[] { "Power semiconductors: Thyristors, MOSFETs, and IGBTs", "Controlled rectifiers and phase-controlled converters", "DC-DC converters: Buck, Boost, and Buck-Boost topologies", "DC-AC inverters: single-phase and three-phase PWM inverters", "Snubber circuits and thermal heat sink management" }),
                ("Electric Drives & Motor Control", new[] { "Electric drive fundamentals and four-quadrant operation", "DC drive speed control using converters and choppers", "Variable Frequency Drives (VFD) for induction motors", "Vector control and Field Oriented Control (FOC)", "Regenerative braking and industrial motion applications" }),
                ("Industrial Automation, PLC & SCADA", new[] { "Programmable Logic Controllers (PLC) architecture", "Ladder logic programming and function block diagrams", "Industrial sensors, actuators, and relay logic", "SCADA interface design and HMI programming", "Industrial fieldbus networks: Modbus, Profibus, Ethernet/IP" })
            }),
            ("PCB Design, Smart Grids & Capstone Project", "ProLevel", new[]
            {
                ("PCB Design & Hardware Prototyping", new[] { "Schematic capture using KiCad or Altium Designer", "PCB layout guidelines, trace impedance, and routing", "Ground planes, electromagnetic compatibility (EMC), and EMI", "Design for Manufacturing (DFM) and Gerber file generation", "SMD component soldering, assembly, and board bring-up" }),
                ("Smart Grid & Renewable Energy Systems", new[] { "Solar photovoltaic system sizing and MPPT algorithms", "Wind energy conversion systems and grid integration", "Battery Energy Storage Systems (BESS) and BMS design", "Smart metering, microgrids, and protection coordination", "Grid codes and power quality standards (IEEE 519)" }),
                ("Comprehensive Electrical & Electronics Capstone", new[] { "System requirements and hardware specification document", "Complete circuit schematic and simulation verification", "PCB layout, physical assembly, and laboratory testing", "Control algorithm implementation and firmware deployment", "Final test validation report and professional technical review" })
            })
        };
        return CreateStructuredPhases("ElectricalElectronics", careerTitle, phases);
    }

    private static List<PhaseTemp> GetChemicalEngineeringPhaseTemplates(string careerTitle)
    {
        var phases = new (string, string, (string, string[])[])[]
        {
            ("Material Balances, Chemistry & Thermodynamics", "Beginner", new[]
            {
                ("Process Calculations & Material Balances", new[] { "Units, dimensions, and process variables", "Material balances for non-reacting systems", "Material balances with chemical reactions and stoichiometry", "Recycle, bypass, and purge stream computations", "Combustion calculations and flue gas analysis" }),
                ("Chemical Engineering Thermodynamics", new[] { "First and second laws of thermodynamics in flow systems", "PVT behavior of pure substances and equations of state", "Heat effects: sensible heat, latent heat, heat of reaction", "Phase equilibria: Raoult's law, VLE, and fugacity", "Chemical reaction equilibria and equilibrium constants" }),
                ("Applied Chemistry & Process Analysis", new[] { "Industrial organic and inorganic chemical pathways", "Reaction kinetics and chemical equilibrium in solutions", "Electrochemistry and industrial corrosion prevention", "Standard analytical techniques and laboratory titration", "Raw material quality evaluation and characterization" })
            }),
            ("Transport Phenomena (Fluid, Heat & Mass)", "Intermediate", new[]
            {
                ("Fluid Flow Operations", new[] { "Fluid statics, manometry, and pressure drops", "Newtonian and non-Newtonian fluid rheology", "Navier-Stokes equations and laminar/turbulent flow in pipes", "Pumps, compressors, blowers, and Net Positive Suction Head (NPSH)", "Flow measurement: Orifice, Venturi, and Rotameter meters" }),
                ("Heat Transfer Operations", new[] { "Conduction: Fourier's law and composite walls", "Convection: forced and natural convection heat transfer coefficients", "Radiation heat transfer and view factors", "Design of shell-and-tube and plate heat exchangers", "Evaporators, condensers, and reboilers" }),
                ("Mass Transfer Principles", new[] { "Fick's law of diffusion and mass transfer coefficients", "Interphase mass transfer and two-film theory", "Distillation: McCabe-Thiele and Ponchon-Savarit methods", "Gas absorption and stripping column design", "Humidification, dehumidification, and cooling tower operations" })
            }),
            ("Reaction Engineering & Separation Processes", "Advanced", new[]
            {
                ("Chemical Reaction Engineering", new[] { "Kinetics of homogeneous and catalytic reactions", "Ideal reactor design: Batch, CSTR, and PFR", "Reactor sizing, series/parallel combinations", "Multiple reactions, yield, and selectivity optimization", "Temperature and pressure effects in non-isothermal reactors" }),
                ("Advanced Separation Processes", new[] { "Liquid-liquid extraction and ternary phase diagrams", "Leaching, solid-liquid extraction, and washing", "Adsorption principles and fixed-bed breakthrough curves", "Membrane separation: reverse osmosis, ultrafiltration", "Industrial crystallization and drying equipment design" }),
                ("Process Dynamics & Instrumentation", new[] { "First-order and second-order dynamic systems", "Sensors: temperature, pressure, flow, and level instruments", "Feedback control loops: P, PI, and PID controllers", "Controller tuning and frequency response (Bode/Nyquist)", "Distributed Control Systems (DCS) and safety interlocks" })
            }),
            ("Plant Safety, Equipment Design & Economics", "Advanced", new[]
            {
                ("Chemical Process Equipment Design", new[] { "Mechanical design of thin and thick-walled pressure vessels", "Design of storage tanks, heads, and nozzles (ASME codes)", "Piping and Instrumentation Diagrams (P&ID) development", "Support structures: skirt, saddle, and bracket supports", "Gasket, flange, and bolt selection standards" }),
                ("Process Safety & Loss Prevention (HAZOP)", new[] { "Hazard and Operability (HAZOP) analysis methodologies", "Toxicity, flammability, and explosion limits (LEL/UEL)", "Relief valve sizing and flare systems design", "Source models for toxic gas dispersion and fire scenarios", "OSHA Process Safety Management (PSM) regulations" }),
                ("Process Economics & Cost Estimation", new[] { "Capital investment (CAPEX) and operating expenses (OPEX)", "Cost indices and equipment scaling factors", "Depreciation, taxes, and cash flow analysis", "Profitability criteria: Net Present Value (NPV), Payback, IRR", "Pinch analysis and energy integration in chemical plants" })
            }),
            ("Process Simulation & Capstone Chemical Plant Design", "ProLevel", new[]
            {
                ("Process Flowsheet Simulation", new[] { "Process simulation using Aspen Plus or DWSIM", "Thermodynamic property method selection (NRTL, Peng-Robinson)", "Flowsheet convergence, recycle loops, and design specifications", "Sensitivity analysis and process parameter optimization", "Equipment rating and column hydraulics in simulation tools" }),
                ("Environmental Engineering & Sustainable Processes", new[] { "Industrial wastewater treatment (biological and physicochemical)", "Air emission controls: scrubbers, baghouses, and ESPs", "Solid hazardous waste treatment and disposal", "Carbon capture, utilization, and storage (CCUS)", "Life Cycle Assessment (LCA) and circular chemistry" }),
                ("Comprehensive Chemical Plant Design Capstone", new[] { "Process selection and comprehensive block flow diagram (BFD)", "Complete mass and energy balances around all unit operations", "Detailed mechanical and process sizing of key reactor and columns", "Plant safety HAZOP, P&ID diagram, and site layout plan", "Techno-economic feasibility study and final design defense" })
            })
        };
        return CreateStructuredPhases("ChemicalEngineering", careerTitle, phases);
    }

    private static List<PhaseTemp> GetMedicineClinicalPhaseTemplates(string careerTitle)
    {
        var phases = new (string, string, (string, string[])[])[]
        {
            ("Anatomy, Physiology & Medical Biochemistry", "Beginner", new[]
            {
                ("Human Gross Anatomy & Histology", new[] { "Musculoskeletal system and joint biomechanics", "Thoracic, abdominal, and pelvic cavity anatomy", "Neuroanatomy: central and peripheral nervous systems", "Cardiovascular and bronchopulmonary anatomy", "Microscopic histology of primary tissues and organs" }),
                ("Medical Physiology & Organ Systems", new[] { "Cardiac cycle, hemodynamics, and blood pressure regulation", "Respiratory mechanics, gas exchange, and acid-base balance", "Renal clearance, glomerular filtration, and electrolyte homeostasis", "Gastrointestinal digestion, absorption, and motility", "Endocrine feedback loops and reproductive physiology" }),
                ("Medical Biochemistry & Molecular Genetics", new[] { "Carbohydrate, lipid, and amino acid metabolism", "Enzyme kinetics, cofactors, and clinical enzymology", "DNA replication, transcription, and translation", "Genetic mutations, chromosomal disorders, and molecular diagnostics", "Nutrition, vitamins, and mineral deficiencies" })
            }),
            ("Pathology, Microbiology & Pharmacology", "Intermediate", new[]
            {
                ("General & Systemic Pathology", new[] { "Cell injury, necrosis, apoptosis, and cellular adaptations", "Acute and chronic inflammation, wound repair, and fibrosis", "Hemodynamic disorders: thrombosis, embolism, infarction, shock", "Neoplasia: oncogenes, tumor suppressors, and metastasis", "Systemic pathology of cardiovascular, respiratory, and renal diseases" }),
                ("Medical Microbiology & Immunology", new[] { "Bacterial taxonomy, pathogenesis, and culture techniques", "Viral structure, replication cycles, and clinical virology", "Mycology and parasitic infections of clinical importance", "Innate and adaptive immune mechanisms, antibodies, and T-cells", "Vaccines, hypersensitivity reactions, and autoimmune disorders" }),
                ("Medical Pharmacology & Therapeutics", new[] { "Pharmacokinetics: ADME (Absorption, Distribution, Metabolism, Excretion)", "Pharmacodynamics: receptors, dose-response, and mechanisms", "Autonomic nervous system pharmacology (adrenergic and cholinergic)", "Cardiovascular and renal pharmacology (anti-hypertensives, diuretics)", "Antimicrobial agents, resistance mechanisms, and antibiotic stewardship" })
            }),
            ("Clinical Skills, Diagnostics & Internal Medicine", "Advanced", new[]
            {
                ("Clinical Examination & Patient Communication", new[] { "Comprehensive medical history taking and review of systems", "Physical examination of cardiovascular and respiratory systems", "Abdominal palpation, percussion, and neurological examination", "Vital signs monitoring, triage assessment, and clinical documentation", "Doctor-patient communication, empathetic counseling, and consent" }),
                ("Diagnostic Modalities & Laboratory Medicine", new[] { "12-Lead Electrocardiogram (ECG) interpretation and arrhythmias", "Chest X-ray (CXR) and plain film radiologic interpretation", "Fundamentals of CT, MRI, and ultrasound imaging", "Complete blood count (CBC), coagulation profiles, and blood smears", "Comprehensive metabolic panel (CMP), liver and renal function tests" }),
                ("Internal Medicine Principles", new[] { "Cardiovascular disorders: coronary artery disease, heart failure", "Respiratory medicine: COPD, asthma, pneumonia, tuberculosis", "Gastroenterology and hepatology: ulcers, cirrhosis, hepatitis", "Endocrinology: Type 1 & 2 diabetes, thyroid diseases", "Infectious diseases: sepsis protocols, vector-borne diseases" })
            }),
            ("Surgery, Emergency Medicine & Critical Care", "Advanced", new[]
            {
                ("Surgical Principles & Perioperative Care", new[] { "Aseptic technique, surgical sterilization, and operating room etiquette", "Wound closure techniques, suturing, and knot tying", "Pre-operative evaluation and surgical risk stratification", "Post-operative monitoring, pain management, and wound complications", "Surgical emergencies: acute abdomen, appendicitis, bowel obstruction" }),
                ("Emergency Medicine, Trauma & Resuscitation", new[] { "Basic Life Support (BLS) and Advanced Cardiac Life Support (ACLS)", "Trauma evaluation: Advanced Trauma Life Support (ATLS) ABCDE approach", "Shock identification, classification, and fluid resuscitation", "Airway management, endotracheal intubation, and oxygen therapy", "Toxicology: acute poisoning management and antidotes" }),
                ("Pediatrics, Obstetrics & Gynecology", new[] { "Normal pediatric growth milestones and immunization schedules", "Common pediatric infections, dehydration, and nutritional disorders", "Antenatal care, normal pregnancy stages, and prenatal screening", "Stages of labor, delivery management, and postpartum care", "Common gynecological conditions and family planning counseling" })
            }),
            ("Ethics, Evidence-Based Medicine & Clinical Capstone", "ProLevel", new[]
            {
                ("Medical Ethics, Law & Healthcare Systems", new[] { "Core medical ethics: autonomy, beneficence, non-maleficence, justice", "Informed consent, confidentiality, and medical documentation laws", "Medical negligence, patient rights, and medicolegal guidelines", "Patient safety protocols, clinical governance, and error reporting", "Public health infrastructure, epidemiology, and disease surveillance" }),
                ("Evidence-Based Medicine & Clinical Research", new[] { "Formulating PICO clinical questions and literature searching", "Critical appraisal of randomized controlled trials (RCTs)", "Biostatistics: p-values, confidence intervals, sensitivity/specificity", "Clinical practice guidelines and evidence hierarchy", "Translating clinical evidence into personalized patient care plans" }),
                ("Comprehensive Clinical Case Management Capstone", new[] { "Complete clinical history and systemic examination case workup", "Differential diagnosis synthesis and diagnostic investigation plan", "Multidisciplinary evidence-based therapeutic treatment plan", "Patient education, lifestyle modification, and follow-up regimen", "Clinical case presentation and peer grand rounds defense" })
            })
        };
        return CreateStructuredPhases("MedicineClinical", careerTitle, phases);
    }

    private static List<PhaseTemp> GetTeachingEducationPhaseTemplates(string careerTitle)
    {
        var phases = new (string, string, (string, string[])[])[]
        {
            ("Foundations of Education & Child Development", "Beginner", new[]
            {
                ("Educational Psychology & Learning Theories", new[] { "Cognitive development theories (Piaget, Vygotsky)", "Behaviorism and operant conditioning in education", "Constructivist learning approaches and experiential learning", "Intrinsic and extrinsic student motivation strategies", "Cognitive load theory and retention mechanisms" }),
                ("Child & Adolescent Developmental Stages", new[] { "Physical and motor skill development in learners", "Social and emotional development milestones", "Language acquisition and cognitive speech milestones", "Adolescent identity formation and social dynamics", "Identifying developmental delays and learning difficulties" }),
                ("Philosophy, History & Policy of Education", new[] { "Major educational philosophies (Idealism, Realism, Pragmatism)", "Evolution of modern schooling and pedagogical movements", "National education policies, curriculum frameworks, and standards", "Educational equity, inclusion, and social justice in schools", "The teacher's role as educator, mentor, and institutional leader" })
            }),
            ("Curriculum Design, Pedagogy & Strategies", "Intermediate", new[]
            {
                ("Curriculum Development & Lesson Planning", new[] { "Understanding by Design (UbD) and backward design", "Formulating SMART learning objectives with Bloom's Taxonomy", "Structuring comprehensive unit and daily lesson plans", "Curriculum alignment with state and national standards", "Scaffolding complex concepts for varied learners" }),
                ("Differentiated Pedagogy & Universal Design", new[] { "Principles of Universal Design for Learning (UDL)", "Tiered assignments and differentiated instruction strategies", "Adapting content, process, and product for diverse needs", "Supporting gifted and accelerated students", "Interventions for struggling readers and remedial learners" }),
                ("Active Learning & Instructional Methods", new[] { "Inquiry-based and problem-based learning (PBL)", "Collaborative group work and cooperative learning models", "Socratic questioning techniques and classroom dialogue", "Flipped classroom model and blended learning design", "Experiential learning labs and project exhibitions" })
            }),
            ("Classroom Management & Inclusive Practices", "Advanced", new[]
            {
                ("Classroom Management & Positive Behavior Support", new[] { "Establishing classroom norms, routines, and culture", "Positive Behavioral Interventions and Supports (PBIS)", "De-escalation strategies and restorative justice in schools", "Conflict resolution among peers and bullying prevention", "Time management and minimizing classroom downtime" }),
                ("Special Education & Inclusive Classrooms", new[] { "Individualized Education Programs (IEPs) and 504 Plans", "Understanding neurodiversity: ADHD, Autism spectrum, Dyslexia", "Accommodations versus modifications in coursework", "Collaborating with special educators and therapists", "Assistive technology tools for inclusive learning" }),
                ("Educational Technology & Digital Learning Tools", new[] { "Learning Management Systems (LMS) navigation and grading", "Interactive whiteboards and digital presentation tools", "Educational software, gamification, and quiz tools", "Digital citizenship, online safety, and media literacy", "Designing engaging virtual and hybrid learning activities" })
            }),
            ("Assessment, Evaluation & Student Progress", "Advanced", new[]
            {
                ("Formative & Summative Assessment Design", new[] { "Formative assessment techniques (exit tickets, checks for understanding)", "Designing valid, reliable summative exams and tests", "Constructing rubric-based grading matrices for open-ended tasks", "Peer assessment and self-evaluation exercises", "Providing timely, actionable, and constructive student feedback" }),
                ("Educational Data Analysis & Monitoring", new[] { "Analyzing standardized test data and classroom metrics", "Diagnostic assessments to identify learning gaps", "Response to Intervention (RTI) data tracking", "Data-informed instructional adjustments and reteaching", "Communicating progress and report cards to parents" }),
                ("Literacy, Language & Communication Strategies", new[] { "Disciplinary literacy across science, math, and humanities", "Supporting English Language Learners (ELL / ESL)", "Academic vocabulary acquisition strategies", "Oral presentation skills and public speaking in class", "Effective parent-teacher conferencing and home communication" })
            }),
            ("Teacher Leadership, Action Research & Practicum", "ProLevel", new[]
            {
                ("Professional Ethics, Law & Institutional Responsibility", new[] { "Teacher professional code of conduct and ethics", "Child protection laws, safety guidelines, and mandatory reporting", "Student privacy rights and educational record compliance (FERPA)", "Professional learning communities (PLCs) and collaboration", "Teacher wellbeing, stress management, and work-life balance" }),
                ("Classroom Action Research & Reflective Teaching", new[] { "Designing and conducting classroom action research", "Reflective teaching journals and video lesson analysis", "Peer observation protocols and pedagogical feedback", "Publishing teaching insights and participating in conferences", "Continuous professional development (CPD) planning" }),
                ("Comprehensive Teaching Practicum Capstone", new[] { "Complete 6-week unit plan with lesson plans and assessments", "Recorded or observed master class instruction demonstration", "Student work portfolio with diagnostic, formative, and summative analysis", "Parent communication log and stakeholder feedback compilation", "Professional teaching philosophy statement and interview portfolio" })
            })
        };
        return CreateStructuredPhases("TeachingEducation", careerTitle, phases);
    }

    private static List<PhaseTemp> GetArchitecturePlanningPhaseTemplates(string careerTitle)
    {
        var phases = new (string, string, (string, string[])[])[]
        {
            ("Drawing, History & Design Fundamentals", "Beginner", new[]
            {
                ("Architectural Graphics & Manual Drafting", new[] { "Orthographic projections: plans, elevations, and sections", "Architectural lettering, line weights, and dimensioning conventions", "Axonometric, isometric, and perspective drawing methods", "Freehand sketching, architectural rendering, and shade/shadow", "Scale, proportion, and drafting sheet layout standards" }),
                ("History & Theory of Architecture", new[] { "Ancient and Classical architecture (Egyptian, Greek, Roman)", "Medieval, Renaissance, and Baroque architectural styles", "Industrial revolution and Modern movement (Bauhaus, Le Corbusier)", "Postmodernism and Contemporary architectural directions", "Vernacular architecture and indigenous construction methods" }),
                ("Design Principles & Spatial Composition", new[] { "Form, space, order, rhythm, hierarchy, and symmetry", "Anthropometrics and ergonomics in architectural space", "Spatial relationships: interlocking spaces, adjacent spaces, circulation", "Site zoning, orientation, and microclimate considerations", "Concept generation and architectural diagramming" })
            }),
            ("Building Materials, Construction & Structures", "Intermediate", new[]
            {
                ("Building Construction & Materials", new[] { "Brick and stone masonry construction techniques", "Timber construction: framing, trusses, and joinery", "Reinforced concrete construction: footings, slabs, beams, columns", "Structural steel framing, decking, and connections", "Roofing systems, waterproofing, and damp-proofing details" }),
                ("Architectural Structures & Statics", new[] { "Structural loads: dead, live, wind, and seismic loads", "Load paths and structural equilibrium in buildings", "Structural systems: post-and-beam, arch, vault, dome, tensile structures", "Integration of structural elements with architectural form", "Sizing basic structural members and foundation types" }),
                ("Digital Drafting & 2D Computer-Aided Design", new[] { "AutoCAD workspace setup, units, and coordinate systems", "Layer management, lineweights, and plotting styles (CTB)", "Precision drafting of architectural floor plans and sections", "Door, window, and finish schedules generation", "Construction drawing sets and annotation standards" })
            }),
            ("3D Modeling, BIM & Environmental Design", "Advanced", new[]
            {
                ("3D Visualization & Architectural Modeling", new[] { "3D massing and surface modeling (SketchUp, Rhino)", "Architectural rendering engines (V-Ray, Enscape, Lumion)", "Materials, texturing, natural and artificial lighting setup", "Camera composition, walkthrough animations, and post-processing", "Physical architectural model making and digital fabrication" }),
                ("Building Information Modeling (BIM Workflows)", new[] { "Revit architectural modeling: walls, floors, roofs, stairs", "BIM family creation and parametric component design", "Automated quantity takeoffs, schedules, and sheet sets", "Clash detection and multi-disciplinary collaboration (MEP/Structural)", "BIM execution plans and Level of Development (LOD) standards" }),
                ("Climate-Responsive & Sustainable Design", new[] { "Sun path diagrams, solar shading devices, and daylighting design", "Natural ventilation strategies and thermal comfort indices", "Building envelope thermal performance and U-value calculations", "Rainwater harvesting and water-sensitive urban design", "Green building rating systems (LEED, GRIHA, BREEAM)" })
            }),
            ("Building Services, Codes & Urban Planning", "Advanced", new[]
            {
                ("Building Services & MEP Integration", new[] { "Water supply and plumbing distribution systems in buildings", "Drainage, sanitation, and wastewater management", "HVAC system selection and duct routing principles", "Electrical layout, lighting design, and emergency power", "Fire safety: egress stairs, fire doors, sprinkler systems, and smoke management" }),
                ("Building Regulations, Codes & Accessibility", new[] { "Zoning laws, Floor Area Ratio (FAR/FSI), and ground coverage", "Building setbacks, height restrictions, and parking requirements", "Building bye-laws and national building codes (NBC/IBC)", "Universal design and accessibility standards (ADA compliance)", "Building permit approval processes and documentation" }),
                ("Urban Design & Site Planning", new[] { "Site topography, contours, and cut-and-fill analysis", "Vehicular and pedestrian circulation networks", "Landscape design: hardscape, softscape, and plant selection", "Urban fabric analysis, public open spaces, and placemaking", "Master planning principles for campus and community development" })
            }),
            ("Professional Practice & Architecture Capstone", "ProLevel", new[]
            {
                ("Professional Practice & Project Management", new[] { "Architectural office management, fee structures, and contracts", "Architect-client relationship and stakeholder management", "Bidding, tender documents, and contractor selection", "Construction site supervision, inspection, and snag lists", "Professional liabilities, copyright, and ethics in practice" }),
                ("Working Drawings & Specifications", new[] { "Comprehensive architectural working drawing package", "Detailed construction drawings: toilets, kitchens, staircases", "Door and window details, curtain walls, and joinery details", "Material specifications and bill of quantities (BOQ)", "Coordination of architectural drawings with structural and MEP sets" }),
                ("Comprehensive Architectural Thesis / Capstone", new[] { "Site selection, client brief, and socio-environmental context study", "Concept development, zoning, and spatial massing scheme", "Complete architectural drawings: site plan, floor plans, sections, elevations", "Structural system, MEP integration, and sustainability strategy", "High-resolution 3D visualizations, physical model, and jury presentation" })
            })
        };
        return CreateStructuredPhases("ArchitecturePlanning", careerTitle, phases);
    }
}
