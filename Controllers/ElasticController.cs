using Microsoft.AspNetCore.Mvc;
using MyProject.Services;

namespace MyProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ElasticController(ElasticSearchService elasticSearchService) : ControllerBase
{
    [HttpGet]
    public IActionResult Get(string name)
    {
        return Ok(elasticSearchService.SearchUser(name));
    }
}