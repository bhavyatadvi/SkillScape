import React, { useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { useAuth } from '@/context/AuthContext';
import { cn } from '@/lib/utils';
import {
  LayoutDashboard,
  Compass,
  Map,
  Trophy,
  FileText,
  Users,
  DollarSign,
  User,
  BarChart3,
  ListChecks,
  ChevronLeft,
  ChevronRight,
  LogOut,
  Settings,
  Sparkles,
  Shield,
  Video,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';

const navItems = [
  { path: '/dashboard', icon: LayoutDashboard, hiddenRoles: ['Admin'] },
  { path: '/quiz', icon: Compass, label: 'Career Quiz', hiddenRoles: ['Admin'] },
  { path: '/career-explorer', icon: Map, label: 'Career Map', hiddenRoles: ['Admin'] },
  { path: '/placement', icon: Trophy, label: 'Skill Analyzer', hiddenRoles: ['Admin'] },
  { path: '/salary-forecast', icon: DollarSign, label: 'Salary Forecast', hiddenRoles: ['Admin'] },
  { path: '/career-analytics', icon: BarChart3, label: 'Career Analytics', hiddenRoles: ['Admin'] },
  { path: '/roadmap', icon: Map, label: 'Roadmap', hiddenRoles: ['Admin'] },
  { path: '/learning-circles', icon: Video, label: 'Learning Circles', hiddenRoles: ['Admin'] },
  { path: '/profile', icon: User, label: 'Profile', hiddenRoles: ['Admin'] },
  { path: '/admin', icon: Shield, label: 'Dashboard', roles: ['Admin'] },
  { path: '/admin/users', icon: Users, label: 'Users', roles: ['Admin'] },
  { path: '/admin/questions', icon: ListChecks, label: 'Questions', roles: ['Admin'] },
  { path: '/admin/roadmap', icon: Map, label: 'Roadmap', roles: ['Admin'] },
  { path: '/admin/analytics', icon: BarChart3, label: 'Analytics', roles: ['Admin'] },
  { path: '/admin/analytics', icon: BarChart3, label: 'Analytics', roles: ['Admin'] },
];

export const Sidebar = () => {
  const [collapsed, setCollapsed] = useState(false);
  const { user, logout } = useAuth();
  const location = useLocation();

  return (
    <motion.aside
      initial={false}
      animate={{ width: collapsed ? 80 : 280 }}
      transition={{ duration: 0.3, ease: 'easeInOut' }}
      className="fixed left-0 top-0 z-40 flex h-screen flex-col bg-sidebar border-r border-sidebar-border"
    >
      {/* Logo Section */}
      <div className="flex h-16 items-center justify-between px-4 border-b border-sidebar-border">
        <AnimatePresence mode="wait">
          {!collapsed && (
            <motion.div
              initial={{ opacity: 0, x: -10 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -10 }}
              transition={{ duration: 0.2 }}
              className="flex items-center gap-3"
            >
              <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-blue-600">
                <Sparkles className="h-5 w-5 text-accent-foreground" />
              </div>
              <span className="text-xl font-bold text-sidebar-foreground">
                SkillScape
              </span>
            </motion.div>
          )}
        </AnimatePresence>

        {collapsed && (
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-blue-600 mx-auto">
            <Sparkles className="h-5 w-5 text-accent-foreground" />
          </div>
        )}
      </div>

      {/* Navigation Links */}
      <nav className="flex-1 space-y-1 overflow-y-auto px-3 py-4">
        {navItems
          .filter((item) => (!item.roles || item.roles.includes(user?.role)) && (!item.hiddenRoles || !item.hiddenRoles.includes(user?.role)))
          .map((item) => {
          const isActive = location.pathname === item.path;

          return (
            <Tooltip key={item.path} delayDuration={0}>
              <TooltipTrigger asChild>
                <NavLink
                  to={item.path}
                  className={cn(
                    'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-all duration-200',
                    isActive
                      ? 'bg-blue-50 text-blue-600 shadow-soft'
                      : 'text-slate-300 hover:bg-slate-800 hover:text-white'
                  )}
                >
                  <item.icon className={cn(
                    'h-5 w-5 flex-shrink-0',
                    isActive ? 'text-blue-600' : 'text-slate-400'
                  )} />
                  <AnimatePresence mode="wait">
                    {!collapsed && (
                      <motion.span
                        initial={{ opacity: 0, width: 0 }}
                        animate={{ opacity: 1, width: 'auto' }}
                        exit={{ opacity: 0, width: 0 }}
                        transition={{ duration: 0.2 }}
                        className="whitespace-nowrap"
                      >
                        {item.label}
                      </motion.span>
                    )}
                  </AnimatePresence>
                  {isActive && (
                    <motion.div
                      layoutId="activeIndicator"
                      className="absolute left-0 h-8 w-1 rounded-r-full bg-blue-600"
                      transition={{ duration: 0.2 }}
                    />
                  )}
                </NavLink>
              </TooltipTrigger>
              {collapsed && (
                <TooltipContent side="right" className="ml-2">
                  {item.label}
                </TooltipContent>
              )}
            </Tooltip>
          );
        })}
      </nav>

      {/* User Section */}
      <div className="border-t border-sidebar-border p-3">
        {!collapsed ? (
          <div className="flex items-center gap-3 rounded-lg bg-sidebar-accent/50 p-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-blue-600">
              <span className="text-sm font-semibold text-accent-foreground">
                {user?.name?.charAt(0).toUpperCase() || 'U'}
              </span>
            </div>
            <div className="flex-1 min-w-0">
              <p className="truncate text-sm font-medium text-sidebar-foreground">
                {user?.name || 'User'}
              </p>
              <p className="truncate text-xs text-sidebar-foreground/60">
                {user?.email || 'user@example.com'}
              </p>
            </div>
            <Button
              variant="ghost"
              size="icon"
              onClick={logout}
              className="h-8 w-8 text-sidebar-foreground/60 hover:text-sidebar-foreground hover:bg-sidebar-accent"
            >
              <LogOut className="h-4 w-4" />
            </Button>
          </div>
        ) : (
          <Tooltip delayDuration={0}>
            <TooltipTrigger asChild>
              <Button
                variant="ghost"
                size="icon"
                onClick={logout}
                className="w-full h-10 text-sidebar-foreground/60 hover:text-sidebar-foreground hover:bg-sidebar-accent"
              >
                <LogOut className="h-5 w-5" />
              </Button>
            </TooltipTrigger>
            <TooltipContent side="right">Logout</TooltipContent>
          </Tooltip>
        )}
      </div>

      {/* Collapse Toggle */}
      <button
        onClick={() => setCollapsed(!collapsed)}
        className="absolute -right-3 top-20 flex h-6 w-6 items-center justify-center rounded-full border border-sidebar-border bg-sidebar text-sidebar-foreground shadow-md hover:bg-sidebar-accent transition-colors"
      >
        {collapsed ? (
          <ChevronRight className="h-4 w-4" />
        ) : (
          <ChevronLeft className="h-4 w-4" />
        )}
      </button>
    </motion.aside>
  );
};

export default Sidebar;
