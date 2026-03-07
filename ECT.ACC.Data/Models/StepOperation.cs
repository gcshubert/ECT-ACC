namespace ECT.ACC.Data.Models;

/// <summary>
/// The arithmetic operation a SubParameter step applies to the running
/// accumulator in a derivation chain.
/// The first step is always the seed value — its operation is ignored;
/// the UI renders it as "=" rather than an operator symbol.
/// </summary>
public enum StepOperation
{
    /// <summary>result = accumulator × step.value</summary>
    Multiply = 0,

    /// <summary>result = accumulator ÷ step.value</summary>
    Divide = 1,

    /// <summary>result = accumulator + step.value</summary>
    Add = 2,

    /// <summary>result = accumulator − step.value</summary>
    Subtract = 3,

    /// <summary>result = accumulator ^ step.value</summary>
    Power = 4,
}