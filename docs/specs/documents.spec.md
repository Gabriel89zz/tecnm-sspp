# Contrato: Expediente documental

> **Estado:** Borrador v0.1. Convenciones en [api-conventions.spec.md](./api-conventions.spec.md). Tabla: `expedient_documents`.

## Acceso

- `student`: sube, lista y descarga sus documentos; borra solo `pending` propios.
- `SocialServiceStaff`: revisa `tramiteType` `social_service` y `general`.
- `InternshipStaff`: revisa `tramiteType` `internship` y `general`.
- `admin`: todo.

## DTOs

```ts
interface ExpedientDocument {
  id: number;
  studentId: number;
  tramiteType: "social_service" | "internship" | "general";
  tramiteId: number | null;
  documentCode: DocumentCode;
  originalFileName: string;
  fileMimeType: string;
  fileSizeBytes: number;
  status: "pending" | "approved" | "rejected" | "superseded";
  rejectionReason: string | null;
  reviewedByName: string | null;
  reviewedAt: string | null;
  createdAt: string;
}

interface ReviewDocumentRequest {
  status: "approved" | "rejected";
  rejectionReason: string | null;
}
```

`storage_path` nunca se expone. Descarga solo vía endpoint autenticado.

## Endpoints

| Método y ruta | Entrada | Éxito | Errores |
| --- | --- | --- | --- |
| `GET /api/students/{studentId}/documents` | `tramiteType`, `tramiteId`, `status`, `includeSuperseded` (default `false`) | `200 ExpedientDocument[]` | `404` |
| `POST /api/students/{studentId}/documents` | `multipart/form-data`: `file`, `tramiteType`, `tramiteId?`, `documentCode` | `201 ExpedientDocument` | `400`, `404`, `413`, `415`, `422 documents.tramite_mismatch`, `409 documents.already_approved` |
| `GET /api/documents/{id}/file` | — | Stream con `Content-Disposition` | `404` |
| `POST /api/documents/{id}/review` | `ReviewDocumentRequest` | `200 ExpedientDocument` | `400`, `404`, `409 documents.invalid_transition` |
| `DELETE /api/documents/{id}` | — | `204` | `409 documents.not_pending` |
| `GET /api/documents/review-queue` | `tramiteType`, `documentCode`, `periodId`, `page`, `pageSize` | `200 PagedResponse<ExpedientDocument & { studentControlNumber: string; studentFullName: string }>` | `403` |

Alias para estudiante: `studentId` puede ser `me` (`/api/students/me/documents`).

## Reglas

- Archivos: PDF (`application/pdf`) ≤ 5 MB. Se valida tipo real por firma, no solo extensión.
- `tramiteType ≠ general` exige `tramiteId` de un trámite existente del mismo `studentId`.
- `tramiteType = general` exige `tramiteId = null`.
- Nueva carga con mismo (`studentId`, `tramiteType`, `tramiteId`, `documentCode`) marca la anterior `pending`/`rejected` como `superseded`. Si existe una `approved`, responde `409 documents.already_approved`.
- `rejected` exige `rejectionReason` (1–1000).
- Transiciones: `pending → approved | rejected`. `approved`, `rejected`, `superseded` no cambian por revisión.
- Almacenamiento v1: sistema de archivos local bajo `Storage__RootPath`; `storage_path` relativo `{studentId}/{tramiteType}/{guid}.pdf`.

## Documentos esperados por trámite

| Trámite | Códigos | Momento |
| --- | --- | --- |
| SS | `SOL` | `requested` |
| SS | `CP` (emitida por ITSM, puede subirla staff) | `approved` |
| SS | `CA` | Antes de `accepted` |
| SS | `CC` | Antes de `active` |
| SS | `R1`–`R3`, `EV1`–`EV3` | `active` |
| SS | `CT` | Antes de `completed_pending_review` |
| PP | `CP`, `CA`, `CT` | Igual que SS |

## Flujos UI esperados

- Estudiante: lista tipo checklist por trámite con estado de cada código y botón "Subir / Reemplazar".
- Staff: bandeja de revisión con visor PDF embebido, aprobar/rechazar con motivo.
