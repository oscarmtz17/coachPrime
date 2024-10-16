public class DiaEntrenamiento
{
    public int DiaEntrenamientoId { get; set; }
    public string DiaSemana { get; set; }  // Lunes, Martes, etc.

    public int RutinaId { get; set; }
    public Rutina Rutina { get; set; }  // Relación con Rutina

    public List<Agrupacion> Agrupaciones { get; set; } = new List<Agrupacion>();  // Relación con Agrupaciones de Ejercicios
}
