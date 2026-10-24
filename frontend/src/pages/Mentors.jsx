import React, { useState, useEffect } from "react";
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Search, MessageSquare, ChevronRight, Filter, Loader2, Calendar, Send } from "lucide-react";
import { mentorsApi, chatApi } from '@/services/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';
import * as signalR from '@microsoft/signalr';
import { getAuthToken } from '@/services/api';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent } from '@/components/ui/card';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Image as ImageIcon, X, PlusCircle } from 'lucide-react';

const ChatWindow = ({ mentor, onClose }) => {
  const { user } = useAuth();
  const [messages, setMessages] = useState([]);
  const [newMessage, setNewMessage] = useState("");
  const [connection, setConnection] = useState(null);
  const [uploadingImage, setUploadingImage] = useState(false);
  const [selectedImage, setSelectedImage] = useState(null);
  const messagesEndRef = React.useRef(null);

  // Load history
  useEffect(() => {
    const fetchHistory = async () => {
      try {
        const res = await chatApi.getHistory(mentor.userId);
        setMessages(res.data?.data || []);
      } catch (err) {
        console.error("Failed to fetch chat history:", err);
      }
    };
    fetchHistory();
  }, [mentor.userId]);

  // Establish SignalR Connection
  useEffect(() => {
    const token = getAuthToken();
    const chatHubUrl = import.meta.env.VITE_API_URL
      ? import.meta.env.VITE_API_URL.replace('/api', '/hubs/chat')
      : 'http://localhost:5000/hubs/chat';

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(chatHubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);
  }, []);

  // Connect & listen
  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => {
          console.log("Connected to ChatHub");
          connection.invoke("JoinChat", mentor.userId);

          connection.on("ReceiveMessage", (message) => {
            setMessages(prev => [...prev, message]);
          });
        })
        .catch(e => console.log("Connection failed: ", e));
    }

    return () => {
      if (connection) {
        connection.invoke("LeaveChat", mentor.userId).catch(console.error);
        connection.off("ReceiveMessage");
        connection.stop();
      }
    };
  }, [connection, mentor.userId]);

  // Auto-scroll to bottom
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const handleImageSelect = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      toast.error("Please select an image file");
      return;
    }

    try {
      setUploadingImage(true);
      const formData = new FormData();
      formData.append('file', file);

      const res = await chatApi.uploadImage(formData);
      const imageUrl = res.data?.data;

      // Send the image as a message immediately
      if (connection && imageUrl) {
        await connection.invoke("SendMessage", mentor.userId, "Sent an image", imageUrl);
      }
    } catch (err) {
      toast.error("Failed to upload image");
    } finally {
      setUploadingImage(false);
      e.target.value = null; // reset input
    }
  };

  const handleSendMessage = async (e) => {
    e.preventDefault();
    if (!newMessage.trim() && !selectedImage) return;

    try {
      if (connection) {
        if (!mentor.userId) {
          toast.error("Mentor User ID is missing! Cannot send message.");
          console.error("Mentor object:", mentor);
          return;
        }

        console.log("Sending message to:", mentor.userId);

        if (selectedImage) {
          // handleImageSelect already implemented for this
        } else {
          await connection.invoke("SendMessage", mentor.userId, newMessage, null);
        }
        setNewMessage("");
      }
    } catch (err) {
      console.error("Failed to send:", err);
      toast.error("Failed to send message: " + err.message);
    }
  };

  return (
    <div className="flex flex-col h-[600px] bg-gray-50 rounded-lg overflow-hidden border">
      {/* Header */}
      <div className="p-4 bg-white border-b flex items-center gap-3">
        <div className="w-10 h-10 rounded-full bg-teal-500 text-white flex items-center justify-center font-semibold">
          {mentor.userName.split(" ").map((n) => n[0]).join("")}
        </div>
        <div>
          <h3 className="font-semibold">{mentor.userName}</h3>
          <p className="text-xs text-teal-600">Active now</p>
        </div>
      </div>

      {/* Messages */}
      <ScrollArea className="flex-1 p-4">
        <div className="space-y-4">
          {messages.map((msg, idx) => {
            const isMe = msg.senderId === user?.id;
            return (
              <div key={idx} className={`flex ${isMe ? 'justify-end' : 'justify-start'}`}>
                <div className={`max-w-[75%] rounded-2xl px-4 py-2 ${isMe ? 'bg-teal-500 text-white rounded-br-none' : 'bg-white border rounded-bl-none text-gray-800'
                  }`}>
                  {msg.imageUrl && (
                    <img src={`${import.meta.env.VITE_API_URL?.replace('/api', '') || 'http://localhost:5000'}${msg.imageUrl}`} alt="Attachment" className="rounded-lg mb-2 max-w-full h-auto object-cover max-h-48" />
                  )}
                  {msg.content !== "Sent an image" && <p className="text-sm">{msg.content}</p>}
                  <span className={`text-[10px] block mt-1 ${isMe ? 'text-teal-100' : 'text-gray-400'}`}>
                    {new Date(msg.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </span>
                </div>
              </div>
            );
          })}
          <div ref={messagesEndRef} />
        </div>
      </ScrollArea>

      {/* Input */}
      <div className="p-4 bg-white border-t">
        <form onSubmit={handleSendMessage} className="flex gap-2 items-center">
          <label className="cursor-pointer p-2 hover:bg-gray-100 rounded-full transition text-gray-500">
            <input
              type="file"
              accept="image/*"
              className="hidden"
              onChange={handleImageSelect}
              disabled={uploadingImage}
            />
            {uploadingImage ? <Loader2 className="w-5 h-5 animate-spin" /> : <ImageIcon className="w-5 h-5" />}
          </label>
          <Input
            value={newMessage}
            onChange={(e) => setNewMessage(e.target.value)}
            placeholder="Type your message..."
            className="flex-1 rounded-full bg-gray-50 focus-visible:ring-teal-500"
          />
          <Button
            type="submit"
            size="icon"
            className="rounded-full bg-teal-500 hover:bg-teal-600 h-10 w-10 shrink-0"
            disabled={(!newMessage.trim() && !selectedImage) || uploadingImage}
          >
            <Send className="w-4 h-4" />
          </Button>
        </form>
      </div>
    </div>
  );
};

const MentorsPage = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [mentors, setMentors] = useState([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [bookingMentor, setBookingMentor] = useState(null);
  const [bookingData, setBookingData] = useState({ topic: '', message: '' });
  const [submitting, setSubmitting] = useState(false);
  const [selectedSpecializations, setSelectedSpecializations] = useState([]);
  const [showSpecializations, setShowSpecializations] = useState(false);
  const { user, updateUser } = useAuth();

  // Check for search param from navbar
  useEffect(() => {
    const searchParam = searchParams.get('search');
    if (searchParam) {
      setSearch(searchParam);
    }
  }, [searchParams]);

  // Mentor Application state
  const [isApplyOpen, setIsApplyOpen] = useState(false);
  const [applyData, setApplyData] = useState({
    expertise: '',
    bio: '',
    yearsOfExperience: 1,
    hourlyRate: 0
  });

  // Autofill BIO when opening apply form
  useEffect(() => {
    if (isApplyOpen && user) {
      setApplyData(prev => ({
        ...prev,
        bio: user.bio || ''
      }));
    }
  }, [isApplyOpen, user]);

  useEffect(() => {
    const fetchMentors = async () => {
      try {
        setLoading(true);
        const response = await mentorsApi.getAllDetailedForUsers();
        setMentors(response.data || []);
      } catch (err) {
        console.error('Failed to fetch mentors:', err);
        setError('Failed to load mentors');
        toast.error('Failed to load mentors');
      } finally {
        setLoading(false);
      }
    };

    fetchMentors();
  }, []);

  const handleBookSession = async (e) => {
    e.preventDefault();
    if (!bookingMentor) return;

    try {
      setSubmitting(true);
      await mentorsApi.requestMentor({
        mentorId: bookingMentor.id,
        topic: bookingData.topic,
        message: bookingData.message,
      });

      toast.success('Mentor request sent successfully!');
      setBookingMentor(null);
      setBookingData({ topic: '', message: '' });
    } catch (err) {
      console.error('Failed to book mentor:', err);
      toast.error(err.message || 'Failed to send mentor request');
    } finally {
      setSubmitting(false);
    }
  };

  const handleApplyMentor = async (e) => {
    e.preventDefault();
    try {
      setSubmitting(true);
      await mentorsApi.applyAsMentor(applyData);
      toast.success('Successfully registered as a Mentor!');
      setIsApplyOpen(false);
      updateUser({ role: 'Mentor' });

      // Update local user state if necessary, or refetch mentors
      const response = await mentorsApi.getAllDetailedForUsers();
      setMentors(response.data || []);

    } catch (err) {
      console.error('Failed to apply:', err);
      toast.error(err.message || 'Failed to apply as mentor');
    } finally {
      setSubmitting(false);
    }
  };

  const getBadgeColor = (isAvailable) => {
    return isAvailable ? "bg-green-100 text-green-600" : "bg-gray-100 text-gray-500";
  };

  const getAvailabilityText = (isAvailable) => {
    return isAvailable ? "available" : "offline";
  };

  // Calculate match score based on user's skills vs mentor's expertise
  const getUserSkillsRegex = () => {
    if (!user?.bio) return null;
    return user.bio.toLowerCase();
  };

  const calculateMatchScore = (mentor) => {
    const userBio = getUserSkillsRegex();
    if (!userBio) return 0;

    let score = 0;
    const specializations = mentor.expertise.split(',').map(s => s.trim().toLowerCase()).filter(Boolean);

    specializations.forEach(s => {
      if (userBio.includes(s)) {
        score += 10;
      }
    });
    return score;
  };

  // Get all unique specializations from mentors
  const getAllSpecializations = () => {
    const specs = new Set();
    mentors.forEach(mentor => {
      mentor.expertise.split(',').forEach(spec => {
        const trimmed = spec.trim();
        if (trimmed) specs.add(trimmed);
      });
    });
    return Array.from(specs).sort();
  };

  // Toggle specialization filter
  const toggleSpecialization = (spec) => {
    setSelectedSpecializations(prev =>
      prev.includes(spec)
        ? prev.filter(s => s !== spec)
        : [...prev, spec]
    );
  };

  // Filter mentors: search match AND exclude current user AND specialization filter
  const filteredMentors = mentors
    .filter((m) => {
      const matchesSearch = m.userId !== user?.id &&
        (m.userName.toLowerCase().includes(search.toLowerCase()) ||
          m.expertise.toLowerCase().includes(search.toLowerCase()));
      
      if (!matchesSearch) return false;

      // If no specializations selected, show all mentors
      if (selectedSpecializations.length === 0) return true;

      // Check if mentor has any of the selected specializations
      const mentorSpecs = m.expertise.split(',').map(s => s.trim());
      return selectedSpecializations.some(spec => mentorSpecs.includes(spec));
    })
    .sort((a, b) => calculateMatchScore(b) - calculateMatchScore(a)); // Sort by match score descending

  // Show loading state
  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="text-center">
          <Loader2 className="h-12 w-12 animate-spin mx-auto text-teal-500" />
          <p className="mt-4 text-gray-500">Loading mentors...</p>
        </div>
      </div>
    );
  }

  // Show error state
  if (error) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Card className="max-w-md">
          <CardContent className="p-6 text-center">
            <p className="text-red-600 mb-4">{error}</p>
            <Button onClick={() => window.location.reload()}>Try Again</Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="p-8 bg-gray-50 min-h-screen">

      {/* Header */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-6 gap-4">
        <div>
          <h1 className="text-3xl font-bold text-gray-800">Mentorship Hub</h1>
          <p className="text-gray-500">
            Connect with experienced professionals to accelerate your career
          </p>
        </div>

        <Dialog open={isApplyOpen} onOpenChange={setIsApplyOpen}>
          <DialogTrigger asChild>
            <Button className="bg-teal-600 hover:bg-teal-700 text-white shadow">
              <PlusCircle className="mr-2 h-4 w-4" /> Become a Mentor
            </Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-md">
            <DialogHeader>
              <DialogTitle>Become a Mentor</DialogTitle>
              <DialogDescription>
                Share your expertise and guide others. Some details are autofilled from your profile.
              </DialogDescription>
            </DialogHeader>
            <form onSubmit={handleApplyMentor} className="space-y-4 py-4">
              <div className="space-y-2">
                <label className="text-sm font-medium">Expertise (comma separated)</label>
                <Input
                  placeholder="e.g. React, Node.js, C#"
                  required
                  value={applyData.expertise}
                  onChange={(e) => setApplyData({ ...applyData, expertise: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Bio</label>
                <Textarea
                  placeholder="Tell mentees about yourself..."
                  rows={3}
                  required
                  value={applyData.bio}
                  onChange={(e) => setApplyData({ ...applyData, bio: e.target.value })}
                />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <label className="text-sm font-medium">Years of Experience</label>
                  <Input
                    type="number"
                    min="1"
                    required
                    value={applyData.yearsOfExperience}
                    onChange={(e) => setApplyData({ ...applyData, yearsOfExperience: parseInt(e.target.value) })}
                  />
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium">Hourly Rate (₹)</label>
                  <Input
                    type="number"
                    min="0"
                    required
                    value={applyData.hourlyRate}
                    onChange={(e) => setApplyData({ ...applyData, hourlyRate: parseInt(e.target.value) })}
                  />
                </div>
              </div>
              <Button type="submit" className="w-full bg-teal-600 hover:bg-teal-700" disabled={submitting}>
                {submitting ? 'Submitting...' : 'Register as Mentor'}
              </Button>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {/* Search + Filter */}
      <div className="flex gap-4 mb-6">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-3 text-gray-400 w-4 h-4" />
          <input
            type="text"
            placeholder="Search mentors by name or skill..."
            className="w-full pl-10 pr-4 py-2 border rounded-lg bg-white shadow-sm"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>

        <div className="relative">
          <button 
            onClick={() => setShowSpecializations(!showSpecializations)}
            className="flex items-center gap-2 px-4 py-2 border rounded-lg bg-white shadow-sm hover:bg-gray-50 transition"
          >
            <Filter className="w-4 h-4" />
            <span>
              {selectedSpecializations.length > 0
                ? `${selectedSpecializations.length} Selected`
                : 'All Specializations'}
            </span>
          </button>

          {showSpecializations && (
            <div className="absolute top-full right-0 mt-2 bg-white border rounded-lg shadow-lg z-10 min-w-[250px] max-w-[400px]">
              <div className="p-3 border-b flex justify-between items-center">
                <span className="font-semibold text-sm">Filter by Specialization</span>
                {selectedSpecializations.length > 0 && (
                  <button
                    onClick={() => setSelectedSpecializations([])}
                    className="text-xs text-cyan-600 hover:text-cyan-700 font-medium"
                  >
                    Clear All
                  </button>
                )}
              </div>
              <div className="max-h-[300px] overflow-y-auto p-3 space-y-2">
                {getAllSpecializations().map((spec) => (
                  <label key={spec} className="flex items-center gap-2 cursor-pointer hover:bg-gray-50 p-2 rounded">
                    <input
                      type="checkbox"
                      checked={selectedSpecializations.includes(spec)}
                      onChange={() => toggleSpecialization(spec)}
                      className="w-4 h-4 cursor-pointer"
                    />
                    <span className="text-sm text-gray-700">{spec}</span>
                  </label>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Mentor Grid */}
      <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredMentors.length > 0 ? filteredMentors.map((mentor) => {
          // Parse expertise into array (comma-separated)
          const specializations = mentor.expertise.split(',').map(s => s.trim()).filter(Boolean);

          return (
            <div
              key={mentor.id}
              className="bg-white rounded-xl shadow-sm p-6 border hover:shadow-md transition"
            >
              {/* Top Section */}
              <div className="flex gap-4 mb-4">
                <div className="w-14 h-14 rounded-full bg-teal-500 text-white flex items-center justify-center font-semibold text-lg">
                  {mentor.userName.split(" ").map((n) => n[0]).join("")}
                </div>

                <div className="flex-1">
                  <div className="flex justify-between items-center">
                    <h3 className="font-semibold text-gray-800 flex items-center gap-2">
                      {mentor.userName}
                      {calculateMatchScore(mentor) > 0 && (
                        <span className="text-[10px] bg-teal-100 text-teal-700 px-2 py-0.5 rounded-full font-bold uppercase tracking-wider">
                          High Match
                        </span>
                      )}
                    </h3>
                    <span
                      className={`text-xs px-2 py-1 rounded-full ${getBadgeColor(
                        mentor.isAvailable
                      )}`}
                    >
                      {getAvailabilityText(mentor.isAvailable)}
                    </span>
                  </div>
                  <p className="text-sm text-gray-500">{mentor.yearsOfExperience} years experience</p>
                  <p className="text-sm text-teal-600">Expert Mentor</p>
                </div>
              </div>

              {/* Bio */}
              <p className="text-sm text-gray-500 mb-4 line-clamp-2">{mentor.bio || 'Experienced professional ready to help you grow.'}</p>

              {/* Skills */}
              <div className="flex flex-wrap gap-2 mb-4">
                {specializations.slice(0, 3).map((skill, idx) => (
                  <span key={idx} className="text-xs bg-gray-100 px-2 py-1 rounded">
                    {skill}
                  </span>
                ))}
              </div>

              {/* Students Assigned */}
              <div className="flex justify-between text-sm mb-4">
                <div className="flex items-center gap-1 text-blue-500">
                  <span className="text-gray-800 font-medium">{mentor.totalStudentsAssigned || 0}</span>
                  <span className="text-gray-500 text-xs">students</span>
                </div>
              </div>

              {/* Price + Button */}
              <div className="flex justify-between items-center gap-2">
                <div>
                  <span className="text-lg font-bold text-gray-800">
                    ₹{mentor.hourlyRate}
                  </span>
                  <span className="text-sm text-gray-500">/hr</span>
                </div>

                <div className="flex gap-2">
                  <button
                    onClick={() => navigate(`/mentor/${mentor.id}`)}
                    className="bg-cyan-500 text-white px-3 py-2 rounded-lg text-sm flex items-center gap-1 hover:bg-cyan-600 transition"
                  >
                    <ChevronRight className="w-4 h-4" />
                    Profile
                  </button>
                  <Dialog>
                    <DialogTrigger asChild>
                      <button
                        onClick={() => setBookingMentor(mentor)}
                        className="bg-teal-500 text-white px-3 py-2 rounded-lg text-sm flex items-center gap-1 hover:bg-teal-600 transition"
                        disabled={!mentor.isAvailable}
                      >
                        <MessageSquare className="w-4 h-4" />
                        Chat
                      </button>
                    </DialogTrigger>
                    <DialogContent className="max-w-2xl p-0 overflow-hidden border-none outline-none">
                      <DialogTitle className="sr-only">Chat with {mentor.userName}</DialogTitle>
                      {bookingMentor?.id === mentor.id && (
                        <ChatWindow mentor={mentor} onClose={() => setBookingMentor(null)} />
                      )}
                    </DialogContent>
                  </Dialog>
                </div>
              </div>
            </div>
          );
        }) : (
          <div className="col-span-full text-center py-12">
            <MessageSquare className="mx-auto h-12 w-12 text-gray-400 opacity-50" />
            <p className="mt-4 text-gray-500">No mentors found matching your search</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default MentorsPage;
