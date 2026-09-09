import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { CommunityOption, CommunityOptionsService } from '../../core/data/community-options.service';
import { FinanceOptionsService, TaxDocumentOption } from '../../core/data/finance-options.service';

@Component({selector:'app-tax',standalone:true,imports:[CommonModule,FormsModule],template:`
<section class="page-heading"><div><span class="eyebrow">SRI</span><h2>Documentos electrónicos</h2><p>Consulta y envío de documentos tributarios mediante el adapter configurado.</p></div></section>
<div class="notice error" *ngIf="error">{{error}}</div><div class="notice success" *ngIf="message">{{message}}</div>
<section class="panel"><h3>Consultar documento</h3><div class="form-grid compact"><label>Comunidad<select [(ngModel)]="communityId" name="communityId" (ngModelChange)="loadDocuments($event)"><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{community.name}} ({{community.code}})</option></select></label><label>Documento<select [(ngModel)]="documentId" name="documentId"><option value="">Seleccione un documento</option><option *ngFor="let item of documents" [value]="item.id">{{item.documentType}} · {{item.accessKey}} · {{item.status}}</option></select></label><div class="form-actions"><button class="button secondary" type="button" (click)="load()" [disabled]="busy||!documentId">Consultar</button><button class="button primary" type="button" (click)="submit()" [disabled]="busy||!documentId">Enviar al adapter</button></div></div><pre class="result-box" *ngIf="result">{{result|json}}</pre></section>
<section class="notice info">Si el proveedor/certificado SRI no está configurado, el backend mantiene el documento en <strong>PendingConfiguration</strong>; la UI no simula autorizaciones.</section>`})
export class TaxComponent implements OnInit{private readonly http=inject(HttpClient);private readonly communityOptions=inject(CommunityOptionsService);private readonly financeOptions=inject(FinanceOptionsService);communities:CommunityOption[]=[];documents:TaxDocumentOption[]=[];busy=false;error='';message='';communityId='';documentId='';result:unknown;
ngOnInit():void{this.communityOptions.load().subscribe({next:v=>this.communities=v,error:()=>this.error='No se pudieron cargar las comunidades disponibles.'});}
loadDocuments(communityId:string):void{this.documents=[];this.documentId='';if(!communityId)return;this.financeOptions.loadTaxDocuments(communityId).subscribe({next:v=>this.documents=v,error:e=>this.error=this.describe(e,'No se pudieron cargar los documentos tributarios.')});}
load():void{this.busy=true;this.error='';this.message='';this.result=undefined;this.http.get(`${API_BASE_URL}/tax/documents/${this.documentId}`).subscribe({next:v=>{this.busy=false;this.result=v;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo consultar el documento.');}})}
submit():void{this.busy=true;this.error='';this.message='';this.http.post(`${API_BASE_URL}/tax/documents/${this.documentId}/submit`,{}).subscribe({next:v=>{this.busy=false;this.result=v;this.message='Solicitud enviada al adapter tributario.';this.loadDocuments(this.communityId);},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo enviar el documento.');}})}
private describe(e:{status?:number},fallback:string){return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos de Tax.':fallback;}}
