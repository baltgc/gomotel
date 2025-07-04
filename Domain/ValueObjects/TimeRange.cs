namespace Gomotel.Domain.ValueObjects;

public record TimeRange(DateTime StartTime, DateTime EndTime)
{
    public static TimeRange Create(DateTime startTime, DateTime endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");
        if (startTime < DateTime.UtcNow.AddMinutes(-5)) // Allow 5 minutes tolerance
            throw new ArgumentException("Start time cannot be in the past");

        return new TimeRange(startTime, endTime);
    }

    public TimeSpan Duration => EndTime - StartTime;

    public bool OverlapsWith(TimeRange other)
    {
        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    public bool Contains(DateTime dateTime)
    {
        return dateTime >= StartTime && dateTime <= EndTime;
    }
}
