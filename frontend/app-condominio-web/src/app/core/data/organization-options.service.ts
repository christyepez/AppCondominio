import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface OrganizationOption {
  id: string;
  name: string;
  taxId?: string | null;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class OrganizationOptionsService {
  private readonly http = inject(HttpClient);

  load(): Observable<OrganizationOption[]> {
    return this.http.get<OrganizationOption[]>(`${API_BASE_URL}/organizations/`).pipe(
      map(items => [...items].sort((a, b) => a.name.localeCompare(b.name)))
    );
  }
}
