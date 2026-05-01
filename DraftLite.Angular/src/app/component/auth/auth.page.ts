import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { AuthStore } from '../../stores/auth.store';
import { environment } from '../../../environments/environment';
import { loadGoogleIdentityService } from '../../plugins/google-identity';
import type { RegisterUserRequest, UserDto } from '../../services/models/user.models';

function base64UrlDecode(input: string): string {
  // Google ID token is a JWT (base64url payload). We only need JSON payload fields.
  const base64 = input.replace(/-/g, '+').replace(/_/g, '/');
  const pad = base64.length % 4 === 0 ? '' : '='.repeat(4 - (base64.length % 4));
  return atob(base64 + pad);
}

function decodeJwtPayload<T extends Record<string, unknown>>(token: string): T | null {
  const parts = token.split('.');
  if (parts.length < 2) return null;
  try {
    const json = base64UrlDecode(parts[1]);
    return JSON.parse(json) as T;
  } catch {
    return null;
  }
}

type GoogleIdTokenPayload = {
  sub?: string;
  email?: string;
  name?: string;
};

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './auth.page.html',
  styleUrl: './auth.page.scss',
})
export class AuthComponent implements OnInit, OnDestroy {
  @ViewChild('googleButton', { static: true }) googleButton!: ElementRef<HTMLDivElement>;

  readonly pseudoForm = new FormGroup({
    pseudo: new FormControl('', [Validators.required, Validators.minLength(2)]),
  });

  showPseudoForm = false;
  pseudoError: string | null = null;
  loading = false;
  signedInUser: UserDto | null = null;

  private googleSub: string | null = null;
  private googleEmail: string | null = null;
  private googleCredential: string | null = null;

  constructor(
    private readonly router: Router,
    private readonly authService: AuthService,
    private readonly authStore: AuthStore,
  ) {}

  async ngOnInit(): Promise<void> {
    await loadGoogleIdentityService();

    const google = window.google?.accounts?.id;
    if (!google) {
      this.pseudoError = 'Google Identity Services unavailable.';
      return;
    }

    google.initialize({
      client_id: environment.googleClientId,
      callback: (response) => void this.onGoogleCredential(response.credential),
      ux_mode: 'popup',
      auto_select: false,
    });

    google.renderButton(this.googleButton.nativeElement, {
      theme: 'outline',
      size: 'large',
      text: 'continue_with',
      shape: 'pill',
      width: 280,
    });

    // Optional: keep UX closer to "sign-in" without requiring click in some scenarios.
    try {
      google.prompt();
    } catch {
      // Ignore prompt failures (blocked by browser policies, etc.).
    }
  }

  ngOnDestroy(): void {
    // Nothing to clean up (google identity is handled by global script).
  }

  private async onGoogleCredential(credential: string): Promise<void> {
    if (!credential) return;

    this.loading = true;
    this.pseudoError = null;
    this.showPseudoForm = false;

    const payload = decodeJwtPayload<GoogleIdTokenPayload>(credential);
    this.googleSub = payload?.sub ?? null;
    this.googleEmail = payload?.email ?? null;
    this.googleCredential = credential;

    if (!this.googleSub || !this.googleEmail) {
      this.loading = false;
      this.pseudoError = 'Google token missing required fields.';
      return;
    }

    try {
      // Backend expects the Google ID token as Bearer (JwtRoutingSecurity).
      this.authStore.setToken(credential);

      this.signedInUser = await this.authStore.loadMe(this.authService);
      this.router.navigateByUrl('/app');
    } catch {
      // Not registered yet: we ask for a unique pseudo then register.
      this.loading = false;
      this.showPseudoForm = true;
    }
  }

  async submitPseudo(): Promise<void> {
    if (this.pseudoForm.invalid) return;
    if (!this.googleEmail || !this.googleSub || !this.googleCredential) return;

    this.loading = true;
    this.pseudoError = null;

    try {
      const pseudo = (this.pseudoForm.value.pseudo ?? '').trim();
      const pseudoLower = pseudo.toLowerCase();

      // En pratique le backend doit garantir l’unicité; on fait aussi un check client
      // pour éviter des erreurs tardives côté DB.
      const candidates = await this.authService.searchUsers(pseudo);
      if (candidates.some((u) => u.pseudo.toLowerCase() === pseudoLower)) {
        this.loading = false;
        this.pseudoError = 'Ce pseudo est déjà utilisé. Choisis-en un autre.';
        return;
      }

      const request: RegisterUserRequest = {
        email: this.googleEmail,
        pseudo,
        googleId: this.googleSub,
      };

      await this.authService.register(request);

      this.signedInUser = await this.authStore.loadMe(this.authService);
      this.router.navigateByUrl('/app');
    } catch (e) {
      this.pseudoError = 'Impossible de finaliser l’inscription. Choisis un pseudo valide (et unique).';
      this.loading = false;
    }
  }
}

