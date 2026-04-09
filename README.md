# Sistema de Gestión de Reservas de Hoteles — SGRH

> Solución empresarial para la administración integral de reservas hoteleras, construida sobre **.NET 8** con arquitectura en capas (monolito modular), API REST centralizada y clientes desacoplados Web y Desktop.
>
> More information: https://www.mintlify.com/Muryokusho0/SGRH

---

## Tabla de contenidos

1. Descripción general
2. Arquitectura
3. Estructura de la solución
4. Capas del sistema
5. Capa de presentación
6. Endpoints de la API
7. Roles y autorización
8. Reglas de negocio clave
9. Tecnologías utilizadas
10. Configuración del entorno
11. Ejecución del proyecto

---

## Descripción general

El SGRH permite gestionar de extremo a extremo el ciclo de vida de las reservas hoteleras:

- **Clientes** — registro y gestión de huéspedes
- **Reservas** — creación, modificación, confirmación, cancelación y finalización
- **Habitaciones** — estados, historial y disponibilidad por temporada
- **Categorías y tarifas** — precios diferenciados por categoría y temporada
- **Servicios adicionales** — servicios vinculados a habitaciones y temporadas
- **Temporadas** — períodos tarifarios con inicio y fin definidos
- **Usuarios** — gestión de acceso por roles
- **Reportes** — indicadores operativos y financieros
- **Auditoría** — trazabilidad completa de todas las operaciones

La lógica de negocio reside íntegramente en `SGRH.Domain`; los clientes (Web y Desktop) se comunican exclusivamente a través de la API REST y nunca contienen reglas de dominio.

---

## Arquitectura

```
┌─────────────────────────────────────────────────────────────────┐
│                        Clientes                                 │
│   ┌────────────────────┐        ┌────────────────────────────┐  │
│   │    SGRH.Web        │        │       Desktop (MAUI)       │  │
│   │  Blazor Server     │        │   Blazor Hybrid (.NET 8)   │  │
│   │  IHttpClientFactory│        │   IHttpClientFactory       │  │
│   └─────────┬──────────┘        └──────────────┬─────────────┘  │
└─────────────┼─────────────────────────────────┼────────────────┘
              │          HTTP / JWT              │
              ▼                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                       SGRH.Api                                  │
│         Controllers · Middleware · Swagger · JWT Auth           │
└──────────────────────────────┬──────────────────────────────────┘
                               │
              ┌────────────────┼────────────────┐
              ▼                ▼                ▼
┌─────────────────┐  ┌──────────────┐  ┌──────────────────┐
│ SGRH.Application│  │  SGRH.Auth   │  │SGRH.Infrastructure│
│  UseCases · DTOs│  │ JWT · BCrypt │  │ EF Core · S3 · SES│
│  Validators     │  │ Policies     │  │ Repositorios      │
└────────┬────────┘  └──────────────┘  └────────┬─────────┘
         │                                       │
         └──────────────┬────────────────────────┘
                        ▼
              ┌─────────────────┐
              │   SGRH.Domain   │
              │ Entities·Enums  │
              │ Guard·Policies  │
              │ Interfaces      │
              └─────────────────┘
                        │
                        ▼
              ┌─────────────────┐
              │ SGRH.Persistence│
              │ DbContext · EF  │
              │ Repositories    │
              │ UnitOfWork      │
              └────────┬────────┘
                       │
                       ▼
              ┌─────────────────┐
              │  SQL Server     │
              │  (AWS RDS)      │
              └─────────────────┘
```

**Principios que guían la arquitectura:**

- El dominio **no depende de ninguna otra capa**; las interfaces en `SGRH.Domain` invierten las dependencias.
- La capa de aplicación orquesta casos de uso **sin acceso directo a la base de datos**.
- Los clientes **no contienen lógica de negocio**; todo se delega a la API.
- `IHttpClientFactory` gestiona el ciclo de vida de los `HttpClient` en la capa de presentación, evitando el agotamiento de sockets (socket exhaustion).

---

## Estructura de la solución

