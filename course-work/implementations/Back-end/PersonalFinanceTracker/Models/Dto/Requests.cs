using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.Models.Dto;

public class RegisterRequest
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    public CurrencyCode DefaultCurrency { get; set; } = CurrencyCode.BGN;
}

public class LoginRequest
{
    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

public class UserCreateRequest : RegisterRequest
{
    public UserRole Role { get; set; } = UserRole.user;
    public bool IsActive { get; set; } = true;
}

public class UserUpdateRequest
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    public CurrencyCode DefaultCurrency { get; set; } = CurrencyCode.BGN;
    public bool IsActive { get; set; } = true;
}

public class CategoryCreateRequest
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    public CategoryType CategoryType { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    [RegularExpression("^#[0-9A-Fa-f]{6}$"), StringLength(7)]
    public string? ColorHex { get; set; }

    [StringLength(50)]
    public string? IconName { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CategoryUpdateRequest : CategoryCreateRequest;

public class FinancialRecordCreateRequest
{
    [Range(1, long.MaxValue)]
    public long CategoryId { get; set; }

    public RecordType RecordType { get; set; }

    [Range(typeof(decimal), "0.01", "100000000")]
    public decimal Amount { get; set; }

    public CurrencyCode Currency { get; set; } = CurrencyCode.BGN;
    public DateOnly RecordDate { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }

    public bool IsRecurring { get; set; }
}

public class FinancialRecordUpdateRequest : FinancialRecordCreateRequest;

public class SubscriptionCreateRequest
{
    [Range(1, long.MaxValue)]
    public long CategoryId { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "100000000")]
    public decimal Amount { get; set; }

    public CurrencyCode Currency { get; set; } = CurrencyCode.BGN;
    public RenewalCycle Cycle { get; set; } = RenewalCycle.monthly;

    [Range(1, 365)]
    public int? CustomCycleDays { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly NextDueDate { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.active;

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IncludeInMonthlyForecast { get; set; } = true;
}

public class SubscriptionUpdateRequest : SubscriptionCreateRequest;

public class BudgetCreateRequest
{
    [Range(1, long.MaxValue)]
    public long CategoryId { get; set; }

    [Range(1, 12)]
    public int Month { get; set; }

    [Range(2000, 2100)]
    public int Year { get; set; }

    [Range(typeof(decimal), "0.01", "100000000")]
    public decimal LimitAmount { get; set; }

    public CurrencyCode Currency { get; set; } = CurrencyCode.BGN;

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public class BudgetUpdateRequest : BudgetCreateRequest;

public class SavingsGoalCreateRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal TargetAmount { get; set; }

    [Range(typeof(decimal), "0", "1000000000")]
    public decimal CurrentAmount { get; set; }

    public CurrencyCode Currency { get; set; } = CurrencyCode.BGN;
    public DateOnly TargetDate { get; set; }
    public SavingsGoalStatus Status { get; set; } = SavingsGoalStatus.active;

    [StringLength(255)]
    public string? Description { get; set; }

    [Range(1, 5)]
    public int Priority { get; set; } = 3;
}

public class SavingsGoalUpdateRequest : SavingsGoalCreateRequest;
