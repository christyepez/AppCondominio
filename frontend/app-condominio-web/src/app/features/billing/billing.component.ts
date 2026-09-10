import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { CommunityOption, CommunityOptionsService } from '../../core/data/community-options.service';
import { DomainOptionsService, ReservableAreaOption, UnitOption } from '../../core/data/domain-options.service';

@Component({
  selector: 'app-billing',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="page-heading">
      <div><span class="eyebrow">Finanzas</span><h2>Facturación</h2><p>Apertura de períodos mensuales y consulta de estado de cuenta por unidad.</p></div>
    </section>

    <div class="notice error" *ngIf="error">{{ error }}</div>
    <div class="notice success" *ngIf="message">{{ message }}</div>

    <section class="panel">
      <h3>Abrir período mensual</h3>
      <form class="form-grid" (ngSubmit)="openPeriod()">
        <label>Comunidad<select [(ngModel)]="period.communityId" name="periodCommunity" required><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label>
        <label>Año<input [(ngModel)]="period.year" name="year" type="number" required></label>
        <label>Mes<input [(ngModel)]="period.month" name="month" type="number" min="1" max="12" required></label>
        <label>Fecha emisión<input [(ngModel)]="period.issueDate" name="issueDate" type="date" required></label>
        <label>Fecha vencimiento<input [(ngModel)]="period.dueDate" name="dueDate" type="date" required></label>
        <div class="form-actions"><button class="button primary" type="submit" [disabled]="busy">Abrir período</button></div>
      </form>
    </section>

    <section class="panel">
      <h3>Estado de cuenta por unidad</h3>
      <div class="form-grid compact">
        <label>Comunidad<select [(ngModel)]="statement.communityId" name="statementCommunity" (ngModelChange)="loadUnits($event)"><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label>
        <label>Unidad<select [(ngModel)]="statement.unitId" name="statementUnit"><option value="">Seleccione una unidad</option><option *ngFor="let unit of units" [value]="unit.id">{{ unit.code }} · {{ unit.location }}</option></select></label>
        <div class="form-actions"><button class="button secondary" type="button" (click)="loadStatement()" [disabled]="busy || !statement.communityId || !statement.unitId">Consultar</button></div>
      </div>
      <pre class="result-box" *ngIf="statementResult">{{ statementResult | json }}</pre>
    </section>
  `
})
export class BillingComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly communityOptions = inject(CommunityOptionsService);
  private readonly domainOptions = inject(DomainOptionsService);
  communities: CommunityOption[] = [];
  units: UnitOption[] = [];
  busy = false;
  error = '';
  message = '';
  statementResult: unknown;
  period = { communityId: '', year: new Date().getFullYear(), month: new Date().getMonth() + 1, issueDate: '', dueDate: '' };
  statement = { communityId: '', unitId: '' };
ngOnInit():void{this.communityOptions.load().subscribe({next:items=>this.communities=items,error:()=>this.error='No se pudieron cargar las comunidades disponibles.'});}

  loadUnits(communityId: string): void {
    this.units = []; this.statement.unitId = "";
    if (!communityId) return;
    this.domainOptions.units(communityId).subscribe({ next: items => this.units = items, error: () => this.error = "No se pudieron cargar las unidades." });
  }

  openPeriod(): void {
    this.busy = true; this.error = ''; this.message = '';
    this.http.post<{ id: string }>(`${API_BASE_URL}/billing/monthly/periods`, this.period).subscribe({
      next: value => { this.busy = false; this.message = `Período creado: ${value.id}`; },
      error: err => { this.busy = false; this.error = this.describe(err, 'No se pudo abrir el período.'); }
    });
  }

  loadStatement(): void {
    this.busy = true; this.error = ''; this.message = ''; this.statementResult = undefined;
    this.http.get(`${API_BASE_URL}/billing/monthly/communities/${this.statement.communityId}/units/${this.statement.unitId}/statement`).subscribe({
      next: value => { this.busy = false; this.statementResult = value; },
      error: err => { this.busy = false; this.error = this.describe(err, 'No se pudo cargar el estado de cuenta.'); }
    });
  }

  private describe(err: { status?: number }, fallback: string): string {
    if (err?.status === 401) return 'Sesión requerida desde PortalCorporativo.';
    if (err?.status === 403) return 'No tiene permisos de Billing.';
    return fallback;
  }
}
