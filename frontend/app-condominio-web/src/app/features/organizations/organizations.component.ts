import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AsyncPipe, JsonPipe } from '@angular/common';
import { API_BASE_URL } from '../../core/config/api.config';

@Component({selector:'app-organizations',standalone:true,imports:[AsyncPipe,JsonPipe],template:`<h2>Organizaciones</h2><p>Vertical slice de referencia.</p><pre>{{ health$ | async | json }}</pre>`})
export class OrganizationsComponent {
  private readonly http = inject(HttpClient);
  readonly health$ = this.http.get(`${API_BASE_URL}/health/ready`);
}
