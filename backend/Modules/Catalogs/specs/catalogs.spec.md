# Catalogs backend

Contrato compartido: [spec raíz de catálogos](../../../../docs/specs/catalogs.spec.md).

Este archivo define responsabilidades backend para el módulo de catálogos base (`Careers`, `AcademicPeriods`, `SocialServicePrograms`, `InstitutionalAuthorities`, `DocumentSeries`).

## Responsabilidades por recurso

- **Carreras (`CareersController`, `CareersService`):**
  - CRUD con paginación (`search` por `code`/`name`, orden por `code`/`name`).
  - Validación de clave mediante regex `^[A-Z0-9_-]+$`.
  - Validación de unicidad de `code` antes de insertar/actualizar (responde `409 careers.duplicated_code`).
  - Toggle de estado con `PATCH /status`.
  - Borrado lógico y bloqueo ante referencias activas (`409 careers.in_use`).

- **Periodos Académicos (`AcademicPeriodsController`, `AcademicPeriodsService`):**
  - CRUD con paginación (`search`, filtro `isActive`, orden `-startDate`).
  - Validación de fechas: `startDate <= endDate`.
  - Activación atómica transaccional (`POST /{id}/activate`): desactiva periodos previos en la misma transacción (`R16`).
  - Consulta de periodo vigente (`GET /current` -> `404 academic_periods.no_current`).

- **Programas de Servicio Social (`SocialServiceProgramsController`, `SocialServiceProgramsService`):**
  - Categorías oficiales de 9 valores, flag `is_internal`.

- **Autoridades Institucionales (`InstitutionalAuthoritiesController`, `InstitutionalAuthoritiesService`):**
  - Una sola autoridad activa por `role_key`. Endpoint `/active` para generadores documentales.

- **Series de Folio (`DocumentSeriesController`, `DocumentSeriesService`):**
  - Incremento atómico transaccional de folios institucionales. Previene rollback de secuencias.

## Verificación

- Validar filtros de exclusión `DeletedAt == null`.
- Comprobar endpoints de `/options` para selectores de frontend.
