using AI.TaskFlow.Application.Common;
using AI.TaskFlow.Application.DTOs;
using AI.TaskFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AI.TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<TaskDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var response = await _taskService.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _taskService.GetByIdAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<IReadOnlyCollection<TaskDto>>> GetByProjectId(Guid projectId, CancellationToken cancellationToken)
    {
        var response = await _taskService.GetByProjectIdAsync(projectId, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var response = await _taskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskDto>> Update(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var response = await _taskService.UpdateAsync(id, request, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _taskService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
