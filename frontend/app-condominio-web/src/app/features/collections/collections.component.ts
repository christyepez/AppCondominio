import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';

@Component({
  selector: 'app-collections',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="page-heading">
      <div><span class="eyebrow">Cartera</span><h2>Cobranza</h2><p>Consulta de cartera/aging y registro de pagos.</p></div>
    </section>

    <div class="notice error" *ngIf="error">{{ error }}</div>
    <div class="notice success" *ngIf="message">{{ message }}</div>

    <section class="panel">
      <h3>Consulta de cartera</h3>
      <div class="form-grid">
        <label>Community Id<input [(ngModel)]="query.communityId" name="queryCommunity"></label>
        <label>Unit Id opcional<input [(ngModel)]="query.unitId" name="queryUnit"></label>
        <label>Fecha de corte<input [(ngModel)]="query.on" name="queryOn" type="date"></label>
        <div class="form-actions">
          <button class="button secondary" type="button" (click)="loadReceivables()" [disabled]="busy || !query.communityId">Ver cuentas por cobrar</button>
          <button class="button secondary" type="button" (click)="loadAging()" [disabled]="busy || !query.communityId">Ver aging</button>
        </div>
      </div>
      <pre class="result-box" *ngIf="result">{{ result | json }}</pre>
    </section>

    <section class="panel">
      <h3>Registrar pago</h3>
      <form class="form-grid" (ngSubmit)="registerPayment()">
        <label>Community Id<input [(ngModel)]="payment.communityId" name="paymentCommunity" required></label>
        <label>Referencia<input [(ngModel)]="payment.reference" name="reference" required></label>
        <label>Método
          <select [(ngModel)]="payment.method" name="method">
            <option [ngValue]="1">Efectivo</option><option [ngValue]="2">Transferencia</option><option [ngValue]="3">Tarjeta</option><option [ngValue]="4">Depósito</option><option [ngValue]="5">Cheque</option><option [ngValue]="99">Otro</option>
          </select>
        </label>
        <label>Fecha recepción<input [(ngModel)]="payment.receivedOn" name="receivedOn" type="date" required></label>
        <label>Monto<input [(ngModel)]="payment.amount" name="amount" type="number" min="0.01" step="0.01" required></label>
        <label>Transacción externa<input [(ngModel)]="payment.externalTransactionId" name="externalTransactionId"></label>
        <div class="form-actions"><button class="button primary" type="submit" [disabled]="busy">Registrar pago</button></div>
      </form>
    </section>
  `
})
export class CollectionsComponent {
  private readonly http = inject(HttpClient);
  busy = false;
  error = '';
  message = '';
  result: unknown;
  query = { communityId: '', unitId: '', on: '' };
  payment = { communityId: '', reference: '', method: 2, receivedOn: '', amount: 0, externalTransactionId: '' };

  loadReceivables(): void {
    const params = new URLSearchParams();
    if (this.query.unitId) params.set('unitId', this.query.unitId);
    if (this.query.on) params.set('on', this.query.on);
    this.load(`${API_BASE_URL}/collections/communities/${this.query.communityId}/receivables?${params.toString()}`);
  }

  loadAging(): void {
    const suffix = this.query.on ? `?on=${encodeURIComponent(this.query.on)}` : '';
    this.load(`${API_BASE_URL}/collections/communities/${this.query.communityId}/aging${suffix}`);
  }

  registerPayment(): void {
    this.busy = true; this.error = ''; this.message = '';
    const payload = { ...this.payment, externalTransactionId: this.payment.externalTransactionId || null };
    this.http.post<{ id: string }>(`${API_BASE_URL}/collections/payments`, payload).subscribe({
      next: value => { this.busy = false; this.message = `Pago registrado: ${value.id}`; },
      error: err => { this.busy = false; this.error = this.describe(err, 'No se pudo registrar el pago.'); }
    });
  }

  private load(url: string): void {
    this.busy = true; this.error = ''; this.message = ''; this.result = undefined;
    this.http.get(url).subscribe({
      next: value => { this.busy = false; this.result = value; },
      error: err => { this.busy = false; this.error = this.describe(err, 'No se pudo consultar cartera.'); }
    });
  }

  private describe(err: { status?: number }, fallback: string): string {
    if (err?.status === 401) return 'Sesión requerida desde PortalCorporativo.';
    if (err?.status === 403) return 'No tiene permisos de Collections.';
    return fallback;
  }
}
