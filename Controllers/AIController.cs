using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using webapi.Services;

namespace webapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly IAIDataService _aiDataService;
        private readonly IRutinaService _rutinaService;
        private readonly IDietaService _dietaService;

        public AIController(
            IAIService aiService,
            IAIDataService aiDataService,
            IRutinaService rutinaService,
            IDietaService dietaService)
        {
            _aiService = aiService;
            _aiDataService = aiDataService;
            _rutinaService = rutinaService;
            _dietaService = dietaService;
        }

        // POST: api/ai/generar/{clienteId}
        [HttpPost("generar/{clienteId}")]
        public async Task<IActionResult> GenerarRutinaYDieta(int clienteId, [FromBody] AIRequestConfiguracion configuracion)
        {
            try
            {
                // Obtener el ID del usuario autenticado
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (usuarioId == 0)
                {
                    return Unauthorized("Usuario no autenticado");
                }

                // Preparar datos para la IA
                var aiRequest = await _aiDataService.PrepararDatosParaIAAsync(clienteId, "Ambos", configuracion);

                // Generar rutina y dieta con IA
                var aiResponse = await _aiService.GenerarRutinaYDietaAsync(aiRequest);

                return Ok(new
                {
                    success = true,
                    data = aiResponse,
                    message = "Rutina y dieta generadas exitosamente con IA"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al generar rutina y dieta: {ex.Message}"
                });
            }
        }

        // POST: api/ai/generar-rutina/{clienteId}
        [HttpPost("generar-rutina/{clienteId}")]
        public async Task<IActionResult> GenerarSoloRutina(int clienteId, [FromBody] AIRequestConfiguracion configuracion)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (usuarioId == 0)
                {
                    return Unauthorized("Usuario no autenticado");
                }

                var aiRequest = await _aiDataService.PrepararDatosParaIAAsync(clienteId, "Rutina", configuracion);
                var aiResponse = await _aiService.GenerarRutinaYDietaAsync(aiRequest);

                return Ok(new
                {
                    success = true,
                    data = aiResponse,
                    message = "Rutina generada exitosamente con IA"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al generar rutina: {ex.Message}"
                });
            }
        }

        // POST: api/ai/generar-dieta/{clienteId}
        [HttpPost("generar-dieta/{clienteId}")]
        public async Task<IActionResult> GenerarSoloDieta(int clienteId, [FromBody] AIRequestConfiguracion configuracion)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (usuarioId == 0)
                {
                    return Unauthorized("Usuario no autenticado");
                }

                var aiRequest = await _aiDataService.PrepararDatosParaIAAsync(clienteId, "Dieta", configuracion);
                var aiResponse = await _aiService.GenerarRutinaYDietaAsync(aiRequest);

                return Ok(new
                {
                    success = true,
                    data = aiResponse,
                    message = "Dieta generada exitosamente con IA"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al generar dieta: {ex.Message}"
                });
            }
        }

        // POST: api/ai/guardar-rutina/{clienteId}
        [HttpPost("guardar-rutina/{clienteId}")]
        public async Task<IActionResult> GuardarRutinaGenerada(int clienteId, [FromBody] AIRutinaGenerada aiRutina)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (usuarioId == 0)
                {
                    return Unauthorized("Usuario no autenticado");
                }

                // Convertir la rutina de IA al formato del sistema
                var rutinaRequest = await _aiDataService.ConvertirAIRutinaACreateRutinaRequestAsync(aiRutina, clienteId, usuarioId);

                // Guardar la rutina
                var resultado = await _rutinaService.CreateRutinaAsync(rutinaRequest);

                if (resultado)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Rutina generada por IA guardada exitosamente"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Error al guardar la rutina"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al guardar rutina: {ex.Message}"
                });
            }
        }

        // POST: api/ai/guardar-dieta/{clienteId}
        [HttpPost("guardar-dieta/{clienteId}")]
        public async Task<IActionResult> GuardarDietaGenerada(int clienteId, [FromBody] AIDietaGenerada aiDieta)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (usuarioId == 0)
                {
                    return Unauthorized("Usuario no autenticado");
                }

                // Convertir la dieta de IA al formato del sistema
                var dietaRequest = await _aiDataService.ConvertirAIDietaADietaRequestAsync(aiDieta);

                // Guardar la dieta
                var resultado = await _dietaService.RegistrarDietaAsync(clienteId, dietaRequest);

                if (resultado)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Dieta generada por IA guardada exitosamente"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Error al guardar la dieta"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al guardar dieta: {ex.Message}"
                });
            }
        }

        // GET: api/ai/configuracion-default
        [HttpGet("configuracion-default")]
        public IActionResult ObtenerConfiguracionDefault()
        {
            var configuracion = new AIRequestConfiguracion
            {
                NivelDificultad = "Intermedio",
                TipoDieta = "Equilibrada",
                EnfoqueRutina = "Mixto",
                IncluirCardio = true,
                IncluirFlexibilidad = true,
                CaloriasObjetivo = 2000,
                ProteinaObjetivo = 1.6,
                CarbohidratosObjetivo = 45,
                GrasasObjetivo = 25
            };

            return Ok(new
            {
                success = true,
                data = configuracion
            });
        }
    }
}