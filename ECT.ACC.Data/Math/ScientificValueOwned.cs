using Microsoft.EntityFrameworkCore;

namespace ECT.ACC.Data.Math;

[Owned]
public class ScientificValueOwned
{
    public double Coefficient { get; set; }
    public double Exponent { get; set; }
}