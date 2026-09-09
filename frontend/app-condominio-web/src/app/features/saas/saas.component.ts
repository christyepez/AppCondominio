import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { OrganizationOption, OrganizationOptionsService } from '../../core/data/organization-options.service';

interface Plan { id:string; code:string; name:string; maxUnits:number; maxUsers:number; maxStorageMb:number; price:number; currency:string; billingPeriod:number; modules:string[]; isActive:boolean; }

@Component({selector:'app-saas',standalone:true,imports:[CommonModule,FormsModule],template:`
<section class="page-heading"><div><span class="eyebrow">SaaS</span><h2>Planes y suscripciones</h2><p>Administración comercial, límites y entitlements de Conjunto al Día.</p></div><button class="button secondary" type="button" (click)="load()">Actualizar</button></section>
<div class="notice error" *ngIf="error">{{error}}</div><div class="notice success" *ngIf="message">{{message}}</div>
<section class="metric-grid" *ngIf="dashboard"><article class="metric" *ngFor="let item of dashboardEntries"><span>{{item.key}}</span><strong>{{item.value}}</strong></article></section>
<section class="panel"><h3>Nuevo plan comercial</h3><form class="form-grid" (ngSubmit)="createPlan()">
<label>Código<input [(ngModel)]="draft.code" name="code" required></label><label>Nombre<input [(ngModel)]="draft.name" name="name" required></label>
<label>Máx. unidades<input [(ngModel)]="draft.maxUnits" name="maxUnits" type="number" min="1" required></label><label>Máx. usuarios<input [(ngModel)]="draft.maxUsers" name="maxUsers" type="number" min="1" required></label>
<label>Storage MB<input [(ngModel)]="draft.maxStorageMb" name="maxStorageMb" type="number" min="1" required></label><label>Precio<input [(ngModel)]="draft.price" name="price" type="number" min="0" step="0.01" required></label>
<label>Moneda<input [(ngModel)]="draft.currency" name="currency" required></label><label>Período<select [(ngModel)]="draft.billingPeriod" name="billingPeriod"><option [ngValue]="1">Mensual</option><option [ngValue]="3">Trimestral</option><option [ngValue]="12">Anual</option></select></label>
<label class="wide">Módulos (separados por coma)<input [(ngModel)]="modulesCsv" name="modulesCsv" placeholder="billing,collections,maintenance"></label>
<div class="form-actions"><button class="button primary" type="submit" [disabled]="busy">Crear plan</button></div></form></section>
<section class="panel"><div class="panel-title"><h3>Planes</h3><span class="badge">{{plans.length}}</span></div><div class="table-wrap"><table *ngIf="plans.length"><thead><tr><th>Código</th><th>Nombre</th><th>Unidades</th><th>Usuarios</th><th>Precio</th><th>Período</th><th>Módulos</th></tr></thead><tbody><tr *ngFor="let p of plans"><td>{{p.code}}</td><td>{{p.name}}</td><td>{{p.maxUnits}}</td><td>{{p.maxUsers}}</td><td>{{p.price}} {{p.currency}}</td><td>{{periodLabel(p.billingPeriod)}}</td><td>{{p.modules.join(', ')}}</td></tr></tbody></table></div></section>
<section class="panel"><h3>Entitlements por organización</h3><div class="form-grid compact"><label class="wide">Organización<select [(ngModel)]="organizationId" name="organizationId"><option value="">Seleccione una organización</option><option *ngFor="let item of organizations" [value]="item.id">{{item.name}} ({{item.taxId || 'sin RUC'}})</option></select></label><div class="form-actions"><button class="button secondary" type="button" (click)="loadEntitlements()" [disabled]="busy||!organizationId">Consultar</button></div></div><pre class="result-box" *ngIf="entitlements">{{entitlements|json}}</pre></section>`})
export class SaasComponent implements OnInit{private readonly http=inject(HttpClient);private readonly organizationOptions=inject(OrganizationOptionsService);busy=false;error='';message='';plans:Plan[]=[];organizations:OrganizationOption[]=[];dashboard:Record<string,unknown>|null=null;entitlements:unknown;organizationId='';modulesCsv='billing,collections';draft={code:'',name:'',maxUnits:300,maxUsers:20,maxStorageMb:1024,price:0,currency:'USD',billingPeriod:1};
get dashboardEntries(){return Object.entries(this.dashboard??{}).filter(([,v])=>typeof v==='number').map(([key,value])=>({key,value}));}
ngOnInit():void{this.load();this.organizationOptions.load().subscribe({next:v=>this.organizations=v,error:e=>this.error=this.describe(e,'No se pudieron cargar las organizaciones.')});}
load():void{this.error='';this.http.get<Plan[]>(`${API_BASE_URL}/saas/plans`).subscribe({next:v=>this.plans=v,error:e=>this.error=this.describe(e,'No se pudieron cargar los planes.')});this.http.get<Record<string,unknown>>(`${API_BASE_URL}/saas/dashboard`).subscribe({next:v=>this.dashboard=v,error:()=>this.dashboard=null});}
createPlan():void{this.busy=true;this.error='';this.message='';const modules=this.modulesCsv.split(',').map(x=>x.trim()).filter(Boolean);this.http.post<Plan>(`${API_BASE_URL}/saas/plans`,{...this.draft,modules}).subscribe({next:v=>{this.busy=false;this.message=`Plan ${v.name} creado.`;this.draft={...this.draft,code:'',name:''};this.load();},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo crear el plan.');}})}
loadEntitlements():void{this.busy=true;this.error='';this.entitlements=undefined;this.http.get(`${API_BASE_URL}/saas/organizations/${this.organizationId}/entitlements`).subscribe({next:v=>{this.busy=false;this.entitlements=v;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudieron consultar los entitlements.');}})}
periodLabel(value:number):string{return value===1?'Mensual':value===3?'Trimestral':value===12?'Anual':String(value);}
private describe(e:{status?:number},fallback:string){return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos de SaaS.':fallback;}}
