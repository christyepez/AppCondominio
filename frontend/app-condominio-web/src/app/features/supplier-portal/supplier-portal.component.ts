import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';

interface SupplierContext { supplierId:string; communityId:string; taxId:string; legalName:string; email:string; }
interface SupplierRound { id:string; title:string; closesAtUtc:string; }
interface SupplierBid { id:string; roundId:string; amount:number; deliveryDays:number; proposalReference:string; submittedAtUtc:string; evaluationScore?:number|null; }
interface SupplierOrder { id:string; number:string; amount:number; payableReference:string; status:number; issuedAtUtc:string; }

@Component({
 selector:'app-supplier-portal', standalone:true, imports:[CommonModule,FormsModule],
 template:`
  <section class="supplier-hero">
   <div><span class="eyebrow">Portal Proveedor</span><h2>{{ context?.legalName || 'Mi empresa' }}</h2><p>Revisa procesos abiertos, presenta ofertas y gestiona tus órdenes de compra sin acceder a información de otros proveedores.</p></div>
   <span class="secure-pill">Identidad validada por PortalCorporativo</span>
  </section>
  <div class="notice error" *ngIf="error">{{ error }}</div><div class="notice success" *ngIf="message">{{ message }}</div>

  <section class="metric-grid" *ngIf="context">
   <article class="metric"><span>RUC / Identificación</span><strong class="small-value">{{ context.taxId }}</strong></article>
   <article class="metric"><span>Procesos abiertos</span><strong>{{ rounds.length }}</strong></article>
   <article class="metric"><span>Mis ofertas</span><strong>{{ bids.length }}</strong></article>
   <article class="metric"><span>Órdenes</span><strong>{{ orders.length }}</strong></article>
  </section>
  <div class="supplier-grid" *ngIf="context">
   <section class="panel">
    <div class="panel-title"><h3>Procesos abiertos</h3><button type="button" class="button secondary" (click)="loadAll()" [disabled]="busy">Actualizar</button></div>
    <div class="round-list" *ngIf="rounds.length; else noRounds">
     <article class="round-card" *ngFor="let round of rounds" [class.selected]="round.id===selectedRoundId" (click)="selectRound(round)">
      <div><strong>{{ round.title }}</strong><small>Cierra: {{ round.closesAtUtc | date:'medium' }}</small></div><button type="button" class="button secondary" (click)="selectRound(round); $event.stopPropagation()" [disabled]="isRoundClosed(round)">Ofertar</button>
     </article>
    </div><ng-template #noRounds><p class="empty-state">No hay procesos abiertos disponibles para su comunidad.</p></ng-template>
   </section>

   <section class="panel">
    <div class="panel-title"><h3>Presentar oferta</h3><small *ngIf="selectedRound">Cierra {{ selectedRound.closesAtUtc | date:'medium' }}</small></div>
    <form class="form-grid" (ngSubmit)="submitBid()">
     <label class="wide">Proceso<select name="roundId" [(ngModel)]="selectedRoundId" required><option value="">Seleccione...</option><option *ngFor="let r of rounds" [value]="r.id" [disabled]="isRoundClosed(r)">{{ r.title }}</option></select></label>
     <label>Monto<input type="number" min="0.01" step="0.01" name="amount" [(ngModel)]="bidForm.amount" required></label>
     <label>Días de entrega<input type="number" min="0" step="1" name="deliveryDays" [(ngModel)]="bidForm.deliveryDays" required></label>
     <label class="wide">Referencia de propuesta<input name="proposalReference" maxlength="500" [(ngModel)]="bidForm.proposalReference" required placeholder="Documento, URL o referencia interna"></label>
     <div class="form-actions"><button class="button primary" [disabled]="busy||!canSubmitBid">Enviar oferta</button></div>
    </form>
   </section>
  </div>
  <section class="panel" *ngIf="context">
   <div class="panel-title"><h3>Mis ofertas</h3><span class="badge">{{ bids.length }}</span></div>
   <div class="table-wrap" *ngIf="bids.length; else noBids"><table><thead><tr><th>Fecha</th><th>Monto</th><th>Entrega</th><th>Referencia</th><th>Evaluación</th></tr></thead><tbody><tr *ngFor="let b of bids"><td>{{ b.submittedAtUtc | date:'short' }}</td><td>{{ b.amount | currency:'USD' }}</td><td>{{ b.deliveryDays }} días</td><td>{{ b.proposalReference }}</td><td>{{ b.evaluationScore == null ? 'Pendiente' : (b.evaluationScore | number:'1.0-2') }}</td></tr></tbody></table></div>
   <ng-template #noBids><p class="empty-state">Todavía no ha presentado ofertas.</p></ng-template>
  </section>

  <section class="panel" *ngIf="context">
   <div class="panel-title"><h3>Órdenes de compra</h3><span class="badge">{{ orders.length }}</span></div>
   <div class="table-wrap" *ngIf="orders.length; else noOrders"><table><thead><tr><th>Número</th><th>Emisión</th><th>Monto</th><th>Referencia pago</th><th>Estado</th><th></th></tr></thead><tbody><tr *ngFor="let o of orders"><td>{{ o.number }}</td><td>{{ o.issuedAtUtc | date:'shortDate' }}</td><td>{{ o.amount | currency:'USD' }}</td><td>{{ o.payableReference }}</td><td>{{ orderStatus(o.status) }}</td><td><button *ngIf="o.status===0" type="button" class="button secondary" (click)="acknowledge(o)" [disabled]="busy">Confirmar recepción</button></td></tr></tbody></table></div>
   <ng-template #noOrders><p class="empty-state">No existen órdenes de compra asignadas a su proveedor.</p></ng-template>
  </section>
 `,
 styles:[`
  .supplier-hero{display:flex;justify-content:space-between;gap:20px;align-items:flex-start;margin-bottom:24px}.supplier-hero h2{font-size:32px;margin:4px 0 6px}.supplier-hero p{margin:0;color:#746b7d;max-width:720px}.secure-pill{background:#eef5ff;color:#24578e;border:1px solid #cfe1f5;border-radius:999px;padding:8px 12px;font-size:12px;font-weight:700}.supplier-grid{display:grid;grid-template-columns:1.1fr .9fr;gap:20px}.round-list{display:grid;gap:10px}.round-card{border:1px solid #e0d9e5;border-radius:12px;padding:14px;display:flex;justify-content:space-between;align-items:center;gap:12px;cursor:pointer}.round-card.selected{border-color:#6d3d91;box-shadow:0 0 0 2px #eadcf4}.round-card div{display:grid;gap:4px}.round-card small{color:#746b7d}.small-value{font-size:20px!important}@media(max-width:850px){.supplier-hero{flex-direction:column}.supplier-grid{grid-template-columns:1fr}}
 `]
})
export class SupplierPortalComponent implements OnInit {
 private readonly http=inject(HttpClient);
 context?:SupplierContext; rounds:SupplierRound[]=[]; bids:SupplierBid[]=[]; orders:SupplierOrder[]=[]; selectedRoundId=''; busy=false; error=''; message='';
 bidForm={amount:0,deliveryDays:0,proposalReference:''};
 get selectedRound():SupplierRound|undefined{return this.rounds.find(r=>r.id===this.selectedRoundId);}
 get canSubmitBid():boolean{const round=this.selectedRound;const amount=Number(this.bidForm.amount);return !!round&&!this.isRoundClosed(round)&&Number.isFinite(amount)&&amount>0&&Number.isInteger(this.bidForm.deliveryDays)&&this.bidForm.deliveryDays>=0&&this.bidForm.proposalReference.trim().length>0;}
 ngOnInit():void{this.loadContext();}
 loadContext():void{this.busy=true;this.error='';this.http.get<SupplierContext>(`${API_BASE_URL}/supplier/me`).subscribe({next:x=>{this.context=x;this.busy=false;this.loadAll();},error:e=>{this.busy=false;this.error=this.errorMessage(e?.status);}});}
 loadAll():void{if(!this.context)return;this.loadRounds();this.loadBids();this.loadOrders();}
 loadRounds():void{this.http.get<SupplierRound[]>(`${API_BASE_URL}/supplier/rounds`).subscribe({next:x=>{this.rounds=x??[];if(this.selectedRound&&!this.rounds.some(r=>r.id===this.selectedRoundId))this.selectedRoundId='';},error:e=>this.error=this.errorMessage(e?.status)});}
 loadBids():void{this.http.get<SupplierBid[]>(`${API_BASE_URL}/supplier/bids`).subscribe({next:x=>this.bids=x??[],error:e=>this.error=this.errorMessage(e?.status)});}
 loadOrders():void{this.http.get<SupplierOrder[]>(`${API_BASE_URL}/supplier/orders`).subscribe({next:x=>this.orders=x??[],error:e=>this.error=this.errorMessage(e?.status)});}
 selectRound(round:SupplierRound):void{if(this.isRoundClosed(round))return;this.selectedRoundId=round.id;}
 isRoundClosed(round:SupplierRound):boolean{const close=new Date(round.closesAtUtc);return Number.isNaN(close.getTime())||close.getTime()<=Date.now();}
 submitBid():void{if(!this.canSubmitBid)return;this.busy=true;this.error='';this.message='';const payload={amount:Number(this.bidForm.amount),deliveryDays:this.bidForm.deliveryDays,proposalReference:this.bidForm.proposalReference.trim()};this.http.post(`${API_BASE_URL}/supplier/rounds/${this.selectedRoundId}/bids`,payload).subscribe({next:()=>{this.busy=false;this.message='Oferta enviada correctamente.';this.bidForm={amount:0,deliveryDays:0,proposalReference:''};this.selectedRoundId='';this.loadBids();this.loadRounds();},error:e=>{this.busy=false;this.error=this.errorMessage(e?.status);}});}
 acknowledge(order:SupplierOrder):void{if(!order?.id||order.status!==0)return;this.busy=true;this.error='';this.message='';this.http.post(`${API_BASE_URL}/supplier/orders/${order.id}/acknowledge`,{}).subscribe({next:()=>{this.busy=false;this.message='Recepción de la orden confirmada.';this.loadOrders();},error:e=>{this.busy=false;this.error=this.errorMessage(e?.status);}});}
 orderStatus(status:number):string{return ({0:'Emitida',1:'Confirmada',2:'Completada',3:'Cancelada'} as Record<number,string>)[status]??'Desconocido';}
 private errorMessage(status?:number):string{return status===401?'Debe iniciar sesión mediante PortalCorporativo.':status===403?'Su usuario no tiene habilitado el Portal Proveedor.':'No fue posible completar la operación.';}
}
