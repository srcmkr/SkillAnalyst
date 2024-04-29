using CsvHelper.Configuration.Attributes;

namespace SkillAnalyst.Models;

public class ImportedJobSummary
{
    [Index(0), Name("job_link")]
    public string JobLink { get; set; }
    
    [Index(1), Name("job_summary")]
    public string JobSummary { get; set; }
}