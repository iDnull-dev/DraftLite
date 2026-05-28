using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DraftLite.DTO;
using DraftLite.Service.Interfaces;

namespace DraftLite.Api.Controllers;

[Route("projects/{projectId:guid}/pages")]
[Authorize]
public sealed class PagesController : BaseController
{
    private readonly IPageService _pages;

    public PagesController(IPageService pages, ILogger<PagesController> logger) : base(logger)
    {
        _pages = pages;
    }

    [HttpGet("")]
    public async Task<ActionResult<IReadOnlyList<PageDto>>> List([FromRoute] Guid projectId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return Unauthorized();
        var dtos = await _pages.ListByProjectAsync(CurrentUserId, projectId, ct);
        return Ok(dtos);
    }

    [HttpPost("")]
    public async Task<ActionResult<PageDto>> Create([FromRoute] Guid projectId, [FromBody] CreatePageRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return Unauthorized();
        var dto = await _pages.CreateAsync(CurrentUserId, projectId, request, ct);
        return Ok(dto);
    }

    [HttpPut("{pageId:guid}")]
    public async Task<ActionResult<PageDto>> Update([FromRoute] Guid projectId, [FromRoute] Guid pageId, [FromBody] UpdatePageRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return Unauthorized();
        var dto = await _pages.UpdateAsync(CurrentUserId, projectId, pageId, request, ct);
        return Ok(dto);
    }

    [HttpDelete("{pageId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid projectId, [FromRoute] Guid pageId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return Unauthorized();
        await _pages.DeleteAsync(CurrentUserId, projectId, pageId, ct);
        return NoContent();
    }
}

