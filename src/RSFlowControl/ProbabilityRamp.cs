namespace RSFlowControl;

/// <summary>
/// A probability ramp that increases the chance of success over a specified timespan.
/// The chance starts at a minimum value and ramps up to a maximum value linearly.
/// </summary>
public class ProbabilityRamp
{
    public double MinimumChance { get; }
    public double MaximumChance { get; }
    public TimeSpan MaxTimespan { get; }
    private readonly object _lock = new object();

    private DateTime _startTime;

    public ProbabilityRamp(double minimumChance, double maximumChance, TimeSpan maxTimespan)
    {
        if (minimumChance < 0 || minimumChance > 1)
            throw new ArgumentOutOfRangeException(nameof(minimumChance), "Must be between 0 and 1");

        if (maximumChance < 0 || maximumChance > 1)
            throw new ArgumentOutOfRangeException(nameof(maximumChance), "Must be between 0 and 1");

        if (minimumChance > maximumChance)
            throw new ArgumentException("Minimum chance cannot be greater than maximum chance");

        if (maxTimespan <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(maxTimespan), "Must be greater than zero");

        MinimumChance = minimumChance;
        MaximumChance = maximumChance;
        MaxTimespan = maxTimespan;
        _startTime = DateTime.UtcNow;
    }

    public double CurrentChance
    {
        get
        {
            lock (_lock)
            {
                var elapsed = DateTime.UtcNow - _startTime;

                if (elapsed >= MaxTimespan)
                    return MaximumChance;

                // Linear interpolation between min and max based on elapsed time
                var progress = elapsed.TotalMilliseconds / MaxTimespan.TotalMilliseconds;
                return MinimumChance + (progress * (MaximumChance - MinimumChance));
            }
        }
    }

    public bool Check()
    {
        lock (_lock)
        {
            var currentChance = CurrentChance;
            var randomValue = Random.Shared.NextDouble();
            return randomValue < currentChance;
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _startTime = DateTime.UtcNow;
        }
    }
}