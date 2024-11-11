using Microsoft.AspNetCore.Mvc;
using webapi.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace webapi.Controllers;

[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService usuarioService;

    public UsuarioController(IUsuarioService service)
    {
        usuarioService = service;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Get()
    {
        var usuarios = usuarioService.Get();
        return Ok(usuarios);
    }

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var usuario = usuarioService.GetUserById(id);
        if (usuario == null)
        {
            return NotFound("Usuario no encontrado");
        }
        return Ok(usuario);
    }


    // Actualizar un Usuario existente

[HttpPut("{id}")]
public async Task<IActionResult> Put(int id, [FromBody] UsuarioUpdateRequest usuarioUpdate)
{
    if (ModelState.IsValid)
    {
        var existingUser = usuarioService.GetUserById(id);
        if (existingUser == null)
        {
            return NotFound("Usuario no encontrado.");
        }

        // Actualizamos solo Nombre, Apellido y Phone
        existingUser.Nombre = usuarioUpdate.Nombre;
        existingUser.Apellido = usuarioUpdate.Apellido;
        existingUser.Phone = usuarioUpdate.Phone;

        await usuarioService.Update(id, existingUser);
        return Ok("Usuario actualizado exitosamente.");
    }

    return BadRequest(ModelState);
}



    // Eliminar un usuario
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await usuarioService.Delete(id);
        return Ok("Usuario Eliminado");
    }

    [HttpGet("check-phone")]
    public IActionResult CheckPhoneExists([FromQuery] string phone)
    {
        var exists = usuarioService.IsPhoneRegistered(phone);
        return Ok(new { exists });
    }

}

