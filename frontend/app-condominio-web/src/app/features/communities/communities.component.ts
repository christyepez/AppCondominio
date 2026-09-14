import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { OrganizationOption, OrganizationOptionsService } from '../../core/data/organization-options.service';

interface Community {
  id: string;
  organizationId: string;
  code: string;
  name: string;
  taxId?: string | null;
  address: string;
  type?: string | null;
  administrator?: string | null;
  president?: string | null;
  status: number | string;
}

@Component({
  selector: 'app-communities',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="page-heading">
      <div><span class="eyebrow">AdministraciÃ³n</span><h2>Comunidades</h2><p>GestiÃ³n del maestro de conjuntos y condominios.</p></div>
      <button class="button secondary" type="button" (click)="load()">Actualizar</button>
    </section>

    <div class="notice error" *ngIf="error">{{ error }}</div>
    <div class="notice success" *ngIf="message">{{ message }}</div>

    <section class="panel">
      <h3>Nueva comunidad</h3>
      <form class="form-grid" (ngSubmit)="create()">
        <label>Organización<select name="organizationId" [(ngModel)]="draft.organizationId" required><option value="">Seleccione una organización</option><option *ngFor="let organization of organizations" [value]="organization.id">{{ organization.name }} ({{ organization.taxId || 'sin RUC' }})</option></select></label>
        <label>CÃ³digo<input name="code" [(ngModel)]="draft.code" required maxlength="40"></label>
        <label>Nombre<input name="name" [(ngModel)]="draft.name" required maxlength="200"></label>
        <label>RUC<input name="taxId" [(ngModel)]="draft.taxId" maxlength="30"></label>
        <label class="wide">DirecciÃ³n<input name="address" [(ngModel)]="draft.address" required maxlength="300"></label>
        <div class="form-actions"><button class="button primary" type="submit" [disabled]="saving">{{ saving ? 'Guardandoâ€¦' : 'Crear comunidad' }}</button></div>
      </form>
    </section>

    <section class="panel">
      <div class="panel-title"><h3>Comunidades registradas</h3><span class="badge">{{ communities.length }}</span></div>
      <div class="table-wrap" *ngIf="communities.length; else empty">
        <table>
          <thead><tr><th>CÃ³digo</th><th>Nombre</th><th>RUC</th><th>DirecciÃ³n</th><th>Estado</th><th>Id</th></tr></thead>
          <tbody><tr *ngFor="let item of communities"><td>{{ item.code }}</td><td>{{ item.name }}</td><td>{{ item.taxId || 'â€”' }}</td><td>{{ item.address }}</td><td>{{ item.status }}</td><td class="mono">{{ item.id }}</td></tr></tbody>
        </table>
      </div>
      <ng-template #empty><p class="empty-state">No hay comunidades disponibles o el usuario no tiene permiso de lectura.</p></ng-template>
    </section>
  `
})
export class CommunitiesComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly organizationOptions = inject(OrganizationOptionsService);
  communities: Community[] = [];
  organizations: OrganizationOption[] = [];
  saving = false;
  error = '';
  message = '';
  draft = { organizationId: '', code: '', name: '', taxId: '', address: '' };

  ngOnInit(): void { this.loadOrganizations(); this.load(); }

  private loadOrganizations(): void {
    this.organizationOptions.load().subscribe({ next: items => this.organizations = items, error: () => this.error = 'No se pudieron cargar las organizaciones disponibles.' });
  }

  load(): void {
    this.error = '';
    this.http.get<Community[]>(`${API_BASE_URL}/communities/`).subscribe({
      next: items => this.communities = items,
      error: err => this.error = this.describeError(err, 'No se pudo cargar comunidades.')
    });
  }

  create(): void {
    if (!this.draft.organizationId || !this.draft.code.trim() || !this.draft.name.trim() || !this.draft.address.trim()) return;
    this.saving = true; this.error = ''; this.message = '';
    this.http.post<Community>(`${API_BASE_URL}/communities/`, { ...this.draft, taxId: this.draft.taxId || null }).subscribe({
      next: created => {
        this.saving = false;
        this.message = `Comunidad ${created.name} creada correctamente.`;
        this.draft = { organizationId: this.draft.organizationId, code: '', name: '', taxId: '', address: '' };
        this.load();
      },
      error: err => { this.saving = false; this.error = this.describeError(err, 'No se pudo crear la comunidad.'); }
    });
  }

  private describeError(err: { status?: number }, fallback: string): string {
    if (err?.status === 401) return 'SesiÃ³n requerida. El JWT debe ser emitido por PortalCorporativo.';
    if (err?.status === 403) return 'El usuario autenticado no tiene el permiso requerido.';
    return fallback;
  }
}
