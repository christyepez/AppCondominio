import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface SupplierOption { id:string; taxId:string; legalName:string; email:string; }
export interface ProcurementRoundOption { id:string; title:string; closesAtUtc:string; }

@Injectable({ providedIn: 'root' })
export class ProcurementOptionsService {
  private readonly http = inject(HttpClient);
  suppliers(communityId:string):Observable<SupplierOption[]> {
    return this.http.get<SupplierOption[]>(`${API_BASE_URL}/procurement/communities/${communityId}/suppliers`);
  }
  rounds(communityId:string):Observable<ProcurementRoundOption[]> {
    return this.http.get<ProcurementRoundOption[]>(`${API_BASE_URL}/procurement/communities/${communityId}/rounds`);
  }
}
