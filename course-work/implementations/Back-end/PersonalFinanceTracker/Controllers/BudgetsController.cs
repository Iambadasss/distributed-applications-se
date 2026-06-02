using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models.Dto;
using PersonalFinanceTracker.Models.Entities;
using RecordType = PersonalFinanceTracker.Models.RecordType;

namespace PersonalFinanceTracker.Controllers;

[Authorize]
[Route("api/budgets")]
public class BudgetsController(ApplicationDbContext dbContext) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<BudgetResponse>>> GetBudgets(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] long? categoryId,
        [FromQuery] bool? isActive,
        [FromQuery] decimal? minLimitAmount,
        [FromQuery] decimal? maxLimitAmount,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "year",
        [FromQuery] string sortDirection = "desc")
    {
        var query = dbContext.Budgets
            .AsNoTracking()
            .Include(budget => budget.Category)
            .Where(budget => budget.UserId == CurrentUserId);

        if (month.HasValue)
        {
            query = query.Where(budget => budget.Month == month.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(budget => budget.Year == year.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(budget => budget.CategoryId == categoryId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(budget => budget.IsActive == isActive.Value);
        }

        if (minLimitAmount.HasValue)
        {
            query = query.Where(budget => budget.LimitAmount >= minLimitAmount.Value);
        }

        if (maxLimitAmount.HasValue)
        {
            query = query.Where(budget => budget.LimitAmount <= maxLimitAmount.Value);
        }

        query = ApplySort(query, sortBy, sortDirection);
        return Ok(await ToBudgetPageAsync(query, page, pageSize));
    }

    [HttpPost]
    public async Task<ActionResult<BudgetResponse>> CreateBudget(BudgetCreateRequest request)
    {
        if (!await UserOwnsCategoryAsync(request.CategoryId))
        {
            return BadRequest(new ProblemDetails { Title = "Invalid category", Status = StatusCodes.Status400BadRequest });
        }

        var duplicate = await dbContext.Budgets.AnyAsync(budget =>
            budget.UserId == CurrentUserId &&
            budget.CategoryId == request.CategoryId &&
            budget.Month == request.Month &&
            budget.Year == request.Year);
        if (duplicate)
        {
            return Conflict(new ProblemDetails { Title = "Budget already exists", Status = StatusCodes.Status409Conflict });
        }

        var budget = new Budget
        {
            UserId = CurrentUserId,
            CategoryId = request.CategoryId,
            Month = request.Month,
            Year = request.Year,
            LimitAmount = request.LimitAmount,
            Currency = request.Currency,
            Description = request.Description,
            IsActive = request.IsActive
        };

        dbContext.Budgets.Add(budget);
        await dbContext.SaveChangesAsync();
        await dbContext.Entry(budget).Reference(item => item.Category).LoadAsync();

        var response = await ToBudgetResponseAsync(budget);
        return CreatedAtAction(nameof(GetBudgetById), new { id = budget.Id }, response);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<BudgetResponse>> GetBudgetById(long id)
    {
        var budget = await dbContext.Budgets
            .AsNoTracking()
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);

        return budget is null ? NotFound() : Ok(await ToBudgetResponseAsync(budget));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<BudgetResponse>> UpdateBudget(long id, BudgetUpdateRequest request)
    {
        var budget = await dbContext.Budgets
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);
        if (budget is null)
        {
            return NotFound();
        }

        if (!await UserOwnsCategoryAsync(request.CategoryId))
        {
            return BadRequest(new ProblemDetails { Title = "Invalid category", Status = StatusCodes.Status400BadRequest });
        }

        var duplicate = await dbContext.Budgets.AnyAsync(item =>
            item.UserId == CurrentUserId &&
            item.Id != id &&
            item.CategoryId == request.CategoryId &&
            item.Month == request.Month &&
            item.Year == request.Year);
        if (duplicate)
        {
            return Conflict(new ProblemDetails { Title = "Budget already exists", Status = StatusCodes.Status409Conflict });
        }

        budget.CategoryId = request.CategoryId;
        budget.Month = request.Month;
        budget.Year = request.Year;
        budget.LimitAmount = request.LimitAmount;
        budget.Currency = request.Currency;
        budget.Description = request.Description;
        budget.IsActive = request.IsActive;
        budget.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        await dbContext.Entry(budget).Reference(item => item.Category).LoadAsync();

        return Ok(await ToBudgetResponseAsync(budget));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteBudget(long id)
    {
        var budget = await dbContext.Budgets.FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);
        if (budget is null)
        {
            return NotFound();
        }

        dbContext.Budgets.Remove(budget);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<PagedResponse<BudgetResponse>> ToBudgetPageAsync(IQueryable<Budget> query, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalItems = await query.CountAsync();
        var budgets = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = new List<BudgetResponse>();
        foreach (var budget in budgets)
        {
            responses.Add(await ToBudgetResponseAsync(budget));
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        return new PagedResponse<BudgetResponse>(
            responses,
            new PaginationMetadata(page, pageSize, totalItems, totalPages, page > 1, page < totalPages));
    }

    private async Task<BudgetResponse> ToBudgetResponseAsync(Budget budget)
    {
        var startDate = new DateOnly(budget.Year, budget.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        var spentAmount = await dbContext.FinancialRecords
            .Where(record =>
                record.UserId == budget.UserId &&
                record.CategoryId == budget.CategoryId &&
                record.RecordType == RecordType.expense &&
                record.RecordDate >= startDate &&
                record.RecordDate <= endDate)
            .SumAsync(record => (decimal?)record.Amount) ?? 0m;

        return budget.ToResponse(spentAmount);
    }

    private async Task<bool> UserOwnsCategoryAsync(long categoryId) =>
        await dbContext.Categories.AnyAsync(category => category.Id == categoryId && category.UserId == CurrentUserId);

    private static IQueryable<Budget> ApplySort(IQueryable<Budget> query, string sortBy, string sortDirection)
    {
        var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sortBy.ToLowerInvariant() switch
        {
            "month" => descending ? query.OrderByDescending(budget => budget.Month) : query.OrderBy(budget => budget.Month),
            "limitamount" => descending ? query.OrderByDescending(budget => budget.LimitAmount) : query.OrderBy(budget => budget.LimitAmount),
            "categoryname" => descending ? query.OrderByDescending(budget => budget.Category!.Name) : query.OrderBy(budget => budget.Category!.Name),
            "createdat" => descending ? query.OrderByDescending(budget => budget.CreatedAt) : query.OrderBy(budget => budget.CreatedAt),
            _ => descending ? query.OrderByDescending(budget => budget.Year).ThenByDescending(budget => budget.Month) : query.OrderBy(budget => budget.Year).ThenBy(budget => budget.Month)
        };
    }
}
