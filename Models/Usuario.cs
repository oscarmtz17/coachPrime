// Modelo de Usuario
public class Usuario
{
    public int UsuarioId { get; set; } 
    public required string Nombre { get; set; }
    public required string Email { get; set; }
    public required string Contraseña { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

}
