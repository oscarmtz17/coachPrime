// Modelo de Usuario
public class Usuario
{
    public int UsuarioId { get; set; } 
    public required string Nombre { get; set; }
    public required string Email { get; set; }
    public required string Contraseña { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    // Propiedad de navegación para la relación con Clientes
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

     // Nueva relación con RefreshTokens
    public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

     // Nuevos campos para recuperación de contraseña
    public string? PasswordResetToken { get; set; } // Campo para almacenar el token de recuperación
    public DateTime? TokenExpirationDate { get; set; } // Fecha de expiración del token de recuperación

    public string Rol { get; set; }  // Nuevo campo para roles

}
