import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Filter, X } from 'lucide-react';

const domains = ['Technology', 'Data & Analytics', 'Business', 'Finance', 'Management', 'Design', 'Engineering', 'Healthcare', 'Research', 'Government / Public Sector', 'Communication'];
const levels = ['Beginner', 'Intermediate', 'Advanced'];
const durations = ['Hours', 'Days', 'Weeks', 'Months'];

export default function CourseFilters({ filters, onFilterChange, onClearFilters }) {
  const choose = (key, value) => onFilterChange({ ...filters, [key]: filters[key] === value ? (key === 'isFree' ? undefined : '') : value });
  const FilterGroup = ({ label, filterKey, options }) => <div><p className="mb-2 text-sm font-semibold text-slate-700">{label}</p><div className="flex flex-wrap gap-1.5">{options.map(option => <button key={option} type="button" onClick={() => choose(filterKey, option)} className={`rounded-md border px-2.5 py-1.5 text-xs font-medium transition-colors ${filters[filterKey] === option ? 'border-blue-600 bg-blue-50 text-blue-700' : 'border-slate-200 bg-white text-slate-600 hover:border-blue-300 hover:bg-blue-50 hover:text-blue-700'}`}>{option}</button>)}</div></div>;
  return <Card className="w-full">
    <CardHeader className="flex flex-row items-center justify-between pb-4"><CardTitle className="flex items-center gap-2 text-lg"><Filter className="h-5 w-5 text-blue-600" />Filters</CardTitle><Button variant="ghost" size="sm" className="h-8 px-2 text-xs" onClick={onClearFilters}><X className="mr-1 h-3.5 w-3.5" />Clear all</Button></CardHeader>
    <CardContent className="space-y-5"><FilterGroup label="Domain / Field" filterKey="domain" options={domains} /><FilterGroup label="Difficulty level" filterKey="level" options={levels} /><FilterGroup label="Duration" filterKey="duration" options={durations} /><div className="border-t pt-4"><p className="mb-2 text-sm font-semibold text-slate-700">Course options</p><div className="flex flex-wrap gap-1.5"><button type="button" onClick={() => choose('isFree', true)} className={`rounded-md border px-2.5 py-1.5 text-xs font-medium ${filters.isFree ? 'border-blue-600 bg-blue-50 text-blue-700' : 'border-slate-200 text-slate-600 hover:border-blue-300 hover:bg-blue-50'}`}>Free only</button><button type="button" onClick={() => choose('type', 'Certificate')} className={`rounded-md border px-2.5 py-1.5 text-xs font-medium ${filters.type ? 'border-blue-600 bg-blue-50 text-blue-700' : 'border-slate-200 text-slate-600 hover:border-blue-300 hover:bg-blue-50'}`}>Certificate available</button></div></div></CardContent>
  </Card>;
}
