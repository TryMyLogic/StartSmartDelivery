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

        // TODO - Constraints and Indexes

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<DeliveryTask>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne<Driver>()
                  .WithMany()
                  .HasForeignKey(e => e.DriverId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Vehicle>()
                  .WithMany()
                  .HasForeignKey(e => e.VehicleId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<DeliveryTask>()
                  .WithMany()
                  .HasForeignKey(e => e.TaskId)
                  .OnDelete(DeleteBehavior.Cascade);
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
