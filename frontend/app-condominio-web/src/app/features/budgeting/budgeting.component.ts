import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { CommunityOption, CommunityOptionsService } from '../../core/data/community-options.service';
import { BudgetPlanOption, FinanceOptionsService } from '../../core/data/finance-options.service';

@Component({selector:'app-budgeting',standalone:true,imports:[CommonModule,FormsModule],template:`
<section class="page-heading"><div><span class="eyebrow">Presupuesto</span><h2>Planes y variaciones</h2><p>Creación de planes presupuestarios y consulta de ejecución.</p></div></section>
<div class="notice error" *ngIf="error">{{error}}</div><div class="notice success" *ngIf="message">{{message}}</div>
<section class="panel"><h3>Nuevo plan</h3><form class="form-grid" (ngSubmit)="createPlan()">
<label>Comunidad<select [(ngModel)]="plan.communityId" name="communityId" (ngModelChange)="loadPlans($event)" required><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label><label>Año<input [(ngModel)]="plan.year" name="year" type="number" required></label>
<label>Versión<input [(ngModel)]="plan.version" name="version" type="number" min="1" required></label><label>Nombre<input [(ngModel)]="plan.name" name="name" required></label>
<div class="form-actions"><button class="button primary" type="submit" [disabled]="busy">Crear plan</button></div></form></section>
<section class="panel"><h3>Variación presupuestaria</h3><div class="form-grid compact"><label>Comunidad<select [(ngModel)]="varianceCommunityId" name="varianceCommunityId" (ngModelChange)="loadPlans($event)"><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label><label>Plan<select [(ngModel)]="planId" name="planId"><option value="">Seleccione un plan</option><option *ngFor="let item of plans" [value]="item.id">{{item.year}} v{{item.version}} · {{item.name}}</option></select></label><div class="form-actions"><button class="button secondary" type="button" (click)="loadVariance()" [disabled]="busy||!planId">Consultar</button></div></div><pre class="result-box" *ngIf="variance">{{variance|json}}</pre></section>`})
export class BudgetingComponent implements OnInit{private readonly http=inject(HttpClient);private readonly communityOptions=inject(CommunityOptionsService);private readonly financeOptions=inject(FinanceOptionsService);communities:CommunityOption[]=[];plans:BudgetPlanOption[]=[];busy=false;error='';message='';variance:unknown;varianceCommunityId='';planId='';plan={communityId:'',year:new Date().getFullYear(),version:1,name:''};
ngOnInit():void{this.communityOptions.load().subscribe({next:items=>this.communities=items,error:()=>this.error='No se pudieron cargar las comunidades disponibles.'});}
loadPlans(communityId:string):void{this.plans=[];this.planId='';if(!communityId)return;this.financeOptions.loadBudgetPlans(communityId).subscribe({next:v=>this.plans=v,error:e=>this.error=this.describe(e,'No se pudieron cargar los planes.')});}
createPlan():void{this.busy=true;this.error='';this.message='';this.http.post<{id:string}>(`${API_BASE_URL}/budgeting/plans`,this.plan).subscribe({next:v=>{this.busy=false;this.planId=v.id;this.varianceCommunityId=this.plan.communityId;this.message=`Plan creado: ${v.id}`;this.loadPlans(this.plan.communityId);},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo crear el plan.');}})}
loadVariance():void{this.busy=true;this.error='';this.variance=undefined;this.http.get(`${API_BASE_URL}/budgeting/plans/${this.planId}/variance`).subscribe({next:v=>{this.busy=false;this.variance=v;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo consultar la variación.');}})}
private describe(e:{status?:number},fallback:string){return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos de Budgeting.':fallback;}}
