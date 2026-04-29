import { Component, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthStore } from '../../stores/auth.store';

@Component({
  selector: 'app-connected',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="shell">
      <h1>DraftLite</h1>
      <p class="muted">Connecté.</p>

      <div class="card" *ngIf="user(); else noUser">
        <p><strong>Pseudo:</strong> {{ user()!.pseudo }}</p>
        <p><strong>Email:</strong> {{ user()!.email }}</p>
      </div>

      <ng-template #noUser>
        <p class="muted">Aucun utilisateur chargé.</p>
      </ng-template>

      <button class="logout" type="button" (click)="logout()">Se déconnecter</button>
    </div>
  `,
  styles: [
    `
      .shell {
        padding: 2rem;
      }
      h1 {
        margin: 0 0 0.25rem 0;
      }
      .muted {
        color: #64748b;
        margin-bottom: 1rem;
      }
      .card {
        padding: 1rem;
        border: 1px solid #e5e7eb;
        border-radius: 12px;
        max-width: 520px;
      }
      .logout {
        margin-top: 1.25rem;
        border: 0;
        background: #ef4444;
        color: #fff;
        padding: 0.6rem 1rem;
        border-radius: 10px;
        cursor: pointer;
      }
    `,
  ],
})
export class ConnectedPageComponent {
  readonly user = computed(() => this.authStore.user());

  constructor(
    private readonly authStore: AuthStore,
    private readonly router: Router,
  ) {}

  logout() {
    this.authStore.resetToAnonymous();
    this.router.navigateByUrl('/auth');
  }
}

