// Modelo de EstadosSuscripcion
public class EstadoSuscripcion
{
    public int EstadoSuscripcionId { get; set; } // Clave primaria
    public string NombreEstado { get; set; } = string.Empty; // Nombre del estado, e.g., "Activa", "Expirada"
    public string? Descripcion { get; set; } // Descripción del estado
    public bool EsFinal { get; set; } // Indica si este es un estado final, e.g., "Cancelada", "Expirada"
}
