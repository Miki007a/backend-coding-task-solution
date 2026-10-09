using System.Threading.Channels;

namespace Claims.Infrastructure.Auditing;

public class AuditBackgroundService : BackgroundService
{
    private readonly Channel<AuditEvent> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditBackgroundService> _logger;

    public AuditBackgroundService(
        Channel<AuditEvent> channel,
        IServiceScopeFactory scopeFactory,
        ILogger<AuditBackgroundService> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var audit in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await SaveAsync(audit, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to persist {Target} audit {Id}.", audit.Target, audit.Id);
            }
        }
    }

    private async Task SaveAsync(AuditEvent audit, CancellationToken stoppingToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AuditContext>();

        if (audit.Target == AuditTarget.Claim)
        {
            context.ClaimAudits.Add(new ClaimAudit
            {
                ClaimId = audit.Id,
                HttpRequestType = audit.HttpRequestType,
                Created = audit.CreatedUtc
            });
        }
        else
        {
            context.CoverAudits.Add(new CoverAudit
            {
                CoverId = audit.Id,
                HttpRequestType = audit.HttpRequestType,
                Created = audit.CreatedUtc
            });
        }

        await context.SaveChangesAsync(stoppingToken);
    }
}
