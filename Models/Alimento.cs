public class Alimento
{
    public int AlimentoId { get; set; }
    public int ComidaId { get; set; }
    public Comida Comida { get; set; }  // Relación con Comida
    public string Nombre { get; set; }  // Ejemplo: "Pan integral"
    public double Cantidad { get; set; }  // Ejemplo: 2
    public string Unidad { get; set; }  // Ejemplo: "rebanadas", "gr", "piezas"
}
