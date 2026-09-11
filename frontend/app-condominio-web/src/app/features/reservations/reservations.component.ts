import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { CommunityOption, CommunityOptionsService } from '../../core/data/community-options.service';
import { DomainOptionsService, ReservableAreaOption } from '../../core/data/domain-options.service';
import { PeopleOptionsService, PersonOption } from '../../core/data/people-options.service';

@Component({selector:'app-reservations',standalone:true,imports:[CommonModule,FormsModule],template:`
<section class="page-heading"><div><span class="eyebrow">Reservas</span><h2>Áreas y reservas</h2><p>Configuración de áreas reservables y solicitud de reservas.</p></div></section>
<div class="notice error" *ngIf="error">{{error}}</div><div class="notice success" *ngIf="message">{{message}}</div>
<section class="panel"><h3>Nueva área reservable</h3><form class="form-grid" (ngSubmit)="createArea()">
<label>Comunidad<select [(ngModel)]="area.communityId" name="communityId" required><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label><label>Código<input [(ngModel)]="area.code" name="code" required></label>
<label>Nombre<input [(ngModel)]="area.name" name="name" required></label><label>Capacidad<input [(ngModel)]="area.capacity" name="capacity" type="number" min="1" step="1" required></label>
<label>Tarifa<input [(ngModel)]="area.fee" name="fee" type="number" min="0" step="0.01"></label><div class="form-actions"><button class="button primary" type="submit" [disabled]="busy||!canCreateArea">Crear área</button></div></form></section>
<section class="panel"><h3>Nueva reserva</h3><form class="form-grid" (ngSubmit)="createBooking()">
<label>Comunidad<select [(ngModel)]="bookingCommunityId" name="bookingCommunityId" (ngModelChange)="loadBookingOptions($event)" required><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label><label>Área<select [(ngModel)]="booking.areaId" name="areaId" required><option value="">Seleccione un área</option><option *ngFor="let item of reservableAreas" [value]="item.id">{{ item.name }} · cap. {{ item.capacity }}</option></select></label><label>Solicitante<select [(ngModel)]="booking.requesterId" name="requesterId" required><option value="">Seleccione una persona</option><option *ngFor="let person of people" [value]="person.id">{{person.displayName}} · {{person.identification}}</option></select></label>
<label>Inicio<input [(ngModel)]="booking.startsAt" name="startsAt" type="datetime-local" required></label><label>Fin<input [(ngModel)]="booking.endsAt" name="endsAt" type="datetime-local" required></label>
<label>Invitados<input [(ngModel)]="booking.guests" name="guests" type="number" min="1" step="1" [max]="selectedArea?.capacity || 999"></label><div class="form-actions"><button class="button primary" type="submit" [disabled]="busy||!canCreateBooking">Solicitar reserva</button></div></form>
<p class="empty-state" *ngIf="bookingCommunityId && !reservableAreas.length">No hay áreas reservables habilitadas para la comunidad seleccionada.</p></section>`})
export class ReservationsComponent implements OnInit{
  private readonly http=inject(HttpClient);
  private readonly communityOptions=inject(CommunityOptionsService);
  private readonly domainOptions=inject(DomainOptionsService);
  private readonly peopleOptions=inject(PeopleOptionsService);
  communities:CommunityOption[]=[];
  reservableAreas:ReservableAreaOption[]=[];
  people:PersonOption[]=[];
  bookingCommunityId='';
  busy=false;
  error='';
  message='';
  area={communityId:'',code:'',name:'',capacity:1,fee:0};
  booking={areaId:'',requesterId:'',startsAt:'',endsAt:'',guests:1};

  get selectedArea():ReservableAreaOption|undefined{return this.reservableAreas.find(x=>x.id===this.booking.areaId);}
  get canCreateArea():boolean{return !!this.area.communityId&&this.area.code.trim().length>0&&this.area.name.trim().length>0&&Number.isInteger(this.area.capacity)&&this.area.capacity>=1&&Number.isFinite(this.area.fee)&&this.area.fee>=0;}
  get canCreateBooking():boolean{const area=this.selectedArea;if(!this.bookingCommunityId||!area||!this.booking.requesterId)return false;const start=new Date(this.booking.startsAt);const end=new Date(this.booking.endsAt);return !Number.isNaN(start.getTime())&&!Number.isNaN(end.getTime())&&end>start&&Number.isInteger(this.booking.guests)&&this.booking.guests>=1&&this.booking.guests<=area.capacity;}

  ngOnInit():void{this.communityOptions.load().subscribe({next:items=>this.communities=items,error:()=>this.error='No se pudieron cargar las comunidades disponibles.'});}
  loadBookingOptions(communityId:string):void{this.reservableAreas=[];this.people=[];this.booking={areaId:'',requesterId:'',startsAt:'',endsAt:'',guests:1};if(!communityId)return;this.domainOptions.reservableAreas(communityId).subscribe({next:items=>this.reservableAreas=items,error:()=>this.error='No se pudieron cargar las áreas reservables.'});this.peopleOptions.load(communityId).subscribe({next:v=>this.people=v,error:e=>this.error=this.describe(e,'No se pudieron cargar las personas de la comunidad.')});}
  createArea():void{if(!this.canCreateArea)return;this.busy=true;this.error='';this.message='';const payload={communityId:this.area.communityId,code:this.area.code.trim(),name:this.area.name.trim(),capacity:this.area.capacity,fee:this.area.fee};this.http.post<{id:string}>(`${API_BASE_URL}/reservations/areas`,payload).subscribe({next:v=>{this.busy=false;this.message=`Área creada: ${v.id}`;const communityId=this.area.communityId;this.area={communityId,code:'',name:'',capacity:1,fee:0};if(this.bookingCommunityId===communityId)this.loadBookingOptions(communityId);},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo crear el área.');}})}
  createBooking():void{if(!this.canCreateBooking)return;this.busy=true;this.error='';this.message='';const payload={areaId:this.booking.areaId,requesterId:this.booking.requesterId,startsAt:new Date(this.booking.startsAt).toISOString(),endsAt:new Date(this.booking.endsAt).toISOString(),guests:this.booking.guests};this.http.post<{id:string}>(`${API_BASE_URL}/reservations/bookings`,payload).subscribe({next:v=>{this.busy=false;this.message=`Reserva creada: ${v.id}`;this.booking={areaId:'',requesterId:'',startsAt:'',endsAt:'',guests:1};},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo crear la reserva.');}})}
  private describe(e:{status?:number},fallback:string){return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos requeridos para esta operación.':fallback;}
}
