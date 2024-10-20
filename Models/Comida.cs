public class Comida
{
    public int ComidaId { get; set; }
    public int DietaId { get; set; }
    public Dieta Dieta { get; set; }  // Relación con Dieta
    public string Nombre { get; set; }  // Ejemplo: "Comida 1"
    public string Hora { get; set; }  // Ejemplo: "6AM"
    public List<Alimento> Alimentos { get; set; }  // Relación con Alimentos
}
