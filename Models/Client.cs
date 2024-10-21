// Modelo de Cliente
public class Cliente
{
    public int ClienteId { get; set; }

    // Campos requeridos para Nombre, Apellido y Email
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required string Email { get; set; }
    
    public string? Telefono { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    // Nuevos campos
    public DateTime FechaNacimiento { get; set; }  // Para calcular la edad
    public string? Sexo { get; set; }  // "Masculino", "Femenino", "Otro"

    // Relación con Usuario (no obligatorio desde el frontend)
    public int UsuarioId { get; set; }  // El campo no se marca como required porque lo asigna el backend
    public Usuario? Usuario { get; set; }

    // Relación con Dietas
    public List<Dieta>? Dietas { get; set; }  // Relación con Dietas

    // Relación con Rutinas
    public List<Rutina> Rutinas { get; set; } = new List<Rutina>();

    // Relación con Progresos
    public List<Progreso> Progresos { get; set; } = new List<Progreso>();
}
