using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DraftLite.DTO;
using DraftLite.Service.Interfaces;

namespace DraftLite.Api.Controllers;

[Route("projects")]
[Authorize]
public sealed class ProjectsController : BaseController
{
    private readonly IProjectService _projects;

    public ProjectsController(IProjectService projects, ILogger<ProjectsController> logger) : base(logger)
    {
        _projects = projects;
    }

    [HttpGet("")]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> List(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return Unauthorized();
        var dtos = await _projects.ListAsync(CurrentUserId, ct);
        return Ok(dtos);
    }

    [HttpPost("")]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return Unauthorized();
        var dto = await _projects.CreateAsync(CurrentUserId, request, ct);
        return Ok(dto);
    }

    [HttpPut("{projectId:guid}")]
    public async Task<ActionResult<ProjectDto>> Update([FromRoute] Guid projectId, [FromBody] UpdateProjectRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return Unauthorized();
        var dto = await _projects.UpdateAsync(CurrentUserId, projectId, request, ct);
        return Ok(dto);
    }

    [HttpDelete("{projectId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid projectId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return Unauthorized();
        await _projects.DeleteAsync(CurrentUserId, projectId, ct);
        return NoContent();
    }
}

