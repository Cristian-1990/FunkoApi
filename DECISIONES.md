# Decisiones de trabajo — API de Funkos

Documento vivo. Una entrada por paso de trabajo, en orden cronológico (la más reciente abajo). Pensado para que Cristian pueda repasarlo y exponerlo en clase: cada entrada dice **qué se hizo, por qué, qué se descartó y qué queda pendiente**.

Normas de este documento (ver `CLAUDE.md` en `Entorno-Servidor/`, puntos 7 y 9):
- Antes de cada comando con efecto (crear rama, modificar fichero, commit), se explica qué hace y se espera confirmación.
- Cada entrada queda cerrada con lo que falta, para no tener que releer todo el historial del chat.
- Nada se sube a GitHub por ahora: todo el trabajo es local hasta que Cristian decida lo contrario.

---

## Entrada 0 — Contexto y punto de partida

**Fecha:** 2026-10-31

**Qué se hizo:**
- Se revisó la pauta oficial de la práctica (`DesarrolloWebEntornosServidor-02-2026-2027/practicas/03-practica-api-funkos.md`, de José Luis García), que define 12 fases: recurso → diseño en papel → Minimal API → controladores → DI → `Result<T>` → DTOs/validadores → config/logs → EF Core → JWT/BCrypt → Swagger/tests → Docker.
- Se clonó temporalmente el repo de un compañero, Jesús Cobo (`https://github.com/JesusCoboArrogante/FunkoApi`), solo para inspección (se borró después), y se auditó contra la pauta y contra nuestras normas de `CLAUDE.md`.
- Se redactó el informe `informe-funko-api.md` (en `Entorno-Servidor/`) con el hallazgo completo. Resumen de lo crítico encontrado en el código de Jesús:
  1. **No compila**: `RepositoryFunko.Update()` usa `funko.Imagen`, propiedad que no existe en el modelo `Funko`.
  2. **El controlador no usa la inyección de dependencias**: crea `new RepositoryFunko()` a mano en un campo `static`, así que el `ServiceFunko` (que tiene las validaciones) nunca se ejecuta.
  3. **Ruta duplicada**: `Update` y `Patch` mapeados los dos a `POST {id}` → error en tiempo de ejecución.
  4. **Falta el `Create`** — el CRUD está incompleto.
  5. **Verbos HTTP equivocados** según la propia tabla de la pauta (debería ser `PUT`/`PATCH`/`GET` según el caso, no todo `POST`).
  6. Falta la propiedad **`Stock`**, exigida por la pauta.
  7. `ExceptionHandlerMiddleware` existe pero está **vacío y sin registrar**.
  8. **Swagger no está configurado** pese a tener el paquete referenciado.
  9. Problemas de entrega: sin tests, sin `.gitignore` (hay `bin/`/`obj/`/`.idea/` comiteados), README roto por encoding, solo 2 commits llamados "first commit".

**Decisión tomada:** partir del código de Jesús en vez de reescribir desde cero.
- **Por qué:** los fallos son localizados y mecánicos (una propiedad, una inyección de dependencias, dos rutas, un middleware vacío) — se arreglan en horas, no en días. El esqueleto de carpetas, el `Dockerfile` multi-etapa y el registro de DI con Scrutor ya están bien planteados y se reaprovechan.
- **Alternativa descartada:** empezar de cero. Se descartó porque tirar el esqueleto que ya funciona (estructura de carpetas, Dockerfile, DI) costaría más tiempo que corregir los bugs puntuales, y además metería más presión sobre la fecha de entrega sin beneficio real.

**Decisión tomada:** clonar en local sin hacer fork ni tocar GitHub.
- **Por qué:** Cristian pidió explícitamente no subir nada a GitHub por ahora y trabajar solo en local. El repo se clonó como `Entorno-Servidor/FunkoApp` (carpeta renombrada respecto al original `FunkoApi`, para ya acercarse al nombre `FunkoApp` que exige el formato de entrega). El único remoto configurado es `origin` → el repo de Jesús, pero no se va a hacer push ahí.
- **Alternativa descartada (de momento):** hacer fork en GitHub inmediatamente. Sigue siendo la recomendación para cuando se decida trabajar en remoto (deja constancia pública de que se parte del trabajo de Jesús), pero se aparca hasta que Cristian lo indique.

**Qué queda pendiente:**
- Arreglar el build (fase 1 de la hoja de ruta: rama `feature/fix-build`).
- Decidir si y cuándo se hace el fork/push a GitHub.
- Seguir documentando cada paso en este documento antes de avanzar al siguiente.

---

## Entrada 1 — `feature/fix-build`: arreglar lo que no compilaba

**Fecha:** 2026-10-31

**Qué se hizo:**
1. Ramas: creada `dev` a partir de `main` (rama de trabajo por defecto, según `CLAUDE.md`), y `feature/fix-build` a partir de `dev`.
2. `Models/Funko.cs`: añadida la propiedad `Stock` (exigida por la pauta y ausente); renombradas `CreateAt`→`CreatedAt`, `UpdateAt`→`UpdatedAt` (coinciden ahora con el nombre exacto de la pauta); sustituida `IsActivo` por `IsDeleted` (invirtiendo la condición en todo el código que la usaba); eliminada `DeleteAt` (no la pide la pauta y quedaba redundante con `IsDeleted`); añadidos valores por defecto a `Nombre`/`Categoria` para evitar warnings de nulabilidad.
3. `Repository/RepositoryFunko.cs`: quitada la asignación a `Imagen` (la propiedad que no existía y rompía la compilación); actualizados todos los usos a los nuevos nombres; `SoftDelete` ahora marca `IsDeleted = true` en vez de rellenar una fecha que nadie leía; `Update` ahora también copia `Stock`.
4. Verificado con `dotnet build`: **compila correctamente** (0 errores). Aparecen 2 warnings `ASP0023` por la colisión de rutas `Update`/`Patch` — es el bug nº3 del informe, ya identificado, pendiente de la fase de rutas.
5. Al compilar se generaron `bin/`/`obj/` y se vio en vivo el problema nº10 del informe (sin `.gitignore`, binarios trackeados). Se resolvió en el mismo bloque: creado `.gitignore` (`bin/`, `obj/`, `.vs/`, `.idea/`, ficheros de usuario) y `git rm --cached` sobre lo que ya estaba versionado.
6. Commits atómicos en `feature/fix-build`: `fix: corregir propiedades del modelo Funko y referencias rotas en el repositorio`, `chore: anadir gitignore y dejar de versionar binarios e idea`, `docs: anadir bitacora de trabajo`. Fusionados en `dev` con `--no-ff` (deja constancia del merge en el historial) y rama `feature/fix-build` borrada tras el merge.

**Decisión tomada:** eliminar `DeleteAt` en vez de mantenerlo junto a `IsDeleted`.
- **Por qué:** la pauta solo exige `IsDeleted`; mantener los dos campos obligaría a sincronizarlos siempre a la vez y es una fuente de bugs (que uno se actualice y el otro no). Un único booleano es suficiente para el borrado lógico que pide la práctica.
- **Alternativa descartada:** conservar `DeleteAt` como fecha de auditoría. Se aparca — si en la fase de EF Core hiciera falta auditoría más fina, se puede reintroducir como campo de solo lectura, no como sustituto de `IsDeleted`.

**Qué queda pendiente:**
- El bug nº2 (controlador sin inyectar el servicio) y nº3 (rutas duplicadas, confirmado ahora también por warning del compilador) — siguiente bloque.
- Bug nº4 (falta `Create`) y nº5 (verbos HTTP) — mismo bloque de CRUD/rutas.
- Middleware de excepciones vacío y Swagger sin configurar — bloques posteriores.
- `.idea/` sigue en el working directory en disco (es normal, lo genera Rider); ya no se versiona.

## Entrada 2 — `feature/controller-rest`: DI real, CRUD completo, verbos correctos

**Fecha:** 2026-10-31

**Qué se hizo:**
1. Rama `feature/controller-rest` desde `dev`.
2. Reescrito `Controller/FunkoController.cs` de cero:
   - Recibe `IServiceFunko` por **primary constructor** (antes: `new RepositoryFunko()` en un campo `static`, que ignoraba por completo la DI y las validaciones del servicio). Ahora toda petición pasa por `ServiceFunko`.
   - Añadido el **`Create`** que faltaba (`[HttpPost]`, devuelve `201 Created` con `CreatedAtAction`).
   - `Update` completo ahora es **`PUT`** (antes `POST`, duplicada con el patch).
   - Actualización de precio ahora es **`PATCH`** (antes `POST`, misma ruta que `Update` → colisión).
   - `Search` ahora es **`GET /api/Funko/search?nombre=...`** (antes `POST`, no tenía sentido como búsqueda de solo lectura).
3. Verificación en tres capas:
   - **Compila**: `dotnet build` → 0 errores, 0 warnings (han desaparecido los `ASP0023` de rutas duplicadas).
   - **Arranca**: `dotnet run` levanta en `http://localhost:5050` sin errores.
   - **Funciona de verdad**: probado con `curl` el ciclo completo — crear (201), listar, consultar por id, actualizar completo (PUT), actualizar precio (PATCH), buscar por nombre, borrar (204, borrado lógico) y comprobar que tras el borrado el GET devuelve 404. Todo correcto.
4. Un commit atómico (`fix: inyectar IServiceFunko en el controlador y completar el CRUD con los verbos correctos`), fusionado en `dev` con `--no-ff`, rama borrada.

**Decisión tomada:** el body de `UpdatePrice` (PATCH) se recibe como `decimal` plano en el body, no como un diccionario genérico (`Dictionary<string, object>`, que era como lo tenía Jesús).
- **Por qué:** el diccionario genérico obligaba a mirar claves por string (`"Precio"`) y castear `JsonElement` a mano — frágil y sin tipado. Al ser un único campo, un `decimal` en el body es más simple y sigue el principio de "si él no usa una técnica más compleja sin necesidad, nosotros tampoco la metemos".
- **Alternativa descartada:** mantener el patch genérico tipo JSON Patch (RFC 6902). Se descarta por ahora porque la pauta no lo pide explícitamente y añadiría complejidad que no toca en esta fase.

