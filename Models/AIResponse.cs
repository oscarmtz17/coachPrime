public class AIResponse
{
    public int ClienteId { get; set; }
    public string TipoGeneracion { get; set; }
    public AIRutinaGenerada? Rutina { get; set; }
    public AIDietaGenerada? Dieta { get; set; }
    public AIAnalisis Analisis { get; set; }
    public List<string> Recomendaciones { get; set; } = new List<string>();
    public double Confianza { get; set; } // 0.0 a 1.0
}

public class AIRutinaGenerada
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string Justificacion { get; set; } // Por qué se eligió esta rutina
    public List<AIDiaEntrenamiento> DiasEntrenamiento { get; set; } = new List<AIDiaEntrenamiento>();
    public AIRutinaConfiguracion Configuracion { get; set; }
}

public class AIDiaEntrenamiento
{
    public string DiaSemana { get; set; }
    public string Enfoque { get; set; } // "Fuerza superior", "Cardio", etc.
    public List<AIAgrupacion> Agrupaciones { get; set; } = new List<AIAgrupacion>();
    public int DuracionEstimada { get; set; } // en minutos
    public string Notas { get; set; }
}

public class AIAgrupacion
{
    public string Tipo { get; set; } // "Calentamiento", "Ejercicio principal", "Cardio", "Enfriamiento"
    public List<AIEjercicio> Ejercicios { get; set; } = new List<AIEjercicio>();
    public string Descripcion { get; set; }
}

public class AIEjercicio
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int Series { get; set; }
    public int Repeticiones { get; set; }
    public string? Peso { get; set; } // "Peso corporal", "10kg", etc.
    public int? Descanso { get; set; } // segundos entre series
    public string Justificacion { get; set; } // Por qué se eligió este ejercicio
    public string? ImagenKey { get; set; }
    public string? ImagenUrl { get; set; }
}

public class AIRutinaConfiguracion
{
    public int SemanasDuracion { get; set; }
    public int DiasPorSemana { get; set; }
    public string Progresion { get; set; } // "Lineal", "Ondulante", "Piramidal"
    public string ObjetivoEspecifico { get; set; }
}

public class AIDietaGenerada
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string Justificacion { get; set; } // Por qué se eligió esta dieta
    public List<AIComida> Comidas { get; set; } = new List<AIComida>();
    public AIDietaConfiguracion Configuracion { get; set; }
    public List<string> Consejos { get; set; } = new List<string>();
}

public class AIComida
{
    public string Nombre { get; set; }
    public string Hora { get; set; }
    public string Descripcion { get; set; }
    public List<AIAlimento> Alimentos { get; set; } = new List<AIAlimento>();
    public int Calorias { get; set; }
    public double Proteinas { get; set; }
    public double Carbohidratos { get; set; }
    public double Grasas { get; set; }
    public string Notas { get; set; }
}

public class AIAlimento
{
    public string Nombre { get; set; }
    public double Cantidad { get; set; }
    public string Unidad { get; set; }
    public int Calorias { get; set; }
    public double Proteinas { get; set; }
    public double Carbohidratos { get; set; }
    public double Grasas { get; set; }
    public string Justificacion { get; set; } // Por qué se eligió este alimento
    public List<string> Sustitutos { get; set; } = new List<string>();
}

public class AIDietaConfiguracion
{
    public int CaloriasTotales { get; set; }
    public double ProteinasTotales { get; set; }
    public double CarbohidratosTotales { get; set; }
    public double GrasasTotales { get; set; }
    public int ComidasPorDia { get; set; }
    public string TipoDieta { get; set; }
    public List<string> Restricciones { get; set; } = new List<string>();
}

public class AIAnalisis
{
    public string ResumenCliente { get; set; }
    public string EvaluacionProgreso { get; set; }
    public string IdentificacionPatrones { get; set; }
    public List<string> Fortalezas { get; set; } = new List<string>();
    public List<string> AreasMejora { get; set; } = new List<string>();
    public string PrediccionResultados { get; set; }
}