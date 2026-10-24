using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Program
{
    private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };
    
    private class QuizOptionData
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("signals")]
        public List<OptionSignal> Signals { get; set; } = new();
    }

    private class OptionSignal
    {
        [JsonPropertyName("nodeId")]
        public string NodeId { get; set; } = string.Empty;

        [JsonPropertyName("weight")]
        public double Weight { get; set; }
    }

    public static void Main()
    {
        var options = new List<object>
        {
            new { index = 0, text = "Test", signals = new[] { new { nodeId = "target1", weight = 0.8 }, new { nodeId = "target2", weight = 0.2 } } }
        };
        var OptionsJson = JsonSerializer.Serialize(options);
        Console.WriteLine("Serialized: " + OptionsJson);
        
        try
        {
            var optionsData = JsonSerializer.Deserialize<List<QuizOptionData>>(OptionsJson, _jsonOpts) ?? new();
            Console.WriteLine("Deserialized count: " + optionsData.Count);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex.ToString());
        }
    }
}
