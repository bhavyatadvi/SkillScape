import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useAuth } from '@/context/AuthContext';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  User,
  Lock,
  Palette,
  Camera,
  Save,
  LogOut,
} from 'lucide-react';
import { toast } from 'sonner';
import { authApi } from '@/services/api';
import PageHeader from '@/components/layout/PageHeader';

const ProfilePage = () => {
  const { user, updateUser, logout } = useAuth();
  const [name, setName] = useState(user?.name || '');
  const [email, setEmail] = useState(user?.email || '');
  const [bio, setBio] = useState('');
  const [profileImage, setProfileImage] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isInitializing, setIsInitializing] = useState(true);

  // Password change state
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isChangingPassword, setIsChangingPassword] = useState(false);
  const [isDeletingAccount, setIsDeletingAccount] = useState(false);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const data = await authApi.getCurrentUser();
        setName(data.fullName || '');
        setEmail(data.email || '');
        setBio(data.bio || '');
        setProfileImage(data.profileImageUrl || '');

        updateUser({
          name: data.fullName,
          email: data.email,
          bio: data.bio,
          profileImageUrl: data.profileImageUrl
        });
      } catch (err) {
        console.error("Failed to load profile:", err);
      } finally {
        setIsInitializing(false);
      }
    };
    fetchProfile();
  }, []);

  const handleSaveProfile = async () => {
    setIsLoading(true);
    try {
      const data = await authApi.updateProfile({ fullName: name, bio });
      updateUser({ name: data.fullName, bio: data.bio });
      toast.success('Profile updated successfully!');
    } catch (error) {
      toast.error(error.message || 'Failed to update profile');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAvatarUpload = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      toast.error("Please select an image file");
      return;
    }

    try {
      setIsLoading(true);
      const formData = new FormData();
      formData.append('file', file);
      const newImageUrl = await authApi.uploadAvatar(formData);

      setProfileImage(newImageUrl);
      updateUser({ profileImageUrl: newImageUrl });
      toast.success('Avatar updated successfully!');
    } catch (err) {
      toast.error('Failed to upload avatar');
    } finally {
      setIsLoading(false);
      e.target.value = null;
    }
  };

  const handleChangePassword = async () => {
    if (!currentPassword || !newPassword || !confirmPassword) {
      toast.error("Please fill in all password fields");
      return;
    }

    if (newPassword !== confirmPassword) {
      toast.error("New passwords do not match");
      return;
    }

    setIsChangingPassword(true);
    try {
      await authApi.changePassword(currentPassword, newPassword);
      toast.success("Password updated successfully!");
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
    } catch (err) {
      toast.error(err.response?.data?.message || err.message || "Failed to update password");
    } finally {
      setIsChangingPassword(false);
    }
  };

  const handleDeleteAccount = async () => {
    if (!window.confirm("Are you absolutely sure you want to delete your account? This action cannot be undone and you will lose all progress and history.")) {
      return;
    }

    setIsDeletingAccount(true);
    try {
      await authApi.deleteAccount();
      toast.success("Account permanently deleted. We're sorry to see you go.");
      logout();
    } catch (err) {
      toast.error(err.response?.data?.message || err.message || "Failed to delete account");
      setIsDeletingAccount(false);
    }
  };

  if (isInitializing) {
    return <div className="flex justify-center items-center min-h-[60vh]"><div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div></div>;
  }

  return (
    <div className="w-full">
      <div className="max-w-5xl mx-auto px-4 md:px-8">
        
        {/* Header */}
        <div className="mb-10">
          <PageHeader 
            icon={User}
            title="Account Settings"
            description="Manage your personal information, security preferences, and app settings."
            className="mb-0"
          />
        </div>

        <div className="bg-white rounded-[10px] shadow-sm border border-slate-200 overflow-hidden">
          <Tabs defaultValue="profile" className="flex flex-col md:flex-row min-h-[600px]">
            
            {/* Sidebar Tabs */}
            <TabsList className="flex flex-col justify-start h-auto w-full md:w-[240px] border-r border-slate-100 bg-slate-50/50 p-4 gap-2 shrink-0">
              <TabsTrigger value="profile" className="w-full justify-start px-4 py-3 rounded-lg data-[state=active]:bg-white data-[state=active]:text-blue-700 data-[state=active]:shadow-sm text-slate-600 font-semibold gap-3 transition-all">
                <User className="h-4 w-4" />
                Profile
              </TabsTrigger>
              <TabsTrigger value="security" className="w-full justify-start px-4 py-3 rounded-lg data-[state=active]:bg-white data-[state=active]:text-blue-700 data-[state=active]:shadow-sm text-slate-600 font-semibold gap-3 transition-all">
                <Lock className="h-4 w-4" />
                Security
              </TabsTrigger>
              <TabsTrigger value="preferences" className="w-full justify-start px-4 py-3 rounded-lg data-[state=active]:bg-white data-[state=active]:text-blue-700 data-[state=active]:shadow-sm text-slate-600 font-semibold gap-3 transition-all">
                <Palette className="h-4 w-4" />
                Preferences
              </TabsTrigger>
            </TabsList>

            <div className="flex-1 p-6 md:p-10">
              
              {/* Profile Tab */}
              <TabsContent value="profile" className="m-0 space-y-8 animate-in fade-in-50">
                <div>
                  <h2 className="text-xl font-bold text-slate-900">Personal Information</h2>
                  <p className="text-sm text-slate-500 mt-1 font-medium">Update your photo and personal details here.</p>
                </div>
                
                <Separator className="bg-slate-100" />

                {/* Avatar Section */}
                <div className="flex flex-col sm:flex-row sm:items-center gap-6">
                  <div className="relative inline-block">
                    <Avatar className="h-24 w-24 border border-slate-200 shadow-sm">
                      <AvatarImage src={profileImage ? `${import.meta.env.VITE_API_URL?.replace('/api', '') || 'http://localhost:5000'}${profileImage}` : ''} />
                      <AvatarFallback className="bg-blue-100 text-blue-700 text-2xl font-bold">
                        {name?.charAt(0).toUpperCase() || 'U'}
                      </AvatarFallback>
                    </Avatar>
                    <label className="absolute -bottom-2 -right-2 h-9 w-9 rounded-full shadow-sm bg-white border border-slate-200 text-slate-700 flex justify-center items-center cursor-pointer hover:bg-slate-50 transition-colors">
                      <input
                        type="file"
                        accept="image/*"
                        className="hidden"
                        onChange={handleAvatarUpload}
                        disabled={isLoading}
                      />
                      <Camera className="h-4 w-4" />
                    </label>
                  </div>
                  <div>
                    <h3 className="font-bold text-slate-900 text-lg">{user?.name}</h3>
                    <p className="text-slate-500 font-medium">{user?.email}</p>
                    <Badge className="mt-2 capitalize bg-blue-50 text-blue-700 hover:bg-blue-50 border-none font-semibold">
                      {user?.role}
                    </Badge>
                  </div>
                </div>

                <div className="space-y-6 max-w-2xl">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div className="space-y-2.5">
                      <Label htmlFor="name" className="text-sm font-bold text-slate-700">Full Name</Label>
                      <Input
                        id="name"
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        placeholder="Your name"
                        className="bg-slate-50 border-slate-200"
                      />
                    </div>
                    <div className="space-y-2.5">
                      <Label htmlFor="email" className="text-sm font-bold text-slate-700">Email Address</Label>
                      <Input
                        id="email"
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        placeholder="your@email.com"
                        className="bg-slate-100 border-transparent text-slate-500 font-medium"
                        disabled
                      />
                      <p className="text-xs text-slate-500 font-medium mt-1">
                        Contact support to change your email
                      </p>
                    </div>
                  </div>

                  <div className="space-y-2.5">
                    <Label htmlFor="bio" className="text-sm font-bold text-slate-700">Bio</Label>
                    <Textarea
                      id="bio"
                      value={bio}
                      onChange={(e) => setBio(e.target.value)}
                      placeholder="Tell us about yourself..."
                      className="min-h-[120px] bg-slate-50 border-slate-200 resize-none"
                    />
                  </div>
                </div>

                <div className="flex justify-start pt-4">
                  <Button
                    onClick={handleSaveProfile}
                    disabled={isLoading}
                    className="bg-slate-900 hover:bg-slate-800 text-white font-semibold px-8 py-5 rounded-[10px] shadow-sm"
                  >
                    <Save className="mr-2 h-4 w-4" />
                    {isLoading ? 'Saving...' : 'Save Changes'}
                  </Button>
                </div>
              </TabsContent>

              {/* Security Tab */}
              <TabsContent value="security" className="m-0 space-y-8 animate-in fade-in-50">
                <div>
                  <h2 className="text-xl font-bold text-slate-900">Security Settings</h2>
                  <p className="text-sm text-slate-500 mt-1 font-medium">Manage your password and account security.</p>
                </div>

                <Separator className="bg-slate-100" />

                <div className="max-w-md space-y-6">
                  <h3 className="font-bold text-slate-900">Change Password</h3>
                  
                  <div className="space-y-4">
                    <div className="space-y-2.5">
                      <Label htmlFor="current-password" className="text-sm font-bold text-slate-700">Current Password</Label>
                      <Input
                        id="current-password"
                        type="password"
                        value={currentPassword}
                        onChange={(e) => setCurrentPassword(e.target.value)}
                        className="bg-slate-50 border-slate-200"
                      />
                    </div>
                    
                    <div className="space-y-2.5">
                      <Label htmlFor="new-password" className="text-sm font-bold text-slate-700">New Password</Label>
                      <Input
                        id="new-password"
                        type="password"
                        value={newPassword}
                        onChange={(e) => setNewPassword(e.target.value)}
                        className="bg-slate-50 border-slate-200"
                      />
                    </div>
                    
                    <div className="space-y-2.5">
                      <Label htmlFor="confirm-password" className="text-sm font-bold text-slate-700">Confirm New Password</Label>
                      <Input
                        id="confirm-password"
                        type="password"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        className="bg-slate-50 border-slate-200"
                      />
                    </div>
                  </div>
                  
                  <Button
                    onClick={handleChangePassword}
                    disabled={isChangingPassword}
                    className="bg-slate-900 hover:bg-slate-800 text-white font-semibold px-8 py-5 rounded-[10px] shadow-sm w-full sm:w-auto mt-2"
                  >
                    {isChangingPassword ? 'Updating...' : 'Update Password'}
                  </Button>
                </div>
              </TabsContent>

              {/* Preferences Tab */}
              <TabsContent value="preferences" className="m-0 space-y-8 animate-in fade-in-50">
                <div>
                  <h2 className="text-xl font-bold text-slate-900">App Preferences</h2>
                  <p className="text-sm text-slate-500 mt-1 font-medium">Customize your experience and manage your account.</p>
                </div>

                <Separator className="bg-slate-100" />

                <div className="space-y-10 max-w-2xl">

                  {/* Language Section */}
                  <div>
                    <h3 className="font-bold text-slate-900 mb-1">Language</h3>
                    <p className="text-sm text-slate-500 font-medium mb-4">Select your preferred language.</p>
                    <Input value="English (US)" disabled className="bg-slate-100 border-transparent max-w-xs font-semibold text-slate-600" />
                  </div>

                  <Separator className="bg-slate-100" />

                  {/* Danger Zone */}
                  <div>
                    <h3 className="font-bold text-rose-600 mb-1">Danger Zone</h3>
                    <p className="text-sm text-slate-500 font-medium mb-4">Irreversible and destructive actions.</p>
                    <div className="flex flex-col sm:flex-row gap-4">
                      <Button
                        variant="outline"
                        className="text-slate-600 border-slate-200 hover:bg-slate-100 font-semibold"
                        onClick={logout}
                      >
                        <LogOut className="mr-2 h-4 w-4" />
                        Sign Out
                      </Button>
                      <Button
                        variant="outline"
                        onClick={handleDeleteAccount}
                        disabled={isDeletingAccount}
                        className="text-rose-600 border-rose-200 hover:bg-rose-50 font-semibold"
                      >
                        {isDeletingAccount ? 'Deleting...' : 'Delete Account'}
                      </Button>
                    </div>
                  </div>

                </div>
              </TabsContent>
            </div>
          </Tabs>
        </div>
      </div>
    </div>
  );
};

export default ProfilePage;

