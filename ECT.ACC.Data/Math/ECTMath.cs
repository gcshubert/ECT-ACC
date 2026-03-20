namespace ECT.ACC.Data.Math;

/// <summary>
/// ECT core formula: k = E × C × T
///
/// Four variables, six solve-for modes.
/// Each mode answers a distinct practitioner question:
///
///   Solve C  (given E, T)        — Feasibility:    can this system do this job?
///   Solve T  (given E, C)        — Scheduling:     how long will this take?
///   Solve E  (given C, T)        — Resource:       what energy do I need?
///   Solve T  (given E×C, k)      — Throughput opt: optimise time given combined budget
///   Solve C  (given E×T, k)      — Precision opt:  optimise control given energy-time budget
///   Solve E×C (given T, k)       — Combined:       what energy-control budget do I need?
///   Solve k  (given E, C, T)     — Capability:     what complexity ceiling can this system achieve?
///
/// ECT formally decomposes the project management golden triangle
/// (Quality / Cost / Time) into four variables by splitting Cost
/// into E (energy — how much work) and C (control — how precisely directed).
/// This decomposition is what makes the framework quantitative rather than qualitative.
///
/// In V2, C_req, C_avail, and Δ are rollup results from a graph traversal
/// (ECT.Graph.Api), not flat parameter lookups. ECTMath operates on those
/// rolled-up ScientificValues — it is agnostic to how they were produced.
/// </summary>
public static class ECTMath
{
    // -------------------------------------------------------------------------
    // Solve-for modes
    // -------------------------------------------------------------------------

    /// <summary>
    /// Solve for C_required: the minimum control capacity needed.
    /// C = k / (E × T)
    /// Mode: Feasibility — given known k, E, T, can available C meet C_req?
    /// </summary>
    public static ScientificValue SolveForC(
        ScientificValue k,
        ScientificValue energy,
        ScientificValue timeAvailable)
    {
        var et = ScientificValue.Multiply(energy, timeAvailable);
        return ScientificValue.Divide(k, et);
    }

    /// <summary>
    /// Solve for T: the time window required.
    /// T = k / (E × C)
    /// Mode: Scheduling — given known k, E, C, how long will this take?
    /// </summary>
    public static ScientificValue SolveForT(
        ScientificValue k,
        ScientificValue energy,
        ScientificValue control)
    {
        var ec = ScientificValue.Multiply(energy, control);
        return ScientificValue.Divide(k, ec);
    }

    /// <summary>
    /// Solve for E: the energy required.
    /// E = k / (C × T)
    /// Mode: Resource planning — given known k, C, T, what energy is needed?
    /// </summary>
    public static ScientificValue SolveForE(
        ScientificValue k,
        ScientificValue control,
        ScientificValue timeAvailable)
    {
        var ct = ScientificValue.Multiply(control, timeAvailable);
        return ScientificValue.Divide(k, ct);
    }

    /// <summary>
    /// Solve for k: the complexity ceiling this system can achieve.
    /// k = E × C × T
    /// Mode: Capability — given known E, C, T, what outcome complexity is achievable?
    ///
    /// This is a capability envelope question, not a deficit question.
    /// The result is the maximum process complexity k this system can reliably achieve
    /// given its energy, control capacity, and time window.
    ///
    /// At node level (from graph traversal) this produces a complexity map —
    /// identifying not just the ceiling but where in the topology it is being set.
    /// </summary>
    public static ScientificValue SolveForK(
        ScientificValue energy,
        ScientificValue control,
        ScientificValue timeAvailable)
    {
        var ec = ScientificValue.Multiply(energy, control);
        return ScientificValue.Multiply(ec, timeAvailable);
    }

    /// <summary>
    /// Solve for T given combined E×C budget and k.
    /// T = k / (E × C)
    /// Mode: Throughput optimisation — fix energy-control product, solve for time.
    /// energyControlProduct is the pre-multiplied E×C rollup from the graph walk.
    /// </summary>
    public static ScientificValue SolveForT_FromECProduct(
        ScientificValue k,
        ScientificValue energyControlProduct)
    {
        return ScientificValue.Divide(k, energyControlProduct);
    }

