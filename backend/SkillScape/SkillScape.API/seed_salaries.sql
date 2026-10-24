DECLARE @SourceId1 VARCHAR(50) = NEWID();
DECLARE @SourceId2 VARCHAR(50) = NEWID();
DECLARE @SourceId3 VARCHAR(50) = NEWID();
DECLARE @SourceId4 VARCHAR(50) = NEWID();
DECLARE @SourceId5 VARCHAR(50) = NEWID();

-- 1. Insert SalaryBenchmarks
-- Software
IF NOT EXISTS (SELECT 1 FROM SalaryBenchmarks WHERE RoleKey = 'software_engineer' AND ExperienceBand = '0-3')
BEGIN
    DECLARE @BId1 VARCHAR(50) = NEWID();
    INSERT INTO SalaryBenchmarks (Id, RoleKey, CanonicalRole, OccupationFamily, Domain, Country, Currency, ExperienceBand, P25Salary, P50Salary, P75Salary, DataSourceCount, SourceDate, LastUpdated, DataQuality, Notes)
    VALUES (@BId1, 'software_engineer', 'Software Engineer', 'IT', 'Technology', 'India', 'INR', '0-3', 502000, 700000, 923000, 1, '2025-10-01', GETUTCDATE(), 'CategoryBased', 'foundit October 2025 Salary Trends report');
    
    INSERT INTO SalaryBenchmarkSources (Id, SalaryBenchmarkId, SourceName, SourceUrl, PublishedAt, RetrievedAt, SampleSize, SourceType, Methodology, Notes)
    VALUES (@SourceId1, @BId1, 'foundit Salary Trends Report', '', '2025-10-01', GETUTCDATE(), 1000, 'IndustryReport', 'CategoryBased', 'Computer Software');
END

-- AI
IF NOT EXISTS (SELECT 1 FROM SalaryBenchmarks WHERE RoleKey = 'ai_engineer' AND ExperienceBand = '0-3')
BEGIN
    DECLARE @BId2 VARCHAR(50) = NEWID();
    INSERT INTO SalaryBenchmarks (Id, RoleKey, CanonicalRole, OccupationFamily, Domain, Country, Currency, ExperienceBand, P25Salary, P50Salary, P75Salary, DataSourceCount, SourceDate, LastUpdated, DataQuality, Notes)
    VALUES (@BId2, 'ai_engineer', 'Artificial Intelligence Engineer', 'IT', 'Technology', 'India', 'INR', '0-3', 792000, 1000000, 1376000, 1, '2025-10-01', GETUTCDATE(), 'CategoryBased', 'foundit October 2025 Salary Trends report');
    
    INSERT INTO SalaryBenchmarkSources (Id, SalaryBenchmarkId, SourceName, SourceUrl, PublishedAt, RetrievedAt, SampleSize, SourceType, Methodology, Notes)
    VALUES (@SourceId2, @BId2, 'foundit Salary Trends Report', '', '2025-10-01', GETUTCDATE(), 1000, 'IndustryReport', 'CategoryBased', 'Artificial Intelligence');
END

-- Analytics
IF NOT EXISTS (SELECT 1 FROM SalaryBenchmarks WHERE RoleKey = 'data_analyst' AND ExperienceBand = '0-3')
BEGIN
    DECLARE @BId3 VARCHAR(50) = NEWID();
    INSERT INTO SalaryBenchmarks (Id, RoleKey, CanonicalRole, OccupationFamily, Domain, Country, Currency, ExperienceBand, P25Salary, P50Salary, P75Salary, DataSourceCount, SourceDate, LastUpdated, DataQuality, Notes)
    VALUES (@BId3, 'data_analyst', 'Data Analyst / Scientist', 'IT', 'Technology', 'India', 'INR', '0-3', 374000, 500000, 708000, 1, '2025-10-01', GETUTCDATE(), 'CategoryBased', 'foundit October 2025 Salary Trends report');
    
    INSERT INTO SalaryBenchmarkSources (Id, SalaryBenchmarkId, SourceName, SourceUrl, PublishedAt, RetrievedAt, SampleSize, SourceType, Methodology, Notes)
    VALUES (@SourceId3, @BId3, 'foundit Salary Trends Report', '', '2025-10-01', GETUTCDATE(), 1000, 'IndustryReport', 'CategoryBased', 'Analytics');
END

-- Security
IF NOT EXISTS (SELECT 1 FROM SalaryBenchmarks WHERE RoleKey = 'cybersecurity_analyst' AND ExperienceBand = '0-3')
BEGIN
    DECLARE @BId4 VARCHAR(50) = NEWID();
    INSERT INTO SalaryBenchmarks (Id, RoleKey, CanonicalRole, OccupationFamily, Domain, Country, Currency, ExperienceBand, P25Salary, P50Salary, P75Salary, DataSourceCount, SourceDate, LastUpdated, DataQuality, Notes)
    VALUES (@BId4, 'cybersecurity_analyst', 'Cybersecurity Analyst', 'IT', 'Technology', 'India', 'INR', '0-3', 378000, 500000, 704000, 1, '2025-10-01', GETUTCDATE(), 'CategoryBased', 'foundit October 2025 Salary Trends report');
    
    INSERT INTO SalaryBenchmarkSources (Id, SalaryBenchmarkId, SourceName, SourceUrl, PublishedAt, RetrievedAt, SampleSize, SourceType, Methodology, Notes)
    VALUES (@SourceId4, @BId4, 'foundit Salary Trends Report', '', '2025-10-01', GETUTCDATE(), 1000, 'IndustryReport', 'CategoryBased', 'Computer & Network Security');
END

