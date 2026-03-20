using ECT.ACC.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ECT.ACC.Data.Context;

public partial class ECTDbContext : DbContext
{
    public ECTDbContext(DbContextOptions<ECTDbContext> options) : base(options) { }

    // ── Existing ──────────────────────────────────────────────────────────────
    public DbSet<Scenario> Scenarios => Set<Scenario>();
    public DbSet<ScenarioParameters> ScenarioParameters => Set<ScenarioParameters>();
    public DbSet<DeficitAnalysis> DeficitAnalyses => Set<DeficitAnalysis>();

    // ── Phase 3 ───────────────────────────────────────────────────────────────
    public DbSet<ParameterDocumentation> ParameterDocumentations => Set<ParameterDocumentation>();
    public DbSet<SubParameter> SubParameters => Set<SubParameter>();
    public DbSet<ParameterVariant> ParameterVariants => Set<ParameterVariant>();
    public DbSet<VariantSubParameter> VariantSubParameters => Set<VariantSubParameter>();

    // ── Phase 3.5 ─────────────────────────────────────────────────────────────
    public DbSet<ProcessDomain> ProcessDomains => Set<ProcessDomain>();
    public DbSet<ParameterTemplate> ParameterTemplates => Set<ParameterTemplate>();
    public DbSet<TemplateParameterDefinition> TemplateParameterDefinitions => Set<TemplateParameterDefinition>();
    public DbSet<ParameterDefinition> ParameterDefinitions => Set<ParameterDefinition>();

    // ── Phase 5a ─────────────────────────────────────────────────────────────
    public DbSet<ScenarioConfiguration> ScenarioConfigurations { get; set; }
    public DbSet<ScenarioConfigurationEntry> ScenarioConfigurationEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Scenario ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Scenario>(static entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.Property(e => e.SolveForMode)
                  .IsRequired()
                  .HasMaxLength(20)
                  .HasDefaultValue("C");

            entity.Property(e => e.ScenarioMode)
                  .IsRequired()
                  .HasMaxLength(20)
                  .HasDefaultValue("Flat");

            // Phase 3.5 — optional domain FK
            entity.HasOne<ProcessDomain>(e => e.ProcessDomain)
                  .WithMany(static d => d.Scenarios)
                  .HasForeignKey(nameof(Scenario.ProcessDomainId))
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── ScenarioParameters — one-to-one with Scenario ────────────────────
        modelBuilder.Entity<ScenarioParameters>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Scenario)
                  .WithOne(s => s.Parameters)
                  .HasForeignKey<ScenarioParameters>(e => e.ScenarioId)
                  .OnDelete(DeleteBehavior.Cascade);

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

