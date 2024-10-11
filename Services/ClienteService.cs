using webapi.Models;

namespace webapi.Services;

public class ClienteService : IClienteService
{
    private readonly TareasContext context;

    public ClienteService(TareasContext dbcontext)
    {
        context = dbcontext;
    }

    // Obtener todos los clientes
    public IEnumerable<Cliente> Get()
    {
        return context.Clientes;
    }

    // Guardar un nuevo cliente
    public async Task Save(Cliente cliente)
    {
        cliente.FechaRegistro = DateTime.Now;  // Asigna la fecha actual
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();
    }

    // Actualizar un cliente existente
    public async Task Update(int id, Cliente cliente)
    {
        var clienteActual = await context.Clientes.FindAsync(id);
        if (clienteActual != null)
        {
            clienteActual.Nombre = cliente.Nombre;
            clienteActual.Apellido = cliente.Apellido;
            clienteActual.Email = cliente.Email;
            clienteActual.Telefono = cliente.Telefono;

            await context.SaveChangesAsync();
        }
    }

    // Eliminar un cliente
    public async Task Delete(int id)
    {
        var clienteActual = await context.Clientes.FindAsync(id);

        if (clienteActual != null)
        {
            context.Clientes.Remove(clienteActual);
            await context.SaveChangesAsync();
        }
    }
}

public interface IClienteService
{
    IEnumerable<Cliente> Get();
    Task Save(Cliente cliente);
    Task Update(int id, Cliente cliente);
    Task Delete(int id);
}
