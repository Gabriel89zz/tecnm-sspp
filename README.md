# TecNM SSPP

<!-- TODO: Completar la descripción del sistema TecNM SSPP (objetivo general, módulos funcionales y usuarios del sistema). -->
> **TODO:** Describir aquí el propósito específico del sistema TecNM SSPP, alcance del proyecto y objetivos institucionales.

---

## 📁 Estructura del Proyecto

```text
tecnm-sspp/
├── README.md                 # Documentación principal del repositorio
├── AGENTS.md                 # Índice y contexto raíz para Agentes de IA
├── .gitignore                # Reglas de exclusión de Git
├── backend/                  # API REST modular en .NET 10 + EF Core + PostgreSQL
├── frontend/                 # SPA en Vue 3 + TypeScript + Vite
├── database/
│   ├── README.md             # Guía y explicación de subdirectorios de base de datos
│   ├── design/               # Borrador activo del nuevo esquema relacional (README.md, schema.sql)
│   ├── seed/                 # Usuarios y datos demo para desarrollo local (01_dev_users.sql)
│   └── legacy/               # Referencia histórica de SET_MX (SQL Server) — SOLO CONSULTA
├── docs/
│   ├── architecture/         # Estándares de arquitectura (AUTH.md, BACKEND, FRONTEND)
│   ├── legacy-db/            # Documentación y visores del esquema previo (85 tablas)
│   ├── specs/                # Especificaciones funcionales y contratos canónicos
│   └── plans/                # Planes de implementación técnica y fases
└── scripts/                  # Scripts de utilería y pruebas de humo
```

---

## 🤖 Desarrollo Guiado por Especificaciones (Spec-Driven Development - SDD)

El trabajo y colaboración con **Agentes de Inteligencia Artificial** en este ecosistema se rige estrictamente bajo **SDD (Spec-Driven Development)**:

1. **Especificación Primero:** Antes de generar o modificar código en cualquier módulo, se deben redactar o actualizar los documentos en su carpeta local `specs/` o en `docs/specs/`.
2. **Contexto Aislado:** Las especificaciones funcionales, casos de uso, contratos y criterios de aceptación residen dentro del propio módulo/slice para dar contexto preciso a los agentes.
3. **Validación Continua:** Los agentes implementan código y pruebas contrastando directamente contra las especificaciones del slice correspondiente.
4. **Regla de Redacción:** Toda especificación y documentación generada por IA debe redactarse utilizando la skill **caveman** para garantizar máxima concisión técnica y eliminar contenido redundante.

### Centralización y Contexto Maestro (`AGENTS.md`)

Para inicializar y mantener la sincronización del contexto global, cada repositorio/proyecto cuenta con un archivo **`AGENTS.md`**:

- **Punto de Entrada del Agente:** Es el primer documento que la IA debe consultar para comprender el sistema.
- **Contenido Requerido:**
  - Resumen y objetivos del proyecto.
  - Reglas operativas, convenciones de código y estándares del campus.
  - Índice / mapa centralizado que referencia a todos los módulos (`Modules/` o `src/modules/`), sus componentes clave y sus carpetas `specs/`.
- **Mantenimiento:** Al inicializar un proyecto o crear un nuevo slice/módulo, debe registrarse y enlazarse inmediatamente en el `AGENTS.md` correspondiente.

### Skills Recomendadas para Agentes

Se recomienda el uso de las siguientes skills para optimizar el flujo de desarrollo, diseño e implementación:

- **caveman:** Reducción de ruido y documentación ultra concisa. *(Obligatoria para generar specs)*.
- **frontend-design:** Dirección estética y diseño visual intencional.
- **ui-ux-pro-max:** Sistema de diseño UI/UX, patrones de interacción, accesibilidad y paletas.
- **ponytail:** Simplicidad pragmática y código minimalista (evita sobreingeniería).

#### Comandos de Instalación de Skills

```bash
npx skills add https://github.com/juliusbrussee/caveman --skill caveman
npx skills add https://github.com/anthropics/skills --skill frontend-design
npx skills add https://github.com/nextlevelbuilder/ui-ux-pro-max-skill --skill ui-ux-pro-max
npx skills add https://github.com/dietrichgebert/ponytail --skill ponytail
```

---

## 🏛️ Especificaciones y Arquitecturas (`docs/`)

El proyecto se rige por los siguientes estándares arquitectónicos:

- **[Estándar de Autenticación y Sesiones](./docs/architecture/AUTH.md):** Especificación de seguridad, cookies `HttpOnly`, ciclo de vida del JWT y soporte para login local y federado (Microsoft Entra / Google).
- **[Arquitectura Backend (.NET 10)](./docs/architecture/BACKEND_ARCHITECTURE.md):** Estructura y lineamientos para la API REST, persistencia con EF Core y PostgreSQL, y módulos de negocio con especificaciones locales.
- **[Arquitectura Frontend (Vue 3 + TypeScript)](./docs/architecture/FRONTEND_ARCHITECTURE.md):** Estructura y lineamientos para la aplicación SPA con Vite, rutas modulares descentralizadas, componentes reutilizables y especificaciones por slice.
- **[Contrato Canónico de Autenticación](./docs/specs/auth.spec.md):** Especificación de endpoints `/api/auth/*`, DTOs, cookies y códigos de error compartidos entre backend y frontend.
- **[Esquema y Análisis Legacy](./docs/legacy-db/ESQUEMA_TABLAS.md):** Documentación del modelo relacional previo de 85 tablas.

---

## 📋 Planes de Implementación (`docs/plans/`)

Las hojas de ruta, fases de ejecución técnica y planes de trabajo generados para agentes o desarrolladores se centralizan en `docs/plans/`:

- **[01. Plan de Autenticación Local](./docs/plans/01-auth-implementation.md):** Plan inicial de construcción del esqueleto Backend, Frontend y flujo de autenticación local con PostgreSQL y JWT en cookies.

---

## 🗄️ Base de Datos (`database/`)

El modelado y gestión de la persistencia de datos se divide en:

- **[Borrador de Diseño (`database/design/`)](./database/design/README.md):** Espacio activo para diseñar el esquema relacional en PostgreSQL bajo convenciones `snake_case` y PKs obligatorias.
- **[Datos Semilla (`database/seed/`)](./database/seed/01_dev_users.sql):** Cuentas demo (`admin`, `usuario`) para desarrollo local.
- **[Referencia Legacy (`database/legacy/`)](./database/legacy/):** Scripts SQL Server del sistema anterior SET_MX (*solo consulta, no ejecutar*).

---

## ℹ️ Créditos

> Basado en [TecNM_Project_Template](https://github.com/AngelZurita28/TecNM_Project_Template) de AngelZurita28.
