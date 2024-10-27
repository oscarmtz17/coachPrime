using BCrypt.Net;
using System.Text.RegularExpressions;

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
            if (usuario != null && BCrypt.Net.BCrypt.Verify(contraseña, usuario.Password))
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
            // Validar seguridad de la contraseña antes de guardar
            if (!IsPasswordSecure(usuario.Password))
            {
                throw new ArgumentException("La contraseña no cumple con los requisitos de seguridad.");
            }

            // Encripta la contraseña antes de guardarla
            usuario.Password = HashPassword(usuario.Password);
            usuario.Phone = usuario.Phone;
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
                usuarioActual.EmailVerificado = usuario.EmailVerificado;
                usuarioActual.EmailVerificationToken = usuario.EmailVerificationToken;

                // Si la contraseña fue modificada, se encripta de nuevo
                if (!BCrypt.Net.BCrypt.Verify(usuario.Password, usuarioActual.Password))
                {
                    usuarioActual.Password = HashPassword(usuario.Password);
                }

                // Guardar los cambios
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

        public Usuario GetUserByEmail(string email)
        {
            return context.Usuarios.SingleOrDefault(u => u.Email == email);
        }

        public void SavePasswordResetToken(int usuarioId, string token)
        {
            var usuario = context.Usuarios.Find(usuarioId);
            if (usuario != null)
            {
                usuario.PasswordResetToken = token;
                usuario.TokenExpirationDate = DateTime.Now.AddHours(1); // El token expira en 1 hora
                context.SaveChanges();
            }
        }

        public Usuario GetUserByPasswordResetToken(string token)
        {
            return context.Usuarios.SingleOrDefault(u => u.PasswordResetToken == token && u.TokenExpirationDate > DateTime.Now);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Actualizar la contraseña del usuario desde la funcionalidad de recuperación
        public void UpdatePassword(int usuarioId, string newPassword)
        {
            var user = context.Usuarios.Find(usuarioId);
            if (user != null)
            {
                // Validar seguridad de la nueva contraseña
                if (!IsPasswordSecure(newPassword))
                {
                    throw new ArgumentException("La nueva contraseña no cumple con los requisitos de seguridad.");
                }

                // Actualizar los campos de la base de datos
                user.Password = HashPassword(newPassword);
                user.PasswordResetToken = null; // Limpia el token de recuperación
                user.TokenExpirationDate = null; // Limpia la fecha de expiración del token

                // Guardar los cambios
                context.SaveChanges();
            }
        }

        // Método para validar la seguridad de la contraseña
        public bool IsPasswordSecure(string password)
        {
            // La contraseña debe tener al menos 8 caracteres
            if (password.Length < 8)
            {
                Console.WriteLine("La contraseña no tiene la longitud mínima de 8 caracteres.");
                return false;
            }

            // Debe contener al menos una letra mayúscula
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                Console.WriteLine("La contraseña debe contener al menos una letra mayúscula.");
                return false;
            }

            // Debe contener al menos una letra minúscula
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                Console.WriteLine("La contraseña debe contener al menos una letra minúscula.");
                return false;
            }

            // Debe contener al menos un dígito
            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                Console.WriteLine("La contraseña debe contener al menos un número.");
                return false;
            }

            // Debe contener al menos un carácter especial
            if (!Regex.IsMatch(password, @"[\W_]"))
            {
                Console.WriteLine("La contraseña debe contener al menos un carácter especial.");
                return false;
            }

            return true;
        }

        public Usuario GetUserByVerificationToken(string token)
        {
            return context.Usuarios.SingleOrDefault(u => u.EmailVerificationToken == token);
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

        // Recuperación de contraseña
        void SavePasswordResetToken(int usuarioId, string token);
        Usuario GetUserByPasswordResetToken(string token);
        Usuario GetUserByEmail(string email);
        void UpdatePassword(int usuarioId, string newPassword);
        string HashPassword(string password);

        // Validación de seguridad de contraseñas
        bool IsPasswordSecure(string password);
        Usuario GetUserByVerificationToken(string token);
    }
}
