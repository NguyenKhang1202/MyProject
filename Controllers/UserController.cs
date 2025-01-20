using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Domain;
using MyProject.Domain.Dtos;
using MyProject.Helpers;
using MyProject.Repos;
using MyProject.Services;

namespace MyProject.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController(ILogger<UserController> logger, IUserService userService) : ControllerBase
{
    private readonly ILogger<UserController> _logger = logger;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int userId)
    {
        return await ControllerHelper.TryCatchAsync(this, "Get", async () =>
        {
            var result = await userService.GetById(userId);
            return Ok(result);
        });
    }
    
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        return await ControllerHelper.TryCatchAsync(this, "Update", async () =>
        {
            var errors = await userService.Update(id, dto);
            if (errors.Count != 0)
            {
                return BadRequest(errors);
            }

            return Ok();
        });
    }
}
