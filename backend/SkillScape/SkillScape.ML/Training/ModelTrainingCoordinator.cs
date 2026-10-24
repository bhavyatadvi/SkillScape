using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.ML.Training;

public class ModelTrainingCoordinator : IModelTrainingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ModelTrainingCoordinator> _logger;
    private readonly MLContext _mlContext;

    public ModelTrainingCoordinator(ApplicationDbContext context, ILogger<ModelTrainingCoordinator> logger)
    {
        _context = context;
        _logger = logger;
        _mlContext = new MLContext(seed: 42);
    }

    public async Task TrainAllModelsAsync()
    {
        _logger.LogInformation("Starting ML.NET Multi-Model Training Pipeline (Parts 26-27)");

        // 1. Ensure we have data
        bool hasData = await _context.TrainingDataRecords.AnyAsync();
        if (!hasData)
        {
            _logger.LogInformation("No training data found. Generating mock training dataset...");
            await GenerateMockDataAsync();
        }

        // 2. Load data from DB
        var rawData = await _context.TrainingDataRecords.ToListAsync();
        var mlData = rawData.Select(r => new MLDataRecord
        {
            MathematicsScore = r.MathematicsScore,
            PhysicsScore = r.PhysicsScore,
            ProgrammingInterest = r.ProgrammingInterest,
            SpatialReasoning = r.SpatialReasoning,
            HandsOnInterest = r.HandsOnInterest,
            DomainLabel = r.DomainLabel,
            FieldLabel = r.FieldLabel,
            SpecializationLabel = r.SpecializationLabel
        }).ToList();

        IDataView dataView = _mlContext.Data.LoadFromEnumerable(mlData);

        // Split data into train and test sets (stratified)
        var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

        // 3. Train MODEL A (Broad Domain Classifier)
        TrainClassifier(split.TrainSet, split.TestSet, "DomainLabel", "ModelA_Domain");

        // 4. Train MODEL B (Field Classifier)
        TrainClassifier(split.TrainSet, split.TestSet, "FieldLabel", "ModelB_Field");

        // 5. Train MODEL C (Specialization Classifier)
        TrainClassifier(split.TrainSet, split.TestSet, "SpecializationLabel", "ModelC_Specialization");

        _logger.LogInformation("All models trained and evaluated successfully.");
    }

    private void TrainClassifier(IDataView trainSet, IDataView testSet, string labelColumnName, string modelName)
    {
        _logger.LogInformation($"Training {modelName} for label: {labelColumnName}");

        // Build the pipeline
        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", labelColumnName)
            .Append(_mlContext.Transforms.Concatenate("Features", 
                nameof(MLDataRecord.MathematicsScore),
                nameof(MLDataRecord.PhysicsScore),
                nameof(MLDataRecord.ProgrammingInterest),
                nameof(MLDataRecord.SpatialReasoning),
                nameof(MLDataRecord.HandsOnInterest)))
            .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(_mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features"))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        // Train
        var model = pipeline.Fit(trainSet);

        // Evaluate
        var predictions = model.Transform(testSet);
        var metrics = _mlContext.MulticlassClassification.Evaluate(predictions);

        _logger.LogInformation($"--- {modelName} Evaluation Metrics ---");
        _logger.LogInformation($"Micro Accuracy: {metrics.MicroAccuracy:0.###}");
        _logger.LogInformation($"Macro Accuracy: {metrics.MacroAccuracy:0.###}");
        _logger.LogInformation($"Log Loss: {metrics.LogLoss:0.###}");
        
        // Save model (mock saving, in reality we'd save to blob/file)
        // _mlContext.Model.Save(model, trainSet.Schema, $"{modelName}.zip");
    }

    private async Task GenerateMockDataAsync()
    {
        var random = new Random(42);
        var records = new List<TrainingDataRecord>();

        // Generate Engineering Profiles
        for (int i = 0; i < 500; i++)
        {
            records.Add(new TrainingDataRecord
            {
                EducationStage = "After12th",
                Stream = "PCM",
                MathematicsScore = (float)random.NextDouble() * 50 + 50, // High math
                PhysicsScore = (float)random.NextDouble() * 50 + 50, // High physics
                ProgrammingInterest = (float)random.NextDouble() * 100,
                SpatialReasoning = (float)random.NextDouble() * 40 + 60,
                HandsOnInterest = (float)random.NextDouble() * 60 + 40,
                DomainLabel = "Engineering",
                FieldLabel = random.NextDouble() > 0.5 ? "Computer Science" : "Civil Engineering",
                SpecializationLabel = random.NextDouble() > 0.5 ? "Machine Learning" : "Structural Engineering"
            });
        }

        // Generate Medicine Profiles
        for (int i = 0; i < 300; i++)
        {
            records.Add(new TrainingDataRecord
            {
                EducationStage = "After12th",
                Stream = "PCB",
                MathematicsScore = (float)random.NextDouble() * 60, // Lower math
                PhysicsScore = (float)random.NextDouble() * 40 + 60, // Med physics
                ProgrammingInterest = (float)random.NextDouble() * 30, // Low prog
                SpatialReasoning = (float)random.NextDouble() * 50 + 50,
                HandsOnInterest = (float)random.NextDouble() * 40 + 60, // High hands on (clinical)
                DomainLabel = "Medicine",
                FieldLabel = "Clinical Medicine",
                SpecializationLabel = random.NextDouble() > 0.5 ? "Surgery" : "Pediatrics"
            });
        }

        await _context.TrainingDataRecords.AddRangeAsync(records);
        await _context.SaveChangesAsync();
    }

    // ── Internal ML Data Mapping ──────────────────────────────────────────────
    
    public class MLDataRecord
    {
        public float MathematicsScore { get; set; }
        public float PhysicsScore { get; set; }
        public float ProgrammingInterest { get; set; }
        public float SpatialReasoning { get; set; }
        public float HandsOnInterest { get; set; }
        
        public string DomainLabel { get; set; } = string.Empty;
        public string FieldLabel { get; set; } = string.Empty;
        public string SpecializationLabel { get; set; } = string.Empty;
    }
}
