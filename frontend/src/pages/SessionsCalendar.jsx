import React, { useState, useEffect, useMemo } from 'react';
import { Calendar, Clock, Video, CheckCircle, XCircle, AlertCircle, Edit, Trash2, Plus, ChevronLeft, ChevronRight } from 'lucide-react';
import { sessionsApi, mentorsApi } from '@/services/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';

const SessionsCalendar = () => {
  const { user } = useAuth();
  const [sessions, setSessions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('all');
  const [assignedStudents, setAssignedStudents] = useState([]);
  const [creatingSession, setCreatingSession] = useState(false);
  const [editingSessionId, setEditingSessionId] = useState(null);
  const [calendarMonth, setCalendarMonth] = useState(() => {
    const now = new Date();
    return new Date(now.getFullYear(), now.getMonth(), 1);
  });

  const [createForm, setCreateForm] = useState({
    studentId: '',
    scheduledDate: '',
    scheduledTime: '',
    durationMinutes: 60,
    notes: '',
    meetingLink: '',
  });

  const [editFormData, setEditFormData] = useState({
    scheduledDateTime: '',
    durationMinutes: 60,
    notes: '',
  });

  const parseSessionDateTime = (value) => {
    if (!value) return new Date('');
    if (value instanceof Date) return value;

    const raw = String(value);
    if (raw.endsWith('Z')) {
      return new Date(raw.slice(0, -1));
    }

    return new Date(raw);
  };

  const formatForDateTimeLocalInput = (value) => {
    const date = parseSessionDateTime(value);
    if (Number.isNaN(date.getTime())) return '';

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  };

  useEffect(() => {
    fetchAllData();
  }, [user?.id]);

  const fetchAllData = async () => {
    try {
      setLoading(true);
      const sessionsResponse = await sessionsApi.getMySessions();
      setSessions(sessionsResponse.data || []);

      if (user?.role === 'Mentor') {
        try {
          const dashboardResponse = await mentorsApi.getDashboard();
          setAssignedStudents(dashboardResponse?.data?.assignedStudents || []);
        } catch (error) {
          setAssignedStudents([]);
        }
      }
    } catch (err) {
      console.error('Failed to fetch sessions:', err);
      toast.error('Failed to load sessions');
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadge = (status) => {
    const statusStyles = {
      Pending: 'bg-yellow-100 text-yellow-800 border-yellow-300',
      Approved: 'bg-blue-100 text-blue-800 border-blue-300',
      Completed: 'bg-green-100 text-green-800 border-green-300',
      Cancelled: 'bg-red-100 text-red-800 border-red-300',
    };

    const statusIcons = {
      Pending: AlertCircle,
      Approved: CheckCircle,
      Completed: CheckCircle,
      Cancelled: XCircle,
    };

    const Icon = statusIcons[status] || AlertCircle;
    const style = statusStyles[status] || 'bg-gray-100 text-gray-800';

    return (
      <Badge className={`${style} border flex items-center gap-1`}>
        <Icon className="w-3 h-3" />
        {status}
      </Badge>
    );
  };

  const getFilteredSessions = () => {
    const now = new Date();

    switch (filter) {
      case 'upcoming':
        return sessions.filter(
          (session) => parseSessionDateTime(session.scheduledDateTime) > now && session.status !== 'Cancelled' && session.status !== 'Completed'
        );
      case 'past':
        return sessions.filter(
          (session) =>
            parseSessionDateTime(session.scheduledDateTime) <= now ||
            session.status === 'Completed' ||
            session.status === 'Cancelled'
        );
      default:
        return sessions;
    }
  };

  const handleCreateSession = async () => {
    if (!createForm.studentId || !createForm.scheduledDate || !createForm.scheduledTime) {
      toast.error('Please select a student, date and time');
      return;
    }

    const composedDateTime = `${createForm.scheduledDate}T${createForm.scheduledTime}:00`;

    try {
      setCreatingSession(true);
      await sessionsApi.createMentorSession({
        studentId: createForm.studentId,
        scheduledDateTime: composedDateTime,
        durationMinutes: Number(createForm.durationMinutes) || 60,
        notes: createForm.notes || null,
        meetingLink: createForm.meetingLink || null,
      });

      toast.success('Session created successfully');
      setCreateForm({ studentId: '', scheduledDate: '', scheduledTime: '', durationMinutes: 60, notes: '', meetingLink: '' });
      await fetchAllData();
    } catch (error) {
      toast.error(error?.response?.data?.message || 'Failed to create session');
    } finally {
      setCreatingSession(false);
    }
  };

  const handleApproveSession = async (sessionId, approve) => {
    try {
      await sessionsApi.approveSession({
        sessionId,
        approve,
        meetingLink: approve ? prompt('Enter meeting link (optional)') || null : null,
      });
      toast.success(approve ? 'Session approved' : 'Session declined');
      await fetchAllData();
    } catch (error) {
      toast.error(error?.response?.data?.message || 'Failed to update session status');
    }
  };

  const handleEditClick = (session) => {
    setEditingSessionId(session.id);
    setEditFormData({
      scheduledDateTime: formatForDateTimeLocalInput(session.scheduledDateTime),
      durationMinutes: session.durationMinutes,
      notes: session.notes || '',
    });
  };

  const handleUpdateSession = async (sessionId) => {
    try {
      await sessionsApi.updateSession(sessionId, {
        scheduledDateTime: editFormData.scheduledDateTime ? `${editFormData.scheduledDateTime}:00` : null,
        durationMinutes: Number(editFormData.durationMinutes) || 60,
        notes: editFormData.notes,
      });
      toast.success('Session updated successfully');
      setEditingSessionId(null);
      await fetchAllData();
    } catch (error) {
      toast.error(error?.response?.data?.message || 'Failed to update session');
    }
  };

  const handleDeleteSession = async (sessionId) => {
    if (!window.confirm('Cancel this session?')) return;

    try {
      await sessionsApi.deleteSession(sessionId);
      toast.success('Session cancelled');
      await fetchAllData();
    } catch (error) {
      toast.error(error?.response?.data?.message || 'Failed to cancel session');
    }
  };

  const filteredSessions = getFilteredSessions();
  const upcomingCount = sessions.filter(
    (s) => parseSessionDateTime(s.scheduledDateTime) > new Date() && s.status !== 'Cancelled' && s.status !== 'Completed'
  ).length;
  const pastCount = sessions.filter(
    (s) => parseSessionDateTime(s.scheduledDateTime) <= new Date() || s.status === 'Completed' || s.status === 'Cancelled'
  ).length;

  const nextUpcomingSession = useMemo(() => {
    const now = new Date();
    return sessions
      .filter((session) => parseSessionDateTime(session.scheduledDateTime) > now && session.status !== 'Cancelled' && session.status !== 'Completed')
      .sort((a, b) => parseSessionDateTime(a.scheduledDateTime) - parseSessionDateTime(b.scheduledDateTime))[0] || null;
  }, [sessions]);

  const dateKey = (date) => {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  };

  const sessionsByDate = useMemo(() => {
    return sessions.reduce((acc, session) => {
      const key = dateKey(parseSessionDateTime(session.scheduledDateTime));
      if (!acc[key]) acc[key] = [];
      acc[key].push(session);
      return acc;
    }, {});
  }, [sessions]);

  const calendarCells = useMemo(() => {
    const year = calendarMonth.getFullYear();
    const month = calendarMonth.getMonth();
    const firstDay = new Date(year, month, 1);
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const leadingBlanks = firstDay.getDay();

    const cells = [];
    for (let index = 0; index < leadingBlanks; index += 1) {
      cells.push(null);
    }
    for (let day = 1; day <= daysInMonth; day += 1) {
      cells.push(new Date(year, month, day));
    }
    while (cells.length % 7 !== 0) {
      cells.push(null);
    }
    return cells;
  }, [calendarMonth]);

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-50 dark:bg-slate-900 p-6 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-cyan-500 mx-auto mb-4"></div>
          <p className="text-slate-600 dark:text-slate-400">Loading sessions...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-900 p-6">
      <div className="max-w-6xl mx-auto">
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-slate-900 dark:text-white flex items-center gap-3">
            <Calendar className="w-8 h-8 text-cyan-500" />
            My Sessions
          </h1>
          <p className="text-slate-600 dark:text-slate-400 mt-2">
            {user?.role === 'Mentor' ? 'Manage your mentoring sessions and appointments' : 'View your scheduled mentoring sessions'}
          </p>
        </div>

        {user?.role === 'Mentor' && (
          <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 mb-6">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Plus className="w-5 h-5 text-cyan-500" />
                Create Session
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                <select
                  className="w-full p-2 border border-slate-300 dark:border-slate-600 rounded-md bg-white dark:bg-slate-900"
                  value={createForm.studentId}
                  onChange={(e) => setCreateForm((prev) => ({ ...prev, studentId: e.target.value }))}
                >
                  <option value="">Select student</option>
                  {assignedStudents.map((student) => (
                    <option key={student.id} value={student.id}>{student.name} ({student.email})</option>
                  ))}
                </select>

                <input
                  type="date"
                  className="w-full p-2 border border-slate-300 dark:border-slate-600 rounded-md bg-white dark:bg-slate-900"
                  value={createForm.scheduledDate}
                  onChange={(e) => setCreateForm((prev) => ({ ...prev, scheduledDate: e.target.value }))}
                />

                <input
                  type="time"
                  className="w-full p-2 border border-slate-300 dark:border-slate-600 rounded-md bg-white dark:bg-slate-900"
                  value={createForm.scheduledTime}
                  onChange={(e) => setCreateForm((prev) => ({ ...prev, scheduledTime: e.target.value }))}
                />

                <input
                  type="number"
                  min="15"
                  max="480"
                  className="w-full p-2 border border-slate-300 dark:border-slate-600 rounded-md bg-white dark:bg-slate-900"
                  value={createForm.durationMinutes}
                  onChange={(e) => setCreateForm((prev) => ({ ...prev, durationMinutes: e.target.value }))}
                  placeholder="Duration (minutes)"
                />

                <input
                  type="text"
                  className="w-full p-2 border border-slate-300 dark:border-slate-600 rounded-md bg-white dark:bg-slate-900"
                  value={createForm.meetingLink}
                  onChange={(e) => setCreateForm((prev) => ({ ...prev, meetingLink: e.target.value }))}
                  placeholder="Meeting link (optional)"
                />
              </div>

              <textarea
                className="w-full p-2 mt-3 border border-slate-300 dark:border-slate-600 rounded-md bg-white dark:bg-slate-900"
                rows={2}
                value={createForm.notes}
                onChange={(e) => setCreateForm((prev) => ({ ...prev, notes: e.target.value }))}
                placeholder="Notes (optional)"
              />

              <Button
                className="mt-3 bg-cyan-500 hover:bg-cyan-600"
                onClick={handleCreateSession}
                disabled={creatingSession}
              >
                {creatingSession ? 'Creating...' : 'Create Session'}
              </Button>
            </CardContent>
          </Card>
        )}

        <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 mb-6">
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <Calendar className="w-5 h-5 text-cyan-500" />
              Session Calendar
            </CardTitle>
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="icon"
                onClick={() => setCalendarMonth((prev) => new Date(prev.getFullYear(), prev.getMonth() - 1, 1))}
              >
                <ChevronLeft className="w-4 h-4" />
              </Button>
              <span className="min-w-[170px] text-center font-medium text-slate-900 dark:text-white">
                {calendarMonth.toLocaleDateString('en-US', { month: 'long', year: 'numeric' })}
              </span>
              <Button
                variant="outline"
                size="icon"
                onClick={() => setCalendarMonth((prev) => new Date(prev.getFullYear(), prev.getMonth() + 1, 1))}
              >
                <ChevronRight className="w-4 h-4" />
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-7 gap-2 text-xs mb-2 text-slate-500 dark:text-slate-400">
              {['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].map((day) => (
                <div key={day} className="text-center font-medium py-1">{day}</div>
              ))}
            </div>
            <div className="grid grid-cols-7 gap-2">
              {calendarCells.map((cell, index) => {
                if (!cell) {
                  return <div key={`blank-${index}`} className="h-16 rounded-md bg-slate-50 dark:bg-slate-900/40" />;
                }

                const key = dateKey(cell);
                const daySessions = sessionsByDate[key] || [];
                const isHighlighted = daySessions.length > 0;
                const statuses = [...new Set(daySessions.map((session) => session.status))];

                const dayColorClass = (() => {
                  if (!isHighlighted) {
                    return 'bg-white border-slate-200 dark:bg-slate-800 dark:border-slate-700';
                  }

                  if (statuses.length > 1) {
                    return 'bg-violet-50 border-violet-300 dark:bg-violet-900/20 dark:border-violet-700';
                  }

                  const onlyStatus = statuses[0];
                  if (onlyStatus === 'Approved') {
                    return 'bg-blue-50 border-blue-300 dark:bg-blue-900/20 dark:border-blue-700';
                  }
                  if (onlyStatus === 'Pending') {
                    return 'bg-amber-50 border-amber-300 dark:bg-amber-900/20 dark:border-amber-700';
                  }
                  if (onlyStatus === 'Completed') {
                    return 'bg-emerald-50 border-emerald-300 dark:bg-emerald-900/20 dark:border-emerald-700';
                  }
                  if (onlyStatus === 'Cancelled') {
                    return 'bg-rose-50 border-rose-300 dark:bg-rose-900/20 dark:border-rose-700';
                  }

                  return 'bg-cyan-50 border-cyan-300 dark:bg-cyan-900/20 dark:border-cyan-700';
                })();

                const dayTextColorClass = (() => {
                  if (!isHighlighted) {
                    return 'text-slate-900 dark:text-white';
                  }

                  if (statuses.length > 1) {
                    return 'text-violet-700 dark:text-violet-300';
                  }

                  const onlyStatus = statuses[0];
                  if (onlyStatus === 'Approved') return 'text-blue-700 dark:text-blue-300';
                  if (onlyStatus === 'Pending') return 'text-amber-700 dark:text-amber-300';
                  if (onlyStatus === 'Completed') return 'text-emerald-700 dark:text-emerald-300';
                  if (onlyStatus === 'Cancelled') return 'text-rose-700 dark:text-rose-300';

                  return 'text-cyan-700 dark:text-cyan-300';
                })();

                const content = (
                  <div
                    className={`h-16 rounded-md border p-1.5 flex flex-col justify-between transition-colors ${dayColorClass}`}
                  >
                    <span className="text-xs font-semibold text-slate-900 dark:text-white">{cell.getDate()}</span>
                    {isHighlighted && (
                      <span className={`text-[10px] font-medium ${dayTextColorClass}`}>
                        {daySessions.length} session{daySessions.length > 1 ? 's' : ''}
                      </span>
                    )}
                  </div>
                );

                if (!isHighlighted) {
                  return <div key={key}>{content}</div>;
                }

                return (
                  <Tooltip key={key}>
                    <TooltipTrigger asChild>{content}</TooltipTrigger>
                    <TooltipContent side="top" className="max-w-xs bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-700 shadow-lg rounded-lg p-3">
                      <div className="space-y-2 text-xs">
                        <p className="font-semibold text-slate-900 dark:text-slate-100 border-b border-slate-200 dark:border-slate-700 pb-1">{cell.toLocaleDateString()}</p>
                        {daySessions.slice(0, 4).map((session) => (
                          <p key={session.id} className="bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-md px-2 py-1 text-slate-700 dark:text-slate-200">
                            {(user?.role === 'Mentor' ? session.studentName : session.mentorName) || 'Session'} •{' '}
                            {parseSessionDateTime(session.scheduledDateTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} • {session.durationMinutes} min
                          </p>
                        ))}
                        {daySessions.length > 4 && <p className="text-slate-600 dark:text-slate-300">+{daySessions.length - 4} more</p>}
                      </div>
                    </TooltipContent>
                  </Tooltip>
                );
              })}
            </div>
          </CardContent>
        </Card>

        <div className="flex gap-2 mb-6">
          <Button variant={filter === 'all' ? 'default' : 'outline'} onClick={() => setFilter('all')} className={filter === 'all' ? 'bg-cyan-500 hover:bg-cyan-600' : ''}>All ({sessions.length})</Button>
          <Button variant={filter === 'upcoming' ? 'default' : 'outline'} onClick={() => setFilter('upcoming')} className={filter === 'upcoming' ? 'bg-cyan-500 hover:bg-cyan-600' : ''}>Upcoming ({upcomingCount})</Button>
          <Button variant={filter === 'past' ? 'default' : 'outline'} onClick={() => setFilter('past')} className={filter === 'past' ? 'bg-cyan-500 hover:bg-cyan-600' : ''}>Past ({pastCount})</Button>
        </div>

        {filteredSessions.length === 0 ? (
          <Card className="bg-white dark:bg-slate-800">
            <CardContent className="py-12 text-center">
              <Calendar className="w-16 h-16 text-slate-300 dark:text-slate-600 mx-auto mb-4" />
              <p className="text-slate-600 dark:text-slate-400 text-lg">No sessions found</p>
              <p className="text-slate-500 dark:text-slate-500 text-sm mt-2">
                {filter === 'all' ? 'You do not have any sessions scheduled yet.' : `No ${filter} sessions found.`}
              </p>
            </CardContent>
          </Card>
        ) : (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            {filteredSessions.map((session) => (
              <Card key={session.id} className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 hover:shadow-lg transition-shadow h-full">
                <CardContent className="p-6">
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-2">
                        <h3 className="text-lg font-semibold text-slate-900 dark:text-white">
                          {user?.role === 'Mentor' ? `Session with ${session.studentName}` : `Session with ${session.mentorName}`}
                        </h3>
                        {getStatusBadge(session.status)}
                      </div>

                      <div className="flex items-center gap-4 text-sm text-slate-600 dark:text-slate-400 mb-3">
                        <div className="flex items-center gap-1">
                          <Calendar className="w-4 h-4" />
                          {parseSessionDateTime(session.scheduledDateTime).toLocaleDateString()}
                        </div>
                        <div className="flex items-center gap-1">
                          <Clock className="w-4 h-4" />
                          {parseSessionDateTime(session.scheduledDateTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                          <span>({session.durationMinutes} min)</span>
                        </div>
                      </div>

                      {session.notes && <p className="text-sm text-slate-700 dark:text-slate-300 mb-3"><span className="font-medium">Notes:</span> {session.notes}</p>}

                      {session.meetingLink && session.status === 'Approved' && (
                        <a href={session.meetingLink} target="_blank" rel="noopener noreferrer" className="inline-flex items-center gap-2 px-4 py-2 bg-cyan-500 text-white rounded-lg hover:bg-cyan-600 transition-colors mb-3">
                          <Video className="w-4 h-4" />
                          Join Meeting
                        </a>
                      )}

                      {user?.role === 'Mentor' && session.status === 'Pending' && (
                        <div className="flex gap-2 mt-2">
                          <Button size="sm" className="bg-green-600 hover:bg-green-700 text-white" onClick={() => handleApproveSession(session.id, true)}>Approve</Button>
                          <Button size="sm" variant="outline" className="border-red-300 text-red-600 hover:bg-red-50" onClick={() => handleApproveSession(session.id, false)}>Decline</Button>
                        </div>
                      )}

                      {user?.role === 'Mentor' && (
                        <div className="flex gap-2 mt-3">
                          <Button size="sm" variant="outline" className="flex items-center gap-1" onClick={() => handleEditClick(session)}>
                            <Edit className="w-4 h-4" />
                            Edit
                          </Button>
                          <Button size="sm" variant="outline" className="border-red-300 text-red-600 hover:bg-red-50 flex items-center gap-1" onClick={() => handleDeleteSession(session.id)}>
                            <Trash2 className="w-4 h-4" />
                            Delete
                          </Button>
                        </div>
                      )}

                      {user?.role === 'Mentor' && editingSessionId === session.id && (
                        <div className="mt-4 p-3 bg-slate-100 dark:bg-slate-700 rounded-lg space-y-2">
                          <input
                            type="datetime-local"
                            className="w-full p-2 border border-slate-300 dark:border-slate-600 rounded-md bg-white dark:bg-slate-900"
                            value={editFormData.scheduledDateTime}
                            onChange={(e) => setEditFormData((prev) => ({ ...prev, scheduledDateTime: e.target.value }))}
                          />
                          <input
                            type="number"
                            min="15"
                            max="480"
                            className="w-full p-2 border border-slate-300 dark:border-slate-600 rounded-md bg-white dark:bg-slate-900"
                            value={editFormData.durationMinutes}
                            onChange={(e) => setEditFormData((prev) => ({ ...prev, durationMinutes: e.target.value }))}
                          />
                          <textarea
                            className="w-full p-2 border border-slate-300 dark:border-slate-600 rounded-md bg-white dark:bg-slate-900"
                            rows={2}
                            value={editFormData.notes}
                            onChange={(e) => setEditFormData((prev) => ({ ...prev, notes: e.target.value }))}
                          />
                          <div className="flex gap-2">
                            <Button size="sm" className="bg-blue-600 hover:bg-blue-700 text-white" onClick={() => handleUpdateSession(session.id)}>Save</Button>
                            <Button size="sm" variant="outline" onClick={() => setEditingSessionId(null)}>Cancel</Button>
                          </div>
                        </div>
                      )}
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default SessionsCalendar;