```
SGRH/
├── SGRH.Api/                        # Punto de entrada REST
│   ├── Controllers/                 # Controladores por agregado
│   ├── Configuration/               # Políticas de autorización
│   ├── Converters/                  # DateTimeLocalConverter
│   ├── Middlewares/                 # ExceptionHandlingMiddleware
│   └── Seed/                        # DbSeeder (admin inicial)
│
├── SGRH.Application/                # Casos de uso y DTOs
│   ├── UseCases/                    # Un directorio por feature
│   ├── Dtos/                        # Contratos de entrada/salida
│   ├── Mappers/                     # Entidad → DTO
│   └── Abstractions/                # IValidator, ValidacionResultado
│
├── SGRH.Domain/                     # Núcleo del negocio (sin dependencias)
│   ├── Entities/                    # Agregados y entidades
│   ├── Enums/                       # EstadoReserva, RolUsuario, etc.
│   ├── Exceptions/                  # Excepciones de dominio tipadas
│   ├── Abstractions/                # Interfaces de repositorios, servicios, email, storage
│   ├── Base/                        # EntityBase
│   └── Common/                      # Guard, HoraLocal
│
├── SGRH.Infrastructure/             # Implementaciones externas
│   ├── EmailSES/                    # Amazon SES (v2)
│   ├── StorageS3/                   # Amazon S3
│   └── Services/                    # ReservaDomainPolicy, SystemClock, AuditoriaService
│
├── SGRH.Persistence/                # Acceso a datos con EF Core
│   ├── Context/                     # SgrhDbContext
│   ├── Configurations/              # Fluent API por entidad
│   ├── Repositories/                # Implementaciones EF de los repositorios
│   ├── UnitOfWork/                  # Patrón Unit of Work
│   └── Queries/                     # ReportesQueryService, ReservaQueries
│
├── SGRH.Auth/                       # Autenticación y seguridad
│   ├── Jwt/                         # JwtTokenGenerator, JwtOptions
│   └── Hashing/                     # BcryptPasswordHasher
│
├── SGRH.Web/                        # Cliente web (Blazor Server)
│   └── SGRH.Web/
│       ├── Components/              # Pages, Layout, _Imports
│       ├── Services/                # Clientes HTTP tipados (IHttpClientFactory)
│       ├── Models/                  # ViewModels de presentación
│       └── wwwroot/                 # Recursos estáticos
│
└── Desktop/                         # Cliente desktop (.NET MAUI Blazor Hybrid)
    ├── Components/                  # Pages y Layout
    ├── Platforms/                   # Android, Windows, iOS, macOS
    └── Resources/                   # Fuentes, iconos, splash
```

---

## Capas del sistema

### Domain
Núcleo de la solución; **no referencia ningún otro proyecto**.

- Define entidades (`Reserva`, `Habitacion`, `Cliente`, `ServicioAdicional`, `Temporada`, …) que extienden `EntityBase`.
- Aplica invariantes mediante la clase estática `Guard`.
- Abstrae tiempo a través de `HoraLocal.Ahora` (nunca `DateTime.Now` directamente).
- Declara interfaces de repositorios, políticas de dominio (`IReservaDomainPolicy`), servicios de tiempo (`ISystemClock`), email y almacenamiento.
- Expone excepciones tipadas: `BusinessRuleViolationException`, `NotFoundException`, `ConflictException`, `ValidationException`.

### Application
Orquesta los casos de uso del negocio.

- Cada feature tiene su propio directorio con `Request`, `Response`, `UseCase` y `Validator`.
- Depende únicamente de `SGRH.Domain`.
- Traduce entidades a DTOs mediante mappers manuales; no usa AutoMapper.
- **No accede directamente a la base de datos** ni a SDKs externos.

### Persistence
Implementa la persistencia con **Entity Framework Core + SQL Server**.

- `SgrhDbContext` centraliza el contexto de EF.
- Las configuraciones Fluent API están en `Configurations/` (una clase por entidad).
- `UnitOfWork` coordina las transacciones entre repositorios.
- `ReportesQueryService` ejecuta consultas de lectura optimizadas.

### Infrastructure
Implementa los contratos de dominio que requieren tecnologías externas.

- **Amazon S3** — almacenamiento de archivos (`S3FileStorage`).
- **Amazon SES v2** — envío de correos (`SesEmailSender`) y notificaciones a administradores (`SesAdminNotifier`).
- **`ReservaDomainPolicy`** — valida disponibilidad de habitaciones y servicios.
- **`SystemClock`** — implementación real de `ISystemClock` que devuelve la hora local del servidor.

