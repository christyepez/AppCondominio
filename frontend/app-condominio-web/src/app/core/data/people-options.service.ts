import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface PersonOption {
  id: string;
  identification: string;
  displayName: string;
}

@Injectable({ providedIn: 'root' })
export class PeopleOptionsService {
  private readonly http = inject(HttpClient);

  load(communityId: string): Observable<PersonOption[]> {
    return this.http.get<PersonOption[]>(`${API_BASE_URL}/people/communities/${communityId}`).pipe(
      map(items => [...items].sort((a, b) => a.displayName.localeCompare(b.displayName)))
    );
  }
}
