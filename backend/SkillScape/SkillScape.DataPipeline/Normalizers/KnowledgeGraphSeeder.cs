using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Services;

namespace SkillScape.DataPipeline.Normalizers;

public static class KnowledgeGraphSeeder
{
    public static async Task SeedMockGovernmentTaxonomyAsync(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("KnowledgeGraphSeeder");
        var kgService = serviceProvider.GetRequiredService<IKnowledgeIngestionService>();

        logger.LogInformation("Starting Tier 1 & Tier 2 Knowledge Graph Ingestion...");

        var provenanceTier1 = new ProvenanceMetadata
        {
            SourceName = "National Career Service (NCS) - India / AICTE",
            SourceType = "OfficialGovernment",
            SourceUrl = "https://www.ncs.gov.in/",
            Confidence = 0.95,
            PublicationDate = new DateTime(2023, 1, 1)
        };

        var provenanceTier2 = new ProvenanceMetadata
        {
            SourceName = "ESCO (European Skills, Competences, Qualifications and Occupations)",
            SourceType = "StructuredTaxonomy",
            SourceUrl = "https://esco.ec.europa.eu/",
            Confidence = 0.90,
            PublicationDate = new DateTime(2022, 1, 1)
        };

        // 1. Broad Domains (Level 1)
        await kgService.IngestNodeAsync("domain_engineering", "BroadDomain", "Engineering and Technology", "Study of engineering, technology, and applied sciences.", "{}", provenanceTier1);
        await kgService.IngestNodeAsync("domain_medicine", "BroadDomain", "Medicine and Health Sciences", "Study of human health, medicine, and clinical sciences.", "{}", provenanceTier1);
        await kgService.IngestNodeAsync("domain_commerce", "BroadDomain", "Commerce and Business", "Study of business, trade, and finance.", "{}", provenanceTier1);
        await kgService.IngestNodeAsync("domain_arts", "BroadDomain", "Arts and Humanities", "Study of human culture, society, and expression.", "{}", provenanceTier1);

        // 2. Fields (Level 2) - Part 31 Differentiation
        // Engineering differentiations
        await kgService.IngestNodeAsync("field_cs", "Field", "Computer Science", "Theory and design of computer systems.", "{}", provenanceTier1);
        await kgService.IngestNodeAsync("field_it", "Field", "Information Technology", "Application of computers to store and manipulate data.", "{}", provenanceTier1);
        await kgService.IngestNodeAsync("field_se", "Field", "Software Engineering", "Engineering principles applied to software development.", "{}", provenanceTier1);
        await kgService.IngestNodeAsync("field_ds", "Field", "Data Science", "Extracting knowledge from structured and unstructured data.", "{}", provenanceTier1);
        await kgService.IngestNodeAsync("field_ai", "Field", "AI/ML", "Development of intelligent systems.", "{}", provenanceTier1);
        await kgService.IngestNodeAsync("field_cyber", "Field", "Cybersecurity", "Protection of computer systems and networks.", "{}", provenanceTier1);
        
        await kgService.IngestNodeAsync("field_civil", "Field", "Civil Engineering", "AICTE approved field for infrastructure and structural design.", "{}", provenanceTier1);
        await kgService.IngestNodeAsync("field_arch", "Field", "Architecture", "Design of buildings and environments.", "{}", provenanceTier1);
        
        await kgService.IngestNodeAsync("field_mech", "Field", "Mechanical Engineering", "Design and manufacturing of mechanical systems.", "{}", provenanceTier1);
        await kgService.IngestNodeAsync("field_elec", "Field", "Electrical Engineering", "Study and application of electricity, electronics, and electromagnetism.", "{}", provenanceTier1);

        // Commerce differentiations
        await kgService.IngestNodeAsync("field_accounting", "Field", "Accounting", "Financial record keeping and reporting.", "{}", provenanceTier1);
        await kgService.IngestNodeAsync("field_finance", "Field", "Finance", "Management of money and investments.", "{}", provenanceTier1);

        // Medicine differentiations
        await kgService.IngestNodeAsync("field_clinical", "Field", "Clinical Medicine", "NMC recognized field of direct patient care.", "{}", provenanceTier1);
        await kgService.IngestNodeAsync("field_dentistry", "Field", "Dentistry", "Diagnosis and treatment of oral diseases.", "{}", provenanceTier1);
        await kgService.IngestNodeAsync("field_nursing", "Field", "Nursing", "Care of individuals to attain or maintain optimal health.", "{}", provenanceTier1);

        // Link Fields to Domains
        await kgService.LinkNodesAsync("domain_engineering", "field_cs", "HAS_FIELD", "{}", provenanceTier1);
        await kgService.LinkNodesAsync("domain_engineering", "field_it", "HAS_FIELD", "{}", provenanceTier1);
        await kgService.LinkNodesAsync("domain_engineering", "field_se", "HAS_FIELD", "{}", provenanceTier1);
        await kgService.LinkNodesAsync("domain_engineering", "field_ds", "HAS_FIELD", "{}", provenanceTier1);
        await kgService.LinkNodesAsync("domain_engineering", "field_ai", "HAS_FIELD", "{}", provenanceTier1);
        await kgService.LinkNodesAsync("domain_engineering", "field_cyber", "HAS_FIELD", "{}", provenanceTier1);
        await kgService.LinkNodesAsync("domain_engineering", "field_civil", "HAS_FIELD", "{}", provenanceTier1);
        await kgService.LinkNodesAsync("domain_engineering", "field_arch", "HAS_FIELD", "{}", provenanceTier1);
        await kgService.LinkNodesAsync("domain_engineering", "field_mech", "HAS_FIELD", "{}", provenanceTier1);
        await kgService.LinkNodesAsync("domain_engineering", "field_elec", "HAS_FIELD", "{}", provenanceTier1);
        
        await kgService.LinkNodesAsync("domain_commerce", "field_accounting", "HAS_FIELD", "{}", provenanceTier1);
        await kgService.LinkNodesAsync("domain_commerce", "field_finance", "HAS_FIELD", "{}", provenanceTier1);
        
        await kgService.LinkNodesAsync("domain_medicine", "field_clinical", "HAS_FIELD", "{}", provenanceTier1);
        await kgService.LinkNodesAsync("domain_medicine", "field_dentistry", "HAS_FIELD", "{}", provenanceTier1);
        await kgService.LinkNodesAsync("domain_medicine", "field_nursing", "HAS_FIELD", "{}", provenanceTier1);

        // 3. Subfields (Level 3) & 4. Specializations
        await kgService.IngestNodeAsync("subfield_structural", "Subfield", "Structural Engineering", "Analysis and design of structures that support or resist loads.", "{}", provenanceTier1);
        await kgService.LinkNodesAsync("field_civil", "subfield_structural", "HAS_SUBFIELD", "{}", provenanceTier1);

        await kgService.IngestNodeAsync("spec_ml", "Specialization", "Deep Learning", "Advanced neural network architectures.", "{}", provenanceTier1);
        await kgService.LinkNodesAsync("field_ai", "spec_ml", "HAS_SPECIALIZATION", "{}", provenanceTier1);

        // 5. Career Families & Specific Careers (Level 5 & 6) - TIER 2 (ESCO/ONET mappings)
        await kgService.IngestNodeAsync("career_family_data", "CareerFamily", "Data and AI Professionals", "Professionals working with data processing and predictive models.", "{}", provenanceTier2);
        await kgService.IngestNodeAsync("career_ml_engineer", "SpecificCareer", "Machine Learning Engineer", "ESCO mapped: ICT professionals designing ML systems.", JsonSerializer.Serialize(new { EscoCode = "2511" }), provenanceTier2);

        await kgService.LinkNodesAsync("spec_ml", "career_family_data", "LEADS_TO_FAMILY", "{}", provenanceTier2);
        await kgService.LinkNodesAsync("career_family_data", "career_ml_engineer", "INCLUDES_CAREER", "{}", provenanceTier2);

        logger.LogInformation("Tier 1 & Tier 2 Knowledge Graph Ingestion Complete.");
    }
}
