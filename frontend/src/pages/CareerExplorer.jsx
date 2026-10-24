import React, { useState, useEffect } from 'react';
import { createPortal } from 'react-dom';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  Briefcase, Code, Server, ShieldAlert, Sparkles, 
  ChevronRight, ChevronDown, Bookmark, ArrowLeft, Loader2, Landmark, Plus, FlaskConical, Target, Wallet,
  Map, GraduationCap, Network, X, Info
} from 'lucide-react';
import { careerGuidanceApi } from '@/services/api/careerGuidance';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
import PageHeader from '@/components/layout/PageHeader';

const getIcon = (iconName) => {
  switch (iconName?.toLowerCase()) {
    case 'flask': return <FlaskConical className="w-5 h-5" />;
    case 'banknote': return <Landmark className="w-5 h-5" />;
    case 'palette': return <Plus className="w-5 h-5" />;
    case 'binary': return <Code className="w-5 h-5" />;
    case 'dna': return <FlaskConical className="w-5 h-5" />;
    case 'cpu': return <Server className="w-5 h-5" />;
    case 'code': return <Code className="w-5 h-5" />;
    case 'sparkles': return <Sparkles className="w-5 h-5" />;
    case 'server': return <Server className="w-5 h-5" />;
    case 'shield-alert': return <ShieldAlert className="w-5 h-5" />;
    case 'briefcase': return <Briefcase className="w-5 h-5" />;
    case 'graduation-cap': return <GraduationCap className="w-5 h-5" />;
    default: return <Briefcase className="w-5 h-5" />;
  }
};

const STREAMS = [
  { id: 'After10th', label: 'After 10th', desc: 'Explore foundational paths after secondary education.' },
  { id: 'After12th', label: 'After 12th', desc: 'Undergraduate pathways and specialized domains.' },
  { id: 'AfterGraduation', label: 'After Graduation', desc: 'Postgraduate and professional career entries.' },
  { id: 'AfterPostGraduation', label: 'After Post Grad', desc: 'Advanced research and highly specialized roles.' },
  { id: 'CareerSwitch', label: 'Career Switch', desc: 'Pivot to a new domain leveraging your existing skills.' },
  { id: 'GovernmentJobs', label: 'Govt Jobs', desc: 'Public sector and administrative pathways.' }
];

