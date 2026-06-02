using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Models.Dto;
using PersonalFinanceTracker.Models.Entities;

namespace PersonalFinanceTracker.Controllers;

[Authorize]
[Route("api/subscriptions")]
public class SubscriptionsController(ApplicationDbContext dbContext) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<SubscriptionResponse>>> GetSubscriptions(
        [FromQuery] string? searchQuery,
        [FromQuery] long? categoryId,
        [FromQuery] SubscriptionStatus? status,
        [FromQuery] RenewalCycle? cycle,
        [FromQuery] DateOnly? nextDueFrom,
        [FromQuery] DateOnly? nextDueTo,
        [FromQuery] decimal? minAmount,
        [FromQuery] decimal? maxAmount,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "nextDueDate",
        [FromQuery] string sortDirection = "asc")
    {
        var query = BuildSubscriptionQuery(searchQuery, categoryId, status, cycle, nextDueFrom, nextDueTo, minAmount, maxAmount);
        query = ApplySort(query, sortBy, sortDirection);

        return Ok(await query.ToPagedResponseAsync(page, pageSize, subscription => subscription.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<SubscriptionResponse>> CreateSubscription(SubscriptionCreateRequest request)
    {
        if (!await UserOwnsCategoryAsync(request.CategoryId))
        {
            return BadRequest(new ProblemDetails { Title = "Invalid category", Status = StatusCodes.Status400BadRequest });
        }

        var subscription = new Subscription
        {
            UserId = CurrentUserId,
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Amount = request.Amount,
            Currency = request.Currency,
            Cycle = request.Cycle,
            CustomCycleDays = request.CustomCycleDays,
            StartDate = request.StartDate,
            NextDueDate = request.NextDueDate,
            Status = request.Status,
            Description = request.Description,
            IncludeInMonthlyForecast = request.IncludeInMonthlyForecast
        };

        dbContext.Subscriptions.Add(subscription);
        await dbContext.SaveChangesAsync();
        await dbContext.Entry(subscription).Reference(item => item.Category).LoadAsync();

        return CreatedAtAction(nameof(GetSubscriptionById), new { id = subscription.Id }, subscription.ToResponse());
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SubscriptionResponse>> GetSubscriptionById(long id)
    {
        var subscription = await dbContext.Subscriptions
            .AsNoTracking()
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);

        return subscription is null ? NotFound() : Ok(subscription.ToResponse());
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<SubscriptionResponse>> UpdateSubscription(long id, SubscriptionUpdateRequest request)
    {
        var subscription = await dbContext.Subscriptions
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);
        if (subscription is null)
        {
            return NotFound();
        }

        if (!await UserOwnsCategoryAsync(request.CategoryId))
        {
            return BadRequest(new ProblemDetails { Title = "Invalid category", Status = StatusCodes.Status400BadRequest });
        }

        subscription.CategoryId = request.CategoryId;
        subscription.Name = request.Name.Trim();
        subscription.Amount = request.Amount;
        subscription.Currency = request.Currency;
        subscription.Cycle = request.Cycle;
        subscription.CustomCycleDays = request.CustomCycleDays;
        subscription.StartDate = request.StartDate;
        subscription.NextDueDate = request.NextDueDate;
        subscription.Status = request.Status;
        subscription.Description = request.Description;
        subscription.IncludeInMonthlyForecast = request.IncludeInMonthlyForecast;
        subscription.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        await dbContext.Entry(subscription).Reference(item => item.Category).LoadAsync();

        return Ok(subscription.ToResponse());
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteSubscription(long id)
    {
        var subscription = await dbContext.Subscriptions.FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);
        if (subscription is null)
        {
            return NotFound();
        }

        dbContext.Subscriptions.Remove(subscription);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private IQueryable<Subscription> BuildSubscriptionQuery(
        string? searchQuery,
        long? categoryId,
        SubscriptionStatus? status,
        RenewalCycle? cycle,
        DateOnly? nextDueFrom,
        DateOnly? nextDueTo,
        decimal? minAmount,
        decimal? maxAmount)
    {
        var query = dbContext.Subscriptions
            .AsNoTracking()
            .Include(subscription => subscription.Category)
            .Where(subscription => subscription.UserId == CurrentUserId);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var search = searchQuery.Trim().ToLowerInvariant();
            query = query.Where(subscription =>
                subscription.Name.ToLower().Contains(search) ||
                (subscription.Description != null && subscription.Description.ToLower().Contains(search)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(subscription => subscription.CategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(subscription => subscription.Status == status.Value);
        }

        if (cycle.HasValue)
        {
            query = query.Where(subscription => subscription.Cycle == cycle.Value);
        }

        if (nextDueFrom.HasValue)
        {
            query = query.Where(subscription => subscription.NextDueDate >= nextDueFrom.Value);
        }

        if (nextDueTo.HasValue)
        {
            query = query.Where(subscription => subscription.NextDueDate <= nextDueTo.Value);
        }

        if (minAmount.HasValue)
        {
            query = query.Where(subscription => subscription.Amount >= minAmount.Value);
        }

        if (maxAmount.HasValue)
        {
            query = query.Where(subscription => subscription.Amount <= maxAmount.Value);
        }

        return query;
    }

    private async Task<bool> UserOwnsCategoryAsync(long categoryId) =>
        await dbContext.Categories.AnyAsync(category => category.Id == categoryId && category.UserId == CurrentUserId);

    private static IQueryable<Subscription> ApplySort(IQueryable<Subscription> query, string sortBy, string sortDirection)
    {
        var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sortBy.ToLowerInvariant() switch
        {
            "name" => descending ? query.OrderByDescending(subscription => subscription.Name) : query.OrderBy(subscription => subscription.Name),
            "amount" => descending ? query.OrderByDescending(subscription => subscription.Amount) : query.OrderBy(subscription => subscription.Amount),
            "createdat" => descending ? query.OrderByDescending(subscription => subscription.CreatedAt) : query.OrderBy(subscription => subscription.CreatedAt),
            "status" => descending ? query.OrderByDescending(subscription => subscription.Status) : query.OrderBy(subscription => subscription.Status),
            _ => descending ? query.OrderByDescending(subscription => subscription.NextDueDate) : query.OrderBy(subscription => subscription.NextDueDate)
        };
    }
}
