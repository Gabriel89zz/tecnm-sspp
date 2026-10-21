<script setup lang="ts">
import { isAxiosError } from 'axios'
import { computed, onMounted, ref, watch } from 'vue'
import {
  UiAlert,
  UiBadge,
  UiBasePage,
  UiButton,
  UiCard,
  UiDialog,
  UiField,
  UiTable,
  UiToast,
} from '../../../shared/components'
import {
  createCareer,
  deleteCareer,
  getCareers,
  updateCareer,
  updateCareerStatus,
} from '../api/careersApi'
import type { Career } from '../types/careers'

interface CareerRow extends Career {
  actions?: string
}

const careers = ref<CareerRow[]>([])
const isLoading = ref(true)
const pageError = ref('')
const toast = ref<InstanceType<typeof UiToast> | null>(null)

// Filtros y paginación
const searchQuery = ref('')
const statusFilter = ref('all')
const currentPage = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)
const pageSize = 10

const filterOptions = [
  { value: 'all', label: 'Todas las carreras' },
  { value: 'active', label: 'Solo activas' },
  { value: 'inactive', label: 'Solo inactivas' },
]

const columns = [
  { key: 'code' as const, label: 'Clave' },
  { key: 'name' as const, label: 'Nombre oficial' },
  { key: 'totalCredits' as const, label: 'Créditos' },
  { key: 'isActive' as const, label: 'Estado' },
  { key: 'actions' as const, label: 'Acciones' },
]

async function loadCareers(): Promise<void> {
  isLoading.value = true
  pageError.value = ''

  try {
    const isActiveParam =
      statusFilter.value === 'active'
        ? true
        : statusFilter.value === 'inactive'
          ? false
          : null

    const data = await getCareers({
      page: currentPage.value,
      pageSize,
      search: searchQuery.value.trim() || undefined,
      isActive: isActiveParam,
      sort: 'code',
    })

    careers.value = data.items
    totalPages.value = data.totalPages
    totalItems.value = data.totalItems
  } catch (error) {
    pageError.value = 'No fue posible cargar el catálogo de carreras. Revisa la conexión con el servidor.'
  } finally {
    isLoading.value = false
  }
}

// Recargar cuando cambien filtros con reinicio a página 1
let searchDebounce: ReturnType<typeof setTimeout> | null = null
watch(searchQuery, () => {
  if (searchDebounce) clearTimeout(searchDebounce)
  searchDebounce = setTimeout(() => {
    currentPage.value = 1
    loadCareers()
  }, 300)
})

watch(statusFilter, () => {
  currentPage.value = 1
  loadCareers()
})

watch(currentPage, loadCareers)

// Estado del Modal de Creación / Edición
const dialogOpen = ref(false)
const isEditing = ref(false)
const editingId = ref<number | null>(null)
const formCode = ref('')
const formName = ref('')
const formCredits = ref('260')
const formError = ref('')
const isSaving = ref(false)

const dialogTitle = computed(() => (isEditing.value ? 'Editar Carrera' : 'Nueva Carrera'))

function openCreateModal(): void {
  isEditing.value = false
  editingId.value = null
  formCode.value = ''
  formName.value = ''
  formCredits.value = '260'
  formError.value = ''
  dialogOpen.value = true
}

function openEditModal(career: Career): void {
  isEditing.value = true
  editingId.value = career.id
  formCode.value = career.code
  formName.value = career.name
  formCredits.value = career.totalCredits.toString()
  formError.value = ''
  dialogOpen.value = true
}

async function handleSave(): Promise<void> {
  formError.value = ''

  const code = formCode.value.trim().toUpperCase()
  const name = formName.value.trim()
  const credits = parseInt(formCredits.value, 10)

  if (!code) {
    formError.value = 'La clave es obligatoria.'
    return
  }
  if (!name) {
    formError.value = 'El nombre es obligatorio.'
    return
  }
  if (isNaN(credits) || credits <= 0) {
    formError.value = 'Los créditos deben ser un número entero mayor a 0.'
    return
  }

  isSaving.value = true

  try {
    if (isEditing.value && editingId.value !== null) {
      await updateCareer(editingId.value, { code, name, totalCredits: credits })
      toast.value?.show(`Carrera "${code}" actualizada correctamente.`)
    } else {
      await createCareer({ code, name, totalCredits: credits })
      toast.value?.show(`Carrera "${code}" registrada exitosamente.`)
    }

    dialogOpen.value = false
    await loadCareers()
  } catch (error) {
    if (isAxiosError(error) && error.response?.data?.title) {
      formError.value = error.response.data.title
    } else if (isAxiosError(error) && error.response?.status === 409) {
      formError.value = 'La clave ingresada ya está registrada en otra carrera.'
    } else {
      formError.value = 'Ocurrió un error al guardar los datos de la carrera.'
    }
  } finally {
    isSaving.value = false
  }
}

