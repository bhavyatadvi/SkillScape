import React, { useState, useEffect } from 'react';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Progress } from '@/components/ui/progress';
import { Download, Plus, Trash2, FileText, Loader2 } from 'lucide-react';
import { toast } from 'sonner';
import { resumeApi } from '@/services/api';

const ResumePage = () => {
  const [loading, setLoading] = useState(true);
  const [baseData, setBaseData] = useState(null);

  const [formData, setFormData] = useState({
    name: '',
    role: '',
    email: '',
    phone: '',
    location: '',
    linkedin: '',
    github: '',
    leetcode: '',
    portfolio: '',
    education: [],
    experience: [],
    projects: [],
    achievements: [],
    extras: [],
  });

  const generateId = () => Math.random().toString(36).substr(2, 9);

  useEffect(() => {
    const fetchResumeData = async () => {
      try {
        setLoading(true);
        const response = await resumeApi.preview();
        const data = response.data;
        setBaseData(data);
        setFormData(prev => ({
          ...prev,
          name: data.personalInfo.name || '',
          email: data.personalInfo.email || '',
        }));
      } catch (err) {
        toast.error('Failed to auto-populate resume data');
      } finally {
        setLoading(false);
      }
    };
    fetchResumeData();
  }, []);

  const loadSampleData = () => {
    setFormData({
      name: 'PRIYAL RAWAL',
      role: 'Software Engineer / AI/ML Enthusiast',
      email: 'priyal.rawal@gmail.com',
      phone: '+91 9876543210',
      location: 'Hyderabad, India',
      linkedin: 'linkedin.com/in/priyal-rawal',
      github: 'github.com/priyal-rawal',
      leetcode: 'LeetCode: Top 15%',
      portfolio: 'GFG Rank: 1251',
      education: [
        {
          _id: generateId(),
          degree: 'Bachelor of Computer Science and Engineering',
          school: 'The Maharaja Sayajirao University of Baroda',
          date: 'Aug 2022 - May 2026',
          score: 'CGPA: 8.42/10.00'
        }
      ],
      experience: [
        {
          _id: generateId(),
          title: 'SWE Intern, Anti-Financial Crime - Payments',
          company: 'Google',
          location: 'Hyderabad, India',
          date: 'May 2023 - August 2023',
          description: `- Was part of the Anti-Financial Crime - Payments team and enhanced financial compliance systems by improving the sanctions screening infrastructure.
- Designed and implemented two new maps analytics tables to identify top restricted parties causing high reverse customer impact and provide a load matrix by product area, country, and other relevant dimensions.
- Built a Java-based backend service that generated real-time, rule-based suggestions to support analyst decision-making.`
        },
        {
          _id: generateId(),
          title: 'Contributor',
          company: 'GirlScript Summer of Code',
          location: 'Remote',
          date: 'May 2023 - Aug 2023',
          description: `- Developed interactive front-end components in React.js
- Refactored legacy UI code, cutting bugs and boosting developer onboarding speed.
- Reviewed 20+ pull requests, enforcing consistency and improving maintainability.`
        }
      ],
      projects: [
        {
          _id: generateId(),
          name: 'JobMate - AI Powered Resume Suite',
          date: 'Apr 2024 - May 2024',
          technologies: 'React.js, Node.js, Express.js, MongoDB, OpenAI API | GitHub',
          description: `- Built an end-to-end AI-based resume builder using GPT embeddings and parsing APIs, improving job match accuracy.
- Implemented real-time mock interview generator with dynamic question generation and webcam-based cheating detection.
- Developed a recruiter panel with auto-ranking and filtering of 100+ resumes based on skill job fit and relevance scores.`
        }
      ],
      achievements: [
        { _id: generateId(), text: 'GFG Global Rank: 1251 (Top 3%) | Rating: 1881' },
        { _id: generateId(), text: 'Leetcode Top 15% | Rating: 1665' },
        { _id: generateId(), text: 'Top 20 out of 800+ teams in Odoo Hackathon' }
      ],
      extras: [
        { _id: generateId(), text: 'Mentored Grade 9-10 students for IMO, resulting in 10% avg. performance improvement.' }
      ]
    });
  };

  const handleExport = () => window.print();

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const addItem = (field) => {
    setFormData(prev => ({
      ...prev,
      [field]: [...prev[field], field === 'achievements' || field === 'extras' ? { _id: generateId(), text: '' } : { _id: generateId() }]
    }));
  };

  const updateItem = (field, id, key, value) => {
    setFormData(prev => {
      const newList = prev[field].map(item => item._id === id ? { ...item, [key]: value } : item);
      return { ...prev, [field]: newList };
    });
  };

  const removeItem = (field, id) => {
    setFormData(prev => {
      const newList = prev[field].filter(item => item._id !== id);
      return { ...prev, [field]: newList };
    });
  };

  const calculateATS = () => {
    let score = 0;
    if (formData.name) score += 5;
    if (formData.email && formData.phone) score += 10;
    if (formData.linkedin || formData.github) score += 5;
    if (formData.experience.length > 0) score += 25;
    if (formData.education.length > 0) score += 15;
    if (formData.projects.length > 0) score += 15;
    if (baseData?.skills?.length > 0) score += 15;
    const expLen = formData.experience.reduce((acc, curr) => acc + (curr.description?.length || 0), 0);
    const projLen = formData.projects.reduce((acc, curr) => acc + (curr.description?.length || 0), 0);
    if (expLen > 50 || projLen > 50) score += 10;
    return Math.min(score, 100);
  };

  const atsScore = calculateATS();

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh] print:hidden">
        <Loader2 className="h-12 w-12 animate-spin text-accent" />
      </div>
    );
  }

  const SectionHeader = ({ title }) => (
    <div className="mb-2 mt-4">
      <h2 className="text-[12px] font-bold text-blue-900 uppercase tracking-widest leading-none">{title}</h2>
      <div className="w-[100%] h-[1.5px] bg-blue-900/40 mt-1 mb-2" />
    </div>
  );

  return (
    <div className="flex flex-col lg:flex-row min-h-[90vh] gap-6">
      <style dangerouslySetInnerHTML={{
        __html: `
        @media print {
          body { visibility: hidden; background: white; }
          .print-area { visibility: visible; position: absolute; left: 0; top: 0; width: 100%; border: none; box-shadow: none; padding: 0; margin: 0; }
          .print-hidden { display: none !important; }
        }
      `}} />

      {/* LEFT COLUMN: BUILDER FORM */}
      <div className="w-full lg:w-[45%] lg:pr-6 lg:border-r border-border space-y-8 print-hidden overflow-y-auto max-h-[85vh] pr-2 custom-scrollbar">
        <div className="flex justify-between items-center sticky top-0 bg-background/95 backdrop-blur z-10 py-4 mb-4 border-b border-border">
          <div>
            <h1 className="text-2xl font-bold text-foreground">ATS Resume Builder</h1>
            <div className="flex items-center gap-2 mt-2">
              <span className="text-sm text-muted-foreground font-medium">ATS Score:</span>
              <Progress value={atsScore} className="h-2 w-24" />
              <span className={`text-sm font-bold ${atsScore >= 80 ? 'text-green-500' : 'text-yellow-500'}`}>{atsScore}%</span>
            </div>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" className="shadow-sm" onClick={loadSampleData}>
              Load Sample Data
            </Button>
            <Button className="bg-gradient-accent shadow-lg text-white" onClick={handleExport}>
              <Download className="mr-2 h-4 w-4" /> Download PDF
            </Button>
          </div>
        </div>

        {/* Personal Details */}
        <div className="space-y-4">
          <h2 className="text-lg font-semibold flex items-center gap-2"><FileText className="h-5 w-5 text-accent" /> Personal Details</h2>
          <div className="grid grid-cols-2 gap-4">
            <Input name="name" value={formData.name} onChange={handleInputChange} placeholder="Full Name" />
            <Input name="role" value={formData.role} onChange={handleInputChange} placeholder="Role / Tagline" />
            <Input name="email" value={formData.email} onChange={handleInputChange} placeholder="Email" />
            <Input name="phone" value={formData.phone} onChange={handleInputChange} placeholder="Phone" />
            <Input name="linkedin" value={formData.linkedin} onChange={handleInputChange} placeholder="LinkedIn URL" />
            <Input name="github" value={formData.github} onChange={handleInputChange} placeholder="GitHub URL" />
            <Input name="leetcode" value={formData.leetcode} onChange={handleInputChange} placeholder="LeetCode/Other URL" />
            <Input name="portfolio" value={formData.portfolio} onChange={handleInputChange} placeholder="Portfolio/GFG URL" />
          </div>
        </div>

        {/* Experience */}
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Experience</h2>
            <Button size="sm" variant="outline" onClick={() => addItem('experience')}><Plus className="h-4 w-4 mr-1" /> Add</Button>
          </div>
          {formData.experience.map((exp) => (
            <Card key={exp._id} className="p-4 relative group">
              <Button size="icon" variant="destructive" className="absolute top-2 right-2 h-7 w-7 opacity-0 group-hover:opacity-100 transition-opacity" onClick={() => removeItem('experience', exp._id)}><Trash2 className="h-4 w-4" /></Button>
              <div className="grid grid-cols-2 gap-3">
                <Input placeholder="Job Title" value={exp.title || ''} onChange={(e) => updateItem('experience', exp._id, 'title', e.target.value)} />
                <Input placeholder="Company" value={exp.company || ''} onChange={(e) => updateItem('experience', exp._id, 'company', e.target.value)} />
                <Input placeholder="Location" value={exp.location || ''} onChange={(e) => updateItem('experience', exp._id, 'location', e.target.value)} />
                <Input placeholder="Date (e.g. May 2023 - Aug 2023)" value={exp.date || ''} onChange={(e) => updateItem('experience', exp._id, 'date', e.target.value)} />
                <Textarea className="col-span-2" placeholder="Description (enter a new line for each bullet point)..." value={exp.description || ''} onChange={(e) => updateItem('experience', exp._id, 'description', e.target.value)} />
              </div>
            </Card>
          ))}
        </div>

        {/* Projects */}
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Projects</h2>
            <Button size="sm" variant="outline" onClick={() => addItem('projects')}><Plus className="h-4 w-4 mr-1" /> Add</Button>
          </div>
          {formData.projects.map((proj) => (
            <Card key={proj._id} className="p-4 relative group">
              <Button size="icon" variant="destructive" className="absolute top-2 right-2 h-7 w-7 opacity-0 group-hover:opacity-100 transition-opacity" onClick={() => removeItem('projects', proj._id)}><Trash2 className="h-4 w-4" /></Button>
              <div className="grid grid-cols-2 gap-3">
                <Input placeholder="Project Name" value={proj.name || ''} onChange={(e) => updateItem('projects', proj._id, 'name', e.target.value)} />
                <Input placeholder="Date" value={proj.date || ''} onChange={(e) => updateItem('projects', proj._id, 'date', e.target.value)} />
                <Input placeholder="Technologies Used" className="col-span-2" value={proj.technologies || ''} onChange={(e) => updateItem('projects', proj._id, 'technologies', e.target.value)} />
                <Textarea className="col-span-2" placeholder="Description (enter a new line for each bullet point)..." value={proj.description || ''} onChange={(e) => updateItem('projects', proj._id, 'description', e.target.value)} />
              </div>
            </Card>
          ))}
        </div>

        {/* Education */}
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Education</h2>
            <Button size="sm" variant="outline" onClick={() => addItem('education')}><Plus className="h-4 w-4 mr-1" /> Add</Button>
          </div>
          {formData.education.map((edu) => (
            <Card key={edu._id} className="p-4 relative group">
              <Button size="icon" variant="destructive" className="absolute top-2 right-2 h-7 w-7 opacity-0 group-hover:opacity-100 transition-opacity" onClick={() => removeItem('education', edu._id)}><Trash2 className="h-4 w-4" /></Button>
              <div className="grid grid-cols-2 gap-3">
                <Input placeholder="Degree" className="col-span-2" value={edu.degree || ''} onChange={(e) => updateItem('education', edu._id, 'degree', e.target.value)} />
                <Input placeholder="Institution" value={edu.school || ''} onChange={(e) => updateItem('education', edu._id, 'school', e.target.value)} />
                <Input placeholder="Date" value={edu.date || ''} onChange={(e) => updateItem('education', edu._id, 'date', e.target.value)} />
                <Input placeholder="Score (CGPA/%)" className="col-span-2" value={edu.score || ''} onChange={(e) => updateItem('education', edu._id, 'score', e.target.value)} />
              </div>
            </Card>
          ))}
        </div>

        {/* Achievements */}
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Achievements</h2>
            <Button size="sm" variant="outline" onClick={() => addItem('achievements')}><Plus className="h-4 w-4 mr-1" /> Add</Button>
          </div>
          {formData.achievements.map((ach) => (
            <div key={ach._id} className="flex items-center gap-2">
              <Input placeholder="Achievement details..." value={ach.text || ''} onChange={(e) => updateItem('achievements', ach._id, 'text', e.target.value)} />
              <Button size="icon" variant="destructive" onClick={() => removeItem('achievements', ach._id)}><Trash2 className="h-4 w-4" /></Button>
            </div>
          ))}
        </div>

        {/* Extras */}
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Extras</h2>
            <Button size="sm" variant="outline" onClick={() => addItem('extras')}><Plus className="h-4 w-4 mr-1" /> Add</Button>
          </div>
          {formData.extras.map((ex) => (
            <div key={ex._id} className="flex items-center gap-2">
              <Input placeholder="Extra activities/hobbies..." value={ex.text || ''} onChange={(e) => updateItem('extras', ex._id, 'text', e.target.value)} />
              <Button size="icon" variant="destructive" onClick={() => removeItem('extras', ex._id)}><Trash2 className="h-4 w-4" /></Button>
            </div>
          ))}
        </div>
      </div>

      {/* RIGHT COLUMN: LIVE A4 PREVIEW */}
      <div className="w-full lg:w-[55%] flex justify-center bg-secondary/30 p-4 lg:p-8 rounded-2xl print:p-0 print:bg-transparent">
        <div className="bg-white print-area text-black w-full max-w-[210mm] min-h-[297mm] px-[15mm] py-[12mm] shadow-xl print:shadow-none text-[12px] font-sans leading-relaxed" style={{ fontFamily: "Arial, sans-serif" }}>

          {/* Top Contact Row */}
          <div className="relative flex justify-between items-end mb-3 text-[11px] pt-1">
            <div className="flex flex-col text-left font-medium w-1/3">
              {formData.linkedin && <span>LinkedIn: {formData.linkedin}</span>}
              {formData.email && <span>{formData.email}</span>}
              {formData.phone && <span>{formData.phone}</span>}
            </div>
            <div className="absolute left-1/2 -translate-x-1/2 flex flex-col items-center w-1/3 text-center bottom-0">
              <h1 className="text-[22px] font-serif font-bold tracking-wider uppercase leading-tight mb-0.5">{formData.name || 'YOUR NAME'}</h1>
              <div className="text-blue-800 font-semibold text-[12px]">{formData.role || 'Software Engineer'}</div>
            </div>
            <div className="flex flex-col text-right font-medium w-1/3">
              {formData.github && <span>GitHub: {formData.github}</span>}
              {formData.leetcode && <span>LeetCode: {formData.leetcode}</span>}
              {formData.portfolio && <span>Portfolio: {formData.portfolio}</span>}
            </div>
          </div>

          {/* Experience */}
          {formData.experience.length > 0 && (
            <div className="mt-4">
              <SectionHeader title="Experience" />
              {formData.experience.map((exp) => (
                <div key={exp._id} className="mb-3">
                  <div className="flex justify-between items-baseline leading-tight mb-0.5">
                    <span className="font-bold text-[13px]">{exp.company}</span>
                    <span className="font-semibold text-[11px]">{exp.location} | {exp.date}</span>
                  </div>
                  <div className="font-medium text-[12px] italic mb-1">{exp.title}</div>
                  <ul className="list-disc leading-snug list-outside ml-4 text-[11px] space-y-0.5">
                    {exp.description?.split('\n').filter(Boolean).map((bullet, i) => (
                      <li key={i}>{bullet.replace(/^- /, '')}</li>
                    ))}
                  </ul>
                </div>
              ))}
            </div>
          )}

          {/* Education */}
          {formData.education.length > 0 && (
            <div className="mt-4">
              <SectionHeader title="Education" />
              {formData.education.map((edu) => (
                <div key={edu._id} className="mb-2">
                  <div className="flex justify-between items-baseline mb-0.5">
                    <span className="font-bold text-[13px]">{edu.degree}</span>
                    <span className="font-semibold text-[11px]">{edu.date}</span>
                  </div>
                  <div className="flex justify-between items-baseline">
                    <span className="text-[12px]">{edu.school}</span>
                    <span className="text-[11px]">{edu.score && `Score: ${edu.score}`}</span>
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Skills */}
          {baseData?.skills?.length > 0 && (
            <div className="mt-4">
              <SectionHeader title="Skills" />
              <div className="text-[12px] leading-snug">
                <span className="font-bold">Technical Skills: </span>
                <span>{baseData?.skills?.join(', ')}</span>
              </div>
            </div>
          )}

          {/* Projects */}
          {formData.projects.length > 0 && (
            <div className="mt-4">
              <SectionHeader title="Projects" />
              {formData.projects.map((proj) => (
                <div key={proj._id} className="mb-3">
                  <div className="flex justify-between items-baseline leading-tight mb-0.5">
                    <span className="font-bold text-[13px]">{proj.name}</span>
                    <span className="font-semibold text-[11px]">{proj.date}</span>
                  </div>
                  <div className="italic text-[11px] mb-1">{proj.technologies}</div>
                  <ul className="list-disc leading-snug list-outside ml-4 text-[11px] space-y-0.5">
                    {proj.description?.split('\n').filter(Boolean).map((bullet, i) => (
                      <li key={i}>{bullet.replace(/^- /, '')}</li>
                    ))}
                  </ul>
                </div>
              ))}
            </div>
          )}

          {/* Achievements */}
          {(formData.achievements.length > 0 || baseData?.certifications?.length > 0) && (
            <div className="mt-4">
              <SectionHeader title="Achievements" />
              <ul className="list-disc leading-snug list-outside ml-4 text-[11px] space-y-1">
                {baseData?.certifications?.map((cert) => (
                  <li key={cert.id} className="font-medium">{cert.name} - {cert.description} ({new Date(cert.earnedAt).getFullYear()})</li>
                ))}
                {formData.achievements.map((ach) => ach.text && (
                  <li key={ach._id}>{ach.text}</li>
                ))}
              </ul>
            </div>
          )}

          {/* Extras */}
          {formData.extras.length > 0 && (
            <div className="mt-4">
              <SectionHeader title="Extras" />
              <ul className="list-disc leading-snug list-outside ml-4 text-[11px] space-y-1">
                {formData.extras.map((ex) => ex.text && (
                  <li key={ex._id}>{ex.text}</li>
                ))}
              </ul>
            </div>
          )}

        </div>
      </div>
    </div>
  );
};

export default ResumePage;
