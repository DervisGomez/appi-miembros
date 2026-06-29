# Church API — Guía de Entrevista Técnica

Preguntas frecuentes con respuestas basadas en este proyecto. Nivel: Semi Senior / Senior.

---

## Cuéntame sobre este proyecto

Church API es una API REST en ASP.NET Core 8 para gestionar miembros de una iglesia y sus donaciones. Lo construí como proyecto de portafolio para demostrar competencias de backend: autenticación JWT, autorización por roles, EF Core con SQL Server, logging estructurado, manejo global de errores, contenedorización y testing automatizado.

La API expone endpoints para registro/login de usuarios, CRUD de miembros, gestión de donaciones con paginación y filtros, y un health check de base de datos. Tiene 46 tests entre unitarios e integración, CI con GitHub Actions, y está preparada para desplegarse en Azure Container Apps con Azure SQL Database.

---

## ¿Qué arquitectura utilizaste?

Usé una **arquitectura en capas dentro de un único proyecto API**, sin Clean Architecture completa porque el dominio es acotado y no justifica la complejidad adicional.

```
Controllers → Services → DbContext → SQL Server
```

- **Controllers**: solo HTTP — routing, autorización, binding de DTOs
- **Services**: lógica de negocio, validaciones de dominio, logging
- **DTOs + Mappers**: desacoplan el contrato HTTP de las entidades EF
- **Middleware**: captura excepciones y devuelve `ProblemDetails`
- **Extensions**: modularizan la configuración (`AddApplicationDatabase`, `AddApplicationAuthentication`, etc.)

Elegí esta estructura porque es mantenible, fácil de navegar en entrevistas, y demuestra separación de responsabilidades sin sobreingeniería.

---

## ¿Cómo desplegaste en Azure?

El flujo de despliegue es:

```
Dockerfile → Azure Container Registry → Azure Container Apps → Azure SQL Database
```

1. **Dockerfile multi-stage**: compila en SDK, publica en runtime slim de ASP.NET 8
2. **ACR**: almacena la imagen con `az acr build`
3. **Container Apps**: ejecuta el contenedor con ingress externo en puerto 8080
4. **Azure SQL**: base de datos gestionada, conectada via `ConnectionStrings__SqlServer`

Los secretos (JWT, connection string) se configuran como variables de entorno o secretos de Container Apps / Key Vault, nunca en el código. El health check en `/health` se usa como probe de liveness/readiness.

Para el primer despliegue activo `Database__ApplyMigrations=true`; en producción madura, las migraciones las ejecutaría un pipeline CI/CD separado.

---

## ¿Cómo manejaste la configuración?

Tres niveles, siguiendo las convenciones de ASP.NET Core:

1. **`appsettings.json`**: valores por defecto sin secretos
2. **User-secrets**: desarrollo local (`dotnet user-secrets set`)
3. **Variables de entorno**: Docker, Azure y CI (`Jwt__Secret`, `ConnectionStrings__SqlServer`)

Usé el **Options Pattern** con `JwtOptions` para configuración tipada de JWT. La validación falla al inicio (`InvalidOperationException`) si faltan valores requeridos, en lugar de fallar en runtime al primer request.

`Database__ApplyMigrations` es un flag opcional que controla si EF aplica migraciones al arrancar — útil en Docker y primer despliegue en Azure.

---

## ¿Cómo protegiste la API?

**Autenticación JWT:**
- Tokens firmados con HMAC-SHA256
- Claims: `NameIdentifier` (user ID) y `Role`
- Validación de issuer, audience, lifetime y signing key
- `ClockSkew = Zero`

**Autorización:**
- `[Authorize]` para endpoints protegidos
- `[Authorize(Roles = "Admin")]` para operaciones privilegiadas (eliminar, actualizar, promover usuarios)

**Contraseñas:**
- Hash con `PasswordHasher` de ASP.NET Core Identity
- Nunca se almacenan ni devuelven en texto plano

**Configuración:**
- `RequireHttpsMetadata = true` fuera de Development
- Secretos fuera del repositorio

Lo que **no** implementé (y mencionaría como mejora): refresh tokens, rate limiting, lockout por intentos fallidos, CORS explícito.

---

## ¿Cómo implementaste Health Checks?

Registré un health check personalizado `SqlServerHealthCheck` que usa `AppDbContext.Database.CanConnectAsync()` para verificar conectividad real a la base de datos.

