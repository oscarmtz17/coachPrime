using Microsoft.EntityFrameworkCore;
using webapi;

public class DietaService : IDietaService
{
    private readonly CoachPrimeContext _context;

    public DietaService(CoachPrimeContext context)
    {
        _context = context;
    }

    // Obtener todas las dietas de un cliente
    public async Task<IEnumerable<Dieta>> GetAllDietasAsync(int clienteId)
    {
        return await _context.Dietas
            .Include(d => d.Comidas)
                .ThenInclude(c => c.Alimentos)
            .Where(d => d.ClienteId == clienteId)
            .ToListAsync();
    }

    // Obtener una dieta específica
    public async Task<Dieta> GetDietaByIdAsync(int clienteId, int dietaId)
    {
        return await _context.Dietas
            .Include(d => d.Comidas)
                .ThenInclude(c => c.Alimentos)
            .FirstOrDefaultAsync(d => d.ClienteId == clienteId && d.DietaId == dietaId);
    }

    // Registrar una nueva dieta
    public async Task<bool> RegistrarDietaAsync(int clienteId, DietaRequest request)
    {
        var cliente = await _context.Clientes.FindAsync(clienteId);
        if (cliente == null) return false;

        // Validar que haya al menos un alimento
        if (!request.Comidas.Any(c => c.Alimentos.Any()))
        {
            throw new Exception("Debes agregar al menos un alimento a la dieta.");
        }

        var dieta = new Dieta
        {
            ClienteId = clienteId,
            Nombre = request.Nombre,
            Notas = request.Notas,
            Comidas = request.Comidas.Select(c => new Comida
            {
                Nombre = c.Nombre,
                Hora = c.Hora,
                Alimentos = c.Alimentos.Select(a => new Alimento
                {
                    Nombre = a.Nombre,
                    Cantidad = a.Cantidad,
                    Unidad = a.Unidad
                }).ToList()
            }).ToList()
        };

        _context.Dietas.Add(dieta);
        await _context.SaveChangesAsync();
        return true;
    }


    // Actualizar una dieta existente
    public async Task<bool> UpdateDietaAsync(int clienteId, int dietaId, DietaRequest request)
    {
        var dieta = await _context.Dietas
            .Include(d => d.Comidas)
                .ThenInclude(c => c.Alimentos)
            .FirstOrDefaultAsync(d => d.ClienteId == clienteId && d.DietaId == dietaId);

        if (dieta == null) return false;

        dieta.Nombre = request.Nombre;
        dieta.Notas = request.Notas;

        // Actualización de las comidas y alimentos
        // 1. Eliminar comidas que no están en la solicitud
        var comidasToDelete = dieta.Comidas.Where(c => !request.Comidas.Any(rc => rc.Nombre == c.Nombre)).ToList();
        foreach (var comida in comidasToDelete)
        {
            _context.Comidas.Remove(comida);
        }

        // 2. Actualizar o agregar comidas de la solicitud
        foreach (var comidaReq in request.Comidas)
        {
            var comidaActual = dieta.Comidas.FirstOrDefault(c => c.Nombre == comidaReq.Nombre);

            if (comidaActual != null)
            {
                // Actualizar comida existente
                comidaActual.Hora = comidaReq.Hora;

                // Eliminar alimentos que no están en la solicitud
                var alimentosToDelete = comidaActual.Alimentos.Where(a => !comidaReq.Alimentos.Any(ra => ra.Nombre == a.Nombre)).ToList();
                foreach (var alimento in alimentosToDelete)
                {
                    _context.Alimentos.Remove(alimento);
                }

                // Actualizar o agregar alimentos
                foreach (var alimentoReq in comidaReq.Alimentos)
                {
                    var alimentoActual = comidaActual.Alimentos.FirstOrDefault(a => a.Nombre == alimentoReq.Nombre);
                    if (alimentoActual != null)
                    {
                        // Actualizar alimento existente
                        alimentoActual.Cantidad = alimentoReq.Cantidad;
                        alimentoActual.Unidad = alimentoReq.Unidad;
                    }
                    else
                    {
                        // Agregar nuevo alimento
                        comidaActual.Alimentos.Add(new Alimento
                        {
                            Nombre = alimentoReq.Nombre,
                            Cantidad = alimentoReq.Cantidad,
                            Unidad = alimentoReq.Unidad
                        });
                    }
                }
            }
            else
            {
                // Agregar nueva comida con sus alimentos
                var nuevaComida = new Comida
                {
                    Nombre = comidaReq.Nombre,
                    Hora = comidaReq.Hora,
                    Alimentos = comidaReq.Alimentos.Select(a => new Alimento
                    {
                        Nombre = a.Nombre,
                        Cantidad = a.Cantidad,
                        Unidad = a.Unidad
                    }).ToList()
                };
                dieta.Comidas.Add(nuevaComida);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // Eliminar una dieta
    public async Task<bool> DeleteDietaAsync(int clienteId, int dietaId)
    {
        var dieta = await _context.Dietas.FirstOrDefaultAsync(d => d.ClienteId == clienteId && d.DietaId == dietaId);
        if (dieta == null) return false;

        _context.Dietas.Remove(dieta);
        await _context.SaveChangesAsync();
        return true;
    }
}

public interface IDietaService
{
    Task<IEnumerable<Dieta>> GetAllDietasAsync(int clienteId);
    Task<Dieta> GetDietaByIdAsync(int clienteId, int dietaId);
    Task<bool> RegistrarDietaAsync(int clienteId, DietaRequest request);
    Task<bool> UpdateDietaAsync(int clienteId, int dietaId, DietaRequest request);
    Task<bool> DeleteDietaAsync(int clienteId, int dietaId);
}
