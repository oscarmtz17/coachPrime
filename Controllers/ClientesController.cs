using Microsoft.AspNetCore.Mvc;
using webapi.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace webapi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ClienteController : ControllerBase
{
    private readonly IClienteService clienteService;

    public ClienteController(IClienteService service)
    {
        clienteService = service;
    }

    // Obtener todos los clientes del usuario autenticado
    [HttpGet]
    public IActionResult Get()
    {
        // Obtener el UsuarioId desde el token
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
if (usuarioIdClaim == null)
{
    return Unauthorized("Token inválido o falta el claim del UsuarioId.");
}

        // Verificar que el UsuarioId se obtuvo correctamente y convertirlo a int
        if (!int.TryParse(usuarioIdClaim, out int usuarioId))
        {
            return Unauthorized("Usuario no autorizado");
        }

        var clientes = clienteService.GetByUsuarioId(usuarioId);
        return Ok(clientes);
    }

    // Obtener un cliente específico del usuario autenticado
    [HttpGet("{id}")]
    public IActionResult GetCliente(int id)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(usuarioIdClaim, out int usuarioId))
        {
            return Unauthorized("Usuario no autorizado");
        }

        var cliente = clienteService.GetClienteByIdAndUsuarioId(id, usuarioId);
        if (cliente != null)
        {
            return Ok(cliente);
        }
        return NotFound("Cliente no encontrado o no pertenece al usuario.");
    }

    // Crear un nuevo cliente
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Cliente cliente)
    {
var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
if (usuarioIdClaim == null)
{
    return Unauthorized("Token inválido o falta el claim del UsuarioId.");
}

Console.WriteLine($"UsuarioIdClaim: {usuarioIdClaim}");

if (!int.TryParse(usuarioIdClaim, out int usuarioId))
{
    return Unauthorized("Usuario no autorizado");
}

if (ModelState.IsValid)
    {
        cliente.UsuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);  // Asigna el UsuarioId desde el token
        await clienteService.Save(cliente);
        return Ok("Cliente creado exitosamente");
    }
    return BadRequest(ModelState);
    }

    // Actualizar un cliente existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Cliente cliente)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(usuarioIdClaim, out int usuarioId))
        {
            return Unauthorized("Usuario no autorizado");
        }

        if (ModelState.IsValid)
        {
            cliente.UsuarioId = usuarioId;
            await clienteService.Update(id, cliente);
            return Ok("Cliente actualizado exitosamente");
        }
        return BadRequest(ModelState);
    }

    // Eliminar un cliente
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(usuarioIdClaim, out int usuarioId))
        {
            return Unauthorized("Usuario no autorizado");
        }

        await clienteService.Delete(id, usuarioId);
        return Ok("Cliente eliminado exitosamente");
    }
}
