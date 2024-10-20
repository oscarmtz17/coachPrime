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
        foreach (var comidaReq in request.Comidas)
        {
            var comidaActual = dieta.Comidas.FirstOrDefault(c => c.Nombre == comidaReq.Nombre);
            if (comidaActual != null)
            {
                comidaActual.Hora = comidaReq.Hora;

                foreach (var alimentoReq in comidaReq.Alimentos)
                {
                    var alimentoActual = comidaActual.Alimentos.FirstOrDefault(a => a.Nombre == alimentoReq.Nombre);
                    if (alimentoActual != null)
                    {
                        alimentoActual.Cantidad = alimentoReq.Cantidad;
                        alimentoActual.Unidad = alimentoReq.Unidad;
                    }
                }
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
