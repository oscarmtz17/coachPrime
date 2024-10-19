public class Progreso
{
    public int ProgresoId { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public required double PesoKg { get; set; }
    public required double EstaturaCm { get; set; }

    public required string NivelActividad { get; set; }  // "Sedentario", "Poco activo", etc.
    public double FactorActividad { get; set; }  // Valores como 1.2, 1.375, etc.

     public double TMB { get; set; }  // Tasa Metabólica Basal
    public double CaloriasDiarias { get; set; }  // TMB x FactorActividad
    public double IMC { get; set; }  // Peso (kg) / [Estatura (m)]^2

      // Medidas adicionales
    public double? CinturaCm { get; set; }
    public double? CaderaCm { get; set; }
    public double? PechoCm { get; set; }
    public double? BrazoCm { get; set; }
    public double? PiernaCm { get; set; }

    public string Notas { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }
}
