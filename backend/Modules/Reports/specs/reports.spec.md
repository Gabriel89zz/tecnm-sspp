# Reports backend

Contrato compartido: [spec raíz de reportes](../../../../docs/specs/reports.spec.md).

## Responsabilidades
- `ReportsController`: Estadísticas, métricas institucionales (Formato 911.10 de SEP/TecNM), tasa de liberación por carrera y periodo.
- `Views`: Encapsulamiento de consultas complejas en Vistas SQL PostgreSQL con entidades sin clave en EF Core.
