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
          <a routerLink="/people" routerLinkActive="active">Personas</a>
          <a routerLink="/billing" routerLinkActive="active">Facturación</a>
          <a routerLink="/collections" routerLinkActive="active">Cobranza</a>
          <a routerLink="/banking" routerLinkActive="active">Banca</a>
          <a routerLink="/accounting" routerLinkActive="active">Contabilidad</a>
          <a routerLink="/treasury" routerLinkActive="active">Tesorería</a>
          <a routerLink="/budgeting" routerLinkActive="active">Presupuesto</a>
          <a routerLink="/procurement" routerLinkActive="active">Compras</a>
          <a routerLink="/maintenance" routerLinkActive="active">Mantenimiento</a>
          <a routerLink="/reservations" routerLinkActive="active">Reservas</a>
          <a routerLink="/security" routerLinkActive="active">Seguridad</a>
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
