import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';

@Component({selector:'app-security-operations',standalone:true,imports:[CommonModule,FormsModule],template:`
<section class="page-heading"><div><span class="eyebrow">Seguridad</span><h2>Visitas e incidentes</h2><p>Autorizaciones de ingreso y consulta de indicadores operativos.</p></div></section>
<div class="notice error" *ngIf="error">{{error}}</div><div class="notice success" *ngIf="message">{{message}}</div>
<section class="panel"><h3>Autorizar visita</h3><form class="form-grid" (ngSubmit)="authorizeVisit()">
<label>Community Id<input [(ngModel)]="visit.communityId" name="communityId" required></label><label>Host Person Id<input [(ngModel)]="visit.hostPersonId" name="hostPersonId" required></label>
<label>Visitante<input [(ngModel)]="visit.visitorName" name="visitorName" required></label><label>Documento<input [(ngModel)]="visit.document" name="document" required></label>
<label>Destino<input [(ngModel)]="visit.destination" name="destination" required></label><label>Desde<input [(ngModel)]="visit.validFrom" name="validFrom" type="datetime-local" required></label>
<label>Hasta<input [(ngModel)]="visit.validTo" name="validTo" type="datetime-local" required></label><div class="form-actions"><button class="button primary" type="submit" [disabled]="busy">Autorizar visita</button></div></form></section>
<section class="panel"><h3>KPI de seguridad</h3><div class="form-grid compact"><label class="wide">Community Id<input [(ngModel)]="kpiCommunityId" name="kpiCommunityId"></label><div class="form-actions"><button class="button secondary" type="button" (click)="loadKpi()" [disabled]="busy||!kpiCommunityId">Consultar</button></div></div><pre class="result-box" *ngIf="kpi">{{kpi|json}}</pre></section>`})
export class SecurityOperationsComponent{private readonly http=inject(HttpClient);busy=false;error='';message='';kpi:unknown;kpiCommunityId='';visit={communityId:'',hostPersonId:'',visitorName:'',document:'',destination:'',validFrom:'',validTo:''};
authorizeVisit():void{this.busy=true;this.error='';this.message='';const payload={...this.visit,validFrom:new Date(this.visit.validFrom).toISOString(),validTo:new Date(this.visit.validTo).toISOString()};this.http.post<{id:string}>(`${API_BASE_URL}/security-operations/visits`,payload).subscribe({next:v=>{this.busy=false;this.message=`Visita autorizada: ${v.id}`;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo autorizar la visita.');}})}
loadKpi():void{this.busy=true;this.error='';this.kpi=undefined;this.http.get(`${API_BASE_URL}/security-operations/communities/${this.kpiCommunityId}/kpi`).subscribe({next:v=>{this.busy=false;this.kpi=v;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo consultar el KPI.');}})}
private describe(e:{status?:number},fallback:string){return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos de SecurityOperations.':fallback;}}
