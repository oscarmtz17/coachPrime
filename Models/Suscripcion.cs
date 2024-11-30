// Modelo de Suscripcion
public class Suscripcion
{
    public int SuscripcionId { get; set; } // Clave primaria
    public int UsuarioId { get; set; } // Relación con el usuario
    public Usuario Usuario { get; set; } = null!; // Propiedad de navegación hacia Usuario

    public int PlanId { get; set; } // Relación con el plan
    public Plan Plan { get; set; } = null!; // Propiedad de navegación hacia Plan

    public int EstadoSuscripcionId { get; set; } // Relación con el estado de la suscripción
    public EstadoSuscripcion EstadoSuscripcion { get; set; } = null!; // Propiedad de navegación hacia EstadoSuscripcion

    public DateTime FechaInicio { get; set; } // Fecha de inicio de la suscripción
    public DateTime? FechaFin { get; set; } // Fecha de finalización (null si es activa indefinidamente)
    public DateTime? FechaCancelacion { get; set; } // Fecha de cancelación (opcional)

    public string? StripeSubscriptionId { get; set; } // ID de Stripe para rastrear la suscripción
}
