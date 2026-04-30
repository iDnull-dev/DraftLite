import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  inject,
  signal,
  computed,
  effect,
  input,
  InputFunction,
  WritableSignal,
  output,
  ViewChild,
  ElementRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, Observable, of } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { ButtonModule } from 'primeng/button';
import { RippleModule } from 'primeng/ripple';
import { AvatarModule } from 'primeng/avatar';
import { BadgeModule } from 'primeng/badge';
import { TooltipModule } from 'primeng/tooltip';
import { DividerModule } from 'primeng/divider';
import { SkeletonModule } from 'primeng/skeleton';
import { ChipModule } from 'primeng/chip';

import type { UserDto } from '../../services/models/user.models';
import{AuthStore} from '../../stores/auth.store';
import { SideNavStore } from '../../stores/sideNav.store';

@Component({
  selector: 'app-header',
  imports: [CommonModule,
    ButtonModule,
    RippleModule,
    AvatarModule,
    BadgeModule,
    TooltipModule,
    DividerModule,
    SkeletonModule,
    ChipModule,],
  template: `<header class="dl-topbar">
  <div class="dl-topbar__brand">
    <span class="dl-topbar__logo-mark">◈</span>
    <span class="dl-topbar__wordmark">DraftLite</span>
  </div>

  <nav class="dl-topbar__actions">

    <!-- Loading spinner while auth is resolving -->
    @if (authStore.status() === 'loading') {
      <i class="pi pi-spin pi-spinner dl-topbar__spinner"></i>
    }

    @if (isAuthenticated() && currentUser()) {
      <!-- Sidebar toggle -->
      @if (isSidebarVisible()){
      <button
        pButton
        pRipple
        pTooltip="Hide panel"
        tooltipPosition="bottom"
        class="p-button-text p-button-rounded dl-topbar__toggle"
        (click)="headerToggleSidebar()"
        aria-label="Toggle sidebar"
      >
        <i class="pi pi-angle-right"></i>      
      </button>
    }@else{
        <button
        pButton
        pRipple
        pTooltip="Show panel"
        tooltipPosition="bottom"
        class="p-button-text p-button-rounded dl-topbar__toggle"
        (click)="headerToggleSidebar()"
        aria-label="Toggle sidebar"
      >
        <i class="pi pi-angle-left"></i>
      </button>
    }

      <!-- Avatar + logout -->
      <p-avatar
        [label]="avatarInitials()"
        shape="circle"
        styleClass="dl-topbar__avatar"
        [pTooltip]="currentUser()!.pseudo"
        tooltipPosition="bottom"
      ></p-avatar>

      <button
        pButton
        pRipple
        label="Sign out"
        class="p-button-text p-button-sm dl-topbar__signout"
        (click)="logout()"
      > <i  class="pi pi-sign-out"></i> </button>

    } @else if (authStore.status() !== 'loading') {
      <button
        pButton
        pRipple
        label="Sign in"
        icon="pi pi-user"
        class="p-button-outlined p-button-sm dl-topbar__signin"
        (click)="mockLogin()"
      ></button>
    }
  </nav>
</header>`,
  styleUrl: './header.Component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})

export class HeaderComponent {
  protected readonly authStore = inject(AuthStore);
  
  protected readonly sideNav = inject(SideNavStore);
  protected readonly isSidebarVisible = computed(
    () => this.sideNav.isSidebarVisible(),
  );

  // ─── Derived state (computed) ─────────────────────────────────────────────
 
  /** True when the user has successfully authenticated. */
  protected readonly isAuthenticated = computed(
    () => this.authStore.status() === 'authenticated',
  );
 
  /** Convenience shortcut to the current UserDto. */
  protected readonly currentUser = computed(() => this.authStore.user());
 
  /** Display initials for the avatar — falls back to 'U'. */
  protected readonly avatarInitials = computed(() => {
    const pseudo = this.authStore.user()?.pseudo ?? '';
    return (
      pseudo
        .split(' ')
        .slice(0, 2)
        .map((w) => w[0]?.toUpperCase() ?? '')
        .join('') || 'U'
    );
  });
 
   // ─── Reactive side-effects ────────────────────────────────────────────────
 
  /**
   * Reacts to auth status changes:
   *  - Opens sidebar and loads project lists on login.
   *  - Clears everything on logout / anonymous state.
   */
  private readonly isSidebarVisibleEffect = effect(() => {
      if (this.isSidebarVisible() ) {     
        console.log("toggleButton true");
      } else {
        console.log("toggleButton false")
      }
  });

  // ─── Actions ──────────────────────────────────────────────────────────────
  /** Demo helper — remove when real login flow is wired up. */
  protected headerToggleSidebar(): void {
    this.sideNav.toggleSidebar();
  }

  protected mockLogin(): void {
  }
 
  protected logout(): void {
  }
}
