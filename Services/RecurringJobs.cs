using Hangfire;
using webapi.Services;

public class RecurringJobs
{
    private readonly ISuscripcionService _suscripcionService;
    private readonly EmailService _emailService;

    public RecurringJobs(ISuscripcionService suscripcionService, EmailService emailService)
    {
        _suscripcionService = suscripcionService;
        _emailService = emailService;
    }

    public void CheckAndNotifySubscriptions()
    {
        var suscripciones = _suscripcionService.GetActiveSubscriptions();

        foreach (var suscripcion in suscripciones)
        {
            if (suscripcion.FechaFin.HasValue)
            {
                var diasRestantes = (suscripcion.FechaFin.Value - DateTime.Now).Days;

                if (diasRestantes <= 7 && diasRestantes > 0)
                {
                    // Enviar recordatorio por correo
                    var usuario = _suscripcionService.GetUsuarioById(suscripcion.UsuarioId).Result;
                    if (usuario != null)
                    {
                        _emailService.SendSubscriptionReminder(
                            usuario.Email,
                            usuario.Nombre,
                            suscripcion.FechaFin.Value,
                            diasRestantes
                        ).Wait();
                    }
                }
                else if (diasRestantes <= 0)
                {
                    // Actualizar estado de la suscripción como "Expirada"
                    suscripcion.EstadoSuscripcionId = 3; // 3 es Expirada
                    _suscripcionService.Update(suscripcion);
                }
            }
        }
    }


}
