import React, { useState, useEffect, useRef } from 'react';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { ScrollArea } from '@/components/ui/scroll-area';
import { MessageCircle, X, Send, Loader2, Sparkles, Trash2, Zap, BookOpen } from 'lucide-react';
import { useAuth } from '@/context/AuthContext';
import { api } from '@/services/api';
import { toast } from 'sonner';

/* ═══════════════════════════════════════════════════════════════════
   MARKDOWN TABLE PARSER
   ═══════════════════════════════════════════════════════════════════ */
const parseMarkdownTable = (lines, startIdx) => {
  const rows = [];
  let i = startIdx;
  while (i < lines.length && lines[i].trim().startsWith('|')) {
    const cols = lines[i].trim().split('|').filter((_, ci) => ci !== 0 || true).map(c => c.trim());
    // Remove empty first/last from split
    const cleaned = cols.filter((c, ci) => !(ci === 0 && c === '') && !(ci === cols.length - 1 && c === ''));
    rows.push(cleaned);
    i++;
  }
  // Second row is separator (---) — skip it
  const headers = rows[0] || [];
  const dataRows = rows.slice(2); // skip separator row
  return { headers, dataRows, endIdx: i };
};

/* ═══════════════════════════════════════════════════════════════════
   RICH MESSAGE RENDERER
   ═══════════════════════════════════════════════════════════════════ */
const renderInline = (text, key) =>
  String(text).split(/(\*\*[^*]+\*\*)/g).filter(Boolean).map((part, i) =>
    part.startsWith('**') && part.endsWith('**')
      ? <strong key={`${key}-b${i}`} className="font-semibold text-slate-900">{part.slice(2, -2)}</strong>
      : <React.Fragment key={`${key}-t${i}`}>{part}</React.Fragment>
  );

