namespace Infrastructure.Persistence;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class DbSeeder
{
    public static async Task MigrateAndSeedAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync(cancellationToken);

        logger.LogInformation("Checking and seeding initial data...");

        // Idempotent check para kay John Doe
        if (!await db.Students.AnyAsync(s => s.Email == "john.doe@example.com", cancellationToken))
        {
            db.Students.Add(new Student 
            { 
                Id = Guid.NewGuid(), 
                FirstName = "John", 
                LastName = "Doe", 
                Email = "john.doe@example.com", 
                DateOfBirth = new DateTime(2000, 1, 15) 
            });
        }

        // Idempotent check para kay Jane Smith
        if (!await db.Students.AnyAsync(s => s.Email == "jane.smith@example.com", cancellationToken))
        {
            db.Students.Add(new Student 
            { 
                Id = Guid.NewGuid(), 
                FirstName = "Jane", 
                LastName = "Smith", 
                Email = "jane.smith@example.com", 
                DateOfBirth = new DateTime(2001, 5, 20) 
            });
        }

        // Idempotent check para sa Courses gamit ang Course Code
        if (!await db.Courses.AnyAsync(c => c.Code == "CS101", cancellationToken))
        {
            db.Courses.AddRange(
                new Course { Id = Guid.NewGuid(), Title = "Introduction to Programming", Code = "CS101", Credits = 3 },
                new Course { Id = Guid.NewGuid(), Title = "Data Structures", Code = "CS201", Credits = 4 },
                new Course { Id = Guid.NewGuid(), Title = "Database Systems", Code = "CS301", Credits = 3 }
            );
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeding check complete.");
    }
}