using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using webapi.Models;

namespace webapi.Services
{
    public interface ISuscripcionService
    {
        Task<IEnumerable<Suscripcion>> GetAll();
        Task<Suscripcion> GetById(int id);
        Task<Suscripcion> GetByUserId(int userId);
        Task<Suscripcion> GetByStripeId(string stripeSubscriptionId);
        Task Save(Suscripcion suscripcion);
        Task Update(Suscripcion suscripcion);
        Task Cancel(int suscripcionId);
        Task<bool> IsActive(int usuarioId);
        Task<Usuario> GetUsuarioById(int usuarioId);
        Task<Plan> GetPlanById(int planId);
        Task<List<Suscripcion>> GetActiveSubscriptions();
        Task<List<Suscripcion>> GetExpiredSubscriptions();
        Task MarkSubscriptionsAsExpired();
    }

    public class SuscripcionService : ISuscripcionService
    {
        private readonly CoachPrimeContext _context;

        public SuscripcionService(CoachPrimeContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Suscripcion>> GetAll()
        {
            return await _context.Suscripcion.ToListAsync();
        }

        public async Task<Suscripcion> GetById(int id)
        {
            return await _context.Suscripcion.FindAsync(id);
        }

        public async Task<Suscripcion> GetByUserId(int userId)
        {
            return await _context.Suscripcion
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.UsuarioId == userId);
        }

        public async Task<Suscripcion> GetByStripeId(string stripeSubscriptionId)
        {
            return await _context.Suscripcion.FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);
        }

        public async Task Save(Suscripcion suscripcion)
        {
            _context.Suscripcion.Add(suscripcion);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Suscripcion suscripcion)
        {
            var existingSuscripcion = await _context.Suscripcion.FindAsync(suscripcion.SuscripcionId);

            if (existingSuscripcion != null)
            {
                existingSuscripcion.PlanId = suscripcion.PlanId;
                existingSuscripcion.FechaInicio = suscripcion.FechaInicio;
                existingSuscripcion.FechaFin = suscripcion.FechaFin;
                existingSuscripcion.EstadoSuscripcionId = suscripcion.EstadoSuscripcionId;
                existingSuscripcion.StripeSubscriptionId = suscripcion.StripeSubscriptionId;
                existingSuscripcion.FechaCancelacion = suscripcion.FechaCancelacion;
            }
            else
            {
                _context.Suscripcion.Add(suscripcion);
            }

            await _context.SaveChangesAsync();
        }

        public async Task Cancel(int suscripcionId)
        {
            var suscripcion = await _context.Suscripcion.FindAsync(suscripcionId);
            if (suscripcion != null)
            {
                suscripcion.EstadoSuscripcionId = 3; // Suponiendo que 3 es 'Cancelada'
                suscripcion.FechaCancelacion = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsActive(int usuarioId)
        {
            return await _context.Suscripcion.AnyAsync(s =>
                s.UsuarioId == usuarioId &&
                s.EstadoSuscripcionId == 2 && // 2 es 'Activa'
                (s.FechaFin == null || s.FechaFin > DateTime.Now));
        }

        public async Task<Usuario> GetUsuarioById(int usuarioId)
        {
            return await _context.Usuarios.FindAsync(usuarioId);
        }

        public async Task<Plan> GetPlanById(int planId)
        {
            return await _context.Planes.FindAsync(planId);
        }

        public async Task<List<Suscripcion>> GetActiveSubscriptions()
        {
            return await _context.Suscripcion
                .Where(s => s.EstadoSuscripcionId == 2 && s.FechaFin > DateTime.Now)
                .ToListAsync();
        }

        public async Task<List<Suscripcion>> GetExpiredSubscriptions()
        {
            return await _context.Suscripcion
                .Where(s => s.EstadoSuscripcionId == 2 && s.FechaFin <= DateTime.Now)
                .ToListAsync();
        }

        public async Task MarkSubscriptionsAsExpired()
        {
            var expiredSubscriptions = await GetExpiredSubscriptions();
            foreach (var suscripcion in expiredSubscriptions)
            {
                suscripcion.EstadoSuscripcionId = 3; // Expirada
            }
            await _context.SaveChangesAsync();
        }
    }
}