### Auth
Módulo de seguridad desacoplado.

- `JwtTokenGenerator` genera tokens firmados con las opciones configuradas.
- `BcryptPasswordHasher` implementa `IPasswordHasher`.
- Extensión `AddAuth()` registra la autenticación JWT Bearer y el hashing.
- Extensión `AddAuthorizationPolicies()` registra las políticas: `SoloAdmin`, `SoloCliente`, `AdminORecepcionista`, `Autenticado`.

### Api
Punto de entrada HTTP de la solución.

- Controladores por agregado (Reservas, Habitaciones, Clientes, Servicios, Tarifas, Temporadas, Categorías, Usuarios, Auth, Auditoría, Me, Reportes).
- `ExceptionHandlingMiddleware` captura excepciones de dominio y las convierte en respuestas Problem Details.
- `DateTimeLocalConverter` serializa `DateTime` sin información de zona horaria.
- Swagger configurado con soporte Bearer para pruebas autenticadas.
- `DbSeeder` crea el primer usuario administrador si no existe ninguno.

---

## Web

La capa de presentación es un proyecto **Blazor Server** (`SGRH.Web`) que consume la API REST. Sigue los lineamientos de arquitectura lógica de presentación definidos en el proyecto:

```
Usuario
   ↓
Components (Pages / Layout)
   ↓
Servicios de consumo de API   ←  IHttpClientFactory + HttpClient
   ↓
SGRH.Api (REST / JSON)
```

### Estructura interna esperada de SGRH.Web

```
SGRH.Web/
├── Components/
│   ├── Pages/          # Páginas Razor (.razor) — orquestan la UI
│   ├── Layout/         # MainLayout, NavMenu
│   └── Shared/         # Componentes reutilizables
├── Services/           # Servicios HTTP tipados (IHttpClientFactory)
│   ├── IReservaService.cs
│   ├── ReservaService.cs
│   ├── IHabitacionService.cs
│   └── ...
├── Models/             # ViewModels de presentación
│   ├── ReservaViewModel.cs
│   ├── HabitacionViewModel.cs
│   └── ...
├── Helpers/            # Utilidades (formatos, conversiones de estado, etc.)
└── Program.cs          # Registro de IHttpClientFactory y servicios
```

---

## Endpoints de la API

La API base está bajo `/api`. Todos los endpoints requieren autenticación JWT salvo `/api/auth/login`.

