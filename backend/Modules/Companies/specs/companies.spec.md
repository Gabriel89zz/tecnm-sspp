# Companies backend

Contrato compartido: [spec raíz de empresas](../../../../docs/specs/companies.spec.md).

## Responsabilidades
- `CompaniesController`: CRUD de empresas y receptores externos/internos.
- `SupervisorsController`: CRUD de supervisores de empresa asignados a una compañía.
- `AgreementsController`: Gestión de convenios de colaboración (marco general, SS, PP), control de vigencia y fechas de inicio/fin.
- Regla R1: Supervisor y convenio deben pertenecer a la misma empresa.
