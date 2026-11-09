using FunkoAppi01.Service.Email;

namespace FunkoAppi01.Infraestructure;

public static class EmailConfig
{
    public static IServiceCollection AddEmail(this IServiceCollection services, IWebHostEnvironment environment)
    {
        // Un servidor SMTP real (MailKit) queda fuera de esta práctica: no aporta
        // nada a lo que se evalúa y añade una dependencia externa innecesaria.
        // MemoryEmailService deja constancia en el log, que es lo que hace falta
        // para demostrar el flujo en ambos entornos.
        services.AddScoped<IEmailService, MemoryEmailService>();
        return services;
    }
}
