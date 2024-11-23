public class Ejercicio
{
    public int EjercicioId { get; set; }
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public int Series { get; set; }
    public int Repeticiones { get; set; }
    public string? ImagenKey { get; set; }  // Key de la imagen en S3
}
