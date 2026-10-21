<script setup lang="ts">
import { isAxiosError } from 'axios'
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import {
  UiAlert,
  UiBadge,
  UiBasePage,
  UiButton,
  UiCallout,
  UiCard,
  UiMetric,
  UiTable,
  UiToast,
} from '../../../shared/components'
import { me } from '../api/authApi'
import type { UserResponse } from '../types/auth'

const router = useRouter()
const user = ref<UserResponse | null>(null)
const errorMessage = ref('')
const isLoading = ref(true)
const toast = ref<InstanceType<typeof UiToast> | null>(null)

const roleLabel = computed(() => {
  if (!user.value?.roles?.length) return 'Sin rol'
  const labels: Record<string, string> = {
    admin: 'Administrador del Sistema',
    student: 'Estudiante / Prestador',
    social_service_officer: 'Encargado(a) de Servicio Social',
    internship_coordinator: 'Coordinador(a) de Prácticas Profesionales',
    department_head: 'Jefe(a) de Departamento Académico',
  }
  return user.value.roles.map((r) => labels[r] || r).join(', ')
})

async function loadSession(): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''

  try {
    user.value = await me()
  } catch (error) {
    if (!isAxiosError(error) || error.response?.status !== 401) {
      errorMessage.value =
        'No fue posible consultar la sesión. Verifica tu conexión e intenta nuevamente.'
    }
  } finally {
    isLoading.value = false
  }
}

onMounted(loadSession)

function notify(message: string): void {
  toast.value?.show(message)
}

function navigateToCareers(): void {
  router.push('/catalogs/careers')
}

// Datos Ficticios de Trámites Recientes
const columns = [
  { key: 'student' as const, label: 'Estudiante / Control' },
  { key: 'career' as const, label: 'Carrera' },
  { key: 'type' as const, label: 'Trámite' },
  { key: 'company' as const, label: 'Dependencia / Empresa' },
  { key: 'status' as const, label: 'Estado del Trámite' },
  { key: 'actions' as const, label: 'Acciones' },
]

interface TramiteRow {
  id: number
  control: string
  name: string
  student: string
  career: string
  type: string
  company: string
  status: string
  tone: 'success' | 'warning' | 'danger' | 'info' | 'purple'
  date: string
  actions?: string
}

const recentTramites = ref<TramiteRow[]>([
  {
    id: 1,
    control: '21040182',
    name: 'Brandon Martínez Valdés',
    student: 'Brandon Martínez Valdés (21040182)',
    career: 'Ing. Informática',
    type: 'Servicio Social',
    company: 'ITSM · Centro de Cómputo',
    status: 'Reporte 2 Aprobado',
    tone: 'success',
    date: 'Hoy, 10:45',
  },
  {
    id: 2,
    control: '20040511',
    name: 'Karla Sofia González Ruiz',
    student: 'Karla Sofia González Ruiz (20040511)',
    career: 'Ing. Industrial',
    type: 'Prácticas Profesionales',
    company: 'Altos Hornos de México (AHMSA)',
    status: 'Carta Aceptación Pendiente',
    tone: 'warning',
    date: 'Ayer, 16:20',
  },
  {
    id: 3,
    control: '22040104',
    name: 'Daniel Ortiz Ramos',
    student: 'Daniel Ortiz Ramos (22040104)',
    career: 'Ing. Electromecánica',
    type: 'Servicio Social',
    company: 'Presidencia Municipal de Monclova',
    status: 'Solicitud Aprobada',
    tone: 'info',
    date: '19 Oct 2026',
  },
  {
    id: 4,
    control: '21040319',
    name: 'Mariana Torres Morales',
    student: 'Mariana Torres Morales (21040319)',
    career: 'Ing. Gestión Empresarial',
    type: 'Prácticas Profesionales',
    company: 'Grupo Industrial Monclova (GIMSA)',
    status: 'En Evaluación Final',
    tone: 'purple',
    date: '18 Oct 2026',
  },
  {
    id: 5,
    control: '19040772',
    name: 'José Alejandro Rivera Peña',
    student: 'José Alejandro Rivera Peña (19040772)',
    career: 'Ing. Electrónica',
    type: 'Servicio Social',
    company: 'Comisión Federal de Electricidad (CFE)',
    status: 'Reporte 1 Observado',
    tone: 'danger',
    date: '17 Oct 2026',
  },
  {
    id: 6,
    control: '22040055',
    name: 'Andrea Estefanía Ramos Luna',
    student: 'Andrea Estefanía Ramos Luna (22040055)',
    career: 'Ing. Energías Renovables',
    type: 'Servicio Social',
    company: 'ITSM · Depto. Ciencias Básicas',
    status: 'Activo (480 h en curso)',
    tone: 'success',
    date: '16 Oct 2026',
  },
])

