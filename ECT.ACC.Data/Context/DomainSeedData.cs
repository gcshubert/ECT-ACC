using ECT.ACC.Data.Math;
using ECT.ACC.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ECT.ACC.Data.Context;

public partial class ECTDbContext
{
    public async Task SeedDomainsAsync()
    {
        if (await ProcessDomains.AnyAsync()) return;   // already seeded

        // ── Domain 1: Evolutionary Biology ───────────────────────────────────
        var bio = new ProcessDomain
        {
            Name = "Evolutionary Biology",
            Description = "Analyses of biological diversification, phylogenetic " +
                          "transitions, and origin-of-function scenarios.",
            IconKey = "biotech",
            Templates =
            {
                new ParameterTemplate
                {
                    Name        = "LUCA Divergence Analysis",
                    Description = "Pre-seeded with ECT core parameters matching " +
                                  "the LUCA Evolution paper parameter set.",
                    ParameterDefinitions =
                    {
                        new TemplateParameterDefinition
                        {
                            Key = "e", Symbol = "E", Label = "Energy Flux",
                            Description = "Usable energy flux available to the biosphere",
                            DefaultUnit = "J·s⁻¹", SortOrder = 0,
                            IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.0, Exponent = 100 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "c", Symbol = "C", Label = "Control Capacity",
                            Description = "Available control capacity (C_sel per clan)",
                            DefaultUnit = "dimensionless", SortOrder = 1,
                            IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.0, Exponent = 37 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "k", Symbol = "k", Label = "Complexity Constant",
                            Description = "Size of the target search space",
                            DefaultUnit = "dimensionless", SortOrder = 2,
                            IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.0, Exponent = 185 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "t", Symbol = "T", Label = "Time Available",
                            Description = "Time since LUCA in seconds",
                            DefaultUnit = "s", SortOrder = 3,
                            IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.26, Exponent = 17 },
                        },
                    },
                },
                new ParameterTemplate
                {
                    Name        = "De Novo Protein Origin",
                    Description = "Focused analysis of a single de novo protein emergence event.",
                    ParameterDefinitions =
                    {
                        new TemplateParameterDefinition
                        {
                            Key = "e", Symbol = "E", Label = "Energy Flux",
                            DefaultUnit = "J·s⁻¹", SortOrder = 0, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.0, Exponent = 100 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "c", Symbol = "C", Label = "Control Capacity",
                            DefaultUnit = "dimensionless", SortOrder = 1, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.0, Exponent = 37 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "k", Symbol = "k", Label = "Sequence Search Space",
                            Description = "k = L × (1/f) where L = protein length, f = functional density",
                            DefaultUnit = "dimensionless", SortOrder = 2, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.0, Exponent = 77 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "t", Symbol = "T", Label = "Time Window",
                            DefaultUnit = "s", SortOrder = 3, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 3.15, Exponent = 14 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "f_func", Symbol = "f", Label = "Functional Density",
                            Description = "Fraction of random sequences with any selectable function",
                            DefaultUnit = "bp⁻¹", SortOrder = 4, IsEctCoreParameter = false,
                            SeedValue = new ScientificValueOwned { Coefficient = 2.0, Exponent = -1 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "L_aa", Symbol = "L", Label = "Protein Length",
                            Description = "Target protein length in amino acids",
                            DefaultUnit = "aa", SortOrder = 5, IsEctCoreParameter = false,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.5, Exponent = 2 },
                        },
                    },
                },
            },
        };

        // ── Domain 2: Thermal / Mechanical Manufacturing ──────────────────────
        var thermal = new ProcessDomain
        {
            Name = "Thermal Manufacturing",
            Description = "Industrial processes involving heat transfer, " +
                          "thermodynamic cycles, and material transformation.",
            IconKey = "factory",
            Templates =
            {
                new ParameterTemplate
                {
                    Name        = "Laser Material Processing",
                    Description = "Fiber laser or CO₂ laser cutting/welding analysis.",
                    ParameterDefinitions =
                    {
                        new TemplateParameterDefinition
                        {
                            Key = "e", Symbol = "P", Label = "Beam Power",
                            Description = "Delivered optical power at workpiece",
                            DefaultUnit = "W", SortOrder = 0, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 3.0, Exponent = 3 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "c", Symbol = "C_ctrl", Label = "Process Control Capacity",
                            Description = "Closed-loop control bandwidth × precision",
                            DefaultUnit = "bits·s⁻¹", SortOrder = 1, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.0, Exponent = 8 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "k", Symbol = "k_proc", Label = "Process Complexity",
                            Description = "Effective search space for acceptable parameter combinations",
                            DefaultUnit = "dimensionless", SortOrder = 2, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.0, Exponent = 12 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "t", Symbol = "T_cyc", Label = "Cycle Time",
                            Description = "Available process window per part",
                            DefaultUnit = "s", SortOrder = 3, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 3.0, Exponent = 0 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "eta_beam", Symbol = "η_b", Label = "Beam Quality Factor",
                            Description = "M² beam quality (1.0 = ideal Gaussian)",
                            DefaultUnit = "dimensionless", SortOrder = 4, IsEctCoreParameter = false,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.1, Exponent = 0 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "v_feed", Symbol = "v_f", Label = "Feed Rate",
                            DefaultUnit = "mm·s⁻¹", SortOrder = 5, IsEctCoreParameter = false,
                            SeedValue = new ScientificValueOwned { Coefficient = 5.0, Exponent = 1 },
                        },
                    },
                },
            },
        };

