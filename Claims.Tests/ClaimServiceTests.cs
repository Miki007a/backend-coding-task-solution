using Claims.Contracts;
using Claims.Domain;
using Claims.Services;
using Xunit;

namespace Claims.Tests;

public class ClaimServiceTests
{
    [Fact]
    public async Task CreateAsync_DamageCostAboveLimit_ReturnsInvalidAndDoesNotSave()
    {
        var fixture = new Fixture();
        fixture.Covers.Covers.Add(Cover());

        var result = await fixture.Service.CreateAsync(Request(damageCost: 100_000.01m));

        Assert.IsType<ServiceResult<Claim>.Invalid>(result);
        Assert.Empty(fixture.Claims.Claims);
        Assert.Equal(0, fixture.Auditer.ClaimAudits);
    }

    [Fact]
    public async Task CreateAsync_CreatedBeforeCover_ReturnsInvalid()
    {
        var fixture = new Fixture();
        fixture.Covers.Covers.Add(Cover());

        var result = await fixture.Service.CreateAsync(Request(created: new DateTime(2026, 5, 31)));

        var invalid = Assert.IsType<ServiceResult<Claim>.Invalid>(result);
        Assert.Contains("Created", invalid.Errors.Keys);
    }

    [Fact]
    public async Task CreateAsync_CreatedAfterCover_ReturnsInvalid()
    {
        var fixture = new Fixture();
        fixture.Covers.Covers.Add(Cover());

        var result = await fixture.Service.CreateAsync(Request(created: new DateTime(2026, 7, 2)));

        var invalid = Assert.IsType<ServiceResult<Claim>.Invalid>(result);
        Assert.Contains("Created", invalid.Errors.Keys);
    }

    [Fact]
    public async Task CreateAsync_MissingCover_ReturnsNotFoundAndDoesNotAudit()
    {
        var fixture = new Fixture();

        var result = await fixture.Service.CreateAsync(Request());

        Assert.IsType<ServiceResult<Claim>.NotFound>(result);
        Assert.Equal(0, fixture.Auditer.ClaimAudits);
    }

    [Fact]
    public async Task CreateAsync_ValidClaim_SavesAndAuditsOnce()
    {
        var fixture = new Fixture();
        fixture.Covers.Covers.Add(Cover());

        var result = await fixture.Service.CreateAsync(Request());

        var success = Assert.IsType<ServiceResult<Claim>.Success>(result);
        Assert.Equal(100_000m, success.Value.DamageCost);
        Assert.Single(fixture.Claims.Claims);
        Assert.Equal(1, fixture.Auditer.ClaimAudits);
        Assert.Equal("POST", fixture.Auditer.LastClaimHttpMethod);
    }

    [Fact]
    public async Task DeleteAsync_MissingClaim_ReturnsNotFoundAndDoesNotAudit()
    {
        var fixture = new Fixture();

        var result = await fixture.Service.DeleteAsync("missing");

        Assert.IsType<ServiceResult.NotFound>(result);
        Assert.Equal(0, fixture.Auditer.ClaimAudits);
    }

    [Fact]
    public async Task DeleteAsync_ExistingClaim_DeletesAndAuditsOnce()
    {
        var fixture = new Fixture();
        fixture.Claims.Claims.Add(new Claim { Id = "claim-1" });

        var result = await fixture.Service.DeleteAsync("claim-1");

        Assert.IsType<ServiceResult.Success>(result);
        Assert.Empty(fixture.Claims.Claims);
        Assert.Equal(1, fixture.Auditer.ClaimAudits);
        Assert.Equal("DELETE", fixture.Auditer.LastClaimHttpMethod);
    }

    private static Cover Cover() => new()
    {
        Id = "cover-1",
        StartDate = new DateTime(2026, 6, 1),
        EndDate = new DateTime(2026, 7, 1)
    };

    private static CreateClaimRequest Request(
        decimal damageCost = 100_000m,
        DateTime? created = null) => new(
        "cover-1",
        created ?? new DateTime(2026, 6, 15),
        "Hull damage",
        ClaimType.Collision,
        damageCost);

    private sealed class Fixture
    {
        public FakeClaimRepository Claims { get; } = new();
        public FakeCoverRepository Covers { get; } = new();
        public FakeAuditer Auditer { get; } = new();
        public ClaimService Service { get; }

        public Fixture()
        {
            Service = new ClaimService(Claims, Covers, Auditer);
        }
    }

    private sealed class FakeClaimRepository : IClaimRepository
    {
        public List<Claim> Claims { get; } = [];

        public Task<IEnumerable<Claim>> GetAllAsync() => Task.FromResult<IEnumerable<Claim>>(Claims);

        public Task<Claim?> GetByIdAsync(string id) => Task.FromResult(Claims.SingleOrDefault(claim => claim.Id == id));

        public Task AddAsync(Claim claim)
        {
            Claims.Add(claim);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string id)
        {
            Claims.RemoveAll(claim => claim.Id == id);
            return Task.CompletedTask;
        }
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
        public int ClaimAudits { get; private set; }
        public string? LastClaimHttpMethod { get; private set; }

        public Task AuditClaim(string id, string httpRequestType)
        {
            ClaimAudits++;
            LastClaimHttpMethod = httpRequestType;
            return Task.CompletedTask;
        }

        public Task AuditCover(string id, string httpRequestType)
        {
            return Task.CompletedTask;
        }
    }
}
