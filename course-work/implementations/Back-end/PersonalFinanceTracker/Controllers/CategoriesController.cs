using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Models.Dto;
using PersonalFinanceTracker.Models.Entities;

namespace PersonalFinanceTracker.Controllers;

[Authorize]
[Route("api/categories")]
public class CategoriesController(ApplicationDbContext dbContext) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<CategoryResponse>>> GetCategories(
        [FromQuery] string? searchQuery,
        [FromQuery] CategoryType? categoryType,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDirection = "asc")
    {
        var query = dbContext.Categories
            .AsNoTracking()
            .Where(category => category.UserId == CurrentUserId);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var search = searchQuery.Trim().ToLowerInvariant();
            query = query.Where(category => category.Name.ToLower().Contains(search));
        }

        if (categoryType.HasValue)
        {
            query = query.Where(category => category.CategoryType == categoryType.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(category => category.IsActive == isActive.Value);
        }

        query = ApplySort(query, sortBy, sortDirection);

        return Ok(await query.ToPagedResponseAsync(page, pageSize, category => category.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> CreateCategory(CategoryCreateRequest request)
    {
        var exists = await dbContext.Categories.AnyAsync(category =>
            category.UserId == CurrentUserId &&
            category.Name.ToLower() == request.Name.Trim().ToLower());
        if (exists)
        {
            return Conflict(new ProblemDetails { Title = "Category already exists", Status = StatusCodes.Status409Conflict });
        }

        var category = new Category
        {
            UserId = CurrentUserId,
            Name = request.Name.Trim(),
            CategoryType = request.CategoryType,
            Description = request.Description,
            ColorHex = request.ColorHex,
            IconName = request.IconName,
            IsActive = request.IsActive
        };

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category.ToResponse());
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CategoryResponse>> GetCategoryById(long id)
    {
        var category = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);

        return category is null ? NotFound() : Ok(category.ToResponse());
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(long id, CategoryUpdateRequest request)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);
        if (category is null)
        {
            return NotFound();
        }

        var exists = await dbContext.Categories.AnyAsync(item =>
            item.UserId == CurrentUserId &&
            item.Id != id &&
            item.Name.ToLower() == request.Name.Trim().ToLower());
        if (exists)
        {
            return Conflict(new ProblemDetails { Title = "Category already exists", Status = StatusCodes.Status409Conflict });
        }

        category.Name = request.Name.Trim();
        category.CategoryType = request.CategoryType;
        category.Description = request.Description;
        category.ColorHex = request.ColorHex;
        category.IconName = request.IconName;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return Ok(category.ToResponse());
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteCategory(long id)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(item => item.Id == id && item.UserId == CurrentUserId);
        if (category is null)
        {
            return NotFound();
        }

        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private static IQueryable<Category> ApplySort(IQueryable<Category> query, string sortBy, string sortDirection)
    {
        var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sortBy.ToLowerInvariant() switch
        {
            "categorytype" => descending ? query.OrderByDescending(category => category.CategoryType) : query.OrderBy(category => category.CategoryType),
            "createdat" => descending ? query.OrderByDescending(category => category.CreatedAt) : query.OrderBy(category => category.CreatedAt),
            _ => descending ? query.OrderByDescending(category => category.Name) : query.OrderBy(category => category.Name)
        };
    }
}
