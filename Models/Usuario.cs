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

}
