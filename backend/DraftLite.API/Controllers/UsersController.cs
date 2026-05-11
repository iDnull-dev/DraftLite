using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DraftLite.DTO;
using DraftLite.Service.Interfaces;
using DraftLite.Data.Entities;

namespace DraftLite.Api.Controllers;

[Route("users")]
public sealed class UsersController : BaseController
{
    private readonly IUserService _users;
    private readonly ILogger<UsersController> _logger;
    private readonly IAppMapper _mapper;

    public UsersController(IUserService users, ILogger<UsersController> logger, IAppMapper mapper) : base(logger)
    {
        _users = users;
        _logger = logger;
        _mapper = mapper;
    }

    // README: POST /users/register (Anonymous)
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        var user = await _users.RegisterAsync(request, ct);

        var userDto = _mapper.Map<User, UserDto>(user);

        return Ok(userDto);
    }

    // README: GET /users/ (JWT)
    [HttpGet("")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(CurrentUserId)) return Forbid();
        try{
            var user = await _users.GetMeAsync(CurrentUserId, ct);
            var userDto = _mapper.Map<User, UserDto>(user);

            return Ok(userDto);
        } catch (KeyNotFoundException ex) {
            return NotFound(ex.Message);
        }
        catch (Exception ex) {
            return StatusCode(500, ex.Message);
        }
    }

    // README: GET /users/{searchName} (JWT)
    [HttpGet("{searchName}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> Search([FromRoute] string searchName, CancellationToken ct)
    {
        var users = await _users.SearchAsync(searchName, ct: ct);
        var userDtos = _mapper.Map<List<User>, List<UserDto>>(users.ToList());

        return Ok(userDtos);
    }

    // README: PUT /users/ (JWT)
    [HttpPut("")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateMeRequest request, CancellationToken ct)
    {
        if (CurrentUserId is null) return Unauthorized();
        var user = await _users.UpdateMeAsync(CurrentUserId, request, ct);
        var userDto = _mapper.Map<User, UserDto>(user);

        return Ok(userDto);
    }

    [HttpGet("theme")]
    [Authorize]
    public async Task<ActionResult<object>> GetTheme(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(googleId)) return Unauthorized();
        var theme = await _users.GetThemeAsync(googleId, ct);
        return Ok(new { theme });
    }

    [HttpPut("theme")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateTheme([FromBody] UpdateThemeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(googleId)) return Unauthorized();
        var user = await _users.UpdateThemeAsync(googleId, request, ct);
        var userDto = _mapper.Map<User, UserDto>(user);

        return Ok(userDto);
    }

    // README: PUT /users/{id} (Admin)
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserDto>> AdminUpdate([FromRoute] Guid id, [FromBody] AdminUpdateUserRequest request, CancellationToken ct)
    {
        var user = await _users.AdminUpdateAsync(id, request, ct);
        var userDto = _mapper.Map<User, UserDto>(user);

        return Ok(userDto);
    }

    // README: DELETE /users/{id} (Admin)
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        await _users.DeleteAsync(id, ct);
        return NoContent();
    }
}
