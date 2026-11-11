# FunkoApp

![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-14-239120?style=flat-square&logo=csharp&logoColor=white)
![Tests](https://img.shields.io/badge/tests-173%20passing-2ea44f?style=flat-square)
![Cobertura](https://img.shields.io/badge/cobertura-88.5%25-2ea44f?style=flat-square)
![SQLite](https://img.shields.io/badge/EF%20Core-SQLite-003B57?style=flat-square&logo=sqlite&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=flat-square&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-multi--stage-2496ED?style=flat-square&logo=docker&logoColor=white)
![Swagger](https://img.shields.io/badge/OpenAPI-Swagger-85EA2D?style=flat-square&logo=swagger&logoColor=black)
![JWT](https://img.shields.io/badge/Auth-JWT%20%2B%20BCrypt-000000?style=flat-square&logo=jsonwebtokens&logoColor=white)

Práctica 3 de Desarrollo Web en Entorno Servidor (DWES), unidad 2: una API REST para
gestionar un catálogo de Funkos (figuras de vinilo coleccionables).

**Autores:** Cristian Álvarez · Jesús Cobo (base inicial del proyecto, ver
[De dónde viene este proyecto](#-de-dónde-viene-este-proyecto))

---

## 🎯 Qué hace

CRUD completo sobre un catálogo de Funkos en memoria, con validación en dos capas,
errores de dominio sin excepciones, documentación Swagger y despliegue con Docker.

| Operación | Verbo | Ruta | Auth | Éxito |
|---|---|---|---|---|
| Listar todos los Funkos | `GET` | `/api/Funko` | pública | `200` |
| Consultar un Funko por id | `GET` | `/api/Funko/{id}` | pública | `200` / `404` |
| Buscar Funkos por nombre | `GET` | `/api/Funko/search?nombre=` | pública | `200` |
| Crear un Funko | `POST` | `/api/Funko` | autenticado | `201` |
| Actualizar un Funko completo | `PUT` | `/api/Funko/{id}` | autenticado | `200` / `404` |
| Actualizar solo el precio | `PATCH` | `/api/Funko/{id}` | autenticado | `200` / `404` |
| Eliminar un Funko (borrado lógico) | `DELETE` | `/api/Funko/{id}` | **solo Admin** | `204` / `404` / `403` |
| Listar/buscar Categorias | `GET` | `/api/Categoria`, `/api/Categoria/search?nombre=` | pública | `200` |
| Consultar una Categoria por id | `GET` | `/api/Categoria/{id}` | pública | `200` / `404` |
| Crear/Actualizar/Borrar Categoria | `POST`/`PUT`/`DELETE` | `/api/Categoria[/{id}]` | **solo Admin** | `201`/`200`/`204` |
| Listar/buscar Proveedores | `GET` | `/api/Proveedor`, `/api/Proveedor/search?nombre=` | pública | `200` |
| Consultar un Proveedor por id | `GET` | `/api/Proveedor/{id}` | pública | `200` / `404` |
| Crear/Actualizar/Borrar Proveedor | `POST`/`PUT`/`DELETE` | `/api/Proveedor[/{id}]` | **solo Admin** | `201`/`200`/`204` |
| Registrarse | `POST` | `/api/auth/signup` | pública | `201` / `409` |
| Iniciar sesión | `POST` | `/api/auth/login` | pública | `200` / `401` |
| Perfil del usuario autenticado | `GET` | `/api/auth/me` | autenticado | `200` / `401` |

---

## 🏗️ Arquitectura

```
Program.cs → FunkoController → ServiceFunko → FunkoValidator
                                             → RepositoryFunko   (EF Core, async)
                                                   → AppDbContext → SQLite (Dev) / PostgreSQL (Prod)
```

El controlador no conoce el repositorio ni el modelo de dominio: solo habla con
`IServiceFunko` y solo recibe y devuelve DTOs (`CreateFunkoDto`, `UpdateFunkoDto`,
`FunkoDto`).

Los errores de negocio no se lanzan como excepciones. Cada operación del servicio
devuelve un `Result<T, DomainError>` de CSharpFunctionalExtensions, y el controlador
traduce cada tipo de error a su código HTTP:

| `DomainError` | Código HTTP | Cuándo |
|---|---|---|
| `NotFound` | `404` | El id no existe (o ya estaba borrado) |
| `AlreadyExists` | `409` | Ya hay un Funko activo con ese nombre |
| `Validation` | `400` | El nombre o la categoría están vacíos/en blanco |
| `Unauthorized` | `401` | Credenciales inválidas, o sin token / token inválido |
| `Conflict` | `409` | Email ya registrado (`signup`) |

La persistencia es real (EF Core + SQLite, con migraciones), no en memoria: hay una
relación **Uno a Muchos** entre `Categoria` y `Funko` en la base de datos, y el
borrado es lógico (`IsDeleted`, oculto automáticamente de cualquier consulta con un
`HasQueryFilter`). El modelo de dominio sigue viendo `Categoria` como un simple
`string` — es el repositorio quien resuelve o crea la fila de categoría al guardar.

El repositorio nunca lanza ni devuelve un `Result`: solo valores planos (`null`,
`false`). Es el servicio quien decide qué significa cada uno. Las Data Annotations
de los DTOs (`[Required]`, `[Range]`, `[StringLength]`) cubren el formato de entrada
y las captura automáticamente `[ApiController]` antes de que la petición llegue al
controlador; el `FunkoValidator` cubre lo que las Data Annotations no ven bien (por
ejemplo, una cadena de solo espacios).

Las excepciones que de verdad son inesperadas (no un fallo de negocio, sino un error
de programación) las captura `ExceptionHandlerMiddleware` y las traduce también a
`ProblemDetails`, para que el cliente nunca vea un 500 en blanco.

---

## 🔐 Autenticación

Lectura pública, escritura solo para usuarios autenticados, borrado solo para el rol
`ADMIN`. JWT firmado con HMAC-SHA256 (sin ASP.NET Core Identity: enfoque manual, con
`JwtService` + `AuthService`), contraseñas con BCrypt (`workFactor: 11`).

**Usuarios de prueba** (creados automáticamente al arrancar si la tabla está vacía):

| Email | Contraseña | Rol |
|---|---|---|
| `admin@funko.com` | `admin123` | `ADMIN` |
| `user@funko.com` | `user123` | `USER` |

```bash
curl -X POST http://localhost:5230/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@funko.com","password":"admin123"}'
```

La respuesta trae `accessToken`: se usa como `Authorization: Bearer <token>` en las
peticiones protegidas. En Swagger (`/swagger`), el botón **Authorize** (arriba a la
derecha) pide el token con el mismo formato (`Bearer eyJ...`).

> [!NOTE]
> El token dura 15 minutos (`Jwt:ExpirationMinutes` en `appsettings.json`). La clave
> de firma vive en claro en `appsettings.json` porque esto es una práctica académica
> con SQLite local — en un proyecto real iría a `dotnet user-secrets`, una variable
> de entorno o un gestor de secretos, nunca comiteada.

---

## 📁 Estructura del proyecto

```
.
├── FunkoAppi01.sln
├── Dockerfile                  (dentro de FunkoAppi01/)
├── compose.yaml
├── FunkoAppi01/
│   ├── Program.cs
│   ├── Controller/             FunkoController
│   ├── Service/                IServiceFunko, ServiceFunko
│   ├── Repository/              IRepositoryFunko, RepositoryFunko (en memoria)
│   ├── Validators/              IFunkoValidator, FunkoValidator
│   ├── Dto/                     CreateFunkoDto, UpdateFunkoDto, FunkoDto
│   ├── Mappers/                 FunkoMapper (DTO ↔ modelo)
│   ├── Models/                  Funko
│   ├── Errors/                  DomainError y su factoría FunkoErrors
│   ├── Middleware/               ExceptionHandlerMiddleware
│   └── Infraestructure/         Registro de dependencias (repos, validadores, servicios)
└── FunkoApp.Test/              31 tests con NUnit, Moq y FluentAssertions
```

---

## ▶️ Cómo ejecutarlo

```bash
dotnet run --project FunkoAppi01
```

Arranca en `http://localhost:5230` (o el puerto que asigne `dotnet run`). La
documentación interactiva está en `http://localhost:5230/swagger`.

### Con Docker (API + PostgreSQL)

```bash
docker compose up --build
```

Levanta **PostgreSQL** (perfil Production) y la API, que espera a que la base de
datos esté realmente lista (`healthcheck` + `depends_on: condition: service_healthy`)
antes de arrancar. Publica la API en `http://localhost:8090` (Swagger en
`http://localhost:8090/swagger`) y PostgreSQL en `localhost:5433`.
Para pararlo: `Ctrl+C` y después `docker compose down` (añade `-v` para borrar
también los datos de Postgres).

> [!NOTE]
> Los puertos del host son **8090** y **5433**, no 8080/5432: en la máquina de
> desarrollo esos puertos ya estaban ocupados por procesos ajenos al proyecto —
> de hecho, así se descubrió que el `compose.yaml` original no publicaba ningún
> puerto en absoluto. Ver [Decisiones de diseño](#-decisiones-de-diseño).

---

## ✅ Tests

```bash
dotnet test
```

**173 tests**, con NUnit, Moq y FluentAssertions, patrón AAA (Arrange, Act, Assert) y
separados en `CasosPositivos` y `CasosNegativos`.

| Qué se prueba | Nº |
|---|---|
| `DomainError` | 7 |
| `FunkoMapper` | 3 |
| `FunkoValidator` | 7 |
| `RepositoryFunko` (SQLite real en memoria, sin mocks) | 13 |
| `RepositoryCategoria` (SQLite real en memoria, sin mocks) | 8 |
| `RepositoryProveedor` (SQLite real en memoria, sin mocks) | 7 |
| `ServiceFunko` (repositorio y validador mockeados) | 11 |
| `ServiceCategoria` (repositorio y validador mockeados) | 11 |
| `CategoriaValidator` | 4 |
| `ServiceProveedor` (repositorio y validador mockeados) | 11 |
| `ProveedorValidator` | 5 |
| `JwtService` | 2 |
| `AuthService` (repositorio mockeado) | 6 |
| `MemoryCacheService` / `RedisCacheService` | 15 |
| `MemoryEmailService` | 2 |
| `ExceptionHandlerMiddleware` | 3 |
| `FunkosQuery` / `FunkoMutation` / `FunkoSubscription` (GraphQL) | 11 |
| `FunkosHub` (SignalR) | 5 |
| `RepositoriesConfig` / `CacheConfig` / `RateLimitConfig` (registro DI) | 5 |
| `FunkoController` — integración con `WebApplicationFactory` | 12 |
| `CategoriaController` — integración con `WebApplicationFactory` | 10 |
| `ProveedorController` — integración con `WebApplicationFactory` | 10 |
| `AuthController` — integración con `WebApplicationFactory` | 6 |

### Cobertura

```bash
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
reportgenerator -reports:"FunkoApp.Test/TestResults/*/coverage.cobertura.xml" -targetdir:docs/coverage -reporttypes:"Html;TextSummary"
```

El informe visual está en [`docs/coverage/index.html`](docs/coverage/index.html). **88.5%**
de líneas cubiertas (excluyendo `Program.cs`, las migraciones de EF Core y el código
autogenerado por el SDK para OpenAPI — ninguno es código de negocio nuestro, igual que
`RepositorioRemoto` excluye su propio `Program.cs`). Prácticamente toda la aplicación tiene
tests directos, incluidos GraphQL, SignalR y la caché, llamando a las clases directamente
con sus dependencias mockeadas en vez de levantar el transporte completo (WebSocket/HTTP) —
más simple y igual de válido para una clase C# normal. La única excepción real es
`RepositoryCategoriaMongo`: verificado en vivo contra Mongo real (`mongosh`, ver
`DECISIONES.md`), pero sin test automático por el tiempo que llevaría montar un Mongo de
pruebas aislado.

Los de integración levantan la API entera (sin mocks en ningún punto) contra una
base de datos EF Core InMemory aislada por test, y no solo miran el código de
estado: también comprueban la **forma** del error (p. ej. que un `400` trae
`errors` agrupados por campo, y que un `404` trae un `detail` legible) — es el
contrato real que vería un cliente.

Los de `ServiceFunko` no solo miran lo que devuelve el método, sino a quién llama:
si la validación falla, o el nombre ya existe, o el precio es negativo, se verifica
con `Times.Never` que el repositorio **no llega a tocarse**. Si alguien rompiera ese
cortocircuito, el valor devuelto podría seguir pareciendo correcto — esa comprobación
es la única que lo detectaría.

---

## 🧠 Decisiones de diseño

### Minimal API vs. controladores

La pauta pide implementar el CRUD primero como Minimal API (fase 3) y después con
controladores (fase 4), comparando ambos enfoques. Partimos del código de un
compañero que ya tenía el CRUD hecho con controladores, así que **no se ha hecho la
versión Minimal API por separado**: decisión consciente, para llegar antes a las
fases avanzadas (validación, errores de dominio, tests) con el tiempo que había.
Si hubiera que elegir solo uno para un proyecto real, los controladores ganan en
cuanto el número de recursos crece: agrupan mejor las acciones relacionadas y se
documentan solos con los atributos `[Http*]` y XMLDoc, mientras que una Minimal API
con muchos endpoints se vuelve un `Program.cs` largo y repetitivo.

### Validación en dos capas, no una

Las Data Annotations de los DTOs cubren lo que se puede expresar declarativamente
(`[Required]`, `[Range]`). El `FunkoValidator` cubre lo que no: comprobamos en vivo
que `[Required]` ya rechaza una cadena de solo espacios (.NET la recorta antes de
comprobar), así que en la práctica el validador de dominio actúa como red de
seguridad para cualquier llamada que no pase por el `ModelState` de HTTP — por
ejemplo, si el servicio se usa algún día desde un test o desde otro punto de entrada.
No es redundancia inútil: es que un camino de entrada (HTTP) ya atrapa el error antes,
pero el servicio sigue estando protegido aunque no sea por ese camino.

### Nombre duplicado es responsabilidad del servicio, no del repositorio

El repositorio solo expone `ExistsByName(nombre, excludeId)` y devuelve `bool`. Es el
`ServiceFunko` quien decide que un nombre duplicado es un `DomainError.AlreadyExists`.
Así todas las reglas de negocio quedan en un único sitio y el repositorio se limita a
guardar y consultar.

### Borrado lógico con un único booleano

El modelo original tenía `IsActivo` (booleano) y `DeleteAt` (fecha) a la vez, pero
solo se usaba el booleano para filtrar. Un único campo `IsDeleted` es suficiente para
lo que pide la pauta; mantener los dos obligaría a sincronizarlos siempre juntos y es
fuente de bugs si alguno se actualiza y el otro no.

### El repositorio pasó a async en cuanto hubo E/S real

Mientras el repositorio era un `Dictionary` en memoria no había ninguna E/S que
esperar, así que se quedó síncrono (ver la decisión equivalente más abajo, cuando
aún no existía la base de datos). En cuanto llegó EF Core con SQLite, todo el camino
—repositorio, servicio y controlador— pasó a `async`/`Task`, que es justo el momento
en el que el `async` empieza a tener sentido real.

### `Categoria` es solo un texto en el dominio, una relación real en la base de datos

Los DTOs de la pauta (`CreateFunkoDto`, `FunkoDto`...) solo piden `Categoria` como
una cadena de texto. La base de datos sí tiene la relación Uno a Muchos real que pide
la pauta (`CategoriaEntity` ← `FunkoEntity`, con clave foránea e índice único). El
repositorio resuelve o crea la fila de categoría por nombre al guardar un Funko, y la
aplana de vuelta a texto al leer. Así no hay que tocar DTOs, validador, mapper ni
tests ya hechos solo para que el dominio conozca una relación que no necesita conocer.

### Migraciones reales, no `EnsureCreated`

La base de datos se crea con `dotnet ef migrations` + `Database.MigrateAsync()` al
arrancar, no con `EnsureCreatedAsync()` (que sí es válido y es lo que usan otras
prácticas de la unidad). Aquí se eligió así porque esta pauta pide explícitamente
"migraciones" por su nombre.

### Autenticación: enfoque manual, no Identity, sin excepciones

El tema de la unidad (`16-autenticacion.md`) trae un "Reto: autenticación con JWT en
FunkoApp" con dos enfoques (manual e Identity) y extras (OAuth2, refresh tokens, rate
limiting). Se implementó solo el **enfoque manual** (`JwtService` + `AuthService`),
que es lo que pide la pauta real de la práctica — Identity, OAuth2 y los extras son
los "puntos extra" del reto, no un requisito de esta fase.

Dos adaptaciones del código tal cual lo trae el tema, ambas para seguir las normas ya
aplicadas al resto del proyecto:
- La entidad `User` del tema lleva `[Required]`/`[MaxLength]` directamente encima.
  Aquí se mantiene limpia (sin atributos) y esas reglas van en Fluent API, igual que
  `FunkoEntity`/`CategoriaEntity`.
- El tema usa `try/catch` con excepciones (`UnauthorizedAccessException`,
  `InvalidOperationException`). Aquí `AuthService` devuelve `Result<T, DomainError>`,
  como el resto de la aplicación — se añadieron `DomainError.Unauthorized` y
  `DomainError.Conflict` para los dos casos nuevos.

### SQLite con migraciones reales, PostgreSQL con `EnsureCreated`

Las migraciones versionadas en el repo se generaron con SQLite como proveedor activo.
Al activar PostgreSQL, EF Core detectó que el modelo no coincide exactamente entre
los dos motores y bloqueó la migración (`PendingModelChangesWarning`). Mantener dos
historiales de migración en paralelo es la solución "de libro", pero no compensa el
esfuerzo en esta fase: PostgreSQL crea el esquema directamente desde el modelo actual
con `EnsureCreatedAsync`, y SQLite sigue con `MigrateAsync` de verdad (que es lo que
pedía explícitamente la fase 9). `Program.cs` decide según el proveedor activo.

### Fechas siempre en UTC

Activar PostgreSQL destapó un bug que SQLite nunca habría mostrado: el código
guardaba `DateTime.Now` (hora local), y Npgsql exige UTC para sus columnas
`timestamp with time zone`. Se cambió a `DateTime.UtcNow` en todo el proyecto — más
allá de arreglar Postgres, es la práctica correcta en cualquier base de datos.

### El puerto del contenedor no estaba publicado

Al preparar este README comprobamos `docker compose up` de verdad (no solo que
compilase el Dockerfile) y la API no respondía desde el host. El contenedor
arrancaba y escuchaba bien en el 8080 **dentro** de la red de Docker, pero
`compose.yaml` no tenía ninguna sección `ports`, así que ese puerto nunca llegaba al
host. Se añadió `ports: ["8090:8080"]` (8090 porque el 8080 de esta máquina ya estaba
en uso por otro proceso) y se volvió a comprobar: Swagger y la creación de un Funko
responden correctamente desde fuera del contenedor.

---

## 🧰 Tecnologías

| Para qué | Qué se usa |
|---|---|
| API REST | ASP.NET Core con controladores |
| Errores sin excepciones | CSharpFunctionalExtensions (`Result<T, DomainError>`) |
| Documentación | Swashbuckle.AspNetCore (Swagger UI en `/swagger`) |
| Logging | Serilog (consola + fichero con rotación diaria, nivel por perfil) |
| Persistencia | EF Core (SQLite en Dev con migraciones; PostgreSQL en Production con Npgsql) |
| Autenticación | JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`) + BCrypt (`BCrypt.Net-Next`) |
| Inyección de dependencias | `Microsoft.Extensions.DependencyInjection` + Scrutor |
| Contenedores | Dockerfile multi-etapa + Docker Compose |
| Tests | NUnit + Moq + FluentAssertions |

---

## 📜 De dónde viene este proyecto

Este repositorio parte del CRUD inicial (en memoria, con DI básica) que hizo Jesús
Cobo en [`FunkoApi`](https://github.com/JesusCoboArrogante/FunkoApi). Se clonó en
local y se ha ido corrigiendo y ampliando por fases desde ahí, documentado paso a
paso en [`DECISIONES.md`](DECISIONES.md). De la base original se arreglaron, entre otras
cosas: un error que impedía compilar, un controlador que ignoraba por completo la
inyección de dependencias, dos endpoints con la misma ruta, y la ausencia de
`Create`, de manejo de excepciones y de documentación Swagger — el detalle completo
está en la Entrada 0 de `DECISIONES.md`.

---

## 🚧 Qué queda fuera

De las 12 fases de la pauta, 11 están hechas. Solo queda fuera, a propósito:

- **Fase 3 (Minimal API):** no se hizo la versión aparte en Minimal API para
  compararla con los controladores, como pedía la fase 4 — se partió de un proyecto
  que ya tenía controladores. Ver [Decisiones de diseño](#-decisiones-de-diseño).

Y un único caso de borde opcional dentro de la fase 11 (tests de integración): no
hay un test dedicado que fuerce un token JWT realmente expirado (requeriría un
`JwtConfig` de expiración casi inmediata solo para ese test); sí está probado que un
token ausente o falsificado se rechaza con `401`.

Sobre los extras pedidos al ampliar el plazo (optimización, Redis, email, SignalR,
GraphQL, MongoDB, Bruno — ver `DECISIONES.md` para el detalle de cada bloque):

- **Identity**: aparcado, pendiente de confirmar con el profesor qué se pedía
  exactamente con ese punto.
- **Redis** solo cachea `Funko`; **MongoDB** solo tiene a `Categoria` como motor
  alternativo — alcance reducido conscientemente por tiempo, no por descuido.
- **Cassandra**: descartado por su coste (exige rediseñar el acceso a datos
  tabla-por-consulta, no es "cambiar de proveedor").
- GraphQL, SignalR y el repositorio Mongo están verificados en vivo con clientes
  reales, pero no tienen tests automáticos todavía (ver la sección de
  [Cobertura](#cobertura) arriba).
- CI/CD con GitHub Actions: mencionado en el material de Docker del profesor, no en
  el encargo concreto de clase — no se ha hecho.

Todo lo demás —persistencia real con migraciones, relaciones Uno a Muchos, JWT +
BCrypt, tests unitarios y de integración, Docker con PostgreSQL— está hecho y
verificado en vivo, no solo "compila".
