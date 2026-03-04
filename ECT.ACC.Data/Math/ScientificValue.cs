namespace ECT.ACC.Data.Math;

public class ScientificValue
{
    public double Coefficient { get; set; }  // e.g. 1.85
    public double Exponent { get; set; }     // e.g. 185163

    public ScientificValue(double coefficient, double exponent)
    {
        Coefficient = coefficient;
        Exponent = exponent;
    }

    // Returns log10 representation for comparison operations
    public double ToLog10() => System.Math.Log10(Coefficient) + Exponent;

    // Human-readable scientific notation string
    public override string ToString() => $"{Coefficient} × 10^{Exponent}";

    // Basic arithmetic in log space to handle astronomical numbers
    public static ScientificValue Multiply(ScientificValue a, ScientificValue b)
    {
        double newExponent = a.Exponent + b.Exponent +
                             System.Math.Floor(System.Math.Log10(a.Coefficient * b.Coefficient));
        double newCoefficient = a.Coefficient * b.Coefficient /
                                System.Math.Pow(10, System.Math.Floor(
                                    System.Math.Log10(a.Coefficient * b.Coefficient)));
        return new ScientificValue(newCoefficient, newExponent);
    }

    public static ScientificValue Divide(ScientificValue a, ScientificValue b)
    {
        double rawCoeff = a.Coefficient / b.Coefficient;
        double rawExp = a.Exponent - b.Exponent + System.Math.Floor(System.Math.Log10(rawCoeff));
        double normCoeff = rawCoeff / System.Math.Pow(10, System.Math.Floor(System.Math.Log10(rawCoeff)));
        return new ScientificValue(normCoeff, rawExp);
    }
}