**Qué queda pendiente:**
- El `Service.Add`/`Update` siguen lanzando `ArgumentException` en validaciones (nombre vacío, precio negativo) y **no hay middleware que las capture** — de momento esas peticiones devuelven un 500 crudo en vez de un `ProblemDetails` 400. Es el bug nº7 del informe, siguiente bloque.
- Swagger sigue sin configurar (bug nº8).
- DTOs de entrada/salida (fase 7 de la pauta) — por ahora el controlador expone el propio `Funko` de dominio, aceptable en esta fase pero a corregir antes de EF Core.

## Entrada 3 — `feature/middleware-swagger`: manejo de errores y documentación

**Fecha:** 2026-10-31

**Qué se hizo:**
1. Rama `feature/middleware-swagger` desde `dev`.
2. Implementado `Middleware/ExceptionHandlerMiddleware.cs` de verdad (antes solo tenía los `using`, ninguna clase):
   - Captura `ArgumentException` (las validaciones de `ServiceFunko`) → `400 Bad Request` con `ProblemDetails`.
   - Captura cualquier otra excepción → `500 Internal Server Error` con `ProblemDetails` genérico (sin filtrar el mensaje interno al cliente) y log con `ILogger`.
   - Registrado en `Program.cs` con `app.UseMiddleware<ExceptionHandlerMiddleware>()`, antes de `MapControllers()`.
3. Swagger: el paquete que ya traía el proyecto (`Microsoft.AspNetCore.OpenApi`) **no sirve una UI**, solo genera el documento OpenAPI nativo de .NET. Para cumplir el requisito de entrega ("debe arrancar y servir la documentación en `/swagger`") se instaló `Swashbuckle.AspNetCore` (10.3.1) y se añadió `AddSwaggerGen()` + `UseSwagger()` + `UseSwaggerUI()`.
4. Verificación en tres capas:
   - **Compila**: 0 errores (tuvimos que matar antes un proceso `FunkoAppi01.exe` que había quedado corriendo de la verificación del bloque anterior y bloqueaba el `.exe` — nada relacionado con el código, solo higiene del entorno).
   - **Arranca**: `dotnet run` sin errores.
   - **Funciona de verdad**: `GET /swagger/index.html` → 200, `GET /swagger/v1/swagger.json` → 200. `POST /api/Funko` con nombre vacío o precio negativo → ahora devuelve `400` con `{"title":"Solicitud invalida","detail":"..."}` en vez del 500 crudo de antes.
5. Dos commits en la rama (`feat: implementar el middleware...`, `chore: anadir Swashbuckle...`), fusionados en `dev` con `--no-ff`, rama borrada.

**Decisión tomada:** usar `Swashbuckle.AspNetCore` en vez del soporte nativo de OpenAPI que ya traía el proyecto.
- **Por qué:** el paquete nativo (`Microsoft.AspNetCore.OpenApi` + `MapOpenApi()`) solo expone el JSON, no una interfaz visual en `/swagger`; la entrega pide explícitamente esa ruta con documentación servida. Swashbuckle es además la opción estándar que se usa en el resto de ejemplos de la asignatura.
- **Alternativa descartada:** quedarnos con `MapOpenApi()` nativo y montar nosotros una UI estática. Más trabajo para el mismo resultado, sin ninguna ventaja para esta fase.

**Nota de transparencia:** el commit `feat: implementar el middleware...` incluye también las dos líneas de `Program.cs` que registran los servicios de Swagger (`AddEndpointsApiExplorer`/`AddSwaggerGen`), porque tocaban el mismo fichero en la misma edición y separarlas habría significado dos ediciones manuales del mismo archivo sin beneficio real; el `UseSwagger()`/`UseSwaggerUI()` y el paquete en sí quedaron en el commit `chore` aparte. No es una violación grave de "un propósito por commit", pero se deja constancia aquí tal como pide el punto 9 de `CLAUDE.md`.

**Qué queda pendiente:**
- DTOs de entrada/salida y validador de dominio (fase 7 de la pauta) — el controlador sigue aceptando/devolviendo el `Funko` de dominio directamente.
- Las validaciones del servicio siguen siendo `ArgumentException`, no `Result<T, DomainError>` — lo correcto según nuestra arquitectura, pendiente para cuando ataquemos esa fase.
- README real con instrucciones + justificación de decisiones (bug nº11 del informe) — sigue sin hacerse.
- Historial de commits: ya llevamos 8 commits atómicos de fases propias desde que se retomó el proyecto, pero los 2 "first commit" de Jesús siguen en la base del historial (es correcto dejarlos, documentan de dónde partimos).

## Entrada 4 — `feature/dtos-validators`: DTOs, `Result<T, DomainError>`, validador y mapper

**Fecha:** 2026-10-31

**Antes de tocar código:** se revisó `RepositorioRemoto/Errors/DomainError.cs`, `Validators/UserValidator.cs`, `Mappers/UserMapper.cs` y `Services/UserService.cs` para copiar exactamente su patrón (regla 1 de `CLAUDE.md`: mirar el material antes de proponer). Se confirmó: `abstract record DomainError(string Message)` con records anidados + factoría estática fuera; validador que devuelve `Result<T, DomainError>` con estilo if/return (nunca `Bind`/`Map`); mapeadores como métodos de extensión; servicio que llama al validador, comprueba duplicados contra el repositorio y traduce todo a `Result`.

**Qué se hizo:**
1. Rama `feature/dtos-validators` desde `dev`. Paquete `CSharpFunctionalExtensions` 3.7.0 (misma versión que `RepositorioRemoto`).
2. `Errors/DomainError.cs`: `DomainError.NotFound`, `.Validation`, `.AlreadyExists` + factoría `FunkoErrors`.
3. `Dto/CreateFunkoDto.cs`, `UpdateFunkoDto.cs`, `FunkoDto.cs`: con Data Annotations (`[Required]`, `[StringLength]`, `[Range]`), tal como pide la pauta ("Valida con Data Annotations y un validador de dominio").
4. `Validators/IFunkoValidator.cs` + `FunkoValidator.cs`: comprueba lo que las Data Annotations no cubren bien (nombre/categoría solo con espacios).
5. `Mappers/FunkoMapper.cs`: `ToFunko()` (desde los DTOs de entrada) y `ToDto()` (hacia el DTO de salida).
6. `Repository`: añadido `ExistsByName(nombre, excludeId)` — sigue devolviendo un valor plano (`bool`), la regla de negocio de "nombre duplicado" la decide el servicio, no el repositorio.
7. `ServiceFunko`/`IServiceFunko`: reescritos para trabajar con DTOs de entrada/salida y devolver `Result<T, DomainError>` en vez de lanzar excepciones. `Add`/`Update` llaman primero al validador, luego comprueban duplicados, y si todo va bien delegan en el repositorio.
8. `FunkoController`: ya no expone el modelo de dominio, habla en DTOs; un único método privado `ToActionResult` traduce cada tipo de `DomainError` a su código HTTP (`NotFound`→404, `AlreadyExists`→409, `Validation`→400), todo como `ProblemDetails`.
9. `Infraestructure/ValidatorsConfig.cs`: nuevo, registra `IFunkoValidator` en el contenedor DI, igual que ya existía para repositorios y servicios.
10. Verificación en tres capas: compila (0 errores, 0 warnings); arranca sin errores; probado en vivo con `curl` — creación (201), **duplicado de nombre (409)**, nombre en blanco (400), precio negativo (400), no encontrado (404) en `GetById` y `Update`, patch de precio, borrado lógico y búsqueda.
11. Dos commits (`feat: anadir DomainError, DTOs...` y `refactor: usar Result<T, DomainError>...`), fusionados en `dev` con `--no-ff`.

**Hallazgo durante la prueba en vivo:** al mandar un nombre con solo espacios (`"   "`), la respuesta 400 vino del `ModelState` automático de `[ApiController]` (por el `[Required]`), **no** de nuestro `FunkoValidator` — resulta que `RequiredAttribute` de .NET recorta espacios antes de comprobar. Esto significa que, llamando por HTTP, el validador de dominio queda casi siempre "tapado" por las Data Annotations para ese caso concreto.
- **Decisión tomada:** dejar las dos capas de todos modos. El validador de dominio sigue sirviendo de red de seguridad si en el futuro se llama al servicio sin pasar por el `ModelState` (por ejemplo, desde un test unitario que construye el DTO a mano, o desde otro punto de entrada que no sea HTTP). No es redundancia inútil, es defensa en profundidad — y la pauta pide explícitamente las dos técnicas.
- **Alternativa descartada:** quitar la comprobación de blancos del validador de dominio ya que "nunca se ejecuta vía HTTP". Se descarta porque sería optimizar para un único punto de entrada y dejaría el servicio desprotegido si se usa desde otro sitio.

**Decisión tomada (recordatorio de la pregunta de antes del bloque):** servicio y repositorio de Funko se quedan síncronos por ahora.
- **Por qué:** decisión de Cristian — seguir el estilo de José Luis sin salirse del temario; como la pauta no especifica async en esta fase y el repositorio sigue sin E/S real, no hay ninguna instrucción del temario que obligue a ello todavía. Se retomará en la fase 9 (EF Core), cuando aparezca E/S real, igual que hace `RepositorioRemoto`.

**Qué queda pendiente:**
- README real con instrucciones, justificación de decisiones (bug nº11 del informe original).
- Tests unitarios (`NUnit` + `Moq` + `FluentAssertions`, `CasosPositivos`/`CasosNegativos`) — ninguno todavía.
- Configuración en `appsettings` + Serilog (fase 8 de la pauta).
- Persistencia real con EF Core (fase 9) — aquí es donde se decidirá si el repositorio pasa a ser async.
- Seguridad JWT + BCrypt (fase 10).
- El modelo `Funko` sigue siendo mutable (`{ get; set; }`) en vez de un `record` posicional inmutable como `User` en `RepositorioRemoto` — item de estilo nº15 del informe, pendiente de limpieza cuando toquemos el modelo otra vez (probablemente en la fase de EF Core).

## Entrada 5 — `feature/tests`: proyecto de tests `FunkoApp.Test`

**Fecha:** 2026-10-31

