namespace ECT.ACC.Data.Models;

public class Scenario
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    //Navigation properties
    public ScenarioParameters? Parameters { get; set; }
    public DeficitAnalysis? DeficitAnalysis { get; set; }
}
