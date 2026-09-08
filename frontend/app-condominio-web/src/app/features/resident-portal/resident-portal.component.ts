import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';

interface ResidentAccess {
  communityId: string;
  unitId: string;
  personId: string;
  displayName: string;
  role: number;
  startsAtUtc: string;
  expiresAtUtc?: string | null;
}

@Component({
  selector: 'app-resident-portal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="resident-hero">
      <div><span class="eyebrow">Mi comunidad</span><h2>Hola{{ residentName ? ', ' + residentName : '' }}</h2><p>Consulta tu estado de cuenta, solicita reservas y autoriza visitas desde un único lugar.</p></div>
      <span class="resident-security">Acceso validado por PortalCorporativo</span>
    </section>

    <div class="notice error" *ngIf="error">{{ error }}</div>
    <div class="notice success" *ngIf="message">{{ message }}</div>

    <section class="panel" *ngIf="accesses.length">
      <div class="panel-title"><h3>Mis unidades</h3><span class="badge">{{ accesses.length }}</span></div>
      <div class="resident-unit-grid">
        <button type="button" class="resident-unit" *ngFor="let access of accesses" [class.selected]="access.unitId === selectedUnitId" (click)="selectUnit(access.unitId)">
          <strong>Unidad</strong><span>{{ access.unitId | slice:0:8 }}</span><small>{{ roleLabel(access.role) }}</small>
        </button>
      </div>
    </section>

    <section class="metric-grid" *ngIf="selectedUnitId">
      <article class="metric"><span>Saldo pendiente</span><strong>{{ outstandingTotal | currency:'USD':'symbol':'1.2-2' }}</strong></article>
      <article class="metric"><span>Obligaciones abiertas</span><strong>{{ receivables.length }}</strong></article>
      <article class="metric"><span>Estado</span><strong class="resident-status">Activo</strong></article>
      <article class="metric"><span>Unidad</span><strong class="resident-unit-code">{{ selectedUnitId | slice:0:8 }}</strong></article>
    </section>

    <section class="panel" *ngIf="selectedUnitId">
      <div class="panel-title"><h3>Estado de cuenta</h3><button type="button" class="button secondary" (click)="loadStatement()" [disabled]="busy">Actualizar</button></div>
      <div class="table-wrap" *ngIf="receivables.length; else noReceivables">
        <table><thead><tr><th>Vencimiento</th><th>Original</th><th>Pendiente</th><th>Estado</th></tr></thead>
          <tbody><tr *ngFor="let row of receivables"><td>{{ row.dueOn }}</td><td>{{ row.originalAmount | currency:'USD' }}</td><td>{{ row.outstandingAmount | currency:'USD' }}</td><td>{{ row.status }}</td></tr></tbody>
        </table>
      </div>
      <ng-template #noReceivables><p class="empty-state">No hay obligaciones pendientes para esta unidad.</p></ng-template>
    </section>

    <div class="resident-action-grid" *ngIf="selectedUnitId">
      <section class="panel">
        <h3>Reservar área comunal</h3>
        <form class="form-grid" (ngSubmit)="requestReservation()">
          <label class="wide">Área Id<input name="areaId" [(ngModel)]="reservation.areaId" required placeholder="Seleccione o ingrese el área"></label>
          <label>Desde<input type="datetime-local" name="startsAt" [(ngModel)]="reservation.startsAt" required></label>
          <label>Hasta<input type="datetime-local" name="endsAt" [(ngModel)]="reservation.endsAt" required></label>
          <label>Invitados<input type="number" min="1" name="guests" [(ngModel)]="reservation.guests" required></label>
          <div class="form-actions"><button class="button primary" [disabled]="busy">Solicitar reserva</button></div>
        </form>
      </section>

      <section class="panel">
        <h3>Autorizar visita</h3>
        <form class="form-grid" (ngSubmit)="authorizeVisit()">
          <label>Visitante<input name="visitorName" [(ngModel)]="visit.visitorName" required></label>
          <label>Documento<input name="document" [(ngModel)]="visit.document" required></label>
          <label class="wide">Destino<input name="destination" [(ngModel)]="visit.destination" required></label>
          <label>Válido desde<input type="datetime-local" name="validFrom" [(ngModel)]="visit.validFrom" required></label>
          <label>Válido hasta<input type="datetime-local" name="validTo" [(ngModel)]="visit.validTo" required></label>
          <div class="form-actions"><button class="button primary" [disabled]="busy">Autorizar visita</button></div>
        </form>
      </section>
    </div>
  `,
  styles: [`
    .resident-hero{display:flex;justify-content:space-between;gap:20px;align-items:flex-start;margin-bottom:24px}.resident-hero h2{font-size:34px;margin:4px 0 6px}.resident-hero p{margin:0;color:#746b7d;max-width:680px}.resident-security{background:#eaf7ef;color:#206c3b;border:1px solid #c9e8d4;border-radius:999px;padding:8px 12px;font-size:12px;font-weight:700}.resident-unit-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(170px,1fr));gap:12px}.resident-unit{border:1px solid #ddd3e5;background:#fff;border-radius:12px;padding:15px;text-align:left;cursor:pointer;display:grid;gap:4px}.resident-unit.selected{border-color:#6d3d91;box-shadow:0 0 0 2px #eadcf4}.resident-unit span{font-family:Consolas,monospace}.resident-unit small{color:#746b7d}.resident-action-grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:20px}.resident-status{font-size:22px!important;color:#237a43}.resident-unit-code{font-size:22px!important;font-family:Consolas,monospace}@media(max-width:850px){.resident-hero{flex-direction:column}.resident-action-grid{grid-template-columns:1fr}}
  `]
})
export class ResidentPortalComponent implements OnInit {
  private readonly http = inject(HttpClient);
  accesses: ResidentAccess[] = [];
  selectedUnitId = '';
  receivables: Array<{ dueOn: string; originalAmount: number; outstandingAmount: number; status: number }> = [];
  busy = false;
  error = '';
  message = '';
  reservation = { areaId: '', startsAt: '', endsAt: '', guests: 1 };
  visit = { visitorName: '', document: '', destination: '', validFrom: '', validTo: '' };

  get residentName(): string { return this.accesses[0]?.displayName ?? ''; }
  get outstandingTotal(): number { return this.receivables.reduce((sum, row) => sum + Number(row.outstandingAmount || 0), 0); }

  ngOnInit(): void { this.loadContext(); }

  loadContext(): void {
    this.busy = true; this.error = '';
    this.http.get<ResidentAccess[]>(`${API_BASE_URL}/resident/me`).subscribe({
      next: rows => { this.busy = false; this.accesses = rows; if (rows.length) this.selectUnit(rows[0].unitId); },
      error: err => { this.busy = false; this.error = this.errorMessage(err?.status); }
    });
  }

  selectUnit(unitId: string): void { this.selectedUnitId = unitId; this.loadStatement(); }

  loadStatement(): void {
    if (!this.selectedUnitId) return;
    this.busy = true; this.error = '';
    this.http.get<{ receivables: typeof this.receivables }>(`${API_BASE_URL}/resident/units/${this.selectedUnitId}/statement`).subscribe({
      next: result => { this.busy = false; this.receivables = result.receivables ?? []; },
      error: err => { this.busy = false; this.error = this.errorMessage(err?.status); }
    });
  }

  requestReservation(): void {
    if (!this.selectedUnitId) return;
    this.busy = true; this.error = ''; this.message = '';
    this.http.post(`${API_BASE_URL}/resident/units/${this.selectedUnitId}/reservations`, {
      areaId: this.reservation.areaId,
      startsAt: new Date(this.reservation.startsAt).toISOString(),
      endsAt: new Date(this.reservation.endsAt).toISOString(),
      guests: this.reservation.guests
    }).subscribe({ next: () => { this.busy = false; this.message = 'Reserva solicitada correctamente.'; }, error: err => { this.busy = false; this.error = this.errorMessage(err?.status); } });
  }

  authorizeVisit(): void {
    if (!this.selectedUnitId) return;
    this.busy = true; this.error = ''; this.message = '';
    this.http.post(`${API_BASE_URL}/resident/units/${this.selectedUnitId}/visits`, {
      visitorName: this.visit.visitorName, document: this.visit.document, destination: this.visit.destination,
      validFrom: new Date(this.visit.validFrom).toISOString(), validTo: new Date(this.visit.validTo).toISOString()
    }).subscribe({ next: () => { this.busy = false; this.message = 'Visita autorizada correctamente.'; }, error: err => { this.busy = false; this.error = this.errorMessage(err?.status); } });
  }

  roleLabel(role: number): string { return ({1:'Propietario residente',2:'Arrendatario',3:'Familiar',4:'Dependiente',5:'Personal de servicio'} as Record<number,string>)[role] ?? 'Residente'; }
  private errorMessage(status?: number): string { return status === 401 ? 'Debe iniciar sesión mediante PortalCorporativo.' : status === 403 ? 'Su usuario no tiene habilitado el Portal Residente.' : 'No fue posible completar la operación.'; }
}
