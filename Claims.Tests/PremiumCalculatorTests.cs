using Claims.Domain;
using Claims.Services;
using Xunit;

namespace Claims.Tests;

public class PremiumCalculatorTests
{
    private static readonly DateTime Start = new(2026, 1, 1);

    public static TheoryData<CoverType, int, decimal> DurationCases { get; } = new()
    {
        { CoverType.Yacht, 1, 1375m },
        { CoverType.PassengerShip, 1, 1500m },
        { CoverType.Tanker, 1, 1875m },
        { CoverType.ContainerShip, 1, 1625m },
        { CoverType.BulkCarrier, 1, 1625m },

        { CoverType.Yacht, 30, 41250m },
        { CoverType.PassengerShip, 30, 45000m },
        { CoverType.Tanker, 30, 56250m },
        { CoverType.ContainerShip, 30, 48750m },
        { CoverType.BulkCarrier, 30, 48750m },

        { CoverType.Yacht, 31, 42556.25m },
        { CoverType.PassengerShip, 31, 46470m },
        { CoverType.Tanker, 31, 58087.50m },
        { CoverType.ContainerShip, 31, 50342.50m },
        { CoverType.BulkCarrier, 31, 50342.50m },

        { CoverType.Yacht, 180, 237187.50m },
        { CoverType.PassengerShip, 180, 265500m },
        { CoverType.Tanker, 180, 331875m },
        { CoverType.ContainerShip, 180, 287625m },
        { CoverType.BulkCarrier, 180, 287625m },

        { CoverType.Yacht, 181, 238452.50m },
        { CoverType.PassengerShip, 181, 266955m },
        { CoverType.Tanker, 181, 333693.75m },
        { CoverType.ContainerShip, 181, 289201.25m },
        { CoverType.BulkCarrier, 181, 289201.25m },

        { CoverType.Yacht, 365, 471212.50m },
        { CoverType.PassengerShip, 365, 534675m },
        { CoverType.Tanker, 365, 668343.75m },
        { CoverType.ContainerShip, 365, 579231.25m },
        { CoverType.BulkCarrier, 365, 579231.25m },
    };

    [Theory]
    [MemberData(nameof(DurationCases))]
    public void ComputePremium_DurationAndCoverType_ReturnsBandedPremium(CoverType coverType, int days, decimal expected)
    {
        var calculator = new PremiumCalculator();

        var premium = calculator.ComputePremium(Start, Start.AddDays(days), coverType);

        Assert.Equal(expected, premium);
    }
}