**Antes de tocar código:** se revisó `RepositorioRemoto.Tests` (estructura de carpetas, `UserValidatorTests.cs`, `DomainErrorTests.cs`, `UserServiceTests.cs`) para copiar exactamente su estilo: mismas versiones de paquetes (`NUnit` 4.3.2, `Moq` 4.20.72, `FluentAssertions` 6.12.2), `[TestFixture]` + clases anidadas `CasosPositivos`/`CasosNegativos`, comentarios `// Arrange` `// Act` `// Assert`, datos de prueba propios (Pikachu/Charmander/Bulbasaur, no copiados de los ejemplos del profesor).

**Qué se hizo:**
1. Rama `feature/tests` desde `dev`. Proyecto `FunkoApp.Test` creado con `dotnet new nunit`, referencia a `FunkoAppi01`, paquetes `FluentAssertions`, `Moq` y `CSharpFunctionalExtensions` en las mismas versiones que usa `RepositorioRemoto`. Añadido a `FunkoAppi01.sln`.
2. `Errors/DomainErrorTests.cs`: comprueba que cada tipo de `DomainError` construye el mensaje correcto (3 tests).
3. `Validators/FunkoValidatorTests.cs`: `CasosPositivos` (datos válidos) y `CasosNegativos` (nombre/categoría en blanco, varios campos a la vez) (7 tests).
4. `Mappers/FunkoMapperTests.cs`: las conversiones DTO↔Funko copian todos los campos (3 tests).
5. `Repository/RepositoryFunkoTests.cs`: contra el repositorio real en memoria (sin mocks, es lo que se está probando): borrado lógico oculta el Funko de `GetAll`/`GetById`, `ExistsByName` se ignora a sí mismo al excluir su propio Id, `Update`/`SoftDelete` sobre un Id inexistente devuelven `null`/`false` (7 tests).
6. `Services/ServiceFunkoTests.cs`: repositorio y validador **mockeados** con Moq. Aquí es donde se verifica no solo el resultado sino el cortocircuito, tal como pide el punto 4 de `CLAUDE.md`:
   - `Add_ValidacionFalla_NoDeberiaLlamarAlRepositorioAdd` → `_mockRepository.Verify(r => r.Add(...), Times.Never)`: si el validador falla, el repositorio **no debe tocarse**.
   - `Add_NombreDuplicado_NoDeberiaLlamarAlRepositorioAdd` → mismo `Times.Never`, ahora por nombre duplicado.
   - `UpdatePrice_PrecioNegativo_NoDeberiaLlamarAlRepositorio` → `Times.Never` sobre `UpdatePrecio`.
   - Más los casos positivos (`Times.Once` implícito al verificar que sí se llama) y los `NotFound` de `GetById`/`Update`/`Delete` (11 tests).
7. Verificación: `dotnet build` de toda la solución → 0 errores, 0 warnings. **`dotnet test` → 31/31 en verde.**
8. 6 commits atómicos (el proyecto en sí, y un commit por carpeta de tests), fusionados en `dev` con `--no-ff`.

**Decisión tomada:** testear `RepositoryFunko` directamente (sin mocks) en vez de mockear algo dentro de él.
- **Por qué:** es un repositorio en memoria, no hay nada que simular — simularlo sería probar el mock, no el código. Se mockea en `ServiceFunkoTests` porque ahí sí hay una dependencia externa real que aislar.
- **Alternativa descartada:** usar TestContainers ya para el repositorio. Se descarta porque TestContainers es para una base de datos real (regla 4 de `CLAUDE.md`); aquí no hay ninguna todavía — llegará en la fase 9 con EF Core, momento en el que este test se sustituirá por uno de integración contra PostgreSQL/SQLite real.

**Qué queda pendiente:**
- Tests de integración del endpoint con `WebApplicationFactory` (fase 11 de la pauta) — de momento solo hay tests unitarios.
- README real con instrucciones, cómo ejecutar los tests y justificación de decisiones.
- Configuración en `appsettings` + Serilog (fase 8).
- Persistencia EF Core (fase 9) — cuando llegue, `RepositoryFunkoTests` pasará a usar TestContainers con una base de datos real, como exige la norma.

## Entrada 6 — `feature/readme`: README real y `docker compose up` comprobado de verdad

**Fecha:** 2026-10-31

**Antes de escribir nada:** se revisó el `README.md` de `RepositorioRemoto` para copiar su estructura (badges, tabla de endpoints, arquitectura, estructura de carpetas, cómo ejecutarlo, tests, decisiones de diseño, tecnologías, qué queda fuera).

**Pregunta real resuelta con Cristian antes de escribir el README:** la pauta pide implementar el CRUD como Minimal API (fase 3) y comparar con la versión de controladores (fase 4) en el README. Partimos directamente de un proyecto que ya tenía controladores (el de Jesús), así que nunca se hizo la versión Minimal API. Cristian decidió **no hacerla** y declararlo abiertamente en el README como una omisión consciente, para priorizar el tiempo en las fases avanzadas.

**Qué se hizo:**
1. Rama `feature/readme` desde `dev`.
2. **Antes de documentar "cómo ejecutar con Docker" se comprobó de verdad con el Docker real de la máquina** (regla 5 de `CLAUDE.md`: no se aprueba nada por "tiene buena pinta"). Resultado: `docker compose up --build` construía la imagen y el contenedor arrancaba y escuchaba bien en el 8080 **dentro** de la red de Docker (confirmado en los logs), pero **no respondía desde el host** porque `compose.yaml` no tenía ninguna sección `ports`. Es un bug real, no solo un hueco de documentación: la entrega exige explícitamente que la API sea accesible via `docker compose up`.
3. Arreglado `compose.yaml`: añadido `ports: ["8090:8080"]` (8090 porque el 8080 de esta máquina ya estaba ocupado por otro proceso ajeno al proyecto — se comprobó con `Get-NetTCPConnection` antes de asumirlo) y `ASPNETCORE_ENVIRONMENT=Development`.
4. Vuelto a comprobar en vivo: `docker compose up --build` → `GET /swagger/index.html` → 200, `POST /api/Funko` → 201. Confirmado desde fuera del contenedor, no solo desde dentro.
5. Escrito `README.md` real: qué hace, tabla de los 6 endpoints (verbo/ruta/código), arquitectura con la tabla de `DomainError`→HTTP, estructura de carpetas, cómo ejecutar (local y Docker, con el aviso del puerto 8090), cómo correr los tests con el desglose por clase, y una sección de decisiones de diseño que reutiliza y resume las ya documentadas en este documento (Minimal API omitida, validación en dos capas, nombre duplicado en el servicio, borrado lógico con un solo campo, repositorio síncrono, el bug del puerto). Sección final "Qué queda fuera" con las fases 3, 8, 9, 10, 11 (parcial) y 12 (parcial) de la pauta, declaradas sin rodeos.
6. Añadida una sección "De dónde viene este proyecto" con el enlace al repo original de Jesús y un resumen de qué se corrigió, con enlace a la Entrada 0 de este documento.
7. **Incidencia de encoding, resuelta en el momento:** el `Write` inicial del README guardó el fichero en **UTF-16LE** (heredado de algún estado del editor/entorno al escribir sobre el README corrupto original de Jesús, que también estaba en UTF-16). `git commit` lo trató como binario (`0 insertions, 0 deletions` en el resumen). Se detectó revisando el propio commit, se convirtió con `iconv -f UTF-16LE -t UTF-8` y se corrigió con `git commit --amend` (permitido: el commit no se había publicado en ningún remoto). Confirmado después con `git show HEAD:README.md | file -` que el contenido guardado es UTF-8 real.
8. Verificación: `dotnet build` (0 errores) y `dotnet test` (31/31) tras el cambio, por si el `--amend` hubiera afectado a algo (no debía, pero se comprobó).
9. Dos commits (`fix: publicar el puerto...`, `docs: escribir el README real...`), fusionados en `dev` con `--no-ff`.

**Decisión tomada:** declarar la fase 3 (Minimal API) omitida en vez de hacerla a posteriori.
- **Por qué:** decisión de Cristian, por tiempo. Es mejor declararlo con claridad (punto 9 de `CLAUDE.md`: "es mejor declararlo tú que lo descubra [él]") que dejar que parezca un olvido.
- **Alternativa descartada:** añadir la versión Minimal API ahora como bloque extra. Cristian prefirió seguir avanzando con las fases que faltan.

**Qué queda pendiente:** todo lo que el propio README declara en su sección "Qué queda fuera" — fase 8 (config/Serilog), fase 9 (EF Core), fase 10 (JWT/BCrypt), fase 11 parcial (tests de integración con `WebApplicationFactory`) y fase 12 parcial (el `docker-compose` solo tiene la API, sin base de datos todavía porque no existe).

## Entrada 7 — `feature/config-serilog`: fase 8 de la pauta (configuración + logs)

**Fecha:** 2026-10-31

**Antes de tocar código:** se revisó `RepositorioRemoto/Config/ApiConfig.cs` (clase por sección + `SectionName` + `IOptions<T>`) y cómo configura Serilog en `Program.cs` (consola + fichero con rotación diaria). Como ahí es una app de consola con `ConfigurationBuilder` manual y aquí es una Web API, se adaptó al modo idiomático de ASP.NET Core (`builder.Host.UseSerilog(...)` leyendo la sección `Serilog` del `appsettings.json` vía `Serilog.Settings.Configuration`), que es la misma librería y el mismo resultado, solo con el mecanismo de arranque propio de un proyecto Web.

