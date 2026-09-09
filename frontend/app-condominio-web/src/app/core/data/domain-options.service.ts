import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface UnitOption { id: string; code: string; location: string; mainAreaM2: number; status: number | string; }
export interface ReservableAreaOption { id: string; code: string; name: string; capacity: number; fee: number; }

@Injectable({ providedIn: 'root' })
export class DomainOptionsService {
  private readonly http = inject(HttpClient);
  units(communityId: string): Observable<UnitOption[]> {
    return this.http.get<UnitOption[]>(`${API_BASE_URL}/properties/communities/${communityId}/units`).pipe(
      map(items => [...items].sort((a, b) => a.code.localeCompare(b.code)))
    );
  }
  reservableAreas(communityId: string): Observable<ReservableAreaOption[]> {
    return this.http.get<ReservableAreaOption[]>(`${API_BASE_URL}/reservations/communities/${communityId}/areas`).pipe(
      map(items => [...items].sort((a, b) => a.name.localeCompare(b.name)))
    );
  }
}
