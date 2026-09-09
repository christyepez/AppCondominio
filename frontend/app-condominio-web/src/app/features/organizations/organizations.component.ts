import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { OrganizationOption, OrganizationOptionsService } from '../../core/data/organization-options.service';

@Component({selector:'app-organizations',standalone:true,imports:[CommonModule,FormsModule],template:`
<section class="page-heading"><div><span class="eyebrow">SaaS</span><h2>Organizaciones</h2><p>Alta y consulta de organizaciones comerciales.</p></div></section>
<div class="notice error" *ngIf="error">{{error}}</div><div class="notice success" *ngIf="message">{{message}}</div>
<section class="panel"><h3>Nueva organización</h3><form class="form-grid" (ngSubmit)="create()"><label>Nombre<input [(ngModel)]="draft.name" name="name" required></label><label>RUC / Tax Id<input [(ngModel)]="draft.taxId" name="taxId"></label><div class="form-actions"><button class="button primary" type="submit" [disabled]="busy">Crear organización</button></div></form></section>
<section class="panel"><h3>Consultar organización</h3><div class="form-grid compact"><label class="wide">Organización<select [(ngModel)]="organizationId" name="organizationId"><option value="">Seleccione una organización</option><option *ngFor="let item of organizations" [value]="item.id">{{item.name}} ({{item.taxId || 'sin RUC'}})</option></select></label><div class="form-actions"><button class="button secondary" type="button" (click)="load()" [disabled]="busy||!organizationId">Consultar</button></div></div><pre class="result-box" *ngIf="result">{{result|json}}</pre></section>`})
export class OrganizationsComponent implements OnInit {
  private readonly http=inject(HttpClient);
  private readonly organizationOptions=inject(OrganizationOptionsService);
  busy=false;error='';message='';organizationId='';result:unknown;organizations:OrganizationOption[]=[];draft={name:'',taxId:''};
  ngOnInit():void{this.refreshOrganizations();}
  create():void{this.busy=true;this.error='';this.message='';this.http.post<{id:string}>(`${API_BASE_URL}/organizations/`,{name:this.draft.name,taxId:this.draft.taxId||null}).subscribe({next:v=>{this.busy=false;this.organizationId=v.id;this.message=`Organización creada: ${v.id}`;this.refreshOrganizations();},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo crear la organización.');}})}
  load():void{this.busy=true;this.error='';this.message='';this.result=undefined;this.http.get(`${API_BASE_URL}/organizations/${this.organizationId}`).subscribe({next:v=>{this.busy=false;this.result=v;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo consultar la organización.');}})}
  private refreshOrganizations():void{this.organizationOptions.load().subscribe({next:v=>this.organizations=v,error:e=>this.error=this.describe(e,'No se pudieron cargar las organizaciones.')});}
  private describe(e:{status?:number},fallback:string){return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos de Organizations.':fallback;}
}
