using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace webapi.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _openAiApiKey;
        private readonly string _openAiEndpoint;

        public AIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _openAiApiKey = _configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API Key no configurada");
            _openAiEndpoint = _configuration["OpenAI:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");
        }

        public async Task<AIResponse> GenerarRutinaYDietaAsync(AIRequest request)
        {
            try
            {
                var prompt = ConstruirPrompt(request);
                var response = await LlamarOpenAIAsync(prompt);
                return ParsearRespuesta(response, request.ClienteId, request.TipoGeneracion);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar rutina/dieta con IA: {ex.Message}", ex);
            }
        }

        private string ConstruirPrompt(AIRequest request)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Eres un entrenador personal y nutricionista experto. Necesitas generar una rutina de ejercicios y/o dieta personalizada basada en los siguientes datos del cliente:");
            sb.AppendLine();

            // Datos del cliente
            sb.AppendLine($"DATOS DEL CLIENTE:");
            sb.AppendLine($"- Nombre: {request.DatosCliente.Nombre} {request.DatosCliente.Apellido}");
            sb.AppendLine($"- Edad: {request.DatosCliente.Edad} años");
            sb.AppendLine($"- Sexo: {request.DatosCliente.Sexo}");
            sb.AppendLine($"- Peso actual: {request.DatosCliente.PesoActual} kg");
            sb.AppendLine($"- Estatura: {request.DatosCliente.Estatura} cm");
            sb.AppendLine($"- Nivel de actividad: {request.DatosCliente.NivelActividad}");
            sb.AppendLine($"- Objetivo: {request.DatosCliente.Objetivo}");
            sb.AppendLine($"- Objetivo específico: {request.DatosCliente.ObjetivoEspecifico}");
            sb.AppendLine($"- Semanas objetivo: {request.DatosCliente.SemanasObjetivo}");
            sb.AppendLine($"- Días de entrenamiento disponibles: {request.DatosCliente.DiasEntrenamiento}");
            sb.AppendLine($"- Minutos por sesión: {request.DatosCliente.MinutosPorSesion}");
            sb.AppendLine($"- Horario preferido: {request.DatosCliente.HorarioPreferido}");
            sb.AppendLine();

            // Historial de progreso
            if (request.DatosCliente.HistorialProgreso.Any())
            {
                sb.AppendLine("HISTORIAL DE PROGRESO:");
                foreach (var progreso in request.DatosCliente.HistorialProgreso.OrderByDescending(p => p.Fecha).Take(5))
                {
                    sb.AppendLine($"- {progreso.Fecha:dd/MM/yyyy}: Peso {progreso.Peso}kg" +
                        (progreso.Cintura.HasValue ? $", Cintura {progreso.Cintura}cm" : "") +
                        (progreso.Cadera.HasValue ? $", Cadera {progreso.Cadera}cm" : "") +
                        (progreso.Pecho.HasValue ? $", Pecho {progreso.Pecho}cm" : "") +
                        (progreso.Brazo.HasValue ? $", Brazo {progreso.Brazo}cm" : "") +
                        (progreso.Pierna.HasValue ? $", Pierna {progreso.Pierna}cm" : "") +
                        (!string.IsNullOrEmpty(progreso.Notas) ? $", Notas: {progreso.Notas}" : ""));
                }
                sb.AppendLine();
            }

            // Preferencias y restricciones
            if (request.DatosCliente.PreferenciasAlimentarias.Any())
            {
                sb.AppendLine($"PREFERENCIAS ALIMENTARIAS: {string.Join(", ", request.DatosCliente.PreferenciasAlimentarias)}");
            }
            if (request.DatosCliente.Alergias.Any())
            {
                sb.AppendLine($"ALERGIAS: {string.Join(", ", request.DatosCliente.Alergias)}");
            }
            if (request.DatosCliente.EjerciciosPreferidos.Any())
            {
                sb.AppendLine($"EJERCICIOS PREFERIDOS: {string.Join(", ", request.DatosCliente.EjerciciosPreferidos)}");
            }
            if (request.DatosCliente.EjerciciosEvitar.Any())
            {
                sb.AppendLine($"EJERCICIOS A EVITAR: {string.Join(", ", request.DatosCliente.EjerciciosEvitar)}");
            }
            if (request.DatosCliente.Lesiones.Any())
            {
                sb.AppendLine($"LESIONES: {string.Join(", ", request.DatosCliente.Lesiones)}");
            }
            sb.AppendLine();

            // Configuración
            sb.AppendLine("CONFIGURACIÓN:");
            sb.AppendLine($"- Nivel de dificultad: {request.Configuracion.NivelDificultad}");
            sb.AppendLine($"- Tipo de dieta: {request.Configuracion.TipoDieta}");
            sb.AppendLine($"- Enfoque de rutina: {request.Configuracion.EnfoqueRutina}");
            sb.AppendLine($"- Incluir cardio: {request.Configuracion.IncluirCardio}");
            sb.AppendLine($"- Incluir flexibilidad: {request.Configuracion.IncluirFlexibilidad}");
            if (request.Configuracion.CaloriasObjetivo > 0)
            {
                sb.AppendLine($"- Calorías objetivo: {request.Configuracion.CaloriasObjetivo}");
            }
            sb.AppendLine();

            // Instrucciones específicas
            sb.AppendLine("INSTRUCCIONES:");
            sb.AppendLine($"Genera {request.TipoGeneracion.ToLower()} en formato JSON. La respuesta debe ser un objeto JSON válido con la siguiente estructura:");

            if (request.TipoGeneracion == "Rutina" || request.TipoGeneracion == "Ambos")
            {
                sb.AppendLine(@"
{
  ""rutina"": {
    ""nombre"": ""Nombre de la rutina"",
    ""descripcion"": ""Descripción general"",
    ""justificacion"": ""Por qué se eligió esta rutina"",
    ""diasEntrenamiento"": [
      {
        ""diaSemana"": ""Lunes"",
        ""enfoque"": ""Fuerza superior"",
        ""duracionEstimada"": 60,
        ""notas"": ""Notas específicas"",
        ""agrupaciones"": [
          {
            ""tipo"": ""Calentamiento"",
            ""descripcion"": ""Descripción del grupo"",
            ""ejercicios"": [
              {
                ""nombre"": ""Nombre del ejercicio"",
                ""descripcion"": ""Descripción del ejercicio"",
                ""series"": 3,
                ""repeticiones"": 12,
                ""peso"": ""Peso corporal"",
                ""descanso"": 60,
                ""justificacion"": ""Por qué se eligió este ejercicio""
              }
            ]
          }
        ]
      }
    ],
    ""configuracion"": {
      ""semanasDuracion"": 8,
      ""diasPorSemana"": 4,
      ""progresion"": ""Lineal"",
      ""objetivoEspecifico"": ""Descripción del objetivo""
    }
  }");
            }

            if (request.TipoGeneracion == "Dieta" || request.TipoGeneracion == "Ambos")
            {
                sb.AppendLine(@",
  ""dieta"": {
    ""nombre"": ""Nombre de la dieta"",
    ""descripcion"": ""Descripción general"",
    ""justificacion"": ""Por qué se eligió esta dieta"",
    ""comidas"": [
      {
        ""nombre"": ""Desayuno"",
        ""hora"": ""7:00 AM"",
        ""descripcion"": ""Descripción de la comida"",
        ""calorias"": 400,
        ""proteinas"": 25,
        ""carbohidratos"": 45,
        ""grasas"": 15,
        ""notas"": ""Notas específicas"",
        ""alimentos"": [
          {
            ""nombre"": ""Avena"",
            ""cantidad"": 50,
            ""unidad"": ""gramos"",
            ""calorias"": 180,
            ""proteinas"": 6,
            ""carbohidratos"": 30,
            ""grasas"": 3,
            ""justificacion"": ""Por qué se eligió este alimento"",
            ""sustitutos"": [""Quinoa"", ""Amaranto""]
          }
        ]
      }
    ],
    ""configuracion"": {
      ""caloriasTotales"": 2000,
      ""proteinasTotales"": 150,
      ""carbohidratosTotales"": 200,
      ""grasasTotales"": 67,
      ""comidasPorDia"": 5,
      ""tipoDieta"": ""Equilibrada"",
      ""restricciones"": [""Sin gluten""]
    },
    ""consejos"": [""Consejo 1"", ""Consejo 2""]
  }");
            }

            sb.AppendLine(@",
  ""analisis"": {
    ""resumenCliente"": ""Resumen del cliente"",
    ""evaluacionProgreso"": ""Evaluación del progreso"",
    ""identificacionPatrones"": ""Patrones identificados"",
    ""fortalezas"": [""Fortaleza 1"", ""Fortaleza 2""],
    ""areasMejora"": [""Área 1"", ""Área 2""],
    ""prediccionResultados"": ""Predicción de resultados""
  },
  ""recomendaciones"": [""Recomendación 1"", ""Recomendación 2""],
  ""confianza"": 0.85
}");

            sb.AppendLine();
            sb.AppendLine("IMPORTANTE: Responde ÚNICAMENTE con el JSON válido, sin texto adicional.");

            return sb.ToString();
        }

        private async Task<string> LlamarOpenAIAsync(string prompt)
        {
            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = "Eres un entrenador personal y nutricionista experto. Responde ÚNICAMENTE con JSON válido." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 4000
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_openAiEndpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var openAiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

            return openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "";
        }

        private AIResponse ParsearRespuesta(string response, int clienteId, string tipoGeneracion)
        {
            try
            {
                // Limpiar la respuesta de posibles caracteres extra
                var cleanResponse = response.Trim();
                if (cleanResponse.StartsWith("```json"))
                {
                    cleanResponse = cleanResponse.Substring(7);
                }
                if (cleanResponse.EndsWith("```"))
                {
                    cleanResponse = cleanResponse.Substring(0, cleanResponse.Length - 3);
                }
                cleanResponse = cleanResponse.Trim();

                var aiResponse = JsonSerializer.Deserialize<AIResponse>(cleanResponse);
                if (aiResponse != null)
                {
                    aiResponse.ClienteId = clienteId;
                    aiResponse.TipoGeneracion = tipoGeneracion;
                }

                return aiResponse ?? new AIResponse
                {
                    ClienteId = clienteId,
                    TipoGeneracion = tipoGeneracion,
                    Analisis = new AIAnalisis { ResumenCliente = "Error al procesar respuesta de IA" },
                    Confianza = 0.0
                };
            }
            catch (Exception ex)
            {
                return new AIResponse
                {
                    ClienteId = clienteId,
                    TipoGeneracion = tipoGeneracion,
                    Analisis = new AIAnalisis { ResumenCliente = $"Error al parsear respuesta: {ex.Message}" },
                    Confianza = 0.0
                };
            }
        }
    }

    public interface IAIService
    {
        Task<AIResponse> GenerarRutinaYDietaAsync(AIRequest request);
    }

    // Clases para deserializar la respuesta de OpenAI
    public class OpenAIResponse
    {
        public List<OpenAIChoice> Choices { get; set; } = new List<OpenAIChoice>();
    }

    public class OpenAIChoice
    {
        public OpenAIMessage Message { get; set; } = new OpenAIMessage();
    }

    public class OpenAIMessage
    {
        public string Content { get; set; } = "";
    }
}