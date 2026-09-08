import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';

@Component({selector:'app-procurement',standalone:true,imports:[CommonModule,FormsModule],template:`
<section class="page-heading"><div><span class="eyebrow">Compras</span><h2>Requisiciones y cotizaciones</h2><p>Creación de requisiciones y consulta de ranking de ofertas.</p></div></section>
<div class="notice error" *ngIf="error">{{error}}</div><div class="notice success" *ngIf="message">{{message}}</div>
<section class="panel"><h3>Nueva requisición</h3><form class="form-grid" (ngSubmit)="createRequisition()">
<label>Community Id<input [(ngModel)]="req.communityId" name="communityId" required></label><label>Número<input [(ngModel)]="req.number" name="number" required></label>
<label class="wide">Descripción<input [(ngModel)]="req.description" name="description" required></label><label>Monto estimado<input [(ngModel)]="req.estimatedAmount" name="estimatedAmount" type="number" step="0.01" min="0" required></label>
<label>Solicitado por<input [(ngModel)]="req.requestedBy" name="requestedBy" required></label><div class="form-actions"><button class="button primary" type="submit" [disabled]="busy">Crear requisición</button></div></form></section>
<section class="panel"><h3>Ranking de ronda</h3><div class="form-grid compact"><label class="wide">Round Id<input [(ngModel)]="roundId" name="roundId"></label><div class="form-actions"><button class="button secondary" type="button" (click)="loadRanking()" [disabled]="busy||!roundId">Consultar</button></div></div><pre class="result-box" *ngIf="ranking">{{ranking|json}}</pre></section>`})
export class ProcurementComponent{private readonly http=inject(HttpClient);busy=false;error='';message='';ranking:unknown;roundId='';req={communityId:'',number:'',description:'',estimatedAmount:0,requestedBy:''};
createRequisition():void{this.busy=true;this.error='';this.message='';this.http.post<{id:string}>(`${API_BASE_URL}/procurement/requisitions`,this.req).subscribe({next:v=>{this.busy=false;this.message=`Requisición creada: ${v.id}`;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo crear la requisición.');}})}
loadRanking():void{this.busy=true;this.error='';this.ranking=undefined;this.http.get(`${API_BASE_URL}/procurement/rounds/${this.roundId}/ranking`).subscribe({next:v=>{this.busy=false;this.ranking=v;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo consultar el ranking.');}})}
private describe(e:{status?:number},fallback:string){return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos de Procurement.':fallback;}}
