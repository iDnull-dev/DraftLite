import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  inject,
  signal,
  computed,
  effect
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

@Component({
  selector: 'app-home',
  imports: [CommonModule,
    ButtonModule,
    RippleModule,
    AvatarModule,
    BadgeModule,
    TooltipModule,
    DividerModule,
    SkeletonModule,
    ChipModule,],
  templateUrl: './home.page.html',
  styleUrl: './home.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})



export class Home{
  protected readonly authStore = inject(AuthStore);
 
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
 
  // ─── Local UI state ───────────────────────────────────────────────────────
 
  protected readonly isSidebarVisible = signal(false);
 
  // ─── Reactive side-effects ────────────────────────────────────────────────
 
  /**
   * Reacts to auth status changes:
   *  - Opens sidebar and loads project lists on login.
   *  - Clears everything on logout / anonymous state.
   */
  private readonly _authEffect = effect(() => {
    if (this.isAuthenticated()) {
      this.isSidebarVisible.set(true);
    } else {
      this.isSidebarVisible.set(false);
    }
  });
 
  // ─── Actions ──────────────────────────────────────────────────────────────
 
  protected toggleSidebar(): void {
    this.isSidebarVisible.update((v) => !v);
  }
 
  /** Demo helper — remove when real login flow is wired up. */
  protected mockLogin(): void {
    this.authStore.switchShowLoginDialogBox();
  }
}