// Modelo de Usuario
public class Usuario
{
    public int UsuarioId { get; set; } 
    public required string Nombre { get; set; }
    public string? Apellido { get; set; } // Nuevo campo para el apellido
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string Password { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    // Propiedad de navegación para la relación con Clientes
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    // Nueva relación con RefreshTokens
    public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    // Nuevos campos para recuperación de contraseña
    public string? PasswordResetToken { get; set; } // Campo para almacenar el token de recuperación
    public DateTime? TokenExpirationDate { get; set; } // Fecha de expiración del token de recuperación

    public string Rol { get; set; }  // Nuevo campo para roles

    // Verificación de correo electrónico
    public bool EmailVerificado { get; set; } = false; // Nuevo campo para verificar si el correo fue confirmado
    public string? EmailVerificationToken { get; set; } // Token para verificar el correo
}
