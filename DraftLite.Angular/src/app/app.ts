import { Component, computed, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import {HeaderComponent} from './component/header.component/header.Component';
import {ProjectNavComponent} from './component/projectNav.component/projectNav.component';
import {AuthComponent} from './component/auth/auth.page';

import {SideNavStore} from './stores/sideNav.store';
import { AuthStore } from './stores/auth.store';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, ProjectNavComponent, AuthComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('DraftLite');
  protected readonly sideNav = inject(SideNavStore);
  
  constructor(
    private readonly authStore: AuthStore,
  ) {}

  protected readonly isSidebarVisible = computed(
    () => this.sideNav.isSidebarVisible(),
  );

  public isShowLoginDilogbox = computed(() => this.authStore.showLoginDialogBox());

  /** True when the user has successfully authenticated. */
  protected readonly isAuthenticated = computed(() =>this.authStore.isUserAuthenticated());
 
  /** Convenience shortcut to the current UserDto. */
  protected readonly currentUser = computed(() => this.authStore.user()); 
}