// Toggle de estado
const togglingId = ref<number | null>(null)

async function handleToggleStatus(career: Career): Promise<void> {
  togglingId.value = career.id
  const nextStatus = !career.isActive

  try {
    await updateCareerStatus(career.id, nextStatus)
    career.isActive = nextStatus
    toast.value?.show(
      `Carrera "${career.code}" ${nextStatus ? 'activada' : 'desactivada'} correctamente.`
    )
  } catch (error) {
    toast.value?.show('No fue posible actualizar el estado de la carrera.')
  } finally {
    togglingId.value = null
  }
}

// Confirmación de Eliminación
const deleteDialogOpen = ref(false)
const careerToDelete = ref<Career | null>(null)
const deleteError = ref('')
const isDeleting = ref(false)

function openDeleteModal(career: Career): void {
  careerToDelete.value = career
  deleteError.value = ''
  deleteDialogOpen.value = true
}

async function handleConfirmDelete(): Promise<void> {
  if (!careerToDelete.value) return

  isDeleting.value = true
  deleteError.value = ''

  try {
    await deleteCareer(careerToDelete.value.id)
    toast.value?.show(`Carrera "${careerToDelete.value.code}" eliminada.`)
    deleteDialogOpen.value = false
    await loadCareers()
  } catch (error) {
    if (isAxiosError(error) && error.response?.status === 409) {
      deleteError.value =
        'No se puede eliminar la carrera porque tiene expedientes o alumnos asociados.'
    } else {
      deleteError.value = 'Ocurrió un error al eliminar la carrera.'
    }
  } finally {
    isDeleting.value = false
  }
}

onMounted(loadCareers)
</script>

