import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface CommunityOption {
  id: string;
  organizationId: string;
  code: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class CommunityOptionsService {
  private readonly http = inject(HttpClient);

  load(): Observable<CommunityOption[]> {
    return this.http.get<CommunityOption[]>(`${API_BASE_URL}/communities/`).pipe(
      map(items => [...items].sort((a, b) => a.name.localeCompare(b.name)))
    );
  }
}