**Qué se hizo:**
1. Rama `feature/config-serilog` desde `dev`. Paquetes `Serilog.AspNetCore`, `Serilog.Sinks.File`, `Serilog.Settings.Configuration`.
2. `appsettings.json`: sección `Serilog` con `MinimumLevel` (Information, overrides a Warning para Microsoft/System) y dos sinks (`Console` y `File` con `rollingInterval: Day`, en `logs/funkoapp-.log`).
3. `appsettings.Development.json`: override del nivel mínimo a `Debug` — perfil por entorno, tal como pide la pauta.
4. `Program.cs`: `builder.Host.UseSerilog(...)` leyendo la configuración, y `app.UseSerilogRequestLogging()` para que cada petición HTTP quede registrada (método, ruta, código, duración) sin tener que escribirlo a mano.
5. `ServiceFunko`: añadido `ILogger<ServiceFunko>` al constructor primario. Log estructurado en las operaciones CRUD: `LogInformation` al crear/actualizar/borrar con éxito, `LogWarning` en los tres casos de fallo de negocio (nombre duplicado, no encontrado al actualizar, no encontrado al borrar).
6. Ajustado `ServiceFunkoTests.cs`: el constructor de `ServiceFunko` cambió (un parámetro más), así que el `SetUp` pasa un `Mock<ILogger<ServiceFunko>>().Object`.
7. `.gitignore`: añadida la carpeta `logs/` (igual que se hizo con `bin`/`obj` en el bloque 1 — no tiene sentido versionar ficheros de log).
8. Verificación en tres capas: `dotnet build` (0 errores) y `dotnet test` (31/31) tras el cambio de constructor; arrancado de verdad con `dotnet run` y comprobado el fichero `logs/funkoapp-AAAAMMDD.log` generado: aparecen tanto las líneas de `UseSerilogRequestLogging` (`HTTP POST /api/Funko responded 201 in 77 ms`) como las propias (`Funko creado: 1 - Jigglypuff`, y los `[WRN]` al crear un duplicado o borrar un id inexistente).
9. Dos commits (`feat: configurar Serilog...`, `feat: anadir log estructurado...`), fusionados en `dev` con `--no-ff`.

**Decisión tomada:** no crear una clase `Config/` + `IOptions<T>` propia para Serilog.
- **Por qué:** Serilog ya tiene su propio mecanismo de lectura de configuración (`Serilog.Settings.Configuration` + `ReadFrom.Configuration`), que es el estándar de la librería y el que de hecho se documenta en su propia web. Forzar una clase `Config/SerilogConfig.cs` solo para envolver eso sería maquinaria de más sin ningún beneficio — y el propio `RepositorioRemoto` tampoco usa `IOptions<T>` para Serilog, lo configura directamente en código.
- **Alternativa descartada:** seguir el patrón de `Config/` + `IOptions<T>` también para Serilog, por pura simetría con el resto del proyecto. Se descarta porque sería forzar un patrón donde la propia herramienta ya da uno mejor.

**Qué queda pendiente:** fase 9 (EF Core — aquí sí aparecerán clases `Config/` propias para la cadena de conexión, con `IOptions<T>`, siguiendo el patrón de `DatabaseConfig` de `RepositorioRemoto`), fase 10 (JWT/BCrypt), fase 11 (tests de integración), fase 12 (docker-compose con base de datos).

## Entrada 8 — `feature/ef-core`: fase 9 de la pauta (persistencia real)

**Fecha:** 2026-10-31

**Antes de tocar código:** se revisó `RepositorioRemoto/Entity/` (`UserEntity`, `AppDbContext` con Fluent API pura, sin atributos en la entidad), `Config/DatabaseConfig.cs` (clase + `SectionName` + `IOptions`) y `Repositories/UserRepository.cs` (repositorio async, `AsNoTracking()` en las lecturas, `FindAsync` para escrituras). Se copió el mismo patrón.

**Decisión de alcance planteada a Cristian antes de empezar y aceptada:** la pauta permite "PostgreSQL (o SQLite en desarrollo)". Para no disparar este bloque, se implementó **solo SQLite** con migraciones reales; PostgreSQL + el `docker-compose` con base de datos quedan para la fase 12.

