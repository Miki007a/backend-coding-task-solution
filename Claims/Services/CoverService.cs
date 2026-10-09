using Claims.Domain;

namespace Claims.Services;

public class CoverService
{
    private readonly ICoverRepository _covers;
    private readonly IAuditer _auditer;
    private readonly PremiumCalculator _premiumCalculator;

    public CoverService(ICoverRepository covers, IAuditer auditer, PremiumCalculator premiumCalculator)
    {
        _covers = covers;
        _auditer = auditer;
        _premiumCalculator = premiumCalculator;
    }

    public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        return _premiumCalculator.ComputePremium(startDate, endDate, coverType);
    }

    public async Task<IEnumerable<Cover>> GetAllAsync()
    {
        return await _covers.GetAllAsync();
    }

    public async Task<Cover?> GetByIdAsync(string id)
    {
        return await _covers.GetByIdAsync(id);
    }

    public async Task<Cover> CreateAsync(Cover cover)
    {
        cover.Id = Guid.NewGuid().ToString();
        cover.Premium = _premiumCalculator.ComputePremium(cover.StartDate, cover.EndDate, cover.Type);
        await _covers.AddAsync(cover);
        _auditer.AuditCover(cover.Id, "POST");
        return cover;
    }

    public async Task DeleteAsync(string id)
    {
        _auditer.AuditCover(id, "DELETE");
        await _covers.DeleteAsync(id);
    }
}
