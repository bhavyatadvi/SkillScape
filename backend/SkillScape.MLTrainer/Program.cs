using System;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace SkillScape.MLTrainer
{
    public class CareerFeatureVector
    {
        [LoadColumn(1, 30)]
        [VectorType(30)]
        public float[] Features { get; set; } = Array.Empty<float>();

        [LoadColumn(0)]
        public string Target { get; set; } = string.Empty;
    }

    public class CareerPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedTarget { get; set; } = string.Empty;
        
        public float[] Score { get; set; } = Array.Empty<float>();
    }

    class Program
    {
        static void Main(string[] args)
        {
            var mlContext = new MLContext(seed: 1);
            
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Training Career Predictor (FastForest)...");
            string dataPath = Path.Combine(Environment.CurrentDirectory, "..", "DataGeneration", "career_training_data.csv");
            string modelPath = Path.Combine(Environment.CurrentDirectory, "CareerPredictor.zip");

            if (!File.Exists(dataPath)) {
                Console.WriteLine("Training data not found at: " + dataPath);
                return;
            }

            Console.WriteLine("Loading data...");
            IDataView dataView = mlContext.Data.LoadFromTextFile<CareerFeatureVector>(dataPath, hasHeader: true, separatorChar: ',');
            var splitInfo = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            Console.WriteLine("Building FastForest pipeline...");
            var pipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(CareerFeatureVector.Target))
                .Append(mlContext.MulticlassClassification.Trainers.FastForest(labelColumnName: "Label", featureColumnName: nameof(CareerFeatureVector.Features), numberOfTrees: 100))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = pipeline.Fit(splitInfo.TrainSet);

            var predictions = model.Transform(splitInfo.TestSet);
            var metrics = mlContext.MulticlassClassification.Evaluate(predictions, "Label", "Score");

            Console.WriteLine($"Macro Accuracy: {metrics.MacroAccuracy:P2}");
            Console.WriteLine($"Micro Accuracy: {metrics.MicroAccuracy:P2}");
            Console.WriteLine($"Log Loss: {metrics.LogLoss:F4}");

            mlContext.Model.Save(model, dataView.Schema, modelPath);
            Console.WriteLine($"Career Predictor saved to {modelPath}");
        }
    }
}
