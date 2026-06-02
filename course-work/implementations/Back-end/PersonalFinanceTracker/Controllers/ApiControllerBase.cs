using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected long CurrentUserId =>
        long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : 0;

    protected bool IsAdmin => User.IsInRole(UserRole.admin.ToString());
}
