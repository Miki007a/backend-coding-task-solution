using Claims.Domain;

namespace Claims.Services;

public interface IClaimRepository
{
    Task<IEnumerable<Claim>> GetAllAsync();

    Task<Claim?> GetByIdAsync(string id);

    Task AddAsync(Claim claim);

    Task DeleteAsync(string id);
}
