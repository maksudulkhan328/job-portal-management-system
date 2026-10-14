using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JobFlow.Domain.Entities;
using JobFlow.Domain.Enums;

namespace JobFlow.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, ILogger logger)
    {
        try
        {
            await context.Database.MigrateAsync();
            await SeedRolesAsync(roleManager);
            await SeedCategoriesAsync(context);
            await SeedSkillsAsync(context);
            await SeedAdminUserAsync(userManager, context);
            await SeedDemoUsersAsync(userManager, context);
            logger.LogInformation("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while seeding the database.");
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var role in Enum.GetNames<UserRole>())
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role, NormalizedName = role.ToUpper() });
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.JobCategories.AnyAsync()) return;

        var categories = new[]
        {
            ("Software Engineering", "software-engineering", "💻"),
            ("Data Science & AI", "data-science-ai", "🤖"),
            ("Design & UX", "design-ux", "🎨"),
            ("Marketing", "marketing", "📣"),
            ("Sales", "sales", "💼"),
            ("Finance", "finance", "💰"),
            ("Human Resources", "human-resources", "👥"),
            ("Customer Support", "customer-support", "🎧"),
            ("Operations", "operations", "⚙️"),
            ("Legal", "legal", "⚖️"),
        };

        foreach (var (name, slug, icon) in categories)
            context.JobCategories.Add(new JobCategory { Name = name, Slug = slug, IconName = icon });

        await context.SaveChangesAsync();
    }

    private static async Task SeedSkillsAsync(ApplicationDbContext context)
    {
        if (await context.Skills.AnyAsync()) return;

        var skills = new[] {
            "C#", ".NET", "ASP.NET Core", "JavaScript", "TypeScript", "React", "Angular", "Vue.js",
            "Node.js", "Python", "Java", "Go", "SQL", "PostgreSQL", "MongoDB", "Redis",
            "Docker", "Kubernetes", "AWS", "Azure", "Git", "Agile", "Scrum",
            "HTML/CSS", "REST APIs", "GraphQL", "TailwindCSS", "Machine Learning"
        };

        foreach (var name in skills)
            context.Skills.Add(new Skill { Name = name, Slug = name.ToLower().Replace("/", "-").Replace(".", "").Replace(" ", "-") });

        await context.SaveChangesAsync();
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        const string adminEmail = "admin@jobflow.dev";
        if (await userManager.FindByEmailAsync(adminEmail) != null) return;

        var admin = new ApplicationUser
        {
            FirstName = "System",
            LastName = "Admin",
            Email = adminEmail,
            UserName = adminEmail,
            Role = UserRole.Admin,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(admin, "Admin@123456");
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    private static async Task SeedDemoUsersAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        const string employerEmail = "employer@demo.com";
        if (await userManager.FindByEmailAsync(employerEmail) == null)
        {
            var employer = new ApplicationUser
            {
                FirstName = "Demo",
                LastName = "Employer",
                Email = employerEmail,
                UserName = employerEmail,
                Role = UserRole.Employer,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(employer, "Demo@123456");
            await userManager.AddToRoleAsync(employer, "Employer");

            var company = new Company
            {
                Name = "TechCorp Solutions",
                Description = "A leading technology company specializing in enterprise software.",
                Website = "https://techcorp.example.com",
                Industry = "Technology",
                Size = "51-200",
                Location = "New York, USA",
                City = "New York",
                Country = "USA",
                OwnerId = employer.Id,
                IsVerified = true
            };
            context.Companies.Add(company);

            var category = await context.JobCategories.FirstOrDefaultAsync(c => c.Slug == "software-engineering");
            if (category != null)
            {
                var job = new Job
                {
                    Title = "Senior .NET Developer",
                    Description = "We are looking for an experienced .NET developer to join our team.",
                    Requirements = "5+ years of experience with .NET, strong SQL skills",
                    Location = "New York, USA",
                    City = "New York",
                    Country = "USA",
                    WorkplaceType = WorkplaceType.Hybrid,
                    EmploymentType = EmploymentType.FullTime,
                    ExperienceLevel = ExperienceLevel.Senior,
                    SalaryMin = 100000,
                    SalaryMax = 140000,
                    SalaryCurrency = "USD",
                    ShowSalary = true,
                    Status = JobStatus.Published,
                    PublishedAt = DateTime.UtcNow,
                    CompanyId = company.Id,
                    CategoryId = category.Id,
                    PostedById = employer.Id,
                    VacancyCount = 2,
                    ApplicationDeadline = DateTime.UtcNow.AddDays(30)
                };
                context.Jobs.Add(job);
            }
            await context.SaveChangesAsync();
        }

        const string seekerEmail = "seeker@demo.com";
        if (await userManager.FindByEmailAsync(seekerEmail) == null)
        {
            var seeker = new ApplicationUser
            {
                FirstName = "Demo",
                LastName = "JobSeeker",
                Email = seekerEmail,
                UserName = seekerEmail,
                Role = UserRole.JobSeeker,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(seeker, "Demo@123456");
            await userManager.AddToRoleAsync(seeker, "JobSeeker");

            context.CandidateProfiles.Add(new CandidateProfile
            {
                UserId = seeker.Id,
                Headline = "Full Stack Developer",
                Summary = "Passionate developer with 3 years experience",
                Location = "New York, USA",
                IsOpenToWork = true
            });
            await context.SaveChangesAsync();
        }
    }
}
