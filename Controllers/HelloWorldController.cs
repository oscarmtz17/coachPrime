using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace webapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloWorldController : ControllerBase
{
    IHelloWorldService helloWorldService;

    CoachPrimeContext dbcontext;

    private readonly ILogger<HelloWorldController> _logger;

    public HelloWorldController(ILogger<HelloWorldController> logger, IHelloWorldService helloWorld, CoachPrimeContext db)
    {
        _logger = logger;
        helloWorldService = helloWorld;
        dbcontext = db;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogDebug("Retornando el helloworld");
        return Ok(helloWorldService.GetHelloWorld());
    }

    [HttpGet]
    [Route("createdb")]

    public IActionResult CreateDatabase()
    {
        dbcontext.Database.EnsureCreated();
        return Ok();
    }

}