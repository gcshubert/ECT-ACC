namespace ECT.ACC.Contracts.DTOs;

public record DiagnosticNodeDto(
    string NodeId,
    string Name,
    string Role,
    string Classification,
    List<DiagnosticNodeDto> Children
);