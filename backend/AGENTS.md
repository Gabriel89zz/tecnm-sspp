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

## Mapa

- [`Core/Entities/`](./Core/Entities/): entidades compartidas; `User` será entidad inicial.
- [`Data/`](./Data/): `TecNMDbContext` y mapeo PostgreSQL.
- [`Modules/Auth/`](./Modules/Auth/): controller, service y DTOs del slice.
- [`Modules/Auth/specs/`](./Modules/Auth/specs/): contexto backend de Auth.
- [Contrato raíz](../docs/specs/auth.spec.md): única fuente de verdad compartida.
