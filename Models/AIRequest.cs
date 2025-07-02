public class AIRequest
{
    public int ClienteId { get; set; }
    public string TipoGeneracion { get; set; } // "Rutina", "Dieta", "Ambos"
    public AIRequestData DatosCliente { get; set; }
    public AIRequestConfiguracion Configuracion { get; set; }
}

public class AIRequestData
{
    // Datos básicos del cliente
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public int Edad { get; set; }
    public string Sexo { get; set; }
    public double PesoActual { get; set; }
    public double Estatura { get; set; }
    public string NivelActividad { get; set; }

    // Objetivos
    public string Objetivo { get; set; } // "Perder grasa", "Ganar masa muscular", "Mantenerse"
    public string ObjetivoEspecifico { get; set; } // Descripción más detallada
    public int SemanasObjetivo { get; set; }

    // Historial de progreso
    public List<AIProgresoData> HistorialProgreso { get; set; } = new List<AIProgresoData>();

    // Rutinas y dietas anteriores
    public List<AIRutinaAnterior> RutinasAnteriores { get; set; } = new List<AIRutinaAnterior>();
    public List<AIDietaAnterior> DietasAnteriores { get; set; } = new List<AIDietaAnterior>();

    // Preferencias y restricciones
    public List<string> PreferenciasAlimentarias { get; set; } = new List<string>(); // "Vegetariano", "Sin gluten", etc.
    public List<string> Alergias { get; set; } = new List<string>();
    public List<string> EjerciciosPreferidos { get; set; } = new List<string>();
    public List<string> EjerciciosEvitar { get; set; } = new List<string>();
    public List<string> Lesiones { get; set; } = new List<string>();

    // Disponibilidad
    public int DiasEntrenamiento { get; set; }
    public int MinutosPorSesion { get; set; }
    public string HorarioPreferido { get; set; } // "Mañana", "Tarde", "Noche"
}

public class AIProgresoData
{
    public DateTime Fecha { get; set; }
    public double Peso { get; set; }
    public double? Cintura { get; set; }
    public double? Cadera { get; set; }
    public double? Pecho { get; set; }
    public double? Brazo { get; set; }
    public double? Pierna { get; set; }
    public string Notas { get; set; }
}

public class AIRutinaAnterior
{
    public string Nombre { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string Descripcion { get; set; }
    public string Evaluacion { get; set; } // "Excelente", "Buena", "Regular", "Mala"
    public string Comentarios { get; set; }
}

public class AIDietaAnterior
{
    public string Nombre { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string Descripcion { get; set; }
    public string Evaluacion { get; set; } // "Excelente", "Buena", "Regular", "Mala"
    public string Comentarios { get; set; }
}

public class AIRequestConfiguracion
{
    public string NivelDificultad { get; set; } // "Principiante", "Intermedio", "Avanzado"
    public string TipoDieta { get; set; } // "Equilibrada", "Alta en proteínas", "Baja en carbohidratos", "Vegetariana"
    public string EnfoqueRutina { get; set; } // "Fuerza", "Cardio", "Flexibilidad", "Mixto"
    public bool IncluirCardio { get; set; } = true;
    public bool IncluirFlexibilidad { get; set; } = true;
    public int CaloriasObjetivo { get; set; }
    public double? ProteinaObjetivo { get; set; } // gramos por kg de peso
    public double? CarbohidratosObjetivo { get; set; } // porcentaje del total
    public double? GrasasObjetivo { get; set; } // porcentaje del total
}