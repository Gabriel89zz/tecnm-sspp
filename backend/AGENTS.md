# Contexto backend

## Objetivo

Construir API .NET 10 modular. Primer slice: autenticación local con PostgreSQL, JWT institucional y cookies seguras.

## Lectura obligatoria

1. [Arquitectura backend](../docs/architecture/BACKEND_ARCHITECTURE.md)
2. [Estándar de autenticación](../docs/architecture/AUTH.md)
3. [Contrato canónico de autenticación](../docs/specs/auth.spec.md)
4. [Spec local de Auth](./Modules/Auth/specs/auth.spec.md)

## Reglas

- Aplicar SDD. Actualizar spec antes de código.
- Usar C# y .NET 10.
- Organizar por vertical slice.
- Mantener controllers delgados. Poner reglas, consultas y tokens en services.
- Usar EF Core async y PostgreSQL. No agregar Repository sin necesidad real.
- Exponer DTOs. Nunca exponer entidades EF.
- Leer JWT desde cookie `auth_token`.
- Mantener secretos y conexión fuera de Git.
- Usar variables `Jwt__SecretKey` y `ConnectionStrings__DefaultConnection`.
- Limitar paquetes directos a los requeridos por el plan.
- Cambiar contrato compartido primero cuando cambie API, cookie, DTO o error.

## Comandos previstos

```bash
dotnet user-secrets set --project backend/TecNM.Api.csproj "ConnectionStrings:DefaultConnection" "Host=127.0.0.1;Port=5432;Database=tecnm_template;Username=tecnm_app;Password=<local>"
dotnet user-secrets set --project backend/TecNM.Api.csproj "Jwt:SecretKey" "<mínimo 32 bytes UTF-8>"
dotnet restore backend/TecNM.Api.csproj
dotnet build backend/TecNM.Api.csproj --no-restore
dotnet run --project backend/TecNM.Api.csproj
```

Prueba de humo prevista: `curl` con cookie jar sobre HTTPS. Debe cubrir login inválido, login válido, `/me`, logout y `/me` posterior.

## Mapa de Módulos (Vertical Slices)

- [`Core/Entities/`](./Core/Entities/): entidades de dominio compartidas POCO (`AuditableEntity`, `User`, `Role`, `Career`, etc.).
- [`Core/Http/`](./Core/Http/): utilidades HTTP (`Paging.cs`, `GlobalExceptionHandler.cs`).
- [`Core/Security/`](./Core/Security/): `CurrentUser.cs` (extracción de claims JWT).
- [`Data/`](./Data/): `TecNMDbContext`, `AuditInterceptor.cs` y configuraciones desacopladas en [`Data/Configurations/`](./Data/Configurations/).
- [`Modules/Auth/`](./Modules/Auth/): autenticación JWT con cookies HttpOnly y spec en [`Modules/Auth/specs/`](./Modules/Auth/specs/auth.spec.md).
- [`Modules/Catalogs/`](./Modules/Catalogs/): carreras, periodos, programas SS, autoridades y folios con spec en [`Modules/Catalogs/specs/`](./Modules/Catalogs/specs/catalogs.spec.md).
- [`Modules/Users/`](./Modules/Users/): gestión de usuarios y roles con spec en [`Modules/Users/specs/`](./Modules/Users/specs/users.spec.md).
- [`Modules/Companies/`](./Modules/Companies/): empresas, supervisores y convenios con spec en [`Modules/Companies/specs/`](./Modules/Companies/specs/companies.spec.md).
- [`Modules/Students/`](./Modules/Students/): padrón, elegibilidad e importación con spec en [`Modules/Students/specs/`](./Modules/Students/specs/students.spec.md).
- [`Modules/Expedients/`](./Modules/Expedients/): expediente documental y almacenamiento con spec en [`Modules/Expedients/specs/`](./Modules/Expedients/specs/documents.spec.md).
- [`Modules/SocialService/`](./Modules/SocialService/): trámite de Servicio Social (Anexos XVIII–XXV) con spec en [`Modules/SocialService/specs/`](./Modules/SocialService/specs/social-service.spec.md).
- [`Modules/Internships/`](./Modules/Internships/): trámite de Prácticas Profesionales (Anexo XVII) con spec en [`Modules/Internships/specs/`](./Modules/Internships/specs/internships.spec.md).
- [`Modules/Quality/`](./Modules/Quality/): trazabilidad y Producto No Conforme (PNC) con spec en [`Modules/Quality/specs/`](./Modules/Quality/specs/quality.spec.md).
- [`Modules/Reports/`](./Modules/Reports/): indicadores institucionales y vistas SQL con spec en [`Modules/Reports/specs/`](./Modules/Reports/specs/reports.spec.md).

Contratos compartidos raíz: directorio [`docs/specs/`](../docs/specs/). Única fuente canónica de verdad.
