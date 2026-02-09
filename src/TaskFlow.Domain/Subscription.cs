namespace TaskFlow.Domain;

/// <summary>
/// Represents a subscription boundary that owns projects, tasks, and focus sessions.
/// </summary>
public class Subscription
{
    private readonly List<SubscriptionSchedule> schedules = [];
    private const string DEFAULT_TIME_ZONE_ID = "Europe/Berlin";

    /// <summary>
    /// Gets the unique identifier of the subscription.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the display name of the subscription.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the subscription tier.
    /// </summary>
    public SubscriptionTier Tier { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the subscription is enabled.
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the subscription was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the IANA time zone identifier used for date bucketing and reminders.
    /// </summary>
    public string TimeZoneId { get; private set; }

    /// <summary>
    /// Gets all schedules attached to this subscription.
    /// </summary>
    public IReadOnlyCollection<SubscriptionSchedule> Schedules => this.schedules.AsReadOnly();

    private Subscription()
    {
        this.Name = string.Empty;
        this.TimeZoneId = DEFAULT_TIME_ZONE_ID;
    }

    /// <summary>
    /// Initializes a new active subscription.
    /// </summary>
    /// <param name="name">Display name of the subscription.</param>
    /// <param name="tier">Commercial tier of the subscription.</param>
    public Subscription(string name, SubscriptionTier tier)
        : this(Guid.NewGuid(), name, tier, true, DEFAULT_TIME_ZONE_ID)
    {
    }

    /// <summary>
    /// Initializes a subscription with an explicit identifier.
    /// </summary>
    /// <param name="id">Subscription identifier.</param>
    /// <param name="name">Display name of the subscription.</param>
    /// <param name="tier">Commercial tier of the subscription.</param>
    /// <param name="isEnabled">Whether the subscription is enabled.</param>
    /// <param name="timeZoneId">IANA time zone identifier.</param>
    public Subscription(Guid id, string name, SubscriptionTier tier, bool isEnabled, string timeZoneId = DEFAULT_TIME_ZONE_ID)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Subscription id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Subscription name cannot be empty.", nameof(name));
        }

        this.Id = id;
        this.Name = name.Trim();
        this.Tier = tier;
        this.IsEnabled = isEnabled;
        this.CreatedAt = DateTime.UtcNow;
        this.TimeZoneId = ValidateTimeZoneId(timeZoneId);
    }

    /// <summary>
    /// Updates the subscription timezone.
    /// </summary>
    /// <param name="timeZoneId">Target IANA time zone identifier.</param>
    public void SetTimeZone(string timeZoneId)
    {
        this.TimeZoneId = ValidateTimeZoneId(timeZoneId);
    }

    /// <summary>
    /// Upgrades the subscription to plus tier.
    /// </summary>
    public void UpgradeToPlus()
    {
        this.Tier = SubscriptionTier.Plus;
    }

    /// <summary>
    /// Upgrades the subscription to pro tier.
    /// </summary>
    public void UpgradeToPro()
    {
        this.Tier = SubscriptionTier.Pro;
    }

    /// <summary>
    /// Downgrades the subscription to free tier.
    /// </summary>
    public void DowngradeToFree()
    {
        this.Tier = SubscriptionTier.Free;
    }

    /// <summary>
    /// Adds an open-ended schedule starting at the provided UTC date.
    /// </summary>
    /// <param name="startsOn">Inclusive start date.</param>
    public void AddOpenEndedSchedule(DateOnly startsOn)
    {
        var schedule = SubscriptionSchedule.CreateOpenEnded(this.Id, startsOn);
        this.schedules.Add(schedule);
    }

    /// <summary>
    /// Adds a bounded schedule between UTC start and end timestamps.
    /// </summary>
    /// <param name="startsOn">Inclusive start date.</param>
    /// <param name="endsOn">Inclusive end date.</param>
    public void AddScheduledWindow(DateOnly startsOn, DateOnly endsOn)
    {
        var schedule = SubscriptionSchedule.CreateWindow(this.Id, startsOn, endsOn);
        this.schedules.Add(schedule);
    }

    /// <summary>
    /// Enables the subscription.
    /// </summary>
    public void Enable()
    {
        this.IsEnabled = true;
    }

    /// <summary>
    /// Disables the subscription.
    /// </summary>
    public void Disable()
    {
        this.IsEnabled = false;
    }

    /// <summary>
    /// Determines whether the subscription is active at the specified UTC instant.
    /// </summary>
    /// <param name="currentDate">Date to evaluate.</param>
    /// <returns><c>true</c> when enabled and at least one schedule is active; otherwise <c>false</c>.</returns>
    public bool IsActiveAt(DateOnly currentDate)
    {
        if (!this.IsEnabled)
        {
            return false;
        }

        return this.schedules.Any(schedule => schedule.IsActiveAt(currentDate));
    }

    private static string ValidateTimeZoneId(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new ArgumentException("Time zone id cannot be empty.", nameof(timeZoneId));
        }

        var normalized = timeZoneId.Trim();
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(normalized);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ArgumentException($"Unknown timezone id '{normalized}'.", nameof(timeZoneId));
        }
        catch (InvalidTimeZoneException)
        {
            throw new ArgumentException($"Invalid timezone id '{normalized}'.", nameof(timeZoneId));
        }

        return normalized;
    }
}
