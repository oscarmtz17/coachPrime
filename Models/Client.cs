// Modelo de Cliente
public class Cliente
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now; // Fecha y hora actual

    // Relación con otras entidades (opcional)
    public List<Rutina>? Rutinas { get; set; } = new List<Rutina>();
    public List<Dieta>? Dietas { get; set; } = new List<Dieta>();
    public List<Progreso>? Progresos { get; set; } = new List<Progreso>();
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
