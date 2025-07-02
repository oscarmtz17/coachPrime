using Microsoft.EntityFrameworkCore;

namespace webapi.Services
{
    public class AIDataService : IAIDataService
    {
        private readonly CoachPrimeContext _context;

        public AIDataService(CoachPrimeContext context)
        {
            _context = context;
        }

        public async Task<AIRequest> PrepararDatosParaIAAsync(int clienteId, string tipoGeneracion, AIRequestConfiguracion configuracion)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Progresos.OrderByDescending(p => p.FechaRegistro))
                .Include(c => c.Rutinas)
                .Include(c => c.Dietas)
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

            if (cliente == null)
                throw new Exception("Cliente no encontrado");

            // Obtener el progreso más reciente
            var progresoReciente = cliente.Progresos.FirstOrDefault();
            if (progresoReciente == null)
                throw new Exception("No hay datos de progreso para el cliente");

            // Calcular edad
            var edad = DateTime.Now.Year - cliente.FechaNacimiento.Year;
            if (DateTime.Now < cliente.FechaNacimiento.AddYears(edad))
                edad--;

            // Preparar datos del cliente
            var datosCliente = new AIRequestData
            {
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Edad = edad,
                Sexo = cliente.Sexo ?? "No especificado",
                PesoActual = progresoReciente.PesoKg,
                Estatura = progresoReciente.EstaturaCm,
                NivelActividad = progresoReciente.NivelActividad,

                // Valores por defecto para objetivos (se pueden modificar desde el frontend)
                Objetivo = "Mantenerse",
                ObjetivoEspecifico = "Mejorar condición física general",
                SemanasObjetivo = 8,
                DiasEntrenamiento = 3,
                MinutosPorSesion = 60,
                HorarioPreferido = "Mañana",

                // Historial de progreso
                HistorialProgreso = cliente.Progresos
                    .OrderByDescending(p => p.FechaRegistro)
                    .Take(10)
                    .Select(p => new AIProgresoData
                    {
                        Fecha = p.FechaRegistro,
                        Peso = p.PesoKg,
                        Cintura = p.CinturaCm,
                        Cadera = p.CaderaCm,
                        Pecho = p.PechoCm,
                        Brazo = p.BrazoCm,
                        Pierna = p.PiernaCm,
                        Notas = p.Notas
                    }).ToList(),

                // Rutinas anteriores
                RutinasAnteriores = cliente.Rutinas
                    .OrderByDescending(r => r.FechaInicio)
                    .Take(5)
                    .Select(r => new AIRutinaAnterior
                    {
                        Nombre = r.Nombre,
                        FechaInicio = r.FechaInicio,
                        FechaFin = r.FechaFin,
                        Descripcion = r.Descripcion ?? "",
                        Evaluacion = "Buena", // Por defecto, se puede mejorar con feedback del usuario
                        Comentarios = ""
                    }).ToList(),

                // Dietas anteriores
                DietasAnteriores = cliente.Dietas
                    .OrderByDescending(d => d.FechaAsignacion)
                    .Take(5)
                    .Select(d => new AIDietaAnterior
                    {
                        Nombre = d.Nombre,
                        FechaInicio = d.FechaAsignacion,
                        FechaFin = null, // Las dietas no tienen fecha fin en el modelo actual
                        Descripcion = d.Notas,
                        Evaluacion = "Buena", // Por defecto
                        Comentarios = ""
                    }).ToList(),

                // Preferencias y restricciones (por defecto vacías, se pueden agregar campos al modelo Cliente)
                PreferenciasAlimentarias = new List<string>(),
                Alergias = new List<string>(),
                EjerciciosPreferidos = new List<string>(),
                EjerciciosEvitar = new List<string>(),
                Lesiones = new List<string>()
            };

            // Aplicar configuración personalizada si se proporciona
            if (configuracion != null)
            {
                datosCliente.DiasEntrenamiento = configuracion.NivelDificultad switch
                {
                    "Principiante" => 3,
                    "Intermedio" => 4,
                    "Avanzado" => 5,
                    _ => 3
                };
            }