// Distribución por Carreras
const careerStats = [
  { name: 'Ingeniería Industrial', count: 48, percentage: 34 },
  { name: 'Ingeniería Informática', count: 36, percentage: 25 },
  { name: 'Ingeniería en Gestión Empresarial', count: 28, percentage: 20 },
  { name: 'Ingeniería Electromecánica', count: 16, percentage: 11 },
  { name: 'Ingeniería Electrónica', count: 9, percentage: 6 },
  { name: 'Ingeniería en Energías Renovables', count: 5, percentage: 4 },
]
</script>

<template>
  <UiBasePage
    fluid
    :title="user ? `Hola, ${user.name}` : 'Panel Institucional'"
    description="Sistema de Servicio Social y Prácticas Profesionales · TecNM Campus Monclova"
  >
    <!-- Barra superior con estatus institucional y acciones -->
    <template #actions>
      <div class="header-actions">
        <span class="period-pill">
          Periodo Activo: <strong>Agosto – Diciembre 2026 (AD26)</strong>
        </span>
        <UiButton variant="primary" @click="navigateToCareers">
          Catálogo de Carreras →
        </UiButton>
      </div>
    </template>

    <!-- Estado de carga / error de cuenta -->
    <UiAlert v-if="isLoading" tone="info" title="Conectando al servidor..." class="mb-4" />
    <UiAlert v-else-if="errorMessage" tone="danger" title="Aviso de sesión" class="mb-4">
      {{ errorMessage }}
      <UiButton class="mt-2" @click="loadSession">Reintentar</UiButton>
    </UiAlert>

    <!-- Banner de Cuenta del Usuario -->
    <UiCard v-if="user" class="identity-card mb-4">
      <div class="user-strip">
        <div class="user-info">
          <span class="user-greeting">Sesión activa como:</span>
          <strong class="user-name">{{ user.name }}</strong>
          <span class="user-details">
            {{ user.email }} · Usuario: <code>{{ user.username }}</code>
          </span>
        </div>
        <div class="user-role-badge">
          <UiBadge tone="info">{{ roleLabel }}</UiBadge>
          <span class="normative-badge">ISO 9001:2015</span>
        </div>
      </div>
    </UiCard>

    <!-- Métricas Clave (KPIs) -->
    <div class="stats-grid mb-4">
      <UiCard class="stat-card" metric>
        <template #header>
          <UiBadge tone="success">+12% vs ciclo anterior</UiBadge>
        </template>
        <UiMetric
          label="Servicio Social Activo"
          value="142"
          description="Estudiantes en programa regular de 480 horas (Anexos XVIII a XXV)."
        />
      </UiCard>

      <UiCard class="stat-card" metric>
        <template #header>
          <UiBadge tone="info">En curso</UiBadge>
        </template>
        <UiMetric
          label="Prácticas Profesionales"
          value="58"
          description="Estudiantes vinculados a empresas acreditando 200 a 480 horas."
        />
      </UiCard>

      <UiCard class="stat-card" metric>
        <template #header>
          <UiBadge tone="warning">Atención prioritaria</UiBadge>
        </template>
        <UiMetric
          label="Reportes por Dictaminar"
          value="19"
          description="11 reportes bimestrales entregados y 8 solicitudes pendientes de firma."
        />
      </UiCard>

      <UiCard class="stat-card" metric>
        <template #header>
          <UiBadge tone="purple">100% validadas</UiBadge>
        </template>
        <UiMetric
          label="Empresas con Convenio"
          value="34"
          description="Unidades receptoras registradas en el directorio de Vinculación."
        />
      </UiCard>
    </div>

    <!-- Layout de Dos Columnas: Trámites Recientes + Panel Operativo -->
    <div class="dashboard-columns mb-4">
      <!-- Columna Principal: Tabla de Trámites -->
      <UiCard
        id="tramites"
        title="Trámites en Curso y Últimas Entregas"
        description="Monitoreo operativo de expedientes, solicitudes y reportes bimestrales."
        class="tramites-card"
      >
        <UiTable
          label="Seguimiento de Trámites"
          :columns="columns"
          :rows="recentTramites"
        >
          <template #cell-student="{ row }">
            <div class="student-cell">
              <strong>{{ row.name }}</strong>
              <small class="control-num">{{ row.control }}</small>
            </div>
          </template>

          <template #cell-career="{ value }">
            <span class="career-text">{{ value }}</span>
          </template>

          <template #cell-type="{ value }">
            <span class="type-text">{{ value }}</span>
          </template>

          <template #cell-company="{ value }">
            <span class="company-text">{{ value }}</span>
          </template>

          <template #cell-status="{ row }">
            <UiBadge :tone="row.tone">{{ row.status }}</UiBadge>
          </template>

          <template #cell-actions="{ row }">
            <UiButton
              variant="secondary"
              class="btn-xs"
              @click="notify(`Consultando expediente de ${row.name} (${row.control})...`)"
            >
              Ver Detalle
            </UiButton>
          </template>
        </UiTable>
      </UiCard>

      <!-- Columna Lateral: Calendario Normativo y Accesos Rápidos -->
      <div class="side-panel">
        <!-- Calendario y Fechas Límite -->
        <UiCard
          id="calendario"
          title="Avisos & Calendario Oficial"
          description="Hitos normativos del periodo AD26"
        >
          <div class="calendar-list">
            <UiCallout title="📅 15 de Noviembre 2026">
              Fecha límite para recepción del <strong>2do Reporte Bimestral</strong> de Servicio Social.
            </UiCallout>

            <UiAlert tone="warning" title="🩺 Validación IMSS Obligatoria">
              Verificar que los practicantes cuenten con Seguro Facultativo vigente antes de iniciar labores.
            </UiAlert>

            <UiCallout title="📋 01 de Diciembre 2026">
              Apertura del pre-registro institucional para el ciclo <strong>Enero – Junio 2027</strong>.
            </UiCallout>
          </div>
        </UiCard>

        <!-- Accesos Rápidos -->
        <UiCard id="accesos" title="Acciones Rápidas" class="mt-4">
          <div class="quick-actions">
            <UiButton variant="primary" class="full-width" @click="navigateToCareers">
              Gestionar Carreras Académicas
            </UiButton>
            <UiButton
              variant="secondary"
              class="full-width"
              @click="notify('El módulo de registro de empresas estará activo en la Fase 3.')"
            >
              + Nueva Empresa Receptora
            </UiButton>
            <UiButton
              variant="secondary"
              class="full-width"
              @click="notify('Generando Formato SEP/TecNM 911.10 preliminar en PDF...')"
            >
              Exportar Reporte SEP 911.10
            </UiButton>
            <UiButton
              variant="secondary"
              class="full-width"
              @click="notify('Consultando bitácora de auditoría ISO 9001...')"
            >
              Auditoría y Trazabilidad (ISO)
            </UiButton>
          </div>
        </UiCard>
      </div>
    </div>

    <!-- Sección Inferior: Distribución de Prestadores por Carrera -->
    <UiCard
      title="Distribución de Prestadores por Carrera"
      description="Padrón total de 142 alumnos activos en Servicio Social (Periodo AD26)"
      class="mb-4"
    >
      <div class="career-bars">
        <div v-for="career in careerStats" :key="career.name" class="career-bar-row">
          <div class="career-bar-labels">
            <span class="career-title">{{ career.name }}</span>
            <span class="career-numbers">
              <strong>{{ career.count }}</strong> alumnos ({{ career.percentage }}%)
            </span>
          </div>
          <div class="progress-track">
            <div
              class="progress-fill"
              :style="{ width: `${career.percentage}%` }"
              :aria-label="`${career.name}: ${career.count} alumnos`"
            />
          </div>
        </div>
      </div>
    </UiCard>

    <!-- Componente Toast de Notificación -->
    <UiToast ref="toast" />
  </UiBasePage>
