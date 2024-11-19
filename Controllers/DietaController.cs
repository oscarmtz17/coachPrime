using Microsoft.AspNetCore.Mvc;
using webapi.Services;

[ApiController]
[Route("api/[controller]")]
public class DietaController : ControllerBase
{
    private readonly IDietaService _dietaService;
    private readonly PdfService _pdfService;

    public DietaController(IDietaService dietaService, PdfService pdfService)
    {
        _dietaService = dietaService;
        _pdfService = pdfService;
    }

    // Endpoint para descargar el PDF de una dieta
    [HttpGet("{clienteId}/{dietaId}/pdf")]
    public async Task<IActionResult> DescargarDietaPdf(int clienteId, int dietaId)
    {
        var dieta = await _dietaService.GetDietaByIdAsync(clienteId, dietaId);

        if (dieta == null)
        {
            return NotFound("Dieta no encontrada.");
        }

        var pdfBytes = _pdfService.GenerarDietaPdf(dieta);
        return File(pdfBytes, "application/pdf", $"Dieta_{dieta.Nombre}.pdf");
    }

    // GET: api/dieta/{clienteId} - Obtener todas las dietas de un cliente
    [HttpGet("{clienteId}")]
    public async Task<IActionResult> GetDietas(int clienteId)
    {
        var dietas = await _dietaService.GetAllDietasAsync(clienteId);
        return Ok(dietas);
    }

    // GET: api/dieta/{clienteId}/{dietaId} - Obtener una dieta específica
    [HttpGet("{clienteId}/{dietaId}")]
    public async Task<IActionResult> GetDieta(int clienteId, int dietaId)
    {
        var dieta = await _dietaService.GetDietaByIdAsync(clienteId, dietaId);
        if (dieta == null) return NotFound();
        return Ok(dieta);
    }

    // POST: api/dieta/{clienteId} - Registrar una nueva dieta
    [HttpPost("{clienteId}")]
    public async Task<IActionResult> RegistrarDieta(int clienteId, [FromBody] DietaRequest request)
    {
        try
        {
            var result = await _dietaService.RegistrarDietaAsync(clienteId, request);
            if (result) return Ok("Dieta registrada exitosamente.");
            return BadRequest("Error al registrar la dieta.");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Debes agregar al menos un alimento"))
            {
                return BadRequest(new { error = ex.Message });
            }
            return StatusCode(500, "Ocurrió un error inesperado.");
        }
    }


    // PUT: api/dieta/{clienteId}/{dietaId} - Actualizar una dieta
    [HttpPut("{clienteId}/{dietaId}")]
    public async Task<IActionResult> UpdateDieta(int clienteId, int dietaId, [FromBody] DietaRequest request)
    {
        try
        {
            var result = await _dietaService.UpdateDietaAsync(clienteId, dietaId, request);
            if (result) return Ok("Dieta actualizada exitosamente.");
            return NotFound("Dieta no encontrada.");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Debes agregar al menos un alimento"))
            {
                return BadRequest(new { error = ex.Message });
            }
            return StatusCode(500, "Ocurrió un error inesperado.");
        }
    }


    // DELETE: api/dieta/{clienteId}/{dietaId} - Eliminar una dieta
    [HttpDelete("{clienteId}/{dietaId}")]
    public async Task<IActionResult> DeleteDieta(int clienteId, int dietaId)
    {
        var result = await _dietaService.DeleteDietaAsync(clienteId, dietaId);
        if (result) return Ok("Dieta eliminada exitosamente.");
        return NotFound("Dieta no encontrada.");
    }
}
