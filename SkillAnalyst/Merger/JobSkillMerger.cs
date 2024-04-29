using LiteDB;
using MoreLinq;
using Serilog;
using SkillAnalyst.Models;

namespace SkillAnalyst.Merger;

public static class JobSkillMerger
{
    public static void MergeAndSaveAsync(List<ImportedJobSkill> skills, List<ImportedJobSummary> summaries, string databaseFilePath)
    {
        using var db = new LiteDatabase(databaseFilePath);
        var collection = db.GetCollection<MergedJobSkills>("jobs");
        var mergedJobs = new List<MergedJobSkills>();
        var entries = 0;
        var jobSummariesByLink = summaries.ToDictionary(s => s.JobLink, s => s.JobSummary);

        foreach (var skill in skills)
        {
            entries++;
            if (entries % 100000 == 0) Log.Information($"Processed {entries} entries");
            if (jobSummariesByLink.TryGetValue(skill.JobLink, out string jobSummary))
            {
                mergedJobs.Add(new MergedJobSkills { JobLink = skill.JobLink, JobSummary = jobSummary, Skills = skill.Skills });
            }
        }

        Log.Information($"Finished process with {entries} entries");
        mergedJobs = mergedJobs.DistinctBy(j => j.JobLink).ToList();

        const int batchSize = 1000;
        var batches = mergedJobs.Batch(batchSize);
        var savedJobs = 0;

        foreach (var batch in batches)
        {
            savedJobs += batch.Length;
            Log.Information($"Saved {savedJobs} jobs to database");
            collection.InsertBulk(batch);
        }
    }
}