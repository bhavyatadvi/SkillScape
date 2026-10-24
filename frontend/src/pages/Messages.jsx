import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useAuth } from '@/context/AuthContext';
import { chatApi } from '@/services/api';
import { getAuthToken } from '@/services/api';
import * as signalR from '@microsoft/signalr';
import { Search, Send, ImageIcon, Loader2, MessageSquare, Trash2, MoreVertical, Phone, Video } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { ScrollArea } from '@/components/ui/scroll-area';
import { toast } from 'sonner';
import PageHeader from '@/components/layout/PageHeader';
import { useSearchParams } from 'react-router-dom';

const ChatWindow = ({ contact, onClose, onMessagesRead }) => {
    const { user } = useAuth();
    const [messages, setMessages] = useState([]);
    const [newMessage, setNewMessage] = useState("");
    const [connection, setConnection] = useState(null);
    const [uploadingImage, setUploadingImage] = useState(false);
    const [selectedImage, setSelectedImage] = useState(null);
    const [isDeleting, setIsDeleting] = useState(false);
    const messagesEndRef = useRef(null);
    const activeContactIdRef = useRef(contact.otherUserId);
    const isMarkingReadRef = useRef(false);
    const lastMarkReadAtRef = useRef(0);

    useEffect(() => {
        activeContactIdRef.current = contact.otherUserId;
    }, [contact.otherUserId]);

    const markCurrentConversationAsRead = useCallback(async (force = false) => {
        const otherUserId = activeContactIdRef.current;
        if (!otherUserId) return;

        const now = Date.now();
        if (!force && (isMarkingReadRef.current || now - lastMarkReadAtRef.current < 800)) {
            return;
        }

        try {
            isMarkingReadRef.current = true;
            await chatApi.markAsRead(otherUserId);
            lastMarkReadAtRef.current = Date.now();
            if (onMessagesRead) {
                onMessagesRead(otherUserId);
            }
        } catch (err) {
            console.error("Failed to mark as read:", err);
        } finally {
            isMarkingReadRef.current = false;
        }
    }, [onMessagesRead]);

    useEffect(() => {
        const fetchHistory = async () => {
            try {
                const res = await chatApi.getHistory(contact.otherUserId);
                setMessages(res.data?.data || []);
            } catch (err) {
                console.error("Failed to fetch chat history:", err);
            }
        };
        fetchHistory();

        if (contact.unreadCount > 0) {
            markCurrentConversationAsRead(true);
        }
    }, [contact.otherUserId, contact.unreadCount, markCurrentConversationAsRead]);

    useEffect(() => {
        const token = getAuthToken();
        const chatHubUrl = import.meta.env.VITE_API_URL
            ? import.meta.env.VITE_API_URL.replace('/api', '/hubs/chat')
            : '/hubs/chat';

        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl(chatHubUrl, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);
    }, []);

    useEffect(() => {
        if (connection) {
            connection.start()
                .then(() => {
                    connection.invoke("JoinChat", contact.otherUserId);

                    connection.off("ReceiveMessage");
                    connection.on("ReceiveMessage", (message) => {
                        setMessages(prev => [...prev, message]);
                        if (message.senderId === activeContactIdRef.current) {
                            markCurrentConversationAsRead();
                        }
                    });
                })
                .catch(e => console.log("Connection failed: ", e));

            return () => {
                if (connection) {
                    connection.invoke("LeaveChat", contact.otherUserId).catch(console.error);
                    connection.off("ReceiveMessage");
                    connection.stop();
                }
            };
        }
    }, [connection, contact.otherUserId, markCurrentConversationAsRead]);

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

            if (connection && imageUrl) {
                await connection.invoke("SendMessage", contact.otherUserId, "Sent an image", imageUrl);
            }
        } catch (err) {
            toast.error("Failed to upload image");
        } finally {
            setUploadingImage(false);
            e.target.value = null;
        }
    };

    const handleSendMessage = async (e) => {
        e.preventDefault();
        if (!newMessage.trim() && !selectedImage) return;

        try {
            if (connection) {
                if (!contact.otherUserId) return;

                if (!selectedImage) {
                    await connection.invoke("SendMessage", contact.otherUserId, newMessage, null);
                }
                setNewMessage("");
            }
        } catch (err) {
            toast.error("Failed to send message: " + err.message);
        }
    };

    const handleDeleteChat = async () => {
        if (!window.confirm("Are you sure you want to delete all messages in this conversation? This cannot be undone.")) return;
        try {
            setIsDeleting(true);
            await chatApi.clearHistory(contact.otherUserId);
            setMessages([]);
            toast.success("Chat history deleted");
        } catch (err) {
            toast.error("Failed to delete chat history");
        } finally {
            setIsDeleting(false);
        }
    };

    return (
        <div className="flex flex-col h-full bg-slate-50">
            {/* Header */}
            <div className="px-6 py-4 bg-white border-b border-slate-200 flex items-center justify-between shrink-0 h-[72px]">
                <div className="flex items-center gap-4">
                    <button onClick={onClose} className="md:hidden text-slate-400 hover:text-slate-600">
                        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                        </svg>
                    </button>
                    <div className="relative">
                        {contact.otherUserAvatar ? (
                            <img src={`${import.meta.env.VITE_API_URL?.replace('/api', '') || 'http://localhost:5000'}${contact.otherUserAvatar}`} alt={contact.otherUserName} className="w-10 h-10 rounded-full object-cover" />
                        ) : (
                            <div className="w-10 h-10 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center font-bold text-sm">
                                {contact.otherUserName.split(" ").map((n) => n[0]).join("").substring(0, 2)}
                            </div>
                        )}
                        <div className="absolute bottom-0 right-0 w-3 h-3 bg-blue-600 border-2 border-white rounded-full"></div>
                    </div>
                    <div>
                        <h3 className="font-bold text-slate-900">{contact.otherUserName}</h3>
                        <p className="text-xs font-medium text-blue-600">Active now</p>
                    </div>
                </div>
                <div className="flex items-center gap-2">
                    <Button variant="ghost" size="icon" className="text-slate-400 hover:text-blue-600 hidden sm:flex">
                        <Phone className="w-5 h-5" />
                    </Button>
                    <Button variant="ghost" size="icon" className="text-slate-400 hover:text-blue-600 hidden sm:flex">
                        <Video className="w-5 h-5" />
                    </Button>
                    <div className="w-px h-6 bg-slate-200 mx-1 hidden sm:block"></div>
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={handleDeleteChat}
                        disabled={isDeleting || messages.length === 0}
                        className="text-slate-400 hover:text-rose-600 hover:bg-rose-50"
                    >
                        {isDeleting ? <Loader2 className="w-5 h-5 animate-spin" /> : <Trash2 className="w-5 h-5" />}
                    </Button>
                </div>
            </div>

            {/* Messages */}
            <ScrollArea className="flex-1 p-6">
                <div className="space-y-6">
                    {messages.map((msg, idx) => {
                        const isMe = msg.senderId === user?.id;
                        return (
                            <div key={idx} className={`flex ${isMe ? 'justify-end' : 'justify-start'}`}>
                                <div className={`max-w-[75%] rounded-[10px] px-5 py-3 ${isMe ? 'bg-blue-600 text-white rounded-br-sm shadow-sm' : 'bg-white border border-slate-200 rounded-bl-sm shadow-sm text-slate-900'}`}>
                                    {msg.imageUrl && (
                                        <img src={`${import.meta.env.VITE_API_URL?.replace('/api', '') || 'http://localhost:5000'}${msg.imageUrl}`} alt="Attachment" className="rounded-lg mb-2 max-w-full h-auto object-cover max-h-60" />
                                    )}
                                    {msg.content !== "Sent an image" && <p className="text-sm leading-relaxed">{msg.content}</p>}
                                    <span className={`text-[10px] block mt-1.5 font-bold ${isMe ? 'text-blue-100' : 'text-slate-400'}`}>
                                        {new Date(msg.sentAt + (!msg.sentAt.endsWith('Z') ? 'Z' : '')).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                    </span>
                                </div>
                            </div>
                        );
                    })}
                    <div ref={messagesEndRef} />
                </div>
            </ScrollArea>

            {/* Input */}
            <div className="p-4 bg-white border-t border-slate-200 shrink-0">
                <form onSubmit={handleSendMessage} className="flex gap-3 items-center max-w-4xl mx-auto w-full">
                    <label className="cursor-pointer p-2 hover:bg-slate-100 rounded-full transition-colors text-slate-400 hover:text-blue-600">
                        <input
                            type="file"
                            accept="image/*"
                            className="hidden"
                            onChange={handleImageSelect}
                            disabled={uploadingImage}
                        />
                        {uploadingImage ? <Loader2 className="w-5 h-5 animate-spin text-blue-600" /> : <ImageIcon className="w-5 h-5" />}
                    </label>
                    <Input
                        value={newMessage}
                        onChange={(e) => setNewMessage(e.target.value)}
                        placeholder="Type your message..."
                        className="flex-1 rounded-full bg-slate-50 border-slate-200 h-12 px-6 focus-visible:ring-blue-600 focus-visible:border-transparent"
                    />
                    <Button
                        type="submit"
                        size="icon"
                        className="rounded-full bg-blue-600 hover:bg-blue-700 h-12 w-12 shrink-0 shadow-sm text-white"
                        disabled={(!newMessage.trim() && !selectedImage) || uploadingImage}
                    >
                        <Send className="w-5 h-5 ml-0.5" />
                    </Button>
                </form>
            </div>
        </div>
    );
};

