-- ============================================================================
-- Proyecto: TecNM SSPP
-- Archivo: database/design/schema.sql
-- Propósito: Borrador iterativo del esquema de base de datos relacional.
-- Motor objetivo: PostgreSQL 16+
--
-- Convenciones:
--   1. Nombres en snake_case (tablas, columnas, índices y constraints).
--   2. Llave Primaria (PK) obligatoria en todas las tablas.
--   3. Llaves Foráneas (FK) con nombres explícitos y reglas de integridad.
--   4. Scripts idempotentes usando DROP TABLE IF EXISTS ... CASCADE.
--   5. Evitar copia 1:1 del legacy (SET_MX / SQL Server).
-- ============================================================================

-- BEGIN;

-- ----------------------------------------------------------------------------
-- Extensiones requeridas
-- ----------------------------------------------------------------------------
-- CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ----------------------------------------------------------------------------
-- Ejemplo de estructura y convenciones:
-- ----------------------------------------------------------------------------
-- DROP TABLE IF EXISTS ejemplo_detalle CASCADE;
-- DROP TABLE IF EXISTS ejemplo_maestro CASCADE;

-- CREATE TABLE ejemplo_maestro (
--     id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
--     codigo varchar(32) NOT NULL,
--     nombre varchar(160) NOT NULL,
--     is_active boolean NOT NULL DEFAULT true,
--     created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
--     updated_at timestamptz,
--     CONSTRAINT uq_ejemplo_maestro_codigo UNIQUE (codigo)
-- );

-- CREATE TABLE ejemplo_detalle (
--     id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
--     maestro_id uuid NOT NULL,
--     descripcion varchar(255) NOT NULL,
--     secuencia integer NOT NULL DEFAULT 1,
--     created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
--     CONSTRAINT fk_ejemplo_detalle__maestro FOREIGN KEY (maestro_id)
--         REFERENCES ejemplo_maestro (id)
--         ON UPDATE CASCADE
--         ON DELETE RESTRICT,
--     CONSTRAINT ck_ejemplo_detalle_secuencia CHECK (secuencia > 0)
-- );

-- CREATE INDEX idx_ejemplo_detalle_maestro_id ON ejemplo_detalle (maestro_id);

-- COMMIT;
