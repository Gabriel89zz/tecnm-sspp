namespace TecNM.Api.Modules.Catalogs;

public sealed record CareerResponse(
    long Id,
    string Code,
    string Name,
    int TotalCredits,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CareerRequest(
    string Code,
    string Name,
    int TotalCredits = 260);

public sealed record UpdateStatusRequest(bool IsActive);

public sealed record AcademicPeriodResponse(
    long Id,
    string Code,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AcademicPeriodRequest(
    string Code,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate);
