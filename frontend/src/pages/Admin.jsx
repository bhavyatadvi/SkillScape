import React, { useEffect, useMemo, useState, useCallback, useRef } from 'react';
import { createPortal } from 'react-dom';
import { useLocation } from 'react-router-dom';
import { toast } from 'sonner';
import { adminApi } from '@/services/api';
import { domainsApi } from '@/services/api/domains';
import { ADAPTIVE_QUESTIONS } from '@/data/adaptiveQuestionBank';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import {
  Loader2, BarChart3, ClipboardList, Map, Shield, Users,
  TrendingUp, UserCheck, Search, Activity, CheckCircle2, XCircle,
  Plus, Pencil, Trash2, X, Save, ChevronDown, ChevronUp, Hash, Award
} from 'lucide-react';
import PageHeader from '@/components/layout/PageHeader';
import { ResponsiveContainer, PieChart as RechartsPieChart, Pie, Cell, Tooltip as RechartsTooltip, Legend } from 'recharts';

// ── Route → section mapping (driven by top navbar links)
const SECTION_KEYS = ['dashboard', 'users', 'questions', 'roadmap', 'peer-groups', 'analytics'];

// ── Tiny modal wrapper
const Modal = ({ title, onClose, children, footer, size = 'max-w-2xl' }) => createPortal(
  <div className="fixed inset-0 z-[200] flex items-center justify-center overflow-y-auto bg-slate-950/45 p-3 backdrop-blur-sm sm:p-6">
    <div className={`my-auto flex max-h-[calc(100dvh-1.5rem)] min-h-0 w-full ${size} flex-col overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-2xl sm:max-h-[calc(100dvh-3rem)]`}>
      <div className="flex shrink-0 items-center justify-between border-b border-slate-200 bg-white px-5 py-4 sm:px-6">
        <h3 className="text-lg font-bold text-slate-900">{title}</h3>
        <button type="button" onClick={onClose} aria-label="Close dialog" className="rounded-lg p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-700"><X className="h-5 w-5" /></button>
      </div>
      <div className="min-h-0 flex-1 overflow-y-auto px-5 py-5 sm:px-6">{children}</div>
      {footer && <div className="shrink-0 border-t border-slate-200 bg-white px-5 py-4 sm:px-6">{footer}</div>}
    </div>
  </div>,
  document.body
);

// ── Field helpers
const Field = ({ label, children }) => (
  <div className="space-y-1.5">
    <label className="text-xs font-semibold text-slate-600 uppercase tracking-wide">{label}</label>
    {children}
  </div>
);

