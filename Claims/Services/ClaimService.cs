using Claims.Domain;

namespace Claims.Services;

public class ClaimService
{
    private readonly IClaimRepository _claims;
    private readonly IAuditer _auditer;

    public ClaimService(IClaimRepository claims, IAuditer auditer)
    {
        _claims = claims;
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

    public async Task<Claim> CreateAsync(Claim claim)
    {
        claim.Id = Guid.NewGuid().ToString();
        await _claims.AddAsync(claim);
        _auditer.AuditClaim(claim.Id, "POST");
        return claim;
    }

    public async Task DeleteAsync(string id)
    {
        _auditer.AuditClaim(id, "DELETE");
        await _claims.DeleteAsync(id);
    }
}
