using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Models.Dto;

namespace PersonalFinanceTracker.Controllers;

[Authorize]
[Route("api/reports")]
public class ReportsController(ApplicationDbContext dbContext) : ApiControllerBase
{
    [HttpGet("monthly-summary")]
    public async Task<ActionResult<MonthlySummaryResponse>> GetMonthlySummary(
        [FromQuery] int month,
        [FromQuery] int year,
        [FromQuery] long? categoryId)
    {
        if (month is < 1 or > 12 || year is < 2000 or > 2100)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid month or year", Status = StatusCodes.Status400BadRequest });
        }

        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        var records = dbContext.FinancialRecords
            .Where(record =>
                record.UserId == CurrentUserId &&
                record.RecordDate >= startDate &&
                record.RecordDate <= endDate);

        if (categoryId.HasValue)
        {
            records = records.Where(record => record.CategoryId == categoryId.Value);
        }

        var totalIncome = await records
            .Where(record => record.RecordType == RecordType.income)
            .SumAsync(record => (decimal?)record.Amount) ?? 0m;

        var totalExpenses = await records
            .Where(record => record.RecordType == RecordType.expense)
            .SumAsync(record => (decimal?)record.Amount) ?? 0m;

        var totalExternalSubscriptions = await dbContext.Subscriptions
            .Where(subscription =>
                subscription.UserId == CurrentUserId &&
                subscription.Status == SubscriptionStatus.active &&
                subscription.IncludeInMonthlyForecast &&
                subscription.NextDueDate >= startDate &&
                subscription.NextDueDate <= endDate)
            .SumAsync(subscription => (decimal?)subscription.Amount) ?? 0m;

        var plannedBudget = await dbContext.Budgets
            .Where(budget =>
                budget.UserId == CurrentUserId &&
                budget.Month == month &&
                budget.Year == year)
            .SumAsync(budget => (decimal?)budget.LimitAmount) ?? 0m;

        var userCurrency = await dbContext.Users
            .Where(user => user.Id == CurrentUserId)
            .Select(user => user.DefaultCurrency)
            .FirstOrDefaultAsync();

        var availableAmount = totalIncome - totalExpenses - totalExternalSubscriptions;
        var potentialSavings = Math.Max(0, availableAmount);

        return Ok(new MonthlySummaryResponse(
            month,
            year,
            totalIncome,
            totalExpenses,
            totalExternalSubscriptions,
            plannedBudget,
            availableAmount,
            potentialSavings,
            userCurrency,
            DateTime.UtcNow));
    }

    [HttpGet("category-summary")]
    public async Task<ActionResult<IReadOnlyList<CategorySummaryItem>>> GetCategorySummary(
        [FromQuery] RecordType? recordType,
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] long? categoryId,
        [FromQuery] string sortDirection = "desc")
    {
        if (fromDate > toDate)
        {
            return BadRequest(new ProblemDetails { Title = "fromDate must be before toDate", Status = StatusCodes.Status400BadRequest });
        }

        var query = dbContext.FinancialRecords
            .AsNoTracking()
            .Include(record => record.Category)
            .Where(record =>
                record.UserId == CurrentUserId &&
                record.RecordDate >= fromDate &&
                record.RecordDate <= toDate);

        if (recordType.HasValue)
        {
            query = query.Where(record => record.RecordType == recordType.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(record => record.CategoryId == categoryId.Value);
        }

        var items = await query
            .GroupBy(record => new
            {
                record.CategoryId,
                CategoryName = record.Category!.Name,
                record.RecordType,
                record.Currency
            })
            .Select(group => new CategorySummaryItem(
                group.Key.CategoryId,
                group.Key.CategoryName,
                group.Key.RecordType,
                group.Sum(record => record.Amount),
                group.Key.Currency,
                group.Count()))
            .ToListAsync();

        var sorted = sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
            ? items.OrderBy(item => item.TotalAmount).ToList()
            : items.OrderByDescending(item => item.TotalAmount).ToList();

        return Ok(sorted);
    }

    [HttpGet("savings-progress")]
    public async Task<ActionResult<PagedResponse<SavingsGoalResponse>>> GetSavingsProgress(
        [FromQuery] SavingsGoalStatus? status,
        [FromQuery] DateOnly? targetDateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = dbContext.SavingsGoals
            .AsNoTracking()
            .Where(goal => goal.UserId == CurrentUserId);

        if (status.HasValue)
        {
            query = query.Where(goal => goal.Status == status.Value);
        }

        if (targetDateTo.HasValue)
        {
            query = query.Where(goal => goal.TargetDate <= targetDateTo.Value);
        }

        query = query.OrderBy(goal => goal.TargetDate);
        return Ok(await query.ToPagedResponseAsync(page, pageSize, goal => goal.ToResponse()));
    }
}
