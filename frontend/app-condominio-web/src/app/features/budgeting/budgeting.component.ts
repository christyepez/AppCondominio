import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { CommunityOption, CommunityOptionsService } from '../../core/data/community-options.service';

@Component({selector:'app-budgeting',standalone:true,imports:[CommonModule,FormsModule],template:`
<section class="page-heading"><div><span class="eyebrow">Presupuesto</span><h2>Planes y variaciones</h2><p>Creación de planes presupuestarios y consulta de ejecución.</p></div></section>
<div class="notice error" *ngIf="error">{{error}}</div><div class="notice success" *ngIf="message">{{message}}</div>
<section class="panel"><h3>Nuevo plan</h3><form class="form-grid" (ngSubmit)="createPlan()">
<label>Comunidad<select [(ngModel)]="plan.communityId" name="communityId" required><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label><label>Año<input [(ngModel)]="plan.year" name="year" type="number" required></label>
<label>Versión<input [(ngModel)]="plan.version" name="version" type="number" min="1" required></label><label>Nombre<input [(ngModel)]="plan.name" name="name" required></label>
<div class="form-actions"><button class="button primary" type="submit" [disabled]="busy">Crear plan</button></div></form></section>
<section class="panel"><h3>Variación presupuestaria</h3><div class="form-grid compact"><label class="wide">Plan Id<input [(ngModel)]="planId" name="planId"></label><div class="form-actions"><button class="button secondary" type="button" (click)="loadVariance()" [disabled]="busy||!planId">Consultar</button></div></div><pre class="result-box" *ngIf="variance">{{variance|json}}</pre></section>`})
export class BudgetingComponent implements OnInit{private readonly http=inject(HttpClient);private readonly communityOptions=inject(CommunityOptionsService);communities:CommunityOption[]=[];busy=false;error='';message='';variance:unknown;planId='';plan={communityId:'',year:new Date().getFullYear(),version:1,name:''};
ngOnInit():void{this.communityOptions.load().subscribe({next:items=>this.communities=items,error:()=>this.error='No se pudieron cargar las comunidades disponibles.'});}
createPlan():void{this.busy=true;this.error='';this.message='';this.http.post<{id:string}>(`${API_BASE_URL}/budgeting/plans`,this.plan).subscribe({next:v=>{this.busy=false;this.planId=v.id;this.message=`Plan creado: ${v.id}`;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo crear el plan.');}})}
loadVariance():void{this.busy=true;this.error='';this.variance=undefined;this.http.get(`${API_BASE_URL}/budgeting/plans/${this.planId}/variance`).subscribe({next:v=>{this.busy=false;this.variance=v;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo consultar la variación.');}})}
private describe(e:{status?:number},fallback:string){return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos de Budgeting.':fallback;}}
