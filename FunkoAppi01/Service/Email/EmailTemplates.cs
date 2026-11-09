namespace FunkoAppi01.Service.Email;

/// <summary>
/// Plantillas HTML de los correos de Funko. HTML inline simple, sin motor de plantillas.
/// </summary>
public static class EmailTemplates
{
    public static string CreateBase(string title, string content) => $"""
        <html>
        <body style="font-family: sans-serif; color: #222;">
            <h2>{title}</h2>
            {content}
            <hr>
            <p style="color: #888; font-size: 12px;">FunkoApp — notificación automática.</p>
        </body>
        </html>
        """;

    public static string FunkoCreado(string nombre, decimal precio) =>
        $"<p>Se ha creado el Funko <strong>{nombre}</strong>, precio {precio:C}.</p>";

    public static string FunkoEliminado(string nombre) =>
        $"<p>Se ha eliminado el Funko <strong>{nombre}</strong> del catálogo.</p>";
}
