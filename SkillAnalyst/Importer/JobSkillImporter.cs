using System.Globalization;
using CsvHelper;
using SkillAnalyst.Models;

namespace SkillAnalyst.Importer;

public static class JobSkillImporter
{
    public static List<ImportedJobSkill> Import(string jobSkillsFilePath)
    {
        using var reader = new StreamReader(jobSkillsFilePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<ImportedJobSkill>().ToList();
    }
}