using GarmentsERP.Auth.Application.Commands.Login;
using GarmentsERP.Auth.Application.Commands.Logout;
using GarmentsERP.Auth.Application.Commands.RefreshToken;
using GarmentsERP.Auth.Application.Commands.RegisterUser;
using GarmentsERP.Auth.Application.DTOs;
using GarmentsERP.Auth.Application.Queries.GetCurrentUser;
using GarmentsERP.Auth.Application.Queries.GetMenuItems;
using GarmentsERP.Auth.Application.Queries.GetUsers;
using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Constants;
using GarmentsERP.Shared.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(ISender sender, ICurrentUserService currentUserService)
    {
        _sender = sender;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Authenticate user with credentials and return JWT Access + Refresh tokens, permissions, and allowed menus.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var deviceInfo = Request.Headers.UserAgent.ToString();

        var command = new LoginCommand(
            request.UserNameOrEmail,
            request.Password,
            request.RememberMe,
            ipAddress,
            deviceInfo);

        var result = await _sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Authentication failed."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result.Value!, "Login successful."));
    }

    /// <summary>
    /// Refresh an expired access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken);
        var result = await _sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Failed to refresh token."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result.Value!, "Token refreshed successfully."));
    }

    /// <summary>
    /// Revoke current refresh token and log user out.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        var command = new LogoutCommand(request.RefreshToken);
        var result = await _sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Logout failed."));

        return Ok(ApiResponse<string>.Ok("Logged out successfully."));
    }

    /// <summary>
    /// Get details of the currently logged-in user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var query = new GetCurrentUserQuery(_currentUserService.UserId);
        var result = await _sender.Send(query, ct);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.Fail(result.Error ?? "User profile not found."));

        return Ok(ApiResponse<UserDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Get dynamic role/permission-based navigation menu for the current user.
    /// </summary>
    [HttpGet("menu")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MenuItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMenu(CancellationToken ct)
    {
        var query = new GetMenuItemsQuery(_currentUserService.UserId);
        var result = await _sender.Send(query, ct);

        return Ok(ApiResponse<IReadOnlyList<MenuItemDto>>.Ok(result.Value ?? []));
    }

    /// <summary>
    /// Register a new system user with assigned roles (Admin/Authorized HR users).
    /// </summary>
    [HttpPost("register")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request, CancellationToken ct)
    {
        if (!_currentUserService.HasPermission($"{AppConstants.Modules.Auth}.Create"))
            return Forbid();

        var command = new RegisterUserCommand(
            request.FullName,
            request.UserName,
            request.Email,
            request.Password,
            request.Phone,
            request.CompanyId == Guid.Empty ? _currentUserService.CompanyId : request.CompanyId,
            request.RoleIds,
            _currentUserService.UserId);

        var result = await _sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Registration failed."));

        return Ok(ApiResponse<Guid>.Ok(result.Value, "User registered successfully."));
    }

    /// <summary>
    /// Query paginated users list within company tenant.
    /// </summary>
    [HttpGet("users")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PagedList<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        if (!_currentUserService.HasPermission($"{AppConstants.Modules.Auth}.Read"))
            return Forbid();

        var query = new GetUsersQuery(_currentUserService.CompanyId, page, pageSize, search);
        var result = await _sender.Send(query, ct);

        return Ok(ApiResponse<PagedList<UserDto>>.Ok(result.Value!));
    }
}

public record RefreshTokenRequest(string AccessToken, string RefreshToken);
public record LogoutRequest(string RefreshToken);
