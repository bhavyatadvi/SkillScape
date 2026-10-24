import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "@/context/AuthContext";
import { ProtectedRoute, PublicRoute } from "@/routes/ProtectedRoute";
import { DashboardLayout, AuthLayout } from "@/components/layout";
import { GlobalBackground } from "@/components/background/GlobalBackground";

// Pages
import LoginPage from "@/pages/Login";
import RegisterPage from "@/pages/Register";
import ForgotPasswordPage from "@/pages/ForgotPassword";
import ResetPasswordPage from "@/pages/ResetPassword";
import DashboardPage from "@/pages/Dashboard";
import LearningPlans from "@/pages/LearningPlans";
import QuizEngine from "@/pages/QuizEngine";
import RoadmapPage from "@/pages/Roadmap";
import MessagesPage from "@/pages/Messages";
import LearningCircles from "@/pages/LearningCircles";
import CircleDashboard from "@/pages/CircleDashboard";
import ProfilePage from "@/pages/Profile";
import AdminPage from "@/pages/Admin";
import UnauthorizedPage from "@/pages/Unauthorized";
import NotFound from "@/pages/NotFound";
import LandingPage from "@/pages/Landing";

// Career Guidance Module Pages
import CareerExplorer from "@/pages/CareerExplorer";
import CareerInsights from "@/pages/CareerInsights";
import SalaryForecast from "@/pages/SalaryForecast";
import CareerAnalytics from "@/pages/CareerAnalytics";
import PlacementReadiness from "@/pages/PlacementReadiness";
import UpskillingDashboard from "@/pages/UpskillingDashboard";

// Dedicated Modules
import GovernmentJobsEntry from "@/pages/GovernmentJobs/GovernmentJobsEntry";
import GovernmentJobsResults from "@/pages/GovernmentJobs/GovernmentJobsResults";
import CareerSwitchEntry from "@/pages/CareerSwitch/CareerSwitchEntry";
import CareerProfileEditor from "@/pages/CareerSwitch/CareerProfileEditor";
import CareerSwitchPreAssessment from "@/pages/CareerSwitch/CareerSwitchPreAssessment";
import AdaptiveAssessment from "@/pages/CareerSwitch/AdaptiveAssessment";
import CareerSwitchResults from "@/pages/CareerSwitch/CareerSwitchResults";
import CareerSwitchAnswers from "@/pages/CareerSwitch/CareerSwitchAnswers";
import { CareerProfileProvider } from "@/context/CareerProfileContext";
import CareerProfilePage from "@/pages/CareerProfile/CareerProfile";
import ResumeUpload from "@/pages/CareerProfile/ResumeUpload";

const queryClient = new QueryClient();

const App = () => (
  <QueryClientProvider client={queryClient}>
    <AuthProvider>
      <CareerProfileProvider>
      <TooltipProvider>
        <Sonner />
        <BrowserRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
          <GlobalBackground />
          <Routes>
            {/* Public Routes */}
            <Route element={<AuthLayout />}>
              <Route
                path="/login"
                element={
                  <PublicRoute>
                    <LoginPage />
                  </PublicRoute>
                }
              />
              <Route
                path="/register"
                element={
                  <PublicRoute>
                    <RegisterPage />
                  </PublicRoute>
                }
              />
              <Route
                path="/forgot-password"
                element={
                  <PublicRoute>
                    <ForgotPasswordPage />
                  </PublicRoute>
                }
              />
              <Route
                path="/reset-password"
                element={
                  <PublicRoute>
                    <ResetPasswordPage />
                  </PublicRoute>
                }
              />
            </Route>

            {/* Protected Dashboard Routes */}
            <Route
              element={
                <ProtectedRoute>
                  <DashboardLayout />
                </ProtectedRoute>
              }
            >
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/learning-plans" element={<LearningPlans />} />
              <Route path="/quiz" element={<QuizEngine />} />
              <Route path="/placement" element={<PlacementReadiness />} />
              <Route path="/skill-analyzer" element={<PlacementReadiness />} />
              
              {/* Career Guidance Module Routes */}
              <Route path="/career-explorer" element={<CareerExplorer />} />
              <Route path="/upskilling" element={<UpskillingDashboard />} />
              <Route path="/career-quiz" element={<Navigate to="/quiz" replace />} />
              <Route path="/career-insights" element={<CareerInsights />} />
              <Route path="/salary-forecast" element={<SalaryForecast />} />
              <Route path="/career-analytics" element={<CareerAnalytics />} />
              
              {/* Dedicated Career Modules */}
              <Route path="/government-jobs" element={<GovernmentJobsEntry />} />
              <Route path="/government-jobs/assessment" element={<Navigate to="/government-jobs" replace />} />
              <Route path="/government-jobs/results" element={<GovernmentJobsResults />} />
              
              {/* Central Career Profile Routes */}
              <Route path="/career-profile" element={<CareerProfilePage />} />
              <Route path="/career-profile/edit" element={<CareerProfileEditor />} />
              <Route path="/career-profile/resume" element={<ResumeUpload />} />

              <Route path="/career-switch" element={<CareerSwitchEntry />} />
              {/* Redirect old route to new central career profile */}
              <Route path="/career-switch/profile" element={<Navigate to="/career-profile/edit" replace />} />
              <Route path="/career-switch/pre-assessment" element={<CareerSwitchPreAssessment />} />
              <Route path="/career-switch/assessment" element={<AdaptiveAssessment />} />
              <Route path="/career-switch/results" element={<CareerSwitchResults />} />
              <Route path="/career-switch/answers" element={<CareerSwitchAnswers />} />

              <Route path="/roadmap" element={<RoadmapPage />} />
              <Route path="/messages" element={<MessagesPage />} />
              <Route path="/learning-circles" element={<LearningCircles />} />
              <Route path="/learning-circles/:circleId" element={<CircleDashboard />} />
              <Route path="/profile" element={<ProfilePage />} />
              <Route
                path="/admin"
                element={
                  <ProtectedRoute roles="Admin">
                    <AdminPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/:section"
                element={
                  <ProtectedRoute roles="Admin">
                    <AdminPage />
                  </ProtectedRoute>
                }
              />
              <Route path="/unauthorized" element={<UnauthorizedPage />} />
            </Route>

            {/* Public Landing */}
            <Route path="/" element={<LandingPage />} />

            {/* Redirects */}
            {/* <Route path="/" element={<Navigate to="/login" replace />} /> */}

            {/* 404 */}
            <Route path="*" element={<NotFound />} />
          </Routes>
        </BrowserRouter>
      </TooltipProvider>
      </CareerProfileProvider>
    </AuthProvider>
  </QueryClientProvider>
);

export default App;