<template>
  <UiBasePage
    fluid
    title="Catálogo de Carreras"
    description="Gestión oficial de carreras y planes de estudio del Instituto Tecnológico de Monclova."
  >
    <template #actions>
      <UiButton variant="primary" @click="openCreateModal">
        + Nueva Carrera
      </UiButton>
    </template>

    <UiCard class="catalogs-card">
      <!-- Barra de Filtros y Búsqueda -->
      <div class="filter-bar">
        <div class="filter-inputs">
          <div class="search-field">
            <UiField
              v-model="searchQuery"
              label="Buscar por clave o nombre"
              placeholder="Ej. INF o Informática..."
            />
          </div>
          <div class="select-field">
            <UiField
              v-model="statusFilter"
              label="Filtrar por estado"
              as="select"
              :options="filterOptions"
            />
          </div>
        </div>
        <div class="summary-text">
          <span>Total: <strong>{{ totalItems }}</strong> carreras</span>
        </div>
      </div>

      <!-- Errores de carga -->
      <UiAlert v-if="pageError" tone="danger" title="Error de conexión" class="mb-4">
        {{ pageError }}
      </UiAlert>

      <!-- Tabla de Carreras -->
      <div v-if="isLoading" class="loading-state">
        <p>Cargando catálogo de carreras...</p>
      </div>

      <template v-else>
        <UiTable
          label="Catálogo de Carreras"
          :columns="columns"
          :rows="careers"
        >
          <template #cell-code="{ value }">
            <strong class="code-badge">{{ value }}</strong>
          </template>

          <template #cell-name="{ value }">
            {{ value }}
          </template>

          <template #cell-totalCredits="{ value }">
            {{ value }} créditos
          </template>

          <template #cell-isActive="{ value }">
            <UiBadge :tone="value ? 'success' : 'danger'">
              {{ value ? 'Activa' : 'Inactiva' }}
            </UiBadge>
          </template>

          <template #cell-actions="{ row }">
            <div class="table-actions">
              <UiButton
                variant="secondary"
                class="btn-sm"
                @click="openEditModal(row)"
              >
                Editar
              </UiButton>
              <UiButton
                variant="secondary"
                class="btn-sm"
                :loading="togglingId === row.id"
                @click="handleToggleStatus(row)"
              >
                {{ row.isActive ? 'Desactivar' : 'Activar' }}
              </UiButton>
              <UiButton
                variant="danger"
                class="btn-sm"
                @click="openDeleteModal(row)"
              >
                Eliminar
              </UiButton>
            </div>
          </template>
        </UiTable>

        <!-- Paginación -->
        <div v-if="totalPages > 1" class="pagination-bar">
          <span class="pagination-info">
            Página {{ currentPage }} de {{ totalPages }}
          </span>
          <div class="pagination-buttons">
            <UiButton
              variant="secondary"
              class="btn-sm"
              :disabled="currentPage <= 1"
              @click="currentPage--"
            >
              Anterior
            </UiButton>
            <UiButton
              variant="secondary"
              class="btn-sm"
              :disabled="currentPage >= totalPages"
              @click="currentPage++"
            >
              Siguiente
            </UiButton>
          </div>
        </div>
      </template>
    </UiCard>

    <!-- Modal de Creación / Edición -->
    <UiDialog v-model="dialogOpen" :title="dialogTitle">
      <form class="dialog-form" @submit.prevent="handleSave">
        <UiAlert v-if="formError" tone="danger" title="Error al guardar" class="mb-4">
          {{ formError }}
        </UiAlert>

        <UiField
          v-model="formCode"
          label="Clave (Siglas)"
          hint="Entre 1 y 16 caracteres alfanuméricos. Ej. ISC, INF, IND"
          required
        />

        <UiField
          v-model="formName"
          label="Nombre de la Carrera"
          hint="Nombre oficial conforme al plan de estudios"
          required
        />

        <UiField
          v-model="formCredits"
          label="Créditos Totales"
          type="number"
          min="1"
          max="500"
          hint="Número total de créditos (generalmente 260)"
          required
        />
      </form>

      <template #actions>
        <UiButton variant="secondary" :disabled="isSaving" @click="dialogOpen = false">
          Cancelar
        </UiButton>
        <UiButton variant="primary" :loading="isSaving" @click="handleSave">
          {{ isEditing ? 'Guardar Cambios' : 'Registrar Carrera' }}
        </UiButton>
      </template>
    </UiDialog>

    <!-- Modal de Confirmación de Eliminación -->
    <UiDialog v-model="deleteDialogOpen" title="Eliminar Carrera">
      <UiAlert v-if="deleteError" tone="danger" title="No se puede eliminar" class="mb-4">
        {{ deleteError }}
      </UiAlert>

      <p v-if="careerToDelete">
        ¿Estás seguro de que deseas dar de baja la carrera
        <strong>{{ careerToDelete.name }} ({{ careerToDelete.code }})</strong>?
      </p>

      <template #actions>
        <UiButton variant="secondary" :disabled="isDeleting" @click="deleteDialogOpen = false">
          Cancelar
        </UiButton>
        <UiButton variant="danger" :loading="isDeleting" @click="handleConfirmDelete">
          Confirmar Eliminación
        </UiButton>
      </template>
    </UiDialog>

    <!-- Notificaciones Toast -->
    <UiToast ref="toast" />
  </UiBasePage>
</template>

<style scoped>
.catalogs-card {
  padding: 24px;
}

.filter-bar {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 16px;
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 1px solid var(--color-border);
}

.filter-inputs {
  display: flex;
  align-items: flex-end;
  flex-wrap: wrap;
  gap: 16px;
  flex: 1;
}

.search-field {
  min-width: 260px;
  flex: 1;
}

.select-field {
  min-width: 200px;
}

.summary-text {
  font-size: 14px;
  color: var(--color-text-secondary);
  align-self: center;
}

.code-badge {
  font-family: monospace;
  font-size: 13px;
  color: var(--color-primary);
  background: var(--color-primary-subtle);
  padding: 2px 6px;
  border-radius: 4px;
}

.table-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.btn-sm {
  min-height: 32px;
  padding: 4px 10px;
  font-size: 12px;
}

.loading-state {
  text-align: center;
  padding: 40px;
  color: var(--color-text-secondary);
}

.pagination-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 20px;
  padding-top: 16px;
  border-top: 1px solid var(--color-border);
}

.pagination-info {
  font-size: 13px;
  color: var(--color-text-secondary);
}

.pagination-buttons {
  display: flex;
  gap: 8px;
}

.dialog-form {
  display: grid;
  gap: 12px;
  margin-top: 12px;
}

.mb-4 {
  margin-bottom: 16px;
}

@media (max-width: 640px) {
  .filter-bar {
    flex-direction: column;
    align-items: stretch;
  }
  .filter-inputs {
    flex-direction: column;
  }
  .search-field,
  .select-field {
    width: 100%;
  }
  .table-actions {
    flex-direction: column;
    align-items: stretch;
  }
}
</style>
