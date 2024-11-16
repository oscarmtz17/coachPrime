using Microsoft.EntityFrameworkCore;
using webapi;

public class ProgresoService : IProgresoService
{
    private readonly CoachPrimeContext _context;
    private readonly S3Service _s3Service;


    public ProgresoService(CoachPrimeContext context, S3Service s3Service)
    {
        _context = context;
        _s3Service = s3Service;
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
    public async Task<int?> RegistrarProgresoAsync(int clienteId, ProgresoRequest request)
    {
        var cliente = await _context.Clientes.FindAsync(clienteId);
        if (cliente == null) return null;

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

        return progreso.ProgresoId; // Devolver el ID del progreso recién creado
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

    // Eliminar un progreso y su carpeta asociada en S3
    public async Task<bool> DeleteProgresoAsync(int clienteId, int progresoId, string userId)
    {
        var progreso = await _context.Progresos.FirstOrDefaultAsync(p => p.ClienteId == clienteId && p.ProgresoId == progresoId);
        if (progreso == null) return false;

        // Eliminar carpeta asociada en S3
        var folderKey = $"private/{userId}/progress/{progresoId}/";
        await _s3Service.DeleteFolderAsync(folderKey);

        // Eliminar el progreso de la base de datos
        _context.Progresos.Remove(progreso);
        await _context.SaveChangesAsync();

        return true;
    }

}

public interface IProgresoService
{
    Task<IEnumerable<Progreso>> GetAllProgresosAsync(int clienteId);
    Task<Progreso> GetProgresoByIdAsync(int clienteId, int progresoId);
    Task<int?> RegistrarProgresoAsync(int clienteId, ProgresoRequest request);
    Task<bool> UpdateProgresoAsync(int clienteId, int progresoId, ProgresoRequest request);
    Task<bool> DeleteProgresoAsync(int clienteId, int progresoId, string userId); // Aquí está el ajuste
}


