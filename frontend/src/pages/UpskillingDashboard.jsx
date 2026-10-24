import React, { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { Award, BookOpen, CheckCircle2, Library, PlayCircle, Search, Sparkles, TrendingUp } from 'lucide-react';
import PageHeader from '@/components/layout/PageHeader';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
import CourseFilters from '@/components/upskilling/CourseFilters';
import CourseCard from '@/components/upskilling/CourseCard';
import CourseDetailsModal from '@/components/upskilling/CourseDetailsModal';
import { courseRepository, matchCourse } from '@/services/courseRepository';
import { upskillingDomains } from '@/data/upskillingCatalog';

const initialFilters = { domain: '', level: '', type: '', isFree: undefined, maxPrice: null, duration: '' };

export default function UpskillingDashboard() {
  const [activeTab, setActiveTab] = useState('discover');
  const [filters, setFilters] = useState(initialFilters);
  const [query, setQuery] = useState('');
  const [savedIds, setSavedIds] = useState(() => courseRepository.savedIds());
  const [selectedCourse, setSelectedCourse] = useState(null);
  const [sortBy, setSortBy] = useState('recommended');
  const [courseProgress, setCourseProgress] = useState(() => courseRepository.progress());
  const [catalogNotice, setCatalogNotice] = useState(false);
  const localContext = useMemo(() => courseRepository.context(), []);
  const [backendContext, setBackendContext] = useState(null);

  const targetCareer = backendContext?.targetCareer?.name;
  const skillsToDisplay = backendContext?.recommendedSkills?.map(s => s.skillName) || [];



  const [courses, setCourses] = useState([]);
  
  useEffect(() => {
    let active = true;
    const fetchCourses = async () => {
      let data = [];
      try {
        if (activeTab === 'saved') data = await courseRepository.saved();

        else if (activeTab === 'certifications') data = await courseRepository.certifications();
        else if (activeTab === 'completed') data = courseRepository.completed();
        else data = await courseRepository.discover({ ...filters, query });
      } catch (err) {
        console.error("Failed to load courses", err);
      }
      if (active) {
        if (activeTab !== 'discover') {
          data = data.filter(course => matchCourse(course, query, filters));
        }
        setCourses(data);
      }
    };
    fetchCourses();
    return () => { active = false; };
  }, [activeTab, filters, query, savedIds]);
  
  const displayedCourses = useMemo(() => [...courses].sort((a, b) => sortBy === 'shortest' ? a.durationValue - b.durationValue : sortBy === 'free' ? Number(b.isFree) - Number(a.isFree) : (b.recommendationScore || 0) - (a.recommendationScore || 0)), [courses, sortBy]);

  const clearFilters = () => { setFilters(initialFilters); setQuery(''); };
  const toggleSave = id => setSavedIds(courseRepository.toggleSaved(id));
  const updateProgress = (id, status) => setCourseProgress(courseRepository.setProgress(id, status));
  const chooseDomain = domain => { setFilters(previous => ({ ...previous, domain: previous.domain === domain ? '' : domain })); setActiveTab('discover'); };

  return <div className="max-w-[1600px] px-4 lg:px-8 mx-auto space-y-6 pb-12">
    <PageHeader title="Upskilling & Certifications" subtitle="Build the skills, credentials and projects needed for your next career move." />

    <div className="grid grid-cols-1 gap-8 lg:grid-cols-4 lg:gap-10">
      <aside className="space-y-5 lg:col-span-1 sticky top-6 h-fit"><CourseFilters filters={filters} onFilterChange={setFilters} onClearFilters={clearFilters} /><div className="rounded-xl border bg-white p-5"><h3 className="flex items-center gap-2 font-semibold text-slate-800"><TrendingUp className="h-4 w-4 text-blue-600" /> Popular skills</h3><p className="mt-1 text-xs text-slate-500">Frequently associated with selected career paths.</p><div className="mt-4 flex flex-wrap gap-2">{['Excel', 'SQL', 'Python', 'Power BI', 'Communication', 'Project Management'].map(skill => <button key={skill} className="rounded-md bg-slate-100 px-2.5 py-1 text-xs text-slate-600 hover:bg-blue-50 hover:text-blue-700" onClick={() => { setQuery(skill); setActiveTab('discover'); }}>{skill}</button>)}</div></div></aside>
      <main className="space-y-5 lg:col-span-3"><div className="flex flex-col gap-3 rounded-xl border bg-white p-2 sm:flex-row sm:items-center sm:justify-between"><Tabs value={activeTab} onValueChange={setActiveTab}><TabsList className="grid w-full grid-cols-4"><TabsTrigger value="discover">Discover</TabsTrigger><TabsTrigger value="certifications">Certificates</TabsTrigger><TabsTrigger value="completed"><CheckCircle2 className="mr-1 h-3.5 w-3.5" />Completed</TabsTrigger><TabsTrigger value="saved"><Library className="mr-1 h-3.5 w-3.5" />Saved</TabsTrigger></TabsList></Tabs><div className="relative w-full sm:w-72"><Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" /><Input value={query} onChange={event => { setQuery(event.target.value); setActiveTab('discover'); }} className="pl-9" placeholder="Search courses, skills, providers..." /></div></div>
        <div className="flex items-center justify-between"><p className="text-sm text-slate-500">{displayedCourses.length} {displayedCourses.length === 1 ? 'course' : 'courses'}</p><select value={sortBy} onChange={event => setSortBy(event.target.value)} className="rounded-md border border-slate-200 bg-white px-2 py-1 text-sm text-slate-600"><option value="recommended">Most relevant</option><option value="shortest">Shortest duration</option><option value="free">Free first</option></select></div>
        {displayedCourses.length ? <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">{displayedCourses.map(course => <div key={course.id} className="relative cursor-pointer h-full" onClick={() => setSelectedCourse(course)}><CourseCard course={course} isSaved={savedIds.has(course.id)} onSave={toggleSave} progress={courseProgress[course.id]} onProgressUpdate={updateProgress} showCertification={activeTab === 'certifications'} /></div>)}</div> : <div className="flex min-h-[300px] flex-col items-center justify-center rounded-xl border border-dashed bg-white px-6 text-center"><BookOpen className="h-9 w-9 text-slate-300" /><h3 className="mt-4 font-semibold text-slate-800">Nothing matches all your filters</h3><p className="mt-1 text-sm text-slate-500">Try another keyword, clear filters, or browse a field.</p><Button variant="outline" className="mt-5" onClick={clearFilters}>Clear filters</Button></div>}
      </main>
    </div>
    <CourseDetailsModal course={selectedCourse} open={Boolean(selectedCourse)} onOpenChange={open => !open && setSelectedCourse(null)} />
  </div>;
}
