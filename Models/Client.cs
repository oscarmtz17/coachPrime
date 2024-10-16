// Modelo de Cliente
public class Cliente
{
    public int ClienteId { get; set; }
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required string Email { get; set; }
    public string? Telefono { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    // Relación con Usuario
    public required int UsuarioId { get; set; }
    
}