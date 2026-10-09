using Claims.Services;
using Xunit;

namespace Claims.Tests;

public class CoverRulesTests
{
    private static CoverRules RulesOn(DateTime today) =>
        new(new FixedTimeProvider(new DateTimeOffset(today, TimeSpan.Zero)));

    [Fact]
    public void Validate_StartDateInThePast_ReturnsStartDateError()
    {
        var errors = RulesOn(new DateTime(2026, 6, 1)).Validate(new DateTime(2026, 5, 31), new DateTime(2026, 6, 30));

        Assert.Contains("StartDate", errors.Keys);
    }

    [Fact]
    public void Validate_StartDateToday_IsAllowed()
    {
        var errors = RulesOn(new DateTime(2026, 6, 1)).Validate(new DateTime(2026, 6, 1), new DateTime(2026, 7, 1));

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_EndDateEqualToStartDate_ReturnsEndDateError()
    {
        var errors = RulesOn(new DateTime(2026, 6, 1)).Validate(new DateTime(2026, 6, 1), new DateTime(2026, 6, 1));

        Assert.Contains("EndDate", errors.Keys);
    }

    [Fact]
    public void Validate_PeriodOfExactlyOneYear_IsAllowed()
    {
        var errors = RulesOn(new DateTime(2026, 1, 1)).Validate(new DateTime(2026, 1, 1), new DateTime(2027, 1, 1));

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_PeriodOfExactlyOneYearAcrossLeapYear_IsAllowed()
    {
        var errors = RulesOn(new DateTime(2024, 1, 1)).Validate(new DateTime(2024, 1, 1), new DateTime(2025, 1, 1));

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_PeriodLongerThanOneYear_ReturnsEndDateError()
    {
        var errors = RulesOn(new DateTime(2026, 1, 1)).Validate(new DateTime(2026, 1, 1), new DateTime(2027, 1, 2));

        Assert.Contains("EndDate", errors.Keys);
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
}