            return new AIRequest
            {
                ClienteId = clienteId,
                TipoGeneracion = tipoGeneracion,
                DatosCliente = datosCliente,
                Configuracion = configuracion ?? new AIRequestConfiguracion
                {
                    NivelDificultad = "Intermedio",
                    TipoDieta = "Equilibrada",
                    EnfoqueRutina = "Mixto",
                    IncluirCardio = true,
                    IncluirFlexibilidad = true,
                    CaloriasObjetivo = (int)progresoReciente.CaloriasDiarias
                }
            };
        }

        public async Task<CreateRutinaRequest> ConvertirAIRutinaACreateRutinaRequestAsync(AIRutinaGenerada aiRutina, int clienteId, int usuarioId)
        {
            var request = new CreateRutinaRequest
            {
                Nombre = aiRutina.Nombre,
                Descripcion = aiRutina.Descripcion,
                ClienteId = clienteId,
                UsuarioId = usuarioId,
                DiasEntrenamiento = new List<DiaEntrenamientoRequest>()
            };

            foreach (var diaAI in aiRutina.DiasEntrenamiento)
            {
                var diaRequest = new DiaEntrenamientoRequest
                {
                    DiaSemana = diaAI.DiaSemana,
                    Agrupaciones = new List<AgrupacionRequest>()
                };

                foreach (var agrupacionAI in diaAI.Agrupaciones)
                {
                    var agrupacionRequest = new AgrupacionRequest
                    {
                        Tipo = agrupacionAI.Tipo,
                        Ejercicios = new List<EjercicioRequest>()
                    };

                    foreach (var ejercicioAI in agrupacionAI.Ejercicios)
                    {
                        var ejercicioRequest = new EjercicioRequest
                        {
                            Nombre = ejercicioAI.Nombre,
                            Descripcion = ejercicioAI.Descripcion,
                            Series = ejercicioAI.Series,
                            Repeticiones = ejercicioAI.Repeticiones,
                            ImagenKey = ejercicioAI.ImagenKey
                        };

                        agrupacionRequest.Ejercicios.Add(ejercicioRequest);
                    }

                    diaRequest.Agrupaciones.Add(agrupacionRequest);
                }

                request.DiasEntrenamiento.Add(diaRequest);
            }

            return request;
        }

        public async Task<DietaRequest> ConvertirAIDietaADietaRequestAsync(AIDietaGenerada aiDieta)
        {
            var request = new DietaRequest
            {
                Nombre = aiDieta.Nombre,
                Notas = aiDieta.Descripcion + "\n\n" + string.Join("\n", aiDieta.Consejos),
                Comidas = new List<ComidaRequest>()
            };

            foreach (var comidaAI in aiDieta.Comidas)
            {
                var comidaRequest = new ComidaRequest
                {
                    Nombre = comidaAI.Nombre,
                    Hora = comidaAI.Hora,
                    Alimentos = new List<AlimentoRequest>()
                };

                foreach (var alimentoAI in comidaAI.Alimentos)
                {
                    var alimentoRequest = new AlimentoRequest
                    {
                        Nombre = alimentoAI.Nombre,
                        Cantidad = alimentoAI.Cantidad,
                        Unidad = alimentoAI.Unidad
                    };

                    comidaRequest.Alimentos.Add(alimentoRequest);
                }

                request.Comidas.Add(comidaRequest);
            }

            return request;
        }
    }

    public interface IAIDataService
    {
        Task<AIRequest> PrepararDatosParaIAAsync(int clienteId, string tipoGeneracion, AIRequestConfiguracion configuracion);
        Task<CreateRutinaRequest> ConvertirAIRutinaACreateRutinaRequestAsync(AIRutinaGenerada aiRutina, int clienteId, int usuarioId);
        Task<DietaRequest> ConvertirAIDietaADietaRequestAsync(AIDietaGenerada aiDieta);
    }
}