        // ── Domain 3: Chemical Synthesis ──────────────────────────────────────
        var chem = new ProcessDomain
        {
            Name = "Chemical Synthesis",
            Description = "Reaction engineering, catalysis efficiency, " +
                          "and synthesis route feasibility analyses.",
            IconKey = "science",
            Templates =
            {
                new ParameterTemplate
                {
                    Name        = "Catalytic Reaction Feasibility",
                    Description = "ECT analysis of a catalysed synthesis route.",
                    ParameterDefinitions =
                    {
                        new TemplateParameterDefinition
                        {
                            Key = "e", Symbol = "G_rxn", Label = "Reaction Free Energy",
                            Description = "Gibbs free energy driving force per mole",
                            DefaultUnit = "kJ·mol⁻¹", SortOrder = 0, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = -8.5, Exponent = 1 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "c", Symbol = "TON", Label = "Turnover Number",
                            Description = "Catalyst turnover number × selectivity",
                            DefaultUnit = "mol·mol⁻¹", SortOrder = 1, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.0, Exponent = 4 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "k", Symbol = "k_route", Label = "Route Complexity",
                            Description = "Number of viable synthetic pathways searched",
                            DefaultUnit = "dimensionless", SortOrder = 2, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.0, Exponent = 6 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "t", Symbol = "t_rxn", Label = "Reaction Time",
                            DefaultUnit = "s", SortOrder = 3, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 3.6, Exponent = 3 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "T_rxn", Symbol = "T", Label = "Reaction Temperature",
                            DefaultUnit = "K", SortOrder = 4, IsEctCoreParameter = false,
                            SeedValue = new ScientificValueOwned { Coefficient = 3.73, Exponent = 2 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "ee", Symbol = "ee", Label = "Enantiomeric Excess",
                            Description = "Chiral selectivity (0–1)",
                            DefaultUnit = "dimensionless", SortOrder = 5, IsEctCoreParameter = false,
                            SeedValue = new ScientificValueOwned { Coefficient = 9.5, Exponent = -1 },
                        },
                    },
                },
            },
        };

        // ── Domain 4: Software / Information Systems ──────────────────────────
        var sw = new ProcessDomain
        {
            Name = "Software & Information Systems",
            Description = "Complexity and control analyses for algorithmic " +
                          "processes, AI training runs, and information-theoretic scenarios.",
            IconKey = "terminal",
            Templates =
            {
                new ParameterTemplate
                {
                    Name        = "AI Training Run Feasibility",
                    Description = "ECT bound on the informational capacity " +
                                  "required for a large-model training run.",
                    ParameterDefinitions =
                    {
                        new TemplateParameterDefinition
                        {
                            Key = "e", Symbol = "P_gpu", Label = "Compute Power",
                            Description = "Total cluster FP16 TFLOPS",
                            DefaultUnit = "TFLOPS", SortOrder = 0, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.0, Exponent = 5 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "c", Symbol = "C_grad", Label = "Gradient Control Capacity",
                            Description = "Effective bits of feedback per parameter update",
                            DefaultUnit = "bits·step⁻¹", SortOrder = 1, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.6, Exponent = 10 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "k", Symbol = "k_loss", Label = "Loss Landscape Complexity",
                            Description = "Effective search space of the parameter manifold",
                            DefaultUnit = "dimensionless", SortOrder = 2, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 1.0, Exponent = 100 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "t", Symbol = "t_train", Label = "Training Duration",
                            DefaultUnit = "s", SortOrder = 3, IsEctCoreParameter = true,
                            SeedValue = new ScientificValueOwned { Coefficient = 8.64, Exponent = 6 },
                        },
                        new TemplateParameterDefinition
                        {
                            Key = "n_params", Symbol = "N", Label = "Parameter Count",
                            DefaultUnit = "parameters", SortOrder = 4, IsEctCoreParameter = false,
                            SeedValue = new ScientificValueOwned { Coefficient = 7.0, Exponent = 10 },
                        },
                    },
                },
            },
        };

        ProcessDomains.AddRange(bio, thermal, chem, sw);
        await SaveChangesAsync();
    }
}