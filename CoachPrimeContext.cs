using Microsoft.EntityFrameworkCore;

namespace webapi;

public class CoachPrimeContext : DbContext
{
    public CoachPrimeContext(DbContextOptions<CoachPrimeContext> options) : base(options) { }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Rutina> Rutinas { get; set; }
    public DbSet<Agrupacion> Agrupaciones { get; set; }
    public DbSet<DiaEntrenamiento> DiasEntrenamiento { get; set; }
    public DbSet<EjercicioAgrupado> EjercicioAgrupado { get; set; }
    public DbSet<Ejercicio> Ejercicios { get; set; }
    public DbSet<Progreso> Progresos { get; set; }

    // Nuevas tablas para las dietas
    public DbSet<Dieta> Dietas { get; set; }
    public DbSet<Comida> Comidas { get; set; }
    public DbSet<Alimento> Alimentos { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Suscripcion> Suscripcion { get; set; }
    public DbSet<Plan> Planes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relación Cliente - Usuario
        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Usuario)
            .WithMany(u => u.Clientes)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);  // Cambiamos a NO ACTION

        // Relación Dieta - Cliente
        modelBuilder.Entity<Dieta>()
            .HasOne(d => d.Cliente)
            .WithMany(c => c.Dietas)
            .HasForeignKey(d => d.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación Comida - Dieta
        modelBuilder.Entity<Comida>()
            .HasOne(c => c.Dieta)
            .WithMany(d => d.Comidas)
            .HasForeignKey(c => c.DietaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación Alimento - Comida
        modelBuilder.Entity<Alimento>()
            .HasOne(a => a.Comida)
            .WithMany(c => c.Alimentos)
            .HasForeignKey(a => a.ComidaId)
            .OnDelete(DeleteBehavior.Cascade);

        //RefreshToken
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.Usuario)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación Usuario - Suscripciones
        modelBuilder.Entity<Suscripcion>()
            .HasOne(s => s.Usuario)
            .WithMany(u => u.Suscripciones)
            .HasForeignKey(s => s.UsuarioId);

        // Relación Plan - Suscripciones
        modelBuilder.Entity<Suscripcion>()
            .HasOne(s => s.Plan)
            .WithMany()
            .HasForeignKey(s => s.PlanId);

        modelBuilder.Entity<Suscripcion>().ToTable("Suscripcion");
        modelBuilder.Entity<Plan>().ToTable("Plan");

        // Seed para Plan
        modelBuilder.Entity<Plan>().HasData(
            new Plan
            {
                PlanId = 1,
                Nombre = "Básico",
                Precio = 0.00m,
                Frecuencia = "Mensual",
                Beneficios = "Acceso básico a la app: Sin reportes avanzados.",
                Estado = "Activo",
                MaxClientes = 5
            },
            new Plan
            {
                PlanId = 3,
                Nombre = "Premium",
                Precio = 499.00m,
                Frecuencia = "Mensual",
                Beneficios = "Reportes avanzados: Soporte dedicado; Integración avanzada.",
                Estado = "Activo",
                StripePriceId = "price_1Q8KUQBZAdKpouIVDKjLz25"
            },
            new Plan
            {
                PlanId = 4,
                Nombre = "Anual Premium",
                Precio = 4999.00m,
                Frecuencia = "Anual",
                Beneficios = "Igual que Premium; Descuento anual.",
                Estado = "Activo",
                StripePriceId = "price_1Q9r7hBZAdKpouIVK5WRxMl"
            }
        );

        // Seed para EstadoSuscripcion
        modelBuilder.Entity<EstadoSuscripcion>().HasData(
            new EstadoSuscripcion { EstadoSuscripcionId = 1, NombreEstado = "Pendiente", Descripcion = "Pago iniciado pero no completado.", EsFinal = false },
            new EstadoSuscripcion { EstadoSuscripcionId = 2, NombreEstado = "Activa", Descripcion = "Suscripción activa y en buen estado.", EsFinal = false },
            new EstadoSuscripcion { EstadoSuscripcionId = 3, NombreEstado = "Expirada", Descripcion = "Periodo de suscripción finalizado.", EsFinal = true },
            new EstadoSuscripcion { EstadoSuscripcionId = 4, NombreEstado = "Cancelada", Descripcion = "Usuario canceló la suscripción.", EsFinal = true },
            new EstadoSuscripcion { EstadoSuscripcionId = 5, NombreEstado = "Suspendida", Descripcion = "Periodo de suspensión/cancelación.", EsFinal = false },
            new EstadoSuscripcion { EstadoSuscripcionId = 6, NombreEstado = "Reactivada", Descripcion = "Reactivada después de suspensión/cancelación.", EsFinal = false },
            new EstadoSuscripcion { EstadoSuscripcionId = 7, NombreEstado = "Prueba", Descripcion = "Periodo de prueba gratuita.", EsFinal = false }
        );
    }
}
