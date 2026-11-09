using System.Reflection;
using System.Text.Json;
using FunkoAppi01.Entity;
using FunkoAppi01.Hubs;
using FunkoAppi01.Infraestructure;
using FunkoAppi01.Middleware;
using FunkoAppi01.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega aquí el token, con el prefijo \"Bearer \" (p. ej. \"Bearer eyJ...\")"
    });

    options.EnableAnnotations();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
{
    // "AllowAll" solo vale para desarrollo. En producción habría que restringir
    // con WithOrigins(...) a los dominios reales del cliente.
    // SetIsOriginAllowed(_ => true) en vez de AllowAnyOrigin(): SignalR necesita
    // AllowCredentials() para la negociación de la conexión, y AllowAnyOrigin()
    // es incompatible con AllowCredentials() (ASP.NET Core lo rechaza en tiempo
    // de ejecución si se combinan).
    options.AddPolicy("AllowAll", policy =>
        policy.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials());
});

builder.Services
    .AddPersistence(builder.Configuration)
    .AddRepositories(builder.Configuration)
    .AddValidators()
    .AddServices()
    .AddAuthenticationJwt(builder.Configuration)
    .AddOutputCacheConfig()
    .AddRateLimitConfig()
    .AddCache(builder.Configuration, builder.Environment)
    .AddEmail(builder.Environment)
    .AddSignalRConfiguration()
    .AddGraphQLConfig(builder.Environment);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Las migraciones versionadas en el repo se generaron con SQLite como proveedor
    // activo; aplicarlas contra PostgreSQL dispara "pending model changes" porque el
    // modelo no coincide exactamente entre proveedores. Mantener dos historiales de
    // migracion en paralelo no compensa en esta fase: PostgreSQL crea el esquema
    // directamente desde el modelo actual con EnsureCreated. InMemory (tests de
    // integracion) tampoco soporta migraciones.
    if (db.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        await db.Database.MigrateAsync();
    else
        await db.Database.EnsureCreatedAsync();

    await UserSeeder.SeedAsync(db);
}

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseRateLimiter();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

// El orden importa: primero quien eres (Authentication), luego que puedes hacer (Authorization).
app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCacheConfig();

app.MapControllers();
app.MapHub<FunkosHub>("/hubs/funkos").RequireAuthorization();
app.MapGraphQLEndpoints();

app.Run();

// Visible para WebApplicationFactory<Program> en los tests de integracion
// (Program.cs usa Top Level Statements, que generan una clase Program implicita).
public partial class Program;