using ECT.ACC.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ECT.ACC.Data.Context;

public class ECTDbContext : DbContext
{
    public ECTDbContext(DbContextOptions<ECTDbContext> options) : base(options) { }

    public DbSet<Scenario> Scenarios => Set<Scenario>();
    public DbSet<ScenarioParameters> ScenarioParameters => Set<ScenarioParameters>();
    public DbSet<DeficitAnalysis> DeficitAnalyses => Set<DeficitAnalysis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Scenario
        modelBuilder.Entity<Scenario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
        });

        // ScenarioParameters — one-to-one with Scenario
        modelBuilder.Entity<ScenarioParameters>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Scenario)
                  .WithOne(s => s.Parameters)
                  .HasForeignKey<ScenarioParameters>(e => e.ScenarioId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Owned ScientificValue configurations
            entity.OwnsOne(e => e.Energy, sv =>
            {
                sv.Property(p => p.Coefficient).HasColumnName("EnergyCoefficient");
                sv.Property(p => p.Exponent).HasColumnName("EnergyExponent");
            });
            entity.OwnsOne(e => e.Control, sv =>
            {
                sv.Property(p => p.Coefficient).HasColumnName("ControlCoefficient");
                sv.Property(p => p.Exponent).HasColumnName("ControlExponent");
            });
            entity.OwnsOne(e => e.Complexity, sv =>
            {
                sv.Property(p => p.Coefficient).HasColumnName("ComplexityCoefficient");
                sv.Property(p => p.Exponent).HasColumnName("ComplexityExponent");
            });
            entity.OwnsOne(e => e.TimeAvailable, sv =>
            {
                sv.Property(p => p.Coefficient).HasColumnName("TimeAvailableCoefficient");
                sv.Property(p => p.Exponent).HasColumnName("TimeAvailableExponent");
            });
        });

        // DeficitAnalysis — one-to-one with Scenario
        modelBuilder.Entity<DeficitAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Scenario)
                  .WithOne(s => s.DeficitAnalysis)
                  .HasForeignKey<DeficitAnalysis>(e => e.ScenarioId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.OwnsOne(e => e.CRequired, sv =>
            {
                sv.Property(p => p.Coefficient).HasColumnName("CRequiredCoefficient");
                sv.Property(p => p.Exponent).HasColumnName("CRequiredExponent");
            });
            entity.OwnsOne(e => e.CAvailable, sv =>
            {
                sv.Property(p => p.Coefficient).HasColumnName("CAvailableCoefficient");
                sv.Property(p => p.Exponent).HasColumnName("CAvailableExponent");
            });
            entity.OwnsOne(e => e.CDeficit, sv =>
            {
                sv.Property(p => p.Coefficient).HasColumnName("CDeficitCoefficient");
                sv.Property(p => p.Exponent).HasColumnName("CDeficitExponent");
            });

            entity.Property(e => e.DeficitType).IsRequired().HasMaxLength(10);
            entity.Property(e => e.ClassificationNotes).HasMaxLength(2000);
        });
    }
}
