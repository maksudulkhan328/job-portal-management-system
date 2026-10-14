using JobFlow.Application.DTOs.Profile;

namespace JobFlow.Application.Interfaces;

public interface IProfileService
{
    Task<CandidateProfileDto> GetProfileAsync(Guid userId);
    Task<CandidateProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task UpdateCvAsync(Guid userId, string fileName, string fileUrl);
    Task<CandidateProfileDto> AddEducationAsync(Guid userId, AddEducationRequest request);
    Task<CandidateProfileDto> UpdateEducationAsync(Guid userId, Guid educationId, AddEducationRequest request);
    Task<CandidateProfileDto> DeleteEducationAsync(Guid userId, Guid educationId);
    Task<CandidateProfileDto> AddWorkExperienceAsync(Guid userId, AddWorkExperienceRequest request);
    Task<CandidateProfileDto> UpdateWorkExperienceAsync(Guid userId, Guid expId, AddWorkExperienceRequest request);
    Task<CandidateProfileDto> DeleteWorkExperienceAsync(Guid userId, Guid expId);
    Task<CandidateProfileDto> UpdateSkillsAsync(Guid userId, List<Guid> skillIds);
}
