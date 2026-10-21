# Contrato: Servicio Social

> **Estado:** Borrador v0.1. Convenciones en [api-conventions.spec.md](./api-conventions.spec.md). Tablas: `social_services`, `social_service_applications`, `social_service_commitments`, `social_service_bi_monthly_reports`, `social_service_evaluations`, `social_service_activity_assessments`. Escribe en `tramite_status_logs`, `non_conformances`, `document_series`.

## Acceso

- `SocialServiceStaff`: todo.
- `internship_coordinator`, `department_head`: lectura.
- `student`: crea y opera solo sus trámites; acciones limitadas por `allowedActions`.

---

## 1. DTOs principales

```ts
interface SocialServiceListItem {
  id: number;
  studentId: number;
  studentControlNumber: string;
  studentFullName: string;
  careerCode: string;
  periodCode: string;
  programName: string;
  companyName: string;
  status: TramiteStatus;
  accumulatedHours: number;
  requiredHours: number;
  startDate: string;
  endDate: string;
  pendingReviewsCount: number;   // documentos + reportes por revisar
  updatedAt: string;
}

interface SocialServiceDetail {
  id: number;
  student: { id: number; controlNumber: string; fullName: string; careerName: string; hasValidImss: boolean };
  period: { id: number; code: string };
  program: { id: number; code: string; name: string; category: string };
  company: { id: number; name: string; isTecnmInternal: boolean };
  supervisor: { id: number; fullName: string; position: string };
  agreement: { id: number; agreementNumber: string; validity: string } | null;
  semesterEnrolled: number;
  modality: "internal" | "external";
  startDate: string;
  endDate: string;
  requiredHours: number;
  accumulatedHours: number;
  status: TramiteStatus;
  presentationLetterDate: string | null;
  acceptanceLetterDate: string | null;
  terminationLetterDate: string | null;
  originalTerminationLetterReceived: boolean;
  releaseCertificateFolio: string | null;
  releasedAt: string | null;
  finalGradeLevel: QualitativeGrade | null;
  finalScore: number | null;
  schoolServicesActSent: boolean;
  schoolServicesActSentAt: string | null;
  schoolServicesActFolio: string | null;
  beneficiariesCount: number;
  hasInclusionActivities: boolean;
  targetCommunity: string | null;
  observations: string | null;
  cancellationReason: string | null;
  application: SocialServiceApplication;
  commitment: SocialServiceCommitment | null;
  reports: BiMonthlyReport[];
  evaluations: SocialServiceEvaluation[];
  activityAssessment: ActivityAssessment | null;
  allowedActions: SocialServiceAction[];
  checklist: TramiteChecklistItem[];
  createdAt: string;
  updatedAt: string;
}

type SocialServiceAction =
  | "edit" | "review_application" | "register_presentation_letter" | "register_acceptance"
  | "register_commitment" | "activate" | "submit_report" | "review_report"
  | "register_evaluation" | "register_self_evaluation" | "register_activity_assessment"
  | "update_impact" | "complete" | "release" | "send_school_act" | "cancel" | "mark_non_compliant";
```

## 2. Crear solicitud (Anexo XVIII)

`POST /api/social-services` — crea `social_services` (`status=requested`) + `social_service_applications` en una transacción.

```ts
interface CreateSocialServiceRequest {
  studentId: number | null;     // staff: requerido; student: se ignora (usa el propio)
  periodId: number;
  programId: number;
  companyId: number;
  supervisorId: number;
  agreementId: number | null;
  modality: "internal" | "external";
  startDate: string;
  endDate: string;
  targetCommunity: string | null;
  application: {
    studentAddress: string;
    studentPhone: string;
    periodType: "august_january" | "february_july";
    dependencyName: string;          // UI precarga desde empresa
    dependencyHeadName: string;      // UI precarga desde supervisor
    dependencyHeadPosition: string;  // UI precarga desde supervisor
    activitiesOutline: string;
  };
}
```

| Campo | Regla | Error |
| --- | --- | --- |
| `periodId`, `programId`, `companyId`, `supervisorId` | Requeridos; existentes y activos | `400` |
| `supervisorId` | Debe pertenecer a `companyId` | `422 social_services.supervisor_company_mismatch` |
| `agreementId` | Opcional; misma empresa, tipo compatible y vigente en `startDate` | `422 social_services.agreement_invalid` |
| `modality` | `internal` exige `company.isTecnmInternal=true`; `external` exige `false` | `422 social_services.modality_mismatch` |
| `startDate`/`endDate` | `endDate ≥ startDate + 6 meses` y `≤ startDate + 2 años` | `422 social_services.invalid_duration` |
| `targetCommunity` | Opcional, máx. 200 | `400` |
| `application.studentAddress` | Requerido | `400` |
| `application.studentPhone` | Requerido, máx. 20 | `400` |
| `application.dependencyName` | Requerido, 1–200 | `400` |
| `application.dependencyHeadName` | Requerido, 1–180 | `400` |
| `application.dependencyHeadPosition` | Requerido, 1–150 | `400` |
| `application.activitiesOutline` | Requerido | `400` |
| Estudiante | Elegible para SS (ver students.spec) | `422 social_services.student_not_eligible` |

