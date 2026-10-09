using Claims.Contracts;
using Claims.Domain;
using Claims.Services;
using Xunit;

namespace Claims.Tests;

public class CoverServiceTests
{
    private static readonly DateTime Today = new(2026, 6, 1);

    [Fact]
    public async Task CreateAsync_StartDateInThePast_DoesNotSaveOrAudit()
    {
        var fixture = new Fixture(Today);

        var result = await fixture.Service.CreateAsync(new CreateCoverRequest(
            new DateTime(2026, 5, 31),
            new DateTime(2026, 6, 30),
            CoverType.Yacht));

        Assert.IsType<ServiceResult<Cover>.Invalid>(result);
        Assert.Empty(fixture.Covers.Covers);
        Assert.Equal(0, fixture.Auditer.CoverAudits);
    }

    [Fact]
    public async Task CreateAsync_ValidCover_SavesPremiumAndAuditsOnce()
    {
        var fixture = new Fixture(Today);

        var result = await fixture.Service.CreateAsync(new CreateCoverRequest(
            Today,
            Today.AddDays(1),
            CoverType.Yacht));

        var success = Assert.IsType<ServiceResult<Cover>.Success>(result);
        Assert.Equal(1375m, success.Value.Premium);
        Assert.Single(fixture.Covers.Covers);
        Assert.Equal(1, fixture.Auditer.CoverAudits);
    }

    [Fact]
    public async Task DeleteAsync_MissingCover_DoesNotAudit()
    {
        var fixture = new Fixture(Today);

        var result = await fixture.Service.DeleteAsync("missing");

        Assert.IsType<ServiceResult.NotFound>(result);
        Assert.Equal(0, fixture.Auditer.CoverAudits);
    }

    private sealed class Fixture
    {
        public FakeCoverRepository Covers { get; } = new();
        public FakeAuditer Auditer { get; } = new();
        public CoverService Service { get; }

        public Fixture(DateTime today)
        {
            Service = new CoverService(
                Covers,
                Auditer,
                new PremiumCalculator(),
                new CoverRules(new FixedTimeProvider(new DateTimeOffset(today, TimeSpan.Zero))));
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    private sealed class FakeCoverRepository : ICoverRepository
    {
        public List<Cover> Covers { get; } = [];

        public Task<IEnumerable<Cover>> GetAllAsync() => Task.FromResult<IEnumerable<Cover>>(Covers);

        public Task<Cover?> GetByIdAsync(string id) => Task.FromResult(Covers.SingleOrDefault(cover => cover.Id == id));

        public Task AddAsync(Cover cover)
        {
            Covers.Add(cover);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string id)
        {
            Covers.RemoveAll(cover => cover.Id == id);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAuditer : IAuditer
    {
        public int CoverAudits { get; private set; }

        public void AuditClaim(string id, string httpRequestType)
        {
        }

        public void AuditCover(string id, string httpRequestType)
        {
            CoverAudits++;
        }
    }
}
