namespace ECT.ACC.Data.Math;

public static class ECTMath
{
    /// <summary>
    /// Core ECT equation: T = k / (E × C)
    /// All values represented as ScientificValue
    /// </summary>
    public static ScientificValue ComputeTime(
        ScientificValue k,
        ScientificValue energy,
        ScientificValue control)
    {
        var ec = ScientificValue.Multiply(energy, control);
        return ScientificValue.Divide(k, ec);
    }

    /// <summary>
    /// Minimum control required given available energy and time:
    /// C_min = k / (E_max × T_available)
    /// </summary>
    public static ScientificValue ComputeMinimumControl(
        ScientificValue k,
        ScientificValue energyMax,
        ScientificValue timeAvailable)
    {
        var et = ScientificValue.Multiply(energyMax, timeAvailable);
        return ScientificValue.Divide(k, et);
    }

    /// <summary>
    /// C Deficit = C_required / C_available
    /// </summary>
    public static ScientificValue ComputeDeficit(
        ScientificValue cRequired,
        ScientificValue cAvailable)
    {
        return ScientificValue.Divide(cRequired, cAvailable);
    }

    /// <summary>
    /// ACC deficit classification based on exponent magnitude
    /// </summary>
    public static string ClassifyDeficit(ScientificValue cDeficit)
    {
        double log = cDeficit.ToLog10();

        return log switch
        {
            < 3 => "None",      // C_deficit < 10^3
            < 10 => "Type A",    // Throughput gap
            < 50 => "Type B",    // Precision gap
            < 100 => "Type C",    // Coordination gap
            _ => "Type D"     // Specification gap
        };
    }
}