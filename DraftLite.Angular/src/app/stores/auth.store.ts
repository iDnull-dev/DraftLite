import { Injectable, signal } from '@angular/core';
import type { UserDto } from '../services/models/user.models';
import type { AuthService } from '../services/auth/auth.service';

const AUTH_TOKEN_KEY = 'draftlite_token';

export type AuthStatus = 'anonymous' | 'loading' | 'authenticated' | 'needs_register' | 'error';

@Injectable({
  providedIn: 'root',
})
export class AuthStore {
  readonly token = signal<string | null>(sessionStorage.getItem(AUTH_TOKEN_KEY));
  readonly user = signal<UserDto | null>(null);
  readonly status = signal<AuthStatus>('anonymous');
  readonly errorMessage = signal<string | null>(null);

  setToken(token: string | null) {
    this.token.set(token);
    if (token) sessionStorage.setItem(AUTH_TOKEN_KEY, token);
    else sessionStorage.removeItem(AUTH_TOKEN_KEY);
  }

  setUser(user: UserDto | null) {
    this.user.set(user);
  }

  resetToAnonymous() {
    this.setUser(null);
    this.status.set('anonymous');
    this.errorMessage.set(null);
    this.setToken(null);
  }

  async loadMe(authService: AuthService) {
    this.status.set('loading');
    this.errorMessage.set(null);
    try {
      const me = await authService.fetchMe();
      this.user.set(me);
      this.status.set('authenticated');
      return me;
    } catch (e) {
      this.user.set(null);
      this.status.set('needs_register');
      throw e;
    }
  }
}

