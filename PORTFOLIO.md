# Church API — Resumen para Portafolio

## Problema que resuelve

Las organizaciones religiosas y comunitarias necesitan un sistema centralizado para registrar miembros y llevar control de sus donaciones. Church API ofrece una solución backend que permite gestionar esta información de forma segura, con control de acceso por roles y trazabilidad de operaciones.

## Tecnologías

| Categoría | Stack |
|---|---|
| Backend | ASP.NET Core 8, C# 12 |
| Persistencia | Entity Framework Core 8, SQL Server |
| Autenticación | JWT Bearer, ASP.NET Core Identity PasswordHasher |
| Logging | Serilog |
| Contenedores | Docker, Docker Compose |
| Cloud | Azure Container Apps, Azure SQL Database, Azure Container Registry |
| Testing | xUnit, Moq, FluentAssertions, WebApplicationFactory |
| CI/CD | GitHub Actions |

## Arquitectura

API REST con arquitectura en capas (Controllers → Services → DbContext) dentro de un único proyecto, priorizando simplicidad y mantenibilidad sobre complejidad innecesaria.

- **Controllers**: contratos HTTP y autorización
- **Services**: lógica de negocio y manejo de excepciones de dominio
- **DTOs + Mappers**: desacoplamiento entre API y modelo de datos
- **Middleware**: respuestas de error consistentes con RFC 7807
- **Extension Methods**: configuración modular y `Program.cs` limpio

## Principales retos

1. **Manejo de concurrencia en registro de usuarios**: validación previa combinada con captura de violaciones de constraint único en base de datos para evitar condiciones de carrera.

2. **Traducción de excepciones de persistencia**: abstracción que mapea errores de SQL Server y SQLite a excepciones de dominio (`ConflictException`, `NotFoundException`), permitiendo tests de integración con SQLite y producción con SQL Server.

3. **Configuración segura multi-entorno**: secretos fuera del repositorio mediante user-secrets, variables de entorno y preparación para Azure Key Vault.

4. **Tests de integración sin dependencia de SQL Server**: `WebApplicationFactory` con SQLite in-memory y seed de datos de prueba.

5. **Contenedorización production-ready**: Dockerfile multi-stage, usuario no root, health checks y migraciones automáticas configurables.

## Qué aprendí

- Diseño de APIs REST con convenciones HTTP correctas (`201 Created`, `204 No Content`, `ProblemDetails`)
- Implementación de JWT con validación completa de tokens
- Configuración de EF Core con constraints, índices y migraciones
- Logging estructurado con Serilog en aplicaciones ASP.NET Core
- Estrategias de testing: unit tests con mocks + integration tests con pipeline HTTP completo
- Docker multi-stage builds y orquestación con Docker Compose
- Preparación de aplicaciones .NET para despliegue en Azure Container Apps

## Resultado final

API funcional con 46 tests automatizados, pipeline CI, documentación Swagger, health checks, logging estructurado y despliegue containerizado. Lista para demostrar en entrevistas técnicas como evidencia de competencias en backend .NET, buenas prácticas y pensamiento orientado a producción.

**Repositorio:** [github.com/DervisGomez/appi-miembros](https://github.com/DervisGomez/appi-miembros)
