import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';

@Component({selector:'app-tax',standalone:true,imports:[CommonModule,FormsModule],template:`
<section class="page-heading"><div><span class="eyebrow">SRI</span><h2>Documentos electrónicos</h2><p>Consulta y envío de documentos tributarios mediante el adapter configurado.</p></div></section>
<div class="notice error" *ngIf="error">{{error}}</div><div class="notice success" *ngIf="message">{{message}}</div>
<section class="panel"><h3>Consultar documento</h3><div class="form-grid compact"><label class="wide">Document Id<input [(ngModel)]="documentId" name="documentId"></label><div class="form-actions"><button class="button secondary" type="button" (click)="load()" [disabled]="busy||!documentId">Consultar</button><button class="button primary" type="button" (click)="submit()" [disabled]="busy||!documentId">Enviar al adapter</button></div></div><pre class="result-box" *ngIf="result">{{result|json}}</pre></section>
<section class="notice info">Si el proveedor/certificado SRI no está configurado, el backend mantiene el documento en <strong>PendingConfiguration</strong>; la UI no simula autorizaciones.</section>`})
export class TaxComponent{private readonly http=inject(HttpClient);busy=false;error='';message='';documentId='';result:unknown;
load():void{this.busy=true;this.error='';this.message='';this.result=undefined;this.http.get(`${API_BASE_URL}/tax/documents/${this.documentId}`).subscribe({next:v=>{this.busy=false;this.result=v;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo consultar el documento.');}})}
submit():void{this.busy=true;this.error='';this.message='';this.http.post(`${API_BASE_URL}/tax/documents/${this.documentId}/submit`,{}).subscribe({next:v=>{this.busy=false;this.result=v;this.message='Solicitud enviada al adapter tributario.';},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo enviar el documento.');}})}
private describe(e:{status?:number},fallback:string){return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos de Tax.':fallback;}}
