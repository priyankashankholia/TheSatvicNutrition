using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nutritionist.Api.Models;

namespace Nutritionist.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<NutritionDbContext>();

        await db.Database.MigrateAsync();

        var passwordHasher = new PasswordHasher<User>();

        // Create the single nutritionist account.
        if (!await db.Users.AnyAsync(x => x.Role == UserRole.Nutritionist))
        {
            var nutritionist = new User
            {
                FirstName = "Satvic",
                LastName = "Nutritionist",
                Email = "nutritionist@satvicnutrition.com",
                Role = UserRole.Nutritionist,
                IsActive = true
            };

            nutritionist.PasswordHash =
                passwordHasher.HashPassword(
                    nutritionist,
                    "ChangeMe123!");

            db.Users.Add(nutritionist);

            db.NutritionistProfiles.Add(
                new NutritionistProfile
                {
                    UserId = nutritionist.Id,
                    Qualification = "Certified Nutritionist",
                    Specialization = "Weight Management & Lifestyle Nutrition",
                    ExperienceYears = 5,
                    Bio = "Personalized nutrition and lifestyle coaching.",
                    Email = nutritionist.Email,
                    IsAvailable = true
                });
        }

        // Create initial packages.
        if (!await db.Packages.AnyAsync())
        {
            db.Packages.AddRange(
                new Package
                {
                    Name = "Starter",
                    Description =
                        "A simple four-week nutrition program.",
                    Price = 2999,
                    DurationWeeks = 4,
                    AppointmentsPerWeek = 1,
                    IncludesDietPlan = true,
                    IncludesProgressTracking = true,
                    IncludesMessaging = true,
                    IncludesPhotoTracking = true
                },
                new Package
                {
                    Name = "Transformation",
                    Description =
                        "An eight-week guided nutrition transformation program.",
                    Price = 5499,
                    DurationWeeks = 8,
                    AppointmentsPerWeek = 1,
                    IncludesDietPlan = true,
                    IncludesProgressTracking = true,
                    IncludesMessaging = true,
                    IncludesPhotoTracking = true
                },
                new Package
                {
                    Name = "Premium",
                    Description =
                        "A twelve-week intensive nutrition coaching program.",
                    Price = 7999,
                    DurationWeeks = 12,
                    AppointmentsPerWeek = 1,
                    IncludesDietPlan = true,
                    IncludesProgressTracking = true,
                    IncludesMessaging = true,
                    IncludesPhotoTracking = true
                });
        }

        await db.SaveChangesAsync();
    }
}