import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface BudgetPlanOption {
  id: string;
  year: number;
  version: number;
  name: string;
  status: number | string;
}

export interface TaxDocumentOption {
  id: string;
  documentType: string;
  accessKey: string;
  status: number | string;
}

@Injectable({ providedIn: 'root' })
export class FinanceOptionsService {
  private readonly http = inject(HttpClient);

  loadBudgetPlans(communityId: string): Observable<BudgetPlanOption[]> {
    return this.http.get<BudgetPlanOption[]>(`${API_BASE_URL}/budgeting/communities/${communityId}/plans`).pipe(
      map(items => [...items].sort((a, b) => b.year - a.year || b.version - a.version))
    );
  }

  loadTaxDocuments(communityId: string): Observable<TaxDocumentOption[]> {
    return this.http.get<TaxDocumentOption[]>(`${API_BASE_URL}/tax/communities/${communityId}/documents`);
  }
}
