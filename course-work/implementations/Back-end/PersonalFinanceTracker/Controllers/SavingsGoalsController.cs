using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Models.Dto;
using PersonalFinanceTracker.Models.Entities;

namespace PersonalFinanceTracker.Controllers;

[Authorize]
[Route("api/savings-goals")]
public class SavingsGoalsController(ApplicationDbContext dbContext) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<SavingsGoalResponse>>> GetSavingsGoals(
        [FromQuery] string? searchQuery,
        [FromQuery] SavingsGoalStatus? status,
        [FromQuery] DateOnly? targetDateFrom,
        [FromQuery] DateOnly? targetDateTo,
        [FromQuery] decimal? minTargetAmount,
        [FromQuery] decimal? maxTargetAmount,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "targetDate",
        [FromQuery] string sortDirection = "asc")
    {
        var query = dbContext.SavingsGoals
            .AsNoTracking()
            .Where(goal => goal.UserId == CurrentUserId);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var search = searchQuery.Trim().ToLowerInvariant();
            query = query.Where(goal =>
                goal.Name.ToLower().Contains(search) ||
                (goal.Description != null && goal.Description.ToLower().Contains(search)));
        }

        if (status.HasValue)
        {
            query = query.Where(goal => goal.Status == status.Value);
        }

        if (targetDateFrom.HasValue)
        {
            query = query.Where(goal => goal.TargetDate >= targetDateFrom.Value);
        }

        if (targetDateTo.HasValue)
        {
            query = query.Where(goal => goal.TargetDate <= targetDateTo.Value);
        }

        if (minTargetAmount.HasValue)
        {
            query = query.Where(goal => goal.TargetAmount >= minTargetAmount.Value);
        }

        if (maxTargetAmount.HasValue)
        {
            query = query.Where(goal => goal.TargetAmount <= maxTargetAmount.Value);
        }

        query = ApplySort(query, sortBy, sortDirection);

        return Ok(await query.ToPagedResponseAsync(page, pageSize, goal => goal.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<SavingsGoalResponse>> CreateSavingsGoal(SavingsGoalCreateRequest request)
    {
        if (request.CurrentAmount > request.TargetAmount)
        {
            return BadRequest(new ProblemDetails { Title = "Current amount cannot exceed target amount", Status = StatusCodes.Status400BadRequest });
        }

        var goal = new SavingsGoal
        {
            UserId = CurrentUserId,
            Name = request.Name.Trim(),
            TargetAmount = request.TargetAmount,
            CurrentAmount = request.CurrentAmount,
            Currency = request.Currency,
            TargetDate = request.TargetDate,
            Status = request.Status,
            Description = request.Description,
            Priority = request.Priority
        };

        dbContext.SavingsGoals.Add(goal);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSavingsGoalById), new { id = goal.Id }, goal.ToResponse());
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SavingsGoalResponse>> GetSavingsGoalById(long id)
    {
        var goal = await dbContext.SavingsGoals
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);

        return goal is null ? NotFound() : Ok(goal.ToResponse());
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<SavingsGoalResponse>> UpdateSavingsGoal(long id, SavingsGoalUpdateRequest request)
    {
        if (request.CurrentAmount > request.TargetAmount)
        {
            return BadRequest(new ProblemDetails { Title = "Current amount cannot exceed target amount", Status = StatusCodes.Status400BadRequest });
        }

        var goal = await dbContext.SavingsGoals.FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);
        if (goal is null)
        {
            return NotFound();
        }

        goal.Name = request.Name.Trim();
        goal.TargetAmount = request.TargetAmount;
        goal.CurrentAmount = request.CurrentAmount;
        goal.Currency = request.Currency;
        goal.TargetDate = request.TargetDate;
        goal.Status = request.Status;
        goal.Description = request.Description;
        goal.Priority = request.Priority;
        goal.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return Ok(goal.ToResponse());
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteSavingsGoal(long id)
    {
        var goal = await dbContext.SavingsGoals.FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);
        if (goal is null)
        {
            return NotFound();
        }

        dbContext.SavingsGoals.Remove(goal);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private static IQueryable<SavingsGoal> ApplySort(IQueryable<SavingsGoal> query, string sortBy, string sortDirection)
    {
        var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sortBy.ToLowerInvariant() switch
        {
            "name" => descending ? query.OrderByDescending(goal => goal.Name) : query.OrderBy(goal => goal.Name),
            "targetamount" => descending ? query.OrderByDescending(goal => goal.TargetAmount) : query.OrderBy(goal => goal.TargetAmount),
            "currentamount" => descending ? query.OrderByDescending(goal => goal.CurrentAmount) : query.OrderBy(goal => goal.CurrentAmount),
            "createdat" => descending ? query.OrderByDescending(goal => goal.CreatedAt) : query.OrderBy(goal => goal.CreatedAt),
            "status" => descending ? query.OrderByDescending(goal => goal.Status) : query.OrderBy(goal => goal.Status),
            _ => descending ? query.OrderByDescending(goal => goal.TargetDate) : query.OrderBy(goal => goal.TargetDate)
        };
    }
}