    /// <summary>
    /// Solve for C given combined E×T budget and k.
    /// C = k / (E × T)
    /// Mode: Precision optimisation — fix energy-time product, solve for control.
    /// energyTimeProduct is the pre-multiplied E×T rollup from the graph walk.
    /// </summary>
    public static ScientificValue SolveForC_FromETProduct(
        ScientificValue k,
        ScientificValue energyTimeProduct)
    {
        return ScientificValue.Divide(k, energyTimeProduct);
    }

    /// <summary>
    /// Solve for E×C combined budget given T and k.
    /// E×C = k / T
    /// Mode: Combined budget — given T and k, what energy-control product is needed?
    /// </summary>
    public static ScientificValue SolveForEC(
        ScientificValue k,
        ScientificValue timeAvailable)
    {
        return ScientificValue.Divide(k, timeAvailable);
    }

    // -------------------------------------------------------------------------
    // Deficit and classification
    // -------------------------------------------------------------------------

    /// <summary>
    /// Control deficit ratio: Δ = C_required / C_available
    /// Δ > 1  →  deficit (C_req exceeds C_avail)
    /// Δ ≤ 1  →  sufficient capacity
    ///
    /// In V2, C_required and C_available are rollup results from the graph walk,
    /// not flat parameter lookups.
    /// </summary>
    public static ScientificValue ComputeDeficit(
        ScientificValue cRequired,
        ScientificValue cAvailable)
    {
        return ScientificValue.Divide(cRequired, cAvailable);
    }

    /// <summary>
    /// ACC-H transitional classifier.
    ///
    /// Gates on solveForMode and domain before applying magnitude thresholds.
    /// Prevents spurious "None" classification for non-control solve-for modes —
    /// the root cause of the V1 "None for everything" bug on Laser Cut scenarios.
    ///
    /// Classification states:
    ///   N/A    — solve-for mode does not produce a control deficit
    ///   None   — deficit below threshold (Δ &lt; 10^3 in log10 terms)
    ///   Type A — throughput gap
    ///   Type B — precision gap
    ///   Type C — coordination gap
    ///   Type D — specification gap
    /// </summary>
    public static string ClassifyDeficit(
        ScientificValue cDeficit,
        string solveForMode,
        string domain)
    {
        if (!IsControlSolveMode(solveForMode)) return "N/A";
        if (!IsControlRelevantDomain(domain)) return "N/A";

        double log = cDeficit.ToLog10();

        return log switch
        {
            < 3 => "None",
            < 10 => "Type A",
            < 50 => "Type B",
            < 100 => "Type C",
            _ => "Type D"
        };
    }

    /// <summary>
    /// V1 backward-compatible classifier — no mode or domain gating.
    /// Retained so existing V1 call sites continue to compile without change.
    /// V2 call sites should use ClassifyDeficit(deficit, solveForMode, domain).
    /// </summary>
    public static string ClassifyDeficit(ScientificValue cDeficit)
    {
        double log = cDeficit.ToLog10();

        return log switch
        {
            < 3 => "None",
            < 10 => "Type A",
            < 50 => "Type B",
            < 100 => "Type C",
            _ => "Type D"
        };
    }

    /// <summary>
    /// Returns true if the solve-for mode produces a control deficit (C_req vs C_avail).
    /// Only "C" modes yield a meaningful deficit — T, E, k, and combined modes do not.
    /// </summary>
    private static bool IsControlSolveMode(string solveForMode) =>
        solveForMode is "C" or "C_FromET";

    /// <summary>
    /// Returns true if the domain is one where control deficit analysis is meaningful.
    /// Extend this list as additional domains are validated against the ECT framework.
    /// </summary>
    private static bool IsControlRelevantDomain(string domain) =>
        domain is "Manufacturing" or "Laser" or "Thermal" or "Biological";
}