namespace ECT.ACC.Contracts.DTOs;

public record CreateParameterVariantRequest(
    string Name,
    IReadOnlyList<UpsertVariantSubParameterRequest> SubParameters
);
