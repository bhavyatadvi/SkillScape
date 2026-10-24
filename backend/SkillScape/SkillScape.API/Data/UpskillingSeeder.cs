using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.API.Data
{
    public static class UpskillingSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Courses.AnyAsync())
            {
                return; // Already seeded
            }

            // Seed Providers
            var providers = new List<CourseProvider>
            {
                new CourseProvider { Id = "prov_nptel", Name = "NPTEL", ProviderType = "Government", AuthorityScore = 1.0, LogoUrl = "https://nptel.ac.in/assets/nptel_logo.png", WebsiteUrl = "https://nptel.ac.in/" },
                new CourseProvider { Id = "prov_swayam", Name = "SWAYAM", ProviderType = "Government", AuthorityScore = 1.0, LogoUrl = "https://swayam.gov.in/nc_details/swayam_logo.png", WebsiteUrl = "https://swayam.gov.in/" },
                new CourseProvider { Id = "prov_coursera", Name = "Coursera", ProviderType = "Commercial", AuthorityScore = 0.9, LogoUrl = "https://d3njjcbhbojbot.cloudfront.net/api/utilities/v1/imageproxy/https://coursera.s3.amazonaws.com/media/coursera-logo-square.png", WebsiteUrl = "https://www.coursera.org/" },
                new CourseProvider { Id = "prov_edx", Name = "edX", ProviderType = "Commercial", AuthorityScore = 0.9, LogoUrl = "https://www.edx.org/images/logos/edx-logo-elm.svg", WebsiteUrl = "https://www.edx.org/" },
                new CourseProvider { Id = "prov_google", Name = "Google", ProviderType = "Commercial", AuthorityScore = 0.95, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/2/2f/Google_2015_logo.svg", WebsiteUrl = "https://grow.google/certificates/" },
                new CourseProvider { Id = "prov_microsoft", Name = "Microsoft Learn", ProviderType = "Commercial", AuthorityScore = 0.95, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/4/44/Microsoft_logo.svg", WebsiteUrl = "https://learn.microsoft.com/" },
                new CourseProvider { Id = "prov_aws", Name = "AWS Training", ProviderType = "Commercial", AuthorityScore = 0.95, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/9/93/Amazon_Web_Services_Logo.svg", WebsiteUrl = "https://aws.amazon.com/training/" },
                new CourseProvider { Id = "prov_udemy", Name = "Udemy", ProviderType = "Commercial", AuthorityScore = 0.8, LogoUrl = "https://www.udemy.com/staticx/udemy/images/v7/logo-udemy.svg", WebsiteUrl = "https://www.udemy.com/" },
                new CourseProvider { Id = "prov_nsdc", Name = "NSDC", ProviderType = "Government", AuthorityScore = 0.95, LogoUrl = "https://nsdcindia.org/sites/default/files/nsdc-logo.png", WebsiteUrl = "https://nsdcindia.org/" }
            };

            await context.CourseProviders.AddRangeAsync(providers);
            await context.SaveChangesAsync();

            // Trending Skills
            var trendingSkills = new List<TrendingSkill>
            {
                new TrendingSkill { SkillName = "Generative AI", TrendingScore = 98.5, TrendDirection = "+24% ↑" },
                new TrendingSkill { SkillName = "Data Science", TrendingScore = 95.0, TrendDirection = "+12% ↑" },
                new TrendingSkill { SkillName = "Cloud Computing", TrendingScore = 92.5, TrendDirection = "+8% ↑" },
                new TrendingSkill { SkillName = "Cybersecurity", TrendingScore = 91.0, TrendDirection = "+15% ↑" },
                new TrendingSkill { SkillName = "Project Management", TrendingScore = 88.0, TrendDirection = "Stable" },
                new TrendingSkill { SkillName = "Digital Marketing", TrendingScore = 85.0, TrendDirection = "+5% ↑" },
                new TrendingSkill { SkillName = "Business Analytics", TrendingScore = 86.5, TrendDirection = "+7% ↑" },
                new TrendingSkill { SkillName = "UI/UX Design", TrendingScore = 82.0, TrendDirection = "Stable" }
            };
            await context.TrendingSkills.AddRangeAsync(trendingSkills);

            // Courses
            var courses = new List<Course>
            {
                new Course
                {
                    Id = "c_google_da",
                    Title = "Google Data Analytics Professional Certificate",
                    Description = "Get started in the fast-growing field of data analytics with a professional certificate from Google.",
                    ProviderId = "prov_google",
                    Url = "https://www.coursera.org/professional-certificates/google-data-analytics",
                    ImageUrl = "https://d3njjcbhbojbot.cloudfront.net/api/utilities/v1/imageproxy/https://d15cw65ipcemk0.cloudfront.net/images/1/88/Google-Data-Analytics.png",
                    Domain = "Technology",
                    Subdomain = "Data Analytics",
                    Level = "Beginner",
                    DurationValue = 6,
                    DurationUnit = "Months",
                    LearningMode = "Online",
                    CertificateAvailable = true,
                    CertificateType = "Professional Certificate",
                    IsFree = false,
                    Price = 12000,
                    FinancialAidAvailable = true,
                    Rating = 4.8,
                    ReviewCount = 145000,
                    TrendingScore = 98.0,
                    TrendDirection = "+12% ↑",
                    RelevanceScore = 95.0,
                    Source = "Coursera API",
                    LastVerified = DateTime.UtcNow
                },
                new Course
                {
                    Id = "c_nptel_ml",
                    Title = "Introduction to Machine Learning",
                    Description = "Prof. Balaraman Ravindran | IIT Madras. An introductory course on machine learning covering classification, regression, clustering and more.",
                    ProviderId = "prov_nptel",
                    Url = "https://onlinecourses.nptel.ac.in/noc23_cs98/preview",
                    ImageUrl = "https://nptel.ac.in/assets/images/nptel-logo.png",
                    Domain = "Technology",
                    Subdomain = "Machine Learning",
                    Level = "Intermediate",
                    DurationValue = 12,
                    DurationUnit = "Weeks",
                    LearningMode = "Online",
                    CertificateAvailable = true,
                    CertificateType = "Government Certificate",
                    IsFree = true,
                    Price = 0,
                    FinancialAidAvailable = false,
                    Rating = 4.6,
                    ReviewCount = 45000,
                    TrendingScore = 95.0,
                    TrendDirection = "+5% ↑",
                    RelevanceScore = 90.0,
                    Source = "NPTEL XML API",
                    LastVerified = DateTime.UtcNow
                },
                new Course
                {
                    Id = "c_swayam_business",
                    Title = "Business Environment",
                    Description = "IIM Bangalore's course on Business Environment providing a conceptual framework for understanding the national and global environment of business.",
                    ProviderId = "prov_swayam",
                    Url = "https://onlinecourses.swayam2.ac.in/imb23_mg01/preview",
                    ImageUrl = "https://swayam.gov.in/nc_details/swayam_logo.png",
                    Domain = "Business",
                    Subdomain = "Strategy",
                    Level = "Intermediate",
                    DurationValue = 6,
                    DurationUnit = "Weeks",
                    LearningMode = "Online",
                    CertificateAvailable = true,
                    CertificateType = "Government Certificate",
                    IsFree = true,
                    Price = 0,
                    FinancialAidAvailable = false,
                    Rating = 4.7,
                    ReviewCount = 12000,
                    TrendingScore = 85.0,
                    TrendDirection = "Stable",
                    RelevanceScore = 88.0,
                    Source = "SWAYAM Feed",
                    LastVerified = DateTime.UtcNow
                },
                new Course
                {
                    Id = "c_ms_az900",
                    Title = "Microsoft Azure Fundamentals (AZ-900)",
                    Description = "Learn cloud concepts, core Azure Services, Azure pricing and support, and the fundamentals of cloud security, privacy, compliance, and trust.",
                    ProviderId = "prov_microsoft",
                    Url = "https://learn.microsoft.com/en-us/training/paths/azure-fundamentals/",
                    ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/4/44/Microsoft_logo.svg",
                    Domain = "Technology",
                    Subdomain = "Cloud Computing",
                    Level = "Beginner",
                    DurationValue = 15,
                    DurationUnit = "Hours",
                    LearningMode = "Self-paced",
                    CertificateAvailable = true,
                    CertificateType = "Industry Certification",
                    IsFree = true,
                    Price = 0,
                    FinancialAidAvailable = false,
                    Rating = 4.8,
                    ReviewCount = 350000,
                    TrendingScore = 92.0,
                    TrendDirection = "+8% ↑",
                    RelevanceScore = 94.0,
                    Source = "Microsoft Learn API",
                    LastVerified = DateTime.UtcNow
                },
                new Course
                {
                    Id = "c_aws_practitioner",
                    Title = "AWS Certified Cloud Practitioner",
                    Description = "This course is for anyone looking to learn the AWS Cloud from a business perspective.",
                    ProviderId = "prov_aws",
                    Url = "https://aws.amazon.com/certification/certified-cloud-practitioner/",
                    ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/9/93/Amazon_Web_Services_Logo.svg",
                    Domain = "Technology",
                    Subdomain = "Cloud Computing",
                    Level = "Beginner",
                    DurationValue = 6,
                    DurationUnit = "Hours",
                    LearningMode = "Self-paced",
                    CertificateAvailable = true,
                    CertificateType = "Industry Certification",
                    IsFree = true,
                    Price = 0,
                    FinancialAidAvailable = false,
                    Rating = 4.7,
                    ReviewCount = 200000,
                    TrendingScore = 90.0,
                    TrendDirection = "+5% ↑",
                    RelevanceScore = 92.0,
                    Source = "AWS Training RSS",
                    LastVerified = DateTime.UtcNow
                },
                new Course
                {
                    Id = "c_coursera_medicine",
                    Title = "Anatomy: Human Neuroanatomy",
                    Description = "Learn about the complex structure and function of the human central nervous system from the University of Michigan.",
                    ProviderId = "prov_coursera",
                    Url = "https://www.coursera.org/learn/anatomy",
                    ImageUrl = "https://d3njjcbhbojbot.cloudfront.net/api/utilities/v1/imageproxy/https://coursera.s3.amazonaws.com/topics/anatomy/icon.png",
                    Domain = "Healthcare & Medical",
                    Subdomain = "Medicine",
                    Level = "Intermediate",
                    DurationValue = 24,
                    DurationUnit = "Hours",
                    LearningMode = "Online",
                    CertificateAvailable = true,
                    CertificateType = "Course Certificate",
                    IsFree = false,
                    Price = 3500,
                    FinancialAidAvailable = true,
                    Rating = 4.9,
                    ReviewCount = 8500,
                    TrendingScore = 78.0,
                    TrendDirection = "Stable",
                    RelevanceScore = 80.0,
                    Source = "Coursera API",
                    LastVerified = DateTime.UtcNow
                },
                new Course
                {
                    Id = "c_swayam_law",
                    Title = "Intellectual Property Rights and Competition Law",
                    Description = "IIT Kharagpur course exploring the interplay between Intellectual Property Rights and Competition Law.",
                    ProviderId = "prov_swayam",
                    Url = "https://onlinecourses.nptel.ac.in/noc23_lw01/preview",
                    ImageUrl = "https://swayam.gov.in/nc_details/swayam_logo.png",
                    Domain = "Law",
                    Subdomain = "Intellectual Property",
                    Level = "Advanced",
                    DurationValue = 8,
                    DurationUnit = "Weeks",
                    LearningMode = "Online",
                    CertificateAvailable = true,
                    CertificateType = "Government Certificate",
                    IsFree = true,
                    Price = 0,
                    FinancialAidAvailable = false,
                    Rating = 4.5,
                    ReviewCount = 4200,
                    TrendingScore = 72.0,
                    TrendDirection = "Stable",
                    RelevanceScore = 85.0,
                    Source = "SWAYAM Feed",
                    LastVerified = DateTime.UtcNow
                },
                new Course
                {
                    Id = "c_coursera_design",
                    Title = "Google UX Design Professional Certificate",
                    Description = "Get started in the fast-growing field of user experience (UX) design with a professional certificate from Google.",
                    ProviderId = "prov_google",
                    Url = "https://www.coursera.org/professional-certificates/google-ux-design",
                    ImageUrl = "https://d3njjcbhbojbot.cloudfront.net/api/utilities/v1/imageproxy/https://d15cw65ipcemk0.cloudfront.net/images/1/88/Google-UX-Design.png",
                    Domain = "Design & Media",
                    Subdomain = "UI/UX",
                    Level = "Beginner",
                    DurationValue = 6,
                    DurationUnit = "Months",
                    LearningMode = "Online",
                    CertificateAvailable = true,
                    CertificateType = "Professional Certificate",
                    IsFree = false,
                    Price = 12000,
                    FinancialAidAvailable = true,
                    Rating = 4.8,
                    ReviewCount = 95000,
                    TrendingScore = 93.0,
                    TrendDirection = "+10% ↑",
                    RelevanceScore = 94.0,
                    Source = "Coursera API",
                    LastVerified = DateTime.UtcNow
                },
                new Course
                {
                    Id = "c_udemy_civil",
                    Title = "AutoCAD 2023 MasterClass: Beginner to Advanced",
                    Description = "Learn AutoCAD 2023 completely from scratch to advanced level.",
                    ProviderId = "prov_udemy",
                    Url = "https://www.udemy.com/course/autocad-2023-masterclass/",
                    ImageUrl = "https://www.udemy.com/staticx/udemy/images/v7/logo-udemy.svg",
                    Domain = "Engineering",
                    Subdomain = "Civil Engineering",
                    Level = "Beginner",
                    DurationValue = 18,
                    DurationUnit = "Hours",
                    LearningMode = "Self-paced",
                    CertificateAvailable = true,
                    CertificateType = "Course Certificate",
                    IsFree = false,
                    Price = 3499,
                    FinancialAidAvailable = false,
                    Rating = 4.7,
                    ReviewCount = 28000,
                    TrendingScore = 82.0,
                    TrendDirection = "Stable",
                    RelevanceScore = 88.0,
                    Source = "Udemy API",
                    LastVerified = DateTime.UtcNow
                },
                new Course
                {
                    Id = "c_edx_finance",
                    Title = "Finance for Everyone: Smart Tools for Decision-Making",
                    Description = "Learn practical frameworks and tools to make smart financial decisions, from the University of Michigan.",
                    ProviderId = "prov_edx",
                    Url = "https://www.edx.org/course/finance-for-everyone-smart-tools-for-decision-making",
                    ImageUrl = "https://www.edx.org/images/logos/edx-logo-elm.svg",
                    Domain = "Business",
                    Subdomain = "Finance",
                    Level = "Beginner",
                    DurationValue = 6,
                    DurationUnit = "Weeks",
                    LearningMode = "Online",
                    CertificateAvailable = true,
                    CertificateType = "University Certificate",
                    IsFree = true,
                    Price = 0,
                    FinancialAidAvailable = false,
                    Rating = 4.6,
                    ReviewCount = 15000,
                    TrendingScore = 80.0,
                    TrendDirection = "+2% ↑",
                    RelevanceScore = 85.0,
                    Source = "edX API",
                    LastVerified = DateTime.UtcNow
                },
                new Course
                {
                    Id = "c_gov_banking",
                    Title = "IBPS PO Preparation Course",
                    Description = "Comprehensive preparation for Banking exams covering Quantitative Aptitude, Reasoning, and Banking Awareness.",
                    ProviderId = "prov_nsdc",
                    Url = "https://nsdcindia.org/",
                    ImageUrl = "https://nsdcindia.org/sites/default/files/nsdc-logo.png",
                    Domain = "Government / Public Sector",
                    Subdomain = "Government Exam Preparation",
                    Level = "Intermediate",
                    DurationValue = 3,
                    DurationUnit = "Months",
                    LearningMode = "Online",
                    CertificateAvailable = false,
                    CertificateType = "",
                    IsFree = false,
                    Price = 1500,
                    FinancialAidAvailable = false,
                    Rating = 4.5,
                    ReviewCount = 5600,
                    TrendingScore = 89.0,
                    TrendDirection = "+15% ↑",
                    RelevanceScore = 90.0,
                    Source = "NSDC Feed",
                    LastVerified = DateTime.UtcNow
                }
            };
            await context.Courses.AddRangeAsync(courses);
            
            // Link Skills and Eligibilities
            var courseSkills = new List<CourseSkill>
            {
                new CourseSkill { CourseId = "c_google_da", SkillName = "Data Analysis" },
                new CourseSkill { CourseId = "c_google_da", SkillName = "SQL" },
                new CourseSkill { CourseId = "c_google_da", SkillName = "R Programming" },
                new CourseSkill { CourseId = "c_google_da", SkillName = "Tableau" },
                new CourseSkill { CourseId = "c_nptel_ml", SkillName = "Machine Learning" },
                new CourseSkill { CourseId = "c_nptel_ml", SkillName = "Python" },
                new CourseSkill { CourseId = "c_nptel_ml", SkillName = "Data Science" },
                new CourseSkill { CourseId = "c_ms_az900", SkillName = "Azure" },
                new CourseSkill { CourseId = "c_ms_az900", SkillName = "Cloud Computing" },
                new CourseSkill { CourseId = "c_aws_practitioner", SkillName = "AWS" },
                new CourseSkill { CourseId = "c_aws_practitioner", SkillName = "Cloud Architecture" },
                new CourseSkill { CourseId = "c_udemy_civil", SkillName = "AutoCAD" },
                new CourseSkill { CourseId = "c_udemy_civil", SkillName = "Drafting" },
                new CourseSkill { CourseId = "c_coursera_medicine", SkillName = "Anatomy" },
                new CourseSkill { CourseId = "c_coursera_medicine", SkillName = "Neuroscience" },
                new CourseSkill { CourseId = "c_gov_banking", SkillName = "Quantitative Aptitude" },
                new CourseSkill { CourseId = "c_gov_banking", SkillName = "Logical Reasoning" },
                new CourseSkill { CourseId = "c_gov_banking", SkillName = "Banking Awareness" }
            };
            await context.CourseSkills.AddRangeAsync(courseSkills);

            var educationEligibilities = new List<CourseEducationEligibility>
            {
                new CourseEducationEligibility { CourseId = "c_google_da", EducationLevel = "No prerequisite" },
                new CourseEducationEligibility { CourseId = "c_nptel_ml", EducationLevel = "Undergraduate" },
                new CourseEducationEligibility { CourseId = "c_nptel_ml", EducationLevel = "Graduate" },
                new CourseEducationEligibility { CourseId = "c_ms_az900", EducationLevel = "No prerequisite" },
                new CourseEducationEligibility { CourseId = "c_coursera_medicine", EducationLevel = "Undergraduate" },
                new CourseEducationEligibility { CourseId = "c_coursera_medicine", EducationLevel = "MBBS" },
                new CourseEducationEligibility { CourseId = "c_gov_banking", EducationLevel = "Graduate" }
            };
            await context.CourseEducationEligibilities.AddRangeAsync(educationEligibilities);
            
            // Dummy Careers mapping
            var courseCareers = new List<CourseCareer>
            {
                new CourseCareer { CourseId = "c_google_da", CareerTitle = "Data Analyst" },
                new CourseCareer { CourseId = "c_google_da", CareerTitle = "Data Scientist" },
                new CourseCareer { CourseId = "c_nptel_ml", CareerTitle = "Machine Learning Engineer" },
                new CourseCareer { CourseId = "c_udemy_civil", CareerTitle = "Civil Engineer" },
                new CourseCareer { CourseId = "c_coursera_medicine", CareerTitle = "Doctor" },
                new CourseCareer { CourseId = "c_coursera_medicine", CareerTitle = "Medical Researcher" },
                new CourseCareer { CourseId = "c_swayam_law", CareerTitle = "Lawyer" },
                new CourseCareer { CourseId = "c_swayam_law", CareerTitle = "Corporate Lawyer" },
                new CourseCareer { CourseId = "c_gov_banking", CareerTitle = "Bank PO" }
            };
            await context.CourseCareers.AddRangeAsync(courseCareers);

            await context.SaveChangesAsync();
        }
    }
}
