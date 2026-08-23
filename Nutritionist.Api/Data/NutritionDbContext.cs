using Microsoft.EntityFrameworkCore;
using Nutritionist.Api.Models;

namespace Nutritionist.Api.Data;

public class NutritionDbContext : DbContext
{
    public NutritionDbContext(DbContextOptions<NutritionDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>();
    public DbSet<NutritionistProfile> NutritionistProfiles => Set<NutritionistProfile>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<DietPlan> DietPlans => Set<DietPlan>();
    public DbSet<ProgressEntry> ProgressEntries => Set<ProgressEntry>();
    public DbSet<ProgressPhoto> ProgressPhotos => Set<ProgressPhoto>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<ClientProfile>()
            .HasOne(x => x.User)
            .WithOne(x => x.ClientProfile)
            .HasForeignKey<ClientProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NutritionistProfile>()
            .HasOne(x => x.User)
            .WithOne(x => x.NutritionistProfile)
            .HasForeignKey<NutritionistProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Purchase>()
            .HasOne(x => x.ClientProfile)
            .WithMany(x => x.Purchases)
            .HasForeignKey(x => x.ClientProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Purchase>()
            .HasOne(x => x.Package)
            .WithMany(x => x.Purchases)
            .HasForeignKey(x => x.PackageId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(x => x.ClientProfile)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.ClientProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(x => x.Purchase)
            .WithMany()
            .HasForeignKey(x => x.PurchaseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Assessment>()
            .HasOne(x => x.ClientProfile)
            .WithMany(x => x.Assessments)
            .HasForeignKey(x => x.ClientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DietPlan>()
            .HasOne(x => x.ClientProfile)
            .WithMany()
            .HasForeignKey(x => x.ClientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DietPlan>()
            .HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProgressEntry>()
            .HasOne(x => x.ClientProfile)
            .WithMany(x => x.ProgressEntries)
            .HasForeignKey(x => x.ClientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProgressPhoto>()
            .HasOne(x => x.ClientProfile)
            .WithMany(x => x.ProgressPhotos)
            .HasForeignKey(x => x.ClientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(x => x.ClientProfile)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.ClientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(x => x.SenderUser)
            .WithMany()
            .HasForeignKey(x => x.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Package>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);
    }
}