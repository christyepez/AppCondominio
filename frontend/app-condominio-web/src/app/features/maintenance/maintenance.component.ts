import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';

@Component({selector:'app-maintenance',standalone:true,imports:[CommonModule,FormsModule],template:`
<section class="page-heading"><div><span class="eyebrow">Mantenimiento</span><h2>Activos y órdenes</h2><p>Registro de activos y consulta de indicadores de mantenimiento.</p></div></section>
<div class="notice error" *ngIf="error">{{error}}</div><div class="notice success" *ngIf="message">{{message}}</div>
<section class="panel"><h3>Nuevo activo</h3><form class="form-grid" (ngSubmit)="createAsset()">
<label>Community Id<input [(ngModel)]="asset.communityId" name="communityId" required></label><label>Código<input [(ngModel)]="asset.code" name="code" required></label>
<label>Nombre<input [(ngModel)]="asset.name" name="name" required></label><label>Categoría<input [(ngModel)]="asset.category" name="category" required></label>
<label class="wide">Ubicación<input [(ngModel)]="asset.location" name="location" required></label><div class="form-actions"><button class="button primary" type="submit" [disabled]="busy">Crear activo</button></div></form></section>
<section class="panel"><h3>KPI de mantenimiento</h3><div class="form-grid"><label>Community Id<input [(ngModel)]="kpi.communityId" name="kpiCommunity"></label><label>Fecha<input [(ngModel)]="kpi.asOf" name="asOf" type="date"></label><div class="form-actions"><button class="button secondary" type="button" (click)="loadKpi()" [disabled]="busy||!kpi.communityId">Consultar</button></div></div><pre class="result-box" *ngIf="kpiResult">{{kpiResult|json}}</pre></section>`})
export class MaintenanceComponent{private readonly http=inject(HttpClient);busy=false;error='';message='';kpiResult:unknown;asset={communityId:'',code:'',name:'',category:'',location:''};kpi={communityId:'',asOf:''};
createAsset():void{this.busy=true;this.error='';this.message='';this.http.post<{id:string}>(`${API_BASE_URL}/maintenance/assets`,this.asset).subscribe({next:v=>{this.busy=false;this.message=`Activo creado: ${v.id}`;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo crear el activo.');}})}
loadKpi():void{this.busy=true;this.error='';this.kpiResult=undefined;const q=this.kpi.asOf?`?asOf=${encodeURIComponent(this.kpi.asOf)}`:'';this.http.get(`${API_BASE_URL}/maintenance/communities/${this.kpi.communityId}/kpi${q}`).subscribe({next:v=>{this.busy=false;this.kpiResult=v;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo consultar el KPI.');}})}
private describe(e:{status?:number},fallback:string){return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos de Maintenance.':fallback;}}
