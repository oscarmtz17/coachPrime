using Microsoft.EntityFrameworkCore;
namespace webapi.Services;

public class RutinaService : IRutinaService
{
    private readonly CoachPrimeContext _context;

    public RutinaService(CoachPrimeContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateRutinaAsync(CreateRutinaRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Crear la rutina, días de entrenamiento, agrupaciones y ejercicios (similar al ejemplo anterior)
            var rutina = new Rutina
            {
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                ClienteId = request.ClienteId,
                UsuarioId = request.UsuarioId,
                FechaInicio = DateTime.Now
            };
            _context.Rutinas.Add(rutina);
            await _context.SaveChangesAsync();

            foreach (var dia in request.DiasEntrenamiento)
            {
                var diaEntrenamiento = new DiaEntrenamiento
                {
                    DiaSemana = dia.DiaSemana,
                    RutinaId = rutina.RutinaId
                };
                _context.DiasEntrenamiento.Add(diaEntrenamiento);
                await _context.SaveChangesAsync();

                foreach (var agrupacion in dia.Agrupaciones)
                {
                    var nuevaAgrupacion = new Agrupacion
                    {
                        Tipo = agrupacion.Tipo,
                        DiaEntrenamientoId = diaEntrenamiento.DiaEntrenamientoId
                    };
                    _context.Agrupaciones.Add(nuevaAgrupacion);
                    await _context.SaveChangesAsync();

                    foreach (var ejercicio in agrupacion.Ejercicios)
                    {
                        var nuevoEjercicio = new Ejercicio
                        {
                            Nombre = ejercicio.Nombre,
                            Series = ejercicio.Series,
                            Repeticiones = ejercicio.Repeticiones
                        };
                        _context.Ejercicios.Add(nuevoEjercicio);
                        await _context.SaveChangesAsync();

                        var ejercicioAgrupado = new EjercicioAgrupado
                        {
                            AgrupacionId = nuevaAgrupacion.AgrupacionId,
                            EjercicioId = nuevoEjercicio.EjercicioId
                        };
                        _context.EjercicioAgrupado.Add(ejercicioAgrupado);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<Rutina> GetRutinaByIdAsync(int rutinaId)
    {
        return await _context.Rutinas
            .Include(r => r.DiasEntrenamiento)
            .ThenInclude(d => d.Agrupaciones)
            .ThenInclude(a => a.EjerciciosAgrupados)
            .ThenInclude(ea => ea.Ejercicio)
            .FirstOrDefaultAsync(r => r.RutinaId == rutinaId);
    }

    public async Task<bool> UpdateRutinaAsync(int rutinaId, CreateRutinaRequest request)
    {
        var rutina = await _context.Rutinas
            .Include(r => r.DiasEntrenamiento)
            .ThenInclude(d => d.Agrupaciones)
            .FirstOrDefaultAsync(r => r.RutinaId == rutinaId);

        if (rutina != null)
        {
            rutina.Nombre = request.Nombre;
            rutina.Descripcion = request.Descripcion;

            // Se podría agregar lógica para actualizar los días de entrenamiento y agrupaciones

            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> DeleteRutinaAsync(int rutinaId)
    {
        var rutina = await _context.Rutinas
            .Include(r => r.DiasEntrenamiento)
            .ThenInclude(d => d.Agrupaciones)
            .FirstOrDefaultAsync(r => r.RutinaId == rutinaId);

        if (rutina != null)
        {
            _context.Rutinas.Remove(rutina);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}

public interface IRutinaService
{
    Task<bool> CreateRutinaAsync(CreateRutinaRequest request);
    Task<Rutina> GetRutinaByIdAsync(int rutinaId);
    Task<bool> UpdateRutinaAsync(int rutinaId, CreateRutinaRequest request);
    Task<bool> DeleteRutinaAsync(int rutinaId);
}
