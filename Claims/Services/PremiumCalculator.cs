using Claims.Domain;

namespace Claims.Services;

public class PremiumCalculator
{
    private const decimal BaseDailyRate = 1250m;
    private const decimal OtherTypeMultiplier = 1.30m;

    private static readonly Dictionary<CoverType, decimal> TypeMultipliers = new()
    {
        [CoverType.Yacht] = 1.10m,
        [CoverType.PassengerShip] = 1.20m,
        [CoverType.Tanker] = 1.50m,
    };

    private static readonly PremiumBand[] Bands =
    [
        new(MaxDays: 30, YachtDiscount: 0m, OtherDiscount: 0m),
        new(MaxDays: 150, YachtDiscount: 0.05m, OtherDiscount: 0.02m),
        new(MaxDays: null, YachtDiscount: 0.08m, OtherDiscount: 0.03m),
    ];

    public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        var insuredDays = (endDate.Date - startDate.Date).Days;
        if (insuredDays <= 0)
        {
            return 0m;
        }

        var dailyRate = BaseDailyRate * MultiplierFor(coverType);
        var remainingDays = insuredDays;
        var total = 0m;

        foreach (var band in Bands)
        {
            int daysInBand;
            if (band.MaxDays.HasValue)
            {
                daysInBand = Math.Min(remainingDays, band.MaxDays.Value);
            }
            else
            {
                daysInBand = remainingDays;
            }

            var discount = coverType == CoverType.Yacht ? band.YachtDiscount : band.OtherDiscount;
            total += daysInBand * dailyRate * (1m - discount);
            remainingDays -= daysInBand;

            if (remainingDays == 0)
            {
                break;
            }
        }

        return total;
    }

    private static decimal MultiplierFor(CoverType coverType) =>
        TypeMultipliers.TryGetValue(coverType, out var multiplier)
            ? multiplier
            : OtherTypeMultiplier;

    private readonly record struct PremiumBand(int? MaxDays, decimal YachtDiscount, decimal OtherDiscount);
}
