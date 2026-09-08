import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root', standalone: true, imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    @if (isResidentPortal) {
      <div class="resident-shell">
        <header class="resident-topbar"><div class="resident-brand"><span class="brand-mark">CD</span><div><strong>Conjunto al Día</strong><small>Portal Residente</small></div></div><span class="portal-note">PortalCorporativo · Acceso seguro</span></header>
        <main class="resident-main"><router-outlet /></main>
      </div>
    } @else {
      <div class="shell"><aside><div class="brand"><span class="brand-mark">CD</span><div><h1>Conjunto al Día</h1><small>Administración</small></div></div><nav>
        <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a><a routerLink="/organizations" routerLinkActive="active">Organizaciones</a><a routerLink="/saas" routerLinkActive="active">SaaS</a><a routerLink="/communities" routerLinkActive="active">Comunidades</a><a routerLink="/properties" routerLinkActive="active">Propiedades</a><a routerLink="/people" routerLinkActive="active">Personas</a><a routerLink="/billing" routerLinkActive="active">Facturación</a><a routerLink="/collections" routerLinkActive="active">Cobranza</a><a routerLink="/banking" routerLinkActive="active">Banca</a><a routerLink="/tax" routerLinkActive="active">SRI / Tax</a><a routerLink="/accounting" routerLinkActive="active">Contabilidad</a><a routerLink="/treasury" routerLinkActive="active">Tesorería</a><a routerLink="/budgeting" routerLinkActive="active">Presupuesto</a><a routerLink="/procurement" routerLinkActive="active">Compras</a><a routerLink="/maintenance" routerLinkActive="active">Mantenimiento</a><a routerLink="/reservations" routerLinkActive="active">Reservas</a><a routerLink="/security" routerLinkActive="active">Seguridad</a><a routerLink="/governance" routerLinkActive="active">Gobernanza</a>
      </nav><div class="sidebar-note">Seguridad y permisos administrados por PortalCorporativo.</div></aside><main><header><span class="header-title">Centro de Operaciones</span><span class="portal-note">Angular 21 LTS · .NET 10</span></header><section class="content"><router-outlet /></section></main></div>
    }`,
  styles:[`.resident-shell{min-height:100vh;background:#f7f5f8}.resident-topbar{height:68px;background:#fff;border-bottom:1px solid #e6dfea;display:flex;align-items:center;justify-content:space-between;padding:0 28px;position:sticky;top:0;z-index:10}.resident-brand{display:flex;align-items:center;gap:10px}.resident-brand div{display:grid}.resident-brand small{color:#7f7487}.resident-main{max-width:1180px;margin:0 auto;padding:32px 24px 48px}@media(max-width:700px){.resident-topbar{padding:0 16px}.resident-topbar .portal-note{display:none}.resident-main{padding:22px 14px 36px}}`]
})
export class AppComponent { private readonly router=inject(Router); get isResidentPortal():boolean{return this.router.url.startsWith('/resident');} }
