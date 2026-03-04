
namespace ECT.ACC.Contracts.DTOs;

public class ScientificValueDto
{
    public double Coefficient { get; set; }
    public double Exponent { get; set; }
    public string Display => $"{Coefficient} × 10^{Exponent}";
}
