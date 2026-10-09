using System.Threading.Channels;
using Claims.Services;

namespace Claims.Infrastructure.Auditing;

public class Auditer : IAuditer
{
    private readonly Channel<AuditEvent> _channel;
    private readonly TimeProvider _timeProvider;

    public Auditer(Channel<AuditEvent> channel, TimeProvider timeProvider)
    {
        _channel = channel;
        _timeProvider = timeProvider;
    }

    public async Task AuditClaim(string id, string httpRequestType)
    {
        await Enqueue(AuditTarget.Claim, id, httpRequestType);
    }

    public async Task AuditCover(string id, string httpRequestType)
    {
        await Enqueue(AuditTarget.Cover, id, httpRequestType);
    }

    private async Task Enqueue(AuditTarget target, string id, string httpRequestType)
    {
        var audit = new AuditEvent(target, id, httpRequestType, _timeProvider.GetUtcNow().UtcDateTime);
        await _channel.Writer.WriteAsync(audit);
    }
}
