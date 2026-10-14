using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobFlow.Domain.Entities;

namespace JobFlow.Infrastructure.Persistence.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Title).HasMaxLength(200).IsRequired();
        builder.Property(j => j.Description).IsRequired();
        builder.Property(j => j.Location).HasMaxLength(200);
        builder.Property(j => j.City).HasMaxLength(100);
        builder.Property(j => j.Country).HasMaxLength(100);
        builder.Property(j => j.SalaryMin).HasColumnType("decimal(18,2)");
        builder.Property(j => j.SalaryMax).HasColumnType("decimal(18,2)");
        builder.Property(j => j.SalaryCurrency).HasMaxLength(10);

        builder.HasIndex(j => j.Title);
        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => j.PublishedAt);
        builder.HasIndex(j => j.CompanyId);
        builder.HasIndex(j => j.CategoryId);

        builder.HasOne(j => j.Company)
            .WithMany(c => c.Jobs)
            .HasForeignKey(j => j.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.Category)
            .WithMany(c => c.Jobs)
            .HasForeignKey(j => j.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.PostedBy)
            .WithMany()
            .HasForeignKey(j => j.PostedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
