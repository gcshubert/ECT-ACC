using ECT.ACC.Data.Math;
namespace ECT.ACC.Data.Models;

public class ScenarioParameters
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }
    public ScientificValue Energy { get; set; }
    public ScientificValue Control { get; set; }
    public ScientificValue Complexity { get; set; }
    public ScientificValue TimeAvailable { get; set; }

    // Navigation property
    public Scenario? Scenario { get; set; }
}
