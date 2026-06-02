using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models.Dto;
using PersonalFinanceTracker.Models.Entities;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Controllers;

[Route("api/auth")]
public class AuthController(
    ApplicationDbContext dbContext,
    JwtTokenService jwtTokenService) : ApiControllerBase
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var emailExists = await dbContext.Users.AnyAsync(user => user.Email == email);
        if (emailExists)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Email already exists",
                Status = StatusCodes.Status409Conflict,
                Detail = "A user with this email already exists."
            });
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            DefaultCurrency = request.DefaultCurrency,
            IsActive = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, jwtTokenService.CreateAuthResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(item => item.Email == email && item.IsActive);
        if (user is null)
        {
            return Unauthorized();
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        return Ok(jwtTokenService.CreateAuthResponse(user));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me()
    {
        var user = await dbContext.Users.FindAsync(CurrentUserId);
        return user is null ? Unauthorized() : Ok(user.ToResponse());
    }
}
