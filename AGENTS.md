# Contexto Raíz — TecNM SSPP

Punto de entrada maestro para Agentes de Inteligencia Artificial en el proyecto **TecNM SSPP**.

---

## 🎯 Regla Fundamental sobre Base de Datos Legacy

> [!CAUTION]
> **`database/legacy/` es EXCLUSIVAMENTE para consulta y referencia conceptual.**
> - Contiene scripts del sistema anterior SET_MX (SQL Server).
> - **NUNCA ejecutar** estos scripts contra PostgreSQL ni intentar migraciones directas 1:1.
> - El modelo relacional del proyecto se construye desde cero en [`database/design/`](./database/design/).

---

## 🧭 Mapa de Navegación del Proyecto

| Dominio | Documento Maestro | Descripción |
| :--- | :--- | :--- |
| **Backend** | [`backend/AGENTS.md`](./backend/AGENTS.md) | Reglas .NET 10, vertical slices, convenciones C#, DbContext y comandos de API. |
| **Frontend** | [`frontend/AGENTS.md`](./frontend/AGENTS.md) | Reglas Vue 3 + TypeScript, diseño visual, composición de UI, router y API Axios. |
| **Documentación** | [`docs/`](./docs/) | Arquitecturas ([AUTH](./docs/architecture/AUTH.md), [Backend](./docs/architecture/BACKEND_ARCHITECTURE.md), [Frontend](./docs/architecture/FRONTEND_ARCHITECTURE.md)), [specs canónicos](./docs/specs/), [planes](./docs/plans/) y [análisis legacy](./docs/legacy-db/). |
| **Base de Datos** | [`database/README.md`](./database/README.md) | [Borrador de diseño](./database/design/README.md), [seed demo](./database/seed/01_dev_users.sql) y [legacy histórico](./database/legacy/). |

---

## 📋 Reglas Operativas Generales

1. **Spec-Driven Development (SDD):** Todo cambio funcional debe actualizar primero su especificación canónica en `docs/specs/` o la especificación local del módulo antes de tocar código.
2. **Concisión (Skill `caveman`):** Mantener respuestas y documentación técnica concisas, directas y sin explicaciones superfluas.
3. **Persistencia y Esquemas:** Toda tabla diseñada debe usar PostgreSQL, nomenclatura `snake_case`, PK explícita y FKs con integridad referencial.
