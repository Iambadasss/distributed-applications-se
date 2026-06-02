using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models.Dto;
using PersonalFinanceTracker.Models.Entities;

namespace PersonalFinanceTracker.Controllers;

[Authorize]
[Route("api/users")]
public class UsersController(ApplicationDbContext dbContext) : ApiControllerBase
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    [HttpGet]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetUsers(
        [FromQuery] string? searchQuery,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortDirection = "desc")
    {
        var query = dbContext.Users.AsNoTracking().AsQueryable();

        if (!IsAdmin)
        {
            query = query.Where(user => user.Id == CurrentUserId);
        }

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var search = searchQuery.Trim().ToLowerInvariant();
            query = query.Where(user =>
                user.FirstName.ToLower().Contains(search) ||
                user.LastName.ToLower().Contains(search) ||
                user.Email.ToLower().Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(user => user.IsActive == isActive.Value);
        }

        query = ApplySort(query, sortBy, sortDirection);

        return Ok(await query.ToPagedResponseAsync(page, pageSize, user => user.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser(UserCreateRequest request)
    {
        if (!IsAdmin)
        {
            return Forbid();
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await dbContext.Users.AnyAsync(user => user.Email == email))
        {
            return Conflict(new ProblemDetails { Title = "Email already exists", Status = StatusCodes.Status409Conflict });
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            DefaultCurrency = request.DefaultCurrency,
            IsActive = request.IsActive,
            Role = request.Role
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user.ToResponse());
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserResponse>> GetUserById(long id)
    {
        if (!IsAdmin && id != CurrentUserId)
        {
            return Forbid();
        }

        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return user is null ? NotFound() : Ok(user.ToResponse());
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(long id, UserUpdateRequest request)
    {
        if (!IsAdmin && id != CurrentUserId)
        {
            return Forbid();
        }

        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var emailExists = await dbContext.Users.AnyAsync(item => item.Email == email && item.Id != id);
        if (emailExists)
        {
            return Conflict(new ProblemDetails { Title = "Email already exists", Status = StatusCodes.Status409Conflict });
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = email;
        user.DefaultCurrency = request.DefaultCurrency;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return Ok(user.ToResponse());
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        if (!IsAdmin && id != CurrentUserId)
        {
            return Forbid();
        }

        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private static IQueryable<User> ApplySort(IQueryable<User> query, string sortBy, string sortDirection)
    {
        var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sortBy.ToLowerInvariant() switch
        {
            "firstname" => descending ? query.OrderByDescending(user => user.FirstName) : query.OrderBy(user => user.FirstName),
            "lastname" => descending ? query.OrderByDescending(user => user.LastName) : query.OrderBy(user => user.LastName),
            "email" => descending ? query.OrderByDescending(user => user.Email) : query.OrderBy(user => user.Email),
            _ => descending ? query.OrderByDescending(user => user.CreatedAt) : query.OrderBy(user => user.CreatedAt)
        };
    }
}