</template>

<style scoped>
.header-actions {
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

.period-pill {
  font-size: 13px;
  color: var(--color-primary);
  background: var(--color-primary-subtle);
  border: 1px solid var(--color-border);
  padding: 6px 14px;
  border-radius: 99px;
}

.identity-card {
  border-left: 4px solid var(--color-accent);
}

.user-strip {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.user-greeting {
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-secondary);
  display: block;
}

.user-name {
  font-size: 20px;
  color: var(--color-text-primary);
  display: block;
  margin-top: 2px;
}

.user-details {
  font-size: 13px;
  color: var(--color-text-secondary);
  margin-top: 4px;
  display: block;
}

.user-details code {
  color: var(--color-primary);
  font-weight: 600;
}

.user-role-badge {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.normative-badge {
  font-size: 11px;
  font-weight: 600;
  color: var(--color-accent);
  background: var(--color-accent-light);
  padding: 3px 8px;
  border-radius: 4px;
  border: 1px solid var(--color-accent);
}

/* Grilla de Métricas */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
}

.stat-card {
  border-top: 4px solid var(--color-accent);
}

/* Grilla de Columnas */
.dashboard-columns {
  display: grid;
  grid-template-columns: minmax(0, 1.8fr) minmax(0, 1.2fr);
  gap: 20px;
  align-items: start;
}

.tramites-card {
  padding: 24px;
}

.student-cell {
  display: flex;
  flex-direction: column;
}

.control-num {
  font-family: monospace;
  font-size: 11px;
  color: var(--color-primary);
  margin-top: 2px;
}

.career-text,
.type-text,
.company-text {
  font-size: 13px;
}

.btn-xs {
  min-height: 28px;
  padding: 2px 8px;
  font-size: 11px;
}

/* Panel Lateral */
.side-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.calendar-list {
  display: grid;
  gap: 12px;
}

.quick-actions {
  display: grid;
  gap: 10px;
}

.full-width {
  width: 100%;
  justify-content: center;
  text-align: center;
}

/* Barras de Progreso de Carreras */
.career-bars {
  display: grid;
  gap: 16px;
  margin-top: 8px;
}

.career-bar-row {
  display: grid;
  gap: 6px;
}

.career-bar-labels {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
}

.career-title {
  color: var(--color-text-primary);
  font-weight: 500;
}

.career-numbers {
  color: var(--color-text-secondary);
  font-size: 12px;
}

.progress-track {
  width: 100%;
  height: 8px;
  background: var(--color-gray-100);
  border-radius: 99px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: var(--color-primary);
  border-radius: 99px;
  transition: width 400ms ease;
}

/* Espaciadores auxiliares */
.mb-4 {
  margin-bottom: 20px;
}

.mt-2 {
  margin-top: 8px;
}

.mt-4 {
  margin-top: 16px;
}

@media (max-width: 1100px) {
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  .dashboard-columns {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }
  .header-actions {
    flex-direction: column;
    align-items: stretch;
  }
  .period-pill {
    text-align: center;
  }
  .user-strip {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
