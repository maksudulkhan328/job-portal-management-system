namespace JobFlow.Domain.Enums;

public enum UserRole
{
    Admin = 1,
    Employer = 2,
    JobSeeker = 3
}

public enum JobStatus
{
    Draft = 1,
    PendingApproval = 2,
    Published = 3,
    Closed = 4,
    Archived = 5,
    Rejected = 6
}

public enum ApplicationStatus
{
    Submitted = 1,
    UnderReview = 2,
    Shortlisted = 3,
    InterviewScheduled = 4,
    Offered = 5,
    Hired = 6,
    Rejected = 7,
    Withdrawn = 8
}

public enum WorkplaceType
{
    OnSite = 1,
    Remote = 2,
    Hybrid = 3
}

public enum EmploymentType
{
    FullTime = 1,
    PartTime = 2,
    Contract = 3,
    Internship = 4,
    Freelance = 5,
    Temporary = 6
}

public enum ExperienceLevel
{
    EntryLevel = 1,
    Junior = 2,
    MidLevel = 3,
    Senior = 4,
    Lead = 5,
    Manager = 6,
    Director = 7,
    Executive = 8
}

public enum InterviewType
{
    Phone = 1,
    Video = 2,
    OnSite = 3,
    Technical = 4,
    HR = 5
}

public enum InterviewStatus
{
    Scheduled = 1,
    Completed = 2,
    Cancelled = 3,
    Rescheduled = 4
}

public enum NotificationType
{
    ApplicationReceived = 1,
    ApplicationStatusChanged = 2,
    InterviewScheduled = 3,
    JobApproved = 4,
    JobRejected = 5,
    NewJobPosted = 6,
    ProfileViewed = 7,
    General = 8
}
