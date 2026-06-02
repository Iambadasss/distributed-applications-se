using PersonalFinanceTracker.Models.Dto;
using PersonalFinanceTracker.Models.Entities;

namespace PersonalFinanceTracker;

public static class MappingExtensions
{
    public static UserResponse ToResponse(this User user) =>
        new(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.DefaultCurrency,
            user.IsActive,
            user.Role.ToString(),
            user.CreatedAt,
            user.UpdatedAt);

    public static CategoryResponse ToResponse(this Category category) =>
        new(
            category.Id,
            category.UserId,
            category.Name,
            category.CategoryType,
            category.Description,
            category.ColorHex,
            category.IconName,
            category.IsActive,
            category.CreatedAt,
            category.UpdatedAt);

    public static FinancialRecordResponse ToResponse(this FinancialRecord record) =>
        new(
            record.Id,
            record.UserId,
            record.CategoryId,
            record.Category?.Name ?? string.Empty,
            record.RecordType,
            record.Amount,
            record.Currency,
            record.RecordDate,
            record.Description,
            record.Note,
            record.IsRecurring,
            record.CreatedAt,
            record.UpdatedAt);

    public static SubscriptionResponse ToResponse(this Subscription subscription) =>
        new(
            subscription.Id,
            subscription.UserId,
            subscription.CategoryId,
            subscription.Category?.Name ?? string.Empty,
            subscription.Name,
            subscription.Amount,
            subscription.Currency,
            subscription.Cycle,
            subscription.CustomCycleDays,
            subscription.StartDate,
            subscription.NextDueDate,
            subscription.Status,
            subscription.Description,
            subscription.IncludeInMonthlyForecast,
            subscription.CreatedAt,
            subscription.UpdatedAt);

    public static BudgetResponse ToResponse(this Budget budget, decimal spentAmount)
    {
        var remainingAmount = budget.LimitAmount - spentAmount;

        return new BudgetResponse(
            budget.Id,
            budget.UserId,
            budget.CategoryId,
            budget.Category?.Name ?? string.Empty,
            budget.Month,
            budget.Year,
            budget.LimitAmount,
            spentAmount,
            remainingAmount,
            budget.Currency,
            budget.Description,
            budget.IsActive,
            budget.CreatedAt,
            budget.UpdatedAt);
    }

    public static SavingsGoalResponse ToResponse(this SavingsGoal goal)
    {
        var remainingAmount = Math.Max(0, goal.TargetAmount - goal.CurrentAmount);
        var progressPercent = goal.TargetAmount <= 0
            ? 0
            : Math.Min(100, (double)(goal.CurrentAmount / goal.TargetAmount * 100m));

        return new SavingsGoalResponse(
            goal.Id,
            goal.UserId,
            goal.Name,
            goal.TargetAmount,
            goal.CurrentAmount,
            remainingAmount,
            Math.Round(progressPercent, 2),
            goal.Currency,
            goal.TargetDate,
            goal.Status,
            goal.Description,
            goal.Priority,
            goal.CreatedAt,
            goal.UpdatedAt);
    }
}
