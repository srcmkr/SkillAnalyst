﻿using Serilog;
using SkillAnalyst.Importer;
using SkillAnalyst.LLM;
using SkillAnalyst.Merger;

const string jobSkillsFilePath = "../../../job_skills.csv";
const string jobSummaryFilePath = "../../../job_summary.csv";
const string databaseFilePath = "../../../merged_jobs.db";
const string requestUrl = "http://localhost:8090/v1/chat/completions";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Information()
    .CreateLogger();

Log.Information("""
                Welcome to SkillAnalyst
                Auto-Matcher for 1.3m LinkedIn Jobs & Skills
                https://www.kaggle.com/datasets/asaniczka/1-3m-linkedin-jobs-and-skills-2024
                16 GByte of RAM is required, more is better
                """);


Log.Information("Database file not found, starting import and merge process");
if (!File.Exists(jobSkillsFilePath) || !File.Exists(jobSummaryFilePath))
{
    Log.Error("Missing input files (job_skills.csv or job_summary.csv)");
    return;
}

Log.Information("Importing job skills");
var skills = JobSkillImporter.Import(jobSkillsFilePath);

Log.Information("Importing job descriptions");
var summaries = JobSummaryImporter.Import(jobSummaryFilePath);

Log.Information("Merging job skills and descriptions, writing to database");
var mergedSkills = JobSkillMerger.MergeAndSaveAsync(skills, summaries, databaseFilePath);

Log.Information("Merged dataset created, starting LLM process");
await LocalLlm.EnrichAndSaveAsync(mergedSkills, requestUrl, databaseFilePath);