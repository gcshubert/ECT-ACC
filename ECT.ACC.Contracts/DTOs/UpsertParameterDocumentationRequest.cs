namespace ECT.ACC.Contracts.DTOs;

public record UpsertParameterDocumentationRequest(
    string ParameterKey,
    string Label,
    string DerivationNarrative,
    IReadOnlyList<CreateSubParameterRequest> SubParameters
);
