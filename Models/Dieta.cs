public class Dieta
{
    public int DietaId { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }  // Relación con Cliente
    public string Nombre { get; set; }  // Ejemplo: "Dieta alta en proteínas"
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;  // Fecha de asignación
    public List<Comida> Comidas { get; set; }  // Relación con Comidas
    public string Notas { get; set; }  // Notas adicionales, por ejemplo: "No agregar sal extra"
}
