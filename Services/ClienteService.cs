using Microsoft.EntityFrameworkCore;

namespace webapi.Services
{
    public class ClienteService : IClienteService
    {
        private readonly CoachPrimeContext context;

        public ClienteService(CoachPrimeContext dbcontext)
        {
            context = dbcontext;
        }

        // Obtener los clientes del usuario autenticado
        public IEnumerable<Cliente> GetByUsuarioId(int usuarioId)
        {
            return context.Clientes.Where(c => c.UsuarioId == usuarioId);
        }

        // Obtener un cliente específico del usuario autenticado
        public Cliente GetClienteByIdAndUsuarioId(int clienteId, int usuarioId)
        {
            return context.Clientes.FirstOrDefault(c => c.ClienteId == clienteId && c.UsuarioId == usuarioId);
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
            if (clienteActual != null && clienteActual.UsuarioId == cliente.UsuarioId) // Validar que el cliente pertenezca al usuario
            {
                clienteActual.Nombre = cliente.Nombre;
                clienteActual.Apellido = cliente.Apellido;
                clienteActual.Email = cliente.Email;
                clienteActual.Telefono = cliente.Telefono;

                await context.SaveChangesAsync();
            }
        }

        // Eliminar un cliente
        public async Task Delete(int id, int usuarioId)
        {
            var clienteActual = await context.Clientes.FindAsync(id);

            if (clienteActual != null && clienteActual.UsuarioId == usuarioId) // Validar que el cliente pertenezca al usuario
            {
                context.Clientes.Remove(clienteActual);
                await context.SaveChangesAsync();
            }
        }
    }

    public interface IClienteService
    {
        IEnumerable<Cliente> GetByUsuarioId(int usuarioId);
        Cliente GetClienteByIdAndUsuarioId(int clienteId, int usuarioId);
        Task Save(Cliente cliente);
        Task Update(int id, Cliente cliente);
        Task Delete(int id, int usuarioId);
    }
}
