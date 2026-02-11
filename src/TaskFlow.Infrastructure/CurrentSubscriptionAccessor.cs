using Microsoft.Extensions.Configuration;
using TaskFlow.Domain;

namespace TaskFlow.Infrastructure;

/// <summary>
/// Provides the current subscription context for infrastructure operations.
/// </summary>
[RegisterScoped(ServiceType = typeof(ICurrentSubscriptionAccessor))]
public sealed class CurrentSubscriptionAccessor : ICurrentSubscriptionAccessor
{
    private const string DEFAULT_SUBSCRIPTION_ID = "00000000-0000-0000-0000-000000000001";
    private const string DEFAULT_TIME_ZONE_ID = "Europe/Berlin";
    private readonly Subscription subscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentSubscriptionAccessor"/> class.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    public CurrentSubscriptionAccessor(IConfiguration configuration)
    {
        var configuredId = configuration["Subscription:CurrentSubscriptionId"];
        var subscriptionId = Guid.Parse(DEFAULT_SUBSCRIPTION_ID);
        if (!string.IsNullOrWhiteSpace(configuredId) && Guid.TryParse(configuredId, out var parsedId))
        {
            subscriptionId = parsedId;
        }

        var name = configuration["Subscription:CurrentSubscriptionName"];
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Default";
        }

        var tier = SubscriptionTier.Free;
        var configuredTier = configuration["Subscription:CurrentSubscriptionTier"];
        if (!string.IsNullOrWhiteSpace(configuredTier))
        {
            Enum.TryParse(configuredTier, true, out tier);
        }

        var isEnabled = true;
        var configuredEnabled = configuration["Subscription:IsEnabled"];
        if (!string.IsNullOrWhiteSpace(configuredEnabled) && bool.TryParse(configuredEnabled, out var parsedEnabled))
        {
            isEnabled = parsedEnabled;
        }

        var timeZoneId = configuration["Subscription:TimeZoneId"];
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            timeZoneId = DEFAULT_TIME_ZONE_ID;
        }

        this.subscription = new Subscription(subscriptionId, name, tier, isEnabled, timeZoneId);

        var startsOn = DateOnly.FromDateTime(DateTime.UtcNow);
        var configuredStartsOn = configuration["Subscription:Schedule:StartsOn"];
        if (!string.IsNullOrWhiteSpace(configuredStartsOn) && DateOnly.TryParse(configuredStartsOn, out var parsedStartsOn))
        {
            startsOn = parsedStartsOn;
        }

        var configuredEndsOn = configuration["Subscription:Schedule:EndsOn"];
        if (string.IsNullOrWhiteSpace(configuredEndsOn))
        {
            this.subscription.AddOpenEndedSchedule(startsOn);
        }
        else if (DateOnly.TryParse(configuredEndsOn, out var parsedEndsOn))
        {
            this.subscription.AddScheduledWindow(startsOn, parsedEndsOn);
        }
        else
        {
            this.subscription.AddOpenEndedSchedule(startsOn);
        }
    }

    /// <inheritdoc/>
    public Subscription GetCurrentSubscription()
    {
        return this.subscription;
    }
}
