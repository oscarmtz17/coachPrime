public class Rutina
{
    public int RutinaId { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public DateTime FechaInicio { get; set; } = DateTime.Now;
    public DateTime? FechaFin { get; set; }  // Fecha opcional para finalización de la rutina

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }  // Relación con Cliente

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }  // Relación con Usuario (entrenador)

    public List<DiaEntrenamiento> DiasEntrenamiento { get; set; } = new List<DiaEntrenamiento>();  // Relación con Días de Entrenamiento
}
