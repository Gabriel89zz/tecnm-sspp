# Expedients backend

Contrato compartido: [spec raíz de documentos](../../../../docs/specs/documents.spec.md).

## Responsabilidades
- `DocumentsController`: Subida, validación de tipo MIME/tamaño, consulta y descarga de archivos de expediente.
- `DocumentsReviewController`: Aprobación y rechazo con observaciones (`approve`, `reject`).
- Reglas: Manejo polimórfico de expedientes (`social_service`, `internship`, `general`), versión superseded automática al resubir documento observado.
