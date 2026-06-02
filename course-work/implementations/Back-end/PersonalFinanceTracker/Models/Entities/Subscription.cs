namespace PersonalFinanceTracker.Models.Entities;

public class Subscription
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; } = CurrencyCode.BGN;
    public RenewalCycle Cycle { get; set; } = RenewalCycle.monthly;
    public int? CustomCycleDays { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly NextDueDate { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.active;
    public string? Description { get; set; }
    public bool IncludeInMonthlyForecast { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
    public Category? Category { get; set; }
}
