namespace Claims.Infrastructure.Auditing;

public enum AuditTarget
{
    Claim,
    Cover
}

public readonly record struct AuditEvent(AuditTarget Target, string Id, string HttpRequestType, DateTime CreatedUtc);
