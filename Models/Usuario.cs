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

    // Relación con Clientes
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    // Relación con RefreshTokens
    public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    // Recuperación de contraseña
    public string? PasswordResetToken { get; set; }
    public DateTime? TokenExpirationDate { get; set; }

    // Roles y autenticación
    public string Rol { get; set; }
    public bool EmailVerificado { get; set; } = false;
    public string? EmailVerificationToken { get; set; }

    // Relación con Suscripciones
    public ICollection<Suscripcion> Suscripciones { get; set; } = new List<Suscripcion>();
}
