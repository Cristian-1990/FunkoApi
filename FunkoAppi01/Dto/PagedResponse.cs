namespace FunkoAppi01.Dto;

/// <summary>
/// Respuesta paginada genérica: los elementos de la página actual más los metadatos
/// necesarios para pedir la siguiente.
/// </summary>
/// <typeparam name="T">Tipo de los elementos devueltos.</typeparam>
/// <param name="Data">Elementos de la página actual.</param>
/// <param name="Pagination">Metadatos de la paginación.</param>
public record PagedResponse<T>(IEnumerable<T> Data, PageMetadata Pagination);

/// <summary>
/// Metadatos de una respuesta paginada.
/// </summary>
/// <param name="Page">Página actual (empieza en 1).</param>
/// <param name="PageSize">Elementos por página.</param>
/// <param name="TotalPages">Número total de páginas.</param>
/// <param name="TotalItems">Número total de elementos.</param>
public record PageMetadata(int Page, int PageSize, int TotalPages, int TotalItems);
