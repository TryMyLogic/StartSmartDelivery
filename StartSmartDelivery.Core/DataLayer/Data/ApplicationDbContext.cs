using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using StartSmartDelivery.Core.DataLayer.Models;

namespace StartSmartDelivery.Core.DataLayer.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options) { }

    public DbSet<User> User { get; set; }
    public DbSet<Driver> Driver { get; set; }
    public DbSet<Vehicle> Vehicle { get; set; }
    public DbSet<DeliveryTask> DeliveryTask { get; set; }
    public DbSet<Delivery> Delivery { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Username)
                  .IsRequired()
                  .HasMaxLength(50);
            entity.HasIndex(e => e.Username)
                  .IsUnique();

            entity.Property(e => e.Password)
                  .IsRequired()
                  .HasMaxLength(100); // ASP.NET Identity hashing will fall within this range

            entity.Property(e => e.UserPermissions)
                  .HasConversion<int>()
                  .IsRequired();
        });

        modelBuilder.Entity<DeliveryTask>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OrderNumber)
                  .IsRequired()
                  .HasMaxLength(50);
            entity.HasIndex(e => e.OrderNumber)
                  .IsUnique();

            entity.Property(e => e.CustomerCode)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Telephone)
                  .HasMaxLength(15);

            entity.Property(e => e.Cellphone)
                  .IsRequired()
                  .HasMaxLength(15);

            entity.Property(e => e.Email)
                  .HasMaxLength(100);

            entity.Property(e => e.Address)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.Product)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Amount)
                  .HasPrecision(10, 2);

            entity.Property(e => e.PaymentMethod)
                  .HasConversion<int>()
                  .IsRequired();

            entity.Property(e => e.Notes)
                  .HasMaxLength(2000); // May lower Length.

            entity.Property(e => e.ReceivedTimestamp)
                  .HasDefaultValueSql(null); // Service layer sets the default value using DateTime.UtcNow
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.Surname)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.EmployeeNo)
                  .IsRequired()
                  .HasMaxLength(10);
            entity.HasIndex(e => e.EmployeeNo)
                  .IsUnique();

            entity.Property(e => e.LicenseType)
                  .HasConversion<int>()
                  .IsRequired();
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Make)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.Model)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.Year)
                  .IsRequired();

            entity.Property(e => e.NumberPlate)
                  .IsRequired()
                  .HasMaxLength(10); 
            entity.HasIndex(e => e.NumberPlate)
                  .IsUnique();

            entity.Property(e => e.Availability)
                  .IsRequired();
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne<Driver>()
                  .WithMany()
                  .HasForeignKey(e => e.DriverId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            entity.HasOne<Vehicle>()
                  .WithMany()
                  .HasForeignKey(e => e.VehicleId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            entity.HasOne<DeliveryTask>()
                  .WithMany()
                  .HasForeignKey(e => e.TaskId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired();
        });
    }
}

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder optionsBuilder = new();
        // TODO - Move the connectionString to appropriate spot for prod
        string connectionString = "Server=FUDGEDESKTOP\\SQLEXPRESS;Database=StartSmartTempTest;Trusted_Connection=true;TrustServerCertificate=true";
        optionsBuilder.UseSqlServer(connectionString);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
