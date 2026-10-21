# Students backend

Contrato compartido: [spec raíz de estudiantes](../../../../docs/specs/students.spec.md).

## Responsabilidades
- `StudentsController`: Padrón de alumnos, consulta de elegibilidad (`/eligibility`), expediente `/me` para perfil de estudiante.
- `StudentImportsController`: Procesamiento de lotes de importación masiva (`student_import_batches`), lectura de plantillas Excel y reporte de filas con error.
- Reglas: Cálculo de porcentaje de avance curricular (`credits / career.totalCredits * 100`), validación de vigencia IMSS.
