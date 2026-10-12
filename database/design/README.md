# Borrador de Diseño de Base de Datos (`database/design/`)

Este documento sirve como **plantilla y registro de trabajo en curso** para documentar las tablas, campos, relaciones y decisiones arquitectónicas de la base de datos de **TecNM SSPP**.

---

## 📐 Convenciones de Diseño

1. **Motor de Base de Datos:** PostgreSQL 16+.
2. **Nomenclatura (`snake_case`):**
   - Tablas en plural o sustantivo de entidad: `alumnos`, `empresas`, `solicitudes_servicio`.
   - Columnas en minúsculas con guion bajo: `id`, `numero_control`, `created_at`.
   - Constraints con prefijo estándar: `pk_tabla`, `fk_tabla_origen__tabla_destino`, `uq_tabla_columna`, `ck_tabla_condicion`.
3. **Llaves Primarias (PK):**
   - **Toda tabla debe tener una PK explícita.**
   - Preferencia por UUID (`id uuid PRIMARY KEY DEFAULT gen_random_uuid()`) o enteros auto-incrementales (`id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY`).
4. **Llaves Foráneas (FK) Explícitas:**
   - Declarar restricciones de integridad referencial completas con cláusulas explícitas (`ON DELETE RESTRICT`, `ON UPDATE CASCADE`, etc.).
5. **Cero copia 1:1 del sistema legacy:**
   - La base de datos anterior de SQL Server (`database/legacy/`) contiene tablas desnormalizadas, campos redundantes y falta de PKs.
   - Extraer **únicamente las reglas de negocio necesarias**, normalizando e implementando buenas prácticas relacionales.
6. **Idempotencia:**
   - Scripts estructurados con `DROP TABLE IF EXISTS ... CASCADE;` y bloques transaccionales `BEGIN; ... COMMIT;`.

---

## 📋 Registro de Tablas Diseñadas

| Tabla | Módulo / Dominio | PK | Descripción General | Estado |
| :--- | :--- | :--- | :--- | :--- |
| *ej. `alumnos`* | *Control Escolar* | `id` | *Expediente principal del estudiante* | *Borrador* |
| *ej. `empresas`* | *Vinculación* | `id` | *Empresas receptoras para servicio/residencia* | *Pendiente* |

---

## 🔍 Detalle de Tablas y Atributos

### 1. `nombre_tabla`
- **Descripción:** Propósito funcional de la entidad.

| Columna | Tipo de Dato | Nulo | Default | Constraint / FK | Descripción |
| :--- | :--- | :---: | :--- | :--- | :--- |
| `id` | `uuid` | No | `gen_random_uuid()` | `PK` | Identificador único |
| `nombre` | `varchar(150)` | No | | | Nombre oficial |
| `created_at` | `timestamptz` | No | `CURRENT_TIMESTAMP` | | Fecha de creación |

---

## 🔗 Relaciones y Llaves Foráneas

- `tabla_a.columna_id` ➔ `tabla_b.id` (1:N, `ON DELETE RESTRICT`)

---

## 💡 Decisiones de Diseño (ADR / Rationale)

- **[Fecha - Decisión]:** Justificación de por qué se modeló de cierta forma en lugar de seguir la estructura legacy o alternativas.

---

## 📝 Pendientes y Preguntas de Negocio

- [ ] Validar catálogo de estatus con los lineamientos académicos vigentes.
- [ ] Definir longitud estándar para números de control o matrículas institucionales.
