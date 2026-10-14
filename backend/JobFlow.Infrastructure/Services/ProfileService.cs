using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobFlow.Application.DTOs.Profile;
using JobFlow.Application.Interfaces;
using JobFlow.Domain.Entities;
using JobFlow.Infrastructure.Persistence;

namespace JobFlow.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<CandidateProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found.");

        var profile = await _context.CandidateProfiles
            .Include(p => p.Educations)
            .Include(p => p.WorkExperiences)
            .Include(p => p.Skills).ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? new CandidateProfile { UserId = userId };

        return MapToDto(user, profile);
    }

    public async Task<CandidateProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found.");

        var profile = await _context.CandidateProfiles
            .Include(p => p.Educations)
            .Include(p => p.WorkExperiences)
            .Include(p => p.Skills).ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            profile = new CandidateProfile { UserId = userId };
            _context.CandidateProfiles.Add(profile);
        }

        profile.Headline = request.Headline;
        profile.Summary = request.Summary;
        profile.PhoneNumber = request.PhoneNumber;
        profile.Location = request.Location;
        profile.City = request.City;
        profile.Country = request.Country;
        profile.LinkedInUrl = request.LinkedInUrl;
        profile.GitHubUrl = request.GitHubUrl;
        profile.PortfolioUrl = request.PortfolioUrl;
        profile.WebsiteUrl = request.WebsiteUrl;
        profile.IsProfilePublic = request.IsProfilePublic;
        profile.IsOpenToWork = request.IsOpenToWork;
        profile.ProfileCompletionPercent = CalculateCompletion(profile);

        await _context.SaveChangesAsync();
        return MapToDto(user, profile);
    }

    public async Task UpdateCvAsync(Guid userId, string fileName, string fileUrl)
    {
        var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            profile = new CandidateProfile { UserId = userId };
            _context.CandidateProfiles.Add(profile);
        }
        profile.CvFileName = fileName;
        profile.CvFileUrl = fileUrl;
        profile.CvUploadedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<CandidateProfileDto> AddEducationAsync(Guid userId, AddEducationRequest request)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        _context.Educations.Add(new Education
        {
            CandidateProfileId = profile.Id, Institution = request.Institution,
            Degree = request.Degree, FieldOfStudy = request.FieldOfStudy,
            StartDate = request.StartDate, EndDate = request.EndDate,
            IsCurrentlyStudying = request.IsCurrentlyStudying, Grade = request.Grade,
            Description = request.Description
        });
        await _context.SaveChangesAsync();
        return await GetProfileAsync(userId);
    }

    public async Task<CandidateProfileDto> UpdateEducationAsync(Guid userId, Guid educationId, AddEducationRequest request)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        var edu = await _context.Educations
            .FirstOrDefaultAsync(e => e.Id == educationId && e.CandidateProfileId == profile.Id)
            ?? throw new KeyNotFoundException("Education entry not found.");

        edu.Institution = request.Institution; edu.Degree = request.Degree;
        edu.FieldOfStudy = request.FieldOfStudy; edu.StartDate = request.StartDate;
        edu.EndDate = request.EndDate; edu.IsCurrentlyStudying = request.IsCurrentlyStudying;
        edu.Grade = request.Grade; edu.Description = request.Description;
        await _context.SaveChangesAsync();
        return await GetProfileAsync(userId);
    }

    public async Task<CandidateProfileDto> DeleteEducationAsync(Guid userId, Guid educationId)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        var edu = await _context.Educations
            .FirstOrDefaultAsync(e => e.Id == educationId && e.CandidateProfileId == profile.Id)
            ?? throw new KeyNotFoundException("Education entry not found.");
        _context.Educations.Remove(edu);
        await _context.SaveChangesAsync();
        return await GetProfileAsync(userId);
    }

    public async Task<CandidateProfileDto> AddWorkExperienceAsync(Guid userId, AddWorkExperienceRequest request)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        _context.WorkExperiences.Add(new WorkExperience
        {
            CandidateProfileId = profile.Id, CompanyName = request.CompanyName,
            JobTitle = request.JobTitle, Location = request.Location,
            StartDate = request.StartDate, EndDate = request.EndDate,
            IsCurrentJob = request.IsCurrentJob, Description = request.Description
        });
        await _context.SaveChangesAsync();
        return await GetProfileAsync(userId);
    }

    public async Task<CandidateProfileDto> UpdateWorkExperienceAsync(Guid userId, Guid expId, AddWorkExperienceRequest request)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        var exp = await _context.WorkExperiences
            .FirstOrDefaultAsync(e => e.Id == expId && e.CandidateProfileId == profile.Id)
            ?? throw new KeyNotFoundException("Work experience entry not found.");

        exp.CompanyName = request.CompanyName; exp.JobTitle = request.JobTitle;
        exp.Location = request.Location; exp.StartDate = request.StartDate;
        exp.EndDate = request.EndDate; exp.IsCurrentJob = request.IsCurrentJob;
        exp.Description = request.Description;
        await _context.SaveChangesAsync();
        return await GetProfileAsync(userId);
    }

    public async Task<CandidateProfileDto> DeleteWorkExperienceAsync(Guid userId, Guid expId)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        var exp = await _context.WorkExperiences
            .FirstOrDefaultAsync(e => e.Id == expId && e.CandidateProfileId == profile.Id)
            ?? throw new KeyNotFoundException("Work experience entry not found.");
        _context.WorkExperiences.Remove(exp);
        await _context.SaveChangesAsync();
        return await GetProfileAsync(userId);
    }

    public async Task<CandidateProfileDto> UpdateSkillsAsync(Guid userId, List<Guid> skillIds)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        var existing = await _context.CandidateSkills.Where(cs => cs.CandidateProfileId == profile.Id).ToListAsync();
        _context.CandidateSkills.RemoveRange(existing);

        var validSkills = await _context.Skills.Where(s => skillIds.Contains(s.Id)).Select(s => s.Id).ToListAsync();
        foreach (var sid in validSkills)
            _context.CandidateSkills.Add(new CandidateSkill { CandidateProfileId = profile.Id, SkillId = sid });

        await _context.SaveChangesAsync();
        return await GetProfileAsync(userId);
    }

    private async Task<CandidateProfile> GetOrCreateProfileAsync(Guid userId)
    {
        var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile != null) return profile;
        profile = new CandidateProfile { UserId = userId };
        _context.CandidateProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    private static int CalculateCompletion(CandidateProfile p)
    {
        int score = 0;
        if (!string.IsNullOrEmpty(p.Headline)) score += 10;
        if (!string.IsNullOrEmpty(p.Summary)) score += 15;
        if (!string.IsNullOrEmpty(p.PhoneNumber)) score += 5;
        if (!string.IsNullOrEmpty(p.Location)) score += 5;
        if (!string.IsNullOrEmpty(p.LinkedInUrl)) score += 10;
        if (!string.IsNullOrEmpty(p.CvFileUrl)) score += 20;
        if (p.Educations.Any()) score += 15;
        if (p.WorkExperiences.Any()) score += 15;
        if (p.Skills.Any()) score += 5;
        return Math.Min(score, 100);
    }

    private static CandidateProfileDto MapToDto(ApplicationUser user, CandidateProfile p) => new(
        p.Id, user.Id, user.FirstName, user.LastName, user.Email!, user.AvatarUrl,
        p.Headline, p.Summary, p.PhoneNumber, p.Location, p.City, p.Country,
        p.LinkedInUrl, p.GitHubUrl, p.PortfolioUrl, p.WebsiteUrl, p.CvFileUrl,
        p.IsProfilePublic, p.IsOpenToWork, p.ProfileCompletionPercent,
        p.Educations.OrderByDescending(e => e.StartDate).Select(e => new EducationDto(
            e.Id, e.Institution, e.Degree, e.FieldOfStudy, e.StartDate, e.EndDate,
            e.IsCurrentlyStudying, e.Grade, e.Description
        )).ToList(),
        p.WorkExperiences.OrderByDescending(e => e.StartDate).Select(e => new WorkExperienceDto(
            e.Id, e.CompanyName, e.JobTitle, e.Location, e.StartDate, e.EndDate,
            e.IsCurrentJob, e.Description
        )).ToList(),
        p.Skills.Select(cs => new SkillItemDto(cs.SkillId, cs.Skill.Name, cs.ProficiencyLevel)).ToList()
    );
}
