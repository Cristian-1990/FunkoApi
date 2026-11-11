# Guía para la exposición — FunkoApp

Documento para repasar antes del jueves. Dos partes: **checklist** de todo lo pedido vs. lo
hecho, y la **explicación en tres niveles** para que la domines de menos a más detalle.

---

## 1. Checklist de requisitos

### Las 12 fases de la pauta oficial (Práctica 3)

| # | Fase | Estado |
|---|---|---|
| 1 | Definir el recurso Funko y justificar propiedades | ✅ Hecho |
| 2 | Diseño en papel (tabla de endpoints) | ✅ Hecho |
| 3 | Minimal API | ❌ Omitida a propósito — se partió de un proyecto que ya tenía controladores (declarado en README) |
| 4 | Controladores MVC + comparación Minimal vs. Controllers | ✅ Hecho |
| 5 | Inyección de dependencias completa | ✅ Hecho |
| 6 | `Result<T>` para errores de dominio | ✅ Hecho |
| 7 | DTOs, validadores, mappers | ✅ Hecho |
| 8 | Configuración y logs (Serilog) | ✅ Hecho |
| 9 | Persistencia real con EF Core | ✅ Hecho (SQLite dev / PostgreSQL prod) |
| 10 | Seguridad JWT + BCrypt | ✅ Hecho |
| 11 | Documentación Swagger + tests | ✅ Hecho (82 tests unitarios + 14 de integración) |
| 12 | Despliegue con Docker | ✅ Hecho |

**11 de 12. La única omitida está declarada y justificada, no es un olvido.**

### Los extras pedidos al ampliar el plazo (pizarra de clase)

| Extra | Estado | Dónde |
|---|---|---|
| Swagger documentado por endpoint (XMLDoc) | ✅ Hecho | Bloque 1 |
| Optimización: paginación, `AsNoTracking` | ✅ Hecho | Bloque 1 |
| OutputCache | ✅ Hecho | Bloque 1 |
| CORS | ✅ Hecho | Bloque 1 |
| Rate limiting | ✅ Hecho | Bloque 1 |
| Auditoría de accesos (intentos fallidos) | ✅ Hecho | Bloque 1 |
| Caché con Redis | ✅ Hecho (sobre Funko) | Bloque 2 |
| Mail | ✅ Hecho (notificación al crear/borrar un Funko) | Bloque 3 |
| WebSocket (SignalR) | ✅ Hecho, **probado con un cliente real** | Bloque 5 |
| GraphQL | ✅ Hecho (queries, mutations, subscriptions) | Bloque 6 |
| MongoDB | ✅ Hecho (Categoria, alternativa seleccionable) | Bloque 7 |
| Cassandra | ❌ Descartado — coste desproporcionado (rediseño de acceso a datos tabla-por-consulta), decisión tomada contigo | — |
| Identity | ⏸️ Aparcado — pendiente de confirmar con Jesús/profesor qué significa exactamente | — |
| Todo con Docker (dev/prod) | ✅ Hecho | Todos los bloques |
| Test con Bruno | ✅ Hecho (colección representativa, no exhaustiva) | Bloque 8 |
| Proveedor expuesto en Funko | ✅ Hecho | Cerrado al revisar (y no fusionar) el trabajo paralelo de Jesús — detectamos que era un pendiente nuestro y lo resolvimos a nuestra manera |

### Lo que queda honestamente fuera o a medias

- **Identity**: aparcado desde el principio, sin resolver.
- **Subscriptions de GraphQL**: cableadas con el mismo mecanismo que SignalR, pero no probadas con un cliente real (sí lo están las de SignalR).
- **CI/CD con GitHub Actions**: el material de Docker de José Luis lo menciona como parte de su propio reto, pero no apareció en tu nota de clase — no se ha hecho, queda fuera salvo que lo pidas.
- **`.env` y nombre `docker-compose.yml`**: usamos variables de entorno directas en `compose.yaml` (nombre moderno del mismo fichero) en vez de un `.env` aparte — funciona igual, pero si el profesor pide el fichero `.env` literal, no está.
- **MongoDB solo en Categoria**, no en Proveedor — alcance reducido por tiempo, documentado.
- **Redis solo cachea Funko**, no Categoria/Proveedor — mismo motivo.

---

## 2. Explicación en tres niveles

### Nivel 1 — La versión de 30 segundos

FunkoApp es una API REST para gestionar el catálogo de una tienda de Funkos: crear, consultar,
actualizar y borrar figuras, categorías y proveedores. Cualquiera puede consultar el catálogo;
solo un usuario con sesión iniciada (y rol Admin, en algunos casos) puede modificarlo. Los datos
se guardan de verdad en una base de datos, no se pierden al reiniciar. Todo corre dentro de
contenedores Docker, documentado, probado y con capas extra de rendimiento, seguridad y
comunicación en tiempo real.

### Nivel 2 — La versión de 2-3 minutos

**Qué resuelve.** Partí de un proyecto heredado de un compañero que no compilaba y tenía
varios fallos de diseño (sin inyección de dependencias, rutas duplicadas, sin manejo de
errores). Lo reconstruí siguiendo la arquitectura de nuestro profesor: un controlador que solo
habla con un servicio, un servicio que aplica las reglas de negocio y nunca lanza excepciones
de negocio (usa `Result<T, DomainError>`), y un repositorio que solo habla con la base de datos
y no decide nada.

**Las tres entidades.** Funko es el producto. Cada uno pertenece a una Categoria y,
opcionalmente, a un Proveedor. Las tres tienen CRUD completo: cualquiera puede leer, solo un
usuario autenticado (y con rol Admin para Categoria/Proveedor) puede escribir.

