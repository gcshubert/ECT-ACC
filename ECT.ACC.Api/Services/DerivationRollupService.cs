using ECT.ACC.Data.Math;
using ECT.ACC.Data.Models;

namespace ECT.ACC.Api.Services;

/// <summary>
/// Evaluates an ordered chain of SubParameter steps into a single
/// ScientificValueOwned result.  Works in log-space for Multiply/Divide
/// to preserve precision at extreme exponents (e.g. 10^174000).
/// Add/Subtract fall back to linear arithmetic.
/// </summary>
public static class DerivationRollupService
{
    public static ScientificValueOwned Compute(
        IEnumerable<(double Coefficient, int Exponent, StepOperation Op)> steps)
    {
        var ordered = steps.ToList();
        if (ordered.Count == 0)
            return new ScientificValueOwned { Coefficient = 0, Exponent = 0 };

        double accCoeff = ordered[0].Coefficient;
        long accExp = ordered[0].Exponent;

        foreach (var (coeff, exp, op) in ordered.Skip(1))
        {
            switch (op)
            {
                case StepOperation.Multiply:
                    accCoeff *= coeff;
                    accExp += exp;
                    NormaliseCoeff(ref accCoeff, ref accExp);
                    break;

                case StepOperation.Divide:
                    if (coeff == 0) throw new DivideByZeroException(
                        "SubParameter step has zero coefficient in Divide operation.");
                    accCoeff /= coeff;
                    accExp -= exp;
                    NormaliseCoeff(ref accCoeff, ref accExp);
                    break;

                case StepOperation.Add:
                    {
                        var (a, b, sharedExp) = AlignExponents(accCoeff, accExp, coeff, exp);
                        accCoeff = a + b;
                        accExp = sharedExp;
                        NormaliseCoeff(ref accCoeff, ref accExp);
                        break;
                    }

                case StepOperation.Subtract:
                    {
                        var (a, b, sharedExp) = AlignExponents(accCoeff, accExp, coeff, exp);
                        accCoeff = a - b;
                        accExp = sharedExp;
                        NormaliseCoeff(ref accCoeff, ref accExp);
                        break;
                    }

                case StepOperation.Power:
                    double n = coeff * Math.Pow(10, exp);
                    accCoeff = Math.Pow(accCoeff, n);
                    accExp = (long)(accExp * n);
                    NormaliseCoeff(ref accCoeff, ref accExp);
                    break;
            }
        }

        return new ScientificValueOwned
        {
            Coefficient = Math.Round(accCoeff, 6),
            Exponent = (int)Math.Clamp(accExp, int.MinValue, int.MaxValue),
        };
    }

    private static void NormaliseCoeff(ref double coeff, ref long exp)
    {
        if (coeff == 0) return;
        int adj = (int)Math.Floor(Math.Log10(Math.Abs(coeff)));
        coeff /= Math.Pow(10, adj);
        exp += adj;
    }

    private static (double a, double b, long sharedExp) AlignExponents(
        double coeffA, long expA, double coeffB, long expB)
    {
        if (expA >= expB)
        {
            double bScaled = coeffB * Math.Pow(10, expB - expA);
            return (coeffA, bScaled, expA);
        }
        else
        {
            double aScaled = coeffA * Math.Pow(10, expA - expB);
            return (aScaled, coeffB, expB);
        }
    }
}