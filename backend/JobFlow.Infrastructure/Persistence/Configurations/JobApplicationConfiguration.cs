using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobFlow.Domain.Entities;

namespace JobFlow.Infrastructure.Persistence.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => new { a.JobId, a.ApplicantId }).IsUnique();
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.ApplicantId);

        builder.HasOne(a => a.Job)
            .WithMany(j => j.Applications)
            .HasForeignKey(a => a.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Applicant)
            .WithMany()
            .HasForeignKey(a => a.ApplicantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.CoverLetter).HasMaxLength(5000);
        builder.Property(a => a.RecruiterNotes).HasMaxLength(2000);
    }
}
