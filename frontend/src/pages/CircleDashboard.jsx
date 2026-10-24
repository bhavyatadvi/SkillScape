import React, { useState, useEffect, useRef } from 'react';
import { toast } from 'sonner';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiClient, getAuthToken } from '../services/api';
import * as signalR from '@microsoft/signalr';
import {
  Brain, ArrowLeft, Users, MessageSquare, 
  MonitorUp, FileText, CheckSquare, Sparkles, TrendingUp,
  Video, Download, ThumbsUp, Send, Loader2, Target, Edit3, X
} from 'lucide-react';
import { Button } from '../components/ui/button';
import { Badge } from '../components/ui/badge';
import { Input } from '../components/ui/input';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../components/ui/tabs';
import { LiveVideoSession } from '../components/LiveVideoSession';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';


const CircleDashboard = () => {
  const { circleId } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  const chatEndRef = useRef(null);
  
  const [circle, setCircle] = useState(null);
  const [loading, setLoading] = useState(true);
  const [joinRequests, setJoinRequests] = useState([]);
  const [scheduleTime, setScheduleTime] = useState('');
  
  // Discussions State
  const [discussions, setDiscussions] = useState([]);
  const [newDiscussionContent, setNewDiscussionContent] = useState('');
  
  // Live Chat State
  const [newMessage, setNewMessage] = useState('');

  // Resources State
  const [resources, setResources] = useState([]);
  const [newResourceTitle, setNewResourceTitle] = useState('');
  const [newResourceUrl, setNewResourceUrl] = useState('');

  // Meeting State
  const [inMeeting, setInMeeting] = useState(false);

  // Members State
  const [members, setMembers] = useState([]);

  // Tasks State
  const [tasks, setTasks] = useState([]);

  // Notes State
  const [noteContent, setNoteContent] = useState('');
  const [whiteboardContent, setWhiteboardContent] = useState('');
  
  // AI Mentor State
  const [aiMentorMessages, setAiMentorMessages] = useState([
    { role: 'ai', content: 'Hello! I\'m your AI Mentor for this circle. How can I assist your learning today? You can ask me to explain a concept or create a study plan!' }
  ]);
  const [newAiMentorMessage, setNewAiMentorMessage] = useState('');
  const [aiMentorLoading, setAiMentorLoading] = useState(false);

  // Progress Board State
  const [progressBoard, setProgressBoard] = useState([]);

  // Overview State
  const [overview, setOverview] = useState(null);
  const [expandedNoteId, setExpandedNoteId] = useState(null);
  const [roomChats, setRoomChats] = useState([]);

  const [dashboardConnection, setDashboardConnection] = useState(null);

  const isCreator = user && circle && circle.creatorId === user.id;

  useEffect(() => {
    let newConnection;
    const setupSignalR = async () => {
        const token = getAuthToken();
        const chatHubUrl = import.meta.env.VITE_API_URL
          ? import.meta.env.VITE_API_URL.replace('/api', '/hubs/chat')
          : '/hubs/chat';

        newConnection = new signalR.HubConnectionBuilder()
          .withUrl(chatHubUrl, { accessTokenFactory: () => token })
          .withAutomaticReconnect()
          .build();

        try {
            await newConnection.start();
            await newConnection.invoke("JoinCircleGroup", circleId);
            setDashboardConnection(newConnection);

            newConnection.on("ReceiveMessage", (msg) => {
                setDiscussions(prev => {
                    if (prev.some(d => d.id === msg.id)) return prev;
                    return [...prev, msg];
                });
                setTimeout(() => chatEndRef.current?.scrollIntoView({ behavior: 'smooth' }), 100);
            });

            newConnection.on("ReceiveResource", (res) => {
                setResources(prev => {
                    if (prev.some(r => r.id === res.id)) return prev;
                    return [res, ...prev];
                });
            });

            newConnection.on("NotesUpdated", (note) => {
                setNoteContent(note.content);
            });

            newConnection.on("ReceiveWhiteboard", (content, senderConnId) => {
                if (newConnection.connectionId !== senderConnId) {
                    setWhiteboardContent(content);
                }
            });

            newConnection.on("ReceiveTask", (task) => {
                setTasks(prev => {
                    if (prev.some(t => t.id === task.id)) return prev;
                    return [task, ...prev];
                });
            });

            newConnection.on("TaskUpdated", (data) => {
                setTasks(prev => prev.map(t => t.id === data.taskId ? { ...t, status: data.status } : t));
            });

            newConnection.on("SessionEnded", () => {
                setCircle(prev => prev ? { ...prev, isMeetingActive: false } : prev);
                setDiscussions([]);
                setWhiteboardContent('');
                // We don't save notes to resources here because only the creator should do it
                // and we already do it in toggleMeeting for the creator.
            });

            newConnection.on("ReceiveRoomChat", (msg) => {
                setRoomChats(prev => {
                    if (prev.some(d => d.id === msg.id)) return prev;
                    return [...prev, msg];
                });
            });

        } catch (err) {
            console.error("Dashboard SignalR error", err);
        }
    };
    if (circleId) setupSignalR();

    return () => {
        if (newConnection) {
            newConnection.invoke("LeaveCircleGroup", circleId).catch(() => {});
            newConnection.stop();
        }
    };
  }, [circleId]);

  useEffect(() => {
    const fetchCircle = async () => {
      try {
        const response = await apiClient.get(`/learningcircles/${circleId}`);
        setCircle(response.data);
        
        if (user && response.data.creatorId === user.id) {
          const reqResponse = await apiClient.get(`/learningcircles/${circleId}/requests`);
          setJoinRequests(reqResponse.data);
        }

        // Fetch Discussions
        try {
          const discResponse = await apiClient.get(`/learningcircles/${circleId}/discussions`);
          setDiscussions(discResponse.data);
        } catch (discErr) {
          console.error("Failed to fetch discussions", discErr);
        }

        // Fetch Resources
        try {
          const resResponse = await apiClient.get(`/learningcircles/${circleId}/resources`);
          setResources(resResponse.data);
        } catch (resErr) {
          console.error("Failed to fetch resources", resErr);
        }

        // Fetch Members
        try {
          const memResponse = await apiClient.get(`/learningcircles/${circleId}/members`);
          setMembers(memResponse.data);
        } catch (memErr) {
          console.error("Failed to fetch members", memErr);
        }

        // Fetch Tasks
        try {
          const taskResponse = await apiClient.get(`/learningcircles/${circleId}/tasks`);
          setTasks(taskResponse.data);
        } catch (err) {
          console.error("Failed to fetch tasks", err);
        }

        // Fetch Notes
        try {
          const notesResponse = await apiClient.get(`/learningcircles/${circleId}/notes`);
          setNoteContent(notesResponse.data?.content || '');
        } catch (err) {
          console.error("Failed to fetch notes", err);
        }

        // Fetch Progress Board
        try {
          const progressResponse = await apiClient.get(`/learningcircles/${circleId}/progress`);
          setProgressBoard(progressResponse.data);
        } catch (err) {
          console.error("Failed to fetch progress board", err);
        }

        // Fetch Overview
        try {
          const overviewResponse = await apiClient.get(`/learningcircles/${circleId}/overview`);
          setOverview(overviewResponse.data);
        } catch (err) {
          console.error("Failed to fetch overview", err);
        }

      } catch (err) {
        console.error("Error fetching circle details", err);
      } finally {
        setLoading(false);
      }
    };
    fetchCircle();
  }, [circleId, user]);

  const handleProcessRequest = async (reqId, approve) => {
    try {
      await apiClient.post(`/learningcircles/requests/${reqId}/process?approve=${approve}`);
      setJoinRequests(joinRequests.filter(r => r.id !== reqId));
      toast.success(approve ? 'Request approved' : 'Request rejected');
    } catch (error) {
      toast.error("Failed to process request");
    }
  };

  const toggleMeeting = async () => {
    try {
      const newStatus = !circle.isMeetingActive;
      await apiClient.post(`/learningcircles/${circleId}/meeting-status?isActive=${newStatus}`);
      setCircle({ ...circle, isMeetingActive: newStatus });
      toast.success(newStatus ? 'Meeting started' : 'Meeting ended');
      
      if (!newStatus) {
        // Meeting ended
        setRoomChats([]);
        
        // Save Room Notes as a new Shared Note Resource
        if (noteContent && noteContent.trim() !== '') {
            try {
                const response = await apiClient.post(`/learningcircles/${circleId}/resources`, { 
                    title: `Meeting Notes - ${new Date().toLocaleDateString()}`, 
                    url: noteContent 
                });
                setResources(prev => [response.data, ...prev]);
                setNoteContent('');
            } catch (err) {
                console.error("Failed to save room notes as resource");
            }
        }
        
        // Clear whiteboard
        setWhiteboardContent('');
      }
    } catch (error) {
      toast.error("Failed to toggle meeting");
    }
  };

  const downloadWhiteboard = () => {
    if (!whiteboardContent) return;
    const element = document.createElement("a");
    const file = new Blob([whiteboardContent], {type: 'text/plain'});
    element.href = URL.createObjectURL(file);
    element.download = `Whiteboard_${new Date().toLocaleDateString()}.txt`;
    document.body.appendChild(element); // Required for this to work in FireFox
    element.click();
    document.body.removeChild(element);
  };

  const handleScheduleMeeting = async () => {
    if (!scheduleTime) {
      toast.error("Please select a date and time.");
      return;
    }
    try {
      const utcDate = new Date(scheduleTime).toISOString();
      await apiClient.post(`/learningcircles/${circleId}/schedule`, { meetingTime: utcDate });
      toast.success("Live session scheduled! Members have been notified.");
      setCircle({ ...circle, scheduledMeetingTime: utcDate });
      setScheduleTime('');
    } catch (error) {
      toast.error("Failed to schedule meeting");
    }
  };

  const handleCreateDiscussion = async () => {
    if (!newDiscussionContent.trim()) return;
    try {
      const response = await apiClient.post(`/learningcircles/${circleId}/discussions`, { content: newDiscussionContent });
      setDiscussions([...discussions, response.data]);
      setNewDiscussionContent('');
      toast.success('Message posted');
    } catch (error) {
      toast.error('Failed to post message');
    }
  };

  const handleSendMessage = async (e) => {
    if (e) e.preventDefault();
    if (!newMessage.trim()) return;
    try {
      const msgObj = {
          id: Math.random().toString(),
          userId: user.id,
          userName: user.fullName,
          content: newMessage,
          createdAt: new Date().toISOString()
      };
      
      setRoomChats(prev => [...prev, msgObj]);
      if (dashboardConnection) {
          await dashboardConnection.invoke("SyncRoomChat", circleId, msgObj).catch(err => console.error(err));
      }
      setNewMessage('');
      setTimeout(() => chatEndRef.current?.scrollIntoView({ behavior: 'smooth' }), 100);
    } catch (error) {
      toast.error('Failed to send message');
    }
  };
  const handleCreateResource = async () => {
    if (!newResourceTitle.trim() || !newResourceUrl.trim()) {
      toast.error("Please provide both title and URL");
      return;
    }
    try {
      const response = await apiClient.post(`/learningcircles/${circleId}/resources`, { 
        title: newResourceTitle, 
        url: newResourceUrl 
      });
      setResources([response.data, ...resources]);
      setNewResourceTitle('');
      setNewResourceUrl('');
      toast.success('Resource shared');
    } catch (error) {
      toast.error('Failed to share resource');
    }
  };

  const handleCreateTask = async (e) => {
    e.preventDefault();
    const formData = new FormData(e.target);
    const title = formData.get('title');
    const description = formData.get('description');
    
    if (!title || !description) {
      toast.error("Please provide both title and description");
      return;
    }
    
    try {
      const response = await apiClient.post(`/learningcircles/${circleId}/tasks`, { 
        title, 
        description 
      });
      setTasks([response.data, ...tasks]);
      e.target.reset();
      toast.success('Task created successfully');
    } catch (error) {
      toast.error('Failed to create task');
    }
  };

  const handleUpdateTaskStatus = async (taskId, newStatus) => {
    try {
      await apiClient.put(`/learningcircles/${circleId}/tasks/${taskId}`, { status: newStatus });
      setTasks(tasks.map(t => t.id === taskId ? { ...t, status: newStatus } : t));
      const progressResponse = await apiClient.get(`/learningcircles/${circleId}/progress`);
      setProgressBoard(progressResponse.data);
      toast.success(`Task marked as ${newStatus}`);
    } catch (error) {
      toast.error('Failed to update task');
    }
  };

  const handleSyncNotes = async () => {
    try {
      const response = await apiClient.put(`/learningcircles/${circleId}/notes`, { content: noteContent });
      setNoteContent(response.data.content);
      toast.success('Notes synced successfully');
    } catch (error) {
      toast.error('Failed to sync notes');
    }
  };

  const handleAskAiMentor = async (queryOverride) => {
    const query = queryOverride || newAiMentorMessage;
    if (!query.trim()) return;

    setAiMentorMessages(prev => [...prev, { role: 'user', content: query }]);
    setNewAiMentorMessage('');
    setAiMentorLoading(true);

    try {
      const response = await apiClient.post(`/learningcircles/${circleId}/ai-mentor`, { query });
      setAiMentorMessages(prev => [...prev, { role: 'ai', content: response.data.response }]);
    } catch (error) {
      toast.error('Failed to get response from AI Mentor');
      setAiMentorMessages(prev => [...prev, { role: 'ai', content: 'Sorry, I encountered an error. Please try again.' }]);
    } finally {
      setAiMentorLoading(false);
    }
  };
  
  const handleRemoveMember = async (memberId) => {
    if (!window.confirm("Are you sure you want to remove this member?")) return;
    try {
      await apiClient.delete(`/learningcircles/${circleId}/members/${memberId}`);
      setMembers(members.filter(m => m.id !== memberId));
      toast.success('Member removed successfully');
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to remove member');
    }
  };

  const handleLeaveCircle = async () => {
    if (!window.confirm("Are you sure you want to leave this circle?")) return;
    try {
      await apiClient.post(`/learningcircles/${circleId}/leave`);
      toast.success('You have left the circle');
      navigate('/learning-circles');
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to leave circle');
    }
  };

  const regularResources = resources.filter(r => !r.title.startsWith('Meeting Notes - '));
  const meetingNotes = resources.filter(r => r.title.startsWith('Meeting Notes - '));

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[calc(100vh-72px)] bg-[#F8FAFC]">
        <Loader2 className="w-8 h-8 animate-spin text-blue-600" />
      </div>
    );
  }

  if (!circle) {
    return (
      <div className="p-8 text-center bg-[#F8FAFC] min-h-[calc(100vh-72px)]">
        <h2 className="text-xl font-bold text-slate-900">Circle Not Found</h2>
        <Button onClick={() => navigate('/learning-circles')} className="mt-4">Back to Circles</Button>
      </div>
    );
  }

  if (circle.isDisabledByAdmin) {
    return (
      <div className="min-h-[calc(100vh-72px)] bg-[#F8FAFC] p-6 md:p-10">
        <div className="mx-auto max-w-3xl rounded-2xl border border-amber-200 bg-white p-8 shadow-sm">
          <Button variant="ghost" size="sm" onClick={() => navigate('/learning-circles')} className="mb-5 text-slate-500"><ArrowLeft className="mr-2 h-4 w-4"/>Back to circles</Button>
          <Badge className="mb-4 bg-amber-100 text-amber-900 hover:bg-amber-100">Disabled by admin</Badge>
          <h1 className="text-2xl font-bold text-slate-900">{circle.name}</h1>
          <p className="mt-2 text-slate-600">This group has been disabled by an administrator. Group activities are unavailable while it is disabled.</p>
          {circle.adminDisabledReason && <p className="mt-4 rounded-lg bg-amber-50 p-4 text-sm text-amber-900">{circle.adminDisabledReason}</p>}
          <p className="mt-4 text-sm text-slate-500">{circle.careerDomain} · {circle.memberCount} members · Created by {circle.creatorName}</p>
        </div>
      </div>
    );
  }

  const isScheduledAndReady = circle?.scheduledMeetingTime && new Date(circle.scheduledMeetingTime) <= new Date();

  return (
    <div className="min-h-[calc(100vh-72px)] bg-[#F8FAFC] flex flex-col">
      {/* HEADER SECTION */}
      <div className="bg-white border-b border-[#E2E8F0] shadow-sm sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-4 lg:px-8 py-6">
          <Button variant="ghost" size="sm" onClick={() => navigate(-1)} className="mb-4 text-slate-500 hover:text-slate-900 -ml-2">
            <ArrowLeft className="w-4 h-4 mr-2" /> Back
          </Button>

          <div className="flex flex-col md:flex-row md:items-start justify-between gap-6">
            <div>
              <div className="flex items-center gap-3 mb-2">
                <div className="h-10 w-10 bg-blue-100 text-blue-600 rounded-xl flex items-center justify-center shrink-0">
                  <MonitorUp className="w-5 h-5" />
                </div>
                <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight">{circle.name}</h1>
                {circle.isMeetingActive && (
                  <Badge className="bg-red-500 hover:bg-red-600 animate-pulse border-0">Live Meeting</Badge>
                )}
              </div>
              <p className="text-slate-600 max-w-3xl text-base">{circle.description}</p>
              
              <div className="flex flex-wrap items-center gap-4 mt-5">
                <Badge variant="outline" className="bg-blue-50 text-blue-700 border-blue-200 py-1.5 px-3 rounded-full text-xs font-semibold">
                  {circle.careerDomain}
                </Badge>
                <div className="flex items-center text-slate-500 text-sm font-medium">
                  <Users className="w-4 h-4 mr-1.5 text-slate-400" />
                  {circle.memberCount || 56} Members
                </div>
                <div className="flex items-center text-slate-500 text-sm font-medium">
                  <span className="h-5 w-5 rounded-full bg-slate-200 text-slate-600 flex items-center justify-center text-[10px] font-bold mr-1.5">
                    {circle.creatorName?.charAt(0).toUpperCase() || 'A'}
                  </span>
                  Created by {circle.creatorName || 'Registered member'}
                </div>
              </div>
            </div>

            <div className="flex flex-col gap-3 min-w-[200px]">
                <>
                  <Button onClick={() => {
                      if (!circle.isMeetingActive) toggleMeeting();
                      setInMeeting(true);
                  }} variant={circle.isMeetingActive ? "destructive" : "default"} className={`w-full shadow-sm font-semibold tracking-wide ${!circle.isMeetingActive ? 'bg-indigo-600 hover:bg-indigo-700' : ''}`}>
                    <Video className="w-4 h-4 mr-2" />
                    {circle.isMeetingActive ? 'Join Ongoing Session' : 'Start Study Session'}
                  </Button>
                  
                  {circle.isMeetingActive && (
                    <Button onClick={() => {
                        toggleMeeting();
                        setInMeeting(false);
                    }} variant="outline" className="w-full shadow-sm border-red-200 text-red-600 hover:bg-red-50 mt-1">
                        End Study Session
                    </Button>
                  )}
                  
                  {isCreator && (
                    <div className="flex flex-col gap-2 p-3 bg-white border border-slate-200 rounded-xl mt-3 shadow-sm">
                      <span className="text-xs font-semibold text-slate-700">Schedule Live Session</span>
                      <Input 
                        type="datetime-local" 
                        value={scheduleTime} 
                        onChange={(e) => setScheduleTime(e.target.value)}
                        className="h-8 text-xs bg-slate-50 border-slate-200" 
                      />
                      <Button onClick={handleScheduleMeeting} size="sm" variant="outline" className="w-full text-xs h-8 bg-slate-50 hover:bg-slate-100 border-slate-200 text-slate-700 font-medium">
                        Schedule & Notify
                      </Button>
                    </div>
                  )}

                  {!isCreator && (
                    <Button onClick={handleLeaveCircle} variant="outline" size="sm" className="w-full shadow-sm border-slate-200 text-slate-600 hover:bg-red-50 hover:text-red-600 hover:border-red-200 mt-2">
                      Leave Circle
                    </Button>
                  )}

                  {circle.scheduledMeetingTime && new Date(circle.scheduledMeetingTime) > new Date() && (
                    <div className="p-3 bg-blue-50/80 border border-blue-200 rounded-xl text-center mt-2 shadow-sm">
                      <span className="block text-[10px] font-bold text-blue-600 uppercase tracking-wider mb-1">Upcoming Session</span>
                      <span className="text-sm font-semibold text-blue-900">
                        {new Date(circle.scheduledMeetingTime).toLocaleString([], { dateStyle: 'short', timeStyle: 'short' })}
                      </span>
                    </div>
                  )}
                </>

              {isCreator && joinRequests.length > 0 && (
                <div className="bg-amber-50 border border-amber-200 rounded-lg p-3 text-sm">
                  <div className="font-semibold text-amber-800 mb-2">{joinRequests.length} Pending Request(s)</div>
                  {joinRequests.map(req => (
                    <div key={req.id} className="flex items-center justify-between mt-2">
                      <span className="text-slate-700 truncate mr-2">{req.userName}</span>
                      <div className="flex gap-1 shrink-0">
                        <Button size="sm" variant="outline" className="h-7 px-2 text-xs text-green-700 border-green-200 bg-green-50 hover:bg-green-100" onClick={() => handleProcessRequest(req.id, true)}>Allow</Button>
                        <Button size="sm" variant="outline" className="h-7 px-2 text-xs text-red-700 border-red-200 bg-red-50 hover:bg-red-100" onClick={() => handleProcessRequest(req.id, false)}>Deny</Button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* WORKSPACE AREA OR MEETING AREA */}
      <div className="max-w-7xl mx-auto px-4 lg:px-8 py-8 w-full flex-1">
        {inMeeting ? (
          <div className="flex flex-col lg:flex-row gap-6">
            <div className="flex-1 flex flex-col gap-4">
              <LiveVideoSession circleId={circle.id} onClose={() => setInMeeting(false)} />
              <div className="flex flex-col md:flex-row gap-4 h-[30vh]">
                <div className="flex-1 bg-white rounded-2xl shadow-sm border border-slate-200 p-4 flex flex-col">
                  <div className="flex items-center justify-between mb-2">
                    <h3 className="font-bold text-slate-700 flex items-center text-sm"><Edit3 className="w-4 h-4 mr-2 text-slate-400" /> Shared Whiteboard</h3>
                    <Button onClick={downloadWhiteboard} size="sm" className="h-7 text-xs bg-slate-700 hover:bg-slate-800">Save</Button>
                  </div>
                  <textarea 
                    value={whiteboardContent}
                    onChange={(e) => {
                        const val = e.target.value;
                        setWhiteboardContent(val);
                        if (dashboardConnection) {
                            dashboardConnection.invoke("SyncWhiteboard", circleId, val).catch(() => {});
                        }
                    }}
                    className="flex-1 w-full resize-none outline-none text-sm text-slate-600 bg-transparent placeholder:text-slate-300" 
                    placeholder="Sketch ideas as text, links, diagrams, or decisions..." 
                  />
                </div>
                <div className="flex-1 bg-white rounded-2xl shadow-sm border border-slate-200 p-4 flex flex-col">
                  <div className="flex items-center justify-between mb-2">
                    <h3 className="font-bold text-slate-700 flex items-center text-sm"><FileText className="w-4 h-4 mr-2 text-slate-400" /> Room Notes</h3>
                    <Button onClick={handleSyncNotes} size="sm" variant="outline" className="h-7 text-xs bg-slate-100 border-slate-200 hover:bg-slate-200 text-slate-600">Sync</Button>
                  </div>
                  <textarea 
                    value={noteContent}
                    onChange={(e) => setNoteContent(e.target.value)}
                    className="flex-1 w-full resize-none outline-none text-sm text-slate-600 bg-transparent placeholder:text-slate-300" 
                    placeholder="Agenda, resources, decisions, next steps..." 
                  />
                </div>
              </div>
            </div>
            
            <div className="w-full lg:w-80 shrink-0 bg-[#F8FAFC] rounded-2xl border border-slate-200 shadow-sm flex flex-col h-[90vh] lg:h-auto overflow-hidden">
              <div className="p-4 border-b border-slate-200 bg-white">
                <h3 className="font-bold text-slate-700 text-sm">Room Chat</h3>
              </div>
              <div className="flex-1 overflow-y-auto p-4 space-y-3 bg-[#F1F5F9]/50">
                {roomChats.map((msg, i) => {
                  const isMe = user && msg.userId === user.id;
                  return (
                    <div key={i} className={`flex flex-col ${isMe ? 'items-end' : 'items-start'}`}>
                      <div className={`max-w-[85%] rounded-xl px-3 py-2 ${isMe ? 'bg-[#3b82f6]/90 text-white rounded-tr-sm shadow-sm' : 'bg-white text-slate-700 rounded-tl-sm shadow-sm border border-slate-100'}`}>
                        {!isMe && <div className="text-[10px] font-bold text-slate-500 mb-0.5">{msg.userName}</div>}
                        <p className="text-xs whitespace-pre-wrap">{msg.content}</p>
                      </div>
                    </div>
                  );
                })}
                <div ref={chatEndRef} />
              </div>
              <div className="p-3 border-t border-slate-200 bg-white">
                <form onSubmit={handleSendMessage} className="relative flex gap-2">
                  <Input 
                    placeholder="Message the room..." 
                    className="flex-1 bg-slate-50 border-slate-200 text-xs h-9 focus-visible:ring-blue-500 rounded-lg"
                    value={newMessage}
                    onChange={(e) => setNewMessage(e.target.value)}
                  />
                  <Button type="submit" size="icon" className="h-9 w-9 shrink-0 rounded-lg bg-teal-600 hover:bg-teal-700 shadow-sm">
                    <Send className="w-3.5 h-3.5" />
                  </Button>
                </form>
              </div>
            </div>
          </div>
        ) : (
          <Tabs defaultValue="overview" className="w-full">
          
          <TabsList className="bg-white border border-slate-200 shadow-sm rounded-xl p-1 mb-6 flex gap-1 overflow-x-auto overflow-y-hidden h-auto">
            <TabsTrigger value="overview" className="rounded-lg data-[state=active]:bg-blue-50 data-[state=active]:text-blue-700 hover:bg-slate-100 hover:text-slate-900 data-[state=active]:shadow-none py-2.5 px-4 font-medium"><MonitorUp className="w-4 h-4 mr-2"/> Overview</TabsTrigger>
            <TabsTrigger value="discussions" className="rounded-lg data-[state=active]:bg-blue-50 data-[state=active]:text-blue-700 hover:bg-slate-100 hover:text-slate-900 data-[state=active]:shadow-none py-2.5 px-4 font-medium"><MessageSquare className="w-4 h-4 mr-2"/> Discussions</TabsTrigger>
            <TabsTrigger value="resources" className="rounded-lg data-[state=active]:bg-blue-50 data-[state=active]:text-blue-700 hover:bg-slate-100 hover:text-slate-900 data-[state=active]:shadow-none py-2.5 px-4 font-medium"><Download className="w-4 h-4 mr-2"/> Resources</TabsTrigger>
            <TabsTrigger value="notes" className="rounded-lg data-[state=active]:bg-blue-50 data-[state=active]:text-blue-700 hover:bg-slate-100 hover:text-slate-900 data-[state=active]:shadow-none py-2.5 px-4 font-medium"><FileText className="w-4 h-4 mr-2"/> Shared Notes</TabsTrigger>
            <TabsTrigger value="tasks" className="rounded-lg data-[state=active]:bg-blue-50 data-[state=active]:text-blue-700 hover:bg-slate-100 hover:text-slate-900 data-[state=active]:shadow-none py-2.5 px-4 font-medium"><CheckSquare className="w-4 h-4 mr-2"/> Tasks</TabsTrigger>
            <TabsTrigger value="members" className="rounded-lg data-[state=active]:bg-blue-50 data-[state=active]:text-blue-700 hover:bg-slate-100 hover:text-slate-900 data-[state=active]:shadow-none py-2.5 px-4 font-medium"><Users className="w-4 h-4 mr-2"/> Members</TabsTrigger>
            <TabsTrigger value="ai-mentor" className="rounded-lg data-[state=active]:bg-indigo-50 data-[state=active]:text-indigo-700 hover:bg-slate-100 hover:text-slate-900 data-[state=active]:shadow-none py-2.5 px-4 font-medium"><Sparkles className="w-4 h-4 mr-2"/> AI Mentor</TabsTrigger>
            <TabsTrigger value="progress" className="rounded-lg data-[state=active]:bg-blue-50 data-[state=active]:text-blue-700 hover:bg-slate-100 hover:text-slate-900 data-[state=active]:shadow-none py-2.5 px-4 font-medium"><TrendingUp className="w-4 h-4 mr-2"/> Progress Board</TabsTrigger>
          </TabsList>

          {/* OVERVIEW TAB */}
          <TabsContent value="overview" className="mt-0 outline-none">
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
              <div className="lg:col-span-2 space-y-6">
                
                <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm">
                  <h3 className="text-lg font-bold text-slate-900 mb-4 flex items-center">
                    <MessageSquare className="w-5 h-5 mr-2 text-blue-600" /> Recent Activity
                  </h3>
                  <div className="space-y-4">
                    {overview?.recentActivities?.length === 0 ? (
                      <div className="text-sm text-slate-500 py-4 text-center">No recent activity yet.</div>
                    ) : (
                      overview?.recentActivities?.map((act, i) => (
                        <div key={i} className="flex items-start gap-4 pb-4 border-b border-slate-100 last:border-0 last:pb-0">
                          <div className={`mt-0.5 h-2 w-2 rounded-full shrink-0 ${act.type === 'discussion' ? 'bg-blue-500' : act.type === 'resource' ? 'bg-emerald-500' : 'bg-purple-500'}`} />
                          <div>
                            <p className="text-sm font-medium text-slate-800">{act.text}</p>
                            <p className="text-xs text-slate-500 mt-1">{new Date(act.timestamp).toLocaleDateString()}</p>
                          </div>
                        </div>
                      ))
                    )}
                  </div>
                </div>

              </div>

              <div className="space-y-6">
                
                <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm">
                  <h3 className="text-lg font-bold text-slate-900 mb-4">Circle Stats</h3>
                  <div className="grid grid-cols-2 gap-4">
                    <div className="text-center p-3 bg-slate-50 rounded-xl">
                      <div className="text-2xl font-black text-blue-600">{overview?.discussionsCount || 0}</div>
                      <div className="text-[10px] uppercase font-bold text-slate-500 mt-1">Discussions</div>
                    </div>
                    <div className="text-center p-3 bg-slate-50 rounded-xl">
                      <div className="text-2xl font-black text-emerald-600">{overview?.resourcesCount || 0}</div>
                      <div className="text-[10px] uppercase font-bold text-slate-500 mt-1">Resources</div>
                    </div>
                    <div className="text-center p-3 bg-slate-50 rounded-xl">
                      <div className="text-2xl font-black text-purple-600">{overview?.tasksCompletedCount || 0}</div>
                      <div className="text-[10px] uppercase font-bold text-slate-500 mt-1">Tasks Done</div>
                    </div>
                    <div className="text-center p-3 bg-slate-50 rounded-xl">
                      <div className="text-2xl font-black text-amber-600">{overview?.membersCount || 1}</div>
                      <div className="text-[10px] uppercase font-bold text-slate-500 mt-1">Members</div>
                    </div>
                  </div>
                </div>

              </div>
            </div>
          </TabsContent>

          {/* DISCUSSIONS TAB */}
          <TabsContent value="discussions" className="mt-0 outline-none">
            <div className="bg-white rounded-2xl border border-slate-200 shadow-sm flex flex-col h-[600px]">
              <div className="p-4 border-b border-slate-200 flex justify-between items-center">
                <h3 className="text-lg font-bold text-slate-900 flex items-center">
                  <MessageSquare className="w-5 h-5 mr-2 text-blue-600" /> Circle Chat
                </h3>
              </div>
              <div className="flex-1 overflow-y-auto p-4 space-y-4">
                {discussions.length === 0 ? (
                  <div className="flex flex-col items-center justify-center h-full text-slate-400">
                    <MessageSquare className="w-12 h-12 mb-3 opacity-20" />
                    <p>No messages yet. Be the first to say hello!</p>
                  </div>
                ) : (
                  discussions.map((msg, i) => {
                    const isMe = user && msg.userId === user.id;
                    return (
                      <div key={i} className={`flex ${isMe ? 'justify-end' : 'justify-start'}`}>
                        <div className={`max-w-[70%] rounded-2xl px-4 py-2.5 ${isMe ? 'bg-blue-600 text-white rounded-tr-sm' : 'bg-slate-100 text-slate-900 rounded-tl-sm'}`}>
                          {!isMe && <div className="text-xs font-bold text-blue-600 mb-1">{msg.userName}</div>}
                          <p className="text-sm whitespace-pre-wrap">{msg.content}</p>
                          <div className={`text-[10px] mt-1 text-right ${isMe ? 'text-blue-200' : 'text-slate-400'}`}>
                            {new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                          </div>
                        </div>
                      </div>
                    );
                  })
                )}
              </div>
              <div className="p-4 border-t border-slate-200 bg-slate-50 rounded-b-2xl flex gap-3 items-end">
                <Input 
                  placeholder="Type a message..."
                  className="flex-1 bg-white border-slate-300"
                  value={newDiscussionContent}
                  onChange={(e) => setNewDiscussionContent(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleCreateDiscussion()}
                />
                <Button onClick={handleCreateDiscussion} className="bg-blue-600 hover:bg-blue-700">
                  <Send className="w-4 h-4 mr-2" /> Send
                </Button>
              </div>
            </div>
          </TabsContent>

          {/* RESOURCES TAB */}
          <TabsContent value="resources" className="mt-0 outline-none">
            <div className="bg-white rounded-2xl border border-slate-200 shadow-sm p-6">
              <div className="flex items-center justify-between mb-6">
                <h2 className="text-xl font-bold text-slate-900">Resource Library</h2>
              </div>

              <div className="mb-6 bg-slate-50 border border-slate-200 p-4 rounded-xl flex gap-3 flex-col sm:flex-row">
                <Input 
                  placeholder="Resource Title (e.g., Intro to React)" 
                  value={newResourceTitle} 
                  onChange={e => setNewResourceTitle(e.target.value)} 
                  className="bg-white"
                />
                <Input 
                  placeholder="URL (e.g., https://youtube.com/...)" 
                  value={newResourceUrl} 
                  onChange={e => setNewResourceUrl(e.target.value)} 
                  className="bg-white"
                />
                <Button onClick={handleCreateResource} className="shrink-0 bg-blue-600 hover:bg-blue-700">
                  <Download className="w-4 h-4 mr-2" /> Share Link
                </Button>
              </div>

              <div className="space-y-4">
                {regularResources.length === 0 ? (
                  <div className="text-center py-8 text-slate-400">No resources shared yet.</div>
                ) : (
                  regularResources.map((res, i) => (
                    <div key={res.id || i} className="flex gap-4 p-4 rounded-xl border border-slate-200 hover:border-blue-300 hover:shadow-sm transition-all group bg-white">
                      <div className="h-10 w-10 rounded-lg bg-blue-50 text-blue-600 flex items-center justify-center shrink-0">
                        <FileText className="w-5 h-5" />
                      </div>
                      <div className="flex-1">
                        <h3 className="text-base font-bold text-slate-900 mb-1">{res.title}</h3>
                        <p className="text-xs text-slate-500 mb-2">Shared by <span className="font-semibold">{res.addedByUserName}</span></p>
                        <a href={res.url} target="_blank" rel="noopener noreferrer" className="text-sm text-blue-600 hover:underline flex items-center">
                          View Resource <Target className="w-3 h-3 ml-1" />
                        </a>
                      </div>
                      <div className="flex flex-col items-end gap-2 text-xs text-slate-400 mt-1">
                        <div>{new Date(res.addedAt).toLocaleDateString()}</div>
                        {isCreator && (
                            <Button size="sm" variant="ghost" className="h-6 text-xs text-red-500 hover:bg-red-50" onClick={async () => {
                                try {
                                    await apiClient.delete(`/learningcircles/${circleId}/resources/${res.id}`);
                                    setResources(prev => prev.filter(r => r.id !== res.id));
                                    toast.success("Resource deleted");
                                } catch (e) {
                                    toast.error("Failed to delete resource");
                                }
                            }}>Delete</Button>
                        )}
                      </div>
                    </div>
                  ))
                )}
              </div>
            </div>
          </TabsContent>
          
          {/* NOTES TAB */}
          <TabsContent value="notes" className="mt-0 outline-none">
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
              <div className="lg:col-span-2 bg-white rounded-2xl border border-slate-200 shadow-sm p-6 flex flex-col min-h-[600px]">
                <div className="flex items-center justify-between mb-4">
                  <h2 className="text-xl font-bold text-slate-900">Live Shared Note</h2>
                  <Button onClick={handleSyncNotes} className="bg-blue-600 hover:bg-blue-700">
                    <FileText className="w-4 h-4 mr-2" /> Sync Notes
                  </Button>
                </div>
                <textarea 
                  value={noteContent}
                  onChange={(e) => setNoteContent(e.target.value)}
                  className="flex-1 w-full p-4 border border-slate-200 rounded-xl resize-none outline-none text-slate-700 bg-slate-50 focus:bg-white focus:ring-2 focus:ring-blue-500/20"
                  placeholder="Type your notes here... Everyone in the circle can see and edit them."
                />
              </div>

              <div className="bg-white rounded-2xl border border-slate-200 shadow-sm p-6 flex flex-col h-[600px]">
                <h3 className="text-lg font-bold text-slate-900 mb-4">Past Meeting Notes</h3>
                <div className="flex-1 overflow-y-auto space-y-3">
                  {meetingNotes.length === 0 ? (
                    <div className="text-sm text-slate-500 text-center py-8">No past notes saved yet.</div>
                  ) : (
                    meetingNotes.map((note) => (
                      <div key={note.id} className="p-3 border border-slate-100 rounded-xl hover:shadow-sm bg-slate-50 relative group transition-all">
                        <h4 className="font-semibold text-slate-800 text-sm mb-1">{note.title}</h4>
                        <div className="text-[10px] text-slate-400 mb-3">Saved by {note.addedByUserName}</div>
                        <Button
                          size="sm"
                          variant="link"
                          className="h-auto p-0 text-xs font-medium text-blue-600 hover:underline"
                          onClick={() => setExpandedNoteId(expandedNoteId === note.id ? null : note.id)}
                        >
                          {expandedNoteId === note.id ? 'Hide Notes' : 'View Notes'}
                        </Button>
                        {expandedNoteId === note.id && (
                          <div className="mt-2 p-3 bg-white border border-slate-200 rounded-lg text-sm text-slate-700 whitespace-pre-wrap max-h-48 overflow-y-auto">
                            {note.url}
                          </div>
                        )}
                        {isCreator && (
                          <Button size="sm" variant="ghost" className="absolute top-2 right-2 h-6 w-6 p-0 text-red-400 opacity-0 group-hover:opacity-100" onClick={async () => {
                              try {
                                  await apiClient.delete(`/learningcircles/${circleId}/resources/${note.id}`);
                                  setResources(prev => prev.filter(r => r.id !== note.id));
                                  toast.success("Note deleted");
                              } catch (e) {
                                  toast.error("Failed to delete note");
                              }
                          }}>
                            &times;
                          </Button>
                        )}
                      </div>
                    ))
                  )}
                </div>
              </div>
            </div>
          </TabsContent>

          {/* TASKS TAB */}
          <TabsContent value="tasks" className="mt-0 outline-none">
            <div className="bg-white rounded-2xl border border-slate-200 shadow-sm p-6 min-h-[600px]">
              <h2 className="text-xl font-bold text-slate-900 mb-6">Circle Tasks</h2>
              
                <form onSubmit={handleCreateTask} className="mb-8 bg-slate-50 border border-slate-200 p-4 rounded-xl flex gap-3 flex-col sm:flex-row">
                  <Input name="title" placeholder="Task Title" className="bg-white flex-1" required />
                  <Input name="description" placeholder="Short description..." className="bg-white flex-1" required />
                  <Button type="submit" className="shrink-0 bg-blue-600 hover:bg-blue-700">
                    <CheckSquare className="w-4 h-4 mr-2" /> Add Task
                  </Button>
                </form>

              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {tasks.length === 0 ? (
                  <div className="col-span-full text-center py-8 text-slate-400">No tasks created yet.</div>
                ) : (
                  tasks.map(task => (
                    <div key={task.id} className="p-4 rounded-xl border border-slate-200 bg-white shadow-sm flex flex-col h-full hover:border-blue-300 transition-all">
                      <div className="flex justify-between items-start mb-2">
                        <h3 className="font-bold text-slate-800 line-clamp-1">{task.title}</h3>
                        <Badge variant="outline" className={`shrink-0 ${task.status === 'Completed' ? 'bg-green-50 text-green-700 border-green-200' : 'bg-amber-50 text-amber-700 border-amber-200'}`}>
                          {task.status}
                        </Badge>
                      </div>
                      <p className="text-sm text-slate-600 mb-4 line-clamp-2 flex-1">{task.description}</p>
                      <div className="flex items-center justify-between mt-auto pt-3 border-t border-slate-100">
                        <span className="text-xs text-slate-400">By {task.createdByUserName}</span>
                        {task.status !== 'Completed' ? (
                          <Button 
                            size="sm" 
                            variant="outline" 
                            onClick={() => handleUpdateTaskStatus(task.id, 'Completed')}
                            className="h-7 text-xs text-blue-600 border-blue-200 hover:bg-blue-50"
                          >
                            Mark Done
                          </Button>
                        ) : (
                          <Button 
                            size="sm" 
                            variant="ghost" 
                            onClick={() => handleUpdateTaskStatus(task.id, 'Pending')}
                            className="h-7 text-xs text-slate-400"
                          >
                            Undo
                          </Button>
                        )}
                      </div>
                    </div>
                  ))
                )}
              </div>
            </div>
          </TabsContent>
          <TabsContent value="ai-mentor" className="mt-0 outline-none">
            <div className="bg-white rounded-2xl border border-indigo-200 shadow-sm overflow-hidden flex flex-col h-[600px]">
              <div className="p-6 bg-gradient-to-r from-indigo-50 to-blue-50 border-b border-indigo-100 flex items-center gap-3">
                <div className="h-12 w-12 rounded-xl bg-indigo-600 text-white flex items-center justify-center shadow-md">
                  <Brain className="w-6 h-6" />
                </div>
                <div>
                  <h2 className="text-xl font-bold text-indigo-950">Circle AI Mentor</h2>
                  <p className="text-xs font-medium text-indigo-700">Ask concepts, generate roadmaps, or summarize discussions.</p>
                </div>
              </div>
              
              <div className="flex-1 p-6 overflow-y-auto bg-slate-50/50 space-y-4">
                {aiMentorMessages.map((msg, idx) => (
                  <div key={idx} className={`flex gap-3 max-w-3xl ${msg.role === 'user' ? 'ml-auto flex-row-reverse' : ''}`}>
                    <div className={`h-8 w-8 rounded-full flex items-center justify-center shrink-0 ${msg.role === 'user' ? 'bg-blue-600 text-white' : 'bg-indigo-100 text-indigo-600'}`}>
                      {msg.role === 'user' ? <span className="text-xs font-bold">{user?.fullName?.charAt(0) || 'U'}</span> : <Brain className="w-4 h-4" />}
                    </div>
                    <div className={`p-4 rounded-2xl text-sm shadow-sm ${msg.role === 'user' ? 'bg-blue-600 text-white rounded-tr-none' : 'bg-white border border-slate-200 text-slate-700 rounded-tl-none prose prose-sm max-w-none'}`}>
                      {msg.role === 'user' ? (
                        msg.content
                      ) : (
                        <ReactMarkdown remarkPlugins={[remarkGfm]}>
                          {msg.content}
                        </ReactMarkdown>
                      )}
                    </div>
                  </div>
                ))}
                {aiMentorLoading && (
                  <div className="flex gap-3 max-w-3xl">
                    <div className="h-8 w-8 rounded-full bg-indigo-100 flex items-center justify-center shrink-0">
                      <Brain className="w-4 h-4 text-indigo-600" />
                    </div>
                    <div className="bg-white p-4 rounded-2xl rounded-tl-none border border-slate-200 shadow-sm text-sm text-slate-500 flex items-center gap-2">
                      <Loader2 className="w-4 h-4 animate-spin text-indigo-600" /> Thinking...
                    </div>
                  </div>
                )}
              </div>

              <div className="p-4 bg-white border-t border-slate-200">
                <div className="flex gap-2 mb-3 overflow-x-auto pb-1 scrollbar-hide">
                  <Badge onClick={() => handleAskAiMentor("Explain Regression")} variant="outline" className="cursor-pointer whitespace-nowrap bg-indigo-50 hover:bg-indigo-100 text-indigo-700 border-indigo-200">Explain Regression</Badge>
                  <Badge onClick={() => handleAskAiMentor("Create 7 Day Plan")} variant="outline" className="cursor-pointer whitespace-nowrap bg-indigo-50 hover:bg-indigo-100 text-indigo-700 border-indigo-200">Create 7 Day Plan</Badge>
                  <Badge onClick={() => handleAskAiMentor("Summarize Discussions")} variant="outline" className="cursor-pointer whitespace-nowrap bg-indigo-50 hover:bg-indigo-100 text-indigo-700 border-indigo-200">Summarize Discussions</Badge>
                </div>
                <form onSubmit={(e) => { e.preventDefault(); handleAskAiMentor(); }} className="relative flex items-center">
                  <Input 
                    value={newAiMentorMessage}
                    onChange={(e) => setNewAiMentorMessage(e.target.value)}
                    placeholder="Message AI Mentor..." 
                    className="pr-12 h-12 bg-slate-50 border-slate-200 focus-visible:ring-indigo-500 rounded-xl flex-1" 
                  />
                  <Button type="submit" disabled={aiMentorLoading || !newAiMentorMessage.trim()} size="icon" className="absolute right-1 top-1 h-10 w-10 rounded-lg bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50">
                    <Send className="w-4 h-4" />
                  </Button>
                </form>
              </div>
            </div>
          </TabsContent>



          <TabsContent value="members" className="mt-0 outline-none">
            <div className="bg-white rounded-2xl border border-slate-200 shadow-sm p-6">
              <h2 className="text-xl font-bold text-slate-900 mb-6">Members ({members.length})</h2>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {members.map((m, i) => (
                  <div key={m.id || i} className="p-4 rounded-xl border border-slate-200 flex items-center gap-4 hover:shadow-sm">
                    <div className="h-10 w-10 rounded-full bg-slate-200 text-slate-600 flex items-center justify-center font-bold text-sm">
                      {m.fullName.charAt(0)}
                    </div>
                    <div className="flex-1 min-w-0">
                      <h4 className="font-bold text-slate-900 text-sm truncate">{m.fullName}</h4>
                      <p className="text-[11px] text-slate-500 font-medium">{m.role}</p>
                    </div>
                    {m.role !== "Facilitator" && (
                      <Badge variant="outline" className="bg-emerald-50 text-emerald-700 border-emerald-200 shrink-0 text-[10px]">
                        Active
                      </Badge>
                    )}
                    {user?.id === circle?.creatorId && m.userId !== user?.id && (
                      <Button variant="ghost" size="sm" className="h-8 w-8 p-0 text-red-500 hover:text-red-700 hover:bg-red-50 ml-2" onClick={() => handleRemoveMember(m.id)}>
                        <X className="h-4 w-4" />
                      </Button>
                    )}
                  </div>
                ))}
              </div>
            </div>
          </TabsContent>

          <TabsContent value="progress" className="mt-0 outline-none">
            <div className="bg-white rounded-2xl border border-slate-200 shadow-sm p-6 min-h-[600px]">
              <div className="flex items-center gap-3 mb-6">
                <div className="h-10 w-10 bg-blue-100 text-blue-600 rounded-xl flex items-center justify-center">
                  <TrendingUp className="w-5 h-5" />
                </div>
                <div>
                  <h2 className="text-xl font-bold text-slate-900">Circle Progress Board</h2>
                  <p className="text-sm text-slate-500">Track top contributors based on completed tasks, resources shared, and discussions started.</p>
                </div>
              </div>

              {progressBoard.length === 0 ? (
                <div className="text-center py-12 text-slate-400">
                  No progress data available yet. Start sharing resources or completing tasks!
                </div>
              ) : (
                <div className="space-y-4">
                  {progressBoard.map((progress, idx) => (
                    <div key={progress.userId} className="flex flex-col md:flex-row items-center gap-6 p-4 rounded-xl border border-slate-200 bg-white hover:border-blue-300 hover:shadow-sm transition-all">
                      <div className="flex items-center gap-4 w-full md:w-auto md:min-w-[250px]">
                        <div className={`h-12 w-12 rounded-full flex items-center justify-center font-bold text-lg shrink-0 ${
                          idx === 0 ? 'bg-blue-100 text-blue-600 border-2 border-blue-300' : 
                          idx === 1 ? 'bg-slate-200 text-slate-600 border-2 border-slate-300' :
                          idx === 2 ? 'bg-indigo-100 text-indigo-700 border-2 border-indigo-300' :
                          'bg-slate-100 text-slate-500'
                        }`}>
                          {idx < 3 ? `#${idx + 1}` : progress.userName.charAt(0).toUpperCase()}
                        </div>
                        <div>
                          <h3 className="font-bold text-slate-900 text-base">{progress.userName}</h3>
                          <div className="flex items-center gap-1 mt-1">
                            <Sparkles className="w-3.5 h-3.5 text-blue-500" />
                            <span className="text-xs font-semibold text-blue-600">{progress.score} Points</span>
                          </div>
                        </div>
                      </div>
                      
                      <div className="flex flex-1 justify-between w-full md:w-auto items-center gap-4 bg-slate-50 p-3 rounded-lg border border-slate-100">
                        <div className="flex flex-col items-center text-center">
                          <span className="text-[10px] font-semibold text-slate-500 uppercase tracking-wider mb-1">Tasks Done</span>
                          <span className="text-sm font-bold text-slate-800">{progress.tasksCompleted}</span>
                        </div>
                        <div className="h-8 w-px bg-slate-200"></div>
                        <div className="flex flex-col items-center text-center">
                          <span className="text-[10px] font-semibold text-slate-500 uppercase tracking-wider mb-1">Resources</span>
                          <span className="text-sm font-bold text-slate-800">{progress.resourcesShared}</span>
                        </div>
                        <div className="h-8 w-px bg-slate-200"></div>
                        <div className="flex flex-col items-center text-center">
                          <span className="text-[10px] font-semibold text-slate-500 uppercase tracking-wider mb-1">Discussions</span>
                          <span className="text-sm font-bold text-slate-800">{progress.discussionsStarted}</span>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </TabsContent>

        </Tabs>
        )}
      </div>
    </div>
  );
};

export default CircleDashboard;
