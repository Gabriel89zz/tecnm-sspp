BEGIN;

-- 1. Catálogo oficial de roles institucionales
INSERT INTO roles (name, description)
VALUES
    ('admin', 'Administrador del Sistema'),
    ('student', 'Estudiante / Prestador de SS / Practicante'),
    ('social_service_officer', 'Encargado(a) de Servicio Social'),
    ('internship_coordinator', 'Coordinador(a) de Prácticas Profesionales'),
    ('department_head', 'Jefe(a) de Departamento Académico')
ON CONFLICT (name) DO UPDATE SET
    description = EXCLUDED.description;

-- 2. Cuentas demo de desarrollo (contraseña: TecNM-Demo-2026!)
INSERT INTO users (id, username, email, password_hash, name, is_active)
OVERRIDING SYSTEM VALUE
VALUES
    (
        1,
        'admin',
        'admin@tecnm.local',
        'AQAAAAIAAYagAAAAEPjbneKeORd2reFIG62af0ufOMPtwf64j0jpBznNBo3Y8K/BUoUW+IhJOm0b5iqWOg==',
        'Administrador Demo',
        true
    ),
    (
        2,
        'usuario',
        'usuario@tecnm.local',
        'AQAAAAIAAYagAAAAECBhXWISKf+YayCz0xB8E4vyjO6chAfRLTq+f2eA4ztdtdDqA99jr+VsfZXUoKbfdQ==',
        'Usuario Demo',
        true
    )
ON CONFLICT (username) DO UPDATE SET
    email = EXCLUDED.email,
    password_hash = EXCLUDED.password_hash,
    name = EXCLUDED.name,
    is_active = EXCLUDED.is_active;

-- Sincronizar secuencia de users tras inserción con IDs explícitos
SELECT setval(pg_get_serial_sequence('users', 'id'), COALESCE(MAX(id), 1)) FROM users;

-- 3. Asignación de roles a usuarios demo
INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id
FROM users u
CROSS JOIN roles r
WHERE (u.username = 'admin' AND r.name = 'admin')
   OR (u.username = 'usuario' AND r.name = 'student')
ON CONFLICT (user_id, role_id) DO NOTHING;

COMMIT;
