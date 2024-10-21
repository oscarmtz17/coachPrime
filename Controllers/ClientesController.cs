using Microsoft.AspNetCore.Mvc;
// using webapi.Models;
using webapi.Services;
using Microsoft.AspNetCore.Authorization;


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

    // Obtener todos los clientes
    [HttpGet]
    public IActionResult Get()
    {
        var clientes = clienteService.Get();
        return Ok(clientes);
    }

    // Crear un nuevo cliente
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Cliente cliente)
    {
        if (ModelState.IsValid)
        {
            await clienteService.Save(cliente);
            return Ok("Cliente creado exitosamente");
        }
        return BadRequest(ModelState);
    }

    // Actualizar un cliente existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Cliente cliente)
    {
        if (ModelState.IsValid)
        {
            await clienteService.Update(id, cliente);
            return Ok("Cliente actualizado exitosamente");
        }
        return BadRequest(ModelState);
    }

    // Eliminar un cliente
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await clienteService.Delete(id);
        return Ok("Cliente eliminado exitosamente");
    }
}
