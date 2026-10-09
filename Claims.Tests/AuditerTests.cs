using System.Threading.Channels;
using Claims.Infrastructure.Auditing;
using Xunit;

namespace Claims.Tests;

public class AuditerTests
{
    [Fact]
    public async Task AuditClaim_EnqueuesClaimEvent()
    {
        var channel = Channel.CreateBounded<AuditEvent>(1);
        var createdAt = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var auditer = new Auditer(channel, new FixedTimeProvider(createdAt));

        await auditer.AuditClaim("claim-1", "POST");

        var audit = await channel.Reader.ReadAsync(TestContext.Current.CancellationToken);
        Assert.Equal(AuditTarget.Claim, audit.Target);
        Assert.Equal("claim-1", audit.Id);
        Assert.Equal("POST", audit.HttpRequestType);
        Assert.Equal(createdAt, audit.CreatedUtc);
    }

    [Fact]
    public async Task AuditCover_EnqueuesCoverEvent()
    {
        var channel = Channel.CreateBounded<AuditEvent>(1);
        var createdAt = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var auditer = new Auditer(channel, new FixedTimeProvider(createdAt));

        await auditer.AuditCover("cover-1", "DELETE");

        var audit = await channel.Reader.ReadAsync(TestContext.Current.CancellationToken);
        Assert.Equal(AuditTarget.Cover, audit.Target);
        Assert.Equal("cover-1", audit.Id);
        Assert.Equal("DELETE", audit.HttpRequestType);
        Assert.Equal(createdAt, audit.CreatedUtc);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTime utcNow)
        {
            _utcNow = new DateTimeOffset(utcNow);
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
