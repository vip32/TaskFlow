namespace TaskFlow.Domain;

/// <summary>
/// Represents an activation schedule window for a subscription.
/// </summary>
public class SubscriptionSchedule
{
    /// <summary>
    /// Gets the unique identifier of the schedule.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the subscription identifier that owns this schedule.
    /// </summary>
    public Guid SubscriptionId { get; private set; }

    /// <summary>
    /// Gets the inclusive start date.
    /// </summary>
    public DateOnly StartsOn { get; private set; }

    /// <summary>
    /// Gets the inclusive end date.
    /// Null indicates an open-ended schedule.
    /// </summary>
    public DateOnly? EndsOn { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this schedule is open ended.
    /// </summary>
    public bool IsOpenEnded => !this.EndsOn.HasValue;

    private SubscriptionSchedule()
    {
    }

    /// <summary>
    /// Creates an open-ended schedule.
    /// </summary>
    /// <param name="subscriptionId">Owning subscription identifier.</param>
    /// <param name="startsOn">Inclusive start date.</param>
    /// <returns>A new open-ended schedule instance.</returns>
    public static SubscriptionSchedule CreateOpenEnded(Guid subscriptionId, DateOnly startsOn)
    {
        if (subscriptionId == Guid.Empty)
        {
            throw new ArgumentException("Subscription id cannot be empty.", nameof(subscriptionId));
        }

        return new SubscriptionSchedule
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscriptionId,
            StartsOn = startsOn,
            EndsOn = null,
        };
    }

    /// <summary>
    /// Creates a bounded schedule window.
    /// </summary>
    /// <param name="subscriptionId">Owning subscription identifier.</param>
    /// <param name="startsOn">Inclusive start date.</param>
    /// <param name="endsOn">Inclusive end date.</param>
    /// <returns>A new bounded schedule instance.</returns>
    public static SubscriptionSchedule CreateWindow(Guid subscriptionId, DateOnly startsOn, DateOnly endsOn)
    {
        if (subscriptionId == Guid.Empty)
        {
            throw new ArgumentException("Subscription id cannot be empty.", nameof(subscriptionId));
        }

        if (endsOn < startsOn)
        {
            throw new ArgumentException("Schedule end must be equal to or after schedule start.", nameof(endsOn));
        }

        return new SubscriptionSchedule
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscriptionId,
            StartsOn = startsOn,
            EndsOn = endsOn,
        };
    }

    /// <summary>
    /// Determines whether this schedule is active at the specified UTC instant.
    /// </summary>
    /// <param name="currentDate">Date to evaluate.</param>
    /// <returns><c>true</c> when the schedule is active; otherwise <c>false</c>.</returns>
    public bool IsActiveAt(DateOnly currentDate)
    {
        if (currentDate < this.StartsOn)
        {
            return false;
        }

        if (this.IsOpenEnded)
        {
            return true;
        }

        return currentDate <= this.EndsOn.Value;
    }
}
