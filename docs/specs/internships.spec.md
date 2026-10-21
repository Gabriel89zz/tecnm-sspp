# Contrato: Prácticas Profesionales

> **Estado:** Borrador v0.1. Convenciones en [api-conventions.spec.md](./api-conventions.spec.md). Tablas: `internships`, `internship_evaluations`. Escribe en `tramite_status_logs`, `non_conformances`, `document_series`. Máquina de estados compartida con SS (sección 6.1 de convenciones).

## Acceso

- `InternshipStaff`: todo.
- `social_service_officer`, `department_head`: lectura.
- `student`: crea y consulta sus trámites.

## 1. DTOs

```ts
interface InternshipListItem {
  id: number;
  studentId: number;
  studentControlNumber: string;
  studentFullName: string;
  careerCode: string;
  periodCode: string;
  companyName: string;
  status: TramiteStatus;
  completedHours: number;
  requiredHours: number;
  isExpedientComplete: boolean;
  startDate: string;
  endDate: string;
  updatedAt: string;
}

interface InternshipDetail {
  id: number;
  student: { id: number; controlNumber: string; fullName: string; careerName: string; hasValidImss: boolean };
  period: { id: number; code: string };
  company: { id: number; name: string };
  supervisor: { id: number; fullName: string; position: string };
  agreement: { id: number; agreementNumber: string; validity: string } | null;
  semesterEnrolled: number;
  startDate: string;
  endDate: string;
  scheduleDescription: string;
  requiredHours: number;
  completedHours: number;
  status: TramiteStatus;
  creditsAwarded: 2;
  presentationLetterDate: string | null;
  acceptanceLetterDate: string | null;
  terminationLetterDate: string | null;
  originalTerminationLetterReceived: boolean;
  releaseCertificateFolio: string | null;
  releaseDate: string | null;
  releasedAt: string | null;
  isExpedientComplete: boolean;
  schoolServicesActSent: boolean;
  schoolServicesActSentAt: string | null;
  schoolServicesActFolio: string | null;
  observations: string | null;
  cancellationReason: string | null;
  evaluation: InternshipEvaluation | null;
  allowedActions: InternshipAction[];
  checklist: TramiteChecklistItem[];
  createdAt: string;
  updatedAt: string;
}

type InternshipAction =
  | "edit" | "approve" | "register_presentation_letter" | "register_acceptance" | "activate"
  | "update_hours" | "complete" | "register_evaluation" | "update_expedient"
  | "release" | "send_school_act" | "cancel" | "mark_non_compliant";

interface CreateInternshipRequest {
  studentId: number | null;   // staff: requerido; student: se ignora
  periodId: number;
  companyId: number;
  supervisorId: number;
  agreementId: number | null;
  startDate: string;
  endDate: string;
  scheduleDescription: string;
  requiredHours: number;      // default UI 200
}

interface InternshipEvaluationRequest {   // Anexo XVII
  evaluationStartDate: string;
  evaluationEndDate: string;
  criteria: [number, number, number, number, number, number, number]; // enteros 0–4
  evaluatorSupervisorId: number | null;
  evaluatorSignatureDate: string;
  observations: string | null;
}

interface InternshipEvaluation extends InternshipEvaluationRequest {
  id: number;
  activityName: string;          // "PRODUCTIVIDAD LABORAL"
  numericalAverage: number;      // calculado, 2 decimales
  performanceLevel: QualitativeGrade; // calculado
  isCompetencyAchieved: boolean; // calculado: promedio ≥ 1.00
}
```

## 2. Validación

| Campo | Regla | Error |
| --- | --- | --- |
| `periodId`, `companyId`, `supervisorId` | Requeridos; activos | `400` |
| `supervisorId` | Pertenece a `companyId` | `422 internships.supervisor_company_mismatch` |
| `agreementId` | Opcional; misma empresa, tipo compatible, vigente en `startDate` | `422 internships.agreement_invalid` |
| `startDate`, `endDate` | Requeridas; `endDate ≥ startDate` | `400` |
| `scheduleDescription` | Requerido, 1–150 | `400` |
| `requiredHours` | Entero 100–480 según DDL (**ver decisión D6: README indica 200–480**) | `400` |
| `criteria[i]` | Exactamente 7 enteros 0–4 | `400` |
| `evaluationStartDate ≤ evaluationEndDate` | Requeridas | `400` |
| `evaluatorSignatureDate` | Requerida | `400` |
| Estudiante | Elegible para PP | `422 internships.student_not_eligible` |

## 3. Endpoints

