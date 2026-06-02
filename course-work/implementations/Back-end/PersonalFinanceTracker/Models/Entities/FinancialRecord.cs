namespace PersonalFinanceTracker.Models.Entities;

public class FinancialRecord
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long CategoryId { get; set; }
    public RecordType RecordType { get; set; }
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; } = CurrencyCode.BGN;
    public DateOnly RecordDate { get; set; }
    public string? Description { get; set; }
    public string? Note { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
    public Category? Category { get; set; }
}
