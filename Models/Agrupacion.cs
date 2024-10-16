public class Agrupacion
{
    public int AgrupacionId { get; set; }
    public string Tipo { get; set; }  // Tipo de agrupación: Individual, Bi-serie, Tri-serie, Circuito, etc.

    public int DiaEntrenamientoId { get; set; }
    public DiaEntrenamiento DiaEntrenamiento { get; set; }  // Relación con el día de entrenamiento

    public List<EjercicioAgrupado> EjerciciosAgrupados { get; set; } = new List<EjercicioAgrupado>();  // Relación con los ejercicios agrupados
}