-- Networking
IF NOT EXISTS (SELECT 1 FROM SalaryBenchmarks WHERE RoleKey = 'network_engineer' AND ExperienceBand = '0-3')
BEGIN
    DECLARE @BId5 VARCHAR(50) = NEWID();
    INSERT INTO SalaryBenchmarks (Id, RoleKey, CanonicalRole, OccupationFamily, Domain, Country, Currency, ExperienceBand, P25Salary, P50Salary, P75Salary, DataSourceCount, SourceDate, LastUpdated, DataQuality, Notes)
    VALUES (@BId5, 'network_engineer', 'Network Engineer', 'IT', 'Technology', 'India', 'INR', '0-3', 443000, 600000, 798000, 1, '2025-10-01', GETUTCDATE(), 'CategoryBased', 'foundit October 2025 Salary Trends report');
    
    INSERT INTO SalaryBenchmarkSources (Id, SalaryBenchmarkId, SourceName, SourceUrl, PublishedAt, RetrievedAt, SampleSize, SourceType, Methodology, Notes)
    VALUES (@SourceId5, @BId5, 'foundit Salary Trends Report', '', '2025-10-01', GETUTCDATE(), 1000, 'IndustryReport', 'CategoryBased', 'Computer Networking');
END

-- Ensure CareerSalaryMappings
-- Map Software Engineer careers
DECLARE @CS VARCHAR(100);
DECLARE CareerCursor CURSOR FOR 
SELECT Id FROM CareerTaxonomyV2 WHERE Id IN ('sci.eng.cs', 'sci.eng.cs.software', 'tech.cyber.cloud_sec');
OPEN CareerCursor;
FETCH NEXT FROM CareerCursor INTO @CS;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM CareerSalaryMappings WHERE CareerPathId = @CS)
        INSERT INTO CareerSalaryMappings (Id, CareerPathId, SalaryBenchmarkRoleKey, MappingConfidence, MappingMethod, CreatedAt) VALUES (NEWID(), @CS, 'software_engineer', 1.0, 'ManualRule', GETUTCDATE());
    FETCH NEXT FROM CareerCursor INTO @CS;
END;
CLOSE CareerCursor; DEALLOCATE CareerCursor;

-- Map AI careers
DECLARE CareerCursor2 CURSOR FOR 
SELECT Id FROM CareerTaxonomyV2 WHERE Id IN ('sci.eng.cs.ai_ml', 'tech.data.ml_eng');
OPEN CareerCursor2;
FETCH NEXT FROM CareerCursor2 INTO @CS;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM CareerSalaryMappings WHERE CareerPathId = @CS)
        INSERT INTO CareerSalaryMappings (Id, CareerPathId, SalaryBenchmarkRoleKey, MappingConfidence, MappingMethod, CreatedAt) VALUES (NEWID(), @CS, 'ai_engineer', 1.0, 'ManualRule', GETUTCDATE());
    FETCH NEXT FROM CareerCursor2 INTO @CS;
END;
CLOSE CareerCursor2; DEALLOCATE CareerCursor2;

-- Map Data careers
DECLARE CareerCursor3 CURSOR FOR 
SELECT Id FROM CareerTaxonomyV2 WHERE Id IN ('tech.data.analytics', 'tech.data.ds', 'sci.eng.cs.data');
OPEN CareerCursor3;
FETCH NEXT FROM CareerCursor3 INTO @CS;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM CareerSalaryMappings WHERE CareerPathId = @CS)
        INSERT INTO CareerSalaryMappings (Id, CareerPathId, SalaryBenchmarkRoleKey, MappingConfidence, MappingMethod, CreatedAt) VALUES (NEWID(), @CS, 'data_analyst', 1.0, 'ManualRule', GETUTCDATE());
    FETCH NEXT FROM CareerCursor3 INTO @CS;
END;
CLOSE CareerCursor3; DEALLOCATE CareerCursor3;

-- Map Security careers
DECLARE CareerCursor4 CURSOR FOR 
SELECT Id FROM CareerTaxonomyV2 WHERE Id IN ('tech.cyber', 'tech.cyber.forensics', 'tech.cyber.pentest', 'tech.cyber.soc', 'tech.cyber.threat_intel', 'sci.eng.cs.cybersec');
OPEN CareerCursor4;
FETCH NEXT FROM CareerCursor4 INTO @CS;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM CareerSalaryMappings WHERE CareerPathId = @CS)
        INSERT INTO CareerSalaryMappings (Id, CareerPathId, SalaryBenchmarkRoleKey, MappingConfidence, MappingMethod, CreatedAt) VALUES (NEWID(), @CS, 'cybersecurity_analyst', 1.0, 'ManualRule', GETUTCDATE());
    FETCH NEXT FROM CareerCursor4 INTO @CS;
END;
CLOSE CareerCursor4; DEALLOCATE CareerCursor4;

-- Map Network careers
DECLARE CareerCursor5 CURSOR FOR 
SELECT Id FROM CareerTaxonomyV2 WHERE Id IN ('sci.eng.cs.networks');
OPEN CareerCursor5;
FETCH NEXT FROM CareerCursor5 INTO @CS;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM CareerSalaryMappings WHERE CareerPathId = @CS)
        INSERT INTO CareerSalaryMappings (Id, CareerPathId, SalaryBenchmarkRoleKey, MappingConfidence, MappingMethod, CreatedAt) VALUES (NEWID(), @CS, 'network_engineer', 1.0, 'ManualRule', GETUTCDATE());
    FETCH NEXT FROM CareerCursor5 INTO @CS;
END;
CLOSE CareerCursor5; DEALLOCATE CareerCursor5;

SELECT 'Seeding Complete' as Result;
