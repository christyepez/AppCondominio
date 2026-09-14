import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { CommunityOption, CommunityOptionsService } from '../../core/data/community-options.service';

@Component({selector:'app-accounting',standalone:true,imports:[CommonModule,FormsModule],template:`
<section class="page-heading"><div><span class="eyebrow">Contabilidad</span><h2>Períodos y balance de comprobación</h2><p>Operaciones contables conectadas al módulo Accounting.</p></div></section>
<div class="notice error" *ngIf="error">{{error}}</div><div class="notice success" *ngIf="message">{{message}}</div>
<section class="panel"><h3>Abrir período contable</h3><form class="form-grid" (ngSubmit)="openPeriod()">
<label>Comunidad<select [(ngModel)]="period.communityId" name="periodCommunity" required><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{community.name}} ({{community.code}})</option></select></label>
<label>Año<input [(ngModel)]="period.year" name="year" type="number" min="2000" max="2100" step="1" required></label>
<label>Mes<input [(ngModel)]="period.month" name="month" type="number" min="1" max="12" step="1" required></label>
<div class="form-actions"><button class="button primary" type="submit" [disabled]="busy||!canOpenPeriod">Abrir período</button></div></form></section>
<section class="panel"><h3>Balance de comprobación</h3><div class="form-grid compact"><label class="wide">Comunidad<select [(ngModel)]="trialCommunityId" name="trialCommunityId"><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{community.name}} ({{community.code}})</option></select></label><div class="form-actions"><button class="button secondary" type="button" (click)="loadTrialBalance()" [disabled]="busy||!trialCommunityId">Consultar</button></div></div><pre class="result-box" *ngIf="trialBalance">{{trialBalance|json}}</pre></section>`})
export class AccountingComponent implements OnInit{
private readonly http=inject(HttpClient);private readonly communityOptions=inject(CommunityOptionsService);communities:CommunityOption[]=[];busy=false;error='';message='';trialBalance:unknown;trialCommunityId='';period={communityId:'',year:new Date().getFullYear(),month:new Date().getMonth()+1};
get canOpenPeriod():boolean{const year=Number(this.period.year);const month=Number(this.period.month);return !!this.period.communityId&&Number.isInteger(year)&&year>=2000&&year<=2100&&Number.isInteger(month)&&month>=1&&month<=12;}
ngOnInit():void{this.communityOptions.load().subscribe({next:items=>this.communities=items,error:()=>this.error='No se pudieron cargar las comunidades disponibles.'});}
openPeriod():void{if(!this.canOpenPeriod){this.error='Revise comunidad, año y mes antes de abrir el período.';return;}this.busy=true;this.error='';this.message='';const payload={communityId:this.period.communityId,year:Number(this.period.year),month:Number(this.period.month)};this.http.post<{id:string}>(`${API_BASE_URL}/accounting/periods`,payload).subscribe({next:v=>{this.busy=false;this.message=`Período contable creado: ${v.id}`;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo abrir el período contable.');}});}
loadTrialBalance():void{if(!this.trialCommunityId)return;this.busy=true;this.error='';this.message='';this.trialBalance=undefined;this.http.get(`${API_BASE_URL}/accounting/communities/${this.trialCommunityId}/trial-balance`).subscribe({next:v=>{this.busy=false;this.trialBalance=v;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo consultar el balance.');}});}
private describe(e:{status?:number},fallback:string):string{return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos de Accounting.':fallback;}
}