**Qué se hizo:**
1. Rama `feature/ef-core` desde `dev`. Paquetes: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`, `Npgsql.EntityFrameworkCore.PostgreSQL` (esta última ya instalada para cuando se active el perfil Production, aunque no se usa todavía — el `Provider` en `appsettings.json` decide cuál se registra).
2. `Config/DatabaseConfig.cs`: `Provider` (`Sqlite` por defecto) + `ConnectionString`, igual que `DatabaseConfig` de `RepositorioRemoto`.
3. `Entity/FunkoEntity.cs` y `Entity/CategoriaEntity.cs`: entidades limpias, sin atributos. **Relación Uno a Muchos** `CategoriaEntity 1 → N FunkoEntity`, configurada en `AppDbContext.OnModelCreating` con Fluent API: `HasOne().WithMany().HasForeignKey()`, índice único en `CategoriaEntity.Nombre`, índice único filtrado en `FunkoEntity.Nombre` (solo entre los no borrados) y `HasQueryFilter(f => !f.IsDeleted)` para que cualquier consulta normal oculte los borrados sin tener que repetir el filtro en cada método.
4. `Mappers/FunkoEntityMapper.cs`: aplana `FunkoEntity.Categoria.Nombre` al `string Categoria` del modelo de dominio (el dominio no conoce la relación, solo la tabla — mismo principio que "la tabla es plana" del README de `RepositorioRemoto`, pero a la inversa: aquí la relación vive en la base de datos y se aplana al leer, porque la pauta pide la relación real en la tabla pero los DTOs de Funko solo piden el nombre de la categoría como texto).
5. `Repository/RepositoryFunko.cs`: reescrito entero sobre `AppDbContext`, async de arriba a abajo. `AddAsync`/`UpdateAsync` resuelven o crean la fila de `Categoria` por nombre (`ObtenerOCrearCategoria`) antes de guardar el Funko.
6. `Service`/`Controller`: todas las firmas pasan a `Task`/`async`. Es el cambio que se había dejado pendiente desde el bloque 4 ("por qué el repositorio sigue siendo síncrono") — ahora sí hay E/S real que justifica el `async`.
7. `Infraestructure/PersistenceConfig.cs`: `AddPersistence(configuration)` registra el `AppDbContext` eligiendo `UseSqlite`/`UseNpgsql` según `DatabaseSettings:Provider`. `RepositoriesConfig`: `IRepositoryFunko` pasó de `AddSingleton` a **`AddScoped`** (un `DbContext` no puede vivir dentro de un singleton sin arriesgar fugas de conexión entre peticiones).
8. **Migraciones reales** (no `EnsureCreated`, que es lo que usa `RepositorioRemoto` pero la pauta de Funko pide explícitamente "migraciones"): `dotnet ef migrations add InitialCreate`, aplicada en `Program.cs` con `await db.Database.MigrateAsync()` al arrancar.
9. Verificación en tres capas, con especial cuidado en la tercera porque es persistencia real:
   - **Compila**: 0 errores.
   - **Arranca**: `dotnet run` crea `funkos.db` y aplica la migración sola.
   - **Funciona de verdad**: ciclo completo con `curl` contra SQLite real (crear, duplicado → 409, dos Funkos con la misma categoría comparten la fila de `Categoria`, actualizar, borrar lógico). Y lo más importante para "persistencia real": **se paró el proceso, se volvió a arrancar, y el Funko seguía ahí** — eso es lo que distingue esta fase de las anteriores, donde todo vivía en un `Dictionary` que se vaciaba en cada reinicio.
   - Probado también **dentro de Docker** (`docker compose up --build`): SQLite funciona igual en el contenedor Linux, sin tener que instalar nada adicional.
10. Tests: `RepositoryFunkoTests` ya no mockea nada — usa una base SQLite real en memoria (`DataSource=:memory:` con una conexión abierta durante el test), siguiendo la regla 4 de `CLAUDE.md` ("base de datos real, no simulada"). `ServiceFunkoTests` se adaptó a async (`ReturnsAsync`, `async Task`). **32/32 tests en verde** (31 anteriores + 1 nuevo: que dos Funkos con la misma categoría comparten la fila).
11. Tres commits (`feat: anadir persistencia EF Core...`, `refactor: pasar repositorio, servicio y controlador...`, `test: adaptar los tests...`), fusionados en `dev` con `--no-ff`.

**Decisión tomada:** el dominio (`Funko.Categoria`) sigue siendo un `string`, la relación real solo existe en la base de datos.
- **Por qué:** los DTOs de la pauta (`CreateFunkoDto`, `FunkoDto`...) solo piden `Categoria` como texto, no como un objeto con su propio id. Convertir el dominio a conocer la relación obligaría a tocar DTOs, validador, mapper y tests ya hechos sin que la pauta lo exija. El repositorio resuelve (o crea) la fila de `Categoria` por nombre al guardar, y la aplana al leer — la relación es un detalle de persistencia, no de dominio.
- **Alternativa descartada:** modelar `Categoria` como entidad de dominio propia (con su `Id`) y propagarla por DTOs/validador/mapper. Se descarta por ahora: más trabajo, sin que la pauta lo pida, y con el riesgo de romper fases ya cerradas y verificadas.

**Decisión tomada:** usar migraciones de EF Core en vez de `EnsureCreatedAsync` (que es lo que hace `RepositorioRemoto`).
- **Por qué:** la pauta de Funko pide migraciones explícitamente ("con migraciones"); la de `RepositorioRemoto` no lo pedía con esas palabras. Se sigue la pauta de cada práctica, no se copia el `RepositorioRemoto` a ciegas donde las pautas difieren.

**Qué queda pendiente:**
- PostgreSQL para el perfil Production + `docker-compose` con la base de datos (aparcado a la fase 12, con el paquete Npgsql ya instalado).
- Seguridad JWT + BCrypt (fase 10).
- Tests de integración con `WebApplicationFactory` (fase 11, parcial).
- Dos vulnerabilidades conocidas en paquetes transitivos (`SQLitePCLRaw.lib.e_sqlite3` 2.1.11, `System.Security.Cryptography.Pkcs` 9.0.0), avisadas por `dotnet build`/`dotnet add`. Son dependencias indirectas de EF Core/Npgsql en esta versión de paquetes para .NET 10; no se ha fijado una versión distinta porque podría romper la compatibilidad — queda anotado para revisar si salen versiones corregidas antes de la entrega.

## Entrada 9 — `feature/auth-jwt`: fase 10 de la pauta (seguridad)

**Fecha:** 2026-10-31

**Antes de tocar código:** se buscó en el material de José Luis (regla 1 de `CLAUDE.md`) antes de escribir una sola línea. El tema `16-autenticacion.md` tiene justo un apartado **16.9 "Reto: autenticación con JWT en FunkoApp"**, y el 16.4 ("Enfoque manual, estilo tienda") trae el código completo: `JwtService`, `AuthService`, `AuthController`, `AuthenticationConfig`, modelo `User` con `PasswordHash`/`Role`. Se copió ese patrón.

**Diferencias deliberadas frente al código literal del tema (marcadas como decisión nuestra, no como error):**
- El tema pone `[Required]`/`[MaxLength]` directamente en la clase `User`. Nuestro `CLAUDE.md` exige "la entidad limpia de atributos, solo Fluent API" — regla que ya se aplica a `FunkoEntity`/`CategoriaEntity`. Se mantuvo esa misma regla para `UserEntity`: nada de Data Annotations en la entidad, todo en `AppDbContext.OnModelCreating`.
- El tema usa `try/catch` con `InvalidOperationException`/`UnauthorizedAccessException` en `AuthService` y las atrapa en el controlador. Como el resto de `FunkoApp` usa `Result<T, DomainError>` (fase 6), se tradujo el mismo enfoque manual a nuestro patrón: `AuthService` devuelve `Result`, se añadieron dos tipos nuevos a `DomainError` (`Unauthorized`, `Conflict`) y una factoría `AuthErrors` paralela a `FunkoErrors`.
- El propio tema avisa (nota al margen de 16.4.3): "en proyectos reales es preferible `IOptions<JwtOptions>`" en vez de leer `configuration["Jwt:Secret"]` suelto. Se siguió esa propia recomendación: `Config/JwtConfig.cs` con `IOptions<JwtConfig>`, igual que `DatabaseConfig`.
- No se implementó el **enfoque Identity** (7+ tablas) ni **OAuth2** ni **refresh tokens** ni **rate limiting**: eso pertenece a los "puntos extra" del Reto (16.9) y al punto 16.6/16.5 del tema, no a la fase 10 de la pauta real de la práctica (`03-practica-api-funkos.md`), que solo pide "JWT y BCrypt... protege las operaciones de escritura: solo usuarios autenticados; el borrado, solo rol Admin". Se implementó exactamente eso, sin ampliar el alcance.

**Qué se hizo:**
1. Rama `feature/auth-jwt` desde `dev`. Paquetes `Microsoft.AspNetCore.Authentication.JwtBearer` y `BCrypt.Net-Next`.
2. `Entity/UserEntity.cs` (limpia) + `UserRoles` (`ADMIN`/`USER`), añadida a `AppDbContext` con Fluent API e índice único en `Email`. Migración `AddUsers`.
3. `Config/JwtConfig.cs` (`Secret`, `Issuer`, `Audience`, `ExpirationMinutes`, vía `IOptions`).
4. `DomainError.Unauthorized`/`DomainError.Conflict` + factoría `AuthErrors`.
5. `Dto/LoginDto.cs`, `SignupDto.cs` (con Data Annotations: email válido, contraseña mínimo 8 caracteres) y `AuthResponseDto.cs`.
6. `Repository/IRepositoryUser.cs`/`RepositoryUser.cs`: `FindByEmailAsync`, `CreateAsync`, `UpdateAsync` — valores planos, igual que `RepositoryFunko`.
7. `Service/JwtService.cs`: genera el JWT (claims `sub`, `email`, `username`, `role`, `jti`) firmado con HMAC-SHA256. **Sin** `ValidateToken` manual: la validación la hace el middleware `JwtBearer` (más abajo), no hace falta duplicarla.
8. `Service/AuthService.cs`: `SignupAsync` (hashea con BCrypt, `workFactor: 11`, comprueba email duplicado) y `LoginAsync` (verifica con `BCrypt.Verify`, actualiza `LastLoginAt`). Todo en `Result<AuthResponseDto, DomainError>`.
9. `Controller/AuthController.cs`: `POST /api/auth/signup` (201), `POST /api/auth/login` (200/401), `GET /api/auth/me` (`[Authorize]`, lee los claims del token).
10. `Infraestructure/AuthenticationConfig.cs`: registra el esquema `JwtBearer` con `TokenValidationParameters` (emisor, audiencia, firma, expiración, `ClockSkew = Zero`) y `RoleClaimType = "role"` (para que `[Authorize(Roles = ...)]` lea el claim correcto).
11. `FunkoController`: `[Authorize]` a nivel de clase + `[AllowAnonymous]` en `GetAll`/`GetById`/`Search` (lectura pública) + `[Authorize(Roles = UserRoles.Admin)]` en `Delete` (solo Admin).
12. `Program.cs`: `AddAuthenticationJwt`, Swagger con botón "Authorize" (`AddSecurityDefinition`), `UseAuthentication()` antes de `UseAuthorization()` (el orden es crítico), y `UserSeeder.SeedAsync` tras la migración.
13. `Infraestructure/UserSeeder.cs`: crea `admin@funko.com`/`admin123` (rol ADMIN) y `user@funko.com`/`user123` (rol USER) si la tabla está vacía — sin esto no habría manera de probar el rol Admin, porque el registro público siempre asigna `USER`.
14. **Incidencia real de versión, resuelta en el momento:** `Microsoft.OpenApi` 2.x (la versión que trae `Swashbuckle.AspNetCore` 10.3.1) movió sus tipos de `Microsoft.OpenApi.Models` a `Microsoft.OpenApi` a secas, y cambió cómo se referencian los esquemas de seguridad (ya no hay `OpenApiReference`/`.Reference` tal cual los documenta la teoría, pensada para una versión anterior). Se ajustó el `using` y se simplificó a solo `AddSecurityDefinition` (el botón "Authorize" de Swagger sigue apareciendo; se dejó fuera el candado por endpoint, que no es necesario para que funcione).
15. Verificación en tres capas, exhaustiva porque es la fase más sensible a fallos silenciosos (un 200 donde debería haber un 401 no se nota a simple vista):
    - **Compila**: 0 errores.
    - **Arranca**: migración `AddUsers` aplicada, *seed* de los dos usuarios.
    - **Funciona de verdad**, con `curl` real: lectura sin token → 200 (pública); crear sin token → 401; login admin → token válido; login con contraseña incorrecta → 401; `GET /me` con token → claims correctos; crear con token de usuario normal → 201 (escribir solo exige estar autenticado); **borrar con token de usuario normal → 403** (no es Admin); borrar con token de Admin → 204; *signup* nuevo → 201; *signup* con email duplicado → 409; contraseña corta en *signup* → 400; token falsificado → 401.
16. Tests nuevos: `JwtServiceTests` (los claims generados son correctos) y `AuthServiceTests` (alta con éxito, email duplicado, login correcto, contraseña incorrecta, email inexistente, usuario borrado) — con Moq sobre `IRepositoryUser`, sin BCrypt real de por medio salvo para fabricar el hash de prueba. **40/40 tests en verde** (32 anteriores + 8 nuevos).
17. Cuatro commits (`feat: anadir entidad User...`, `feat: anadir JwtService...`, `feat: proteger FunkoController...`, `test: anadir tests...`), fusionados en `dev` con `--no-ff`.

**Decisión tomada:** el secreto JWT vive en `appsettings.json`, en claro.
- **Por qué:** es una práctica académica con SQLite local, no un despliegue real. Documentarlo aquí para que quede claro que en un proyecto real este valor iría a `dotnet user-secrets`, una variable de entorno o un gestor de secretos — nunca comiteado.
- **Pendiente si se quisiera pulir:** moverlo a una variable de entorno antes de subir el repo a ningún sitio público, sobre todo si en algún momento se hace el fork a GitHub.

**Qué queda pendiente:**
- Tests de integración con `WebApplicationFactory` (fase 11, parcial) — serían el sitio natural para probar "token expirado" de verdad (con un `JwtConfig` de expiración casi inmediata) y "acceso sin token" de extremo a extremo, que con tests unitarios de servicio no se pueden cubrir del todo.
- PostgreSQL + `docker-compose` con base de datos (fase 12, aparcado desde la fase 9).
- README: falta añadir la sección de autenticación (endpoints, cómo probar con Swagger, credenciales de los usuarios semilla).

## Entrada 10 — `feature/integration-tests`: fase 11 de la pauta (tests de integración)

**Fecha:** 2026-10-31

**Antes de tocar código:** se buscó en el material y apareció justo lo necesario: `28-testing.md`, apartado **28.10 "Tests de controladores con WebApplicationFactory"**, con el patrón completo (paquetes, `WebApplicationFactory<Program>` personalizada que sustituye el `DbContext`, tests con `HttpClient` real) y el 28.10.1 sobre comprobar la **forma** de los errores (`ProblemDetails` con `errors` por campo en 400, `message`/`detail` en 404/409), no solo el código de estado.

**Qué se hizo:**
1. Rama `feature/integration-tests` desde `dev`. Paquetes `Microsoft.AspNetCore.Mvc.Testing` (proyecto de tests) y `Microsoft.EntityFrameworkCore.InMemory` (proyecto de tests primero, luego también en `FunkoAppi01` — ver más abajo).
2. `Program.cs`: añadido `public partial class Program;` al final — imprescindible porque usa Top Level Statements y `WebApplicationFactory<Program>` necesita una clase `Program` visible desde el proyecto de tests (aviso explícito del tema 28.10).
3. `FunkoControllerIntegrationTests` y `AuthControllerIntegrationTests`: `HttpClient` real contra un servidor en memoria, sin mocks en ningún punto de la pila. Casos cubiertos: lectura pública, crear con token de usuario normal (201), borrar con token de usuario normal (403) vs. con token de Admin (204), crear sin token (401), cuerpo inválido → 400 con `ProblemDetails.errors` por campo, 404 con `detail` legible, login correcto/incorrecto, *signup*/duplicado, `/me` con y sin token.

**El verdadero trabajo de este bloque — tres incidencias reales de infraestructura, resueltas en orden (y es justo lo que pide documentar la fase 5 de `CLAUDE.md`):**

1. **"Varios proveedores de base de datos registrados."** El primer intento fue el de calco literal del tema: en `ConfigureWebHost`, quitar el descriptor `DbContextOptions<AppDbContext>` y volver a registrar el `AppDbContext` con `UseInMemoryDatabase`. Falló: `AddDbContext` deja más registros que ese único descriptor (los servicios internos del proveedor SQLite seguían ahí), y EF Core no admite dos proveedores a la vez en el mismo contenedor de DI.
   - *Primer intento de arreglo:* un `IServiceProvider` interno propio para el proveedor InMemory (`UseInternalServiceProvider`, patrón oficial de Microsoft para este mismo problema). Quitó el primer error, pero sacó uno nuevo: `MigrateAsync` fallaba porque el contexto parecía "relacional" (por restos de la configuración de SQLite) pero el proveedor activo era InMemory, que no implementa `Migrate`.
   - *Solución definitiva, más simple que las dos anteriores:* en vez de tocar los servicios de DI ya registrados, se le dio a `PersistenceConfig.AddPersistence` una tercera rama (`Provider == "InMemory"` → `UseInMemoryDatabase`), igual que ya decide entre SQLite y PostgreSQL. Los tests solo tienen que decirle a la configuración qué proveedor quieren, no desmontar nada.
2. **El cambio de configuración no llegaba a tiempo.** Con la solución anterior, `ConfigureWebHost` cambiaba la configuración con `builder.ConfigureAppConfiguration(...)` — y los tests seguían fallando, pero de otra forma: `Create`/`Signup` con datos nuevos devolvían `409` como si el dato ya existiera. Investigando apareció un `funkos.db` **real** en la carpeta de salida de los tests: con Top Level Statements, `builder.Configuration` de `Program.cs` ya está resuelto en el momento en que se ejecuta `AddPersistence`, antes de que `ConfigureAppConfiguration` tuviera ocasión de añadir nada — así que todos los tests estaban escribiendo, sin darse cuenta, en una base SQLite real y compartida entre ejecuciones.
   - *Arreglo:* `builder.UseSetting(...)` en vez de `ConfigureAppConfiguration`. Es el mecanismo que `WebApplicationFactory` documenta específicamente para este caso, y sí llega a tiempo.
3. Verificación de que el arreglo era de verdad y no casualidad: se borró el `funkos.db` que había quedado de las ejecuciones fallidas, se comprobó que no volvía a aparecer, y se corrió la suite **tres veces seguidas** — 53/53 las tres veces — antes de dar el bloque por cerrado. Y se comprobó además que la aplicación real (con SQLite, `dotnet run`) seguía funcionando exactamente igual que antes, para asegurar que el nuevo proveedor "InMemory" no había roto nada fuera de los tests.
4. Quitada la dependencia `Microsoft.EntityFrameworkCore.InMemory` del proyecto de tests una vez dejó de usarse directamente ahí (ahora vive en `FunkoAppi01`, y le llega al proyecto de tests por la referencia).
5. Dos commits (`feat: anadir soporte de proveedor InMemory...`, `test: anadir tests de integracion...`), fusionados en `dev` con `--no-ff`.

**Decisión tomada:** el proveedor "InMemory" se decide por configuración, no manipulando los servicios ya registrados en el contenedor de DI.
- **Por qué:** es muchísimo más simple y no depende de conocer los detalles internos de cómo `AddDbContext` registra sus servicios (que además pueden cambiar entre versiones de EF Core). La propia aplicación ya sabía elegir el proveedor por configuración (SQLite/PostgreSQL); darle una tercera opción reutiliza ese mismo mecanismo en vez de inventar uno nuevo solo para tests.
- **Alternativa descartada:** `UseInternalServiceProvider` con un `IServiceProvider` propio para el proveedor de test. Es el patrón "oficial" de Microsoft para este problema, pero aun resolviendo el primer error dejó uno nuevo (la detección de "relacional" seguía confundida) — más frágil que simplemente decirle a la configuración qué proveedor usar.

**Resultado:** **53/53 tests en verde** (40 unitarios anteriores + 13 de integración nuevos), verificados en tres ejecuciones consecutivas sin fallos ni ficheros residuales.

**Qué queda pendiente:**
- PostgreSQL + `docker-compose` con base de datos (fase 12, aparcado desde la fase 9).
- Tests de "token expirado" de verdad (necesitarían un `JwtConfig` de expiración casi inmediata en un test dedicado) — no cubierto todavía, es un caso de borde razonable para añadir si hay tiempo antes de la entrega.
- README: añadir la sección de tests de integración.

## Entrada 11 — `feature/postgres`: fase 12 de la pauta (PostgreSQL + Docker)

**Fecha:** 2026-10-31

**Antes de tocar código:** se revisó `RepositorioRemoto/docker-compose.yml` (servicio Postgres con `healthcheck` + `depends_on: condition: service_healthy`, variables de entorno con `__` para sobrescribir `appsettings`, perfil Production) y su `appsettings.Production.json`. Se copió esa misma estructura.

**Qué se hizo, y sobre todo qué falló al comprobarlo de verdad (regla 5 de `CLAUDE.md`: nada se aprueba sin ejecutarlo) — esta fase fue la que más "verificación en tres capas" necesitó de todo el proyecto:**

1. `appsettings.Production.json` (`DatabaseSettings.Provider = PostgreSQL`) y `compose.yaml` con un servicio `postgres` (imagen `postgres:16-alpine`, `healthcheck` con `pg_isready`, volumen persistente) + `funkoappi01` esperando a `service_healthy` y con `ASPNETCORE_ENVIRONMENT=Production`.
2. **Primer fallo real:** `docker compose up` no arrancaba — puerto `5432` ya ocupado por otro Postgres de la propia máquina de desarrollo (nada que ver con el proyecto). Se remapeó a `5433:5432`.
3. **Segundo fallo real:** con Postgres arriba y sano, la app moría al aplicar las migraciones: `PendingModelChangesWarning`. Las migraciones de este repo se generaron con SQLite como proveedor activo; EF Core detecta que el modelo no coincide exactamente al validarlas contra PostgreSQL (los dos motores difieren en cómo mapean tipos por defecto). Mantener dos historiales de migración en paralelo (uno por proveedor) es la solución "correcta" pero no compensa el esfuerzo en esta fase — se decidió que PostgreSQL cree el esquema con `EnsureCreatedAsync` (desde el modelo actual, sin pasar por el historial de migraciones), y que SQLite siga con `MigrateAsync` de verdad (que es lo que pedía explícitamente la fase 9). `Program.cs` decide por `db.Database.ProviderName`.
4. **Tercer fallo real**, ya con `EnsureCreated` funcionando para crear las tablas: el índice único filtrado de `FunkoEntity.Nombre` usaba `"is_deleted" = 0` (sintaxis de SQLite, donde el booleano se guarda como entero); PostgreSQL con su tipo `boolean` real no admite comparar `boolean = integer` y la creación del esquema fallaba. Cambiado a `"is_deleted" = FALSE` (válido en los dos motores) y regenerada la migración de SQLite (`FixDeletedFilterPortable`) para que su historial siga coincidiendo con el modelo.
5. **Cuarto fallo real**, ya con el esquema creado: el *seed* de usuarios fallaba al guardar — `Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone', only UTC is supported`. Todo el código usaba `DateTime.Now` (hora local); Npgsql exige UTC para ese tipo de columna. Cambiado a `DateTime.UtcNow` en `RepositoryFunko`, `AuthService` y `UserSeeder` — además de arreglar Postgres, es la práctica correcta en cualquier proveedor (guardar en UTC, convertir a local solo al mostrar).
6. Con los cuatro fallos resueltos, verificación de verdad completa contra PostgreSQL real (no solo "arranca"): login, crear (201), nombre duplicado (409, confirma que la categoría y el índice único funcionan de verdad en Postgres), listar, borrar (204). Y, para no romper lo que ya funcionaba, se repitió la misma prueba contra **SQLite** (perfil normal, `dotnet run`) después de todos estos cambios: sigue funcionando igual.
7. Tres commits (`fix: usar DateTime.UtcNow...`, `fix: hacer portable...`, `feat: activar PostgreSQL...`), fusionados en `dev` con `--no-ff`.

