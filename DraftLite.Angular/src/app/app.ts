import { Component, computed, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import {HeaderComponent} from './component/header.component/header.Component';
import {ProjectNavComponent} from './component/projectNav.component/projectNav.component'

import {SideNavStore} from './stores/sideNav.store'

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, ProjectNavComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('DraftLite');
  protected readonly sideNav = inject(SideNavStore);

  protected readonly isSidebarVisible = computed(
    () => this.sideNav.isSidebarVisible(),
  );
}