Backend fija: `semesterEnrolled` y `application.currentSemester` = `student.currentSemester`; `requiredHours=480`; `accumulatedHours=0`. Respuesta: `201 SocialServiceDetail`.

## 3. Endpoints

| Método y ruta | Rol | Entrada | Transición | Éxito | Errores |
| --- | --- | --- | --- | --- | --- |
| `GET /api/social-services` | Staff, lectura | `periodId`, `status`, `careerId`, `companyId`, `programId`, `search`, `sort` (`updatedAt`, `startDate`, `studentControlNumber`) | — | `200 PagedResponse<SocialServiceListItem>` | — |
| `GET /api/social-services/me` | student | — | — | `200 SocialServiceListItem[]` | `404 students.profile_not_linked` |
| `GET /api/social-services/{id}` | Todos (propio) | — | — | `200 SocialServiceDetail` | `404` |
| `POST /api/social-services` | student, staff | `CreateSocialServiceRequest` | → `requested` | `201` | `400`, `422` |
| `PUT /api/social-services/{id}` | student (`requested`), staff (no terminal) | Igual a crear sin `studentId` | Reinicia dictamen a `null` si estaba rechazado | `200` | `409 invalid_transition`, `422` |
| `POST .../{id}/application/review` | staff | `{ isAccepted, rejectionReason, officeObservations }` | `requested → approved` si `isAccepted` | `200` | `409`, `400` (motivo requerido si rechaza) |
| `POST .../{id}/presentation-letter` | staff | `{ presentationLetterDate }` | — (en `approved`) | `200` | `409` |
| `POST .../{id}/acceptance` | staff | `{ acceptanceLetterDate }` | `approved → accepted` | `200` | `409`, `422 documents_missing` (`CA` aprobado) |
| `PUT .../{id}/commitment` | student, staff | `CommitmentRequest` | — (en `accepted`) | `200` | `409`, `400` |
| `POST .../{id}/activate` | staff | — | `accepted → active` | `200` | `409`, `422 imss_invalid`, `422 commitment_missing` |
| `POST .../{id}/reports` | student, staff | `ReportRequest` | — (en `active`) | `201 BiMonthlyReport` | `409`, `422` |
| `PUT .../{id}/reports/{reportId}` | student, staff | `ReportRequest` | Reenvío: `rejected → submitted` | `200` | `409 reports.not_editable` |
| `POST .../{id}/reports/{reportId}/review` | staff | `{ status: "approved"\|"rejected", officeObservations }` | — | `200 BiMonthlyReport` | `409`, `400` |
| `PUT .../{id}/evaluations/{evaluatorType}/{bimonthlyPeriod}` | staff (`external_supervisor`), student (`student_self`) | `EvaluationRequest` | — (en `active` o `completed_pending_review`) | `200 SocialServiceEvaluation` | `403`, `409`, `400` |
| `PUT .../{id}/activity-assessment` | student, staff | `ActivityAssessmentRequest` | — (en `completed_pending_review`) | `200` | `409` |
| `PATCH .../{id}/impact` | staff | `{ beneficiariesCount, hasInclusionActivities, targetCommunity, observations }` | — | `200` | `400` |
| `POST .../{id}/complete` | staff | `{ terminationLetterDate }` | `active → completed_pending_review` | `200` | `409`, `422 hours_incomplete`, `422 reports_incomplete` |
| `POST .../{id}/release` | staff | `{ finalGradeLevel, finalScore, originalTerminationLetterReceived }` | `completed_pending_review → released` | `200` (incluye folio) | `409`, `422 evaluations_incomplete`, `422 assessment_missing`, `422 original_letter_missing` |
| `POST .../{id}/school-services-act` | staff | — | — (en `released`) | `200` (folio de acta) | `409 act_already_sent` |
| `POST .../{id}/cancel` | staff; student solo en `requested` | `{ reason }` | no terminal → `cancelled` | `200` | `409`, `400` |
| `POST .../{id}/non-compliance` | staff | `{ category, description }` | `approved…completed_pending_review → non_compliant` | `200` (+ PNC creado) | `409`, `400` |
| `GET .../{id}/history` | Todos (propio) | — | — | `200 StatusLogItem[]` (ver quality.spec) | `404` |
| `GET .../{id}/print-data/{document}` | staff, student (propio) | `document`: `application`, `presentation_letter`, `commitment`, `release_certificate` | — | `200` objeto plano con datos + autoridades activas + folio | `404`, `409 not_available` |

Prefijo de `code` de error: `social_services.` (p. ej. `social_services.hours_incomplete`).

## 4. Sub-DTOs

