namespace FunkoAppi01.Service.Email;

/// <summary>
/// Envío de correo. La implementación (en memoria/log, o un servidor SMTP real)
/// se decide en <c>EmailConfig</c> según el entorno.
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(EmailMessage message);
    Task EnqueueEmailAsync(EmailMessage message);
}
