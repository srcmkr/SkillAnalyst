using System.Net.Http.Json;
using LiteDB;
using Serilog;
using SkillAnalyst.Models;

namespace SkillAnalyst.LLM;

public class LocalLLM
{
    public static async Task EnrichAndSaveAsync(string databaseFilePath)
    {
        using var db = new LiteDatabase(databaseFilePath);
        var collection = db.GetCollection<MergedJobSkills>("jobs");
        
        var entries = 0;

        var preprompt =
            """
            You are a skill management expert. You have been hired by a company to analyze the required skills for jobs. You have been given a database of job descriptions and skills. Your task is to analyze the skills required for each job and provide a list of skills for each job. You will be paid based on the number of jobs.
            Because you're an expert, you merge similar skills together. For example, if you see 'Python' and 'Python 3', you merge them into 'Python'. The same goes for '.NET/C#' and '.NET Core 3.1', you'll merge to ".NET" You also remove any skills that are not relevant to the job or very common. For example, if you see 'Microsoft Word' in a job description, you remove it.
            Your output format is strict. You just provide a comma-separated list of skills for each job. You must not provide any other information. Any other format than the comma-separated list will destroy the dataset and you will not be paid.
            Here is the job description and the the skills an intern found out:  
            """;
        
        var requestUrl = "http://localhost:1234/v1/chat/completions";

        using var httpClient = new HttpClient();
        
        foreach (var job in collection.FindAll())
        {
            entries++;
            if (entries % 100 == 0) Log.Information($"Processed {entries} entries so far");

            if (string.IsNullOrWhiteSpace(job.LlmSkills))
            {
                var request = new LmStudioRequest
                {
                    Temperature = 0.7,
                    MaxTokens = 2000,
                    Stream = false,
                    Messages = [
                        new LmMessage
                        {
                            Role = "user",
                            Content = preprompt
                        },
                        new LmMessage
                        {
                            Role = "user",
                            Content = $"{job.JobSummary}, Skills: {job.Skills}"
                        }
                    ]
                };
                var response = await httpClient.PostAsJsonAsync(requestUrl, request);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    //TODO: Parse responseContent and update job.LlmSkills
                    //for now: just flush the responseContent to the log
                    Log.Warning(responseContent);
                }
            }

        }
        
        Log.Information($"Finished process with {entries} entries");
    }
}