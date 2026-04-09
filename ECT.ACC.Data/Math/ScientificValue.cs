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
        double productCoeff = a.Coefficient * b.Coefficient;
        
        // Handle edge cases
        if (productCoeff == 0)
            return new ScientificValue(0, 0);
        
        if (productCoeff < 0)
            throw new InvalidOperationException("Cannot multiply scientific values with negative resulting coefficient");
        
        double logProduct = System.Math.Log10(productCoeff);
        double newExponent = a.Exponent + b.Exponent + System.Math.Floor(logProduct);
        double newCoefficient = productCoeff / System.Math.Pow(10, System.Math.Floor(logProduct));
        return new ScientificValue(newCoefficient, newExponent);
    }

    public static ScientificValue Divide(ScientificValue a, ScientificValue b)
    {
        if (b.Coefficient == 0)
            throw new InvalidOperationException("Cannot divide by zero coefficient");
        
        double rawCoeff = a.Coefficient / b.Coefficient;
        
        // Handle edge cases
        if (rawCoeff == 0)
            return new ScientificValue(0, 0);
        
        if (rawCoeff < 0)
            throw new InvalidOperationException("Cannot divide scientific values with negative resulting coefficient");
        
        double logRaw = System.Math.Log10(rawCoeff);
        double rawExp = a.Exponent - b.Exponent + System.Math.Floor(logRaw);
        double normCoeff = rawCoeff / System.Math.Pow(10, System.Math.Floor(logRaw));
        return new ScientificValue(normCoeff, rawExp);
    }
}
