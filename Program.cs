using webapi;
using webapi.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Amazon.S3;
using Amazon.Runtime;
using System.Text;
using Microsoft.AspNetCore.Cors;
using Amazon.Extensions.NETCore.Setup;
using Stripe;
using Hangfire;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

// Configurar el cliente de Amazon S3 con credenciales específicas desde appsettings.json
var accessKey = builder.Configuration["AWS:AccessKey"];
var secretKey = builder.Configuration["AWS:SecretKey"];
var region = builder.Configuration["AWS:Region"];

var credentials = new BasicAWSCredentials(accessKey, secretKey);
var s3Config = new AmazonS3Config { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region) };
var s3Client = new AmazonS3Client(credentials, s3Config);

// Registra el cliente de Amazon S3 en el contenedor de servicios
builder.Services.AddSingleton<IAmazonS3>(s3Client);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSqlServer<CoachPrimeContext>("Data Source=LAPTOP-NTB0BH1O\\MSSQLSERVERAPP; Initial Catalog=CoachPrimeDB; user id=sa; password=P@ssw0rd; TrustServerCertificate=True");

// Registro de servicios
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IRutinaService, RutinaService>();
builder.Services.AddScoped<IProgresoService, ProgresoService>();
builder.Services.AddScoped<IDietaService, DietaService>();
builder.Services.AddScoped<ISuscripcionService, SuscripcionService>(); // Registro del servicio de suscripciones
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<S3Service>();
builder.Services.AddTransient<EmailService>();

// Añadir autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        x.JsonSerializerOptions.WriteIndented = true; // Para formatear la salida de manera más legible
    });

// Configurar Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .WithOrigins("http://localhost:3000") // Reemplaza esto por la URL de tu frontend
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Configura el logging para usar consola
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


// Configuración de Stripe
StripeConfiguration.ApiKey = builder.Configuration["Stripe: SecretKey"];

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Configurar el Dashboard de Hangfire
app.UseHangfireDashboard();

// Registrar tareas recurrentes
RecurringJob.AddOrUpdate<RecurringJobs>(
    "CheckAndNotifySubscriptions",
    job => job.CheckAndNotifySubscriptions(),
    Cron.Daily // Ejecutar diariamente
);

// Al final de tu configuración
RecurringJob.AddOrUpdate<ISuscripcionService>(
    "ActualizarSuscripcionesVencidas", // Nombre del job
    service => service.MarkSubscriptionsAsExpired(), // Método a ejecutar
    Cron.Daily // Ejecutar diariamente
);

app.UseCors("AllowReactApp");
app.UseHttpsRedirection();

// Habilitar autenticación
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
