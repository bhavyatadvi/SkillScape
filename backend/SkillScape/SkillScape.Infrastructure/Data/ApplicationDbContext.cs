using Microsoft.EntityFrameworkCore;
using SkillScape.Domain.Entities;

namespace SkillScape.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<ApplicationUser> Users { get; set; } = null!;
    public DbSet<CareerDomain> CareerDomains { get; set; } = null!;
    public DbSet<Skill> Skills { get; set; } = null!;
    public DbSet<UserSkill> UserSkills { get; set; }

    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
    public DbSet<QuizOption> QuizOptions { get; set; } = null!;
    public DbSet<QuizResponse> QuizResponses { get; set; } = null!;
    public DbSet<QuizResult> QuizResults { get; set; } = null!;
    public DbSet<Badge> Badges { get; set; } = null!;
    public DbSet<UserBadge> UserBadges { get; set; } = null!;
    public DbSet<RoadmapStep> RoadmapSteps { get; set; } = null!;
    public DbSet<RoadmapTopic> RoadmapTopics { get; set; } = null!;
    public DbSet<RoadmapBlueprint> RoadmapBlueprints { get; set; } = null!;
    public DbSet<RoadmapPhase> RoadmapPhases { get; set; } = null!;
    public DbSet<RoadmapModule> RoadmapModules { get; set; } = null!;
    public DbSet<RoadmapModuleQuiz> RoadmapModuleQuizzes { get; set; } = null!;
    public DbSet<RoadmapQuizQuestion> RoadmapQuizQuestions { get; set; } = null!;
    public DbSet<UserRoadmapModuleQuizAttempt> UserRoadmapModuleQuizAttempts { get; set; } = null!;
    public DbSet<UserSavedRoadmap> UserSavedRoadmaps { get; set; } = null!;
    public DbSet<UserRoadmapDismissal> UserRoadmapDismissals { get; set; } = null!;
    public DbSet<UserModuleProgress> UserModuleProgressions { get; set; } = null!;
    public DbSet<UserCustomModuleProgress> UserCustomModuleProgressions { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<AdminAuditLog> AdminAuditLogs { get; set; } = null!;
    public DbSet<LearningCircle> LearningCircles { get; set; } = null!;
    public DbSet<LearningCircleMember> LearningCircleMembers { get; set; } = null!;
    public DbSet<LearningCircleDiscussion> LearningCircleDiscussions { get; set; } = null!;
    public DbSet<LearningCircleResource> LearningCircleResources { get; set; } = null!;
    public DbSet<LearningCircleJoinRequest> LearningCircleJoinRequests { get; set; } = null!;
    public DbSet<LearningCircleTask> LearningCircleTasks { get; set; } = null!;
    public DbSet<LearningCircleNote> LearningCircleNotes { get; set; } = null!;
    public DbSet<CareerPath> CareerPaths { get; set; } = null!;
    public DbSet<HierarchicalQuizQuestion> HierarchicalQuizQuestions { get; set; } = null!;
    public DbSet<UserCareerProfile> UserCareerProfiles { get; set; } = null!;
    
    // Quiz Engine Reboot Entities
    public DbSet<QuizPhase> QuizPhases { get; set; } = null!;
    public DbSet<CareerTaxonomyNode> CareerTaxonomyNodes { get; set; } = null!;
    public DbSet<QuizQuestionSignal> QuizQuestionSignals { get; set; } = null!;
    public DbSet<EligibilityRule> EligibilityRules { get; set; } = null!;

    // ADAPTIVE QUIZ ENGINE V2
    public DbSet<CareerTaxonomyV2> CareerTaxonomyV2 { get; set; } = null!;
    public DbSet<QuizQuestionV2> QuizQuestionsV2 { get; set; } = null!;
    public DbSet<QuizOptionSignalV2> QuizOptionSignalsV2 { get; set; } = null!;
    public DbSet<EligibilityConstraintV2> EligibilityConstraintsV2 { get; set; } = null!;
    public DbSet<AdaptiveQuizSession> AdaptiveQuizSessions { get; set; } = null!;
    public DbSet<QuizResultV2> QuizResultsV2 { get; set; } = null!;
    public DbSet<QuizQuestionRecordV2> QuizQuestionRecordsV2 { get; set; } = null!;

    // Career Knowledge Graph (Parts 19-21)
    public DbSet<KgNode> KgNodes { get; set; } = null!;
    public DbSet<KgEdge> KgEdges { get; set; } = null!;

    // ML.NET Training Data
    public DbSet<TrainingDataRecord> TrainingDataRecords { get; set; } = null!;

    // Placement Readiness and Skill Gap Analysis
    public DbSet<PlacementAssessmentQuestion> PlacementAssessmentQuestions { get; set; } = null!;
    public DbSet<PlacementAssessment> PlacementAssessments { get; set; } = null!;
    public DbSet<RoleSkillProfile> RoleSkillProfiles { get; set; } = null!;

    // Normalized taxonomy and versioning
    public DbSet<NormCareerDomain> NormCareerDomains { get; set; } = null!;
    public DbSet<NormCareerField> NormCareerFields { get; set; } = null!;
    public DbSet<NormCareerSubField> NormCareerSubFields { get; set; } = null!;
    public DbSet<NormCareerSpecialization> NormCareerSpecializations { get; set; } = null!;
    public DbSet<NormCareerFamily> NormCareerFamilies { get; set; } = null!;
    public DbSet<NormOccupation> NormOccupations { get; set; } = null!;
    public DbSet<NormEducationStage> NormEducationStages { get; set; } = null!;
    public DbSet<NormCourse> NormCourses { get; set; } = null!;
    public DbSet<NormSubject> NormSubjects { get; set; } = null!;
    public DbSet<NormCertification> NormCertifications { get; set; } = null!;
    public DbSet<NormGovernmentExam> NormGovernmentExams { get; set; } = null!;
    public DbSet<NormEntranceExam> NormEntranceExams { get; set; } = null!;
    public DbSet<NormSkill> NormSkills { get; set; } = null!;
    public DbSet<NormAbility> NormAbilities { get; set; } = null!;
    public DbSet<NormInterest> NormInterests { get; set; } = null!;
    public DbSet<NormCareerSkillMapping> NormCareerSkillMappings { get; set; } = null!;
    public DbSet<NormCareerEducationMapping> NormCareerEducationMappings { get; set; } = null!;
    public DbSet<NormCareerOccupationMapping> NormCareerOccupationMappings { get; set; } = null!;
    public DbSet<NormQuestionBank> NormQuestionBanks { get; set; } = null!;
    public DbSet<NormQuestionOption> NormQuestionOptions { get; set; } = null!;
    public DbSet<NormQuestionSignal> NormQuestionSignals { get; set; } = null!;
    public DbSet<NormQuestionSource> NormQuestionSources { get; set; } = null!;
    public DbSet<NormQuestionFieldMapping> NormQuestionFieldMappings { get; set; } = null!;
    public DbSet<NormQuizSession> NormQuizSessions { get; set; } = null!;
    public DbSet<NormQuizResponse> NormQuizResponses { get; set; } = null!;
    public DbSet<NormPredictionResult> NormPredictionResults { get; set; } = null!;
    public DbSet<NormModelVersion> NormModelVersions { get; set; } = null!;

    // Central Career Profile (new — maps to "CentralCareerProfiles" table)
    public DbSet<CareerProfile> CareerProfiles { get; set; } = null!;

    // Reverse Career Finder taxonomy (maps to "CareerSwitchNodes" table)
    public DbSet<CareerSwitchNode> CareerSwitchNodes { get; set; } = null!;
    public DbSet<TransferableSkill> TransferableSkills { get; set; } = null!;
    public DbSet<CareerTransition> CareerTransitions { get; set; } = null!;
    public DbSet<CareerSwitchQuestion> CareerSwitchQuestions { get; set; } = null!;
    public DbSet<CareerSwitchQuestionOption> CareerSwitchQuestionOptions { get; set; } = null!;
    
    // Salary Intelligence
    public DbSet<SalaryBenchmark> SalaryBenchmarks { get; set; } = null!;
    public DbSet<SalaryBenchmarkSource> SalaryBenchmarkSources { get; set; } = null!;
    public DbSet<CareerSalaryMapping> CareerSalaryMappings { get; set; } = null!;

    // Upskilling and certifications
    public DbSet<CourseProvider> CourseProviders { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<CourseSkill> CourseSkills { get; set; } = null!;
    public DbSet<CourseCareer> CourseCareers { get; set; } = null!;
    public DbSet<CourseEducationEligibility> CourseEducationEligibilities { get; set; } = null!;
    public DbSet<TrendingSkill> TrendingSkills { get; set; } = null!;
    public DbSet<SavedCourse> SavedCourses { get; set; } = null!;
    public DbSet<LearningProgress> LearningProgresses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProfileCompleted).IsRequired();
            entity.Property(e => e.BlockedReason).HasMaxLength(500);
            entity.Property(e => e.LearningDomain).HasMaxLength(256);
            entity.Property(e => e.LearningGoals).HasMaxLength(1000);
            entity.Property(e => e.Availability).HasMaxLength(50);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<CareerDomain>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.HasMany(d => d.Skills).WithOne(s => s.CareerDomain).HasForeignKey(s => s.CareerDomainId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(d => d.RoadmapSteps).WithOne(r => r.CareerDomain).HasForeignKey(r => r.CareerDomainId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.DifficultyLevel).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<UserSkill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(us => us.User).WithMany(u => u.UserSkills).HasForeignKey(us => us.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(us => us.Skill).WithMany(s => s.UserSkills).HasForeignKey(us => us.SkillId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.SkillId }).IsUnique();
        });

        modelBuilder.Entity<CareerProfile>(entity =>
        {
            entity.ToTable("CentralCareerProfiles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasOne(e => e.User)
                  .WithOne()
                  .HasForeignKey<CareerProfile>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CareerSwitchNode>(entity =>
        {
            entity.ToTable("CareerSwitchNodes");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<CareerTransition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.FromCareer).WithMany(c => c.OutgoingTransitions).HasForeignKey(e => e.FromCareerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ToCareer).WithMany(c => c.IncomingTransitions).HasForeignKey(e => e.ToCareerId).OnDelete(DeleteBehavior.Restrict);
        });

        // QuizQuestion
        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired();
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            
            entity.HasMany(q => q.Options)
                .WithOne(o => o.QuizQuestion)
                .HasForeignKey(o => o.QuizQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // QuizOption
        modelBuilder.Entity<QuizOption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired();
            entity.Property(e => e.DomainWeightJson).IsRequired();
            
            entity.HasMany(o => o.Signals)
                .WithOne(s => s.QuizOption)
                .HasForeignKey(s => s.QuizOptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // QuizPhase
        modelBuilder.Entity<QuizPhase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Key).IsUnique();
        });

        // CareerTaxonomyNode
        modelBuilder.Entity<CareerTaxonomyNode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Level).IsRequired().HasMaxLength(50);
        });

        // EligibilityRule
        modelBuilder.Entity<EligibilityRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetDomain).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RequiredSubject).IsRequired().HasMaxLength(100);
        });

        // QuizResponse
        modelBuilder.Entity<QuizResponse>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(qr => qr.User)
                .WithMany(u => u.QuizResponses)
                .HasForeignKey(qr => qr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(qr => qr.QuizQuestion)
                .WithMany(q => q.Responses)
                .HasForeignKey(qr => qr.QuizQuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(qr => qr.QuizOption)
                .WithMany(o => o.Responses)
                .HasForeignKey(qr => qr.QuizOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // QuizResult
        modelBuilder.Entity<QuizResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ScoresJson).IsRequired();
            
            entity.HasOne(qr => qr.User)
                .WithMany()
                .HasForeignKey(qr => qr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Badge
        modelBuilder.Entity<Badge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Rarity).IsRequired().HasMaxLength(50);
        });

        // UserBadge
        modelBuilder.Entity<UserBadge>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(ub => ub.User)
                .WithMany(u => u.UserBadges)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(ub => ub.Badge)
                .WithMany(b => b.UserBadges)
                .HasForeignKey(ub => ub.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.UserId, e.BadgeId }).IsUnique();
        });

        // RoadmapStep
        modelBuilder.Entity<RoadmapStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(256);
            
            entity.HasOne(r => r.Skill)
                .WithMany(s => s.RoadmapSteps)
                .HasForeignKey(r => r.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RoadmapTopic
        modelBuilder.Entity<RoadmapTopic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ModuleId).IsRequired(false);
            entity.Property(e => e.RoadmapModuleId).IsRequired(false);
            
            entity.HasOne(t => t.Module)
                .WithMany(m => m.Topics)
                .HasForeignKey(t => t.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.RoadmapModule)
                .WithMany(m => m.Topics)
                .HasForeignKey(t => t.RoadmapModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Comprehensive Roadmap Entities
        modelBuilder.Entity<RoadmapBlueprint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CareerKey).IsRequired().HasMaxLength(256);
            entity.Property(e => e.CareerTitle).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.CareerKey);
        });

        modelBuilder.Entity<RoadmapPhase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(256);
            entity.HasOne(p => p.Blueprint)
                .WithMany(b => b.Phases)
                .HasForeignKey(p => p.BlueprintId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoadmapModule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(256);
            entity.HasOne(m => m.Phase)
                .WithMany(p => p.Modules)
                .HasForeignKey(m => m.PhaseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoadmapModuleQuiz>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(q => q.Module)
                .WithOne(m => m.Quiz)
                .HasForeignKey<RoadmapModuleQuiz>(q => q.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoadmapQuizQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuestionText).IsRequired();
            entity.Property(e => e.OptionsJson).IsRequired();
            entity.HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserRoadmapModuleQuizAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.ModuleId });
        });

        modelBuilder.Entity<UserSavedRoadmap>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Blueprint)
                .WithMany()
                .HasForeignKey(s => s.BlueprintId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.CareerKey }).IsUnique();
        });

        modelBuilder.Entity<UserRoadmapDismissal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.CareerKey }).IsUnique();
        });

        // UserModuleProgress
        modelBuilder.Entity<UserModuleProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(ump => ump.User)
                .WithMany()
                .HasForeignKey(ump => ump.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(ump => ump.Module)
                .WithMany(m => m.UserProgressions)
                .HasForeignKey(ump => ump.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.UserId, e.ModuleId }).IsUnique();
        });

        // UserCustomModuleProgress
        modelBuilder.Entity<UserCustomModuleProgress>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(ump => ump.User)
                .WithMany(u => u.UserCustomModuleProgresses)
                .HasForeignKey(ump => ump.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.ModuleId }).IsUnique();
        });

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);

            entity.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AdminAuditLog
        modelBuilder.Entity<AdminAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(300);
            entity.Property(e => e.TargetEntity).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TargetEntityId).HasMaxLength(450);

            entity.HasOne(a => a.Admin)
                .WithMany()
                .HasForeignKey(a => a.AdminId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ChatMessage
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            
            entity.HasOne(cm => cm.Sender)
                .WithMany()
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(cm => cm.Receiver)
                .WithMany()
                .HasForeignKey(cm => cm.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LearningCircle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Members)
                  .WithOne(m => m.Circle)
                  .HasForeignKey(m => m.CircleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Discussions)
                  .WithOne(d => d.Circle)
                  .HasForeignKey(d => d.CircleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Resources)
                  .WithOne(r => r.Circle)
                  .HasForeignKey(r => r.CircleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.JoinRequests)
                  .WithOne(jr => jr.Circle)
                  .HasForeignKey(jr => jr.CircleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LearningCircleJoinRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(jr => jr.User)
                  .WithMany()
                  .HasForeignKey(jr => jr.UserId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<LearningCircleMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(m => m.User)
                  .WithMany()
                  .HasForeignKey(m => m.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.CircleId, e.UserId }).IsUnique();
        });

        modelBuilder.Entity<LearningCircleDiscussion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(d => d.User)
                  .WithMany()
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LearningCircleResource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(r => r.AddedByUser)
                  .WithMany()
                  .HasForeignKey(r => r.AddedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // CareerPath
        modelBuilder.Entity<CareerPath>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Tags).HasMaxLength(1000);
            entity.Property(e => e.Certifications).HasMaxLength(1000);
            entity.Property(e => e.Colleges).HasMaxLength(1000);
            
            // Self-referencing recursive relationship
            entity.HasOne(e => e.ParentCareer)
                .WithMany(p => p.SubCareers)
                .HasForeignKey(e => e.ParentCareerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // HierarchicalQuizQuestion
        modelBuilder.Entity<HierarchicalQuizQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuestionText).IsRequired();
            entity.Property(e => e.OptionsJson).IsRequired();
        });

        // UserCareerProfile
        modelBuilder.Entity<UserCareerProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuizAnswersJson).IsRequired();
            entity.Property(e => e.RoadmapJson).IsRequired();
            entity.Property(e => e.SkillGapJson).IsRequired();
            entity.Property(e => e.BookmarkedPaths).IsRequired();
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Navigation property without strict DB foreign key constraint
            entity.HasOne(e => e.RecommendedCareer)
                .WithMany()
                .HasForeignKey(e => e.RecommendedCareerId)
                .IsRequired(false) // Allow it to point to non-existent GUIDs without throwing
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ── Salary Intelligence ──────────────────────────────────────────
        modelBuilder.Entity<SalaryBenchmark>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleKey).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.RoleKey);
        });

        modelBuilder.Entity<SalaryBenchmarkSource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(s => s.Benchmark)
                .WithMany(b => b.Sources)
                .HasForeignKey(s => s.SalaryBenchmarkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CareerSalaryMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(m => m.CareerPath)
                .WithMany()
                .HasForeignKey(m => m.CareerPathId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CareerPathId).IsUnique();
        });

        // ── Placement Readiness Module ────────────────────────────────────────

        // RoleSkillProfile
        modelBuilder.Entity<RoleSkillProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SkillsJson).IsRequired();
            entity.HasIndex(e => e.RoleName).IsUnique();
        });

        // PlacementAssessmentQuestion
        modelBuilder.Entity<PlacementAssessmentQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuestionText).IsRequired();
            entity.Property(e => e.OptionsJson).IsRequired();
            entity.Property(e => e.CorrectAnswer).IsRequired().HasMaxLength(500);
            entity.Property(e => e.SkillMapped).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Difficulty).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ApplicableRoles).IsRequired().HasMaxLength(1000);
        });

        // PlacementAssessment
        modelBuilder.Entity<PlacementAssessment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetRole).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SkillScoresJson).IsRequired();
            entity.Property(e => e.StrongSkillsJson).IsRequired();
            entity.Property(e => e.WeakSkillsJson).IsRequired();
            entity.Property(e => e.RecommendationsJson).IsRequired();
            entity.Property(e => e.RoadmapJson).IsRequired();

            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
        });

        modelBuilder.Entity<KgEdge>()
            .HasOne(e => e.SourceNode)
            .WithMany(n => n.OutgoingEdges)
            .HasForeignKey(e => e.SourceNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<KgEdge>()
            .HasOne(e => e.TargetNode)
            .WithMany(n => n.IncomingEdges)
            .HasForeignKey(e => e.TargetNodeId)
            .OnDelete(DeleteBehavior.NoAction);

        // ── PART 33 / 34: CONSTRAINTS ──────────────────────────────────────────
        // Ensure (EntityId, Version) is Unique for all versioned entities
        var versionedTypes = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(IVersionedEntity).IsAssignableFrom(t.ClrType));

        foreach (var entityType in versionedTypes)
        {
            modelBuilder.Entity(entityType.ClrType)
                .HasIndex(nameof(IVersionedEntity.EntityId), nameof(IVersionedEntity.Version))
                .IsUnique();
        }

        modelBuilder.Entity<NormCareerField>()
            .HasOne(cf => cf.Domain)
            .WithMany(d => d.Fields)
            .HasForeignKey(cf => cf.DomainId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NormCareerSubField>()
            .HasOne(sf => sf.Field)
            .WithMany(f => f.SubFields)
            .HasForeignKey(sf => sf.FieldId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NormCareerSpecialization>()
            .HasOne(cs => cs.SubField)
            .WithMany(sf => sf.Specializations)
            .HasForeignKey(cs => cs.SubFieldId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NormOccupation>()
            .HasOne(o => o.Family)
            .WithMany(f => f.Occupations)
            .HasForeignKey(o => o.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<NormQuestionBank>()
            .HasOne(q => q.Source)
            .WithMany()
            .HasForeignKey(q => q.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NormQuestionOption>()
            .HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NormQuestionSignal>()
            .HasOne(s => s.Option)
            .WithMany(o => o.Signals)
            .HasForeignKey(s => s.OptionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<NormQuestionFieldMapping>()
            .HasOne(m => m.Question)
            .WithMany(q => q.TargetFields)
            .HasForeignKey(m => m.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<NormQuestionFieldMapping>()
            .HasOne(m => m.Field)
            .WithMany()
            .HasForeignKey(m => m.FieldId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NormQuizResponse>()
            .HasOne(r => r.Session)
            .WithMany(s => s.Responses)
            .HasForeignKey(r => r.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<NormQuizResponse>()
            .HasOne(r => r.Question)
            .WithMany()
            .HasForeignKey(r => r.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NormQuizResponse>()
            .HasOne(r => r.Option)
            .WithMany()
            .HasForeignKey(r => r.OptionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NormPredictionResult>()
            .HasOne(p => p.Session)
            .WithMany(s => s.Predictions)
            .HasForeignKey(p => p.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NormDataIngestionLog>()
            .HasOne(l => l.DataSource)
            .WithMany()
            .HasForeignKey(l => l.DataSourceId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Upskilling & Certifications Configuration ───────────────────────
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Provider)
            .WithMany(p => p.Courses)
            .HasForeignKey(c => c.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseSkill>()
            .HasOne(cs => cs.Course)
            .WithMany(c => c.CourseSkills)
            .HasForeignKey(cs => cs.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseCareer>()
            .HasOne(cc => cc.Course)
            .WithMany(c => c.CourseCareers)
            .HasForeignKey(cc => cc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseEducationEligibility>()
            .HasOne(cee => cee.Course)
            .WithMany(c => c.EducationEligibilities)
            .HasForeignKey(cee => cee.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SavedCourse>()
            .HasOne(sc => sc.User)
            .WithMany()
            .HasForeignKey(sc => sc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SavedCourse>()
            .HasOne(sc => sc.Course)
            .WithMany(c => c.SavedByUsers)
            .HasForeignKey(sc => sc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LearningCircleTask>()
            .HasOne(t => t.CreatedByUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LearningCircleTask>()
            .HasOne(t => t.AssignedToUser)
            .WithMany()
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LearningProgress>()
            .HasOne(lp => lp.User)
            .WithMany()
            .HasForeignKey(lp => lp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LearningProgress>()
            .HasOne(lp => lp.Course)
            .WithMany(c => c.LearningProgresses)
            .HasForeignKey(lp => lp.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