const FormattedAssistantMessage = ({ content }) => {
  const raw   = String(content || '').replace(/\r/g, '');
  const lines = raw.split('\n');
  const elements = [];
  let i = 0;

  while (i < lines.length) {
    const line = lines[i];

    // Blank line
    if (!line.trim()) { elements.push(<div key={`sp-${i}`} className="h-1" />); i++; continue; }

    // Heading
    const heading = line.match(/^#{1,3}\s+(.+)$/);
    if (heading) {
      elements.push(
        <h4 key={`h-${i}`} className="mt-3 mb-1 font-bold text-blue-700 text-sm border-b border-blue-100 pb-0.5">
          {renderInline(heading[1], `h${i}`)}
        </h4>
      );
      i++; continue;
    }

    // Horizontal rule
    if (/^---+$/.test(line.trim())) {
      elements.push(<hr key={`hr-${i}`} className="my-2 border-gray-200" />);
      i++; continue;
    }

    // Markdown Table — detect with leading |
    if (line.trim().startsWith('|') && i + 1 < lines.length && /^\|[-| :]+\|/.test(lines[i + 1])) {
      const { headers, dataRows, endIdx } = parseMarkdownTable(lines, i);
      elements.push(
        <div key={`tbl-${i}`} className="my-3 overflow-x-auto rounded-lg border border-gray-200 shadow-sm">
          <table className="min-w-full text-xs">
            <thead className="bg-blue-50">
              <tr>
                {headers.map((h, hi) => (
                  <th key={hi} className="px-3 py-2 text-left font-semibold text-blue-800 border-b border-blue-200">
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {dataRows.map((row, ri) => (
                <tr key={ri} className={ri % 2 === 0 ? 'bg-white' : 'bg-gray-50'}>
                  {headers.map((_, ci) => (
                    <td key={ci} className="px-3 py-2 border-b border-gray-100 text-gray-700">
                      {renderInline(row[ci] || '', `cell-${ri}-${ci}`)}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      );
      i = endIdx; continue;
    }

    // Bullet list
    const bullet = line.match(/^[-*]\s+(.+)$/);
    if (bullet) {
      elements.push(
        <div key={`b-${i}`} className="flex gap-2 items-start py-0.5">
          <span className="mt-2 h-1.5 w-1.5 shrink-0 rounded-full bg-blue-500" />
          <span className="text-gray-700">{renderInline(bullet[1], `b${i}`)}</span>
        </div>
      );
      i++; continue;
    }

    // Numbered list
    const numbered = line.match(/^(\d+)[.)]\s+(.+)$/);
    if (numbered) {
      elements.push(
        <div key={`n-${i}`} className="flex gap-2 items-start py-0.5">
          <span className="font-bold text-blue-600 shrink-0">{numbered[1]}.</span>
          <span className="text-gray-700">{renderInline(numbered[2], `n${i}`)}</span>
        </div>
      );
      i++; continue;
    }

    // Regular paragraph
    elements.push(<p key={`p-${i}`} className="text-gray-800 leading-relaxed">{renderInline(line, `p${i}`)}</p>);
    i++;
  }

  return <div className="space-y-1 text-sm">{elements}</div>;
};

/* ═══════════════════════════════════════════════════════════════════
   CONSTANTS
   ═══════════════════════════════════════════════════════════════════ */
const WELCOME_TEXT = "Hi! I'm Skill Scapy 👋 Your personal AI career guide. Choose Quick or Detailed mode below. What would you like to explore today?";

/* ═══════════════════════════════════════════════════════════════════
   MAIN COMPONENT
   ═══════════════════════════════════════════════════════════════════ */
const ChatBot = () => {
  const { user } = useAuth();
  const [isOpen,      setIsOpen]      = useState(false);
  const [messages,    setMessages]    = useState([]);
  const [loading,     setLoading]     = useState(false);
  const [inputValue,  setInputValue]  = useState('');
  const [responseMode, setResponseMode] = useState('detailed'); // 'quick' | 'detailed'

  const textareaRef        = useRef(null);
  const messagesEndRef     = useRef(null);

  /* ── Auto-resize textarea ─────────────────────────────────────── */
  useEffect(() => {
    const el = textareaRef.current;
    if (!el) return;
    el.style.height = 'auto';
    el.style.height = Math.min(el.scrollHeight, 120) + 'px';
  }, [inputValue]);

  /* ── Scroll to bottom ────────────────────────────────────────── */
  const scrollToBottom = () =>
    setTimeout(() => messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' }), 100);
  useEffect(() => { scrollToBottom(); }, [messages, loading]);

  /* ── Load history ────────────────────────────────────────────── */
  useEffect(() => {
    if (isOpen && user && messages.length === 0) loadConversationHistory();
  }, [isOpen, user]);

    /* ── Load conversation history ───────────────────────────────── */
  const loadConversationHistory = async () => {
    try {
      const res = await api.get('/chatbot/history?limit=5');
      if (res.data?.length > 0) {
        setMessages(res.data.map(m => ({ role: m.role, content: m.content, timestamp: new Date() })));
      } else setInitialMessage();
    } catch { setInitialMessage(); }
  };

  const setInitialMessage = () => {
    setMessages([{ role: 'assistant', content: WELCOME_TEXT, timestamp: new Date() }]);
  };

  /* ── Send message ────────────────────────────────────────────── */
  const handleSendMessage = async (e) => {
    e.preventDefault();
    const trimmed = inputValue.trim();
    if (!trimmed || loading) return;

    
    // Prepend mode instruction to the message
    const modePrefix = responseMode === 'quick'
      ? '[QUICK OVERVIEW - respond in 3-5 concise bullet points, no tables, under 100 words] '
      : '[DETAILED OVERVIEW - respond comprehensively with sections, bullet points, and tables where helpful] ';

    const messageToSend = modePrefix + trimmed;

    setMessages(prev => [...prev, { role: 'user', content: trimmed, timestamp: new Date(), mode: responseMode }]);
    setInputValue('');
    setLoading(true);

    try {
      const historyForAPI = messages.map(m => ({ role: m.role, content: m.content }));
      const response = await api.post('/chatbot/message', {
        message: messageToSend,
        conversationHistory: historyForAPI,
      });
      const chatResponse = response.data;
      const assistantMessage = {
        role: 'assistant',
        content: chatResponse.message,
        timestamp: new Date(),
        careers: chatResponse.careers || [],
        skills:  chatResponse.skills  || [],
        roadmap: chatResponse.roadmap || null,
      };
      setMessages(prev => [...prev, assistantMessage]);
          } catch (error) {
      let msg = "I'm having a temporary issue. Please try again.";
      if (error.response?.status === 401) msg = 'Session expired. Please log in again.';
      else if (error.message === 'Network Error') msg = 'Cannot reach the server. Is the backend running?';
      else msg = error.response?.data?.message || msg;
      setMessages(prev => [...prev, { role: 'assistant', content: msg, timestamp: new Date() }]);
      toast.error('Chat error: ' + msg);
    } finally { setLoading(false); }
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); handleSendMessage(e); }
  };

  const handleClearHistory = async () => {
    try {
      await api.delete('/chatbot/history');
      setInitialMessage();
      toast.success('Conversation cleared');
    } catch { toast.error('Failed to clear conversation'); }
  };

  /* ═══════════════════════════════════════════════════════════════ */
  return (
    <div className="fixed bottom-6 right-6 z-50 flex flex-col items-end">

      {/* ── Chat Widget ─────────────────────────────────────────── */}
      {isOpen && (
        <Card className="w-[440px] h-[calc(100vh-140px)] max-h-[680px] flex flex-col shadow-2xl border border-gray-200 bg-white rounded-2xl overflow-hidden mb-4">

          {/* Header */}
          <div className="bg-gradient-to-r from-blue-600 to-indigo-600 text-white px-5 py-4 flex items-center justify-between flex-shrink-0">
            <div className="flex items-center gap-3">
              <div className="relative">
                <div className="bg-white/20 p-2 rounded-xl backdrop-blur-sm">
                  <Sparkles size={20} className="text-white" />
                </div>
                
              </div>
              <div>
                <h3 className="font-bold text-base">Skill Scapy</h3>
                <p className="text-xs text-blue-200">
                  'AI Career Guide'
                </p>
              </div>
            </div>
            <div className="flex gap-1">
              <Button variant="ghost" size="sm" onClick={handleClearHistory}
                className="text-white hover:bg-white/20 h-8 w-8 p-0 rounded-lg" title="Clear conversation">
                <Trash2 size={15} />
              </Button>
              <Button variant="ghost" size="sm" onClick={() => setIsOpen(false)}
                className="text-white hover:bg-white/20 h-8 w-8 p-0 rounded-lg">
                <X size={17} />
              </Button>
            </div>
          </div>

          {/* Messages */}
          <ScrollArea className="flex-1 bg-gray-50">
            <div className="space-y-4 px-4 py-4">
              {messages.map((msg, idx) => (
                <div key={idx} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
                  {/* Avatar for assistant */}
                  {msg.role === 'assistant' && (
                    <div className="w-7 h-7 rounded-full bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center mr-2 mt-1 flex-shrink-0 shadow-sm">
                      <Sparkles size={12} className="text-white" />
                    </div>
                  )}
                  <div className={`max-w-[80%] px-4 py-3 rounded-2xl shadow-sm text-sm ${
                    msg.role === 'user'
                      ? 'bg-blue-600 text-white rounded-br-sm'
                      : 'bg-white text-gray-900 rounded-bl-sm border border-gray-100 shadow-md'
                  }`}>
                    {/* Mode badge on user messages */}
                    {msg.role === 'user' && msg.mode && (
                      <span className={`inline-block text-[10px] font-semibold px-1.5 py-0.5 rounded-full mb-1 ${
                        msg.mode === 'quick' ? 'bg-blue-500 text-white' : 'bg-indigo-500 text-white'
                      }`}>
                        {msg.mode === 'quick' ? '\u26A1 Quick' : '\uD83D\uDCD6 Detailed'}
                      </span>
                    )}

                    {msg.role === 'assistant'
                      ? <FormattedAssistantMessage content={msg.content} />
                      : <p className="leading-relaxed whitespace-pre-wrap">{msg.content}</p>}

                    {/* Careers */}
                    {msg.careers?.length > 0 && (
                      <div className="mt-3 pt-2 border-t border-gray-100">
                        <p className="text-xs font-bold mb-2 text-gray-600">\uD83D\uDCBC Suggested Careers</p>
                        <div className="space-y-1.5">
                          {msg.careers.map((career, i) => (
                            <div key={i} className="bg-blue-50 px-3 py-2 rounded-lg text-xs border border-blue-100">
                              <p className="font-semibold text-blue-900">{career.title}</p>
                              <div className="flex justify-between mt-0.5 text-gray-500">
                                {Number.isFinite(career.matchScore) && <span>Match: {(career.matchScore * 100).toFixed(0)}%</span>}
                                {career.averageSalary && <span className="text-blue-700 font-medium">{career.averageSalary}</span>}
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}

                    {/* Skills */}
                    {msg.skills?.length > 0 && (
                      <div className="mt-3 pt-2 border-t border-gray-100">
                        <p className="text-xs font-bold mb-2 text-gray-600">\uD83C\uDFAF Required Skills</p>
                        <div className="flex flex-wrap gap-1.5">
                          {msg.skills.slice(0, 6).map((skill, i) => (
                            <span key={i} className="bg-indigo-50 text-indigo-700 text-xs px-2 py-1 rounded-full border border-indigo-100 font-medium">
                              {skill.name}
                            </span>
                          ))}
                        </div>
                      </div>
                    )}

                    {/* Roadmap */}
                    {msg.roadmap && (
                      <div className="mt-3 pt-2 border-t border-gray-100">
                        <p className="text-xs font-bold mb-1.5 text-gray-600">\uD83D\uDDFA\uFE0F Learning Roadmap</p>
                        <div className="bg-indigo-50 px-3 py-2 rounded-lg text-xs border border-indigo-100">
                          <p className="font-semibold text-indigo-900">{msg.roadmap.title}</p>
                          <p className="text-gray-500 mt-0.5">{msg.roadmap.totalDurationMonths} months &bull; {msg.roadmap.phases?.length || 0} phases</p>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              ))}

              {/* Thinking */}
              {loading && (
                <div className="flex justify-start">
                  <div className="w-7 h-7 rounded-full bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center mr-2 mt-1 flex-shrink-0">
                    <Sparkles size={12} className="text-white animate-pulse" />
                  </div>
                  <div className="bg-white border border-gray-100 shadow-md px-4 py-3 rounded-2xl rounded-bl-sm flex items-center gap-2">
                    <Loader2 size={15} className="animate-spin text-blue-500" />
                    <span className="text-sm text-gray-500">Skill Scapy is thinking...</span>
                  </div>
                </div>
              )}
              <div ref={messagesEndRef} />
            </div>
          </ScrollArea>

          {/* Input Area */}
          <div className="border-t border-gray-100 bg-white px-4 py-3 flex-shrink-0">

            {/* Mode selector */}
            <div className="flex items-center justify-between mb-2.5">
              {/* Quick / Detailed toggle */}
              <div className="flex items-center bg-gray-100 rounded-lg p-0.5 gap-0.5">
                <button
                  onClick={() => setResponseMode('quick')}
                  className={`flex items-center gap-1 text-xs px-2.5 py-1 rounded-md font-medium transition-all ${
                    responseMode === 'quick'
                      ? 'bg-white text-blue-600 shadow-sm'
                      : 'text-gray-500 hover:text-gray-700'
                  }`}
                >
                  <Zap size={10} />
                  Quick
                </button>
                <button
                  onClick={() => setResponseMode('detailed')}
                  className={`flex items-center gap-1 text-xs px-2.5 py-1 rounded-md font-medium transition-all ${
                    responseMode === 'detailed'
                      ? 'bg-white text-indigo-600 shadow-sm'
                      : 'text-gray-500 hover:text-gray-700'
                  }`}
                >
                  <BookOpen size={10} />
                  Detailed
                </button>
              </div>
            </div>

            {/* Input row */}
            <form onSubmit={handleSendMessage} className="flex gap-2 items-end">
              

              {/* Auto-expanding textarea */}
              <textarea
                ref={textareaRef}
                value={inputValue}
                onChange={(e) => setInputValue(e.target.value)}
                onKeyDown={handleKeyDown}
                placeholder={`${responseMode === 'quick' ? '⚡ Quick' : '📖 Detailed'} mode • Ask about careers, skills...`}
                rows={1}
                disabled={loading}
                className="flex-1 resize-none text-sm border rounded-xl px-3 py-2.5 outline-none transition-all placeholder:text-gray-400 border-gray-200 focus:border-blue-400 focus:ring-1 focus:ring-blue-100 disabled:opacity-50 bg-gray-50"
                style={{ minHeight: '40px', maxHeight: '120px', overflowY: 'auto' }}
              />

              {/* Send */}
              <Button type="submit" size="sm" disabled={loading || !inputValue.trim()}
                className={`h-10 w-10 p-0 flex-shrink-0 flex items-center justify-center rounded-xl text-white ${
                  responseMode === 'quick'
                    ? 'bg-blue-600 hover:bg-blue-700'
                    : 'bg-indigo-600 hover:bg-indigo-700'
                }`}>
                <Send size={15} />
              </Button>
            </form>

            <p className="text-[10px] text-slate-300 mt-1.5 text-center">
              Skill Scapy AI &bull; Enter to send &bull; Shift+Enter for new line
            </p>
          </div>
        </Card>
      )}

      {/* Floating button */}
      <button
        onClick={() => setIsOpen(p => !p)}
        className={`w-16 h-16 rounded-full shadow-xl flex items-center justify-center transition-all transform hover:scale-110 ${
          isOpen
            ? 'bg-slate-800 hover:bg-slate-900'
            : 'bg-gradient-to-br from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 animate-bounce'
        } text-white`}
        title={isOpen ? 'Close' : 'Chat with Skill Scapy'}>
        {isOpen ? <X size={24} /> : <MessageCircle size={24} />}
      </button>
    </div>
  );
};

export default ChatBot;
