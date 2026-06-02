namespace PersonalFinanceTracker.Models.Entities;

public class Category
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType CategoryType { get; set; }
    public string? Description { get; set; }
    public string? ColorHex { get; set; }
    public string? IconName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
    public ICollection<FinancialRecord> FinancialRecords { get; set; } = new List<FinancialRecord>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
