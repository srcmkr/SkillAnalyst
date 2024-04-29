using System.Globalization;
using CsvHelper;
using SkillAnalyst.Models;

namespace SkillAnalyst.Importer;

public static class JobSummaryImporter
{
    public static List<ImportedJobSummary> Import(string jobSummaryFilePath)
    {
        using var reader = new StreamReader(jobSummaryFilePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<ImportedJobSummary>().ToList();
    }
}