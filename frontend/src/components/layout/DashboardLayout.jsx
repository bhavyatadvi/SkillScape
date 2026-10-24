import React, { useEffect, useState } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import { Navbar } from './Navbar';
import ChatBot from '../ChatBot';
import * as signalR from '@microsoft/signalr';
import { getAuthToken } from '@/services/api';
import { toast } from 'sonner';
import { useAuth } from '@/context/AuthContext';
import { useCareerProfile } from '@/context/CareerProfileContext';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { GraduationCap } from 'lucide-react';

export const DashboardLayout = () => {
  const { user } = useAuth();
  const { profile, profileLoadedForUserId, error: careerProfileError } = useCareerProfile();
  const [connection, setConnection] = useState(null);
  const [isCareerProfilePromptOpen, setIsCareerProfilePromptOpen] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    if (!user?.id || user.role === 'Admin' || profileLoadedForUserId !== user.id) {
      setIsCareerProfilePromptOpen(false);
      return;
    }

    if (profile || careerProfileError) {
      setIsCareerProfilePromptOpen(false);
      return;
    }

    // Dismiss only for this signed-in session. DashboardLayout unmounts on
    // logout, so an incomplete profile is prompted again on the next login.
    setIsCareerProfilePromptOpen(true);
  }, [user, profile, profileLoadedForUserId, careerProfileError]);

  const dismissCareerProfilePrompt = () => {
    setIsCareerProfilePromptOpen(false);
  };

  const openCareerProfile = () => {
    dismissCareerProfilePrompt();
    navigate('/career-profile/edit');
  };

  useEffect(() => {
    if (!user) return;

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
  }, [user]);

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => {
          console.log("Global Notification Hub Connected");

          connection.on("ReceiveNotification", (notification) => {
            window.dispatchEvent(new CustomEvent('skillscape:notification-received', {
              detail: notification
            }));

            if (notification.type === 'achievement') {
              toast.success(`${notification.title} - ${notification.message}`, { duration: 5000 });
            } else if (notification.type === 'chat') {
              toast(notification.title, {
                duration: 5000,
                description: (
                  <div className="flex flex-col gap-2">
                    <span className="text-sm text-[#64748B]">{notification.message !== "Sent an image" ? notification.message : "Sent an image"}</span>
                    {notification.imageUrl && (
                      <img
                        src={`${import.meta.env.VITE_API_URL?.replace('/api', '') || 'http://localhost:5000'}${notification.imageUrl}`}
                        alt="Attachment"
                        className="rounded-lg object-cover max-h-32 mt-1 border border-[#E2E8F0]"
                      />
                    )}
                  </div>
                ),
                action: {
                  label: "View",
                  onClick: () => navigate('/messages')
                },
              });
            } else if (notification.type === 'join_request') {
              toast(notification.title, {
                duration: 6000,
                description: <div className="text-sm text-slate-500">{notification.message}</div>,
                action: {
                  label: "View Circle",
                  onClick: () => navigate(`/learning-circles/${notification.circleId || ''}`)
                },
              });
            } else if (notification.type === 'join_request_processed') {
              toast(notification.title, {
                duration: 6000,
                description: <div className="text-sm text-slate-500">{notification.message}</div>,
                action: {
                  label: "Explore",
                  onClick: () => navigate('/learning-circles')
                },
              });
            } else {
              toast.info(`${notification.title} - ${notification.message}`, { duration: 5000 });
            }
          });
        })
        .catch(e => console.log("Connection failed: ", e));
    }

    return () => {
      if (connection) {
        connection.off("ReceiveNotification");
        connection.stop();
      }
    };
  }, [connection]);

  return (
    <div className="min-h-screen w-full bg-transparent relative overflow-hidden">
      {/* Top Navbar */}
      <Navbar />

      {/* Page Content — full width, centered */}
      <main className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-12 pt-6 pb-12 relative z-10">
        <Outlet />
      </main>

      {/* ChatBot Widget */}
      <ChatBot />

      <Dialog
        open={isCareerProfilePromptOpen}
        onOpenChange={(open) => {
          if (!open) {
            dismissCareerProfilePrompt();
          }
        }}
      >
        <DialogContent>
          <DialogHeader className="items-center text-center">
            <div className="mb-2 flex h-14 w-14 items-center justify-center rounded-full bg-primary/10 text-primary">
              <GraduationCap className="h-7 w-7" aria-hidden="true" />
            </div>
            <DialogTitle>Make your career profile now?</DialogTitle>
            <DialogDescription>
              Add your education, skills, and career goals to get guidance tailored to you.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter className="flex-col-reverse gap-2 sm:flex-row sm:gap-2 sm:space-x-0">
            <Button variant="outline" onClick={dismissCareerProfilePrompt}>
              Make career profile later
            </Button>
            <Button onClick={openCareerProfile}>
              Make career profile
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default DashboardLayout;
