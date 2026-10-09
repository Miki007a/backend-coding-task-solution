using Claims.Contracts;
using Claims.Domain;

namespace Claims.Services;

public class CoverService
{
    private readonly ICoverRepository _covers;
    private readonly IAuditer _auditer;
    private readonly PremiumCalculator _premiumCalculator;
    private readonly CoverRules _coverRules;

    public CoverService(
        ICoverRepository covers,
        IAuditer auditer,
        PremiumCalculator premiumCalculator,
        CoverRules coverRules)
    {
        _covers = covers;
        _auditer = auditer;
        _premiumCalculator = premiumCalculator;
        _coverRules = coverRules;
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

    public async Task<ServiceResult<Cover>> CreateAsync(CreateCoverRequest request)
    {
        var errors = _coverRules.Validate(request.StartDate, request.EndDate);
        if (errors.Count > 0)
        {
            return new ServiceResult<Cover>.Invalid(errors);
        }

        var cover = new Cover
        {
            Id = Guid.NewGuid().ToString(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Type = request.Type,
            Premium = _premiumCalculator.ComputePremium(request.StartDate, request.EndDate, request.Type)
        };

        await _covers.AddAsync(cover);
        await _auditer.AuditCover(cover.Id, "POST");
        return new ServiceResult<Cover>.Success(cover);
    }

    public async Task<ServiceResult> DeleteAsync(string id)
    {
        var cover = await _covers.GetByIdAsync(id);
        if (cover is null)
        {
            return new ServiceResult.NotFound();
        }

        await _covers.DeleteAsync(id);
        await _auditer.AuditCover(id, "DELETE");
        return new ServiceResult.Success();
    }
}
