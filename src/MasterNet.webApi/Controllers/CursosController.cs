using MasterNet.Application.Core;
using MasterNet.Application.Cursos.CursoCreate;
using MasterNet.Application.Cursos.GetCursos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static MasterNet.Application.Cursos.CursoCreate.CursoCreateCommand;
using static MasterNet.Application.Cursos.CursoReporteExcel.CursoReporteExcelQuery;
using static MasterNet.Application.Cursos.GetCurso.GetQueryCurso;
using static MasterNet.Application.Cursos.GetCursos.GetCursoQuery;

namespace MasterNet.webApi.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class CursosController : ControllerBase
  {
    private readonly ISender _sender;
    public CursosController(ISender sender)
    {
      _sender = sender;
    }
    
    [HttpGet]
    public async Task<ActionResult> PaginationCursos(
      [FromQuery] GetCursosRequest request,
      CancellationToken cancellationToken
    )
    {
      var query = new GetCursosQueryRequest { CursosRequest = request };
      var resultado = await _sender.Send(query, cancellationToken);
      
      return resultado.IsSuccess ? Ok(resultado.Value) : NotFound();
    }
    
    [HttpPost("Create")]
    public async Task<ActionResult<Result<Guid>>> CursoCreate(
      [FromForm] CursoCreateRequest request, 
      CancellationToken cancellationToken
    )
    {
      var command = new CursoCreateCommandRequest(request);
      return await _sender.Send(command, cancellationToken);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> CursoGet(
      Guid id, 
      CancellationToken cancellationToken
    )
    {
      var query = new GetCursoQueryRequest { Id = id };
      var resultado = await _sender.Send(query, cancellationToken);
      return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest();
    }
    
    [HttpGet("Reporte")]
    public async Task<ActionResult> ReporteCSV(CancellationToken cancellationToken)
    {
      var query = new CursoReporteExcelQueryRequest();
      var resultado = await _sender.Send(query, cancellationToken);
      byte[] excelBytes = resultado.ToArray();
      return File(excelBytes, "text/csv", "cursos.csv");
    }
  }
}