# Roadmap Backend — Progresión de Desarrollo

> **Objetivo:** Guía ejecutiva de avance fase por fase para la construcción del backend .NET 10 y su integración con PostgreSQL y Frontend.
> **Convenciones y contratos:** Ver [`docs/specs/api-conventions.spec.md`](../specs/api-conventions.spec.md).

---

## 🧭 Estado General del Proyecto

| Fase | Módulo / Componente | Entregables Clave | Estado |
| :---: | :--- | :--- | :---: |
| **0** | **Infraestructura & Auth** | Docker Compose (3 servicios), PostgreSQL 16 (24 tablas), Auth JWT con cookies HttpOnly. | **100% Hecho** |
| **1** | **Fundaciones & Catálogos Base** | `AuditableEntity`, Soft Delete global, RFC 7807 ProblemDetails, Carreras y Periodos. | **100% Hecho** |
| **2** | **Catálogos Restantes & Folios** | Programas SS, Autoridades Institucionales, Series de Folio atómicas. | **100% Hecho** |
| **3** | **Empresas & Convenios** | Directorio de empresas, supervisores externos y convenios de colaboración. | **Siguiente** |
| **4** | **Estudiantes & Padrón** | Expediente de alumno, validación IMSS, importador Excel masivo. | Pendiente |
| **5** | **Expediente Documental** | Almacenamiento local, subida de PDFs, estados de revisión y versionado. | Pendiente |
| **6** | **Servicio Social (SS)** | Trámite completo (480 h), Anexos XVIII–XXV, 3 reportes bimestrales y liberación. | Pendiente |
| **7** | **Prácticas Profesionales (PP)** | Trámite completo (200–480 h), evaluación Anexo XVII y acreditación. | Pendiente |
| **8** | **Calidad & Reportes** | Trazabilidad de estados, Producto No Conforme (PNC ISO 9001) y Formato 911.10. | Pendiente |

---

## 📋 Progresión Detallada por Fases

### Fase 0: Infraestructura & Autenticación
- [x] Docker Compose configurado (`tecnm_postgres`, `tecnm_backend`, `tecnm_frontend`).
- [x] Base de datos PostgreSQL con las 24 tablas de [`schema.sql`](../../database/design/schema.sql).
- [x] Semilla oficial de usuarios y roles cargada ([`01_dev_users.sql`](../../database/seed/01_dev_users.sql)).
- [x] Login JWT con cookies seguras `auth_token` (`HttpOnly`, `SameSite=Lax`) y endpoints `/login`, `/me`, `/logout`.

### Fase 1: Fundaciones y Primeros Catálogos
- [x] Entidad base `AuditableEntity` (6 campos) e interceptor `AuditInterceptor` para soft delete automático.
- [x] Manejo global de excepciones (`GlobalExceptionHandler`) con RFC 7807 y códigos PostgreSQL (`23505`, `23503`).
- [x] Utilidades de paginación transversal `Paging.cs` (`PagedRequest`, `PagedResponse<T>`, `OptionItem`).
- [x] Catálogo **Carreras** (`/api/careers`): CRUD, regex de clave, toggle de estado y `/options`.
- [x] Catálogo **Periodos Académicos** (`/api/academic-periods`): CRUD, activación transaccional única y `/current`.

### Fase 2: Catálogos Restantes & Motor de Folios
- [x] **Programas de Servicio Social** (`/api/social-service-programs`): 9 categorías oficiales, flag `is_internal`.
- [x] **Autoridades Institucionales** (`/api/institutional-authorities`): cargos oficiales, regla de una autoridad activa por puesto y endpoint `/active` para generador de constancias.
- [x] **Series de Folio** (`/api/document-series`): incremento atómico (`SELECT FOR UPDATE`), prevención de rollback y vista previa de folios con tokens (`{PREFIX}{SEQ:4}/{DD}/{MM}/{YYYY}`).
- [x] **Semilla oficial de catálogos** ([`database/seed/02_dev_catalogs.sql`](../../database/seed/02_dev_catalogs.sql)) con datos reales del ITSM (6 carreras, 3 periodos, 9 programas normativos, 4 autoridades, 3 series de folio).

### Fase 3: Empresas, Supervisores y Convenios
- [ ] Módulo **Empresas** (`/api/companies`): sectores (público, privado, social, educativo), tamaños y receptores TecNM internos.
- [ ] Módulo **Supervisores** (`/api/companies/{id}/supervisors`): vinculados a su empresa receptora.
- [ ] Módulo **Convenios** (`/api/agreements`): tipos de convenio, control de vigencia y fechas de inicio/fin.
- [ ] Regla de integridad: supervisor y convenio del trámite deben pertenecer a la misma empresa.

### Fase 4: Estudiantes, IMSS e Importación Masiva
- [ ] Módulo **Estudiantes** (`/api/students`): expediente completo, número de control, CURP y semestre.
- [ ] Cálculo de avance curricular (`credits / career.totalCredits * 100`).
- [ ] Validación médica de vigencia IMSS (`imss_validated_at`, `imss_validated_by`).
- [ ] Asistente de importación masiva por Excel (`/api/students/import-batches`) con reporte de filas con error.

### Fase 5: Expediente Documental & Archivos
- [ ] Servicio de almacenamiento físico local en volumen Docker (`IFileStorage`).
- [ ] Subida y descarga de archivos PDF (`/api/documents`).
- [ ] Flujo de revisión: `approve`, `reject` con observaciones.
- [ ] Versionado automático `superseded` al re-subir documentos observados.

### Fase 6: Servicio Social Operativo (Anexos XVIII–XXV)
- [ ] Máquina de estados: `requested` → `approved` → `accepted` → `active` → `completed_pending_review` → `released`.
- [ ] Solicitud y Carta Compromiso (Anexos XVIII y XIX).
- [ ] Tres reportes bimestrales secuenciales con acumulación de horas verificada (mínimo 480 h).
- [ ] Evaluaciones bimestrales y final (Anexo XXIII externo y XXIV autoevaluación).
- [ ] Liberación con asignación atómica de folio institucional.

### Fase 7: Prácticas Profesionales (Anexo XVII)
- [ ] Solicitud y registro de horas (200 a 480 horas, 2 créditos).
- [ ] Evaluación integral Anexo XVII (10 criterios evaluados, cálculo de promedio y nivel de desempeño).
- [ ] Acreditación y folio oficial de constancia.

### Fase 8: Calidad ISO 9001 & Reportes Institucionales
- [ ] Bitácora append-only de cambios de estado (`tramite_status_logs`).
- [ ] Módulo de Producto No Conforme (`/api/non-conformances`): registro, tratamiento y cierre.
- [ ] Vistas de base de datos (`CREATE VIEW`) para reportes estadísticos y formato oficial SEP/TecNM 911.10.
