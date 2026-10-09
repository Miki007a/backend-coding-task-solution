using Claims.Domain;

namespace Claims.Contracts;

public record CreateClaimRequest(
    string CoverId,
    DateTime Created,
    string Name,
    ClaimType Type,
    decimal DamageCost);

public record CreateCoverRequest(
    DateTime StartDate,
    DateTime EndDate,
    CoverType Type);
