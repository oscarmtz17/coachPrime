using Microsoft.EntityFrameworkCore;

namespace webapi.Services
{
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
                // Validar que haya al menos un ejercicio
                if (!request.DiasEntrenamiento.Any(d => d.Agrupaciones.Any(a => a.Ejercicios.Any())))
                {
                    throw new Exception("Debes agregar al menos un ejercicio a tu rutina.");
                }

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
                                Repeticiones = ejercicio.Repeticiones,
                                ImagenUrl = ejercicio.ImagenKey
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
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.Error.WriteLine($"Error en CreateRutinaAsync: {ex.Message}");
                throw; // Re-lanzamos la excepción para que sea manejada en el controlador
            }
        }


        public async Task<Rutina> GetRutinaByIdAsync(int clienteId, int rutinaId)
        {
            return await _context.Rutinas
                .Include(r => r.DiasEntrenamiento)
                    .ThenInclude(d => d.Agrupaciones)
                        .ThenInclude(a => a.EjerciciosAgrupados)
                            .ThenInclude(ea => ea.Ejercicio)
                .FirstOrDefaultAsync(r => r.ClienteId == clienteId && r.RutinaId == rutinaId);
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

        public async Task<List<RutinaBasicInfo>> GetRutinasByClienteIdAsync(int clienteId)
        {
            return await _context.Rutinas
                .Where(r => r.ClienteId == clienteId)
                .Select(r => new RutinaBasicInfo
                {
                    RutinaId = r.RutinaId,
                    Nombre = r.Nombre,
                    Descripcion = r.Descripcion,
                    FechaInicio = r.FechaInicio
                })
                .ToListAsync();
        }



        public async Task<bool> UpdateRutinaAsync(int rutinaId, CreateRutinaRequest request)
        {
            // Validar que haya al menos un ejercicio en la solicitud
            if (!request.DiasEntrenamiento.Any(d => d.Agrupaciones.Any(a => a.Ejercicios.Any())))
            {
                throw new Exception("Debes agregar al menos un ejercicio a tu rutina.");
            }

            var rutina = await _context.Rutinas
                .Include(r => r.DiasEntrenamiento)
                    .ThenInclude(d => d.Agrupaciones)
                        .ThenInclude(a => a.EjerciciosAgrupados)
                            .ThenInclude(ea => ea.Ejercicio)
                .FirstOrDefaultAsync(r => r.RutinaId == rutinaId);

            if (rutina == null)
            {
                return false;
            }

            rutina.Nombre = request.Nombre;
            rutina.Descripcion = request.Descripcion;

            // Actualizar días de entrenamiento
            foreach (var diaRequest in request.DiasEntrenamiento)
            {
                var diaExistente = rutina.DiasEntrenamiento.FirstOrDefault(d => d.DiaSemana == diaRequest.DiaSemana);

                // Si el día de entrenamiento no existe, se agrega
                if (diaExistente == null)
                {
                    diaExistente = new DiaEntrenamiento
                    {
                        DiaSemana = diaRequest.DiaSemana,
                        RutinaId = rutinaId
                    };
                    _context.DiasEntrenamiento.Add(diaExistente);
                    await _context.SaveChangesAsync();
                }

                // Actualizar agrupaciones dentro de cada día
                foreach (var agrupacionRequest in diaRequest.Agrupaciones)
                {
                    var agrupacionExistente = diaExistente.Agrupaciones.FirstOrDefault(a => a.Tipo == agrupacionRequest.Tipo);

                    // Si la agrupación no existe, se agrega
                    if (agrupacionExistente == null)
                    {
                        agrupacionExistente = new Agrupacion
                        {
                            Tipo = agrupacionRequest.Tipo,
                            DiaEntrenamientoId = diaExistente.DiaEntrenamientoId
                        };
                        _context.Agrupaciones.Add(agrupacionExistente);
                        await _context.SaveChangesAsync();
                    }

                    // Actualizar ejercicios en cada agrupación
                    var ejerciciosExistentes = agrupacionExistente.EjerciciosAgrupados.ToList();
                    foreach (var ejercicioRequest in agrupacionRequest.Ejercicios)
                    {
                        var ejercicioExistente = ejerciciosExistentes.FirstOrDefault(e => e.Ejercicio.Nombre == ejercicioRequest.Nombre);

                        if (ejercicioExistente == null)
                        {
                            // Si el ejercicio no existe, se agrega
                            var nuevoEjercicio = new Ejercicio
                            {
                                Nombre = ejercicioRequest.Nombre,
                                Descripcion = ejercicioRequest.Descripcion,
                                Series = ejercicioRequest.Series,
                                Repeticiones = ejercicioRequest.Repeticiones,
                                ImagenUrl = ejercicioRequest.ImagenKey
                            };
                            _context.Ejercicios.Add(nuevoEjercicio);
                            await _context.SaveChangesAsync();

                            // Crear la relación en EjercicioAgrupado
                            var nuevoEjercicioAgrupado = new EjercicioAgrupado
                            {
                                AgrupacionId = agrupacionExistente.AgrupacionId,
                                EjercicioId = nuevoEjercicio.EjercicioId
                            };
                            _context.EjercicioAgrupado.Add(nuevoEjercicioAgrupado);
                        }
                        else
                        {
                            // Si el ejercicio existe, se actualiza
                            ejercicioExistente.Ejercicio.Series = ejercicioRequest.Series;
                            ejercicioExistente.Ejercicio.Repeticiones = ejercicioRequest.Repeticiones;
                            ejercicioExistente.Ejercicio.ImagenUrl = ejercicioRequest.ImagenKey;
                        }
                    }

                    // Eliminar ejercicios no presentes en la solicitud
                    foreach (var ejercicioExistente in ejerciciosExistentes)
                    {
                        if (!agrupacionRequest.Ejercicios.Any(e => e.Nombre == ejercicioExistente.Ejercicio.Nombre))
                        {
                            _context.Ejercicios.Remove(ejercicioExistente.Ejercicio);
                            _context.EjercicioAgrupado.Remove(ejercicioExistente);
                        }
                    }
                }

                // Eliminar agrupaciones no presentes en la solicitud
                foreach (var agrupacionExistente in diaExistente.Agrupaciones.ToList())
                {
                    if (!diaRequest.Agrupaciones.Any(a => a.Tipo == agrupacionExistente.Tipo))
                    {
                        _context.Agrupaciones.Remove(agrupacionExistente);
                    }
                }
            }

            // Eliminar días de entrenamiento no presentes en la solicitud
            foreach (var diaExistente in rutina.DiasEntrenamiento.ToList())
            {
                if (!request.DiasEntrenamiento.Any(d => d.DiaSemana == diaExistente.DiaSemana))
                {
                    _context.DiasEntrenamiento.Remove(diaExistente);
                }
            }

            await _context.SaveChangesAsync();
            return true;
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
        Task<bool> UpdateRutinaAsync(int rutinaId, CreateRutinaRequest request);
        Task<bool> DeleteRutinaAsync(int rutinaId);
        Task<Rutina> GetRutinaByIdAsync(int clienteId, int rutinaId);
        Task<Rutina> GetRutinaByIdAsync(int rutinaId);
        Task<List<RutinaBasicInfo>> GetRutinasByClienteIdAsync(int clienteId);
    }
}
