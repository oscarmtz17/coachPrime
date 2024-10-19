// Modelo de Cliente
public class Cliente
{
    public int ClienteId { get; set; }
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required string Email { get; set; }
    public string? Telefono { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    // Nuevos campos
    public DateTime FechaNacimiento { get; set; }  // Para calcular la edad
    public string Sexo { get; set; }  // "Masculino", "Femenino", "Otro"
    // Relación con Usuario
    public required int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }
    

    public List<Rutina> Rutinas { get; set; } = new List<Rutina>();
    public List<Progreso> Progresos { get; set; } = new List<Progreso>();  // Relación con Progresos
    
}