using Microsoft.EntityFrameworkCore;
using webapi;

public class ProgresoService : IProgresoService
{
    private readonly CoachPrimeContext _context;

    public ProgresoService(CoachPrimeContext context)
    {
        _context = context;
    }

    // Obtener todos los progresos de un cliente
    public async Task<IEnumerable<Progreso>> GetAllProgresosAsync(int clienteId)
    {
        return await _context.Progresos.Where(p => p.ClienteId == clienteId).ToListAsync();
    }

    // Obtener un progreso específico
    public async Task<Progreso> GetProgresoByIdAsync(int clienteId, int progresoId)
    {
        return await _context.Progresos.FirstOrDefaultAsync(p => p.ClienteId == clienteId && p.ProgresoId == progresoId);
    }

    // Registrar un nuevo progreso
    public async Task<bool> RegistrarProgresoAsync(int clienteId, ProgresoRequest request)
    {
        var cliente = await _context.Clientes.FindAsync(clienteId);
        if (cliente == null) return false;

        int edad = DateTime.Now.Year - cliente.FechaNacimiento.Year;
        if (DateTime.Now.DayOfYear < cliente.FechaNacimiento.DayOfYear) edad--;

        double tmb = (cliente.Sexo == "Masculino")
            ? 66 + (13.7 * request.PesoKg) + (5 * request.EstaturaCm) - (6.8 * edad)
            : 655 + (9.6 * request.PesoKg) + (1.8 * request.EstaturaCm) - (4.7 * edad);

        double caloriasDiarias = tmb * request.FactorActividad;

        var progreso = new Progreso
        {
            ClienteId = clienteId,
            PesoKg = request.PesoKg,
            EstaturaCm = request.EstaturaCm,
            NivelActividad = request.NivelActividad,
            FactorActividad = request.FactorActividad,
            TMB = tmb,
            CaloriasDiarias = caloriasDiarias,
            CinturaCm = request.CinturaCm,
            CaderaCm = request.CaderaCm,
            PechoCm = request.PechoCm,
            BrazoCm = request.BrazoCm,
            PiernaCm = request.PiernaCm,
            Notas = request.Notas,
            FechaRegistro = DateTime.Now
        };

        _context.Progresos.Add(progreso);
        await _context.SaveChangesAsync();
        return true;
    }

    // Actualizar un progreso existente
    public async Task<bool> UpdateProgresoAsync(int clienteId, int progresoId, ProgresoRequest request)
    {
        var progresoActual = await _context.Progresos.FirstOrDefaultAsync(p => p.ClienteId == clienteId && p.ProgresoId == progresoId);
        if (progresoActual == null) return false;

        progresoActual.PesoKg = request.PesoKg;
        progresoActual.EstaturaCm = request.EstaturaCm;
        progresoActual.NivelActividad = request.NivelActividad;
        progresoActual.FactorActividad = request.FactorActividad;
        progresoActual.CinturaCm = request.CinturaCm;
        progresoActual.CaderaCm = request.CaderaCm;
        progresoActual.PechoCm = request.PechoCm;
        progresoActual.BrazoCm = request.BrazoCm;
        progresoActual.PiernaCm = request.PiernaCm;
        progresoActual.Notas = request.Notas;

        await _context.SaveChangesAsync();
        return true;
    }

    // Eliminar un progreso
    public async Task<bool> DeleteProgresoAsync(int clienteId, int progresoId)
    {
        var progreso = await _context.Progresos.FirstOrDefaultAsync(p => p.ClienteId == clienteId && p.ProgresoId == progresoId);
        if (progreso == null) return false;

        _context.Progresos.Remove(progreso);
        await _context.SaveChangesAsync();
        return true;
    }
}

public interface IProgresoService
{
    Task<IEnumerable<Progreso>> GetAllProgresosAsync(int clienteId);
    Task<Progreso> GetProgresoByIdAsync(int clienteId, int progresoId);
    Task<bool> RegistrarProgresoAsync(int clienteId, ProgresoRequest request);
    Task<bool> UpdateProgresoAsync(int clienteId, int progresoId, ProgresoRequest request);
    Task<bool> DeleteProgresoAsync(int clienteId, int progresoId);
}
