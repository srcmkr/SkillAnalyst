using System.Net.Http.Json;
using LiteDB;
using Newtonsoft.Json;
using Serilog;
using SkillAnalyst.Models;

namespace SkillAnalyst.LLM;

public static class LocalLlm
{
    public static async Task EnrichAndSaveAsync(List<MergedJobSkills> mergedSkills, string requestUrl, string databaseFilePath)
    {
        using var db = new LiteDatabase(databaseFilePath);
        var collection = db.GetCollection<MergedJobSkills>("jobs");
        var skillSet = new HashSet<string>();
        
        var entries = 0;

        var preprompt =
            """
            Extract primary skills from job descriptions, merging similar ones. For example, 'Python' and 'Python 3' become 'Python'. Similarly, '.NET/C#' and '.NET Core 3.1' merge into '.NET'. 'Project Management' and 'Project Manager' merge to 'Project Management'.
            Use the more general skill. Only include skills from the job description; do not add extra ones or add 'skill' to a skill. For example, 'Communication Skills' should be 'Communication'.
            Output format: Comma-separated list of skills per job, no additional information.
            Example output: "Python, SQL, Project Management, .NET, PHP, Ruby, Documentation, Accounting Software, System Testing".
            Here's the job description and the skills discovered by an intern:
            """;

        using var httpClient = new HttpClient();
        
        foreach (var job in mergedSkills)
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
                            Role = "system",
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
                    var responseJson = JsonConvert.DeserializeObject<LmStudioResponse>(responseContent);

                    if (responseJson != null)
                    {
                        var firstSkill = responseJson.Choices.FirstOrDefault();
                        if (firstSkill?.Message == null)
                        {
                            Log.Fatal($"No response from LLM for {responseContent}");
                        } else
                        {
                            var skills = firstSkill.Message.Content;
                            var skillList = skills.Split(',').Select(s => s.Trim()).ToList();
                            
                            if (skillList.Count != 0)
                            {
                                foreach (var skill in skillList) skillSet.Add(skill);
                                // write skill to file, append if file exists, one skill per line
                                await File.AppendAllLinesAsync("enriched_skills_wip.txt", skillList);
                            }
                            
                            Log.Information("Unique skills so far: " + skillSet.Count);
                        }
                    }
                    else
                    {
                        Log.Fatal($"Failed to deserialize response from LLM: {responseContent}");
                    }
                }
            }
        }
        Log.Information($"Finished process with {entries} entries. Saving enriched skills to file.");
        
        // Save the enriched skills back to a file, one skill per line
        var enrichedSkills = skillSet.ToList();
        enrichedSkills.Sort();
        await File.WriteAllLinesAsync("enriched_skills_full.txt", enrichedSkills);
        
        Log.Information("Enriched skills saved to file");
    }
}