**Seguridad.** JWT firmado a mano (sin librerías de identidad completas), contraseñas con
BCrypt. Además, ahora registramos en el log cada intento de acceso fallido, por si hace falta
auditar un abuso.

**Rendimiento.** Dos capas de caché distintas: una a nivel de respuesta HTTP (OutputCache,
vale para cualquier endpoint de lectura pública) y otra a nivel de aplicación (Redis en
producción, memoria en desarrollo), que evita volver a consultar la base de datos cuando el
mismo dato se pide muchas veces seguidas. Las dos se invalidan automáticamente en cuanto algo
cambia, para no servir datos viejos.

**Tiempo real y alternativas de consulta.** Si creas, actualizas o borras un Funko, dos cosas
pasan a la vez: se manda un email de notificación (simulado, queda en el log) y se avisa por
WebSocket (SignalR) a quien esté conectado — lo comprobé con un cliente real escrito en Node.js,
no es teoría. Además de los endpoints REST normales, hay una API GraphQL alternativa en
`/graphql`, pensada para quien prefiera pedir justo los campos que necesita en una sola
petición.

**Persistencia políglota.** La base de datos principal es SQL (SQLite en desarrollo,
PostgreSQL en producción), pero Categoria puede guardarse también en MongoDB — es una decisión
de configuración, no de código: el controlador y el servicio nunca se enteran de qué motor hay
detrás.

**Todo dockerizado y probado de verdad.** Un `docker compose up` levanta la API, PostgreSQL,
Redis y MongoDB juntos. 82 tests automáticos (unitarios con Moq + de integración con la API real
levantada). Y nada de esto lo doy por bueno porque "compile": cada pieza la he arrancado y
probado en caliente antes de darla por terminada.

### Nivel 3 — La versión detallada, en tres pasos

#### Paso 1: la base (arquitectura y CRUD)

Empieza explicando la cadena `Controller → Service → Validator/Repository`. Dibuja (o describe)
el flujo de una petición `POST /api/Funko`:
1. El controlador recibe el DTO (`CreateFunkoDto`), ASP.NET Core ya valida las Data Annotations
   antes de que llegue el método.
2. El servicio valida lo que las Data Annotations no cubren (p. ej. un nombre de solo espacios),
   comprueba que no exista ya un Funko con ese nombre, y si todo va bien, llama al repositorio.
3. El repositorio guarda en la base de datos y devuelve la entidad creada (un valor plano, nunca
   un `Result`).
4. El servicio convierte eso en un DTO de salida y lo envuelve en `Result.Success`.
5. El controlador traduce ese resultado a un código HTTP (`201 Created`, con la URL del nuevo
   recurso).

Si algo falla en cualquier paso, el error vuelve como `Result.Failure<DomainError>`, nunca como
una excepción — así el controlador siempre sabe qué código HTTP devolver, con un `switch` sobre
el tipo de error (`NotFound`→404, `AlreadyExists`→409, `Validation`→400).

Menciona que esto mismo se repite igual para Categoria y Proveedor — el mismo patrón, no tres
diseños distintos.

#### Paso 2: lo que hace que esto sea "de verdad" (persistencia, seguridad, rendimiento)

- **Persistencia real**: EF Core con migraciones versionadas en SQLite; en producción,
  PostgreSQL crea el esquema directamente desde el modelo (`EnsureCreated`), porque mantener dos
  historiales de migración en paralelo no compensaba en esta fase — decisión documentada.
- **Seguridad**: JWT con claims propios (`sub`, `email`, `role`), sin ASP.NET Core Identity
  (decisión deliberada: más control, menos magia). BCrypt con factor de coste 11.
- **Caché en dos capas**: explica la diferencia entre OutputCache (cachea la respuesta HTTP
  entera, antes de que la petición llegue al controlador) y la caché de aplicación con Redis
  (cachea el resultado de una consulta concreta, reutilizable aunque la petición no venga por
  HTTP — por ejemplo, desde GraphQL). Las dos se invalidan juntas tras cada escritura.
- **Seguridad de la API en sí**: CORS, rate limiting (100 peticiones/minuto por IP con la API
  nativa de .NET, no una librería de terceros), y log de cada intento de acceso fallido.

#### Paso 3: lo que la hace moderna (tiempo real, GraphQL, políglota) y cómo se verificó

- **SignalR**: cuando se crea/actualiza/borra un Funko, se notifica al grupo `admins` conectado
  por WebSocket. Lo probaste con un cliente Node.js real, no simulado: el cliente se conectó, se
  suscribió, y en menos de un segundo recibió el evento con los datos exactos del Funko creado
  por REST en paralelo.
- **GraphQL**: una API alternativa (HotChocolate) que convive con los controladores REST,
  hablando con los mismos servicios — nunca duplicando lógica de negocio. Queries, mutations con
  autorización por rol, y subscriptions cableadas con el mismo mecanismo que SignalR.
- **MongoDB**: Categoria puede vivir en MongoDB en vez de PostgreSQL, decidido por configuración
  (`DatabaseSettings:CategoriaProvider`). El controlador, el servicio y los DTOs no cambian ni
  una línea — solo cambia qué clase implementa `IRepositoryCategoria`. Lo verificaste mirando
  directamente dentro del contenedor de Mongo con `mongosh` y viendo el documento real.
- **Cómo se verificó todo**: nunca "parece que funciona". Cada bloque se arrancó de verdad
  (`dotnet run` o `docker compose up`), se probó con `curl` o un cliente real, y solo entonces se
  dio por cerrado y documentado en `DECISIONES.md`.

**Cierre sugerido**: "Lo más importante no es la lista de tecnologías, es que cada una se montó
siguiendo el código real de nuestro profesor, se verificó en caliente, y se decidió
conscientemente qué quedaba dentro y qué fuera, según el tiempo disponible — no por accidente."
