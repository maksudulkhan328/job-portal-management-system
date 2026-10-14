using Microsoft.EntityFrameworkCore;
using JobFlow.Application.Common;
using JobFlow.Application.DTOs.Applications;
using JobFlow.Application.Interfaces;
using JobFlow.Domain.Entities;
using JobFlow.Domain.Enums;
using JobFlow.Infrastructure.Persistence;

namespace JobFlow.Infrastructure.Services;

public class ApplicationService : IApplicationService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    private static readonly Dictionary<ApplicationStatus, ApplicationStatus[]> AllowedTransitions = new()
    {
        [ApplicationStatus.Submitted] = [ApplicationStatus.UnderReview, ApplicationStatus.Rejected],
        [ApplicationStatus.UnderReview] = [ApplicationStatus.Shortlisted, ApplicationStatus.Rejected],
        [ApplicationStatus.Shortlisted] = [ApplicationStatus.InterviewScheduled, ApplicationStatus.Rejected],
        [ApplicationStatus.InterviewScheduled] = [ApplicationStatus.Offered, ApplicationStatus.Rejected],
        [ApplicationStatus.Offered] = [ApplicationStatus.Hired, ApplicationStatus.Rejected],
        [ApplicationStatus.Hired] = [],
        [ApplicationStatus.Rejected] = [],
        [ApplicationStatus.Withdrawn] = [],
    };

    public ApplicationService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<ApplicationDto> ApplyAsync(Guid jobId, Guid applicantId, ApplyForJobRequest request, string? cvFileName, string? cvFileUrl)
    {
        var job = await _context.Jobs
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == jobId && j.Status == JobStatus.Published)
            ?? throw new KeyNotFoundException("Job not found or not accepting applications.");

        var alreadyApplied = await _context.JobApplications
            .AnyAsync(a => a.JobId == jobId && a.ApplicantId == applicantId);

        if (alreadyApplied)
            throw new InvalidOperationException("You have already applied for this job.");

        if (job.ApplicationDeadline.HasValue && job.ApplicationDeadline < DateTime.UtcNow)
            throw new InvalidOperationException("Application deadline has passed.");

        var application = new JobApplication
        {
            JobId = jobId,
            ApplicantId = applicantId,
            CoverLetter = request.CoverLetter,
            CvFileName = cvFileName,
            CvFileUrl = cvFileUrl,
            Status = ApplicationStatus.Submitted
        };

        _context.JobApplications.Add(application);
        await _context.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(
            job.PostedById, "New Application Received",
            $"A new application was received for '{job.Title}'",
            NotificationType.ApplicationReceived,
            $"/employer/applications/{application.Id}",
            application.Id
        );

        return await MapToDto(application);
    }

    public async Task<PagedResult<ApplicationDto>> GetMyApplicationsAsync(Guid applicantId, int page, int pageSize)
    {
        var q = _context.JobApplications
            .Include(a => a.Job).ThenInclude(j => j.Company)
            .Where(a => a.ApplicantId == applicantId)
            .OrderByDescending(a => a.CreatedAt);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ApplicationDto>
        {
            Items = (await Task.WhenAll(items.Select(a => MapToDto(a)))).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<PagedResult<ApplicationDetailDto>> GetJobApplicationsAsync(Guid jobId, Guid employerId, int page, int pageSize)
    {
        var job = await _context.Jobs
            .FirstOrDefaultAsync(j => j.Id == jobId && j.PostedById == employerId)
            ?? throw new UnauthorizedAccessException("Job not found or access denied.");

        var q = _context.JobApplications
            .Include(a => a.Applicant).ThenInclude(u => u.CandidateProfile)
            .Include(a => a.Interviews)
            .Where(a => a.JobId == jobId)
            .OrderByDescending(a => a.CreatedAt);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ApplicationDetailDto>
        {
            Items = items.Select(MapToDetailDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<ApplicationDetailDto> GetApplicationByIdAsync(Guid id, Guid userId)
    {
        var application = await _context.JobApplications
            .Include(a => a.Job).ThenInclude(j => j.Company)
            .Include(a => a.Applicant).ThenInclude(u => u.CandidateProfile)
            .Include(a => a.Interviews)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException("Application not found.");

        if (application.ApplicantId != userId && application.Job.PostedById != userId)
            throw new UnauthorizedAccessException("Access denied.");

        return MapToDetailDto(application);
    }

    public async Task<ApplicationDetailDto> UpdateStatusAsync(Guid id, ApplicationStatus newStatus, string? notes, Guid employerId)
    {
        var application = await _context.JobApplications
            .Include(a => a.Job)
            .Include(a => a.Applicant)
            .Include(a => a.Interviews)
            .FirstOrDefaultAsync(a => a.Id == id && a.Job.PostedById == employerId)
            ?? throw new UnauthorizedAccessException("Application not found or access denied.");

        if (!AllowedTransitions.TryGetValue(application.Status, out var allowed) || !allowed.Contains(newStatus))
            throw new InvalidOperationException($"Cannot transition from '{application.Status}' to '{newStatus}'.");

        application.Status = newStatus;
        application.StatusChangedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(notes))
            application.RecruiterNotes = notes;

        await _context.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(
            application.ApplicantId, "Application Status Updated",
            $"Your application for '{application.Job.Title}' has been updated to '{newStatus}'",
            NotificationType.ApplicationStatusChanged,
            $"/seeker/applications/{application.Id}",
            application.Id
        );

        return MapToDetailDto(application);
    }

    public async Task WithdrawApplicationAsync(Guid id, Guid applicantId, string? reason)
    {
        var application = await _context.JobApplications
            .FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == applicantId)
            ?? throw new UnauthorizedAccessException("Application not found or access denied.");

        if (application.Status is ApplicationStatus.Hired or ApplicationStatus.Withdrawn)
            throw new InvalidOperationException("Cannot withdraw this application.");

        application.Status = ApplicationStatus.Withdrawn;
        application.IsWithdrawn = true;
        application.WithdrawnAt = DateTime.UtcNow;
        application.WithdrawalReason = reason;
        await _context.SaveChangesAsync();
    }

    public async Task<ApplicationDetailDto> AddRecruiterNotesAsync(Guid id, string notes, Guid employerId)
    {
        var application = await _context.JobApplications
            .Include(a => a.Job)
            .Include(a => a.Applicant).ThenInclude(u => u.CandidateProfile)
            .Include(a => a.Interviews)
            .FirstOrDefaultAsync(a => a.Id == id && a.Job.PostedById == employerId)
            ?? throw new UnauthorizedAccessException("Application not found or access denied.");

        application.RecruiterNotes = notes;
        await _context.SaveChangesAsync();
        return MapToDetailDto(application);
    }

    private static Task<ApplicationDto> MapToDto(JobApplication a)
    {
        return Task.FromResult(new ApplicationDto(
            a.Id, a.JobId, a.Job.Title, a.Job.Company.Name, a.Job.Company.LogoUrl,
            $"{a.Applicant?.FirstName} {a.Applicant?.LastName}".Trim(),
            a.Applicant?.Email ?? "",
            a.Status, a.CoverLetter, a.CvFileUrl, a.RecruiterNotes,
            a.CreatedAt, a.StatusChangedAt
        ));
    }

    private static ApplicationDetailDto MapToDetailDto(JobApplication a) => new(
        a.Id, a.JobId, a.Job.Title, a.Job.Company?.Name ?? "",
        a.Job.Company?.LogoUrl,
        new CandidateSummaryDto(
            a.ApplicantId,
            a.Applicant?.FullName ?? "",
            a.Applicant?.Email ?? "",
            a.Applicant?.CandidateProfile?.Headline,
            a.Applicant?.CandidateProfile?.Location,
            a.Applicant?.AvatarUrl,
            a.Applicant?.CandidateProfile?.LinkedInUrl,
            a.Applicant?.CandidateProfile?.GitHubUrl,
            a.Applicant?.CandidateProfile?.PortfolioUrl
        ),
        a.Status, a.CoverLetter, a.CvFileUrl, a.RecruiterNotes,
        a.CreatedAt, a.StatusChangedAt,
        a.Interviews.Select(i => new InterviewDto(
            i.Id, i.Type, i.Status, i.ScheduledAt, i.DurationMinutes,
            i.Location, i.MeetingLink, i.Notes, i.InterviewerName
        )).ToList()
    );
}
