namespace PersonalFinanceTracker.Models.Entities;

public class SavingsGoal
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public CurrencyCode Currency { get; set; } = CurrencyCode.BGN;
    public DateOnly TargetDate { get; set; }
    public SavingsGoalStatus Status { get; set; } = SavingsGoalStatus.active;
    public string? Description { get; set; }
    public int Priority { get; set; } = 3;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
}
