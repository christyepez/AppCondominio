import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `<div class="shell"><aside><h1>Conjunto al Día</h1><nav><a routerLink="/dashboard" routerLinkActive="active">Dashboard</a><a routerLink="/organizations" routerLinkActive="active">Organizaciones</a></nav></aside><main><header><span>AppCondominio</span><span class="portal-note">PortalCorporativo integration: Sprint 2</span></header><section class="content"><router-outlet /></section></main></div>`
})
export class AppComponent {}
