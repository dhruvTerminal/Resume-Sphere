using System.ComponentModel.DataAnnotations;

namespace ResumeAPI.Models;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(256)]
    public string FullName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<JobDescription> JobDescriptions { get; set; } = new List<JobDescription>();
    public ICollection<Analysis> Analyses { get; set; } = new List<Analysis>();
    public ICollection<UserCourseSelection> CourseSelections { get; set; } = new List<UserCourseSelection>();
    public ICollection<UserCourseProgress> CourseProgresses { get; set; } = new List<UserCourseProgress>();
    public ICollection<GeneratedResume> GeneratedResumes { get; set; } = new List<GeneratedResume>();
    public ICollection<AnalysisHistory> AnalysisHistories { get; set; } = new List<AnalysisHistory>();
}
