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
    }
}
