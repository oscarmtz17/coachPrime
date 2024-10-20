public class DietaRequest
{
    public string Nombre { get; set; }  // Nombre de la dieta, ej: "Dieta alta en proteínas"
    public string Notas { get; set; }  // Notas adicionales, ej: "No agregar sal"
    
    // Comidas que forman parte de la dieta
    public List<ComidaRequest> Comidas { get; set; }
}

public class ComidaRequest
{
    public string Nombre { get; set; }  // Nombre de la comida, ej: "Comida 1"
    public string Hora { get; set; }  // Hora de la comida, ej: "6AM"
    
    // Alimentos que forman parte de esta comida
    public List<AlimentoRequest> Alimentos { get; set; }
}

public class AlimentoRequest
{
    public string Nombre { get; set; }  // Nombre del alimento, ej: "Pan integral"
    public double Cantidad { get; set; }  // Cantidad, ej: 2
    public string Unidad { get; set; }  // Unidad, ej: "rebanadas"
}
