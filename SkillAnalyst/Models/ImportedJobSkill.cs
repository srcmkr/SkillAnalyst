using CsvHelper.Configuration.Attributes;

namespace SkillAnalyst.Models;

public class ImportedJobSkill
{
    [Index(0), Name("job_link")]
    public string JobLink { get; set; }
    
    [Index(1), Name("job_skills")]
    public string Skills { get; set; }
}