import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { CommunityOption, CommunityOptionsService } from '../../core/data/community-options.service';
import { DomainOptionsService, ReservableAreaOption, UnitOption } from '../../core/data/domain-options.service';

@Component({
  selector: 'app-people',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="page-heading">
      <div><span class="eyebrow">Personas</span><h2>Propietarios y residentes</h2><p>Alta de persona natural y consulta de la composición de una unidad.</p></div>
    </section>

    <div class="notice error" *ngIf="error">{{ error }}</div>
    <div class="notice success" *ngIf="message">{{ message }}</div>

    <section class="panel">
      <h3>Nueva persona natural</h3>
      <form class="form-grid" (ngSubmit)="createNaturalPerson()">
        <label>Comunidad<select name="communityId" [(ngModel)]="person.communityId" required><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label>
        <label>Identificación<input name="identification" [(ngModel)]="person.identification" required></label>
        <label>Nombres<input name="names" [(ngModel)]="person.names" required></label>
        <label>Email<input name="email" [(ngModel)]="person.email" type="email"></label>
        <label>Teléfono<input name="phone" [(ngModel)]="person.phone"></label>
        <label>Fecha nacimiento<input name="birthDate" [(ngModel)]="person.birthDate" type="date"></label>
        <label class="wide">Dirección<input name="address" [(ngModel)]="person.address"></label>
        <div class="form-actions"><button class="button primary" type="submit" [disabled]="busy">Crear persona</button></div>
      </form>
    </section>

    <section class="panel">
      <h3>Ficha de ocupación por unidad</h3>
      <div class="form-grid compact">
        <label>Comunidad<select [(ngModel)]="lookup.communityId" name="lookupCommunity" (ngModelChange)="loadUnits($event)"><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label>
        <label>Unidad<select [(ngModel)]="lookup.unitId" name="lookupUnit"><option value="">Seleccione una unidad</option><option *ngFor="let unit of units" [value]="unit.id">{{ unit.code }} · {{ unit.location }}</option></select></label>
        <div class="form-actions"><button class="button secondary" type="button" (click)="loadUnit()" [disabled]="busy || !lookup.communityId || !lookup.unitId">Consultar</button></div>
      </div>
      <pre class="result-box" *ngIf="unitRecord">{{ unitRecord | json }}</pre>
    </section>
  `
})
export class PeopleComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly communityOptions = inject(CommunityOptionsService);
  private readonly domainOptions = inject(DomainOptionsService);
  communities: CommunityOption[] = [];
  units: UnitOption[] = [];
  busy = false;
  error = '';
  message = '';
  unitRecord: unknown;
  person = { communityId: '', identification: '', names: '', email: '', phone: '', birthDate: '', address: '' };
  lookup = { communityId: '', unitId: '' };

  ngOnInit(): void {
    this.communityOptions.load().subscribe({
      next: items => this.communities = items,
      error: () => this.error = 'No se pudieron cargar las comunidades disponibles.'
    });
  }

  loadUnits(communityId: string): void {
    this.units = []; this.lookup.unitId = "";
    if (!communityId) return;
    this.domainOptions.units(communityId).subscribe({ next: items => this.units = items, error: () => this.error = "No se pudieron cargar las unidades." });
  }

  createNaturalPerson(): void {
    if (!this.person.communityId || !this.person.identification.trim() || !this.person.names.trim()) return;
    this.busy = true; this.error = ''; this.message = '';
    const payload = {
      ...this.person,
      email: this.person.email || null,
      phone: this.person.phone || null,
      birthDate: this.person.birthDate || null,
      address: this.person.address || null
    };
    this.http.post<{ id: string }>(`${API_BASE_URL}/people/natural`, payload).subscribe({
      next: value => {
        this.busy = false;
        this.message = `Persona creada: ${value.id}`;
        this.person = { communityId: this.person.communityId, identification: '', names: '', email: '', phone: '', birthDate: '', address: '' };
      },
      error: err => { this.busy = false; this.error = this.describe(err, 'No se pudo crear la persona.'); }
    });
  }

  loadUnit(): void {
    this.busy = true; this.error = ''; this.message = ''; this.unitRecord = undefined;
    this.http.get(`${API_BASE_URL}/people/communities/${this.lookup.communityId}/units/${this.lookup.unitId}`).subscribe({
      next: value => { this.busy = false; this.unitRecord = value; },
      error: err => { this.busy = false; this.error = this.describe(err, 'No se pudo consultar la unidad.'); }
    });
  }

  private describe(err: { status?: number }, fallback: string): string {
    if (err?.status === 401) return 'Sesión requerida desde PortalCorporativo.';
    if (err?.status === 403) return 'No tiene permisos de People.';
    return fallback;
  }
}
