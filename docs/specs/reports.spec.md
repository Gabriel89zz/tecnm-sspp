# Contrato: Reportes e indicadores

> **Estado:** Borrador v0.1. Convenciones en [api-conventions.spec.md](./api-conventions.spec.md). Solo lectura sobre todas las tablas. Política: `ReportViewer`.

Todos los endpoints aceptan `periodId` (default: periodo activo) y responden JSON. Exportación a Excel/PDF la hace frontend (SheetJS / jsPDF).

## 1. Tablero — `GET /api/reports/dashboard`

```ts
interface DashboardResponse {
  period: { id: number; code: string };
  socialService: Record<TramiteStatus, number>;
  internships: Record<TramiteStatus, number>;
  pendingDocumentReviews: number;
  pendingReportReviews: number;
  agreementsExpiringSoon: number;   // validity = expiring
  openNonConformances: number;
}
```

## 2. Seguimiento de Vinculación — `GET /api/reports/tracking`

Reemplaza la hoja `SEGUIMIENTO VINCULACIÓN`. Query: `periodId`, `tramiteType`* (`social_service` | `internship`), `status`, `careerId`.

```ts
interface TrackingRow {
  tramiteId: number;
  controlNumber: string;
  studentFullName: string;
  careerCode: string;
  semesterEnrolled: number;
  gender: "M" | "F" | null;
  companyName: string;
  companySector: string;
  agreementNumber: string | null;
  programName: string | null;        // solo SS
  startDate: string;
  endDate: string;
  status: TramiteStatus;
  presentationLetterDate: string | null;
  acceptanceLetterDate: string | null;
  terminationLetterDate: string | null;
  originalTerminationLetterReceived: boolean;
  releaseCertificateFolio: string | null;
  schoolServicesActSent: boolean;
  beneficiariesCount: number | null; // solo SS
}
```

Respuesta: `200 TrackingRow[]` sin paginar (volumen semestral acotado).

## 3. Estadística SEP/TecNM 911.10 — `GET /api/reports/911`

Query: `periodId`, `tramiteType`*.

```ts
interface Report911Response {
  period: { id: number; code: string };
  tramiteType: "social_service" | "internship";
  byCareerAndGender: { careerCode: string; male: number; female: number; unspecified: number; total: number }[];
  byCompanySector: { sector: string; total: number }[];
  byCompanySize: { companySize: string; total: number }[];
  withDisability: { male: number; female: number; total: number };
  byShift: { shift: string; total: number }[];
  withInclusionActivities: number;   // solo SS
  totalBeneficiaries: number;        // solo SS
}
```

Universo: trámites del periodo con estado `active`, `completed_pending_review` o `released` (**confirmar criterio oficial**, decisión D10).

## 4. Indicador PNC (ISO 9001) — `GET /api/reports/non-conformance-indicator`

```ts
interface NonConformanceIndicator {
  period: { id: number; code: string };
  tramiteType: "social_service" | "internship";
  applicants: number;        // trámites creados en el periodo (excluye cancelled por el propio alumno en requested)
  released: number;
  nonCompliant: number;
  openNonConformances: number;
  releaseRate: number;       // released / applicants, 0–1
  byCategory: { category: string; total: number }[];
}
```

## 5. Tiempos de ciclo — `GET /api/reports/cycle-times` (fase posterior)

Promedio de días entre transiciones a partir de `tramite_status_logs`.

```ts
interface CycleTimesResponse {
  tramiteType: "social_service" | "internship";
  transitions: { from: string; to: string; averageDays: number; samples: number }[];
}
```

## Flujos UI esperados

- Dashboard inicial de staff con tarjetas y gráficas (Chart.js).
- Pantalla "Reportes" con selector de periodo/tipo y botones "Exportar Excel" / "Exportar PDF".
