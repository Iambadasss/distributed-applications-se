using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Models.Dto;
using PersonalFinanceTracker.Models.Entities;

namespace PersonalFinanceTracker.Controllers;

[Authorize]
[Route("api/financial-records")]
public class FinancialRecordsController(ApplicationDbContext dbContext) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<FinancialRecordResponse>>> GetFinancialRecords(
        [FromQuery] string? searchQuery,
        [FromQuery] long? categoryId,
        [FromQuery] RecordType? recordType,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] decimal? minAmount,
        [FromQuery] decimal? maxAmount,
        [FromQuery] CurrencyCode? currency,
        [FromQuery] bool? isRecurring,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "recordDate",
        [FromQuery] string sortDirection = "desc")
    {
        var query = dbContext.FinancialRecords
            .AsNoTracking()
            .Include(record => record.Category)
            .Where(record => record.UserId == CurrentUserId);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var search = searchQuery.Trim().ToLowerInvariant();
            query = query.Where(record =>
                (record.Description != null && record.Description.ToLower().Contains(search)) ||
                (record.Note != null && record.Note.ToLower().Contains(search)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(record => record.CategoryId == categoryId.Value);
        }

        if (recordType.HasValue)
        {
            query = query.Where(record => record.RecordType == recordType.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(record => record.RecordDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(record => record.RecordDate <= toDate.Value);
        }

        if (minAmount.HasValue)
        {
            query = query.Where(record => record.Amount >= minAmount.Value);
        }

        if (maxAmount.HasValue)
        {
            query = query.Where(record => record.Amount <= maxAmount.Value);
        }

        if (currency.HasValue)
        {
            query = query.Where(record => record.Currency == currency.Value);
        }

        if (isRecurring.HasValue)
        {
            query = query.Where(record => record.IsRecurring == isRecurring.Value);
        }

        query = ApplySort(query, sortBy, sortDirection);

        return Ok(await query.ToPagedResponseAsync(page, pageSize, record => record.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<FinancialRecordResponse>> CreateFinancialRecord(FinancialRecordCreateRequest request)
    {
        if (!await UserOwnsCategoryAsync(request.CategoryId))
        {
            return BadRequest(new ProblemDetails { Title = "Invalid category", Status = StatusCodes.Status400BadRequest });
        }

        var record = new FinancialRecord
        {
            UserId = CurrentUserId,
            CategoryId = request.CategoryId,
            RecordType = request.RecordType,
            Amount = request.Amount,
            Currency = request.Currency,
            RecordDate = request.RecordDate,
            Description = request.Description,
            Note = request.Note,
            IsRecurring = request.IsRecurring
        };

        dbContext.FinancialRecords.Add(record);
        await dbContext.SaveChangesAsync();
        await dbContext.Entry(record).Reference(item => item.Category).LoadAsync();

        return CreatedAtAction(nameof(GetFinancialRecordById), new { id = record.Id }, record.ToResponse());
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<FinancialRecordResponse>> GetFinancialRecordById(long id)
    {
        var record = await dbContext.FinancialRecords
            .AsNoTracking()
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);

        return record is null ? NotFound() : Ok(record.ToResponse());
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<FinancialRecordResponse>> UpdateFinancialRecord(long id, FinancialRecordUpdateRequest request)
    {
        var record = await dbContext.FinancialRecords
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);
        if (record is null)
        {
            return NotFound();
        }

        if (!await UserOwnsCategoryAsync(request.CategoryId))
        {
            return BadRequest(new ProblemDetails { Title = "Invalid category", Status = StatusCodes.Status400BadRequest });
        }

        record.CategoryId = request.CategoryId;
        record.RecordType = request.RecordType;
        record.Amount = request.Amount;
        record.Currency = request.Currency;
        record.RecordDate = request.RecordDate;
        record.Description = request.Description;
        record.Note = request.Note;
        record.IsRecurring = request.IsRecurring;
        record.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        await dbContext.Entry(record).Reference(item => item.Category).LoadAsync();

        return Ok(record.ToResponse());
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteFinancialRecord(long id)
    {
        var record = await dbContext.FinancialRecords.FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);
        if (record is null)
        {
            return NotFound();
        }

        dbContext.FinancialRecords.Remove(record);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> UserOwnsCategoryAsync(long categoryId) =>
        await dbContext.Categories.AnyAsync(category => category.Id == categoryId && category.UserId == CurrentUserId);

    private static IQueryable<FinancialRecord> ApplySort(IQueryable<FinancialRecord> query, string sortBy, string sortDirection)
    {
        var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sortBy.ToLowerInvariant() switch
        {
            "amount" => descending ? query.OrderByDescending(record => record.Amount) : query.OrderBy(record => record.Amount),
            "categoryname" => descending ? query.OrderByDescending(record => record.Category!.Name) : query.OrderBy(record => record.Category!.Name),
            "createdat" => descending ? query.OrderByDescending(record => record.CreatedAt) : query.OrderBy(record => record.CreatedAt),
            _ => descending ? query.OrderByDescending(record => record.RecordDate) : query.OrderBy(record => record.RecordDate)
        };
    }
}