```csharp
services.AddHealthChecks()
    .AddCheck<SqlServerHealthCheck>("sqlserver", tags: new[] { "database", "critical" });
```

El endpoint `GET /health` devuelve JSON con estado, duración y detalle por check. Retorna `200` si está healthy y `503` si no.

En Docker Compose y Azure Container Apps, este endpoint sirve como health probe. En Serilog, las peticiones a `/health` se loguean en nivel `Debug` para no contaminar los logs.

---

## ¿Cómo manejaste las migraciones?

EF Core Migrations con historial versionado en `Migrations/`. El modelo tiene constraints explícitos: índices únicos, longitudes máximas, `decimal(18,2)`, FK con `DeleteBehavior.Restrict`.

**Desarrollo local:**
```bash
dotnet ef database update --project src/ChurchApi
```

**Docker / Azure (primer despliegue):**
`Database__ApplyMigrations=true` ejecuta `dbContext.Database.Migrate()` al iniciar la aplicación.

En producción madura, separaría las migraciones del startup y las ejecutaría en el pipeline de CI/CD antes del despliegue, para evitar race conditions con múltiples réplicas.

---

## ¿Cómo estructuraste los tests?

**Unit tests (32):** prueban servicios directamente con EF Core InMemory y Moq para `IJwtTokenService`. Cubren happy path, conflictos, not found, validaciones.

**Integration tests (14):** usan `WebApplicationFactory<Program>` con SQLite in-memory, reemplazando el DbContext de SQL Server. Prueban el pipeline HTTP completo: routing, auth, middleware, serialización.

Patrón Arrange-Act-Assert con FluentAssertions. Factory con seed de usuario admin para tests que requieren rol privilegiado.

---

## ¿Qué desafíos encontraste?

1. **Race conditions en registro**: dos requests simultáneos con el mismo email. Solución: check previo + catch de unique constraint con `PersistenceExceptionTranslator`.

2. **Tests cross-provider**: producción usa SQL Server, tests usan SQLite. Creé un traductor de excepciones que mapea códigos de error de ambos proveedores a excepciones de dominio.

3. **Configuración en integration tests**: la app valida connection string y JWT al arrancar. La factory inyecta configuración en memoria antes de reemplazar el DbContext.

4. **Paginación duplicada**: la lógica estaba repetida en `MemberService` y `DonationService`. La extraje a `PaginationHelper` sin cambiar comportamiento.

---

## ¿Qué mejorarías?

| Mejora | Motivo |
|---|---|
| Refresh tokens | Mejor UX sin comprometer seguridad del access token corto |
| Rate limiting | Proteger `/login` y `/register` contra fuerza bruta |
| FluentValidation | Reglas de negocio más expresivas que Data Annotations |
| OpenTelemetry | Trazas distribuidas en Azure Application Insights |
| Migraciones en CI/CD | Separar del startup para entornos con múltiples réplicas |
| Cobertura en CI | `coverlet` con umbral mínimo en GitHub Actions |
| API versioning | Preparar evolución del contrato sin breaking changes |

---

## Preguntas rápidas de seguimiento

**¿Por qué no usaste Clean Architecture?**
El dominio tiene 3 entidades y 3 servicios. Clean Architecture añadiría 4+ proyectos sin beneficio proporcional. La separación actual es suficiente y demuestra el criterio de no sobreingeniería.

**¿Por qué mappers manuales y no AutoMapper?**
Control explícito, sin dependencia adicional, y en entrevistas puedo explicar exactamente qué se mapea y por qué.

**¿Por qué Serilog y no el logger por defecto?**
Logging estructurado, configuración declarativa en `appsettings.json`, request logging integrado, y preparación para sinks adicionales (Application Insights, Seq).

**¿Cómo manejas errores?**
Excepciones de dominio (`NotFoundException`, `ConflictException`, etc.) capturadas por `ExceptionMiddleware` y convertidas a `ProblemDetails` con el status code HTTP correcto. Errores no controlados devuelven 500 sin exponer detalles internos.

**¿El Dockerfile sigue buenas prácticas?**
Sí: multi-stage build, imagen base `aspnet` (no SDK) en runtime, usuario no root (`$APP_UID`), solo `curl` para health check, restore con cache de capas separado del código fuente.
