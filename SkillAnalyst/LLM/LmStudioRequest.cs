using System.Text.Json.Serialization;

namespace SkillAnalyst.LLM;

public class LmStudioRequest
{
    [JsonPropertyName("messages")] 
    public List<LmMessage> Messages { get; set; }
    
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }
    
    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }
    
    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}

public class LmMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; }
    
    [JsonPropertyName("content")]
    public string Content { get; set; }
}