**Decisión tomada:** PostgreSQL usa `EnsureCreated`, no el historial de migraciones de SQLite.
- **Por qué:** los dos proveedores no comparten exactamente el mismo modelo generado (tipos, longitudes, defaults), y EF Core lo detecta y lo bloquea como medida de seguridad (`PendingModelChangesWarning`). Mantener una segunda carpeta de migraciones solo para Postgres es la solución "de libro", pero añade mantenimiento doble sin ningún beneficio real a estas alturas del curso.
- **Alternativa descartada:** una segunda carpeta de migraciones específica para PostgreSQL (`MigrationsAssembly` separado). Queda anotada como posible mejora si en algún momento se necesitara evolucionar el esquema en producción de forma controlada; para esta práctica no compensa.

**Resultado:** 53/53 tests siguen en verde (no afectados, usan SQLite/InMemory), y **PostgreSQL funciona de verdad** dentro de Docker: esquema, relación con categoría, índice único, JWT, todo probado en vivo.

**Qué queda pendiente:**
- README: añadir cómo levantar el perfil Production con `docker compose up`.
- Nada más de la pauta original queda sin abordar salvo la fase 3 (Minimal API, omitida a propósito desde el bloque del README) y el caso de borde de "token expirado" de verdad (fase 11, opcional).

## Entrada 12. Ritmo del temario: qué se adelantó y qué se queda pendiente de revisar

**Qué se hizo:** Cristian avisa de que la clase va por el tema 18, mientras que la fase 11 (temas 24 y 28) y la fase 12 (tema 29) de la pauta corresponden a temario que todavía no se ha visto. Se revisa qué partes de lo ya construido dependen de eso.

**Decisión tomada:**
- Los tests (NUnit + Moq + FluentAssertions, unitarios y de integración) **no son el problema**: siguen el mismo patrón que `RepositorioRemoto`, que Cristian ya domina. Se mantienen tal cual.
- Swagger/OpenAPI con XMLDoc (parte de la fase 11) y todo el despliegue con Docker (fase 12, tema 29) sí son temario no visto todavía. El código ya existe, compila, pasa los tests y se verificó en vivo — no se revierte, porque sería tirar trabajo verificado — pero se marca aquí como **adelanto pendiente de repasar con Cristian antes de exponerlo**, para que no llegue a la exposición sin entender alguna de las dos partes.
- Repasando `RepositorioRemoto` por este motivo, se encuentra que sus tests de repositorio (`UserRepositoryTests.cs`) usan `Testcontainers.PostgreSql` (contenedor real de Postgres durante el test), tal como pide la regla 4 del `CLAUDE.md` ("TestContainers para la capa de datos, base de datos real, no simulada"). Nuestros tests de datos usan SQLite en memoria y el proveedor InMemory de EF Core, no Testcontainers — una desviación de la referencia que nunca se había marcado como decisión nuestra (incumplía el punto 1 de la regla de oro).
- **Esa desviación se mantiene y ahora queda justificada:** Testcontainers depende de Docker, y Docker es justo la fase 12 (tema 29) que todavía no se ha visto. Adoptar Testcontainers ahora introduciría una dependencia de una técnica de temario futuro antes de tiempo. Se retoma cuando la clase llegue a Docker.

