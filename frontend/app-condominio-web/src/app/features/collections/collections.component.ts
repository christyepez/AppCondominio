import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { CommunityOption, CommunityOptionsService } from '../../core/data/community-options.service';
import { DomainOptionsService, UnitOption } from '../../core/data/domain-options.service';

@Component({
  selector: 'app-collections',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="page-heading"><div><span class="eyebrow">Cartera</span><h2>Cobranza</h2><p>Consulta de cartera/aging y registro de pagos.</p></div></section>
    <div class="notice error" *ngIf="error">{{ error }}</div><div class="notice success" *ngIf="message">{{ message }}</div>
    <section class="panel"><h3>Consulta de cartera</h3><div class="form-grid">
      <label>Comunidad<select [(ngModel)]="query.communityId" name="queryCommunity" (ngModelChange)="loadUnits($event)"><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label>
      <label>Unidad opcional<select [(ngModel)]="query.unitId" name="queryUnit"><option value="">Todas las unidades</option><option *ngFor="let unit of units" [value]="unit.id">{{ unit.code }} · {{ unit.location }}</option></select></label>
      <label>Fecha de corte<input [(ngModel)]="query.on" name="queryOn" type="date"></label>
      <div class="form-actions"><button class="button secondary" type="button" (click)="loadReceivables()" [disabled]="busy||!canQuery">Ver cuentas por cobrar</button><button class="button secondary" type="button" (click)="loadAging()" [disabled]="busy||!canQuery">Ver aging</button></div>
    </div><pre class="result-box" *ngIf="result">{{ result | json }}</pre></section>
    <section class="panel"><h3>Registrar pago</h3><form class="form-grid" (ngSubmit)="registerPayment()">
      <label>Comunidad<select [(ngModel)]="payment.communityId" name="paymentCommunity" required><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label>
      <label>Referencia<input [(ngModel)]="payment.reference" name="reference" required></label>
      <label>Método<select [(ngModel)]="payment.method" name="method"><option [ngValue]="1">Efectivo</option><option [ngValue]="2">Transferencia</option><option [ngValue]="3">Tarjeta</option><option [ngValue]="4">Depósito</option><option [ngValue]="5">Cheque</option><option [ngValue]="99">Otro</option></select></label>
      <label>Fecha recepción<input [(ngModel)]="payment.receivedOn" name="receivedOn" type="date" required></label>
      <label>Monto<input [(ngModel)]="payment.amount" name="amount" type="number" min="0.01" step="0.01" required></label>
      <label>Transacción externa<input [(ngModel)]="payment.externalTransactionId" name="externalTransactionId"></label>
      <div class="form-actions"><button class="button primary" type="submit" [disabled]="busy||!canRegisterPayment">Registrar pago</button></div>
    </form></section>`
})
export class CollectionsComponent implements OnInit {
  private readonly http=inject(HttpClient); private readonly communityOptions=inject(CommunityOptionsService); private readonly domainOptions=inject(DomainOptionsService);
  communities:CommunityOption[]=[]; units:UnitOption[]=[]; busy=false; error=''; message=''; result:unknown;
  query={communityId:'',unitId:'',on:''};
  payment={communityId:'',reference:'',method:2,receivedOn:'',amount:0,externalTransactionId:''};
  private readonly allowedMethods=[1,2,3,4,5,99];
  get canQuery():boolean { return !!this.query.communityId && (!this.query.on || this.isValidDate(this.query.on)); }
  get canRegisterPayment():boolean {
    const amount=Number(this.payment.amount);
    return !!this.payment.communityId && !!this.payment.reference.trim() && this.allowedMethods.includes(Number(this.payment.method)) &&
      this.isValidDate(this.payment.receivedOn) && Number.isFinite(amount) && amount>0;
  }
  ngOnInit():void { this.communityOptions.load().subscribe({next:items=>this.communities=items,error:()=>this.error='No se pudieron cargar las comunidades disponibles.'}); }
  loadUnits(communityId:string):void { this.units=[]; this.query.unitId=''; this.result=undefined; if(!communityId)return; this.domainOptions.units(communityId).subscribe({next:items=>this.units=items,error:()=>this.error='No se pudieron cargar las unidades.'}); }
  loadReceivables():void { if(!this.canQuery)return; const params=new URLSearchParams(); if(this.query.unitId)params.set('unitId',this.query.unitId); if(this.query.on)params.set('on',this.query.on); this.load(`${API_BASE_URL}/collections/communities/${this.query.communityId}/receivables?${params.toString()}`); }
  loadAging():void { if(!this.canQuery)return; const suffix=this.query.on?`?on=${encodeURIComponent(this.query.on)}`:''; this.load(`${API_BASE_URL}/collections/communities/${this.query.communityId}/aging${suffix}`); }
  registerPayment():void {
    if(!this.canRegisterPayment)return;
    this.busy=true; this.error=''; this.message='';
    const communityId=this.payment.communityId;
    const payload={...this.payment,reference:this.payment.reference.trim(),method:Number(this.payment.method),amount:Number(this.payment.amount),externalTransactionId:this.payment.externalTransactionId.trim()||null};
    this.http.post<{id:string}>(`${API_BASE_URL}/collections/payments`,payload).subscribe({
      next:value=>{this.busy=false;this.message=`Pago registrado: ${value.id}`;this.payment={communityId,reference:'',method:2,receivedOn:'',amount:0,externalTransactionId:''};},
      error:err=>{this.busy=false;this.error=this.describe(err,'No se pudo registrar el pago.');}
    });
  }
  private load(url:string):void { this.busy=true;this.error='';this.message='';this.result=undefined;this.http.get(url).subscribe({next:value=>{this.busy=false;this.result=value;},error:err=>{this.busy=false;this.error=this.describe(err,'No se pudo consultar cartera.');}}); }
  private isValidDate(value:string):boolean { if(!value)return false; const parsed=new Date(`${value}T00:00:00`); return !Number.isNaN(parsed.getTime()); }
  private describe(err:{status?:number},fallback:string):string { if(err?.status===401)return 'Sesión requerida desde PortalCorporativo.'; if(err?.status===403)return 'No tiene permisos de Collections.'; return fallback; }
}
