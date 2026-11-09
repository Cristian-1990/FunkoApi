namespace FunkoAppi01.Service.Email;

/// <summary>
/// Implementación de <see cref="IEmailService"/> que no envía nada de verdad:
/// deja constancia del correo en el log. Sirve para desarrollo y para esta
/// práctica, donde no hace falta un servidor SMTP real para demostrar el flujo.
/// </summary>
public class MemoryEmailService(ILogger<MemoryEmailService> logger) : IEmailService
{
    public Task SendEmailAsync(EmailMessage message)
    {
        logger.LogInformation(
            "=== EMAIL SENT === Para: {To} | Asunto: {Subject} | Html: {IsHtml}",
            message.To, message.Subject, message.IsHtml);
        logger.LogDebug("Cuerpo del email: {Body}", message.Body);
        return Task.CompletedTask;
    }

    public Task EnqueueEmailAsync(EmailMessage message)
    {
        logger.LogInformation(
            "=== EMAIL ENQUEUED === Para: {To} | Asunto: {Subject} | Html: {IsHtml}",
            message.To, message.Subject, message.IsHtml);
        logger.LogDebug("Cuerpo del email: {Body}", message.Body);
        return Task.CompletedTask;
    }
}
