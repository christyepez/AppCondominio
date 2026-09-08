import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { CommunityOption, CommunityOptionsService } from '../../core/data/community-options.service';

@Component({selector:'app-treasury',standalone:true,imports:[CommonModule,FormsModule],template:`
<section class="page-heading"><div><span class="eyebrow">Tesorería</span><h2>Cuentas por pagar y caja</h2><p>Registro de obligaciones a proveedores y proyección de caja.</p></div></section>
<div class="notice error" *ngIf="error">{{error}}</div><div class="notice success" *ngIf="message">{{message}}</div>
<section class="panel"><h3>Nueva cuenta por pagar</h3><form class="form-grid" (ngSubmit)="createPayable()">
<label>Comunidad<select [(ngModel)]="payable.communityId" name="communityId" required><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label><label>Supplier Id<input [(ngModel)]="payable.supplierId" name="supplierId" required></label>
<label>Documento<input [(ngModel)]="payable.documentNumber" name="documentNumber" required></label><label>Fecha documento<input [(ngModel)]="payable.documentDate" name="documentDate" type="date" required></label>
<label>Vencimiento<input [(ngModel)]="payable.dueDate" name="dueDate" type="date" required></label><label>Monto<input [(ngModel)]="payable.amount" name="amount" type="number" step="0.01" min="0.01" required></label>
<label>Cuenta gasto<input [(ngModel)]="payable.expenseAccount" name="expenseAccount" required></label><label>Cuenta por pagar<input [(ngModel)]="payable.payableAccount" name="payableAccount" required></label>
<div class="form-actions"><button class="button primary" type="submit" [disabled]="busy">Registrar obligación</button></div></form></section>
<section class="panel"><h3>Proyección de caja</h3><div class="form-grid compact"><label class="wide">Comunidad<select [(ngModel)]="forecastCommunityId" name="forecastCommunityId"><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label><div class="form-actions"><button class="button secondary" type="button" (click)="loadForecast()" [disabled]="busy||!forecastCommunityId">Consultar</button></div></div><pre class="result-box" *ngIf="forecast">{{forecast|json}}</pre></section>`})
export class TreasuryComponent implements OnInit{private readonly http=inject(HttpClient);private readonly communityOptions=inject(CommunityOptionsService);communities:CommunityOption[]=[];busy=false;error='';message='';forecast:unknown;forecastCommunityId='';payable={communityId:'',supplierId:'',documentNumber:'',documentDate:'',dueDate:'',amount:0,expenseAccount:'',payableAccount:''};
ngOnInit():void{this.communityOptions.load().subscribe({next:items=>this.communities=items,error:()=>this.error='No se pudieron cargar las comunidades disponibles.'});}
createPayable():void{this.busy=true;this.error='';this.message='';this.http.post<{id:string}>(`${API_BASE_URL}/treasury/payables`,this.payable).subscribe({next:v=>{this.busy=false;this.message=`Obligación creada: ${v.id}`;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo crear la obligación.');}})}
loadForecast():void{this.busy=true;this.error='';this.forecast=undefined;this.http.get(`${API_BASE_URL}/treasury/communities/${this.forecastCommunityId}/cash-forecast`).subscribe({next:v=>{this.busy=false;this.forecast=v;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo consultar caja.');}})}
private describe(e:{status?:number},fallback:string){return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos de Treasury.':fallback;}}
