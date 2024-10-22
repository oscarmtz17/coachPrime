using BCrypt.Net;

namespace webapi.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly CoachPrimeContext context;

        public UsuarioService(CoachPrimeContext dbcontext)
        {
            context = dbcontext;
        }

        // Validar usuario con contraseña encriptada
        public Usuario ValidateUser(string email, string contraseña)
        {
            var usuario = context.Usuarios.SingleOrDefault(u => u.Email == email);

            // Si el usuario existe, verificar la contraseña encriptada
            if (usuario != null && BCrypt.Net.BCrypt.Verify(contraseña, usuario.Contraseña))
            {
                return usuario;
            }

            return null; // Devuelve null si el usuario no es válido
        }

        // Obtener todos los usuarios
        public IEnumerable<Usuario> Get()
        {
            return context.Usuarios;
        }

        // Guardar un nuevo usuario con la contraseña encriptada
        public async Task Save(Usuario usuario)
        {
            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña); // Encripta la contraseña
            usuario.FechaRegistro = DateTime.Now;
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
        }

        // Actualizar un usuario existente con la posibilidad de encriptar la nueva contraseña
        public async Task Update(int id, Usuario usuario)
        {
            var usuarioActual = await context.Usuarios.FindAsync(id);
            if (usuarioActual != null)
            {
                usuarioActual.Nombre = usuario.Nombre;
                usuarioActual.Email = usuario.Email;
                
                // Si la contraseña fue modificada, se encripta de nuevo
                if (!BCrypt.Net.BCrypt.Verify(usuario.Contraseña, usuarioActual.Contraseña))
                {
                    usuarioActual.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);
                }

                await context.SaveChangesAsync();
            }
        }

        // Eliminar un usuario
        public async Task Delete(int id)
        {
            var usuarioActual = await context.Usuarios.FindAsync(id);
            if (usuarioActual != null)
            {
                context.Remove(usuarioActual);
                await context.SaveChangesAsync();
            }
        }

        // Implementación de manejo de Refresh Tokens
        public void SaveRefreshToken(int usuarioId, string refreshToken)
        {
            var usuario = context.Usuarios.Find(usuarioId);
            if (usuario != null)
            {
                var newRefreshToken = new RefreshToken
                {
                    Token = refreshToken,
                    UsuarioId = usuarioId,
                    ExpirationDate = DateTime.Now.AddDays(7) // Tiempo de expiración del Refresh Token
                };
                context.RefreshTokens.Add(newRefreshToken);
                context.SaveChanges();
            }
        }

        public string GetRefreshToken(int usuarioId)
        {
            return context.RefreshTokens
                .Where(rt => rt.UsuarioId == usuarioId && rt.ExpirationDate > DateTime.Now)
                .OrderByDescending(rt => rt.ExpirationDate)
                .Select(rt => rt.Token)
                .FirstOrDefault();
        }

        public void RevokeRefreshToken(int usuarioId)
        {
            var refreshTokens = context.RefreshTokens.Where(rt => rt.UsuarioId == usuarioId);
            context.RefreshTokens.RemoveRange(refreshTokens);
            context.SaveChanges();
        }

        public bool IsRefreshTokenRevoked(int usuarioId)
        {
            return !context.RefreshTokens.Any(rt => rt.UsuarioId == usuarioId && rt.ExpirationDate > DateTime.Now);
        }

        public Usuario GetUserById(int usuarioId)
        {
            return context.Usuarios.Find(usuarioId);
        }
    }

    public interface IUsuarioService
    {
        IEnumerable<Usuario> Get();
        Task Save(Usuario usuario);
        Task Update(int id, Usuario usuario);
        Task Delete(int id);
        Usuario ValidateUser(string email, string contraseña);

        // Métodos para manejo de Refresh Tokens
        void SaveRefreshToken(int usuarioId, string refreshToken);
        string GetRefreshToken(int usuarioId);
        void RevokeRefreshToken(int usuarioId);
        bool IsRefreshTokenRevoked(int usuarioId);
        Usuario GetUserById(int usuarioId);
    }
}
