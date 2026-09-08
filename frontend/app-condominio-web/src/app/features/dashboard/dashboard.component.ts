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

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page-heading">
      <div><span class="eyebrow">Conjunto al Día</span><h2>Dashboard administrativo</h2><p>Resumen operativo conectado a las APIs del producto.</p></div>
      <button class="button secondary" type="button" (click)="load()">Actualizar</button>
    </section>

    <div class="notice error" *ngIf="error">{{ error }}</div>
    <div class="notice info" *ngIf="session && !session.isAuthenticated">La aplicación requiere una sesión emitida por PortalCorporativo.</div>

    <section class="metric-grid" *ngIf="dashboard">
      <article class="metric"><span>Comunidades</span><strong>{{ dashboard.communities }}</strong></article>
      <article class="metric"><span>Activas</span><strong>{{ dashboard.activeCommunities }}</strong></article>
      <article class="metric"><span>Áreas comunes</span><strong>{{ dashboard.commonAreas }}</strong></article>
      <article class="metric"><span>Cuentas bancarias</span><strong>{{ dashboard.bankAccounts }}</strong></article>
      <article class="metric"><span>Autoridades</span><strong>{{ dashboard.authorities }}</strong></article>
      <article class="metric"><span>Documentos</span><strong>{{ dashboard.documents }}</strong></article>
    </section>

    <section class="panel">
      <div class="panel-title"><h3>Estado de sesión</h3><span class="badge" *ngIf="session">{{ session.isAuthenticated ? 'Autenticado' : 'Sin sesión' }}</span></div>
      <p *ngIf="session?.userId"><strong>Usuario:</strong> <span class="mono">{{ session?.userId }}</span></p>
      <p *ngIf="session?.permissions?.length"><strong>Permisos:</strong> {{ session?.permissions?.join(', ') }}</p>
      <p *ngIf="!session">No fue posible consultar la sesión actual.</p>
    </section>
  `
})
export class DashboardComponent implements OnInit {
  private readonly http = inject(HttpClient);
  dashboard?: CommunityDashboard;
  session?: SessionInfo;
  error = '';

  ngOnInit(): void { this.load(); }

  load(): void {
    this.error = '';
    this.http.get<SessionInfo>(`${API_BASE_URL}/session`).subscribe({
      next: value => this.session = value,
      error: () => this.session = undefined
    });
    this.http.get<CommunityDashboard>(`${API_BASE_URL}/communities/dashboard`).subscribe({
      next: value => this.dashboard = value,
      error: (err: { status?: number }) => {
        this.dashboard = undefined;
        this.error = err?.status === 401 ? 'Sesión requerida. Ingrese desde PortalCorporativo para visualizar métricas.' : err?.status === 403 ? 'El usuario no tiene permiso appcondominio.communities.read.' : 'No se pudo cargar el dashboard.';
      }
    });
  }
}
