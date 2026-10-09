namespace Claims.Services;

public class CoverRules
{
    private readonly TimeProvider _timeProvider;

    public CoverRules(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Dictionary<string, string[]> Validate(DateTime startDate, DateTime endDate)
    {
        var errors = new Dictionary<string, string[]>();
        var today = _timeProvider.GetUtcNow().UtcDateTime.Date;

        if (startDate.Date < today)
        {
            errors["StartDate"] = ["Start date cannot be in the past."];
        }

        if (endDate.Date <= startDate.Date)
        {
            errors["EndDate"] = ["End date must be after the start date."];
            return errors;
        }

        if (endDate.Date > startDate.Date.AddYears(1))
        {
            errors["EndDate"] = ["The insurance period cannot exceed 1 year."];
        }

        return errors;
    }
}