const AdminPage = () => {
  const location = useLocation();
  const activeSection = SECTION_KEYS.find(s => location.pathname.endsWith(`/${s}`)) || 'dashboard';

  // ── Global state
  const [loading, setLoading] = useState(true);
  const [loadErrors, setLoadErrors] = useState([]);
  const [stats, setStats]         = useState(null);
  const [users, setUsers]         = useState([]);
  const [analytics, setAnalytics] = useState(null);
  const [questions, setQuestions] = useState([]);  // Backend DB: /admin/quiz/questions
  const [adaptiveQs, setAdaptiveQs] = useState(() => JSON.parse(JSON.stringify(ADAPTIVE_QUESTIONS))); // local file, editable in-memory
  const [modules, setModules]     = useState([]);
  const [progress, setProgress]   = useState([]);
  const [domains, setDomains]     = useState([]);
  const [peerRooms, setPeerRooms] = useState([]);
  const [hierarchicalQs, setHierarchicalQs] = useState([]);
  const [hModal, setHModal] = useState(null); // for editing hierarchical qs

  // ── Search
  const [userSearch, setUserSearch] = useState('');
  const [qSearch, setQSearch]       = useState('');
  const [modSearch, setModSearch]   = useState('');

  // ── CRUD modals
  const [qModal, setQModal]     = useState(null);
  const [modModal, setModModal] = useState(null);
  const [adaptiveModal, setAdaptiveModal] = useState(null); // { index, data } for editing adaptive question

  const loadSequence = useRef(0);

  // ── Load data for the active section
  const loadData = useCallback(async () => {
    const requestId = ++loadSequence.current;
    setLoading(true);
    const asList = value => Array.isArray(value) ? value : [];
    const questionSection = activeSection === 'questions';
    const requests = questionSection
      ? [
          { label: 'quiz questions', request: adminApi.getQuizQuestions(), apply: value => setQuestions(asList(value)) },
          {
            label: 'career domains',
            request: domainsApi.getAll(),
            apply: value => setDomains(asList(value?.Data ?? value?.data ?? value)),
          },
          { label: 'career path questions', request: adminApi.getHierarchicalQuestions(), apply: value => setHierarchicalQs(asList(value)) },
        ]
      : [
          { label: 'dashboard', request: adminApi.getDashboard(), apply: setStats },
          { label: 'users', request: adminApi.getUsers(), apply: value => setUsers(asList(value)) },
          { label: 'analytics', request: adminApi.getAnalytics(), apply: setAnalytics },
          { label: 'quiz questions', request: adminApi.getQuizQuestions(), apply: value => setQuestions(asList(value)) },
          { label: 'roadmap modules', request: adminApi.getRoadmapModules(), apply: value => setModules(asList(value)) },
          { label: 'progress', request: adminApi.getProgressSnapshot(), apply: value => setProgress(asList(value)) },
          {
            label: 'career domains',
            request: domainsApi.getAll(),
            apply: value => setDomains(asList(value?.Data ?? value?.data ?? value)),
          },
          { label: 'peer groups', request: adminApi.getPeerRooms(), apply: value => setPeerRooms(asList(value)) },
          { label: 'career path questions', request: adminApi.getHierarchicalQuestions(), apply: value => setHierarchicalQs(asList(value)) },
        ];
    const results = await Promise.allSettled(requests.map(item => item.request));
    if (requestId !== loadSequence.current) return;

    const errors = [];

    results.forEach((result, index) => {
      const request = requests[index];
      if (result.status === 'rejected') {
        errors.push({ source: request.label, message: getAdminErrorMessage(result.reason, 'Request failed') });
        return;
      }

      request.apply(result.value);
    });

    setLoadErrors(errors);
    if (errors.length) {
      toast.error(`Could not load: ${errors.map(error => error.source).join(', ')}. Other data loaded successfully.`);
      console.error('Admin data requests failed:', errors);
    }
    setLoading(false);
  }, [activeSection]);

  // Save edited adaptive question back into in-memory state
  const saveAdaptiveQuestion = (index, updated) => {
    setAdaptiveQs(prev => {
      if (index === -2) return prev.filter(question => question.id !== updated);
      if (index < 0) return [...prev, updated];
      const copy = [...prev];
      copy[index] = updated;
      return copy;
    });
    setAdaptiveModal(null);
    toast.success(index === -2 ? 'Question deleted (in-memory).' : index < 0 ? 'Question added (in-memory).' : 'Question updated (in-memory).');
  };

  const exportAllQuestions = () => downloadJsonFile({
    exportedAt: new Date().toISOString(),
    adaptiveQuestions: adaptiveQs,
    domainQuestions: questions,
    careerPathQuestions: hierarchicalQs,
  }, 'skillscape-all-questions.json');

  useEffect(() => { loadData(); }, [loadData]);

  // ── Filtered lists
  const filteredUsers = useMemo(() => {
    const q = userSearch.trim().toLowerCase();
    return q ? users.filter(u => `${u.fullName} ${u.email} ${u.role}`.toLowerCase().includes(q)) : users;
  }, [userSearch, users]);

  const filteredQuestions = useMemo(() => {
    const q = qSearch.trim().toLowerCase();
    return q ? questions.filter(qn => `${qn.text} ${qn.category} ${qn.careerDomainName}`.toLowerCase().includes(q)) : questions;
  }, [qSearch, questions]);

  const filteredModules = useMemo(() => {
    const q = modSearch.trim().toLowerCase();
    return q ? modules.filter(m => `${m.title} ${m.description} ${m.careerDomainName}`.toLowerCase().includes(q)) : modules;
  }, [modSearch, modules]);

  const userRoleData = useMemo(() => countBy(users, user => user.role || 'Unknown'), [users]);
  const accountStatusData = useMemo(() => [
    { label: 'Active', value: users.filter(user => !user.isBlocked).length },
    { label: 'Blocked', value: users.filter(user => user.isBlocked).length },
  ].filter(item => item.value > 0), [users]);
  const questionSourceData = useMemo(() => [
    { label: 'Adaptive', value: adaptiveQs.length },
    { label: 'Domain', value: questions.length },
    { label: 'Career path', value: hierarchicalQs.length },
  ].filter(item => item.value > 0), [adaptiveQs, questions, hierarchicalQs]);
  // Keep the dashboard and analytics total aligned with the three question banks
  // shown in the composition chart (the adaptive bank is stored in the frontend).
  const totalQuestionCount = questionSourceData.reduce((total, source) => total + source.value, 0);
  const quizParticipationData = useMemo(() => [
    { label: 'Attempted', value: progress.filter(user => (user.quizzesTaken || 0) > 0).length },
    { label: 'No attempts', value: progress.filter(user => (user.quizzesTaken || 0) === 0).length },
  ].filter(item => item.value > 0), [progress]);

  // ── User actions
  const toggleBlock = async (user) => {
    try {
      await adminApi.blockUser({ userId: user.id, isBlocked: !user.isBlocked, reason: user.isBlocked ? null : 'Blocked by admin' });
      toast.success(user.isBlocked ? 'User unblocked' : 'User blocked');
      loadData();
    } catch (err) { toast.error(getAdminErrorMessage(err, 'Action failed')); }
  };

  const togglePeerRoomDisabled = async (room) => {
    const disabling = !room.isDisabledByAdmin;
    const reason = disabling ? (window.prompt(`Reason for disabling “${room.name}” (optional):`, 'Disabled by admin') ?? null) : null;
    if (reason === null) return;
    try {
      await adminApi.setPeerRoomModeration(room.id, { isDisabled: disabling, reason });
      setPeerRooms(current => current.map(item => item.id === room.id
        ? { ...item, isDisabledByAdmin: disabling, adminDisabledReason: disabling ? (reason.trim() || 'Disabled by admin') : null, isMeetingActive: disabling ? false : item.isMeetingActive }
        : item));
      toast.success(disabling ? 'Peer group disabled for users' : 'Peer group enabled');
    } catch (err) { toast.error(getAdminErrorMessage(err, 'Could not update peer group')); }
  };

  const updateUserRole = async (user, role) => {
    if (role === user.role) return;
    try {
      await adminApi.updateUserRole({ userId: user.id, role });
      toast.success(`Updated ${user.fullName}'s role`);
      loadData();
    } catch (err) { toast.error(getAdminErrorMessage(err, 'Role update failed')); }
  };

  // ── Question CRUD
  const saveQuestion = async (form) => {
    const payload = { ...form, text: form.text.trim(), options: form.options.filter(option => option.text.trim()).map((option, index) => ({ ...option, text: option.text.trim(), displayOrder: index })) };
    if (!payload.careerDomainId || !payload.text || payload.options.length < 2) {
      toast.error('Select a domain, enter the question, and add at least two options.');
      return;
    }
    try {
      if (qModal.mode === 'create') {
        await adminApi.createQuizQuestion(payload);
        toast.success('Question created');
      } else {
        await adminApi.updateQuizQuestion(qModal.data.id, payload);
        toast.success('Question updated');
      }
      setQModal(null);
      loadData();
    } catch (err) { toast.error(getAdminErrorMessage(err, 'Save failed')); }
  };

  const deleteQuestion = async (id) => {
    if (!window.confirm('Delete this question?')) return;
    try {
      await adminApi.deleteQuizQuestion(id);
      toast.success('Question deleted');
      loadData();
    } catch (err) { toast.error(getAdminErrorMessage(err, 'Delete failed')); }
  };

  const saveHQuestion = async (form) => {
    const payload = { ...form, questionText: form.questionText.trim(), options: form.options.filter(option => option.text.trim()).map(option => ({ ...option, text: option.text.trim() })) };
    if (!payload.questionText || payload.options.length < 2) {
      toast.error('Enter the question and at least two answer options.');
      return;
    }
    try {
      if (hModal.mode === 'create') {
        await adminApi.createHierarchicalQuestion(payload);
        toast.success('Question created');
      } else {
        await adminApi.updateHierarchicalQuestion(hModal.data.id, payload);
        toast.success('Question updated');
      }
      setHModal(null);
      loadData();
    } catch (err) { toast.error(getAdminErrorMessage(err, 'Save failed')); }
  };

  const deleteHQuestion = async (id) => {
    if (!window.confirm('Delete this question?')) return;
    try {
      await adminApi.deleteHierarchicalQuestion(id);
      toast.success('Question deleted');
      loadData();
    } catch (err) { toast.error(getAdminErrorMessage(err, 'Delete failed')); }
  };

  // ── Roadmap Module CRUD
  const saveModule = async (form) => {
    const payload = { ...form, title: form.title.trim(), description: form.description.trim(), topics: form.topics.filter(topic => topic.title.trim()).map((topic, index) => ({ ...topic, title: topic.title.trim(), displayOrder: index })) };
    if (!payload.careerDomainId || !payload.skillId || !payload.title || payload.stepNumber < 1 || payload.estimatedHours < 0) {
      toast.error('Select a domain and skill, enter a title, and use valid step and hour values.');
      return;
    }
    try {
      if (modModal.mode === 'create') {
        await adminApi.createRoadmapModule(payload);
        toast.success('Module created');
      } else {
        await adminApi.updateRoadmapModule(modModal.data.id, payload);
        toast.success('Module updated');
      }
      setModModal(null);
      loadData();
    } catch (err) { toast.error(getAdminErrorMessage(err, 'Save failed')); }
  };

  const deleteModule = async (id) => {
    if (!window.confirm('Delete this module?')) return;
    try {
      await adminApi.deleteRoadmapModule(id);
      toast.success('Module deleted');
      loadData();
    } catch (err) { toast.error(getAdminErrorMessage(err, 'Delete failed')); }
  };

  if (loading) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center flex-col gap-3">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
        <p className="text-sm text-slate-500">Loading admin data...</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <PageHeader
        icon={Shield}
        title="Admin Dashboard"
        description="Manage users, learning content, and analytics."
        action={<Button onClick={loadData} variant="outline">Refresh</Button>}
      />

      {loadErrors.length > 0 && (
        <div role="alert" className="rounded-xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-900">
          <p className="font-semibold">Some database sections could not be loaded.</p>
          <p className="mt-1">The other sections are still shown. {loadErrors.map(error => `${error.source}: ${error.message}`).join(' · ')}</p>
        </div>
      )}

      {/* ══════════ DASHBOARD ══════════ */}
      {activeSection === 'dashboard' && (
        <div className="space-y-5">
          <div className="grid gap-4 md:grid-cols-4">
            <StatCard icon={Users}        label="Total Users"       value={stats?.totalUsers ?? 0}    color="blue" />
            <StatCard icon={UserCheck}    label="Active Students"   value={stats?.activeStudents ?? 0} color="blue" />
            <StatCard icon={ClipboardList} label="Quiz Questions"   value={totalQuestionCount}        color="blue" />
            <StatCard icon={Map}          label="Roadmap Modules"   value={modules.length}             color="blue" />
          </div>
          <div className="grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader><CardTitle className="text-base flex items-center gap-2"><Activity className="h-4 w-4 text-blue-600" />Recent Users</CardTitle></CardHeader>
              <CardContent className="space-y-2">
                {users.slice(0, 6).map(u => (
                  <div key={u.id} className="flex items-center justify-between text-sm py-1">
                    <div className="flex items-center gap-2.5">
                      <div className="w-7 h-7 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center text-xs font-bold shrink-0">
                        {(u.fullName || 'U')[0].toUpperCase()}
                      </div>
                      <div>
                        <p className="font-medium text-slate-800 text-xs">{u.fullName}</p>
                        <p className="text-slate-400 text-[11px]">{u.email}</p>
                      </div>
                    </div>
                    <Badge variant={u.isBlocked ? 'destructive' : 'secondary'} className="text-[10px]">
                      {u.isBlocked ? 'Blocked' : u.role}
                    </Badge>
                  </div>
                ))}
                {users.length === 0 && <p className="text-sm text-slate-400">No users found.</p>}
              </CardContent>
            </Card>
            <Card>
              <CardHeader><CardTitle className="text-base flex items-center gap-2"><TrendingUp className="h-4 w-4 text-green-600" />Platform Analytics</CardTitle></CardHeader>
              <CardContent className="grid grid-cols-2 gap-3">
                <Metric label="Top Career Path"    value={analytics?.mostSelectedCareerPath || '—'} />
                <Metric label="Avg Quiz Score"     value={analytics?.averageQuizScore != null ? `${Number(analytics.averageQuizScore).toFixed(1)}%` : '—'} />
                <Metric label="Total Quizzes"      value={analytics?.totalQuizzesTaken ?? 0} />
                <Metric label="Active (30 days)"   value={analytics?.activeUsersLast30Days ?? 0} />
              </CardContent>
            </Card>
          </div>
        </div>
      )}

      {/* ══════════ USERS ══════════ */}
      {activeSection === 'users' && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2"><Users className="h-5 w-5" />User Management<Badge variant="outline">{users.length}</Badge></CardTitle>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
              <Input placeholder="Search by name, email or role…" value={userSearch} onChange={e => setUserSearch(e.target.value)} className="pl-9" />
            </div>
          </CardHeader>
          <CardContent>
            {filteredUsers.length === 0 && <p className="text-sm text-slate-400 py-4 text-center">No users found.</p>}
            <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-3">
              {filteredUsers.map(user => (
                <div key={user.id} className="min-w-0 rounded-xl border border-slate-200 bg-white p-4 hover:border-blue-200 hover:shadow-sm transition-all">
                  <div className="flex items-start gap-3">
                    <div className="w-10 h-10 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center font-bold text-sm shrink-0">
                      {(user.fullName || 'U')[0].toUpperCase()}
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="font-semibold text-sm text-slate-900 truncate">{user.fullName}</p>
                      <p className="text-xs text-slate-500 truncate" title={user.email}>{user.email}</p>
                      <p className="text-[11px] text-slate-400 mt-1">Joined {new Date(user.createdAt).toLocaleDateString()}</p>
                    </div>
                  </div>
                  <div className="mt-4 flex items-center justify-between gap-2 border-t border-slate-100 pt-3">
                    <div className="flex flex-wrap items-center gap-1.5">
                      <select aria-label={`Role for ${user.fullName}`} value={user.role} onChange={e => updateUserRole(user, e.target.value)} className="h-8 rounded-md border border-slate-200 bg-white px-2 text-xs text-slate-700 focus:outline-none focus:ring-2 focus:ring-blue-500">
                        <option value="Student">Student</option><option value="Admin">Admin</option>
                      </select>
                      {user.profileCompleted && <Badge variant="secondary" className="text-[10px]">Profile ✓</Badge>}
                      {user.isBlocked && <Badge variant="destructive" className="text-[10px]">Blocked</Badge>}
                    </div>
                    <Button size="sm" variant={user.isBlocked ? 'default' : 'outline'}
                      className={user.isBlocked ? 'bg-blue-600 hover:bg-blue-700 text-white' : 'text-red-600 border-red-200 hover:bg-red-50'}
                      onClick={() => toggleBlock(user)}>
                      {user.isBlocked ? <><CheckCircle2 className="w-3.5 h-3.5 mr-1"/>Unblock</> : <><XCircle className="w-3.5 h-3.5 mr-1"/>Block</>}
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* ══════════ QUESTIONS ══════════ */}
      {activeSection === 'questions' && (
        <QuestionsSection
          questions={questions}
          adaptiveQs={adaptiveQs}
          domains={domains}
          qSearch={qSearch}
          setQSearch={setQSearch}
          qModal={qModal}
          setQModal={setQModal}
          saveQuestion={saveQuestion}
          deleteQuestion={deleteQuestion}
          filteredQuestions={filteredQuestions}
          adaptiveModal={adaptiveModal}
          setAdaptiveModal={setAdaptiveModal}
          saveAdaptiveQuestion={saveAdaptiveQuestion}
          exportAllQuestions={exportAllQuestions}
          hierarchicalQs={hierarchicalQs}
          hModal={hModal}
          setHModal={setHModal}
          saveHQuestion={saveHQuestion}
          deleteHQuestion={deleteHQuestion}
        />
      )}

      {/* ══════════ PEER GROUPS ══════════ */}
      {activeSection === 'peer-groups' && (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg font-bold flex items-center gap-2"><Users className="h-5 w-5"/>Peer Learning Groups</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {peerRooms.length === 0 && <p className="text-sm text-slate-400">No peer groups found.</p>}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {peerRooms.map(room => (
                <div key={room.id} className={`border rounded-xl p-4 transition-shadow bg-white ${room.isDisabledByAdmin ? 'border-amber-300 bg-amber-50/40' : 'border-slate-200 hover:shadow-md'}`}>
                  <div className="flex justify-between items-start mb-2">
                    <h3 className="font-bold text-slate-800 leading-tight">{room.name}</h3>
                    {room.isDisabledByAdmin ? <Badge variant="outline" className="bg-amber-100 text-amber-800">Disabled by admin</Badge> : room.isPublic ? <Badge variant="outline" className="bg-green-50 text-green-700">Public</Badge> : <Badge variant="outline">Private</Badge>}
                  </div>
                  <p className="text-xs text-slate-500 mb-3 line-clamp-2">{room.description}</p>
                  <div className="space-y-1 text-xs font-medium text-slate-600">
                    <div className="flex justify-between"><span>Domain:</span> <span className="text-slate-800">{room.careerDomain}</span></div>
                    <div className="flex justify-between"><span>Members:</span> <span className="text-slate-800">{room.memberCount}</span></div>
                    <div className="flex justify-between"><span>Meeting:</span> <span className={room.isMeetingActive ? "text-red-500 font-bold" : "text-slate-800"}>{room.isMeetingActive ? 'Live Now' : 'Inactive'}</span></div>
                  </div>
                  {room.isDisabledByAdmin && room.adminDisabledReason && <p className="mt-3 text-xs text-amber-800">Reason: {room.adminDisabledReason}</p>}
                  <Button size="sm" variant={room.isDisabledByAdmin ? 'default' : 'destructive'} className="mt-4 w-full" onClick={() => togglePeerRoomDisabled(room)}>
                    {room.isDisabledByAdmin ? 'Enable group' : 'Disable group'}
                  </Button>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* ══════════ ROADMAP ══════════ */}
      {activeSection === 'roadmap' && (
        <>
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle className="flex items-center gap-2"><Map className="h-5 w-5"/>Roadmap Modules<Badge variant="outline">{modules.length} total</Badge></CardTitle>
                <Button size="sm" className="bg-blue-600 hover:bg-blue-700 text-white" onClick={() => setModModal({ mode: 'create', data: null })}>
                  <Plus className="w-4 h-4 mr-1"/>Add Module
                </Button>
              </div>
              <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
                <Input placeholder="Search by title, domain…" value={modSearch} onChange={e => setModSearch(e.target.value)} className="pl-9" />
              </div>
            </CardHeader>
            <CardContent>
              {filteredModules.length === 0 && <p className="text-sm text-slate-400 text-center py-4">No modules found.</p>}
              <div className="grid grid-cols-1 lg:grid-cols-2 2xl:grid-cols-3 gap-3">
                {filteredModules.map((m, idx) => (
                  <ModuleRow key={m.id || idx} m={m} onEdit={() => setModModal({ mode: 'edit', data: m })} onDelete={() => deleteModule(m.id)} />
                ))}
              </div>
            </CardContent>
          </Card>
          {modModal && <ModuleModal modal={modModal} domains={domains} onSave={saveModule} onClose={() => setModModal(null)} />}
        </>
      )}

      {/* ══════════ ANALYTICS ══════════ */}
      {activeSection === 'analytics' && (
        <div className="space-y-5">
          {/* Platform-level */}
          <div className="grid gap-4 md:grid-cols-3 lg:grid-cols-6">
            <Metric label="Top Career Path"   value={analytics?.totalQuizzesTaken > 0 ? analytics?.mostSelectedCareerPath : 'No quiz results'} />
            <Metric label="Least answered category" value={analytics?.weakestQuizCategory && analytics.weakestQuizCategory !== 'N/A' ? analytics.weakestQuizCategory : '0'} />
            <Metric label="Avg Quiz Score"    value={analytics?.totalQuizzesTaken > 0 ? `${Number(analytics?.averageQuizScore ?? 0).toFixed(1)}%` : 'No attempts'} />
            <Metric label="Total Quizzes"     value={analytics?.totalQuizzesTaken ?? 0} />
            <Metric label="Active Domains"    value={analytics?.totalActiveDomains ?? 0} />
            <Metric label="Questions in Bank" value={totalQuestionCount || analytics?.totalQuestionsInBank || 0} />
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <DonutChartCard title="Users by role" description="Account roles from registered users" data={userRoleData} />
            <DonutChartCard title="Account status" description="Active and blocked accounts in the database" data={accountStatusData} />
            <DonutChartCard title="Quiz participation" description="Users with at least one saved quiz result" data={quizParticipationData} />
            <DonutChartCard title="Question bank composition" description="Question counts by source" data={questionSourceData} />
          </div>

          {/* User Progress Table */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2"><BarChart3 className="h-5 w-5"/>User Progress Analytics<Badge variant="outline">{progress.length} users</Badge></CardTitle>
            </CardHeader>
            <CardContent>
              {progress.length === 0 && <p className="text-sm text-slate-400 text-center py-4">No progress data yet.</p>}
              <div className="space-y-0 divide-y divide-slate-100">
                {/* Header */}
                {progress.length > 0 && (
                  <div className="grid grid-cols-4 gap-3 px-3 py-2 text-[10px] font-black uppercase tracking-widest text-slate-400">
                    <span className="col-span-2">User</span>
                    <span>Quizzes Taken</span>
                    <span>Skills Done</span>
                  </div>
                )}
                {progress.map((p, idx) => (
                    <div key={p.userId || idx} className="grid grid-cols-4 gap-3 px-3 py-3 items-center hover:bg-slate-50 transition-colors text-sm">
                    <div className="col-span-2 flex items-center gap-2.5">
                      <div className="w-8 h-8 rounded-full bg-gradient-to-br from-blue-500 to-blue-700 text-white flex items-center justify-center text-xs font-bold shrink-0">
                        {(p.fullName || 'U')[0].toUpperCase()}
                      </div>
                      <div className="min-w-0">
                        <p className="font-semibold text-slate-800 truncate">{p.fullName}</p>
                        <p className="text-[11px] text-slate-400 truncate">{p.email}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-1">
                      <span className="font-semibold text-blue-600">{p.quizzesTaken?.toLocaleString() || 0}</span>
                      <span className="text-slate-400 text-xs ml-1">Quizzes</span>
                    </div>
                    <div className="flex items-center gap-1">
                      <Hash className="w-3.5 h-3.5 text-slate-400" />
                      <span>{p.completedSkills}</span>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>
      )}

    </div>
  );
};

// ════════════════════════════════
// Sub-components
// ════════════════════════════════

const StatCard = ({ icon: Icon, label, value, color }) => {
  const clr = { blue:'bg-blue-50 text-blue-600' };
  return (
    <Card>
      <CardContent className="p-5 flex items-center gap-4">
        <div className={`w-11 h-11 rounded-xl flex items-center justify-center shrink-0 ${clr[color] || clr.blue}`}><Icon className="w-5 h-5" /></div>
        <div><p className="text-sm text-slate-500">{label}</p><p className="mt-0.5 text-2xl font-bold text-slate-900">{value}</p></div>
      </CardContent>
    </Card>
  );
};

const Metric = ({ label, value }) => (
  <div className="rounded-xl bg-slate-50 border border-slate-100 p-4">
    <p className="text-[10px] text-slate-500 font-semibold uppercase tracking-wider">{label}</p>
    <p className="mt-1 text-xl font-bold text-slate-900 break-words">{value ?? 0}</p>
  </div>
);

const CHART_COLORS = ['#2563EB', '#3B82F6', '#60A5FA', '#93C5FD', '#1D4ED8', '#0EA5E9', '#64748B', '#BFDBFE'];
const PHASE_FILTERS = [
  { value: 'all', label: 'All stages' },
  { value: 'After10th', label: 'After 10th' },
  { value: 'After12th', label: 'After 12th' },
  { value: 'AfterGraduation', label: 'After graduation' },
  { value: 'AfterPostGraduation', label: 'After post graduation' },
  { value: 'CareerSwitch', label: 'Career switch' },
  { value: 'GovernmentJobs', label: 'Government jobs' },
  { value: 'adaptive-domain', label: 'Adaptive & Domain Questions' },
];

const normalizePhase = value => String(value || '').replace(/\s+/g, '').toLowerCase();

const DonutChartCard = ({ title, description, data }) => (
  <Card>
    <CardHeader className="pb-2">
      <CardTitle className="text-base">{title}</CardTitle>
      <p className="text-xs text-slate-500">{description}</p>
    </CardHeader>
    <CardContent>
      {data.length === 0 ? <ChartEmpty /> : (
        <div className="grid grid-cols-1 sm:grid-cols-[minmax(180px,1fr)_minmax(130px,0.8fr)] gap-2 items-center">
          <div className="h-56 min-w-0">
            <ResponsiveContainer width="100%" height="100%">
              <RechartsPieChart>
                <Pie data={data} dataKey="value" nameKey="label" innerRadius={54} outerRadius={82} paddingAngle={2}>
                  {data.map((item, index) => <Cell key={item.label} fill={CHART_COLORS[index % CHART_COLORS.length]} />)}
                </Pie>
                <RechartsTooltip formatter={(value, name) => [value, name]} />
              </RechartsPieChart>
            </ResponsiveContainer>
          </div>
          <div className="space-y-2">
            {data.map((item, index) => (
              <div key={item.label} className="flex items-center justify-between gap-3 text-xs">
                <span className="flex items-center gap-2 min-w-0 text-slate-600"><span className="w-2.5 h-2.5 rounded-sm shrink-0" style={{ backgroundColor: CHART_COLORS[index % CHART_COLORS.length] }} /><span className="truncate">{item.label}</span></span>
                <span className="font-semibold text-slate-900">{item.value}</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </CardContent>
  </Card>
);

const ChartEmpty = () => <div className="h-56 flex items-center justify-center rounded-xl bg-slate-50 text-sm text-slate-500">No database records for this chart yet.</div>;

const countBy = (items, getLabel) => {
  // `Map` is also imported above as a Lucide icon, so use the browser's
  // built-in collection explicitly here.
  const counts = new globalThis.Map();
  items.forEach(item => {
    const label = getLabel(item) || 'Unknown';
    counts.set(label, (counts.get(label) || 0) + 1);
  });
  return Array.from(counts, ([label, value]) => ({ label, value }));
};

const downloadJsonFile = (data, filename) => {
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = filename;
  anchor.click();
  window.setTimeout(() => URL.revokeObjectURL(url), 1000);
  toast.success(`Downloaded ${filename}`);
};

const getAdminErrorMessage = (error, fallback) =>
  error?.response?.data?.message ?? error?.response?.data?.Message ?? error?.message ?? fallback;

// ── Question row
const QuestionRow = ({ q, onEdit, onDelete }) => {
  const [open, setOpen] = useState(false);
  return (
    <div className="rounded-xl border border-slate-200 overflow-hidden">
      <div className="flex items-start gap-3 p-3 hover:bg-slate-50 transition-colors">
        <button onClick={() => setOpen(o => !o)} className="mt-0.5 text-slate-400 shrink-0">
          {open ? <ChevronUp className="w-4 h-4" /> : <ChevronDown className="w-4 h-4" />}
        </button>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium text-slate-800 leading-snug line-clamp-2">{q.text}</p>
          <div className="flex flex-wrap gap-1.5 mt-1.5">
            {q.careerDomainName && <Badge variant="secondary" className="text-[10px]">{q.careerDomainName}</Badge>}
            {q.category         && <Badge variant="outline"    className="text-[10px]">{q.category}</Badge>}
            {q.displayOrder > 0 && <span className="text-[10px] text-slate-400">#{q.displayOrder}</span>}
            {!q.isActive        && <Badge variant="destructive" className="text-[10px]">Inactive</Badge>}
          </div>
        </div>
        <div className="flex gap-1 shrink-0">
          <Button size="icon" variant="ghost" className="h-7 w-7 text-slate-400 hover:text-blue-600" onClick={onEdit}><Pencil className="w-3.5 h-3.5" /></Button>
          <Button size="icon" variant="ghost" className="h-7 w-7 text-slate-400 hover:text-red-600"  onClick={onDelete}><Trash2 className="w-3.5 h-3.5" /></Button>
        </div>
      </div>
      {open && q.options?.length > 0 && (
        <div className="border-t bg-slate-50 px-4 py-3 grid grid-cols-2 gap-2">
          {q.options.map((opt, i) => {
            const text = typeof opt === 'object' ? (opt.text ?? '') : String(opt);
            return (
              <div key={i} className="text-xs px-2 py-1.5 rounded-lg bg-white border border-slate-200 text-slate-600">
                {text}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};

// ── Module row
const ModuleRow = ({ m, onEdit, onDelete }) => {
  const [open, setOpen] = useState(false);
  return (
    <div className="h-full rounded-xl border border-slate-200 bg-white overflow-hidden hover:border-blue-200 hover:shadow-sm transition-all">
      <div className="flex items-start gap-3 p-3 hover:bg-slate-50 transition-colors">
        <button onClick={() => setOpen(o => !o)} className="mt-0.5 text-slate-400 shrink-0">
          {open ? <ChevronUp className="w-4 h-4" /> : <ChevronDown className="w-4 h-4" />}
        </button>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-semibold text-slate-800 leading-snug">{m.title}</p>
          <p className="text-xs text-slate-500 mt-0.5 line-clamp-1">{m.description}</p>
          <div className="flex flex-wrap gap-1.5 mt-1.5">
            {m.careerDomainName && <Badge variant="secondary" className="text-[10px]">{m.careerDomainName}</Badge>}
            {m.skillName        && <Badge variant="outline"    className="text-[10px]">{m.skillName}</Badge>}
            {m.estimatedHours   && <span className="text-[10px] text-slate-400">{m.estimatedHours}h</span>}
            {m.stepNumber > 0   && <span className="text-[10px] text-slate-400">Step {m.stepNumber}</span>}
            {!m.isActive        && <Badge variant="destructive" className="text-[10px]">Inactive</Badge>}
          </div>
        </div>
        <div className="flex gap-1 shrink-0">
          <Button size="icon" variant="ghost" className="h-7 w-7 text-slate-400 hover:text-blue-600" onClick={onEdit}><Pencil className="w-3.5 h-3.5" /></Button>
          <Button size="icon" variant="ghost" className="h-7 w-7 text-slate-400 hover:text-red-600"  onClick={onDelete}><Trash2 className="w-3.5 h-3.5" /></Button>
        </div>
      </div>
      {open && m.topics?.length > 0 && (
        <div className="border-t bg-slate-50 px-4 py-3 space-y-1">
          {m.topics.map((t, i) => (
            <div key={i} className="text-xs flex items-center gap-2 text-slate-600">
              <span className="w-4 h-4 rounded-full bg-blue-100 text-blue-600 text-[9px] font-bold flex items-center justify-center shrink-0">{i+1}</span>
              {t.title}
              {t.resourceUrl && <a href={t.resourceUrl} target="_blank" rel="noreferrer" className="text-blue-500 hover:underline text-[10px] ml-auto">Link</a>}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

// ── Question Form Modal
const QuestionModal = ({ modal, domains, onSave, onClose }) => {
  const q = modal.data;
  const [form, setForm] = useState({
    careerDomainId: q?.careerDomainId || '',
    text:           q?.text           || '',
    category:       q?.category       || '',
    displayOrder:   q?.displayOrder   || 0,
    isActive:       q?.isActive       !== false,
    options:        q?.options?.length ? q.options.map(o => typeof o === 'object' ? o : { text: o, displayOrder: 0, domainWeights: {} }) : [{ text:'', displayOrder:0, domainWeights:{} }],
  });

  const setOpt = (i, val) => {
    const opts = [...form.options];
    opts[i] = { ...opts[i], text: val };
    setForm(f => ({ ...f, options: opts }));
  };
  const addOpt = () => setForm(f => ({ ...f, options: [...f.options, { text:'', displayOrder: f.options.length, domainWeights:{} }] }));
  const removeOpt = (i) => setForm(f => ({ ...f, options: f.options.filter((_, idx) => idx !== i) }));

  return (
    <Modal title={modal.mode === 'create' ? 'Add Quiz Question' : 'Edit Quiz Question'} onClose={onClose}>
      <div className="space-y-4">
        <Field label="Career Domain">
          <select className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            value={form.careerDomainId} onChange={e => setForm(f => ({ ...f, careerDomainId: e.target.value }))}>
            <option value="">— Select Domain —</option>
            {domains.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
          </select>
        </Field>
        <Field label="Question Text">
          <textarea className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-blue-500"
            rows={3} value={form.text} onChange={e => setForm(f => ({ ...f, text: e.target.value }))} placeholder="Enter question text…" />
        </Field>
        <div className="grid grid-cols-2 gap-3">
          <Field label="Category">
            <Input value={form.category} onChange={e => setForm(f => ({ ...f, category: e.target.value }))} placeholder="e.g. Technical" />
          </Field>
          <Field label="Display Order">
            <Input type="number" value={form.displayOrder} onChange={e => setForm(f => ({ ...f, displayOrder: +e.target.value }))} />
          </Field>
        </div>
        <Field label="Options">
          <div className="space-y-2">
            {form.options.map((opt, i) => (
              <div key={i} className="flex gap-2 items-center">
                <Input value={opt.text} onChange={e => setOpt(i, e.target.value)} placeholder={`Option ${i + 1}`} className="flex-1" />
                <Button size="icon" variant="ghost" className="text-red-500 shrink-0" onClick={() => removeOpt(i)}><X className="w-4 h-4" /></Button>
              </div>
            ))}
            <Button size="sm" variant="outline" onClick={addOpt}><Plus className="w-3.5 h-3.5 mr-1" />Add Option</Button>
          </div>
        </Field>
        <div className="flex items-center gap-2">
          <input type="checkbox" id="q-active" checked={form.isActive} onChange={e => setForm(f => ({ ...f, isActive: e.target.checked }))} />
          <label htmlFor="q-active" className="text-sm text-slate-600">Active (shown to users)</label>
        </div>
        <div className="flex gap-2 pt-2 border-t">
          <Button className="bg-blue-600 hover:bg-blue-700 text-white flex-1" onClick={() => onSave(form)}>
            <Save className="w-4 h-4 mr-1.5" />Save Question
          </Button>
          <Button variant="outline" onClick={onClose}>Cancel</Button>
        </div>
      </div>
    </Modal>
  );
};

// ── Roadmap Module Form Modal
const ModuleModal = ({ modal, domains, onSave, onClose }) => {
  const m = modal.data;
  const [form, setForm] = useState({
    careerDomainId: m?.careerDomainId || '',
    skillId:        m?.skillId        || '',
    title:          m?.title          || '',
    description:    m?.description    || '',
    stepNumber:     m?.stepNumber     || 1,
    estimatedHours: m?.estimatedHours || 10,
    isActive:       m?.isActive       !== false,
    topics:         m?.topics?.length ? m.topics : [{ title:'', resourceUrl:'', displayOrder:0 }],
  });
  const [skills, setSkills] = useState([]);
  const [skillsLoading, setSkillsLoading] = useState(false);

  useEffect(() => {
    let cancelled = false;
    if (!form.careerDomainId) {
      setSkills([]);
      return undefined;
    }
    setSkillsLoading(true);
    domainsApi.getById(form.careerDomainId)
      .then(response => {
        if (cancelled) return;
        const domain = response?.Data ?? response?.data ?? response;
        const availableSkills = domain?.skills ?? domain?.Skills ?? [];
        setSkills(availableSkills);
        setForm(current => ({
          ...current,
          skillId: availableSkills.some(skill => skill.id === current.skillId)
            ? current.skillId
            : (availableSkills[0]?.id ?? '')
        }));
      })
      .catch(() => { if (!cancelled) { setSkills([]); setForm(current => ({ ...current, skillId: '' })); } })
      .finally(() => { if (!cancelled) setSkillsLoading(false); });
    return () => { cancelled = true; };
  }, [form.careerDomainId]);

  const setTopic = (i, key, val) => {
    const topics = [...form.topics];
    topics[i] = { ...topics[i], [key]: val };
    setForm(f => ({ ...f, topics }));
  };
  const addTopic    = () => setForm(f => ({ ...f, topics: [...f.topics, { title:'', resourceUrl:'', displayOrder: f.topics.length }] }));
  const removeTopic = (i) => setForm(f => ({ ...f, topics: f.topics.filter((_, idx) => idx !== i) }));

  return (
    <Modal
      title={modal.mode === 'create' ? 'Add Roadmap Module' : 'Edit Roadmap Module'}
      onClose={onClose}
      size="max-w-3xl"
      footer={(
        <div className="flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          <Button variant="outline" onClick={onClose} className="sm:min-w-28">Cancel</Button>
          <Button className="bg-blue-600 text-white hover:bg-blue-700 sm:min-w-44" onClick={() => onSave(form)}>
            <Save className="mr-1.5 h-4 w-4" />Save Module
          </Button>
        </div>
      )}
    >
      <div className="space-y-4">
        <Field label="Career Domain">
          <select className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            value={form.careerDomainId} onChange={e => setForm(f => ({ ...f, careerDomainId: e.target.value, skillId: '' }))}>
            <option value="">— Select Domain —</option>
            {domains.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
          </select>
        </Field>
        <Field label="Skill">
          <select className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-slate-50 disabled:text-slate-400"
            value={form.skillId} disabled={!form.careerDomainId || skillsLoading || skills.length === 0}
            onChange={e => setForm(f => ({ ...f, skillId: e.target.value }))}>
            <option value="">{skillsLoading ? 'Loading skills…' : skills.length ? '— Select Skill —' : 'Select a domain first'}</option>
            {skills.map(skill => <option key={skill.id} value={skill.id}>{skill.name}</option>)}
          </select>
        </Field>
        <Field label="Title">
          <Input value={form.title} onChange={e => setForm(f => ({ ...f, title: e.target.value }))} placeholder="Module title…" />
        </Field>
        <Field label="Description">
          <textarea className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-blue-500"
            rows={2} value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} placeholder="Brief description…" />
        </Field>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <Field label="Step Number">
            <Input type="number" value={form.stepNumber} onChange={e => setForm(f => ({ ...f, stepNumber: +e.target.value }))} />
          </Field>
          <Field label="Estimated Hours">
            <Input type="number" value={form.estimatedHours} onChange={e => setForm(f => ({ ...f, estimatedHours: +e.target.value }))} />
          </Field>
        </div>
        <Field label="Topics">
          <div className="space-y-2">
            {form.topics.map((t, i) => (
              <div key={i} className="grid min-w-0 grid-cols-1 items-start gap-3 rounded-xl border border-slate-200 bg-slate-50/70 p-3 sm:grid-cols-[minmax(0,1.2fr)_minmax(0,1fr)_auto]">
                <Field label={`Topic ${i + 1}`}>
                  <textarea value={t.title} onChange={e => setTopic(i, 'title', e.target.value)} placeholder="Describe the topic or learning resource" rows={2} className="w-full min-w-0 resize-y rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm leading-5 text-slate-800 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </Field>
                <Field label="Resource URL (optional)">
                  <Input type="url" value={t.resourceUrl || ''} onChange={e => setTopic(i, 'resourceUrl', e.target.value)} placeholder="https://example.com/resource" className="min-w-0 bg-white" />
                </Field>
                <Button type="button" size="icon" variant="ghost" aria-label={`Remove topic ${i + 1}`} className="mt-5 shrink-0 text-slate-400 hover:bg-red-50 hover:text-red-600" onClick={() => removeTopic(i)}><Trash2 className="h-4 w-4" /></Button>
              </div>
            ))}
            <Button type="button" size="sm" variant="outline" className="border-blue-200 text-blue-700 hover:bg-blue-50" onClick={addTopic}><Plus className="w-3.5 h-3.5 mr-1" />Add Topic</Button>
          </div>
        </Field>
        <div className="flex items-center gap-2">
          <input type="checkbox" id="mod-active" checked={form.isActive} onChange={e => setForm(f => ({ ...f, isActive: e.target.checked }))} />
          <label htmlFor="mod-active" className="text-sm text-slate-600">Active (visible in roadmap)</label>
        </div>
      </div>
    </Modal>
  );
};

export default AdminPage;

// ── QuestionsSection: 3 correct sources
const QuestionsSection = ({ questions, adaptiveQs, hierarchicalQs, domains, qSearch, setQSearch, qModal, setQModal, saveQuestion, deleteQuestion, filteredQuestions, adaptiveModal, setAdaptiveModal, saveAdaptiveQuestion, exportAllQuestions, hModal, setHModal, saveHQuestion, deleteHQuestion }) => {
  const [questionFilter, setQuestionFilter] = useState('all');
  const [domainFilter, setDomainFilter] = useState('all');
  const [phaseFilter, setPhaseFilter] = useState('all');
  const [careerPathPage, setCareerPathPage] = useState(0);
  const [expandedAdaptiveId, setExpandedAdaptiveId] = useState(null);
  const careerPathPageSize = 30;

  const splitFilterValues = value => (Array.isArray(value) ? value : String(value || '').split(/[,;|]/))
    .map(item => String(item).trim()).filter(Boolean);
  const filterQuestions = useMemo(() => [...adaptiveQs, ...questions, ...hierarchicalQs], [adaptiveQs, questions, hierarchicalQs]);
  const domainValues = useMemo(() => {
    const values = filterQuestions.flatMap(question => [
      ...splitFilterValues(question.applicableDomains || question.domains),
      ...splitFilterValues(question.careerDomainName || question.domainName),
      ...splitFilterValues(question.targetCareerTags),
      ...splitFilterValues(question.domain),
    ]);
    return [...new Set(values)].sort((a, b) => a.localeCompare(b));
  }, [filterQuestions]);
  const categoryValues = useMemo(() => {
    const values = filterQuestions.flatMap(question => [question.dimension, question.category, question.aptitudeType]);
    return [...new Set(values.filter(Boolean))].sort((a, b) => a.localeCompare(b));
  }, [filterQuestions]);

  const filteredAdaptive = useMemo(() => {
    const q = qSearch.trim().toLowerCase();
    return adaptiveQs.filter(qn => (!q || `${qn.question} ${qn.dimension} ${qn.id}`.toLowerCase().includes(q)) &&
      (questionFilter === 'all' || qn.dimension === questionFilter) &&
      (domainFilter === 'all' || splitFilterValues(qn.applicableDomains || qn.domain || qn.domains).includes(domainFilter)));
  }, [qSearch, adaptiveQs, questionFilter, domainFilter]);

  const filteredDomainQuestions = useMemo(() => filteredQuestions.filter(question =>
    (questionFilter === 'all' || question.category === questionFilter) &&
    (domainFilter === 'all' || splitFilterValues(question.careerDomainName || question.domainName || question.domain).includes(domainFilter))),
  [filteredQuestions, questionFilter, domainFilter]);

  const filteredHierarchical = useMemo(() => {
    const q = qSearch.trim().toLowerCase();
    return hierarchicalQs.filter(qn => (!q || `${qn.questionText} ${qn.streamType} ${qn.aptitudeType} ${qn.targetCareerTags}`.toLowerCase().includes(q)) &&
      (domainFilter === 'all' || splitFilterValues(qn.targetCareerTags).includes(domainFilter)) &&
      (questionFilter === 'all' || qn.aptitudeType === questionFilter) &&
      (phaseFilter === 'all' || normalizePhase(qn.streamType) === normalizePhase(phaseFilter)));
  }, [qSearch, hierarchicalQs, phaseFilter, questionFilter, domainFilter]);
  const careerPathPageCount = Math.max(1, Math.ceil(filteredHierarchical.length / careerPathPageSize));
  const visibleHierarchical = useMemo(() => filteredHierarchical.slice(
    careerPathPage * careerPathPageSize,
    (careerPathPage + 1) * careerPathPageSize,
  ), [filteredHierarchical, careerPathPage, careerPathPageSize]);

  useEffect(() => {
    setCareerPathPage(0);
  }, [qSearch, domainFilter, questionFilter, phaseFilter]);

  useEffect(() => {
    if (careerPathPage >= careerPathPageCount) {
      setCareerPathPage(Math.max(0, careerPathPageCount - 1));
    }
  }, [careerPathPage, careerPathPageCount]);
  const getFilteredCareerPathCount = phase => {
    const q = qSearch.trim().toLowerCase();
    return hierarchicalQs.filter(question =>
      (!q || `${question.questionText} ${question.streamType} ${question.aptitudeType} ${question.targetCareerTags}`.toLowerCase().includes(q)) &&
      (domainFilter === 'all' || splitFilterValues(question.targetCareerTags).includes(domainFilter)) &&
      (questionFilter === 'all' || question.aptitudeType === questionFilter) &&
      (phase === 'all' || normalizePhase(question.streamType) === normalizePhase(phase))).length;
  };
  const isAdaptiveDomainTab = phaseFilter === 'adaptive-domain';

  const exportQuestions = (data, filename) => downloadJsonFile(data, filename);

  const removeAdaptiveQuestion = (question) => {
    if (!window.confirm(`Delete adaptive question "${question.question.slice(0, 70)}"?`)) return;
    saveAdaptiveQuestion(-2, question.id);
  };

  return (
    <div className="space-y-4">
      <div className="flex flex-col md:flex-row md:items-center gap-3">
        <div className="relative w-full md:max-w-xl">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
          <Input placeholder="Search questions…" value={qSearch} onChange={e => setQSearch(e.target.value)} className="pl-9" />
        </div>
        <select aria-label="Filter questions by domain" value={domainFilter} onChange={e => setDomainFilter(e.target.value)} className="h-10 w-full md:w-52 rounded-md border border-slate-200 bg-white px-3 text-sm text-slate-700 focus:outline-none focus:ring-2 focus:ring-blue-500">
          <option value="all">All domains</option>
          {domainValues.map(value => <option key={value} value={value}>{value}</option>)}
        </select>
        <select aria-label="Filter questions by category" value={questionFilter} onChange={e => setQuestionFilter(e.target.value)} className="h-10 w-full md:w-52 rounded-md border border-slate-200 bg-white px-3 text-sm text-slate-700 focus:outline-none focus:ring-2 focus:ring-blue-500">
          <option value="all">All categories</option>
          {categoryValues.map(value => <option key={value} value={value}>{value}</option>)}
        </select>
        <Button variant="outline" className="h-10 shrink-0 border-blue-200 text-blue-700 hover:bg-blue-50" onClick={exportAllQuestions}>Export All Questions JSON</Button>
      </div>

      {/* Education-stage tabs and a combined Adaptive/Domain tab. */}
      <div className="flex flex-wrap gap-2">
        {PHASE_FILTERS.map(phase => {
          const count = phase.value === 'adaptive-domain'
            ? filteredAdaptive.length + filteredDomainQuestions.length
            : getFilteredCareerPathCount(phase.value);
          return <button key={phase.value} onClick={() => setPhaseFilter(phase.value)} className={`px-3 py-2 rounded-lg border text-xs font-semibold transition-colors ${phaseFilter === phase.value ? 'bg-blue-600 border-blue-600 text-white' : 'bg-white border-slate-200 text-slate-600 hover:border-blue-300'}`}>{phase.label}<span className={`ml-1.5 ${phaseFilter === phase.value ? 'text-blue-100' : 'text-slate-400'}`}>{count}</span></button>;
        })}
      </div>

      {/* ── Adaptive Bank (shown in the combined Adaptive & Domain tab) ── */}
      {isAdaptiveDomainTab && <>
          <div className="flex items-center justify-between gap-3">
            <span className="text-sm text-slate-500">{filteredAdaptive.length} questions shown</span>
            <div className="flex gap-2 shrink-0">
              <Button size="sm" variant="outline" className="h-8 text-xs border-blue-300 text-blue-700 hover:bg-blue-100" onClick={() => setAdaptiveModal({ index: -1, data: { id: `admin_${Date.now()}`, question: '', dimension: '', weight: 1, allowMultiple: false, options: [{ text: '', scores: {} }] } })}>
                <Plus className="w-3.5 h-3.5 mr-1" />Add Question
              </Button>
              <Button size="sm" variant="outline" className="h-8 text-xs border-blue-300 text-blue-700 hover:bg-blue-100" onClick={() => exportQuestions(adaptiveQs, 'skillscape-adaptive-questions.json')}>
                Export Adaptive JSON
              </Button>
            </div>
          </div>
          <Card>
            <CardContent className="grid grid-cols-1 xl:grid-cols-2 gap-3 pt-4">
              {filteredAdaptive.length === 0 && <p className="text-sm text-slate-400 text-center py-4">No questions match.</p>}
              {filteredAdaptive.map((q, idx) => {
                const realIdx = adaptiveQs.findIndex(x => x.id === q.id);
                return (
                  <div key={q.id || idx} className="rounded-xl border border-slate-200 bg-white overflow-hidden hover:border-blue-200 transition-colors">
                    <div className="flex items-start justify-between gap-3 p-4 hover:bg-blue-50/40 transition-colors">
                      <div className="flex-1 min-w-0 space-y-2">
                        <p className="text-sm font-medium text-slate-800 leading-snug">{q.question}</p>
                        <div className="flex flex-wrap gap-1.5 mt-1.5">
                          <Badge className="text-[10px] bg-blue-100 text-blue-700 border-0">{q.dimension}</Badge>
                          {q.weight != null && <span className="text-[10px] text-slate-400">weight: {q.weight}</span>}
                          {q.allowMultiple && <Badge variant="secondary" className="text-[10px]">Multi-select</Badge>}
                          {q.applicableDomains && <Badge variant="outline" className="text-[10px]">Domain-specific</Badge>}
                        </div>
                        <button className="text-xs font-medium text-blue-700 hover:text-blue-800" onClick={() => setExpandedAdaptiveId(current => current === q.id ? null : q.id)}>
                          {expandedAdaptiveId === q.id ? 'Hide answer details' : `Show ${q.options?.length ?? 0} answer options`}
                        </button>
                        {expandedAdaptiveId === q.id && <div className="space-y-1 pt-1">
                          {q.options?.map((opt, i) => (
                            <div key={i} className={`text-xs px-2.5 py-2 rounded-lg border ${opt.isNegative ? 'bg-red-50 border-red-100 text-red-600' : 'bg-slate-50 border-slate-100 text-slate-700'}`}>
                              <div className="font-medium">{opt.text}</div>
                              <div className="flex flex-wrap gap-1 mt-1">
                                {Object.entries(opt.scores || {}).map(([key, value]) => <span key={key} className="text-[9px] bg-white border border-slate-200 rounded px-1 whitespace-nowrap font-mono">{key}: <strong>{value}</strong></span>)}
                                {opt.isNegative && <span className="text-[9px] bg-red-100 rounded px-1">negative</span>}
                              </div>
                            </div>
                          ))}
                        </div>}
                      </div>
                      <div className="flex shrink-0">
                            <Button size="icon" variant="ghost" className="h-7 w-7 text-slate-400 hover:text-blue-600" aria-label="Edit adaptive question" onClick={() => setAdaptiveModal({ index: realIdx, data: JSON.parse(JSON.stringify(q)) })}>
                              <Pencil className="w-3.5 h-3.5" />
                            </Button>
                            <Button size="icon" variant="ghost" className="h-7 w-7 text-slate-400 hover:text-red-600" aria-label="Delete adaptive question" onClick={() => removeAdaptiveQuestion(q)}>
                              <Trash2 className="w-3.5 h-3.5" />
                            </Button>
                          </div>
                    </div>
                  </div>
                );
              })}
            </CardContent>
          </Card>
          {adaptiveModal && adaptiveModal.data && (
            <AdaptiveEditModal
              modal={adaptiveModal}
              onSave={saveAdaptiveQuestion}
              onClose={() => setAdaptiveModal(null)}
            />
          )}
      </>}

      {/* ── Domain Questions (shown with Adaptive Bank in one tab) ── */}
      {isAdaptiveDomainTab && <>
          <div className="flex items-center justify-between gap-3">
            <span className="text-sm text-slate-500">{filteredDomainQuestions.length} questions shown</span>
            <div className="flex gap-2">
              <Button size="sm" className="bg-blue-600 hover:bg-blue-700 text-white h-8 text-xs" onClick={() => setQModal({ mode: 'create', data: null })}><Plus className="w-3.5 h-3.5 mr-1" />Add Question</Button>
              <Button size="sm" variant="outline" className="h-8 text-xs border-blue-300 text-blue-700 hover:bg-blue-50" onClick={() => exportQuestions(questions, 'skillscape-domain-questions.json')}>Export Domain JSON</Button>
            </div>
          </div>
          <Card>
            <CardContent className="grid grid-cols-1 xl:grid-cols-2 gap-3 pt-4">
              {filteredDomainQuestions.length === 0 && <p className="text-sm text-slate-400 text-center py-4">No questions found.</p>}
              {filteredDomainQuestions.map((q, idx) => (
                <QuestionRow key={q.id || idx} q={q} onEdit={() => setQModal({ mode: 'edit', data: q })} onDelete={() => deleteQuestion(q.id)} />
              ))}
            </CardContent>
          </Card>
          {qModal && <QuestionModal modal={qModal} domains={domains} onSave={saveQuestion} onClose={() => setQModal(null)} />}
      </>}

      {/* ── Career Path Questions (one tab per education stage) ── */}
      {!isAdaptiveDomainTab && <>
          <div className="flex flex-col xl:flex-row xl:items-center justify-between gap-3">
            <div className="flex gap-2 shrink-0">
              <Button size="sm" className="bg-blue-600 hover:bg-blue-700 text-white h-8 text-xs" onClick={() => setHModal({ mode: 'create', data: { questionText: '', targetCareerTags: '', hierarchyLevel: 1, streamType: phaseFilter === 'all' ? 'After12th' : phaseFilter, aptitudeType: '', options: [] } })}><Plus className="w-3.5 h-3.5 mr-1" />Add Question</Button>
              <Button size="sm" variant="outline" className="h-8 text-xs border-blue-300 text-blue-700 hover:bg-blue-50" onClick={() => exportQuestions(hierarchicalQs, 'skillscape-career-path-questions.json')}>Export Career Path JSON</Button>
            </div>
          </div>
          <Card>
            <CardContent className="grid grid-cols-1 xl:grid-cols-2 2xl:grid-cols-3 gap-3 pt-4">
              {filteredHierarchical.length === 0 && <p className="text-sm text-slate-400 text-center py-4">No questions found.</p>}
              {visibleHierarchical.map((q, idx) => (
                <div key={q.id || idx} className="flex justify-between items-start p-3 border rounded-lg hover:bg-slate-50 transition-colors bg-white group">
                  <div className="space-y-1.5 flex-1 pr-4">
                    <p className="text-sm font-semibold text-slate-800 leading-tight">{q.questionText}</p>
                    <div className="flex items-center gap-2 flex-wrap">
                      <Badge variant="outline" className="text-[10px] bg-slate-100">{q.streamType}</Badge>
                      {q.aptitudeType && <Badge variant="outline" className="text-[10px] bg-blue-50 text-blue-700">{q.aptitudeType}</Badge>}
                      <span className="text-[10px] text-slate-400">{q.options?.length || 0} options</span>
                    </div>
                  </div>
                  <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                    <Button variant="ghost" size="icon" className="h-7 w-7 text-blue-600 hover:bg-blue-50" onClick={() => setHModal({ mode: 'edit', data: q })}>
                      <Pencil className="h-3.5 w-3.5" />
                    </Button>
                    <Button variant="ghost" size="icon" className="h-7 w-7 text-red-600 hover:bg-red-50" onClick={() => deleteHQuestion(q.id)}>
                      <Trash2 className="h-3.5 w-3.5" />
                    </Button>
                  </div>
                </div>
              ))}
            </CardContent>
          </Card>
          {filteredHierarchical.length > careerPathPageSize && (
            <div className="flex items-center justify-between gap-3 rounded-lg border border-slate-200 bg-white px-4 py-3">
              <span className="text-sm text-slate-500">
                Showing {careerPathPage * careerPathPageSize + 1}–{Math.min((careerPathPage + 1) * careerPathPageSize, filteredHierarchical.length)} of {filteredHierarchical.length} questions
              </span>
              <div className="flex items-center gap-2">
                <Button size="sm" variant="outline" disabled={careerPathPage === 0} onClick={() => setCareerPathPage(page => Math.max(0, page - 1))}>Previous</Button>
                <span className="min-w-20 text-center text-sm text-slate-600">Page {careerPathPage + 1} of {careerPathPageCount}</span>
                <Button size="sm" variant="outline" disabled={careerPathPage + 1 >= careerPathPageCount} onClick={() => setCareerPathPage(page => Math.min(careerPathPageCount - 1, page + 1))}>Next</Button>
              </div>
            </div>
          )}
          {hModal && <HierarchicalEditModal modal={hModal} onSave={saveHQuestion} onClose={() => setHModal(null)} />}
      </>}
    </div>
  );
};

// ── Adaptive question score editor modal
const AdaptiveEditModal = ({ modal, onSave, onClose }) => {
  const [form, setForm] = useState(() => {
    const data = JSON.parse(JSON.stringify(modal.data));
    return { ...data, weight: data.weight ?? 1 };
  });

  const setField = (key, val) => setForm(f => ({ ...f, [key]: val }));

  const setOptText  = (i, val) => setForm(f => { const o = [...f.options]; o[i] = { ...o[i], text: val }; return { ...f, options: o }; });
  const setScore    = (optIdx, dim, val) => setForm(f => {
    const o = [...f.options];
    o[optIdx] = { ...o[optIdx], scores: { ...o[optIdx].scores, [dim]: Number(val) } };
    return { ...f, options: o };
  });
  const removeScore = (optIdx, dim) => setForm(f => {
    const o = [...f.options];
    const { [dim]: _, ...rest } = o[optIdx].scores || {};
    o[optIdx] = { ...o[optIdx], scores: rest };
    return { ...f, options: o };
  });
  const addScore = (optIdx) => {
    const dim = prompt('Dimension name (e.g. leadership):');
    if (!dim) return;
    setScore(optIdx, dim.trim(), 3);
  };

  const addOption    = () => setForm(f => ({ ...f, options: [...f.options, { text: '', scores: {} }] }));
  const removeOption = (i) => setForm(f => ({ ...f, options: f.options.filter((_, idx) => idx !== i) }));

  return (
    <Modal title={`${modal.index < 0 ? 'Add' : 'Edit'} Adaptive Question`} onClose={onClose} size="max-w-4xl">
      <div className="space-y-5 text-sm">
        <Field label="Question Text">
          <textarea className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm resize-y focus:outline-none focus:ring-2 focus:ring-blue-500"
            rows={3} value={form.question} onChange={e => setField('question', e.target.value)} />
        </Field>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <Field label="Dimension"><Input value={form.dimension} onChange={e => setField('dimension', e.target.value)} /></Field>
          <Field label="Weight"><Input type="number" min="0" max="10" step="0.1" value={form.weight ?? ''} onChange={e => setField('weight', e.target.value)} /></Field>
          <Field label="Multi-select">
            <div className="flex items-center h-10">
              <input type="checkbox" checked={!!form.allowMultiple} onChange={e => setField('allowMultiple', e.target.checked)} className="mr-2" />
              <span className="text-slate-600">Allowed</span>
            </div>
          </Field>
        </div>

        <Field label="Options & Scores">
          <div className="grid grid-cols-1 xl:grid-cols-2 gap-3">
            {form.options.map((opt, i) => (
              <div key={i} className={`min-w-0 rounded-xl border p-3 space-y-3 ${opt.isNegative ? 'border-red-200 bg-red-50' : 'border-slate-200 bg-slate-50'}`}>
                <div className="grid grid-cols-[1fr_auto_auto] gap-2 items-center">
                  <Input value={opt.text} onChange={e => setOptText(i, e.target.value)} placeholder={`Option ${i + 1}`} className="min-w-0 text-sm" />
                  <label className="flex items-center gap-1 text-[11px] text-slate-500 whitespace-nowrap"><input type="checkbox" checked={!!opt.isNegative} onChange={e => setForm(f => { const o=[...f.options]; o[i]={...o[i],isNegative:e.target.checked}; return {...f,options:o}; })} />Negative</label>
                  <Button size="icon" variant="ghost" aria-label="Remove option" className="h-8 w-8 text-red-500" onClick={() => removeOption(i)}><X className="w-4 h-4"/></Button>
                </div>
                {!opt.isNegative && (
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 items-start">
                    {Object.entries(opt.scores || {}).map(([dim, val]) => (
                      <div key={dim} className="grid grid-cols-[minmax(0,1fr)_64px_28px] gap-1 items-center bg-white border border-slate-200 rounded-lg p-1.5">
                        <span className="truncate text-[11px] font-mono text-slate-600" title={dim}>{dim}</span>
                        <Input aria-label={`${dim} score`} type="number" value={val} onChange={e => setScore(i, dim, e.target.value)} className="h-7 px-2 text-xs text-center font-semibold" />
                        <button aria-label={`Remove ${dim} score`} onClick={() => removeScore(i, dim)} className="h-7 text-slate-400 hover:text-red-500">✕</button>
                      </div>
                    ))}
                    <Button size="sm" variant="outline" className="h-8 text-xs border-blue-200 text-blue-700 hover:bg-blue-50" onClick={() => addScore(i)}>+ Add score</Button>
                  </div>
                )}
              </div>
            ))}
            <Button size="sm" variant="outline" className="h-9 border-blue-200 text-blue-700 hover:bg-blue-50" onClick={addOption}><Plus className="w-3 h-3 mr-1"/>Add Option</Button>
          </div>
        </Field>

        <div className="flex gap-2 pt-2 border-t">
          <Button className="bg-blue-600 hover:bg-blue-700 text-white flex-1" onClick={() => {
            const weight = Number(form.weight);
            if (String(form.weight ?? '').trim() === '' || !Number.isFinite(weight) || weight < 0 || weight > 10) {
              toast.error('Weight must be a number between 0 and 10.');
              return;
            }
            const cleaned = { ...form, question: form.question.trim(), dimension: form.dimension.trim(), weight, options: form.options.filter(option => option.text.trim()).map(option => ({ ...option, text: option.text.trim() })) };
            if (!cleaned.question || !cleaned.dimension || cleaned.options.length === 0) { toast.error('Enter a question, dimension, and at least one option.'); return; }
            onSave(modal.index, cleaned);
          }}>
            <Save className="w-4 h-4 mr-1.5"/>Save Changes
          </Button>
          <Button variant="outline" onClick={onClose}>Cancel</Button>
        </div>
        <p className="text-[10px] text-slate-400 text-center">Changes are in-memory. Click "Export Updated JSON" to save permanently.</p>
      </div>
    </Modal>
  );
};

// ── Hierarchical Question Editor Modal
const HierarchicalEditModal = ({ modal, onSave, onClose }) => {
  const [form, setForm] = useState(JSON.parse(JSON.stringify(modal.data)));

  const setField = (key, val) => setForm(f => ({ ...f, [key]: val }));
  const setOptText = (i, val) => setForm(f => { const o = [...f.options]; o[i] = { ...o[i], text: val }; return { ...f, options: o }; });
  const setOptNextTag = (i, val) => setForm(f => { const o = [...f.options]; o[i] = { ...o[i], nextLevelTags: val }; return { ...f, options: o }; });
  
  const setWeight = (optIdx, dim, val) => setForm(f => {
    const o = [...f.options];
    o[optIdx] = { ...o[optIdx], weights: { ...o[optIdx].weights, [dim]: Number(val) } };
    return { ...f, options: o };
  });
  
  const removeWeight = (optIdx, dim) => setForm(f => {
    const o = [...f.options];
    const { [dim]: _, ...rest } = o[optIdx].weights || {};
    o[optIdx] = { ...o[optIdx], weights: rest };
    return { ...f, options: o };
  });
  
  const addWeight = (optIdx) => {
    const dim = prompt('Career/Skill tag (e.g. engineering, math):');
    if (!dim) return;
    setWeight(optIdx, dim.trim(), 3);
  };

  const addOption = () => setForm(f => ({ ...f, options: [...f.options, { text: '', weights: {}, nextLevelTags: '' }] }));
  const removeOption = (i) => setForm(f => ({ ...f, options: f.options.filter((_, idx) => idx !== i) }));

  return (
    <Modal title={`${modal.mode === 'create' ? 'Add' : 'Edit'} Career Path Question`} onClose={onClose}>
      <div className="space-y-4 text-sm">
        <Field label="Question Text">
          <textarea className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-blue-400"
            rows={3} value={form.questionText} onChange={e => setField('questionText', e.target.value)} />
        </Field>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <Field label="Stream Type (e.g. After10th)">
            <Input value={form.streamType} onChange={e => setField('streamType', e.target.value)} />
          </Field>
          <Field label="Hierarchy Level">
            <Input type="number" min="1" value={form.hierarchyLevel ?? 1} onChange={e => setField('hierarchyLevel', +e.target.value)} />
          </Field>
          <Field label="Aptitude Type / Category">
            <Input value={form.aptitudeType} onChange={e => setField('aptitudeType', e.target.value)} />
          </Field>
          <Field label="Target Career Tags">
            <Input value={form.targetCareerTags || ''} onChange={e => setField('targetCareerTags', e.target.value)} placeholder="e.g. science,engineering" />
          </Field>
        </div>

        <Field label="Options & Weights">
          <div className="space-y-3">
            {form.options.map((opt, i) => (
              <div key={i} className="rounded-xl border p-3 space-y-2 border-slate-200 bg-slate-50">
                <div className="flex gap-2 items-start">
                  <div className="flex-1 space-y-2">
                    <Input value={opt.text} onChange={e => setOptText(i, e.target.value)} placeholder="Option text" className="text-xs" />
                    <Input value={opt.nextLevelTags || ''} onChange={e => setOptNextTag(i, e.target.value)} placeholder="Next Level Tags (e.g. pcm)" className="text-[10px]" />
                  </div>
                  <Button size="icon" variant="ghost" className="h-6 w-6 text-red-400 shrink-0" onClick={() => removeOption(i)}><X className="w-3 h-3"/></Button>
                </div>
                {/* Weights grid */}
                <div className="flex flex-wrap gap-1.5 items-center">
                  {Object.entries(opt.weights || {}).map(([dim, val]) => (
                    <div key={dim} className="flex items-center gap-1 bg-white border border-slate-200 rounded-lg px-2 py-1">
                      <span className="text-[10px] font-mono text-slate-600">{dim}</span>
                      <input type="number" value={val} onChange={e => setWeight(i, dim, e.target.value)} className="w-10 text-[10px] font-bold text-center border-0 bg-transparent focus:outline-none" />
                      <button onClick={() => removeWeight(i, dim)} className="text-red-300 hover:text-red-500 text-[10px]">✕</button>
                    </div>
                  ))}
                  <Button size="sm" variant="outline" className="h-6 text-[10px] px-2" onClick={() => addWeight(i)}>+ tag</Button>
                </div>
              </div>
            ))}
            <Button size="sm" variant="outline" onClick={addOption}><Plus className="w-3 h-3 mr-1"/>Add Option</Button>
          </div>
        </Field>

        <div className="flex gap-2 pt-2 border-t">
          <Button className="bg-blue-700 hover:bg-blue-800 text-white flex-1" onClick={() => onSave(form)}>
            <Save className="w-4 h-4 mr-1.5"/>Save to DB
          </Button>
          <Button variant="outline" onClick={onClose}>Cancel</Button>
        </div>
      </div>
    </Modal>
  );
};
