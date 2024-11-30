public class Plan
{
    public int PlanId { get; set; }
    public string Nombre { get; set; } = string.Empty; // Ej.: Básico, Estándar, Premium
    public decimal Precio { get; set; } // Precio del plan
    public string Frecuencia { get; set; } = "Mensual"; // Ej.: Mensual, Anual
    public int MaxClientes { get; set; } = 0; // Máximo de clientes permitidos
    public string? Beneficios { get; set; } // Lista de beneficios del plan
    public string Estado { get; set; } = "Activo"; // Activo/Inactivo
}
