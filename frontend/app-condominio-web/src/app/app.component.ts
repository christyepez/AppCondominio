import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="shell">
      <aside>
        <div class="brand"><span class="brand-mark">CD</span><div><h1>Conjunto al Día</h1><small>Administración</small></div></div>
        <nav>
          <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
          <a routerLink="/organizations" routerLinkActive="active">Organizaciones</a>
          <a routerLink="/communities" routerLinkActive="active">Comunidades</a>
          <a routerLink="/properties" routerLinkActive="active">Propiedades</a>
          <a routerLink="/governance" routerLinkActive="active">Gobernanza</a>
        </nav>
        <div class="sidebar-note">Seguridad y permisos administrados por PortalCorporativo.</div>
      </aside>
      <main>
        <header><span class="header-title">Centro de Operaciones</span><span class="portal-note">Angular 21 LTS · .NET 10</span></header>
        <section class="content"><router-outlet /></section>
      </main>
    </div>
  `
})
export class AppComponent {}
