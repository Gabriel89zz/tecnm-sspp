# Social Service backend

Contrato compartido: [spec raíz de servicio social](../../../../docs/specs/social-service.spec.md).

## Responsabilidades
- `SocialServicesController`: Trámite maestro de Servicio Social, máquina de estados, `allowedActions`, checklist de avance.
- `ApplicationsController` & `CommitmentsController`: Anexos XVIII y XIX.
- `BiMonthlyReportsController`: Tres reportes bimestrales secuenciales, validación y acumulación de horas.
- `EvaluationsController`: Evaluaciones bimestrales y final (Anexo XXIII externo y XXIV autoevaluación).
- `ReleaseController`: Emisión de carta de terminación y constancia con asignación atómica de folio.
