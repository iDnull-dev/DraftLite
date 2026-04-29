import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth/auth.service';
import { AuthStore } from '../stores/auth.store';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(
    private readonly authStore: AuthStore,
    private readonly authService: AuthService,
    private readonly router: Router,
  ) {}

  async canActivate(): Promise<boolean | UrlTree> {
    const token = this.authStore.token();
    if (!token) return this.router.parseUrl('/auth');

    try {
      await this.authStore.loadMe(this.authService);
      return true;
    } catch {
      this.authStore.resetToAnonymous();
      return this.router.parseUrl('/auth');
    }
  }
}

