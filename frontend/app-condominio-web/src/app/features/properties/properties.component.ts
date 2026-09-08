import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';

@Component({
  selector: 'app-properties',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="page-heading">
      <div><span class="eyebrow">Operación</span><h2>Propiedades y alícuotas</h2><p>Validación de coeficientes y generación de datos piloto conectadas al backend real.</p></div>
    </section>

    <div class="notice error" *ngIf="error">{{ error }}</div>
    <div class="notice success" *ngIf="message">{{ message }}</div>

    <section class="panel form-grid compact">
      <label class="wide">Community Id<input [(ngModel)]="communityId" name="communityId" placeholder="GUID de la comunidad"></label>
      <div class="form-actions">
        <button class="button secondary" type="button" (click)="validate()" [disabled]="busy || !communityId">Validar alícuotas</button>
        <button class="button primary" type="button" (click)="generateDemo()" [disabled]="busy || !communityId">Generar piloto 300 unidades</button>
      </div>
    </section>

    <section class="panel" *ngIf="validation">
      <h3>Resultado de coeficientes</h3>
      <pre class="result-box">{{ validation | json }}</pre>
    </section>

    <section class="panel" *ngIf="demoResult">
      <h3>Resultado de generación piloto</h3>
      <pre class="result-box">{{ demoResult | json }}</pre>
    </section>
  `
})
export class PropertiesComponent {
  private readonly http = inject(HttpClient);
  communityId = '';
  busy = false;
  error = '';
  message = '';
  validation: unknown;
  demoResult: unknown;

  validate(): void {
    this.run(() => this.http.get(`${API_BASE_URL}/properties/communities/${this.communityId}/coefficient-validation`), value => {
      this.validation = value;
      this.message = 'Validación de coeficientes completada.';
    });
  }

  generateDemo(): void {
    this.run(() => this.http.post(`${API_BASE_URL}/properties/communities/${this.communityId}/demo`, { unitCount: 300 }), value => {
      this.demoResult = value;
      this.message = 'Generación piloto ejecutada. Revise el resultado antes de repetirla.';
    });
  }

  private run(request: () => ReturnType<HttpClient['get']>, onSuccess: (value: unknown) => void): void {
    this.busy = true; this.error = ''; this.message = '';
    request().subscribe({
      next: value => { this.busy = false; onSuccess(value); },
      error: (err: { status?: number }) => {
        this.busy = false;
        this.error = err?.status === 401 ? 'Sesión requerida desde PortalCorporativo.' : err?.status === 403 ? 'No tiene permisos de Properties.' : 'La operación no pudo completarse.';
      }
    });
  }
}