**Alternativas descartadas:**
- Revertir las fases 11 (parcial) y 12 del entregable para que cuadre exactamente con el tema 18. Descartada por Cristian: prefiere mantener el trabajo ya verificado y repasarlo con calma antes de exponerlo, no borrarlo.
- Migrar ya los tests de datos a Testcontainers para igualar la referencia. Descartada por la misma razón: necesitaría Docker, que es justo lo que todavía no toca.

**Qué queda pendiente:**
- Repasar con Cristian, antes de la exposición, la parte de Swagger/XMLDoc y todo el bloque de Docker, para que pueda explicarlo con la misma soltura que el resto.
- Cuando la clase llegue al tema 29 (Docker), valorar migrar los tests de datos de SQLite/InMemory a Testcontainers.PostgreSql, para igualar de verdad la técnica de `RepositorioRemoto`.
- Sigue abierta la decisión sobre renombrar `Infraestructure/`→`Infrastructure/`, `Repository/`→`Repositories/` y `Service/`→`Services/` para igualar los nombres de carpeta de `RepositorioRemoto` (detectado en la revisión de esta misma sesión, Cristian no ha confirmado todavía si se hace).

## Entrada 13. CRUD de Categoria y Proveedor (entrega y exposición mañana)

**Qué se hizo:**
1. Sincronización con el repo de Jesús: su `main` ya tenía fusionado nuestro PR entero. Añadió por su cuenta dos clases sueltas sin usar (`Models/Categoria.cs`, `Models/Proveedor.cs`) y tiene una rama propia sin fusionar (`feature-repository`) montando un CRUD de Categoria. No se fusiona nada de eso: no aporta nada que no tengamos ya y mete riesgo a horas de la entrega.
2. Cristian trae aviso de Jesús (confirmado con foto de la pizarra de clase, `Pizarra-Funko.jpg`) de que Categoria y Proveedor necesitan su propio CRUD, no solo existir como relación del Funko. La pizarra muestra más cosas (Redis, MongoDB políglota, subida de imágenes, Bruno automatizado) que exceden claramente el documento oficial de la Práctica 3 — se decide con Cristian limitar el alcance de esta sesión a: CRUD de Categoria y Proveedor en SQL/EF Core, con el mismo patrón que Funko.
3. Añadida `ProveedorEntity` (Fluent API, tabla `proveedores`, borrado lógico) y relación opcional `Funko → Proveedor` (`ProveedorId` nullable, `OnDelete(Restrict)`). Opcional porque exponerla como obligatoria habría exigido tocar `CreateFunkoDto`/`UpdateFunkoDto` y los 53 tests existentes de Funko, un riesgo innecesario a horas de la entrega.
4. `CategoriaEntity` ampliada con `IsDeleted`/`CreatedAt`/`UpdatedAt` (antes solo tenía Id y Nombre): sin eso no se puede dar de baja una categoría igual que un Funko.
5. CRUD completo de Categoria y Proveedor replicando exactamente la arquitectura de Funko: Entity (Fluent API) → Model → Dto → Mapper → Repository (valores planos) → Service (`Result<T, DomainError>`) → Validator → Controller.
6. Lectura pública, escritura (`Crear`/`Actualizar`/`Borrar`) restringida a rol `ADMIN` — tal como muestra la pizarra para Categoria y Proveedor. **No se ha tocado la autorización de `FunkoController`** (que deja Crear/Actualizar a cualquier usuario autenticado, no solo Admin): cambiarla ahora habría sido otro riesgo no pedido explícitamente para Funko.
7. `DomainError.NotFound`/`AlreadyExists` generalizados con un parámetro `Entidad` (por defecto `"Funko"`, no rompe nada existente) para reutilizar un único tipo de error en las tres entidades, igual que había planteado Jesús en su rama (sin fusionarla, pero la idea era buena).
8. Migración `AddCategoriaProveedorCrud` generada y aplicada en limpio contra SQLite: build, 82/82 tests (53 de antes + 29 nuevos) y pruebas en vivo con `curl` — login, crear/duplicar/actualizar/borrar Categoria y Proveedor, lectura anónima en 200 y escritura sin token en 401.
9. Encontrado y corregido en caliente un fallo de concordancia de género en los mensajes de error ("Ya existe un Categoria" en vez de "una Categoria"): se reescribió a una frase neutra, "Ya existe un registro de {Entidad} con el nombre...", que funciona para Funko, Categoria y Proveedor sin más parches.

**Decisión tomada:** CRUD completo de Categoria y Proveedor en SQL/EF Core, mismo patrón que Funko, relación Proveedor→Funko opcional, escritura solo Admin.

**Alternativas descartadas:**
- Fusionar la rama `feature-repository` de Jesús. Descartada: no está en su `main`, es trabajo suyo independiente, y a horas de la entrega no compensa el riesgo de integrarlo.
- Hacer también lo de Redis, MongoDB, imágenes y Bruno de la pizarra. Descartado con Cristian: no está en el documento oficial de la práctica y es inviable en el tiempo que queda.
- Relación `Proveedor` obligatoria en Funko (como Categoria). Descartada por el riesgo de romper los 53 tests existentes de Funko a horas de la entrega; queda como mejora futura.
- Igualar la autorización de Categoria/Proveedor (todo Admin) en `FunkoController` también. Descartada: no se pidió, y tocar algo que ya funciona y está evaluado no compensa el riesgo.

**Qué queda pendiente:**
- Exponer `Proveedor` en los DTOs de Funko (ahora mismo la relación existe en la base de datos pero no se puede asignar todavía desde la API).
- Tests de integración (`WebApplicationFactory`) de `CategoriaController`/`ProveedorController` — por ahora solo hay tests de servicio y validador (Moq), igual que se hizo para Funko en su momento, pero no ha dado tiempo a los de integración con la entrega mañana.
- La búsqueda por nombre (`GetByNameAsync`) distingue mayúsculas de minúsculas en SQLite (heredado del mismo patrón que ya tenía `RepositoryFunko`): buscar "ani" no encuentra "Anime". No es una regresión nueva, es una limitación ya existente; se deja anotada para no repetir la pregunta.
- Sigue sin confirmar el renombrado de carpetas (`Infraestructure/`→`Infrastructure/`, etc.) de la Entrada 12.

## Entrada 14. Cierre: no se renombran las carpetas

**Qué se hizo:** Cristian compara a mano la estructura de carpetas contra el árbol de ejemplo de la pauta oficial. Se confirma que difieren en nombres (`Controller` vs `Controllers`, `Repository` vs `Repositories`, `Service` vs `Services`, `Infraestructure` vs `Infrastructure`), heredado del proyecto de Jesús.

**Decisión tomada:** no se renombra nada. Lo que importa es la arquitectura (capas, separación de responsabilidades, `Result<T, DomainError>`, Fluent API, DTOs) y esa sí coincide con la pauta y con `RepositorioRemoto`. Los nombres de carpeta son cosméticos y Cristian decide que no vale la pena el riesgo de tocar namespaces en todo el proyecto a horas de la entrega.

**Alternativas descartadas:** renombrar `Infraestructure/`→`Infrastructure/`, `Repository/`→`Repositories/`, `Service/`→`Services/`, `Controller/`→`Controllers/` (quedó propuesto en la Entrada 12). Descartado explícitamente por Cristian.

**Qué queda pendiente:** nada de este punto — queda cerrado. Si en algún momento se quiere hacer, es un cambio mecánico (renombrar carpeta + namespace + imports) sin impacto en el comportamiento.

## Entrada 15. Ampliación de la entrega: extras pedidos por el profesor (plazo: jueves 12 nov, 20:30)

**Contexto:** al ampliar el plazo de entrega, el profesor amplió también el alcance (foto de los apuntes de clase de Cristian): documentación Swagger por endpoint, optimización (paginación, `AsNoTracking`, OutputCache), persistencia políglota (Postgres/Mongo/Cassandra), caché con Redis, WebSocket, GraphQL, Mail, Identity, seguridad (CORS, rate limiting, auditoría), todo con Docker, y tests con Bruno. Identity queda aparcado (ambiguo, pendiente de confirmar con Jesús/profesor qué significa exactamente). El resto, confirmado como obligatorio por Cristian.

**Qué se hizo (Bloque 1 — Optimización y seguridad):** antes de tocar nada se investigó en paralelo (6 sub-tareas) el código real de José Luis para cada bloque, para no inventar ningún patrón — regla de oro del proyecto. Para este bloque:
1. **Paginación:** `PagedResponse<T>`/`PageMetadata` (DTOs compartidos), `GetPagedAsync` en los tres repositorios (`Skip`/`Take`/`CountAsync`, `AsNoTracking`) y los tres servicios (clamps `page≥1`, `pageSize∈[1,100]`, igual que hace José Luis), endpoint `GET /api/{recurso}/paged?page=&pageSize=`.
2. **OutputCache:** middleware nativo de ASP.NET Core (`AddOutputCache`/`UseOutputCache`), `[OutputCache(Duration=60, Tags=[...])]` en los `GET` públicos, invalidado con `IOutputCacheStore.EvictByTagAsync` tras cada escritura en los tres servicios (inyectado por constructor).
3. **CORS:** política `AllowAll` en `Program.cs` (documentado como solo válida en desarrollo, siguiendo el aviso explícito del material de José Luis).
4. **Rate limiting:** API nativa `System.Threading.RateLimiting` (nunca el paquete de terceros, también por indicación explícita del material), `FixedWindowRateLimiter` de 100 peticiones/minuto por IP, 429 + `Retry-After` al superarlo.
5. **Auditoría de accesos:** `JwtBearerEvents.OnAuthenticationFailed` registra con `LogWarning` cada intento de acceso con token ausente/caducado/manipulado.
6. **Swagger + XMLDoc:** `GenerateDocumentationFile` en el csproj, `Swashbuckle.AspNetCore.Annotations`, XMLDoc (`summary`/`param`/`response`) y `[SwaggerOperation]`/`[SwaggerResponse]` en los cuatro controladores (Funko, Categoria, Proveedor, Auth).

