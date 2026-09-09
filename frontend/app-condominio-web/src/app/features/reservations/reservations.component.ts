import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { API_BASE_URL } from '../../core/config/api.config';
import { CommunityOption, CommunityOptionsService } from '../../core/data/community-options.service';
import { DomainOptionsService, ReservableAreaOption, UnitOption } from '../../core/data/domain-options.service';

@Component({selector:'app-reservations',standalone:true,imports:[CommonModule,FormsModule],template:`
<section class="page-heading"><div><span class="eyebrow">Reservas</span><h2>Áreas y reservas</h2><p>Configuración de áreas reservables y solicitud de reservas.</p></div></section>
<div class="notice error" *ngIf="error">{{error}}</div><div class="notice success" *ngIf="message">{{message}}</div>
<section class="panel"><h3>Nueva área reservable</h3><form class="form-grid" (ngSubmit)="createArea()">
<label>Comunidad<select [(ngModel)]="area.communityId" name="communityId" required><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label><label>Código<input [(ngModel)]="area.code" name="code" required></label>
<label>Nombre<input [(ngModel)]="area.name" name="name" required></label><label>Capacidad<input [(ngModel)]="area.capacity" name="capacity" type="number" min="1" required></label>
<label>Tarifa<input [(ngModel)]="area.fee" name="fee" type="number" min="0" step="0.01"></label><div class="form-actions"><button class="button primary" type="submit" [disabled]="busy">Crear área</button></div></form></section>
<section class="panel"><h3>Nueva reserva</h3><form class="form-grid" (ngSubmit)="createBooking()">
<label>Comunidad<select [(ngModel)]="bookingCommunityId" name="bookingCommunityId" (ngModelChange)="loadAreas($event)" required><option value="">Seleccione una comunidad</option><option *ngFor="let community of communities" [value]="community.id">{{ community.name }} ({{ community.code }})</option></select></label><label>Área<select [(ngModel)]="booking.areaId" name="areaId" required><option value="">Seleccione un área</option><option *ngFor="let item of reservableAreas" [value]="item.id">{{ item.name }} · cap. {{ item.capacity }}</option></select></label><label>Requester Id<input [(ngModel)]="booking.requesterId" name="requesterId" required></label>
<label>Inicio<input [(ngModel)]="booking.startsAt" name="startsAt" type="datetime-local" required></label><label>Fin<input [(ngModel)]="booking.endsAt" name="endsAt" type="datetime-local" required></label>
<label>Invitados<input [(ngModel)]="booking.guests" name="guests" type="number" min="0"></label><div class="form-actions"><button class="button primary" type="submit" [disabled]="busy">Solicitar reserva</button></div></form></section>`})
export class ReservationsComponent implements OnInit{private readonly http=inject(HttpClient);private readonly communityOptions=inject(CommunityOptionsService);private readonly domainOptions=inject(DomainOptionsService);communities:CommunityOption[]=[];reservableAreas:ReservableAreaOption[]=[];bookingCommunityId='';busy=false;error='';message='';area={communityId:'',code:'',name:'',capacity:1,fee:0};booking={areaId:'',requesterId:'',startsAt:'',endsAt:'',guests:0};
ngOnInit():void{this.communityOptions.load().subscribe({next:items=>this.communities=items,error:()=>this.error='No se pudieron cargar las comunidades disponibles.'});}
loadAreas(communityId:string):void{this.reservableAreas=[];this.booking.areaId='';if(!communityId)return;this.domainOptions.reservableAreas(communityId).subscribe({next:items=>this.reservableAreas=items,error:()=>this.error='No se pudieron cargar las áreas reservables.'});}
createArea():void{this.busy=true;this.error='';this.message='';this.http.post<{id:string}>(`${API_BASE_URL}/reservations/areas`,this.area).subscribe({next:v=>{this.busy=false;this.message=`Área creada: ${v.id}`;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo crear el área.');}})}
createBooking():void{this.busy=true;this.error='';this.message='';const payload={...this.booking,startsAt:new Date(this.booking.startsAt).toISOString(),endsAt:new Date(this.booking.endsAt).toISOString()};this.http.post<{id:string}>(`${API_BASE_URL}/reservations/bookings`,payload).subscribe({next:v=>{this.busy=false;this.message=`Reserva creada: ${v.id}`;},error:e=>{this.busy=false;this.error=this.describe(e,'No se pudo crear la reserva.');}})}
private describe(e:{status?:number},fallback:string){return e?.status===401?'Sesión requerida desde PortalCorporativo.':e?.status===403?'No tiene permisos de Reservations.':fallback;}}
