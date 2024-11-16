using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProgresoController : ControllerBase
{
    private readonly IProgresoService _progresoService;

    public ProgresoController(IProgresoService progresoService)
    {
        _progresoService = progresoService;
    }

    // GET: api/progreso/{clienteId} - Obtener todos los progresos de un cliente
    [HttpGet("{clienteId}")]
    public async Task<IActionResult> GetProgresos(int clienteId)
    {
        var progresos = await _progresoService.GetAllProgresosAsync(clienteId);
        return Ok(progresos);
    }

    // GET: api/progreso/{clienteId}/{progresoId} - Obtener un progreso específico
    [HttpGet("{clienteId}/{progresoId}")]
    public async Task<IActionResult> GetProgreso(int clienteId, int progresoId)
    {
        var progreso = await _progresoService.GetProgresoByIdAsync(clienteId, progresoId);
        if (progreso == null) return NotFound();
        return Ok(progreso);
    }

    // POST: api/progreso/{clienteId} - Registrar un nuevo progreso
    [HttpPost("{clienteId}")]
    public async Task<IActionResult> RegistrarProgreso(int clienteId, [FromBody] ProgresoRequest request)
    {
        var progresoId = await _progresoService.RegistrarProgresoAsync(clienteId, request);
        if (progresoId == null) return BadRequest("Error al registrar el progreso.");

        return Ok(new { ProgresoId = progresoId, Message = "Progreso registrado exitosamente." });
    }


    // PUT: api/progreso/{clienteId}/{progresoId} - Actualizar un progreso
    [HttpPut("{clienteId}/{progresoId}")]
    public async Task<IActionResult> UpdateProgreso(int clienteId, int progresoId, [FromBody] ProgresoRequest request)
    {
        var result = await _progresoService.UpdateProgresoAsync(clienteId, progresoId, request);
        if (result) return Ok("Progreso actualizado exitosamente.");
        return NotFound("Progreso no encontrado.");
    }

    // DELETE: api/progreso/{clienteId}/{progresoId} - Eliminar un progreso
    [HttpDelete("{clienteId}/{progresoId}")]
    public async Task<IActionResult> DeleteProgreso(int clienteId, int progresoId)
    {
        var result = await _progresoService.DeleteProgresoAsync(clienteId, progresoId);
        if (result) return Ok("Progreso eliminado exitosamente.");
        return NotFound("Progreso no encontrado.");
    }
}
