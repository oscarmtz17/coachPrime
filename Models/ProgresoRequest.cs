public class ProgresoRequest
{
    public double PesoKg { get; set; }
    public double EstaturaCm { get; set; }
    public string NivelActividad { get; set; }
    public double FactorActividad { get; set; }

    // Medidas opcionales
    public double? CinturaCm { get; set; }
    public double? CaderaCm { get; set; }
    public double? PechoCm { get; set; }
    public double? BrazoCm { get; set; }
    public double? PiernaCm { get; set; }

    public string Notas { get; set; }
}
