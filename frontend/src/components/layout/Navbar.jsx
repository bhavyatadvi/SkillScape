import React, { useState, useEffect } from 'react';
import { NavLink, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';
import { notificationsApi } from '@/services/api';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import {
  User,
  Settings,
  LogOut,
  ChevronDown,
  LayoutDashboard,
  Compass,
  Map,
  Route,
  Brain,
  Users,
  Menu,
  X,
  DollarSign,
  BarChart3,
  ListChecks,
  Video,
  BellRing,
  UserCircle2,
} from 'lucide-react';
import { AnimatePresence, motion } from 'framer-motion';

/* ──────────────────────────────────────────────
   Navigation items by role
   ────────────────────────────────────────────── */
const studentNavItems = [
  { path: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { path: '/quiz', label: 'Career Quiz', icon: Compass },
  { path: '/career-explorer', label: 'Career Map', icon: Map },
  { path: '/roadmap', label: 'Roadmap', icon: Route },
  { path: '/placement', label: 'Skill Analyzer', icon: Brain },
  { path: '/career-analytics', label: 'Career Analytics', icon: BarChart3 },
  { path: '/learning-circles', label: 'Learning Circles', icon: Video },
];

const adminNavItems = [
  { path: '/admin', label: 'Dashboard', icon: LayoutDashboard },
  { path: '/admin/users', label: 'Users', icon: Users },
  { path: '/admin/questions', label: 'Questions', icon: ListChecks },
  { path: '/admin/roadmap', label: 'Roadmap', icon: Map },
  { path: '/admin/peer-groups', label: 'Peer Groups', icon: Users },
  { path: '/admin/analytics', label: 'Analytics', icon: BarChart3 },
];

/* ──────────────────────────────────────────────
   Logo Component
   ────────────────────────────────────────────── */
const SkillScapeLogo = ({ onClick }) => (
  <div className="flex items-center cursor-pointer select-none" onClick={onClick}>
    <img src="/logo.png" alt="SkillScape Logo" className="h-[60px] object-contain" />
  </div>
);

/* ──────────────────────────────────────────────
   Navbar Component
   ────────────────────────────────────────────── */
export const Navbar = () => {
  const { user, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [isScrolled, setIsScrolled] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [notifications, setNotifications] = useState([]);

  // Listen for global SignalR notifications dispatched from DashboardLayout
  useEffect(() => {
    const handleNotification = (event) => {
      setNotifications(prev => [event.detail, ...prev]);
    };
    window.addEventListener('skillscape:notification-received', handleNotification);
    return () => window.removeEventListener('skillscape:notification-received', handleNotification);
  }, []);

  // Fetch initial notifications from backend
  useEffect(() => {
    if (user) {
      notificationsApi.getNotifications()
        .then(data => setNotifications(data))
        .catch(err => console.error("Failed to load notifications:", err));
    }
  }, [user]);

  const handleMarkAllRead = async () => {
    try {
      await notificationsApi.markAllRead();
      setNotifications([]);
    } catch (err) {
      console.error("Failed to mark notifications as read:", err);
    }
  };

  // Scroll detection for shadow
  useEffect(() => {
    const handleScroll = () => setIsScrolled(window.scrollY > 4);
    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  // Close mobile menu on route change
  useEffect(() => {
    setMobileMenuOpen(false);
  }, [location.pathname]);

  // Select nav items based on user role
  const getNavItems = () => {
    if (user?.role === 'Admin') return adminNavItems;
    return studentNavItems;
  };

  const navItems = getNavItems();

  return (
    <>
      <header
        className={cn(
          'sticky top-0 z-50 flex h-[72px] items-center border-b bg-white/95 backdrop-blur-md px-4 lg:px-6 transition-shadow duration-200',
          isScrolled ? 'shadow-soft border-[#E2E8F0]' : 'border-transparent'
        )}
      >
        {/* LEFT — Logo */}
        <div className="flex items-center mr-8">
          <SkillScapeLogo onClick={() => navigate('/dashboard')} />
        </div>

        {/* CENTER — Navigation (desktop) */}
        <nav className="hidden lg:flex items-center gap-1 flex-1">
          {navItems.map((item) => {
            const isActive = location.pathname === item.path;
            return (
              <NavLink
                key={item.path}
                to={item.path}
                className={cn(
                  'flex items-center gap-2 px-3 py-2 text-sm font-medium rounded-md transition-colors duration-150',
                  isActive
                    ? 'bg-[#EFF6FF] text-[#2563EB]'
                    : 'text-[#64748B] hover:text-[#0F172A] hover:bg-[#F1F5F9]'
                )}
              >
                <item.icon className="h-4 w-4" />
                {item.label}
              </NavLink>
            );
          })}
        </nav>

        {/* RIGHT — Actions */}
        <div className="flex items-center gap-2 ml-auto">

          {/* Notifications */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="relative p-2 rounded-full hover:bg-[#F1F5F9] text-[#64748B]">
                <BellRing className="w-5 h-5" />
                {notifications.length > 0 && (
                  <span className="absolute top-1.5 right-1.5 h-2 w-2 rounded-full bg-red-500 animate-pulse border-2 border-white" />
                )}
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-80 p-0 border-[#E2E8F0] shadow-sm rounded-xl">
              <div className="p-4 border-b border-[#E2E8F0] flex justify-between items-center bg-white rounded-t-xl">
                <span className="font-bold text-[#0F172A]">Notifications</span>
                {notifications.length > 0 && (
                  <span className="text-[11px] font-semibold text-blue-600 bg-blue-50 px-2.5 py-1 rounded-full cursor-pointer hover:bg-blue-100 transition-colors" onClick={handleMarkAllRead}>Mark all read</span>
                )}
              </div>
              <div className="max-h-80 overflow-y-auto">
                {notifications.length === 0 ? (
                  <div className="p-8 text-center text-slate-500 text-sm">
                    No new notifications
                  </div>
                ) : (
                  notifications.map((notif, i) => (
                    <DropdownMenuItem 
                      key={i} 
                      className="flex flex-col items-start gap-1 p-4 border-b border-slate-50 last:border-0 hover:!bg-slate-50 focus:!bg-slate-50 focus:!text-slate-900 cursor-pointer rounded-none" 
                      onClick={() => {
                        if (notif.type === 'join_request') navigate(`/learning-circles/${notif.circleId || ''}`);
                        if (notif.type === 'join_request_processed') navigate(`/learning-circles`);
                      }}
                    >
                      <span className="font-semibold text-sm text-slate-900">{notif.title}</span>
                      <span className="text-xs text-slate-600 line-clamp-2">{notif.message}</span>
                    </DropdownMenuItem>
                  ))
                )}
              </div>
            </DropdownMenuContent>
          </DropdownMenu>

          {/* User Menu */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="ghost"
                className="flex items-center gap-2 h-9 px-2.5 rounded-md bg-[#EFF6FF] hover:bg-[#DBEAFE] transition-colors"
              >
                <div className="flex h-7 w-7 items-center justify-center rounded-full bg-[#2563EB] shrink-0">
                  <span className="text-xs font-semibold text-white">
                    {user?.name?.charAt(0).toUpperCase() || user?.fullName?.charAt(0).toUpperCase() || 'U'}
                  </span>
                </div>
                <span className="hidden md:block text-sm font-medium text-[#0F172A] max-w-[120px] truncate">
                  {user?.name || user?.fullName || 'User'}
                </span>
                <ChevronDown className="h-3.5 w-3.5 text-[#64748B]" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-56 rounded-[10px] border-[#E2E8F0]">
              <DropdownMenuLabel>
                <div className="flex flex-col">
                  <span className="text-[#0F172A] font-semibold">{user?.name || user?.fullName || 'User'}</span>
                  <span className="text-xs font-normal text-[#64748B]">{user?.email || ''}</span>
                </div>
              </DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem className="cursor-pointer text-[#0F172A] focus:bg-[#EFF6FF] focus:text-[#2563EB]" onClick={() => navigate('/profile')}>
                <User className="mr-2 h-4 w-4 text-[#2563EB]" />
                <span>Profile</span>
              </DropdownMenuItem>
              {user?.role !== 'Admin' && (
                <DropdownMenuItem className="cursor-pointer text-[#0F172A] focus:bg-[#EFF6FF] focus:text-[#2563EB]" onClick={() => navigate('/career-profile')}>
                  <UserCircle2 className="mr-2 h-4 w-4 text-[#2563EB]" />
                  <span>Career Profile</span>
                </DropdownMenuItem>
              )}
              <DropdownMenuItem className="cursor-pointer text-[#0F172A] focus:bg-[#EFF6FF] focus:text-[#2563EB]">
                <Settings className="mr-2 h-4 w-4 text-[#2563EB]" />
                <span>Settings</span>
              </DropdownMenuItem>
              <DropdownMenuSeparator className="bg-[#E2E8F0]" />
              <DropdownMenuItem onClick={logout} className="cursor-pointer text-[#DC2626] focus:bg-red-50 focus:text-red-700">
                <LogOut className="mr-2 h-4 w-4" />
                <span>Logout</span>
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>

          {/* Mobile menu toggle */}
          <Button
            variant="ghost"
            size="icon"
            className="lg:hidden h-9 w-9 rounded-md text-[#64748B]"
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
          >
            {mobileMenuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </Button>
        </div>
      </header>

      {/* Mobile Navigation Drawer */}
      <AnimatePresence>
        {mobileMenuOpen && (
          <>
            {/* Overlay */}
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 z-40 bg-black/20 backdrop-blur-sm lg:hidden"
              onClick={() => setMobileMenuOpen(false)}
            />
            {/* Drawer */}
            <motion.div
              initial={{ x: '100%' }}
              animate={{ x: 0 }}
              exit={{ x: '100%' }}
              transition={{ type: 'spring', damping: 25, stiffness: 300 }}
              className="fixed top-0 right-0 z-50 h-full w-72 bg-white border-l border-[#E2E8F0] shadow-xl lg:hidden"
            >
              <div className="flex items-center justify-between p-4 border-b border-[#E2E8F0]">
                <SkillScapeLogo onClick={() => { setMobileMenuOpen(false); navigate('/dashboard'); }} />
                <Button variant="ghost" size="icon" onClick={() => setMobileMenuOpen(false)} className="h-8 w-8 rounded-md">
                  <X className="h-4 w-4" />
                </Button>
              </div>
              <nav className="p-3 space-y-0.5">
                {navItems.map((item) => {
                  const isActive = location.pathname === item.path;
                  return (
                    <NavLink
                      key={item.path}
                      to={item.path}
                      onClick={() => setMobileMenuOpen(false)}
                      className={cn(
                        'flex items-center gap-3 px-3 py-2.5 text-sm font-medium rounded-md transition-colors',
                        isActive
                          ? 'bg-[#EFF6FF] text-[#2563EB]'
                          : 'text-[#475569] hover:bg-[#F1F5F9] hover:text-[#0F172A]'
                      )}
                    >
                      <item.icon className="h-4.5 w-4.5" />
                      {item.label}
                    </NavLink>
                  );
                })}

                {/* Extra links for students */}
                {user?.role !== 'Admin' && (
                  <>
                    <div className="pt-3 mt-3 border-t border-[#E2E8F0]">
                      <NavLink
                        to="/salary-forecast"
                        onClick={() => setMobileMenuOpen(false)}
                        className={cn(
                          'flex items-center gap-3 px-3 py-2.5 text-sm font-medium rounded-md transition-colors',
                          location.pathname === '/salary-forecast'
                            ? 'bg-[#EFF6FF] text-[#2563EB]'
                            : 'text-[#475569] hover:bg-[#F1F5F9]'
                        )}
                      >
                        <DollarSign className="h-4 w-4" />
                        Salary Forecast
                      </NavLink>
                      <NavLink
                        to="/career-analytics"
                        onClick={() => setMobileMenuOpen(false)}
                        className={cn(
                          'flex items-center gap-3 px-3 py-2.5 text-sm font-medium rounded-md transition-colors',
                          location.pathname === '/career-analytics'
                            ? 'bg-[#EFF6FF] text-[#2563EB]'
                            : 'text-[#475569] hover:bg-[#F1F5F9]'
                        )}
                      >
                        <BarChart3 className="h-4 w-4" />
                        Career Analytics
                      </NavLink>
                      <NavLink
                        to="/learning-circles"
                        onClick={() => setMobileMenuOpen(false)}
                        className={cn(
                          'flex items-center gap-3 px-3 py-2.5 text-sm font-medium rounded-md transition-colors',
                          location.pathname === '/learning-circles'
                            ? 'bg-[#EFF6FF] text-[#2563EB]'
                            : 'text-[#475569] hover:bg-[#F1F5F9]'
                        )}
                      >
                        <Video className="h-4 w-4" />
                        Peer Rooms
                      </NavLink>
                    </div>
                  </>
                )}
              </nav>
            </motion.div>
          </>
        )}
      </AnimatePresence>
    </>
  );
};

export default Navbar;