| Método y ruta | Rol | Entrada | Transición | Éxito | Errores |
| --- | --- | --- | --- | --- | --- |
| `GET /api/internships` | Staff, lectura | `periodId`, `status`, `careerId`, `companyId`, `isExpedientComplete`, `search`, `sort` | — | `200 PagedResponse<InternshipListItem>` | — |
| `GET /api/internships/me` | student | — | — | `200 InternshipListItem[]` | `404` |
| `GET /api/internships/{id}` | Todos (propio) | — | — | `200 InternshipDetail` | `404` |
| `POST /api/internships` | student, staff | `CreateInternshipRequest` | → `requested` | `201` | `400`, `422` |
| `PUT /api/internships/{id}` | student (`requested`), staff (no terminal) | Igual sin `studentId` | — | `200` | `409`, `422` |
| `POST .../{id}/approve` | staff | `{ observations }` | `requested → approved` | `200` | `409` |
| `POST .../{id}/presentation-letter` | staff | `{ presentationLetterDate }` | — (en `approved`) | `200` | `409` |
| `POST .../{id}/acceptance` | staff | `{ acceptanceLetterDate }` | `approved → accepted` | `200` | `409`, `422 documents_missing` |
| `POST .../{id}/activate` | staff | — | `accepted → active` | `200` | `409`, `422 imss_invalid` |
| `PATCH .../{id}/hours` | staff | `{ completedHours }` (entero ≥ 0) | — (en `active`) | `200` | `409`, `400` |
| `POST .../{id}/complete` | staff | `{ terminationLetterDate }` | `active → completed_pending_review` | `200` | `422 hours_incomplete` |
| `PUT .../{id}/evaluation` | staff | `InternshipEvaluationRequest` | — (en `completed_pending_review`) | `200 InternshipEvaluation` | `409`, `400` |
| `PATCH .../{id}/expedient` | staff | `{ isExpedientComplete, originalTerminationLetterReceived, observations }` | — | `200` | `400` |
| `POST .../{id}/release` | staff | `{ releaseDate }` | `completed_pending_review → released` | `200` (folio) | `422 evaluation_missing`, `422 competency_not_achieved`, `422 expedient_incomplete` |
| `POST .../{id}/school-services-act` | staff | — | — (en `released`) | `200` (folio acta) | `409 act_already_sent` |
| `POST .../{id}/cancel` | staff; student en `requested` | `{ reason }` | no terminal → `cancelled` | `200` | `409` |
| `POST .../{id}/non-compliance` | staff | `{ category, description }` | → `non_compliant` (+ PNC) | `200` | `409` |
| `GET .../{id}/history` | Todos (propio) | — | — | `200 StatusLogItem[]` | `404` |
| `GET .../{id}/print-data/{document}` | staff, student | `presentation_letter`, `evaluation`, `release_certificate` | — | `200` | `409 not_available` |

Prefijo de `code`: `internships.`.

## 4. Reglas de negocio

1. Máximo un PP no terminal por estudiante; uno `released` impide otro.
2. `semesterEnrolled` = `student.currentSemester` al crear. `creditsAwarded` siempre 2.
3. `complete` exige `completedHours ≥ requiredHours` y documento `CT` aprobado.
4. Evaluación (1:1): backend calcula `numericalAverage = round(avg(criteria), 2)`; `isCompetencyAchieved = numericalAverage ≥ 1.00`; `performanceLevel` por rango (tentativo, validar con Anexo XVII): `≥ 3.50 excellent`, `≥ 2.50 notable`, `≥ 1.50 good`, `≥ 1.00 sufficient`, `< 1.00 insufficient`.
5. `release` exige evaluación con `isCompetencyAchieved=true`, `isExpedientComplete=true`, `originalTerminationLetterReceived=true`. Consume folio `internship_release_certificate`; fija `releaseDate` y `releasedAt`.
6. Evaluación insuficiente: staff usa `non-compliance` con `category=insufficient_grade`.
7. Toda transición escribe `tramite_status_logs`.

## 5. Checklist por estado

| Estado | Claves |
| --- | --- |
| `requested` | `request_reviewed` |
| `approved` | `presentation_letter`, `document_CA` |
| `accepted` | `imss_valid` |
| `active` | `hours_completed`, `document_CT` |
| `completed_pending_review` | `evaluation_registered`, `competency_achieved`, `expedient_complete`, `original_termination_letter` |
| `released` | `school_services_act` |

## 6. Flujos UI esperados

- Estudiante: solicitud simple (empresa, supervisor, fechas, horario, horas) y vista de seguimiento.
- Staff: tablero, detalle con línea de tiempo y botones por `allowedActions`; formulario Anexo XVII con 7 selectores 0–4 y vista previa del promedio (cálculo cliente solo informativo; el valor oficial lo devuelve backend).
