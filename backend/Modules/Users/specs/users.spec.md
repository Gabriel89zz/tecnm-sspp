# Users backend

Contrato compartido: [spec raíz de usuarios](../../../../docs/specs/users.spec.md).

## Responsabilidades
- `UsersController`: CRUD de cuentas institucionales (`/api/users`), asignación de roles (`PUT /roles`) y reseteo de contraseña.
- `UsersService`: validación de unicidad de `username` y `email`, hasheo de contraseñas (`PasswordHasher<User>`), sincronización transaccional de roles.
- `UsersDTOs`: transporte desacoplado sin filtrar `password_hash`.
