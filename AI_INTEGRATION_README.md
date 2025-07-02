# 🤖 Integración de IA para CoachPrime

Esta integración permite a los usuarios generar rutinas y dietas automáticamente usando inteligencia artificial basada en los datos de sus clientes.

## 🚀 Características

- **Generación de Rutinas**: Crea rutinas personalizadas basadas en el nivel, objetivos y progreso del cliente
- **Generación de Dietas**: Diseña planes alimenticios adaptados a las necesidades específicas
- **Análisis de Progreso**: La IA analiza el historial de progreso para hacer recomendaciones
- **Configuración Flexible**: Permite personalizar nivel de dificultad, tipo de dieta, enfoque de entrenamiento, etc.

## 📋 Requisitos

### Backend (.NET)

- .NET 8.0 o superior
- Base de datos SQL Server
- Cuenta de OpenAI con API Key

### Frontend (React)

- Node.js 16+
- React 18+
- TypeScript

## ⚙️ Configuración

### 1. Configurar OpenAI

1. Obtén una API Key de OpenAI en [https://platform.openai.com/api-keys](https://platform.openai.com/api-keys)
2. Agrega la configuración en `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "tu-api-key-aqui",
    "Endpoint": "https://api.openai.com/v1/chat/completions"
  }
}
```

### 2. Registrar Servicios

Los servicios ya están registrados en `Program.cs`:

```csharp
// Servicios de IA
builder.Services.AddHttpClient<IAIService, AIService>();
builder.Services.AddScoped<IAIDataService, AIDataService>();
```

### 3. Configurar Frontend

El componente `AIGenerator.tsx` ya está creado y las traducciones están configuradas.

## 🔧 Uso

### Endpoints Disponibles

#### Generar Rutina y Dieta

```http
POST /api/ai/generar/{clienteId}
Content-Type: application/json

{
  "nivelDificultad": "Intermedio",
  "tipoDieta": "Equilibrada",
  "enfoqueRutina": "Mixto",
  "incluirCardio": true,
  "incluirFlexibilidad": true,
  "caloriasObjetivo": 2000
}
```

#### Generar Solo Rutina

```http
POST /api/ai/generar-rutina/{clienteId}
```

#### Generar Solo Dieta

```http
POST /api/ai/generar-dieta/{clienteId}
```

#### Guardar Rutina Generada

```http
POST /api/ai/guardar-rutina/{clienteId}
```

#### Guardar Dieta Generada

```http
POST /api/ai/guardar-dieta/{clienteId}
```

### Uso en Frontend

```tsx
import AIGenerator from "./components/AIGenerator";

// En tu componente
const [showAIGenerator, setShowAIGenerator] = useState(false);

<AIGenerator
  clienteId={clienteId}
  onRutinaGenerada={(rutina) => {
    // Manejar rutina generada
    console.log("Rutina generada:", rutina);
  }}
  onDietaGenerada={(dieta) => {
    // Manejar dieta generada
    console.log("Dieta generada:", dieta);
  }}
  onClose={() => setShowAIGenerator(false)}
/>;
```

## 📊 Estructura de Datos

### AIRequest

```csharp
public class AIRequest
{
    public int ClienteId { get; set; }
    public string TipoGeneracion { get; set; } // "Rutina", "Dieta", "Ambos"
    public AIRequestData DatosCliente { get; set; }
    public AIRequestConfiguracion Configuracion { get; set; }
}
```

### AIResponse

```csharp
public class AIResponse
{
    public int ClienteId { get; set; }
    public string TipoGeneracion { get; set; }
    public AIRutinaGenerada? Rutina { get; set; }
    public AIDietaGenerada? Dieta { get; set; }
    public AIAnalisis Analisis { get; set; }
    public List<string> Recomendaciones { get; set; }
    public double Confianza { get; set; }
}
```

## 🎯 Flujo de Trabajo

1. **Usuario selecciona cliente** en la aplicación
2. **Hace clic en "Generar con IA"**
3. **Configura parámetros** (nivel, tipo de dieta, etc.)
4. **IA analiza datos** del cliente (progreso, historial, etc.)
5. **IA genera rutina/dieta** personalizada
6. **Usuario revisa** y puede modificar
7. **Usuario guarda** la rutina/dieta en el sistema

## 🔍 Análisis de Datos

La IA analiza automáticamente:

- **Datos básicos**: Edad, peso, altura, sexo
- **Historial de progreso**: Últimos 10 registros de progreso
- **Rutinas anteriores**: Últimas 5 rutinas del cliente
- **Dietas anteriores**: Últimas 5 dietas del cliente
- **Objetivos**: Meta del cliente (perder peso, ganar masa, etc.)
- **Preferencias**: Ejercicios preferidos, alergias, etc.

## 💡 Personalización

### Configuración de IA

Puedes personalizar el comportamiento de la IA modificando el prompt en `AIService.cs`:

```csharp
private string ConstruirPrompt(AIRequest request)
{
    // Personaliza aquí el prompt para la IA
    var sb = new StringBuilder();
    sb.AppendLine("Eres un entrenador personal y nutricionista experto...");
    // ...
}
```

### Agregar Nuevos Campos

Para agregar más datos al análisis:

1. **Extiende el modelo Cliente** con nuevos campos
2. **Actualiza AIRequestData** para incluir los nuevos datos
3. **Modifica AIDataService** para recopilar los nuevos datos
4. **Actualiza el prompt** para usar los nuevos datos

## 🚨 Consideraciones

### Costos

- Cada generación consume tokens de OpenAI
- Monitorea el uso para controlar costos
- Considera implementar límites por usuario/plan

### Privacidad

- Los datos del cliente se envían a OpenAI
- Asegúrate de cumplir con regulaciones de privacidad
- Considera anonimizar datos sensibles

### Rendimiento

- Las llamadas a OpenAI pueden tomar varios segundos
- Implementa timeouts apropiados
- Considera cachear resultados similares

## 🔮 Próximas Mejoras

- [ ] Análisis de fotos de progreso con Computer Vision
- [ ] Chatbot interno para consultas
- [ ] Sistema de recomendaciones inteligentes
- [ ] Predicciones de progreso
- [ ] Marketplace de rutinas generadas por IA
- [ ] Sistema de créditos para generaciones

## 🆘 Soporte

Si tienes problemas:

1. Verifica que la API Key de OpenAI sea válida
2. Revisa los logs del backend para errores
3. Asegúrate de que el cliente tenga datos de progreso
4. Verifica la conectividad a internet

## 📝 Licencia

Esta integración está incluida en el proyecto CoachPrime bajo la misma licencia.
