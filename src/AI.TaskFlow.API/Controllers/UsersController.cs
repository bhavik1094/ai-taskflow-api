using AI.TaskFlow.Application.Common;
using AI.TaskFlow.Application.DTOs;
using AI.TaskFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AI.TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<UserProfileDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var response = await _userService.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserProfileDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _userService.GetByIdAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}
