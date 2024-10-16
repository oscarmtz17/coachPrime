public class EjercicioAgrupado
{
    public int EjercicioAgrupadoId { get; set; }

    public int AgrupacionId { get; set; }
    public Agrupacion Agrupacion { get; set; }  // Relación con la agrupación de ejercicios

    public int EjercicioId { get; set; }
    public Ejercicio Ejercicio { get; set; }  // Relación con el ejercicio
}
