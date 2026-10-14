using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using JobFlow.Application.Common;

namespace JobFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected string ClientIpAddress =>
        HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    protected IActionResult OkResponse<T>(T data, string message = "Success") =>
        Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult CreatedResponse<T>(T data, string message = "Created") =>
        StatusCode(201, ApiResponse<T>.Ok(data, message));
}