### Auth
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/auth/login` | Obtiene token JWT |
| POST | `/api/auth/register` | Registra nuevo usuario (Admin) |

### Reservas
| Método | Ruta | Roles |
|--------|------|-------|
| GET | `/api/reservas` | Todos (Cliente ve solo las suyas) |
| GET | `/api/reservas/{id}` | Todos |
| POST | `/api/reservas` | Todos |
| PATCH | `/api/reservas/{id}/confirmar` | Todos |
| PATCH | `/api/reservas/{id}/cancelar` | Todos |
| PATCH | `/api/reservas/{id}/fechas` | Todos |
| POST | `/api/reservas/{id}/habitaciones` | Todos |
| DELETE | `/api/reservas/{id}/habitaciones/{habitacionId}` | Todos |
| POST | `/api/reservas/{id}/servicios` | Todos |
| DELETE | `/api/reservas/{id}/servicios/{servicioId}` | Todos |

### Habitaciones
| Método | Ruta | Roles |
|--------|------|-------|
| GET | `/api/habitaciones` | Admin, Recepcionista |
| GET | `/api/habitaciones/disponibles` | Todos |
| GET | `/api/habitaciones/{id}` | Admin, Recepcionista |
| GET | `/api/habitaciones/{id}/ocupacion` | Todos |
| POST | `/api/habitaciones` | Admin |
| PATCH | `/api/habitaciones/{id}/bloquear` | Admin, Recepcionista |
| PATCH | `/api/habitaciones/{id}/estado` | Admin, Recepcionista |

### Clientes, Usuarios, Categorías, Servicios, Tarifas, Temporadas, Reportes, Auditoría
Disponibles bajo `/api/{recurso}` con métodos GET, POST y PATCH según el recurso. Consulta el Swagger en `/swagger` para la especificación completa.

---

## Roles y autorización

| Rol | Descripción | Acceso |
|-----|-------------|--------|
| `ADMIN` | Administrador del sistema | Acceso completo: usuarios, habitaciones, configuración global, reportes |
| `RECEPCIONISTA` | Personal de recepción | Reservas, habitaciones, servicios, reportes operativos |
| `CLIENTE` | Huésped del hotel | Sus propias reservas exclusivamente |

Las políticas configuradas son:

- `SoloAdmin` — requiere rol `ADMIN`
- `SoloCliente` — requiere rol `CLIENTE`
- `AdminORecepcionista` — requiere `ADMIN` o `RECEPCIONISTA`
- `Autenticado` — cualquier usuario con token válido

---

## Reglas de negocio clave

### Reservas
- Solo se puede modificar una reserva en estado **Pendiente**.
- Al **confirmar** se requiere al menos una habitación.
- Al **agregar habitaciones** se verifica disponibilidad (no ocupada ni en mantenimiento) y se calcula la tarifa de temporada aplicable.
- Al **agregar servicios** debe existir al menos una habitación; no se permiten duplicados; el servicio debe estar disponible en la temporada activa.
- Una reserva **Finalizada** no puede cancelarse ni modificarse.

### Habitaciones
- Cada habitación mantiene un **historial de estados**; al cambiar de estado se cierra el registro vigente y se abre uno nuevo.
- No se permite registrar el **mismo estado consecutivamente**.
- El primer registro de estado lo genera un **trigger en la base de datos**.

### Servicios adicionales
- Un servicio puede aplicar a **todas las temporadas** o solo a temporadas específicas.
- La disponibilidad se evalúa: *aplica a todas* ∨ *no hay temporada activa* ∨ *está asociado a la temporada solicitada*.

### Clientes
- Campos obligatorios validados con longitudes máximas: número de identidad y teléfono (20 car.), nombres y correo (100 car.).

---

## Tecnologías utilizadas

### Backend
| Tecnología | Uso |
|------------|-----|
| .NET 8 | Framework base |
| ASP.NET Core Web API | API REST |
| Entity Framework Core 8 | ORM |
| SQL Server (AWS RDS) | Base de datos |
| BCrypt.Net | Hash de contraseñas |
| JWT Bearer | Autenticación |
| Swashbuckle / Swagger | Documentación de API |

### Cloud / Infraestructura
| Servicio | Uso |
|----------|-----|
| Amazon S3 | Almacenamiento de archivos |
| Amazon SES v2 | Envío de correos electrónicos |
| AWS RDS SQL Server | Base de datos en la nube |

### Clientes
| Proyecto | Tecnología |
|----------|------------|
| SGRH.Web | Blazor Server (.NET 8) + IHttpClientFactory |
| Desktop | .NET MAUI Blazor Hybrid |

---

## Configuración del entorno

### Requisitos previos

- .NET SDK 8.0 o superior
- SQL Server (local o AWS RDS)
- Visual Studio 2022 17.8+ o Rider / VS Code
- Workload MAUI instalado (solo para el cliente Desktop)
- Acceso a credenciales AWS (S3 y SES) si se usan en desarrollo

---

## Diagrama de arquitectura lógica — Capa de presentación

```
┌─────────────────────────────────────────────────────┐
│                      Usuario                        │
└────────────────────────┬────────────────────────────┘
                         │ Interacción UI
                         ▼
┌─────────────────────────────────────────────────────┐
│          Components / Pages (.razor)                │
│   Orquestan la presentación y eventos de usuario    │
│   No contienen lógica de negocio                    │
└────────────────────────┬────────────────────────────┘
                         │ Invoca
                         ▼
┌─────────────────────────────────────────────────────┐
│        Servicios de consumo de API                  │
│   IReservaService, IHabitacionService, ...          │
│   Encapsulan HttpClient — NUNCA llamadas directas   │
│   desde componentes                                 │
│   IHttpClientFactory gestiona el ciclo de vida      │
└────────────────────────┬────────────────────────────┘
                         │ HTTP + JWT
                         ▼
┌─────────────────────────────────────────────────────┐
│               SGRH.Api (REST)                       │
│   Procesa, valida y ejecuta la lógica de negocio    │
│   Retorna JSON (200 / 400 / 404 / 409 / 500)        │
└─────────────────────────────────────────────────────┘
```

---

*Desarrollado como proyecto académico universitario — Arquitectura de Software.*
