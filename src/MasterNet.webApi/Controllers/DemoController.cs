using Microsoft.AspNetCore.Mvc;

namespace MasterNet.webApi.Controllers;

[ApiController]
[Route("Demo")]
public class DemoController : ControllerBase
{
  private readonly IConfiguration _configuration;
  private readonly IWebHostEnvironment _environment;

  public DemoController(IConfiguration configuration, IWebHostEnvironment environment)
  {
    _configuration = configuration;
    _environment = environment;
  }

  [HttpGet("getstring")]
  public string GetNombre()
  {
    return "vaxidrez.com";
  }

  [HttpGet("Ambiente")]
  public IActionResult GetAmbiente()
  {
    var mensaje = _configuration.GetValue<string>("MiVariable");
    var ambiente = _environment.EnvironmentName;
    return Ok(new { Ambiente = ambiente, Mensaje = mensaje });
  }
}