const Messages = () => {
    const [searchParams] = useSearchParams();
    const [conversations, setConversations] = useState([]);
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState('');
    const [selectedContact, setSelectedContact] = useState(null);

    const handleMessagesRead = useCallback((otherUserId) => {
        setConversations(prev => prev.map(c =>
            c.otherUserId === otherUserId ? { ...c, unreadCount: 0 } : c
        ));
    }, []);

    const fetchConversations = async () => {
        try {
            const res = await chatApi.getConversations();
            setConversations(res.data?.data || []);
        } catch (err) {
            toast.error("Failed to load conversations");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchConversations();
        const interval = setInterval(fetchConversations, 15000);
        return () => clearInterval(interval);
    }, []);

    useEffect(() => {
        const requestedUserId = searchParams.get('userId');
        if (!requestedUserId) return;
        const requestedConversation = conversations.find((conversation) => conversation.otherUserId === requestedUserId);
        setSelectedContact(requestedConversation || {
            otherUserId: requestedUserId,
            otherUserName: searchParams.get('userName') || 'Peer participant',
            otherUserAvatar: searchParams.get('userAvatar') || null,
            unreadCount: 0,
        });
    }, [conversations, searchParams]);

    const filteredConversations = conversations.filter(c =>
        c.otherUserName.toLowerCase().includes(search.toLowerCase())
    );

    if (loading) {
        return (
            <div className="flex flex-col items-center justify-center min-h-[60vh] bg-slate-50">
                <Loader2 className="h-10 w-10 animate-spin text-blue-600 mb-4" />
                <p className="text-slate-500 font-medium">Loading your messages...</p>
            </div>
        );
    }

    return (
        <div className="w-full flex flex-col">
            <div className="flex-1 max-w-[1400px] w-full mx-auto p-4 md:p-6 lg:p-8 flex flex-col">
                
                {/* Header */}
                <div className="mb-6">
                    <PageHeader 
                        icon={MessageSquare}
                        title="Messages"
                        description="Chat natively with other SkillScape users."
                        className="mb-0"
                    />
                </div>

                {/* Main 2-Panel Interface */}
                <div className="flex-1 bg-white rounded-[10px] shadow-sm border border-slate-200 overflow-hidden flex min-h-[600px] max-h-[calc(100vh-200px)]">
                    
                    {/* Left Sidebar: Conversations List */}
                    <div className={`w-full md:w-[380px] border-r border-slate-200 flex flex-col bg-white shrink-0 ${selectedContact ? 'hidden md:flex' : 'flex'}`}>
                        <div className="p-4 border-b border-slate-100">
                            <div className="relative">
                                <Search className="absolute left-4 top-3.5 w-4 h-4 text-slate-400" />
                                <input
                                    type="text"
                                    placeholder="Search conversations..."
                                    className="w-full pl-11 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-[10px] text-sm focus:outline-none focus:ring-2 focus:ring-blue-600 focus:border-transparent transition-all shadow-sm font-medium text-slate-900"
                                    value={search}
                                    onChange={(e) => setSearch(e.target.value)}
                                />
                            </div>
                        </div>

                        <ScrollArea className="flex-1">
                            {filteredConversations.length === 0 ? (
                                <div className="p-12 text-center text-slate-500">
                                    <MessageSquare className="w-10 h-10 mx-auto mb-4 text-slate-300" />
                                    <p className="font-medium">No conversations found.</p>
                                </div>
                            ) : (
                                <div className="divide-y divide-slate-100">
                                    {filteredConversations.map((contact) => (
                                        <button
                                            key={contact.otherUserId}
                                            onClick={() => setSelectedContact(contact)}
                                            className={`w-full text-left p-5 hover:bg-slate-50 transition-colors flex items-start gap-4 ${selectedContact?.otherUserId === contact.otherUserId ? 'bg-blue-50/50 relative before:absolute before:left-0 before:top-0 before:bottom-0 before:w-1 before:bg-blue-600 before:rounded-r' : ''
                                                }`}
                                        >
                                            <div className="relative shrink-0">
                                                {contact.otherUserAvatar ? (
                                                    <img src={`${import.meta.env.VITE_API_URL?.replace('/api', '') || 'http://localhost:5000'}${contact.otherUserAvatar}`} alt={contact.otherUserName} className="w-12 h-12 rounded-full object-cover" />
                                                ) : (
                                                    <div className="w-12 h-12 rounded-full bg-slate-100 text-slate-600 flex items-center justify-center font-bold">
                                                        {contact.otherUserName.split(" ").map((n) => n[0]).join("").substring(0, 2)}
                                                    </div>
                                                )}
                                                {contact.unreadCount > 0 && (
                                                    <div className="absolute -top-1 -right-1 w-5 h-5 bg-blue-600 border-2 border-white rounded-full flex items-center justify-center">
                                                        <span className="text-[9px] font-black text-white">{contact.unreadCount}</span>
                                                    </div>
                                                )}
                                            </div>

                                            <div className="flex-1 min-w-0 pt-1">
                                                <div className="flex justify-between items-baseline mb-1">
                                                    <h4 className={`truncate pr-2 ${contact.unreadCount > 0 ? 'font-bold text-slate-900' : 'font-semibold text-slate-900'}`}>
                                                        {contact.otherUserName}
                                                    </h4>
                                                    <span className={`text-xs whitespace-nowrap ${contact.unreadCount > 0 ? 'font-bold text-blue-600' : 'font-semibold text-slate-400'}`}>
                                                        {new Date(contact.lastMessageTime + (!contact.lastMessageTime.endsWith('Z') ? 'Z' : '')).toLocaleDateString([], { month: 'short', day: 'numeric' })}
                                                    </span>
                                                </div>
                                                <p className={`text-sm truncate ${contact.unreadCount > 0 ? 'text-slate-900 font-semibold' : 'text-slate-500 font-medium'}`}>
                                                    {contact.lastMessage}
                                                </p>
                                            </div>
                                        </button>
                                    ))}
                                </div>
                            )}
                        </ScrollArea>
                    </div>

                    {/* Right Content: Active Chat */}
                    <div className={`flex-1 flex-col bg-slate-50 relative ${!selectedContact ? 'hidden md:flex items-center justify-center' : 'flex min-w-0'}`}>
                        {!selectedContact ? (
                            <div className="text-center px-6">
                                <div className="w-20 h-20 bg-white border border-slate-200 rounded-full flex items-center justify-center mx-auto mb-6 shadow-sm">
                                    <MessageSquare className="w-8 h-8 text-blue-600" />
                                </div>
                                <h3 className="text-xl font-bold text-slate-900 mb-2">Your Messages</h3>
                                <p className="text-slate-500 font-medium max-w-sm mx-auto">Select a conversation from the sidebar to begin.</p>
                            </div>
                        ) : (
                            <ChatWindow
                                contact={selectedContact}
                                onClose={() => setSelectedContact(null)}
                                onMessagesRead={handleMessagesRead}
                            />
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Messages;