export default function CareerExplorer() {
  const navigate = useNavigate();
  const location = useLocation();

  const [selectedStream, setSelectedStream] = useState(null);
  const [pathsRaw, setPathsRaw] = useState([]);
  const [loading, setLoading] = useState(false);
  
  const [activePath, setActivePath] = useState([]); 
  const [selectedLeaf, setSelectedLeaf] = useState(null);
  
  const [accepting, setAccepting] = useState(false);
  const [bookmarked, setBookmarked] = useState(false);
  
  const [isTreeVisible, setIsTreeVisible] = useState(false);
  
  // Global tree state
  const [isGlobalTreeVisible, setIsGlobalTreeVisible] = useState(false);
  const [globalTreeData, setGlobalTreeData] = useState([]);

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const targetStream = params.get('stream');
    
    if (targetStream && !selectedStream) {
      const match = STREAMS.find(s => s.id === targetStream);
      if (match) {
        handleStreamSelect(match);
      }
    }
  }, [location.search]);

  const fetchPaths = async (streamId) => {
    try {
      setLoading(true);
      const data = await careerGuidanceApi.getPathsTree(streamId);
      
      // Helper to remove redundant middle-man nodes that only have 1 child
      // This prevents users from clicking through "Commerce" -> "Commerce Spec" -> "Commerce Final"
      const pruneRedundantNodes = (nodes) => {
        if (!nodes) return [];
        return nodes.map(node => {
          let current = { ...node };
          
          // Collapse single-child chains by inheriting their children
          while (current.subCareers && current.subCareers.length === 1) {
            const singleChild = current.subCareers[0];
            current.subCareers = singleChild.subCareers;
            // Optionally, we could take the child's description if it's more specific
            if (singleChild.description && singleChild.description.length > current.description?.length) {
                current.description = singleChild.description;
            }
          }
          
          if (current.subCareers && current.subCareers.length > 0) {
            current.subCareers = pruneRedundantNodes(current.subCareers);
          }
          
          return current;
        });
      };

      setPathsRaw(pruneRedundantNodes(data));
    } catch (err) {
      toast.error('Failed to load career paths');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenGlobalTree = async () => {
    try {
      setLoading(true);
      // Fetch stream 1 (After 10th) which contains the full, real career pathways in India
      const data = await careerGuidanceApi.getPathsTree(1);
      
      // For the global tree, we want to see the FULL educational hierarchy (10th -> 12th -> Grad -> Careers)
      // So we DO NOT aggressively prune single children here, as that hides the structural stages.
      setGlobalTreeData(data || []);
      setActivePath([]);
      setIsGlobalTreeVisible(true);
    } catch (err) {
      toast.error('Failed to load global career tree');
    } finally {
      setLoading(false);
    }
  };

  const handleStreamSelect = (stream) => {
    if (stream.id === 'GovernmentJobs') {
      navigate('/government-jobs');
      return;
    }
    if (stream.id === 'CareerSwitch') {
      navigate('/career-switch/pre-assessment');
      return;
    }
    setSelectedStream(stream);
    setActivePath([]);
    setSelectedLeaf(null);
    fetchPaths(stream.id);
  };

  const handleNodeSelect = (node, depth) => {
    if (node.subCareers && node.subCareers.length > 0) {
      setActivePath(prev => {
        // If clicking the currently active node, collapse it
        if (prev[depth]?.id === node.id) {
           return prev.slice(0, depth);
        }
        // Otherwise, expand it
        const newPath = prev.slice(0, depth);
        return [...newPath, node];
      });
      setSelectedLeaf(null);
    } else {
      setSelectedLeaf(node);
      setActivePath(prev => {
        const newPath = prev.slice(0, depth);
        return [...newPath, node];
      });
      
      // If clicking a leaf node inside the modal, close the modal so they can see the details panel
      if (isTreeVisible) {
        setIsTreeVisible(false);
      }
    }
  };

  const handleToggleBookmark = async () => {
    if (!selectedLeaf) return;
    try {
      await careerGuidanceApi.toggleBookmark(selectedLeaf.id);
      setBookmarked(prev => !prev);
      toast.success(bookmarked ? 'Removed from bookmarks' : 'Saved to bookmarks');
    } catch (err) {
      toast.error('Failed to update bookmark');
    }
  };

  const handleBreadcrumbClick = (depth) => {
    if (depth === -1) {
      setSelectedStream(null);
      setActivePath([]);
      setSelectedLeaf(null);
      setPathsRaw([]);
      setIsTreeVisible(false);
    } else {
      setActivePath(prev => prev.slice(0, depth + 1));
      setSelectedLeaf(null);
    }
  };

  const handleBack = () => {
    if (selectedLeaf) {
      setSelectedLeaf(null);
      setActivePath(prev => prev.slice(0, prev.length - 1));
    } else if (activePath.length > 0) {
      setActivePath(prev => prev.slice(0, prev.length - 1));
    } else if (selectedStream) {
      setSelectedStream(null);
      setPathsRaw([]);
      setIsTreeVisible(false);
    }
  };

  // ---------------------------------------------------------
  // TEXTUAL LIST COMPONENT (MAIN PAGE)
  // ---------------------------------------------------------

  const CareerListNode = ({ node, depth }) => {
    const isActive = activePath[depth]?.id === node.id;
    const isLeafSelected = selectedLeaf?.id === node.id;
    const hasChildren = node.subCareers && node.subCareers.length > 0;
    const isExpanded = isActive && hasChildren && !selectedLeaf;

    return (
      <div className={`relative h-full flex flex-col ${isExpanded ? 'col-span-full' : ''}`}>
        <button
          onClick={() => handleNodeSelect(node, depth)}
          className={`
            w-full text-left p-5 rounded-xl border transition-all duration-300 flex flex-col justify-between group h-full min-h-[180px]
            ${(isActive || isLeafSelected) 
                ? 'bg-blue-50 border-blue-400 shadow-sm' 
                : 'bg-white border-slate-200 hover:border-blue-300 hover:shadow-md'}
          `}
        >
          <div className="flex flex-col gap-3 w-full">
            <div className={`p-2.5 rounded-lg w-fit ${(isActive || isLeafSelected) ? 'bg-blue-100 text-blue-700' : 'bg-slate-100 text-slate-600 group-hover:bg-blue-50 group-hover:text-blue-600'}`}>
              {getIcon(node.icon)}
            </div>
            <div>
              <h3 className={`font-bold text-base leading-tight mb-1.5 ${(isActive || isLeafSelected) ? 'text-blue-900' : 'text-slate-800'}`}>
                {node.title}
              </h3>
              <p className="text-xs text-slate-500 line-clamp-3">{node.description}</p>
            </div>
          </div>
          
          {hasChildren && (
            <div className="w-full flex justify-end mt-4">
              {isExpanded ? (
                <ChevronDown className="w-5 h-5 text-blue-600" />
              ) : (
                <ChevronRight className="w-5 h-5 text-slate-400 group-hover:text-blue-500" />
              )}
            </div>
          )}
        </button>

        <AnimatePresence>
          {isExpanded && (
            <motion.div
              initial={{ opacity: 0, height: 0 }}
              animate={{ opacity: 1, height: 'auto' }}
              exit={{ opacity: 0, height: 0 }}
              className="ml-8 mt-4 relative"
            >
              <div className="absolute top-0 bottom-4 left-[-20px] w-px bg-slate-200" />
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {node.subCareers.map((childNode) => (
                  <CareerListNode key={childNode.id} node={childNode} depth={depth + 1} />
                ))}
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    );
  };

  // ---------------------------------------------------------
  // HORIZONTAL TREE COMPONENT (MODAL)
  // ---------------------------------------------------------

  const HorizontalTreeNode = ({ node, depth }) => {
    // Determine if this node is part of the currently active/expanded path
    const isActive = activePath[depth]?.id === node.id;
    const isExpanded = isActive;
    const hasChildren = node.subCareers && node.subCareers.length > 0;

    // Filter children: if a child is active (clicked), hide its siblings
    const activeChild = hasChildren ? node.subCareers.find(c => activePath[depth + 1]?.id === c.id) : null;
    const childrenToShow = activeChild ? [activeChild] : node.subCareers;

    return (
      <div className="flex flex-col items-center">
        {/* Node Card */}
        <div 
          className={`
            relative flex flex-col bg-white border-2 rounded-xl shadow-sm w-48 transition-all
            ${isActive ? 'border-blue-500 shadow-blue-500/10' : 'border-slate-200 hover:border-blue-300'}
          `}
        >
          <div className={`p-2 border-b ${isActive ? 'bg-blue-50 border-blue-100' : 'bg-slate-50 border-slate-100'} rounded-t-xl flex items-center gap-2`}>
            <div className={`p-1.5 rounded-lg ${isActive ? 'bg-blue-600 text-white' : 'bg-slate-200 text-slate-700'}`}>
              {getIcon(node.icon)}
            </div>
            <h3 className={`font-bold text-[11px] leading-tight ${isActive ? 'text-blue-900' : 'text-slate-900'}`}>
              {node.title}
            </h3>
          </div>
          <div className="p-2 bg-white rounded-b-xl flex-grow flex flex-col justify-between">
            <p className="text-[10px] text-slate-500 line-clamp-3 mb-2">
              {node.description}
            </p>
            
            {hasChildren ? (
              <Button 
                size="sm" 
                className={`w-full text-xs h-8 ${isActive ? 'bg-blue-600 text-white hover:bg-blue-700 border-none' : 'bg-blue-50 text-blue-700 hover:bg-blue-100 border border-blue-200 shadow-none'}`}
                onClick={() => handleNodeSelect(node, depth)}
              >
                {isActive ? 'Hide Branches' : 'Explore Branches'}
              </Button>
            ) : (
              <Button 
                size="sm" 
                className="w-full text-xs h-8 bg-slate-900 text-white hover:bg-slate-800"
                onClick={() => handleNodeSelect(node, depth)}
              >
                View Details <Info className="w-3 h-3 ml-1"/>
              </Button>
            )}
          </div>
        </div>

        {/* Children Container */}
        <AnimatePresence>
          {isExpanded && hasChildren && (
            <motion.div 
              initial={{ opacity: 0, y: -10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              className="flex flex-col items-center"
            >
              {/* Vertical line from parent dropping down to horizontal level */}
              <div className="w-px h-8 bg-slate-300"></div>

              {/* Flex row for children */}
              <div className="flex justify-center">
                {childrenToShow.map((child, idx) => {
                  const isFirst = idx === 0;
                  const isLast = idx === childrenToShow.length - 1;
                  const isOnly = childrenToShow.length === 1;

                  return (
                    <div key={child.id} className="relative flex flex-col items-center px-4">
                      {/* Horizontal connecting lines */}
                      {!isOnly && (
                        <div className="absolute top-0 left-0 right-0 flex w-full">
                          <div className={`w-1/2 border-t-2 border-slate-300 ${isFirst ? 'border-transparent' : ''}`}></div>
                          <div className={`w-1/2 border-t-2 border-slate-300 ${isLast ? 'border-transparent' : ''}`}></div>
                        </div>
                      )}
                      
                      {/* Vertical line dropping down to this child */}
                      <div className="w-px h-8 bg-slate-300"></div>
                      
                      {/* Recursive Child Node */}
                      <HorizontalTreeNode node={child} depth={depth + 1} />
                    </div>
                  );
                })}
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    );
  };


  // ---------------------------------------------------------
  // RENDER BREADCRUMBS
  // ---------------------------------------------------------
  const renderBreadcrumbs = () => (
    <div className="flex flex-wrap items-center gap-2 text-sm text-slate-500 font-medium mb-6">
      <button 
        onClick={() => handleBreadcrumbClick(-1)} 
        className={`hover:text-blue-600 transition-colors ${!selectedStream ? 'text-slate-900 font-bold' : ''}`}
      >
        Career Map
      </button>
      
      {selectedStream && (
        <>
          <ChevronRight className="w-4 h-4 text-slate-300 shrink-0" />
          <button 
            onClick={() => handleBreadcrumbClick(-1)} 
            className={`hover:text-blue-600 transition-colors ${activePath.length === 0 ? 'text-slate-900 font-bold' : ''}`}
          >
            {selectedStream.label}
          </button>
        </>
      )}

      {activePath.map((node, idx) => (
        <React.Fragment key={node.id}>
          <ChevronRight className="w-4 h-4 text-slate-300 shrink-0" />
          <button 
            onClick={() => handleBreadcrumbClick(idx)} 
            className={`hover:text-blue-600 transition-colors ${idx === activePath.length - 1 ? 'text-slate-900 font-bold' : ''}`}
          >
            {node.title}
          </button>
        </React.Fragment>
      ))}
    </div>
  );

  return (
    <div className="w-full relative">
      <div className="container mx-auto px-4 max-w-5xl">
        
        <PageHeader 
          icon={Map}
          eyebrow={renderBreadcrumbs()}
          title="Career Map"
          description="Explore your career path interactively. Drill down from major stages to specialized roles."
          action={
            <Button 
              onClick={() => {
                if (!selectedStream) {
                  handleOpenGlobalTree();
                } else {
                  setIsTreeVisible(true);
                }
              }}
              variant="outline"
              className="bg-white border-slate-200 text-slate-700 hover:text-blue-700 hover:bg-blue-50 transition-colors shadow-sm"
            >
              <Network className="w-4 h-4 mr-2" />
              View Career Tree
            </Button>
          }
        />

        {/* Action Bar */}
        {selectedStream && (
          <div className="mb-6 flex flex-col sm:flex-row sm:items-center justify-start gap-4">
            <Button variant="ghost" onClick={handleBack} className="text-slate-600 hover:text-blue-600 hover:bg-blue-50 px-3 w-fit">
              <ArrowLeft className="w-4 h-4 mr-2" /> Back
            </Button>
          </div>
        )}

        <AnimatePresence mode="wait">
          
          {/* STAGE 1: ROOT STAGES (Grid) */}
          {!selectedStream && (
            <motion.div
              key="root-stages"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.98 }}
              className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4"
            >
              {STREAMS.map(stream => (
                <button
                  key={stream.id}
                  onClick={() => handleStreamSelect(stream)}
                  className="bg-white border border-slate-200 rounded-[12px] p-6 text-left hover:border-blue-600 hover:shadow-lg hover:shadow-blue-600/5 transition-all group flex flex-col h-full"
                >
                  <div className="bg-blue-50 text-blue-600 p-3 rounded-xl w-fit mb-4 group-hover:bg-blue-600 group-hover:text-white transition-colors">
                    <GraduationCap className="w-6 h-6" />
                  </div>
                  <h3 className="text-lg font-bold text-slate-900 group-hover:text-blue-600 transition-colors mb-2">{stream.label}</h3>
                  <p className="text-sm text-slate-500 leading-relaxed flex-grow">{stream.desc}</p>
                </button>
              ))}
            </motion.div>
          )}

          {/* STAGE 2: TEXTUAL MAP */}
          {selectedStream && !selectedLeaf && (
            <motion.div
              key="path-view"
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: 20 }}
              className="bg-white border border-slate-200 rounded-2xl p-6 md:p-8 shadow-sm max-w-6xl mx-auto"
            >
              <div className="mb-8">
                <h2 className="text-2xl font-bold text-slate-900">{selectedStream.label} Pathway</h2>
                <p className="text-slate-500 mt-1">Select a branch to explore deeper specializations.</p>
              </div>

              {loading ? (
                <div className="py-20 flex justify-center"><Loader2 className="w-8 h-8 animate-spin text-blue-600" /></div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-4">
                  {pathsRaw.map(rootNode => (
                    <CareerListNode key={rootNode.id} node={rootNode} depth={0} />
                  ))}
                </div>
              )}
            </motion.div>
          )}

          {/* STAGE 3: DETAILS PANEL (Leaf Node) */}
          {selectedLeaf && (
            <motion.div
              key="details"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -20 }}
              className="bg-white border border-slate-200 rounded-2xl overflow-hidden shadow-sm max-w-5xl mx-auto"
            >
              {/* Cover Banner */}
              <div className="bg-slate-900 p-8 text-white relative">
                <div className="absolute top-0 right-0 p-8 opacity-10">
                   {getIcon(selectedLeaf.icon)}
                </div>
                <span className="text-xs font-bold uppercase tracking-wider text-blue-400 mb-3 block">
                  Final Destination
                </span>
                <h2 className="text-3xl md:text-4xl font-bold mb-4">{selectedLeaf.title}</h2>
                <p className="text-slate-300 max-w-3xl leading-relaxed text-lg">
                  {selectedLeaf.description}
                </p>

                <div className="flex flex-wrap items-center gap-6 mt-8">
                  <div className="bg-slate-800/50 backdrop-blur px-4 py-2 rounded-lg border border-slate-700">
                    <p className="text-[10px] text-slate-400 uppercase font-bold mb-0.5">Industry Growth</p>
                    <p className="text-xl font-bold text-blue-500">{selectedLeaf.industryGrowth}% CAGR</p>
                  </div>
                  <div className="bg-slate-800/50 backdrop-blur px-4 py-2 rounded-lg border border-slate-700">
                    <p className="text-[10px] text-slate-400 uppercase font-bold mb-0.5">AI Disruption Risk</p>
                    <p className={`text-xl font-bold ${selectedLeaf.automationRisk >= 50 ? 'text-rose-400' : 'text-blue-500'}`}>
                      {selectedLeaf.automationRisk}%
                    </p>
                  </div>
                  <div className="bg-slate-800/50 backdrop-blur px-4 py-2 rounded-lg border border-slate-700">
                    <p className="text-[10px] text-slate-400 uppercase font-bold mb-0.5">Work Environment</p>
                    <p className="text-xl font-bold text-white">{selectedLeaf.workEnvironment}</p>
                  </div>
                </div>
              </div>

              {/* Body Content */}
              <div className="p-8">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-12">
                  <div className="md:col-span-2 space-y-10">
                    
                    {(selectedLeaf.requiredDegrees?.length > 0 || selectedLeaf.difficultyLevel) && (
                      <section>
                        <h3 className="text-lg font-bold text-slate-900 mb-4 flex items-center gap-2">
                          <Target className="w-5 h-5 text-blue-600" /> Core Requirements
                        </h3>
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                          {selectedLeaf.requiredDegrees?.length > 0 && (
                            <div className="bg-slate-50 p-4 rounded-lg border border-slate-200">
                              <span className="text-xs font-semibold text-slate-500 uppercase block mb-1">Education Needed</span>
                              <p className="text-sm font-medium text-slate-900">{selectedLeaf.requiredDegrees.join(', ')}</p>
                            </div>
                          )}
                          {selectedLeaf.difficultyLevel && (
                            <div className="bg-slate-50 p-4 rounded-lg border border-slate-200">
                              <span className="text-xs font-semibold text-slate-500 uppercase block mb-1">Difficulty</span>
                              <p className="text-sm font-medium text-slate-900">{selectedLeaf.difficultyLevel}</p>
                            </div>
                          )}
                        </div>
                      </section>
                    )}

                    {selectedLeaf.skillsRequired?.length > 0 && (
                      <section>
                        <h3 className="text-lg font-bold text-slate-900 mb-4 flex items-center gap-2">
                          <Code className="w-5 h-5 text-blue-600" /> Required Skills
                        </h3>
                        <div className="flex flex-wrap gap-2">
                          {selectedLeaf.skillsRequired.map((skill, i) => (
                            <span key={i} className="bg-blue-50 text-blue-700 border border-blue-200 px-3 py-1.5 rounded-md text-sm font-medium">
                              {skill}
                            </span>
                          ))}
                        </div>
                      </section>
                    )}

                    {selectedLeaf.topCompanies?.length > 0 && (
                      <section>
                        <h3 className="text-lg font-bold text-slate-900 mb-4 flex items-center gap-2">
                          <Briefcase className="w-5 h-5 text-blue-600" /> Top Companies
                        </h3>
                        <div className="flex flex-wrap gap-2">
                          {selectedLeaf.topCompanies.map((company, i) => (
                            <span key={i} className="bg-slate-100 text-slate-700 border border-slate-200 px-3 py-1.5 rounded-md text-sm font-medium">
                              {company}
                            </span>
                          ))}
                        </div>
                      </section>
                    )}
                  </div>

                  {/* Sidebar Action Panel */}
                  <div className="space-y-6">
                    <div className="bg-blue-50 border border-blue-200 rounded-xl p-6 text-center">
                      <Wallet className="w-8 h-8 text-blue-600 mx-auto mb-3" />
                      <h4 className="text-sm font-bold text-slate-900 mb-1">Salary Potential</h4>
                      <p className="text-3xl font-black text-blue-600 mb-4">
                        {selectedLeaf.salaryData ? `₹${(Math.max(...Object.values(selectedLeaf.salaryData)) / 100000).toFixed(1)}L` : 'N/A'}
                      </p>
                      <Link to={`/salary-forecast?id=${selectedLeaf.id}`}>
                        <Button className="w-full bg-blue-600 hover:bg-blue-700 text-white">
                          Forecast Salary
                        </Button>
                      </Link>
                    </div>

                    <div className="space-y-3">
                      <Button
                        onClick={() => handleAcceptCareer(selectedLeaf.id)}
                        disabled={accepting}
                        className="w-full bg-slate-900 hover:bg-slate-800 text-white py-6 rounded-xl font-bold"
                      >
                        {accepting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : null}
                        Accept Career Target
                      </Button>
                      
                      <Button
                        onClick={handleToggleBookmark}
                        variant="outline"
                        className="w-full border-slate-300 text-slate-700 rounded-xl font-bold hover:bg-slate-50"
                      >
                        <Bookmark className={`w-4 h-4 mr-2 ${bookmarked ? 'fill-blue-600 text-blue-600' : ''}`} />
                        {bookmarked ? 'Saved to Bookmarks' : 'Save for Later'}
                      </Button>
                    </div>
                  </div>
                </div>
              </div>
            </motion.div>
          )}

        </AnimatePresence>
      </div>

      {/* FULLSCREEN TREE MODAL */}
      {createPortal(
        <AnimatePresence>
          {((isTreeVisible && selectedStream) || isGlobalTreeVisible) && (
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 z-[99999] flex items-center justify-center bg-slate-900/60 backdrop-blur-sm p-4 md:p-8"
            >
            <div className="bg-slate-50 w-full h-full max-w-[98vw] md:max-w-[95vw] max-h-[95vh] rounded-lg shadow-2xl flex flex-col overflow-hidden border border-slate-300">
              {/* Modal Header */}
              <div className="flex-shrink-0 bg-white border-b border-slate-200 p-4 flex items-center justify-between shadow-sm px-4 md:px-8">
                <div className="flex items-center gap-3">
                  {!isGlobalTreeVisible && (
                    <Button 
                      variant="ghost" 
                      onClick={handleBack} 
                      className="text-slate-600 hover:text-blue-600 hover:bg-blue-50 mr-2"
                    >
                      <ArrowLeft className="w-4 h-4 md:mr-2" /> <span className="hidden md:inline">Back</span>
                    </Button>
                  )}
                  {!isGlobalTreeVisible && <div className="h-6 w-px bg-slate-200 hidden md:block"></div>}
                  <div className="p-2 bg-blue-100 text-blue-600 rounded-lg hidden sm:block">
                    <Network className="w-5 h-5" />
                  </div>
                  <div>
                    <h2 className="text-lg md:text-xl font-bold text-slate-900">
                      {isGlobalTreeVisible ? "End-to-End Career Tree" : `Career Tree: ${selectedStream?.label}`}
                    </h2>
                    <p className="text-xs md:text-sm text-slate-500 font-medium hidden sm:block">Explore the complete organizational pathway.</p>
                  </div>
                </div>
                <Button 
                  variant="ghost" 
                  onClick={() => {
                    setIsTreeVisible(false);
                    setIsGlobalTreeVisible(false);
                  }}
                  className="hover:bg-rose-50 hover:text-rose-600 rounded-full h-10 w-10 p-0 ml-2"
                >
                  <X className="w-5 h-5" />
                </Button>
              </div>

              {/* Modal Canvas (Scrollable) */}
              <div className="flex-grow overflow-auto relative w-full h-full">
                <div className="min-w-max min-h-full p-12 flex justify-center">
                  
                  {/* Root Level Node (Stream Indicator) */}
                  <div className="flex flex-col items-center inline-flex">
                     <div className="flex items-center gap-3 px-6 py-3 rounded-full bg-slate-900 text-white font-bold text-sm shadow-xl z-10 relative">
                       <GraduationCap className="w-5 h-5 text-blue-400" />
                       {isGlobalTreeVisible ? "End-to-End Careers" : `${selectedStream?.label} Start Point`}
                     </div>
                     
                     {/* Line down to children */}
                     {((isGlobalTreeVisible ? globalTreeData : pathsRaw).length > 0) && (
                       <div className="w-px h-12 bg-slate-300"></div>
                     )}

                     {/* Root level horizontal spread */}
                     {((isGlobalTreeVisible ? globalTreeData : pathsRaw).length > 0) && (
                       <div className="flex justify-center">
                          {(() => {
                             const dataToUse = isGlobalTreeVisible ? globalTreeData : pathsRaw;
                             const activeRoot = dataToUse.find(n => activePath[0]?.id === n.id);
                             const rootNodesToShow = activeRoot ? [activeRoot] : dataToUse;
                             
                             return rootNodesToShow.map((rootNode, idx) => {
                               const isFirst = idx === 0;
                               const isLast = idx === rootNodesToShow.length - 1;
                               const isOnly = rootNodesToShow.length === 1;

                               return (
                                 <div key={rootNode.id} className="relative flex flex-col items-center px-2 md:px-4">
                                   {!isOnly && (
                                     <div className="absolute top-0 left-0 right-0 flex w-full">
                                       <div className={`w-1/2 border-t-2 border-slate-300 ${isFirst ? 'border-transparent' : ''}`}></div>
                                       <div className={`w-1/2 border-t-2 border-slate-300 ${isLast ? 'border-transparent' : ''}`}></div>
                                     </div>
                                   )}
                                   <div className="w-px h-8 bg-slate-300"></div>
                                   <HorizontalTreeNode node={rootNode} depth={0} />
                                 </div>
                               )
                             });
                          })()}
                       </div>
                     )}
                  </div>

                </div>
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>,
      document.body
    )}

    </div>
  );
}
