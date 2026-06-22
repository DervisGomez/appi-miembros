# ChurchApi

API REST para la gestión de miembros y donaciones de una iglesia. Desarrollada con **ASP.NET Core 8**, **Entity Framework Core** y autenticación **JWT**.

---

## Tabla de contenidos

- [Características](#características)
- [Stack tecnológico](#stack-tecnológico)
- [Arquitectura](#arquitectura)
- [Requisitos previos](#requisitos-previos)
- [Configuración](#configuración)
- [Puesta en marcha](#puesta-en-marcha)
- [Autenticación](#autenticación)
- [Endpoints](#endpoints)
- [Paginación y filtros](#paginación-y-filtros)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Migraciones de base de datos](#migraciones-de-base-de-datos)

---

## Características

- CRUD completo de **miembros** con datos de contacto.
- Gestión de **donaciones** asociadas a cada miembro.
- Listados **paginados** de miembros y donaciones con ordenamiento configurable.
- Filtros de donaciones por monto, miembro y rango de importes.
- **Registro e inicio de sesión** con tokens JWT.
- Roles de usuario (`User`, `Admin`) con endpoint protegido para promoción de administradores.
- Documentación interactiva con **Swagger UI** en entorno de desarrollo.

---

## Stack tecnológico

| Componente        | Tecnología                                      |
|-------------------|-------------------------------------------------|
| Framework         | ASP.NET Core 8                                  |
| ORM               | Entity Framework Core 8                         |
| Base de datos     | Microsoft SQL Server                            |
| Autenticación     | JWT (handler personalizado)                     |
| Hash de contraseñas | ASP.NET Core Identity `PasswordHasher`        |
| Documentación     | Swashbuckle (Swagger / OpenAPI)                 |

---

## Arquitectura

La aplicación sigue una arquitectura en capas:

```
Controllers  →  Services  →  Data (DbContext)  →  SQL Server
     ↓
   DTOs / Mappers
```

| Capa          | Responsabilidad                                      |
|---------------|------------------------------------------------------|
| `Controllers` | Recibir peticiones HTTP y devolver respuestas        |
| `Services`    | Lógica de negocio e interacción con la base de datos |
| `Dtos`        | Contratos de entrada y salida de la API              |
| `Mappers`     | Transformación entre modelos de dominio y DTOs       |
| `Models`      | Entidades de dominio                                 |
| `Data`        | Contexto de Entity Framework Core                    |
| `Authentication` | Validación de tokens JWT en cada petición         |

---

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server accesible localmente (o contenedor Docker)
- Variable de entorno o configuración de JWT (ver [Configuración](#configuración))

---

## Configuración

### Base de datos

Edita la cadena de conexión en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost,1433;Database=ChurchDB;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True;"
  }
}
```

> También existe soporte para SQLite comentado en `Program.cs` (`UseSqlite`).

### JWT

El secreto JWT se resuelve en este orden de prioridad:

1. Variable de entorno `JWT_SECRET` (recomendado en producción)
2. Clave `Jwt:Secret` en `appsettings.Development.json` (desarrollo local)

```json
{
  "Jwt": {
    "Secret": "ChurchApi-Dev-Secret-Key-Min32Chars!"
  }
}
```

En producción:

```bash
export JWT_SECRET="tu-clave-secreta-de-al-menos-32-caracteres"
```

Los tokens expiran **1 hora** después de su emisión.

---

## Puesta en marcha

```bash
# Clonar e ingresar al proyecto
cd ChurchApi

# Restaurar dependencias
dotnet restore

# Aplicar migraciones a la base de datos
dotnet ef database update

# Ejecutar la aplicación
dotnet run
```

La API queda disponible en:

| Entorno   | URL                                      |
|-----------|------------------------------------------|
| HTTP      | `http://localhost:5101`                  |
| HTTPS     | `https://localhost:7231`                 |
| Swagger   | `http://localhost:5101/swagger`          |

---

## Autenticación

### Flujo

1. **Registrar** un usuario → `POST /api/auth/register`
2. **Iniciar sesión** → `POST /api/auth/login` → recibir `{ "token": "..." }`
3. Incluir el token en peticiones protegidas:

```
Authorization: Bearer {token}
```

### Roles

| Rol     | Descripción                                              |
|---------|----------------------------------------------------------|
| `User`  | Rol asignado por defecto al registrarse                  |
| `Admin` | Acceso a endpoints con `[Authorize(Roles = "Admin")]`   |

### Endpoints protegidos

| Método  | Ruta                          | Rol requerido |
|---------|-------------------------------|---------------|
| `PATCH` | `/api/auth/{userId}/promote`  | `Admin`       |

> El resto de endpoints son públicos en la versión actual.

---

## Endpoints

### Auth — `/api/auth`

| Método  | Ruta                    | Descripción                          | Auth  |
|---------|-------------------------|--------------------------------------|-------|
| `POST`  | `/register`             | Registrar nuevo usuario              | No    |
| `POST`  | `/login`                | Iniciar sesión y obtener JWT         | No    |
| `PATCH` | `/{userId}/promote`     | Promover usuario a administrador     | Admin |

**Registro — cuerpo de ejemplo:**

```json
{
  "username": "jdoe",
  "email": "jdoe@example.com",
  "password": "password123"
}
```

**Login — cuerpo de ejemplo:**

```json
{
  "username": "jdoe",
  "password": "password123"
}
```

**Respuesta de login:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

### Members — `/api/members`

| Método   | Ruta                      | Descripción                              |
|----------|---------------------------|------------------------------------------|
| `GET`    | `/`                       | Listar miembros (paginado)               |
| `GET`    | `/{id}`                   | Obtener miembro por ID (con donaciones)  |
| `POST`   | `/`                       | Crear miembro                            |
| `PUT`    | `/`                       | Actualizar miembro                       |
| `DELETE` | `/{id}`                   | Eliminar miembro                         |
| `GET`    | `/{memberId}/donations`   | Donaciones de un miembro                 |
| `POST`   | `/{memberId}/donations`   | Registrar donación para un miembro       |

**Crear miembro — cuerpo de ejemplo:**

```json
{
  "name": "Juan",
  "lastName": "Pérez",
  "email": "juan@example.com",
  "phone": "3001234567",
  "age": 35
}
```

---

### Donations — `/api/donations`

| Método   | Ruta       | Descripción                              |
|----------|------------|------------------------------------------|
| `GET`    | `/`        | Listar donaciones (paginado y filtrable) |
| `DELETE` | `/{id}`    | Eliminar donación                        |

**Crear donación** (vía miembro):

```
POST /api/members/{memberId}/donations
```

```json
{
  "amount": 150.00,
  "description": "Ofrenda dominical"
}
```

---

## Paginación y filtros

### Miembros — `GET /api/members`

| Parámetro    | Tipo       | Default | Descripción                          |
|--------------|------------|---------|--------------------------------------|
| `page`       | `int`      | `1`     | Número de página                     |
| `pageSize`   | `int`      | `10`    | Elementos por página (máx. 100)      |
| `sortOrder`  | `Asc/Desc` | `Asc`   | Orden por nombre y apellido          |

**Respuesta paginada:**

```json
{
  "items": [ ... ],
  "page": 1,
  "pageSize": 10,
  "totalItems": 25,
  "totalPages": 3
}
```

### Donaciones — `GET /api/donations`

| Parámetro    | Tipo       | Default | Descripción                          |
|--------------|------------|---------|--------------------------------------|
| `page`       | `int`      | `1`     | Número de página                     |
| `pageSize`   | `int`      | `10`    | Elementos por página (máx. 100)      |
| `sortOrder`  | `Asc/Desc` | `Desc`  | Orden por fecha                      |
| `memberId`   | `int?`     | —       | Filtrar por miembro                  |
| `minAmount`  | `decimal?` | —       | Monto mínimo                         |
| `maxAmount`  | `decimal?` | —       | Monto máximo                         |

---

## Estructura del proyecto

```
ChurchApi/
├── Authentication/       # Handler y opciones JWT
├── Controllers/          # AuthController, MembersController, DonationsController
├── Data/                 # AppDbContext
├── Dtos/                 # Objetos de transferencia de datos
├── Enums/                # UserRole, SortOrder
├── Extensions/           # Configuración de servicios (JWT)
├── Helpers/              # AuthPasswordHasher
├── Mappers/              # MemberMapper, DonationMapper
├── Migrations/           # Migraciones EF Core
├── Models/               # Member, Donation, User
├── Services/             # Lógica de negocio
├── Program.cs            # Punto de entrada y DI
└── appsettings.json      # Configuración
```

---

## Migraciones de base de datos

```bash
# Aplicar migraciones pendientes
dotnet ef database update

# Crear una nueva migración (tras cambiar modelos)
dotnet ef migrations add NombreDeLaMigracion

# Revertir a una migración anterior
dotnet ef database update NombreMigracionAnterior
```

### Entidades principales

| Entidad    | Campos clave                                              |
|------------|-----------------------------------------------------------|
| `Member`   | Name, LastName, Email, Phone, Age                         |
| `Donation` | Amount, Date, Description, MemberId                       |
| `User`     | Username, Email, PasswordHash, Role                       |

---

## Licencia

Proyecto de aprendizaje — uso libre con fines educativos.
