namespace webapi.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly CoachPrimeContext context;

        public UsuarioService(CoachPrimeContext dbcontext)
        {
            context = dbcontext;
        }

        public Usuario ValidateUser(string email, string contraseña)
        {
            return context.Usuarios.SingleOrDefault(u => u.Email == email && u.Contraseña == contraseña);  // Corregido
        }

        public IEnumerable<Usuario> Get()
        {
            return context.Usuarios;
        }

        public async Task Save(Usuario usuario)
        {
            usuario.FechaRegistro = DateTime.Now;
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
        }

        // Actualizar un Usuario existente
        public async Task Update(int id, Usuario usuario)
        {
            var usuarioActual = await context.Usuarios.FindAsync(id);
            if (usuarioActual != null)
            {
                usuarioActual.Nombre = usuario.Nombre;
                usuarioActual.Email = usuario.Email;
                usuarioActual.Contraseña = usuario.Contraseña;

                await context.SaveChangesAsync();
            }
        }

        // Eliminar un Usuario
        public async Task Delete(int id)
        {
            var usuarioActual = await context.Usuarios.FindAsync(id);
            if (usuarioActual != null)
            {
                context.Remove(usuarioActual);
                await context.SaveChangesAsync();
            }
        }
    }

    public interface IUsuarioService
    {
        IEnumerable<Usuario> Get();
        Task Save(Usuario usuario);
        Task Update(int id, Usuario usuario);
        Task Delete(int id);
        Usuario ValidateUser(string email, string contraseña);
    }
}
