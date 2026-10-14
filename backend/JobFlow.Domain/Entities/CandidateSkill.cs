namespace JobFlow.Domain.Entities;

public class CandidateSkill
{
    public Guid CandidateProfileId { get; set; }
    public Guid SkillId { get; set; }
    public int? ProficiencyLevel { get; set; }

    public CandidateProfile CandidateProfile { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
