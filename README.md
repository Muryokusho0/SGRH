# Sistema de Gestión de Reservas de Hoteles (SGRH)

## Descripción General

El Sistema de Gestión de Reservas de Hoteles (SGRH) es una solución desarrollada bajo una arquitectura en capas (monolito modular) con un backend centralizado que expone una API REST consumida por clientes Web y Desktop.

El sistema permite gestionar:

- Clientes  
- Reservas  
- Habitaciones  
- Tarifas y temporadas  
- Servicios adicionales  
- Reportes  
- Auditoría y trazabilidad  

La arquitectura implementada sigue el Documento de Arquitectura (SAD), garantizando separación de responsabilidades, inversión de dependencias y desacoplamiento entre capas.

---

## Arquitectura

### Tipo de arquitectura

- Monolito modular  
- Arquitectura en capas  
- Backend centralizado con API REST  
- Clientes desacoplados (Web y Desktop)  
- Infraestructura implementando contratos del dominio  

---

## Estructura de la solución
```
SGRH
├── SGRH.Api
├── SGRH.Application
├── SGRH.Domain
├── SGRH.Infrastructure
├── SGRH.Auth
├── SGRH.Web
└── SGRH.Desktop
```
---

## Capas del sistema

### 1. SGRH.Domain

Contiene el núcleo del negocio:

- `Entities/`
- `Base/`
- `Contracts/`

Características:

- No depende de ningún otro proyecto.
- Define reglas de negocio y contratos (interfaces).
- Aplica inversión de dependencias.

---

### 2. SGRH.Application

Orquesta los casos de uso del sistema:

- `UseCases/`
- `DTOs/`
- Validaciones
- Lógica de aplicación

Depende únicamente de `SGRH.Domain`.

No contiene acceso directo a base de datos ni SDKs externos.

---

### 3. SGRH.Infrastructure

Implementa los contratos definidos en el dominio:

- Persistencia con Entity Framework Core
- Proveedor MySQL
- Integración con Amazon S3
- Integración con Amazon SES

Contiene:
```
Persistence/
StorageS3/
EmailSES/
DependencyInjection/
```

Depende de `SGRH.Domain`.

---

### 4. SGRH.Api

Punto de entrada único del sistema:

- Controladores REST
- Swagger/OpenAPI
- Configuración de seguridad
- Registro de dependencias

Depende de:

- `SGRH.Application`
- `SGRH.Infrastructure`
- `SGRH.Auth`

Expone endpoints REST consumidos por Web y Desktop.

---

### 5. SGRH.Auth

Módulo de autenticación y autorización:

- Generación de tokens (JWT)
- Policies
- Servicios de autenticación

---

### 6. SGRH.Web

Cliente web que consume la API REST.

- No contiene lógica de negocio
- Comunicación vía HTTP
- Interfaz basada en Blazor

---

### 7. SGRH.Desktop

Aplicación .NET MAUI Blazor Hybrid para entorno administrativo:

- Consume la API REST
- Organización por roles (Administrador / Recepcionista)
- No contiene lógica de negocio

---

## Tecnologías utilizadas

### Backend
- .NET 8
- ASP.NET Core Web API
- Swagger/OpenAPI
- Entity Framework Core
- MySQL

### Infraestructura / Cloud
- AWS SDK
- Amazon S3
- Amazon SES

### Clientes
- Blazor Web (Server)
- .NET MAUI Blazor Hybrid (Desktop)

---

## Seguridad

- Autenticación basada en JWT
- Autorización por roles
- Auditoría y trazabilidad
- Uso de HTTPS en desarrollo y producción

---

## Configuración del entorno

### 1. Requisitos

- .NET SDK 8
- MySQL Server
- Visual Studio 2022 o superior
- Workload MAUI (para Desktop)
