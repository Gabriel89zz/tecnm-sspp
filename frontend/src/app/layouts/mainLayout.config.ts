import type { BrandOptions, NavigationItem } from '../../shared/components/navigation'

// Cambiar estas opciones por aplicación. Header y sidebar son independientes.
export const mainLayoutConfig = {
  showHeader: false,
  showSidebar: true,
  header: { title: 'TecNM', subtitle: 'Campus Monclova', showTitle: true, showLogo: false } satisfies BrandOptions,
  sidebar: { title: 'TecNM', subtitle: 'Plataforma institucional', showTitle: true, showLogo: false } satisfies BrandOptions,
}

export const headerNavigation: NavigationItem[] = [
  { label: 'Panel Principal', to: '/' },
  { label: 'Catálogos', children: [
    { label: 'Carreras', to: '/catalogs/careers' },
  ] },
  { label: 'Trámites Recientes', to: '/#tramites' },
  { label: 'Calendario', to: '/#calendario' },
]

export const sidebarNavigation: NavigationItem[] = [
  { label: 'Panel Principal', to: '/' },
  { label: 'Catálogos Institucionales', children: [
    { label: 'Carreras', to: '/catalogs/careers' },
  ] },
  { label: 'Gestión Operativa', children: [
    { label: 'Trámites en Curso', to: '/#tramites' },
    { label: 'Avisos & Calendario', to: '/#calendario' },
    { label: 'Acciones Rápidas', to: '/#accesos' },
  ] },
]