```ts
interface SocialServiceApplication {
  studentAddress: string; studentPhone: string; periodType: string; currentSemester: number;
  dependencyName: string; dependencyHeadName: string; dependencyHeadPosition: string;
  activitiesOutline: string;
  isAcceptedByOffice: boolean | null;   // null = sin dictamen
  rejectionReason: string | null; officeObservations: string | null;
  reviewedByName: string | null; reviewedAt: string | null;
}

interface CommitmentRequest {           // Anexo XX
  studentAddress: string;               // requerido
  studentPhone: string;                 // requerido, máx. 20
  dependencyAddress: string;            // requerido
  commitmentDate: string;               // requerida
  city: string;                         // requerido, 1–100, default "Monclova"
}
interface SocialServiceCommitment extends CommitmentRequest {
  studentControlNumber: string;         // snapshot por backend
  isSignedByStudent: boolean;
  signedAt: string;
}

interface ReportRequest {               // Anexo XXII
  periodStartDate: string;
  periodEndDate: string;
  activitiesSummary: string;
  reportedHours: number;
  evaluatorSupervisorId: number | null;
}
interface BiMonthlyReport extends ReportRequest {
  id: number;
  reportNumber: 1 | 2 | 3;              // asignado por backend
  accumulatedHours: number;             // calculado por backend
  status: "submitted" | "approved" | "rejected";
  officeObservations: string | null;
  approvedByName: string | null;
  approvedAt: string | null;
}

interface EvaluationRequest {           // Anexos XXIII / XXIV
  criteria: [QualitativeGrade, QualitativeGrade, QualitativeGrade, QualitativeGrade,
             QualitativeGrade, QualitativeGrade, QualitativeGrade]; // exactamente 7
  evaluatorSupervisorId: number | null; // requerido si external_supervisor
  observations: string | null;
}
interface SocialServiceEvaluation extends EvaluationRequest {
  id: number;
  evaluatorType: "external_supervisor" | "student_self";
  bimonthlyPeriod: "1" | "2" | "3" | "final";
}

interface ActivityAssessmentRequest {   // Anexo XXV
  questions: QualitativeGrade[];        // exactamente 8
  observations: string | null;
}
type ActivityAssessment = ActivityAssessmentRequest & { id: number; createdAt: string };
```

Textos oficiales de los 7 criterios y 8 reactivos: tomar literal de los PDF fuente en `docs/sources/servicio-social/formatos/` (Anexos XXIII, XXIV, XXV). El orden del arreglo = número de criterio.

## 5. Reglas de negocio

1. Un estudiante tiene máximo un SS no terminal; un SS `released` impide crear otro.
2. Reportes: `reportNumber` = siguiente libre (1→3). Reporte *n* exige reporte *n−1* `approved`. Máximo 3 (`409 reports.limit_reached`).
3. Periodo del reporte dentro de `[startDate, endDate]` del trámite y `periodStartDate ≥ periodEndDate` del reporte anterior (`422 reports.invalid_period`).
4. `accumulatedHours` del reporte = acumulado del reporte anterior aprobado + `reportedHours`. Al aprobar, `social_services.accumulated_hours` se actualiza con ese valor.
5. Editar reporte solo en `submitted` o `rejected`. Editar uno `rejected` lo regresa a `submitted`.
6. `complete` exige 3 reportes `approved` y `accumulatedHours ≥ requiredHours`, más documento `CT` aprobado.
7. `release` exige: evaluaciones `external_supervisor` y `student_self` del periodo `final`, `activityAssessment`, `originalTerminationLetterReceived=true`, `finalScore` 0–100 con 2 decimales y `finalGradeLevel` válido. Consume folio `social_service_release_certificate` de forma atómica y fija `releasedAt`.
8. `school-services-act` exige `released`; consume folio `school_services_act`; fija `schoolServicesActSent=true` y `schoolServicesActSentAt`.
9. `non-compliance` crea `non_conformances` (`tramiteType=social_service`, `periodId` del trámite) en la misma transacción.
10. Toda transición inserta `tramite_status_logs` (`previous_status`, `new_status`, `reason`, `changed_by = sub`).

## 6. Checklist por estado (claves estables)

| Estado | Claves |
| --- | --- |
| `requested` | `application_submitted`, `document_SOL`, `application_reviewed` |
| `approved` | `presentation_letter`, `document_CA` |
| `accepted` | `commitment_registered`, `document_CC`, `imss_valid` |
| `active` | `report_1`, `report_2`, `report_3`, `hours_completed`, `document_CT` |
| `completed_pending_review` | `evaluation_supervisor_final`, `evaluation_self_final`, `activity_assessment`, `original_termination_letter` |
| `released` | `school_services_act` |

## 7. Flujos UI esperados

- **Estudiante:** asistente "Solicitar Servicio Social" (elegibilidad → dependencia/supervisor/convenio → datos Anexo XVIII → confirmar). Después, vista de trámite con línea de tiempo (`history`), checklist, barra de horas `accumulatedHours/requiredHours`, sección de reportes 1–3, documentos y evaluaciones.
- **Staff:** tablero con filtros por estado/periodo y contadores; detalle con las mismas secciones y botones derivados de `allowedActions`; modales para dictamen, reporte, evaluación, liberación y acta.
- **Impresión:** frontend arma PDFs con `print-data` (jsPDF). Backend es dueño de folios y autoridades.