**Verificado en vivo** (no solo "compila"): 82/82 tests en verde; con `curl`, confirmado que la caché se invalida al momento (crear un Funko aparece en el siguiente `GET` sin esperar), que CORS responde con los headers correctos en un preflight, y que el rate limiter corta en 429 al superar el límite (ráfaga de 110 peticiones → 87 pasan, 23 bloqueadas).

**Decisión tomada:** seguir al pie de la letra el código real de las soluciones de José Luis (`soluciones/21-ProductosOptimization`, `soluciones/22-ProductosComplete`, `24-documentacion.md`), no la teoría genérica de los `.md` cuando difiere del código (p. ej. el tipo de paginación se llama `PagedResponse<T>` en su código, aunque el `.md` lo llama `PagedResult<T>`).

**Alternativas descartadas:** ninguna — todo lo de este bloque coincide exactamente con el patrón de José Luis, sin margen de interpretación propia.

**Qué queda pendiente:** Bloques 2-8 (Redis, Mail, WebSocket, GraphQL, Mongo, Bruno), más el checklist final y la presentación en tres niveles.

## Entrada 16. Bloque 2: caché de aplicación con Redis

**Qué se hizo:** `ICacheService` (Get/Set/Remove/Exists/GetOrSet genéricos) con dos implementaciones — `MemoryCacheService` en Development (sin Docker), `RedisCacheService` en Production — exactamente el mismo patrón Dev/Prod que ya usa `DatabaseConfig` para SQLite/PostgreSQL. `GetOrSetAsync` usa un semáforo por clave para evitar cache stampede. Aplicado a `ServiceFunko` (el candidato obvio, más lecturas): `GetAllAsync`/`GetByIdAsync` pasan por la caché, y todas las escrituras invalidan tanto esta caché de aplicación como el OutputCache HTTP del Bloque 1 (un único método privado `InvalidarCacheAsync`). Redis añadido a `compose.yaml` (imagen `redis:7-alpine`, con contraseña, healthcheck).

**Verificado en vivo, con Redis real en Docker** (no solo con el `MemoryCacheService` de desarrollo): `docker compose up`, creado un Funko, comprobado con `redis-cli KEYS "*"` que la clave `funko:1` existe de verdad dentro del contenedor Redis, actualizado el precio y confirmado que la clave desaparece (invalidación real) y se repuebla con el valor correcto en la siguiente lectura.

**Decisión tomada:** solo `ServiceFunko` usa esta caché por ahora, no Categoria/Proveedor — para no dispersar el tiempo disponible; el patrón es el mismo si se quisiera extender luego.

**Alternativas descartadas:** decorar el repositorio con Scrutor en vez de inyectar la caché directamente en el servicio. El propio reto de José Luis lo marca como "puntos extra", no como el camino principal — se sigue el camino principal.

**Qué queda pendiente:** Bloques 3-8 (Mail, WebSocket, GraphQL, Mongo, Bruno), checklist final, presentación en tres niveles. Puerto de PostgreSQL en `compose.yaml` cambiado de 5433 a 5434 porque 5433 lo ocupaba otro proyecto (`beautylab-postgres`) en esta máquina — no es un cambio de diseño, es evitar un conflicto de puertos local.

## Entrada 17. Bloque 3: notificaciones por email

**Qué se hizo:** `IEmailService`/`MemoryEmailService` (deja constancia en el log, no envía nada de verdad) + `EmailTemplates` (HTML inline), siguiendo el código real de `soluciones/19-ProductosEmail` (no la versión más elaborada del `.md`, que incluye adjuntos/plantillas Razor/cola con `BackgroundService` — el código real de José Luis es mucho más simple: solo `MemoryEmailService` en los dos entornos, sin servidor SMTP). Disparo fire-and-forget (`_ = ...`) en `ServiceFunko.AddAsync`/`DeleteAsync`, con try/catch interno para que un fallo de email nunca tumbe la petición.

**Verificado en vivo:** creado y borrado un Funko, confirmado en el log el `=== EMAIL SENT ===` con destinatario, asunto y el nombre real del Funko en los dos casos.

**Decisión tomada:** no se activa MailKit/SMTP real (ni en Production). El propio código de referencia de José Luis lo deja comentado/pendiente en su solución — replicar exactamente eso, no inventar un servidor SMTP que él no activa.

**Alternativas descartadas:** cola de emails con `BackgroundService`/`IServiceScopeFactory` (versión "completa" del `.md`). Descartada: el código real que de verdad compila y pasa sus tests usa fire-and-forget simple, que es lo que se sigue aquí.

**Qué queda pendiente:** Bloques 5-8 (WebSocket, GraphQL, Mongo, Bruno), checklist final, presentación en tres niveles.

## Entrada 18. Bloque 5: WebSocket en tiempo real con SignalR

**Qué se hizo:** `FunkosHub` (SignalR, no WebSocket nativo — el propio material de José Luis dice "si no sabes cuál elegir, usa SignalR"), con el patrón exacto de `soluciones/16-ProductosRealTime`: grupo `admins` al conectar si el usuario tiene ese rol, `JoinCategoriaGroup`/`LeaveCategoriaGroup`, `Ping`/`Pong`. `ServiceFunko` notifica al grupo `admins` (`FunkoCreado`/`FunkoActualizado`/`FunkoEliminado`) tras cada escritura, fire-and-forget con try/catch, igual que el email. Corregido un hueco real entre la teoría y el código de José Luis: el `.md` dice que `OnMessageReceived` (JWT por query string) es "imprescindible" para que el Hub autentique, pero no está en el código de su solución — añadido a mano siguiendo el `.md`. También corregido el CORS del Bloque 1: `AllowAnyOrigin()` es incompatible con `AllowCredentials()` (que SignalR necesita), cambiado a `SetIsOriginAllowed(_ => true)`.

**Verificado en vivo de verdad, no simulado:** escrito un cliente SignalR real en Node.js (paquete oficial `@microsoft/signalr`), conectado con el JWT por query string, suscrito al evento `FunkoCreado`; al crear un Funko por REST en paralelo, el cliente recibió la notificación con el payload exacto en menos de un segundo.

**Decisión tomada:** solo se notifica al grupo `admins`, no por categoría. El reto de José Luis habla de notificar "al propietario" de un Funko, pero nuestro dominio no tiene ese concepto (a diferencia de `Producto.UsuarioId` en su ejemplo) — y el `CategoriaId` real tampoco llega hasta el `FunkoDto`/modelo de dominio (la Categoria se aplana a texto, decisión ya tomada en la Entrada 3). Extender esto exigiría cambiar el modelo de dominio de Funko, que es justo lo que está en juego sin resolver para el Bloque 6 (GraphQL) — se decide no tocarlo aquí.

**Alternativas descartadas:** WebSocket nativo (`app.UseWebSockets()`) en vez de SignalR. Descartado: el material lo marca como "solo informativo", SignalR es el camino recomendado y tiene reconexión automática, grupos y mejor soporte de cliente.

**Qué queda pendiente:** Bloques 6-8 (GraphQL, Mongo, Bruno), checklist final, presentación en tres niveles.

## Entrada 19. Bloque 6: GraphQL con HotChocolate

**Qué se hizo:** `FunkosQuery` (funkos, funko(id), categorias), `FunkoMutation` (crearFunko/actualizarFunko con `[Authorize]`, eliminarFunko con `[Authorize(Roles: Admin)]`) y `FunkoSubscription` (funkoCreado/funkoActualizado/funkoEliminado, publicadas con `ITopicEventSender` desde la Mutation), con HotChocolate 16.6.6 — mismos paquetes y mismo patrón de registro (`AddGraphQLServer().AddQueryType<T>()...`) que `soluciones/17-ProductosGraphQL`. GraphQL convive con los controladores REST en `/graphql`, sin sustituirlos.

**Decisión tomada — adaptación deliberada, no un descuido:** José Luis resuelve la relación Funko→Categoria con un `CategoriaByIdDataLoader` sobre la entidad de EF Core (evita N+1 queries). Aquí no hace falta: nuestro `FunkoDto` ya llega con la Categoria resuelta como texto desde el propio `ServiceFunko`/`RepositoryFunko` (decisión de la Entrada 3), así que Query y Mutation reutilizan directamente `IServiceFunko`/`IServiceCategoria` — los mismos servicios que usa la API REST, con su mismo `Result<T,DomainError>` — en vez de tocar la entidad o montar un DataLoader que no resolvería ningún problema real aquí. Se evita además un hueco que detecté en la investigación: el `CategoriaByIdDataLoader` de referencia pide `IDbContextFactory<AppDbContext>`, que la app real de José Luis nunca registra (solo su proyecto de tests) — no se arrastra ese problema.

**Verificado en vivo con `curl` contra `/graphql`:** query `funkos`/`funko(id)`/`categorias`, mutation `crearFunko` (con y sin token — sin token devuelve `AUTH_NOT_AUTHENTICATED`, con token crea de verdad y aparece en la siguiente query), mutation `eliminarFunko` con rol Admin (refleja el borrado lógico al momento). Las subscriptions están cableadas con el mismo mecanismo (`ITopicEventSender`/`[Subscribe][Topic]`) que ya se demostró en vivo con SignalR en la Entrada 18, pero no se ha probado con un cliente GraphQL-WS por falta de tiempo — queda declarado como pendiente, no como "hecho y probado".

**Alternativas descartadas:** ninguna de fondo — la única desviación real (sin DataLoader) está justificada arriba, no es una desviación arbitraria.

**Qué queda pendiente:** probar las subscriptions GraphQL con un cliente real (como se hizo con SignalR). Bloque 7 (Mongo), Bloque 8 (Bruno), checklist final, presentación en tres niveles.

<!-- La siguiente entrada se añade aquí, con el mismo formato: Qué se hizo / Decisión y porqué / Alternativas descartadas / Pendiente -->
