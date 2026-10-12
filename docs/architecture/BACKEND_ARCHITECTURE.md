# Proyecto Web Backend

## Descripción
**Proyecto Web Backend** es una API RESTful moderna desarrollada sobre el ecosistema .NET. Su propósito principal es desacoplar y centralizar la lógica de negocio empresarial, la persistencia de datos y los servicios transaccionales para alimentar clientes web frontend mediante una arquitectura modular, escalable y mantenible.

---

## Tecnologías Principales (Nivel Raíz)
- **Lenguaje y Runtime:** C# / .NET 10
- **Framework Web:** ASP.NET Core Web API
- **ORM / Persistencia:** Entity Framework Core 10
- **Base de Datos:** PostgreSQL (mediante Npgsql Entity Framework Core Provider)
- **Inyección de Dependencias & Configuración:** Contenedor nativo de .NET (`Microsoft.Extensions.DependencyInjection` / `Microsoft.Extensions.Configuration`)
- **Entorno de Ejecución:** Contenedores Linux / Docker

---

## Arquitectura Estructural (Vertical Slice / Screaming Architecture)

El proyecto organiza su código en función de capacidades y dominios de negocio (*Screaming Architecture*), en lugar de capas técnicas globales. Cada funcionalidad se encapsula en una rebanada vertical (*Vertical Slice*) independiente que incluye sus propias especificaciones bajo el enfoque **SDD (Spec-Driven Development)**.

```text
Proyecto Web Backend/
│
├── Core/
│   └── Entities/                     # Entidades de dominio compartidas (POCOs / Mapeo EF Core)
│
├── Data/
│   └── [App]DbContext.cs             # Configuración del DbContext y mapeo relacional
│
├── Modules/                          # Vertical Slices (Módulos por dominio de negocio)
│   ├── [FeatureModule]/              # Rebanada vertical de un dominio específico
│   │   ├── specs/                    # Especificaciones funcionales, requerimientos y SDD del módulo
│   │   ├── [Feature]Controller.cs    # Thin Controller: enrutamiento HTTP y códigos de respuesta
│   │   ├── [Feature]Service.cs       # Fat Service: lógica de negocio, validaciones y persistencia
│   │   └── [Feature]DTOs.cs          # Contratos de datos (Requests y Responses)
│   └── ...
├── Properties/                       # Perfiles de inicialización y configuración de runtime
├── appsettings.json                  # Configuración de variables de entorno y conexión
└── Program.cs                        # Punto de entrada, configuración de middleware y registro DI
```

### Principios de la Estructura
- **Spec-Driven Development (SDD):** Cada módulo contiene su carpeta `specs/` donde se definen reglas funcionales, contratos y criterios antes de la codificación o asistencia por IA.
- **Thin Controllers:** Responsabilidad única de transporte HTTP; sin lógica de negocio.
- **Fat Services:** Encapsulamiento completo de reglas de negocio, transacciones y consultas LINQ asíncronas.
- **DTOs Desacoplados:** Transferencia de datos mediante modelos dedicados por módulo, sin exponer entidades del ORM.
- **Aislamiento Modular:** Alta cohesión interna por módulo y mínimo acoplamiento transversal.
