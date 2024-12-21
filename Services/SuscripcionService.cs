using System.Linq;
using webapi.Models;
using Microsoft.EntityFrameworkCore;

namespace webapi.Services
{
    public class SuscripcionService : ISuscripcionService
    {
        private readonly CoachPrimeContext _context;

        public SuscripcionService(CoachPrimeContext context)
        {
            _context = context;
        }

        // Guardar una nueva suscripción
        public async Task Save(Suscripcion suscripcion)
        {
            await _context.Suscripcion.AddAsync(suscripcion);
            await _context.SaveChangesAsync();
        }

        // Obtener una suscripción por el ID del usuario
        public Suscripcion GetByUsuarioId(int usuarioId)
        {
            return _context.Suscripcion.FirstOrDefault(s => s.UsuarioId == usuarioId);
        }

        // Actualizar una suscripción existente
        public void Update(Suscripcion suscripcion)
        {
            var existingSuscripcion = _context.Suscripcion.Find(suscripcion.SuscripcionId);
            if (existingSuscripcion != null)
            {
                existingSuscripcion.PlanId = suscripcion.PlanId;
                existingSuscripcion.FechaInicio = suscripcion.FechaInicio;
                existingSuscripcion.FechaFin = suscripcion.FechaFin;
                existingSuscripcion.EstadoSuscripcionId = suscripcion.EstadoSuscripcionId;
                existingSuscripcion.StripeSubscriptionId = suscripcion.StripeSubscriptionId;
                existingSuscripcion.FechaCancelacion = suscripcion.FechaCancelacion;

                _context.SaveChanges();
            }
        }

        // Cancelar una suscripción por ID
        public void Cancel(int suscripcionId)
        {
            var suscripcion = _context.Suscripcion.Find(suscripcionId);
            if (suscripcion != null)
            {
                suscripcion.EstadoSuscripcionId = 3; // Suponiendo que 3 es 'Cancelada'
                suscripcion.FechaCancelacion = DateTime.Now;

                _context.SaveChanges();
            }
        }

        // Verificar si un usuario tiene una suscripción activa
        public bool IsActive(int usuarioId)
        {
            return _context.Suscripcion.Any(s =>
                s.UsuarioId == usuarioId &&
                s.EstadoSuscripcionId == 1 && // Suponiendo que 1 es 'Activa'
                s.FechaFin > DateTime.Now);
        }

        public async Task<Suscripcion> GetById(int id)
        {
            return await _context.Suscripcion.FindAsync(id);
        }


        public async Task ActivarSuscripcion(int suscripcionId)
        {
            // Buscar la suscripción por ID
            var suscripcion = await _context.Suscripcion.FindAsync(suscripcionId);

            if (suscripcion == null)
            {
                throw new KeyNotFoundException($"No se encontró una suscripción con el ID {suscripcionId}");
            }

            // Cambiar el estado a "Activa"
            suscripcion.EstadoSuscripcionId = 2; // 2 corresponde a "Activa"
            suscripcion.FechaInicio = DateTime.Now; // Registrar la fecha de activación

            // Guardar cambios en la base de datos
            _context.Suscripcion.Update(suscripcion);
            await _context.SaveChangesAsync();
        }

        public async Task<Usuario> GetUsuarioById(int usuarioId)
        {
            return await _context.Usuarios.FindAsync(usuarioId);
        }

        public async Task<Plan> GetPlanById(int planId)
        {
            return await _context.Planes.FindAsync(planId);
        }

        public async Task<Suscripcion> GetByStripeId(string stripeSubscriptionId)
        {
            return await _context.Suscripcion.FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);
        }

        public List<Suscripcion> GetActiveSubscriptions()
        {
            return _context.Suscripcion
                .Where(s => s.EstadoSuscripcionId == 2 && s.FechaFin > DateTime.Now) // 2 es "Activa"
                .ToList();
        }

        public List<Suscripcion> GetExpiredSubscriptions()
        {
            return _context.Suscripcion
                .Where(s => s.EstadoSuscripcionId == 2 && s.FechaFin <= DateTime.Now) // Activas pero vencidas
                .ToList();
        }

        public void MarkSubscriptionsAsExpired()
        {
            var expiredSubscriptions = GetExpiredSubscriptions();

            foreach (var suscripcion in expiredSubscriptions)
            {
                suscripcion.EstadoSuscripcionId = 3; // Expirada
            }

            _context.SaveChanges(); // Guardar cambios en la base de datos
        }




    }

    public interface ISuscripcionService
    {
        Task<Suscripcion> GetById(int id);
        Task Save(Suscripcion suscripcion);
        Task ActivarSuscripcion(int suscripcionId);
        Suscripcion GetByUsuarioId(int usuarioId);
        void Update(Suscripcion suscripcion);
        void Cancel(int suscripcionId);
        bool IsActive(int usuarioId);
        Task<Usuario> GetUsuarioById(int usuarioId);
        Task<Plan> GetPlanById(int planId);
        Task<Suscripcion> GetByStripeId(string stripeSubscriptionId);
        List<Suscripcion> GetActiveSubscriptions();
        List<Suscripcion> GetExpiredSubscriptions();
        void MarkSubscriptionsAsExpired();
    }
}
