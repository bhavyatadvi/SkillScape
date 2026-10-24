import React, { useState, useEffect, useMemo } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { 
  DollarSign, MapPin, Award, ArrowLeft, Info, Loader2, BrainCircuit,
  Sparkles, Layers, Search, BookOpen, Clock, Building, TrendingUp
} from 'lucide-react';
import { careerGuidanceApi } from '@/services/api/careerGuidance';
import { roadmapApi } from '@/services/api/roadmap';
import { useAuth } from '@/context/AuthContext';
import { Button } from '@/components/ui/button';
import PageHeader from '@/components/layout/PageHeader';
import { toast } from 'sonner';
import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend, BarChart, Bar
} from 'recharts';

export default function SalaryOutlook() {
  const { user } = useAuth();
  const [searchParams] = useSearchParams();
  const initialPathId = searchParams.get('id');

  const [careerPaths, setCareerPaths] = useState([]);
  const [selectedPathId, setSelectedPathId] = useState(initialPathId || '');
  const [selectedCustomTitle, setSelectedCustomTitle] = useState('');
  const [searchQuery, setSearchQuery] = useState('');

  const [outlook, setOutlook] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isPathsLoading, setIsPathsLoading] = useState(true);

  // Core Parameters
  const [educationTrack, setEducationTrack] = useState('Graduation'); // 'Graduation' or 'PostGraduation'
  const [graduationYear, setGraduationYear] = useState(new Date().getFullYear() + 2);
  const [postGraduationYear, setPostGraduationYear] = useState(new Date().getFullYear() + 4);
  const [locationTier, setLocationTier] = useState('Tier 1'); // Tier 1, Tier 2, Tier 3
  const [skillReadiness, setSkillReadiness] = useState('Intermediate'); // Beginner, Intermediate, Advanced

  useEffect(() => {
    const fetchAllPaths = async () => {
      try {
        const streams = [
          'After10th', 'After12th', 'AfterGraduation', 'AfterPostGraduation',
          'CareerSwitch', 'GovernmentJobs'
        ];
        
        const promises = streams.map(stream => careerGuidanceApi.getPathsTree(stream).catch(() => []));
        const results = await Promise.all(promises);

        const list = [];
        const seenIds = new Set();
        const flatten = (arr) => {
          if (!arr || !Array.isArray(arr)) return;
          arr.forEach(p => {
            // Filter out structural nodes from taxonomy
            const titleLower = (p.title || '').toLowerCase();
            const isStructural = 
              titleLower.includes('advanced professional practice') ||
              titleLower.includes('core field') ||
              titleLower.includes('core domain') ||
              titleLower.includes('advanced studies') ||
              titleLower.includes('specialization');

            if (p.id && p.title && !isStructural && !seenIds.has(p.id)) {
              seenIds.add(p.id);
              list.push({ id: p.id, title: p.title });
            }
            if (p.subCareers) flatten(p.subCareers);
          });
        };
        
        results.forEach(res => flatten(res));
        list.sort((a, b) => a.title.localeCompare(b.title));
        setCareerPaths(list);

        if (!initialPathId && list.length > 0) {
          setSelectedPathId(list[0].id);
        }
      } catch (err) {
        console.error('Failed to load career paths list', err);
        toast.error('Failed to load career paths list');
      } finally {
        setIsPathsLoading(false);
      }
    };
    fetchAllPaths();
  }, [initialPathId]);

  const displayedPaths = useMemo(() => {
    if (searchQuery.trim()) {
      const query = searchQuery.trim().toLowerCase();
      return careerPaths.filter(path => path.title.toLowerCase().includes(query));
    }
    return careerPaths;
  }, [careerPaths, searchQuery]);

  const runForecast = async () => {
    if (!selectedPathId) return;
    setIsLoading(true);
    try {
      const data = await careerGuidanceApi.forecastSalary({
        careerPathId: selectedCustomTitle || selectedPathId,
        educationTrack,
        graduationYear,
        postGraduationYear: educationTrack === 'PostGraduation' ? postGraduationYear : null,
        country: 'India',
        locationTier,
        skillReadiness
      });
      setOutlook(data);
    } catch (err) {
      toast.error('Failed to calculate salary outlook');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    runForecast();
  }, [selectedPathId, educationTrack, graduationYear, postGraduationYear, locationTier, skillReadiness]);

  const formatINR = (val) => {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 0
    }).format(val);
  };

  const tooltipStyle = { backgroundColor: '#ffffff', borderColor: '#e2e8f0', borderRadius: '8px', color: '#0f172a' };
  const tooltipLabelStyle = { color: '#0f172a', fontSize: '12px', fontWeight: 'bold' };

  return (
    <div className="w-full pb-16 bg-slate-50 min-h-screen">
      <div className="max-w-7xl mx-auto px-4 py-8">
        
        <PageHeader 
          icon={TrendingUp}
          eyebrow={
            <Link to="/dashboard" className="inline-flex items-center text-sm font-semibold text-slate-500 hover:text-blue-600 transition-colors">
              <ArrowLeft className="w-4 h-4 mr-2" /> Back to Dashboard
            </Link>
          }
          title="Market Salary Outlook"
          description="Explore realistic, data-driven salary benchmarks based on current market data and skill readiness."
        />

        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 mt-8">
          
          {/* Controls Panel (Left side) */}
          <div className="lg:col-span-4 space-y-6">
            <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm">
              <h3 className="font-bold text-lg text-slate-900 flex items-center gap-2 mb-6 pb-4 border-b border-slate-100">
                <BrainCircuit className="w-5 h-5 text-blue-600" /> Outlook Parameters
              </h3>

              {/* Career Selector */}
              <div className="space-y-3 mb-6">
                <label htmlFor="career-search" className="text-xs font-bold text-slate-500 uppercase tracking-wider block">Search Occupation</label>
                <div className="relative">
                  <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                  <input
                    id="career-search"
                    type="search"
                    value={searchQuery}
                    onChange={event => {
                      setSearchQuery(event.target.value);
                      if (event.target.value.trim() !== '') {
                        setSelectedCustomTitle(event.target.value);
                      } else {
                        setSelectedCustomTitle('');
                      }
                    }}
                    placeholder="e.g. Software Engineer"
                    className="w-full bg-slate-50 border border-slate-200 rounded-xl pl-9 pr-3 py-3 text-sm focus:outline-none focus:border-blue-600 focus:ring-1 focus:ring-blue-600 transition-all"
                  />
                </div>
                {isPathsLoading ? (
                  <div className="h-12 bg-slate-50 border border-slate-200 rounded-xl flex items-center justify-center">
                    <Loader2 className="w-5 h-5 text-blue-600 animate-spin" />
                  </div>
                ) : (
                  <select
                    value={selectedCustomTitle ? "custom-search" : selectedPathId}
                    onChange={(e) => {
                      if (e.target.value === "custom-search") {
                         // already handled by input
                      } else {
                        setSelectedPathId(e.target.value);
                        setSelectedCustomTitle('');
                        setSearchQuery('');
                      }
                    }}
                    className="w-full bg-white border border-slate-200 rounded-xl px-4 py-3 text-sm font-medium text-slate-900 focus:outline-none focus:border-blue-600 focus:ring-1 focus:ring-blue-600 transition-colors cursor-pointer"
                  >
                    {selectedCustomTitle && (
                      <option value="custom-search">
                        Search Market Data for: "{selectedCustomTitle}"
                      </option>
                    )}
                    {selectedPathId && !selectedCustomTitle && !displayedPaths.some(path => path.id === selectedPathId) && (
                      <option value={selectedPathId} disabled>
                        {outlook?.careerTitle || 'Current career selection'}
                      </option>
                    )}
                    {displayedPaths.map((p) => (
                      <option key={p.id} value={p.id}>
                        {p.title}
                      </option>
                    ))}
                  </select>
                )}
              </div>

              {/* Education Track */}
              <div className="space-y-3 mb-6">
                <label className="text-xs font-bold text-slate-500 uppercase tracking-wider flex items-center gap-2">
                  <BookOpen className="w-4 h-4 text-blue-600" /> Education Track
                </label>
                <div className="grid grid-cols-2 gap-2 bg-slate-100 p-1 rounded-xl">
                  {['Graduation', 'PostGraduation'].map((track) => (
                    <button
                      key={track}
                      onClick={() => setEducationTrack(track)}
                      className={`py-2 px-3 text-sm rounded-lg font-semibold transition-all duration-200 ${
                        educationTrack === track
                          ? 'bg-white text-blue-700 shadow-sm'
                          : 'text-slate-600 hover:text-slate-900'
                      }`}
                    >
                      {track === 'PostGraduation' ? 'Post-Grad' : 'Graduation'}
                    </button>
                  ))}
                </div>
              </div>

              {/* Expected Graduation Year Slider */}
              <div className="space-y-4 mb-6">
                <div className="flex justify-between items-center">
                  <label className="text-xs font-bold text-slate-500 uppercase tracking-wider block">Graduation Year</label>
                  <span className="text-sm font-black text-slate-900">{graduationYear}</span>
                </div>
                <input
                  type="range"
                  min="2024"
                  max="2030"
                  value={graduationYear}
                  onChange={(e) => setGraduationYear(parseInt(e.target.value))}
                  className="w-full h-2 bg-slate-200 rounded-lg appearance-none cursor-pointer accent-blue-600"
                />
              </div>

              {educationTrack === 'PostGraduation' && (
                <div className="space-y-4 mb-6">
                  <div className="flex justify-between items-center">
                    <label className="text-xs font-bold text-slate-500 uppercase tracking-wider block">PG Completion Year</label>
                    <span className="text-sm font-black text-slate-900">{postGraduationYear}</span>
                  </div>
                  <input
                    type="range"
                    min={graduationYear + 1}
                    max={graduationYear + 5}
                    value={postGraduationYear}
                    onChange={(e) => setPostGraduationYear(parseInt(e.target.value))}
                    className="w-full h-2 bg-slate-200 rounded-lg appearance-none cursor-pointer accent-purple-600"
                  />
                </div>
              )}

              {/* Geographic Location Tier */}
              <div className="space-y-3 mb-6">
                <label className="text-xs font-bold text-slate-500 uppercase tracking-wider flex items-center gap-2">
                  <MapPin className="w-4 h-4 text-blue-600" /> Geographic Zone
                </label>
                <div className="grid grid-cols-3 gap-2">
                  {['Tier 1', 'Tier 2', 'Tier 3'].map((loc) => (
                    <button
                      key={loc}
                      onClick={() => setLocationTier(loc)}
                      className={`py-2 px-2 text-xs rounded-xl font-bold border transition-all duration-200 ${
                        locationTier === loc
                          ? 'bg-blue-50 border-blue-600 text-blue-700 shadow-sm'
                          : 'bg-white border-slate-200 text-slate-600 hover:border-slate-300'
                      }`}
                    >
                      {loc}
                    </button>
                  ))}
                </div>
              </div>

              {/* Target Skill Readiness */}
              <div className="space-y-3">
                <label className="text-xs font-bold text-slate-500 uppercase tracking-wider flex items-center gap-2">
                  <Award className="w-4 h-4 text-blue-600" /> Skill Readiness
                </label>
                <div className="flex flex-col gap-2">
                  {['Beginner', 'Intermediate', 'Advanced'].map((lvl) => (
                    <button
                      key={lvl}
                      onClick={() => setSkillReadiness(lvl)}
                      className={`py-2.5 px-4 text-sm rounded-xl font-semibold border transition-all duration-200 text-left flex justify-between items-center ${
                        skillReadiness === lvl
                          ? 'bg-blue-50 border-blue-600 text-blue-700 shadow-sm'
                          : 'bg-white border-slate-200 text-slate-600 hover:border-slate-300'
                      }`}
                    >
                      {lvl}
                      {skillReadiness === lvl && <div className="w-2 h-2 rounded-full bg-blue-600"></div>}
                    </button>
                  ))}
                </div>
              </div>

            </div>
          </div>

          {/* Outlook Display (Right Side) */}
          <div className="lg:col-span-8 space-y-6">
            
            {isLoading && !outlook ? (
              <div className="h-[400px] bg-white border border-slate-200 rounded-2xl flex items-center justify-center shadow-sm">
                <div className="flex flex-col items-center gap-3">
                  <Loader2 className="w-8 h-8 text-blue-600 animate-spin" />
                  <p className="text-sm font-medium text-slate-500">Retrieving market benchmarks...</p>
                </div>
              </div>
            ) : outlook ? (
              <>
                {/* Primary Card */}
                <div className="bg-white border border-slate-200 rounded-3xl p-8 text-slate-900 shadow-sm relative overflow-hidden">
                  <div className="absolute top-0 right-0 w-64 h-64 bg-blue-50 rounded-full opacity-50 blur-3xl -translate-y-1/2 translate-x-1/3"></div>
                  
                  <div className="relative z-10 flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
                    <div>
                      <div className="flex items-center gap-2 mb-3">
                        <span className="text-xs font-bold tracking-wider uppercase bg-blue-50 text-blue-600 px-3 py-1 rounded-full border border-blue-100">
                          {outlook.educationTrack}
                        </span>
                        <span className={`text-xs font-bold tracking-wider uppercase px-3 py-1 rounded-full border ${outlook.dataQuality === 'High' ? 'bg-emerald-50 text-emerald-600 border-emerald-100' : 'bg-amber-50 text-amber-600 border-amber-100'}`}>
                          {outlook.dataQuality} Confidence
                        </span>
                      </div>
                      <h2 className="text-3xl font-black mb-2 text-slate-900">
                        {(outlook.careerTitle || '').replace(/^role-/, '').replace(/-/g, ' ').replace(/\b\w/g, l => l.toUpperCase())}
                      </h2>
                      <p className="text-slate-500 font-medium flex items-center gap-2">
                        <MapPin className="w-4 h-4 text-slate-400" /> {locationTier}, {outlook.country || 'India'} • {skillReadiness} Readiness
                      </p>
                    </div>
                    
                    <div className="bg-slate-50 border border-slate-100 p-5 rounded-2xl min-w-[240px]">
                      <span className="text-xs uppercase font-bold text-slate-500 block mb-1">Median Base (Entry)</span>
                      <p className="text-4xl font-black text-blue-600">
                        {formatINR(outlook.benchmark.p50)}
                      </p>
                      <div className="mt-3 flex justify-between text-xs font-medium text-slate-500">
                        <span>P25: {formatINR(outlook.benchmark.p25)}</span>
                        <span className="font-bold text-slate-700">P50: {formatINR(outlook.benchmark.p50)}</span>
                        <span>P75: {formatINR(outlook.benchmark.p75)}</span>
                      </div>
                    </div>
                  </div>
                </div>

                {/* 5-Year Trajectory Chart */}
                <div className="bg-white border border-slate-200 rounded-3xl p-8 shadow-sm">
                  <div className="mb-6">
                    <h4 className="font-bold text-xl text-slate-900">5-Year Compensation Trajectory</h4>
                    <p className="text-sm text-slate-500 mt-1">Projected median base pay progression based on market bands.</p>
                  </div>

                  <div className="h-[350px] w-full">
                    <ResponsiveContainer width="100%" height="100%">
                      <AreaChart
                        data={outlook.projection || []}
                        margin={{ top: 20, right: 20, left: 0, bottom: 0 }}
                      >
                        <defs>
                          <linearGradient id="colorP50" x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor="#2563EB" stopOpacity={0.3}/>
                            <stop offset="95%" stopColor="#2563EB" stopOpacity={0}/>
                          </linearGradient>
                        </defs>
                        <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" vertical={false} />
                        <XAxis 
                          dataKey="calendarYear" 
                          stroke="#94A3B8" 
                          fontSize={12} 
                          tickLine={false} 
                          axisLine={false} 
                          dy={10} 
                          tickFormatter={(val, i) => `Year ${i} (${val})`}
                        />
                        <YAxis 
                          stroke="#94A3B8" 
                          fontSize={12} 
                          tickFormatter={(val) => `₹${(val / 100000).toFixed(0)}L`}
                          tickLine={false}
                          axisLine={false}
                          dx={-10}
                        />
                        <Tooltip 
                          contentStyle={tooltipStyle}
                          formatter={(value, name) => [formatINR(value), name === 'p50' ? 'Median (P50)' : name]}
                          labelStyle={tooltipLabelStyle}
                          labelFormatter={(val) => `Calendar Year: ${val}`}
                        />
                        <Area 
                          name="p50" 
                          type="monotone" 
                          dataKey="p50" 
                          stroke="#2563EB" 
                          strokeWidth={3}
                          fillOpacity={1} 
                          fill="url(#colorP50)" 
                          activeDot={{ r: 6, strokeWidth: 0, fill: '#2563EB' }}
                        />
                        <Area type="monotone" dataKey="p25" stroke="#94A3B8" strokeWidth={1} strokeDasharray="4 4" fill="none" name="Lower Band (P25)" />
                        <Area type="monotone" dataKey="p75" stroke="#10B981" strokeWidth={1} strokeDasharray="4 4" fill="none" name="Upper Band (P75)" />
                      </AreaChart>
                    </ResponsiveContainer>
                  </div>
                </div>

                {/* Year-by-Year Breakdown Table */}
                <div className="bg-white border border-slate-200 rounded-3xl p-8 shadow-sm overflow-hidden">
                  <h4 className="font-bold text-lg text-slate-900 mb-6">Year-by-Year Breakdown</h4>
                  <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                      <thead>
                        <tr className="border-b border-slate-200 text-xs uppercase tracking-wider text-slate-500">
                          <th className="pb-4 font-bold">Experience</th>
                          <th className="pb-4 font-bold">Year</th>
                          <th className="pb-4 font-bold text-right">Lower Band (P25)</th>
                          <th className="pb-4 font-bold text-right text-blue-700">Median (P50)</th>
                          <th className="pb-4 font-bold text-right">Upper Band (P75)</th>
                        </tr>
                      </thead>
                      <tbody className="text-sm font-medium">
                        {(outlook.projection || []).map((point, idx) => (
                          <tr key={idx} className="border-b border-slate-100 last:border-0 hover:bg-slate-50 transition-colors">
                            <td className="py-4 text-slate-900">Year {point.experienceYear}</td>
                            <td className="py-4 text-slate-500">{point.calendarYear}</td>
                            <td className="py-4 text-right text-slate-600">{formatINR(point.p25)}</td>
                            <td className="py-4 text-right text-blue-700 font-bold bg-blue-50/50 px-2">{formatINR(point.p50)}</td>
                            <td className="py-4 text-right text-emerald-600">{formatINR(point.p75)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>


              </>
            ) : (
              <div className="h-[400px] flex items-center justify-center text-slate-500">
                Select a career path to view the salary outlook.
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