        // ── DeficitAnalysis — one-to-many with Scenario (one per configuration) ─
        modelBuilder.Entity<DeficitAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Scenario)
                  .WithMany(s => s.DeficitAnalyses)
                  .HasForeignKey(e => e.ScenarioId)
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

        // ── Phase 3: ParameterDocumentation ──────────────────────────────────
        modelBuilder.Entity<ParameterDocumentation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Scenario)
                  .WithMany(s => s.ParameterDocumentations)
                  .HasForeignKey(e => e.ScenarioId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ScenarioId, e.ParameterKey }).IsUnique();
            entity.Property(e => e.ParameterKey).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Label).HasMaxLength(120);
            entity.Property(e => e.DerivationNarrative).HasColumnType("nvarchar(max)");
        });

        // ── Phase 3: SubParameter ─────────────────────────────────────────────
        modelBuilder.Entity<SubParameter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ParameterDocumentation)
                  .WithMany(d => d.SubParameters)
                  .HasForeignKey(e => e.ParameterDocumentationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.OwnsOne(e => e.Value, sv =>
            {
                sv.Property(p => p.Coefficient).HasColumnName("Value_Coefficient");
                sv.Property(p => p.Exponent).HasColumnName("Value_Exponent");
            });

            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Unit).HasMaxLength(60);
            entity.Property(e => e.Rationale).HasColumnType("nvarchar(max)");
            entity.Property(e => e.SourceReference).HasMaxLength(300);

            // Phase 3.5 — operation column with default
            entity.Property(e => e.Operation).HasDefaultValue(StepOperation.Multiply);
        });

        // ── Phase 3: ParameterVariant ─────────────────────────────────────────
        modelBuilder.Entity<ParameterVariant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ParameterDocumentation)
                  .WithMany(d => d.Variants)
                  .HasForeignKey(e => e.ParameterDocumentationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Name).HasMaxLength(120).IsRequired();
        });

        // ── Phase 3: VariantSubParameter ──────────────────────────────────────
        modelBuilder.Entity<VariantSubParameter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ParameterVariant)
                  .WithMany(v => v.SubParameters)
                  .HasForeignKey(e => e.ParameterVariantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.OwnsOne(e => e.Value, sv =>
            {
                sv.Property(p => p.Coefficient).HasColumnName("Value_Coefficient");
                sv.Property(p => p.Exponent).HasColumnName("Value_Exponent");
            });

            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Unit).HasMaxLength(60);
            entity.Property(e => e.Rationale).HasColumnType("nvarchar(max)");
            entity.Property(e => e.SourceReference).HasMaxLength(300);

            // Phase 3.5 — operation column with default
            entity.Property(e => e.Operation).HasDefaultValue(StepOperation.Multiply);
        });

        // ── Phase 3.5: ProcessDomain ──────────────────────────────────────────
        modelBuilder.Entity<ProcessDomain>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconKey).HasMaxLength(40);
        });

        // ── Phase 3.5: ParameterTemplate ──────────────────────────────────────
        modelBuilder.Entity<ParameterTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ProcessDomain)
                  .WithMany(d => d.Templates)
                  .HasForeignKey(e => e.ProcessDomainId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Name).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // ── Phase 3.5: TemplateParameterDefinition ────────────────────────────
        modelBuilder.Entity<TemplateParameterDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ParameterTemplate)
                  .WithMany(t => t.ParameterDefinitions)
                  .HasForeignKey(e => e.ParameterTemplateId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.OwnsOne(e => e.SeedValue, sv =>
            {
                sv.Property(p => p.Coefficient).HasColumnName("SeedValue_Coefficient");
                sv.Property(p => p.Exponent).HasColumnName("SeedValue_Exponent");
            });

            entity.Property(e => e.Key).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Symbol).HasMaxLength(80);
            entity.Property(e => e.Label).HasMaxLength(120);
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.DefaultUnit).HasMaxLength(60);
        });

        // ── Phase 3.5: ParameterDefinition ───────────────────────────────────
        modelBuilder.Entity<ParameterDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Scenario)
                  .WithMany(s => s.ParameterDefinitions)
                  .HasForeignKey(e => e.ScenarioId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ScenarioId, e.Key }).IsUnique();

            entity.OwnsOne(e => e.DefaultValue, sv =>
            {
                sv.Property(p => p.Coefficient).HasColumnName("DefaultValue_Coefficient");
                sv.Property(p => p.Exponent).HasColumnName("DefaultValue_Exponent");
            });

            entity.Property(e => e.Key).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Symbol).HasMaxLength(80);
            entity.Property(e => e.Label).HasMaxLength(120);
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.Unit).HasMaxLength(60);
        });
        // ── Phase 5a: ScenarioConfiguration ───────────────────────────────────
        modelBuilder.Entity<ScenarioConfiguration>(static entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.Description)
                  .HasMaxLength(1000);

            entity.Property(e => e.SortOrder)
                  .HasDefaultValue(0);

            entity.Property(e => e.CreatedDate)
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Scenario)
                  .WithMany()
                  .HasForeignKey(e => e.ScenarioId)
                  .OnDelete(DeleteBehavior.Cascade);

            // One-to-one back to DeficitAnalysis (optional)
            entity.HasOne(e => e.DeficitAnalysis)
                  .WithOne(d => d.Configuration)
                  .HasForeignKey<DeficitAnalysis>(d => d.ConfigurationId)
                  .OnDelete(DeleteBehavior.NoAction)  // ← to this
                  .IsRequired(false);
        });
        // ── Phase 5a: ScenarioConfigurationEntry ───────────────────────────────────
        modelBuilder.Entity<ScenarioConfigurationEntry>(static entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ParameterKey)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.VariantLabel)
                  .HasMaxLength(200)
                  .HasDefaultValue("Base");

            entity.OwnsOne(e => e.SnapshotValue, static owned =>
            {
                owned.Property(v => v.Coefficient).HasColumnName("SnapshotCoefficient");
                owned.Property(v => v.Exponent).HasColumnName("SnapshotExponent");
            });

            entity.HasOne(e => e.Configuration)
                  .WithMany(c => c.Entries)
                  .HasForeignKey(e => e.ConfigurationId)
                  .OnDelete(DeleteBehavior.Cascade);

            // One entry per parameter per configuration
            entity.HasIndex(e => new { e.ConfigurationId, e.ParameterKey })
                  .IsUnique();
        });
    }
}