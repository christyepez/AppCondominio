import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { CommunityOption, CommunityOptionsService } from '../../core/data/community-options.service';

interface GovernanceKpi {
  openAssemblies: number;
  approvedMotions: number;
  openCases: number;
  resolvedPenalties: number;
}

@Component({
  selector: 'app-governance',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="page-heading">
      <div><span class="eyebrow">Convivencia</span><h2>Gobernanza</h2><p>Indicadores de asambleas, decisiones y casos de convivencia.</p></div>
    </section>

    <div class="notice error" *ngIf="error">{{ error }}</div>
    <section class="panel form-grid compact">
      <label class="wide">Comunidad<select [(ngModel)]="communityId" name="communityId"><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label>
      <div class="form-actions"><button class="button primary" type="button" (click)="load()" [disabled]="loading || !communityId">Consultar KPI</button></div>
    </section>

    <section class="metric-grid" *ngIf="kpi">
      <article class="metric"><span>Asambleas abiertas</span><strong>{{ kpi.openAssemblies }}</strong></article>
      <article class="metric"><span>Mociones aprobadas</span><strong>{{ kpi.approvedMotions }}</strong></article>
      <article class="metric"><span>Casos abiertos</span><strong>{{ kpi.openCases }}</strong></article>
      <article class="metric"><span>Sanciones resueltas</span><strong>{{ kpi.resolvedPenalties | number:'1.2-2' }}</strong></article>
    </section>
  `
})
export class GovernanceComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly communityOptions = inject(CommunityOptionsService);
  communities: CommunityOption[] = [];
  communityId = '';
  loading = false;
  error = '';
  kpi?: GovernanceKpi;

  ngOnInit(): void {
    this.communityOptions.load().subscribe({
      next: items => this.communities = items,
      error: () => this.error = 'No se pudieron cargar las comunidades disponibles.'
    });
  }

  load(): void {
    this.loading = true; this.error = ''; this.kpi = undefined;
    this.http.get<GovernanceKpi>(`${API_BASE_URL}/governance/communities/${this.communityId}/kpi`).subscribe({
      next: value => { this.loading = false; this.kpi = value; },
      error: (err: { status?: number }) => {
        this.loading = false;
        this.error = err?.status === 401 ? 'Sesión requerida desde PortalCorporativo.' : err?.status === 403 ? 'No tiene permiso appcondominio.governance.read.' : 'No se pudo consultar el KPI de gobernanza.';
      }
    });
  }
}
