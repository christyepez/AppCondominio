import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { CommunityOption, CommunityOptionsService } from '../../core/data/community-options.service';

interface TreasurySupplierOption { id: string; taxId: string; legalName: string; }

@Component({selector:'app-treasury',standalone:true,imports:[CommonModule,FormsModule],template:`
<section class="page-heading"><div><span class="eyebrow">Tesorería</span><h2>Cuentas por pagar y caja</h2><p>Registro de obligaciones a proveedores y proyección de caja.</p></div></section>
<div class="notice error" *ngIf="error">{{error}}</div><div class="notice success" *ngIf="message">{{message}}</div>
<section class="panel"><h3>Nueva cuenta por pagar</h3><form class="form-grid" (ngSubmit)="createPayable()">
<label>Comunidad<select [(ngModel)]="payable.communityId" name="communityId" required (ngModelChange)="loadSuppliers($event)"><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label><label>Proveedor<select [(ngModel)]="payable.supplierId" name="supplierId" required [disabled]="!payable.communityId"><option value="">Seleccione un proveedor</option><option *ngFor="let supplier of suppliers" [value]="supplier.id">{{ supplier.legalName }} ({{ supplier.taxId }})</option></select></label>
<label>Documento<input [(ngModel)]="payable.documentNumber" name="documentNumber" required></label><label>Fecha documento<input [(ngModel)]="payable.documentDate" name="documentDate" type="date" required></label>
<label>Vencimiento<input [(ngModel)]="payable.dueDate" name="dueDate" type="date" required></label><label>Monto<input [(ngModel)]="payable.amount" name="amount" type="number" step="0.01" min="0.01" required></label>
<label>Cuenta gasto<input [(ngModel)]="payable.expenseAccount" name="expenseAccount" required></label><label>Cuenta por pagar<input [(ngModel)]="payable.payableAccount" name="payableAccount" required></label>
<div class="form-actions"><button class="button primary" type="submit" [disabled]="busy||!canCreatePayable">Registrar obligación</button></div></form></section>
<section class="panel"><h3>Proyección de caja</h3><div class="form-grid compact"><label class="wide">Comunidad<select [(ngModel)]="forecastCommunityId" name="forecastCommunityId"><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label><div class="form-actions"><button class="button secondary" type="button" (click)="loadForecast()" [disabled]="busy||!forecastCommunityId">Consultar</button></div></div><pre class="result-box" *ngIf="forecast">{{forecast|json}}</pre></section>`})
export class TreasuryComponent implements OnInit{
private readonly http=inject(HttpClient);private readonly communityOptions=inject(CommunityOptionsService);communities:CommunityOption[]=[];suppliers:TreasurySupplierOption[]=[];busy=false;error='';message='';forecast:unknown;forecastCommunityId='';
payable={communityId:'',supplierId:'',documentNumber:'',documentDate:'',dueDate:'',amount:0,expenseAccount:'',payableAccount:''};
get canCreatePayable():boolean{
  const amount=Number(this.payable.amount);
  const documentDate=this.payable.documentDate?new Date(`${this.payable.documentDate}T00:00:00`):null;
  const dueDate=this.payable.dueDate?new Date(`${this.payable.dueDate}T00:00:00`):null;
  return !!this.payable.communityId&&!!this.payable.supplierId&&!!this.payable.documentNumber.trim()&&!!this.payable.expenseAccount.trim()&&!!this.payable.payableAccount.trim()&&!!documentDate&&!Number.isNaN(documentDate.getTime())&&!!dueDate&&!Number.isNaN(dueDate.getTime())&&dueDate>=documentDate&&Number.isFinite(amount)&&amount>0;
}
ngOnInit():void{this.communityOptions.load().subscribe({next:items=>this.communities=items,error:()=>this.error='No se pudieron cargar las comunidades disponibles.'});}
loadSuppliers(communityId:string):void{this.payable.supplierId='';this.suppliers=[];if(!communityId)return;this.http.get<TreasurySupplierOption[]>(`${API_BASE_URL}/treasury/communities/${communityId}/eligible-suppliers`).subscribe({next:items=>this.suppliers=items,error:e=>this.error=this.describe(e,'No se pudieron cargar los proveedores elegibles.')});}
createPayable():void{
  if(!this.canCreatePayable){this.error='Revise proveedor, documento, fechas, monto y cuentas antes de registrar la obligación.';return;}
  this.busy=true;this.error='';this.message='';
  const communityId=this.payable.communityId;
  const payload={...this.payable,documentNumber:this.payable.documentNumber.trim(),expenseAccount:this.payable.expenseAccount.trim(),payableAccount:this.payable.payableAccount.trim(),amount:Number(this.payable.amount)};
  this.http.post<{id:string}>(`${API_BASE_URL}/treasury/payables`,payload).subscribe({next:v=>{this.busy=false;this.message=`Obligación creada: ${v.id}`;this.payable={communityId,supplierId:'',documentNumber:'',documentDate:'',dueDate:'',amount:0,expenseAccount:'',payableAccount:''};this.loadSuppliers(communityId);},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo crear la obligación.');}})
}
loadForecast():void{this.busy=true;this.error='';this.forecast=undefined;this.http.get(`${API_BASE_URL}/treasury/communities/${this.forecastCommunityId}/cash-forecast`).subscribe({next:v=>{this.busy=false;this.forecast=v;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo consultar caja.');}})}
private describe(e:{status?:number},fallback:string){return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos de Treasury.':fallback;}
}
