namespace PropertyManagement.Web.Domain.Scheduling;

/// <summary>
/// Value object representing a date range for scheduling windows
/// </summary>
public record DateRange
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }

    private DateRange(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException("End date cannot be before start date");

        Start = start;
        End = end;
    }

    public static DateRange Create(DateTime start, DateTime end) => new(start, end);

    public int DurationInDays => (End - Start).Days;

    public bool Overlaps(DateRange other)
    {
        return Start < other.End && other.Start < End;
    }

    public bool Contains(DateTime date)
    {
        return date >= Start && date <= End;
    }

    public override string ToString() => $"{Start:yyyy-MM-dd} to {End:yyyy-MM-dd}";
}
