namespace MySentry.Plugin.Features.Crons;

/// <summary>
/// Defines the status of a cron check-in.
/// </summary>
public enum CheckInStatus
{
    /// <summary>
    /// The job is in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// The job completed successfully.
    /// </summary>
    Ok,

    /// <summary>
    /// The job failed.
    /// </summary>
    Error
}

/// <summary>
/// Configuration for a monitored cron job.
/// </summary>
public sealed class CronJobConfig
{
    /// <summary>
    /// Gets or sets the monitor slug (unique identifier).
    /// </summary>
    public string MonitorSlug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cron schedule expression.
    /// </summary>
    public string? Schedule { get; set; }

    /// <summary>
    /// Gets or sets the schedule type.
    /// </summary>
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Crontab;

    /// <summary>
    /// Gets or sets the check-in margin in minutes.
    /// </summary>
    public int? CheckInMarginMinutes { get; set; }

    /// <summary>
    /// Gets or sets the max runtime in minutes.
    /// </summary>
    public int? MaxRuntimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the timezone for the schedule.
    /// </summary>
    public string? Timezone { get; set; }
}

/// <summary>
/// Defines the type of schedule.
/// </summary>
public enum ScheduleType
{
    /// <summary>
    /// Standard crontab format.
    /// </summary>
    Crontab,

    /// <summary>
    /// Interval-based schedule.
    /// </summary>
    Interval
}

/// <summary>
/// Represents a check-in ID.
/// </summary>
public readonly struct CheckInId : IEquatable<CheckInId>
{
    private readonly string _value;

    /// <summary>
    /// Creates a new check-in ID.
    /// </summary>
    public CheckInId(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets the string value of the check-in ID.
    /// </summary>
    public string Value => _value;

    /// <summary>
    /// Creates a new unique check-in ID.
    /// </summary>
    public static CheckInId New() => new(Guid.NewGuid().ToString("N"));

    /// <summary>
    /// Creates an empty check-in ID.
    /// </summary>
    public static CheckInId Empty => new(string.Empty);

    /// <inheritdoc />
    public bool Equals(CheckInId other) => _value == other._value;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is CheckInId other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _value?.GetHashCode() ?? 0;

    /// <inheritdoc />
    public override string ToString() => _value;

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(CheckInId left, CheckInId right) => left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(CheckInId left, CheckInId right) => !left.Equals(right);

    /// <summary>
    /// Implicit conversion to string.
    /// </summary>
    public static implicit operator string(CheckInId id) => id._value;
}
