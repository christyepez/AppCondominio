import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { API_BASE_URL } from '../../core/config/api.config';

interface CommunityDashboard {
  communities: number;
  activeCommunities: number;
  structureNodes: number;
  commonAreas: number;
  bankAccounts: number;
  authorities: number;
  documents: number;
}

interface SessionInfo {
  isAuthenticated: boolean;
  userId?: string | null;
  permissions: string[];
}

interface RuntimeInfo {
  service: string;
  version: string;
  status: string;
}
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page-heading">
      <div>
        <span class="eyebrow">Conjunto al Día</span>
        <h2>Dashboard administrativo</h2>
        <p>Estado operativo del producto y métricas autorizadas.</p>
      </div>
      <button class="button secondary" type="button" (click)="load()">Actualizar</button>
    </section>

    <section class="metric-grid">
      <article class="metric"><span>API</span><strong>{{ runtime?.status === 'ok' ? 'Operativa' : 'Sin respuesta' }}</strong></article>
      <article class="metric"><span>Live probe</span><strong>{{ liveHealthy ? 'Healthy' : 'No disponible' }}</strong></article>
      <article class="metric"><span>Versión API</span><strong>{{ runtime?.version || '—' }}</strong></article>
      <article class="metric"><span>Portal</span><strong>{{ session?.isAuthenticated ? 'Sesión activa' : 'Sin sesión' }}</strong></article>
    </section>

    <div class="notice info" *ngIf="session && !session.isAuthenticated">
      El runtime está disponible. Ingrese desde PortalCorporativo para consultar métricas y ejecutar operaciones protegidas.
    </div>
    <div class="notice error" *ngIf="runtimeError">{{ runtimeError }}</div>
    <div class="notice error" *ngIf="metricsError">{{ metricsError }}</div>
    <section class="metric-grid" *ngIf="dashboard">
      <article class="metric"><span>Comunidades</span><strong>{{ dashboard.communities }}</strong></article>
      <article class="metric"><span>Activas</span><strong>{{ dashboard.activeCommunities }}</strong></article>
      <article class="metric"><span>Áreas comunes</span><strong>{{ dashboard.commonAreas }}</strong></article>
      <article class="metric"><span>Cuentas bancarias</span><strong>{{ dashboard.bankAccounts }}</strong></article>
      <article class="metric"><span>Autoridades</span><strong>{{ dashboard.authorities }}</strong></article>
      <article class="metric"><span>Documentos</span><strong>{{ dashboard.documents }}</strong></article>
    </section>

    <section class="panel">
      <div class="panel-title">
        <h3>Estado de sesión</h3>
        <span class="badge" *ngIf="session">{{ session.isAuthenticated ? 'Autenticado' : 'Sin sesión' }}</span>
      </div>
      <p *ngIf="session?.userId"><strong>Usuario:</strong> <span class="mono">{{ session?.userId }}</span></p>
      <p *ngIf="session?.permissions?.length"><strong>Permisos:</strong> {{ session?.permissions?.join(', ') }}</p>
      <p *ngIf="session && !session.isAuthenticated">Las acciones administrativas permanecen protegidas por PortalCorporativo.</p>
    </section>
  `
})
export class DashboardComponent implements OnInit {
  private readonly http = inject(HttpClient);
  dashboard?: CommunityDashboard;
  session?: SessionInfo;
  runtime?: RuntimeInfo;
  liveHealthy = false;
  runtimeError = '';
  metricsError = '';

  ngOnInit(): void { this.load(); }

  load(): void {
    this.dashboard = undefined;
    this.runtimeError = '';
    this.metricsError = '';
    this.liveHealthy = false;

    this.http.get<RuntimeInfo>(`${API_BASE_URL}/`).subscribe({
      next: value => this.runtime = value,
      error: () => {
        this.runtime = undefined;
        this.runtimeError = 'No fue posible contactar la API de AppCondominio.';
      }
    });

    this.http.get(`${API_BASE_URL}/health/live`, { responseType: 'text' }).subscribe({
      next: () => this.liveHealthy = true,
      error: () => this.liveHealthy = false
    });
    this.http.get<SessionInfo>(`${API_BASE_URL}/session`).subscribe({
      next: value => {
        this.session = value;
        if (value.isAuthenticated) { this.loadMetrics(); }
      },
      error: (err: { status?: number }) => {
        if (err?.status === 401) {
          this.session = { isAuthenticated: false, permissions: [] };
          return;
        }
        this.session = undefined;
        if (!this.runtimeError) { this.runtimeError = 'No fue posible consultar el estado de sesión.'; }
      }
    });
  }

  private loadMetrics(): void {
    this.http.get<CommunityDashboard>(`${API_BASE_URL}/communities/dashboard`).subscribe({
      next: value => this.dashboard = value,
      error: (err: { status?: number }) => {
        this.dashboard = undefined;
        this.metricsError = err?.status === 403
          ? 'El usuario no tiene permiso appcondominio.communities.read.'
          : err?.status === 401
            ? 'La sesión expiró. Ingrese nuevamente desde PortalCorporativo.'
            : 'No se pudieron cargar las métricas administrativas.';
      }
    });
  }
}
