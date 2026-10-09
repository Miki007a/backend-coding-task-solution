using Claims.Contracts;
using Claims.Domain;

namespace Claims.Services;

public class ClaimService
{
    private const decimal MaxDamageCost = 100_000m;

    private readonly IClaimRepository _claims;
    private readonly ICoverRepository _covers;
    private readonly IAuditer _auditer;

    public ClaimService(IClaimRepository claims, ICoverRepository covers, IAuditer auditer)
    {
        _claims = claims;
        _covers = covers;
        _auditer = auditer;
    }

    public async Task<IEnumerable<Claim>> GetAllAsync()
    {
        return await _claims.GetAllAsync();
    }

    public async Task<Claim?> GetByIdAsync(string id)
    {
        return await _claims.GetByIdAsync(id);
    }

    public async Task<ServiceResult<Claim>> CreateAsync(CreateClaimRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.DamageCost > MaxDamageCost)
        {
            errors["DamageCost"] = ["Damage cost cannot exceed 100000."];
        }

        var cover = await _covers.GetByIdAsync(request.CoverId);
        if (cover is null)
        {
            if (errors.Count > 0)
            {
                return new ServiceResult<Claim>.Invalid(errors);
            }

            return new ServiceResult<Claim>.NotFound();
        }

        if (request.Created.Date < cover.StartDate.Date || request.Created.Date > cover.EndDate.Date)
        {
            errors["Created"] = ["Created date must be within the cover period."];
        }

        if (errors.Count > 0)
        {
            return new ServiceResult<Claim>.Invalid(errors);
        }

        var claim = new Claim
        {
            Id = Guid.NewGuid().ToString(),
            CoverId = request.CoverId,
            Created = request.Created,
            Name = request.Name,
            Type = request.Type,
            DamageCost = request.DamageCost
        };

        await _claims.AddAsync(claim);
        _auditer.AuditClaim(claim.Id, "POST");
        return new ServiceResult<Claim>.Success(claim);
    }

    public async Task<ServiceResult> DeleteAsync(string id)
    {
        var claim = await _claims.GetByIdAsync(id);
        if (claim is null)
        {
            return new ServiceResult.NotFound();
        }

        await _claims.DeleteAsync(id);
        _auditer.AuditClaim(id, "DELETE");
        return new ServiceResult.Success();
    }
}
