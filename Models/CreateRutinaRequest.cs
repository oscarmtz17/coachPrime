public class CreateRutinaRequest
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int ClienteId { get; set; }
    public int UsuarioId { get; set; }

    // Lista de días de entrenamiento
    public List<DiaEntrenamientoRequest> DiasEntrenamiento { get; set; }
}

public class DiaEntrenamientoRequest
{
    public string DiaSemana { get; set; }  // Ejemplo: "Lunes", "Martes", etc.
    public List<AgrupacionRequest> Agrupaciones { get; set; }  // Lista de agrupaciones de ejercicios
}

public class AgrupacionRequest
{
    public string Tipo { get; set; }  // Ejemplo: "Bi-serie", "Circuito"
    public List<EjercicioRequest> Ejercicios { get; set; }  // Lista de ejercicios dentro de la agrupación
}

public class EjercicioRequest
{
    public string Nombre { get; set; }
    public int Series { get; set; }
    public int Repeticiones { get; set; }
    public string ImagenUrl { get; set; }
}
