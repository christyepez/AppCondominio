import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';

@Component({
  selector: 'app-banking',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="page-heading">
      <div><span class="eyebrow">Banca</span><h2>Cuentas y conciliación</h2><p>Registro de cuentas bancarias y preparación de la conciliación.</p></div>
    </section>
    <div class="notice error" *ngIf="error">{{ error }}</div><div class="notice success" *ngIf="message">{{ message }}</div>
    <section class="panel"><h3>Nueva cuenta bancaria</h3>
      <form class="form-grid" (ngSubmit)="createAccount()">
        <label>Community Id<input [(ngModel)]="account.communityId" name="communityId" required></label>
        <label>Código banco<input [(ngModel)]="account.bankCode" name="bankCode" required></label>
        <label>Número cuenta<input [(ngModel)]="account.accountNumber" name="accountNumber" required></label>
        <label>Moneda<input [(ngModel)]="account.currency" name="currency" required></label>
        <div class="form-actions"><button class="button primary" type="submit" [disabled]="busy">Crear cuenta</button></div>
      </form>
    </section>
    <section class="panel"><h3>Conciliación</h3><p>Las sugerencias automáticas requieren candidatos de pagos confirmados. Esta pantalla no fabrica coincidencias; usa el endpoint de conciliación cuando se disponga de esos IDs.</p></section>
  `
})
export class BankingComponent {
  private readonly http=inject(HttpClient); busy=false; error=''; message='';
  account={communityId:'',bankCode:'',accountNumber:'',currency:'USD'};
  createAccount():void{this.busy=true;this.error='';this.message='';this.http.post<{id:string}>(`${API_BASE_URL}/banking/accounts`,this.account).subscribe({next:v=>{this.busy=false;this.message=`Cuenta creada: ${v.id}`;},error:e=>{this.busy=false;this.error=this.describe(e);}});}
  private describe(e:{status?:number}):string{return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos de Banking.':'No se pudo crear la cuenta bancaria.';}
}
