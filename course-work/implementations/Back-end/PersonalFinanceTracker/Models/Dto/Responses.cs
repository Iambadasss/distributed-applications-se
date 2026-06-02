namespace PersonalFinanceTracker.Models.Dto;

public record AuthResponse(string AccessToken, string TokenType, DateTime ExpiresAt, UserResponse User);

public record UserResponse(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    CurrencyCode DefaultCurrency,
    bool IsActive,
    string Role,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CategoryResponse(
    long Id,
    long UserId,
    string Name,
    CategoryType CategoryType,
    string? Description,
    string? ColorHex,
    string? IconName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record FinancialRecordResponse(
    long Id,
    long UserId,
    long CategoryId,
    string CategoryName,
    RecordType RecordType,
    decimal Amount,
    CurrencyCode Currency,
    DateOnly RecordDate,
    string? Description,
    string? Note,
    bool IsRecurring,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record SubscriptionResponse(
    long Id,
    long UserId,
    long CategoryId,
    string CategoryName,
    string Name,
    decimal Amount,
    CurrencyCode Currency,
    RenewalCycle Cycle,
    int? CustomCycleDays,
    DateOnly StartDate,
    DateOnly NextDueDate,
    SubscriptionStatus Status,
    string? Description,
    bool IncludeInMonthlyForecast,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record BudgetResponse(
    long Id,
    long UserId,
    long CategoryId,
    string CategoryName,
    int Month,
    int Year,
    decimal LimitAmount,
    decimal SpentAmount,
    decimal RemainingAmount,
    CurrencyCode Currency,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record SavingsGoalResponse(
    long Id,
    long UserId,
    string Name,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal RemainingAmount,
    double ProgressPercent,
    CurrencyCode Currency,
    DateOnly TargetDate,
    SavingsGoalStatus Status,
    string? Description,
    int Priority,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record PaginationMetadata(
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage);

public record PagedResponse<T>(IReadOnlyList<T> Items, PaginationMetadata Pagination);

public record MonthlySummaryResponse(
    int Month,
    int Year,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal TotalExternalSubscriptions,
    decimal PlannedBudget,
    decimal AvailableAmount,
    decimal PotentialSavings,
    CurrencyCode Currency,
    DateTime GeneratedAt);

public record CategorySummaryItem(
    long CategoryId,
    string CategoryName,
    RecordType? RecordType,
    decimal TotalAmount,
    CurrencyCode Currency,
    int RecordsCount);
