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




// Modelo de Rutina
public class Rutina
{
    public int RutinaId { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }
}

// Modelo de Dieta
public class Dieta
{
    public int DietaId { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }
}

// Modelo de Progreso
public class Progreso
{
    public int ProgresoId { get; set; }
    public DateTime Fecha { get; set; }
    public string Detalles { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }
}
