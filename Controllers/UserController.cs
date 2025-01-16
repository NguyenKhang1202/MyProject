using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Domain;
using MyProject.Domain.Dtos;
using MyProject.Helpers;
using MyProject.Repos;

namespace MyProject.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController(ILogger<UserController> logger, IUserRepo userRepo, IMapper mapper) : ControllerBase
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
            var result = await userRepo.Where(x => x.Id == userId).FirstOrDefaultAsync();
            return Ok(result);
        });
    }
    
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await userRepo.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        mapper.Map(dto, user);

        userRepo.Update(user);
        await userRepo.SaveChangesAsync();

        return Ok(new
        {
            Message = "User updated successfully.",
            User = user
        });
    }
}
