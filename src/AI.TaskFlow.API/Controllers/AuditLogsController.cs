using AI.TaskFlow.Application.Common;
using AI.TaskFlow.Domain.Entities;
using AI.TaskFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AI.TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AuditLog>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var response = await _auditLogService.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        return Ok(response);
    }
}
