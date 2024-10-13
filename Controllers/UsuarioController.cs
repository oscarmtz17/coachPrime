using Microsoft.AspNetCore.Mvc;
using webapi.Services;
namespace webapi.Controllers;

[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService usuarioService;

    public UsuarioController(IUsuarioService service)
    {
        usuarioService = service;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var usuarios = usuarioService.Get();
        return Ok(usuarios);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Usuario usuario)
        {
        if (ModelState.IsValid)
        {
            await usuarioService.Save(usuario);
            return Ok("Usuario creado exitosamente");
        }
        return BadRequest(ModelState);
    }

    //Actualizar un Usuario existente

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Usuario usuario)
    {
        if (ModelState.IsValid)
        {
        await usuarioService.Update(id, usuario);
        return Ok();

        }

        return BadRequest(ModelState);
    }

//Eliminar un usuario
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await usuarioService.Delete(id);
        return Ok("Usuario Eliminado");
    }
}
